using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARKit;
using Logger = UnityEngine.XR.ARFoundation.Samples.Logger;

#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine.XR.ARKit;
#endif

public class ARCloudManager : MonoBehaviour
{
    public string serverUrl  = "http://192.168.0.1:3000/cloud";
    ARSession m_ARSession;
    
    private void Awake()
    {
        m_ARSession = FindObjectOfType<ARSession>();
        ARCloudApi.Connect(serverUrl);
    }
    
    void OnEnable()
    {
        var subsystem = GetSubsystem();
        if (!ARKitSessionSubsystem.supportsCollaboration || subsystem == null)
        {
            Debug.Log("Collaborative sessions require iOS 13.");
            return;
        }

        subsystem.collaborationRequested = true;
        
        ARCloudApi.SetQueue();
    }
    
    void OnDisable()
    {
        var subsystem = GetSubsystem();
        if (subsystem != null)
            subsystem.collaborationRequested = false;
    }

    private void Update()
    {
        var subsystem = GetSubsystem();
        if (subsystem == null)
            return;
        
        while (subsystem.collaborationDataCount > 0)
        {
            using (var collaborationData = subsystem.DequeueCollaborationData())
            {
                CollaborationNetworkingIndicator.NotifyHasCollaborationData();
                    
                if (!ARCloudApi.IsConnected)
                    continue;
                    
                using (var serializedData = collaborationData.ToSerialized())
                {
                    var slice = serializedData.bytes.SliceConvert<byte>();
                    var bytes = new byte[slice.Length];
                    slice.CopyTo(bytes);
                    ARCloudApi.SendToAllPeers(bytes);
                    CollaborationNetworkingIndicator.NotifyOutgoingDataSent();
                    if (collaborationData.priority == ARCollaborationDataPriority.Critical)
                    {
                        Logger.Log($"Sent {bytes.Length} bytes of collaboration data.");
                    }
                }   
            }
        }

        while (ARCloudApi.ReceivedDataQueueSize > 0)
        {
            CollaborationNetworkingIndicator.NotifyIncomingDataReceived();
            var data = ARCloudApi.DequeueReceivedData();
            using (var collaborationData = new ARCollaborationData(data))
            {
                if (collaborationData.valid)
                {
                    subsystem.UpdateWithCollaborationData(collaborationData);
                    if (collaborationData.priority == ARCollaborationDataPriority.Critical)
                    {
                        Logger.Log($"Received {data.Length} bytes of collaboration data.");
                    }
                }
                else
                {
                    Logger.Log($"Received {data.Length} bytes from remote, but the collaboration data was not valid.");
                }
            }
        }
    }
    
    ARKitSessionSubsystem GetSubsystem()
    {
        if (m_ARSession == null)
            return null;

        return m_ARSession.subsystem as ARKitSessionSubsystem;
    }
}
