const arcloud = (socket ) => {
  socket.on( 'msg', ( data ) => {
    socket.broadcast.emit( 'msg', data );
  });

};

module.exports = arcloud;