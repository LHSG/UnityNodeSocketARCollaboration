const express = require("express");
const app = express();
var server = require('http').createServer(app);
var io = require('socket.io')(server);
let arcloud = require( './app/ws/arcloud' );

// simple route
app.get("/", (req, res) => {
  res.json({ message: "Welcome to LHSG application." });
});

io.of('/cloud').on('connection', arcloud);

server.listen(3000, function() {
  console.log('Socket IO server listening on port 3000');
});
