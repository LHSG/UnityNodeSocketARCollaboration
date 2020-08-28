using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using socket.io;
using UnityEngine;
using Logger = UnityEngine.XR.ARFoundation.Samples.Logger;

[Serializable]
class ARCloudData
{
    public string evt;
    public byte[] data;
}

public static class ARCloudApi
{
    static Socket socket;
    
    private static Queue<ARCloudData> queue = new Queue<ARCloudData>();
    
    public static void Connect(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            url = "http://localhost:3000";
        }
        socket = Socket.Connect(url);
        socket.On("connect", () => {
            Logger.Log("socket connected");
            IsConnected = true;
        });   
    }
    
    public static void SendToAllPeers(byte[] bytes)
    {
        var data = JsonConvert.SerializeObject(new ARCloudData{ evt = "msg", data = bytes});
        socket.EmitJson("msg", data, 
            (string r) => { Logger.Log(r); }
        );
    }

    public static void SetQueue()
    {
        socket.On("msg", (string data) => {
            var cloudData = JsonConvert.DeserializeObject<ARCloudData>(data);
            queue.Enqueue(cloudData);
        });
    }

    public static bool IsConnected = false;

    public static int ReceivedDataQueueSize => queue.Count;

    public static byte[] DequeueReceivedData()
    {
        return queue.Dequeue().data;
    }
}
