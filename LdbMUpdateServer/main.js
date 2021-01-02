var express = require('express');
var app = express();

app.get('/', function (req, res) {
    res.send('<h1>Servidor de actualizaciones</h1><p>El servidor de actualizaciones está operativo</p>');
})

app.get('/lastversion', function(req, res) {
    res.json({
        Version : "0.1",
        DownloadUrl : "http://localhost:8085/downloads/LiteDBManager-0.2.zip"
    })
})

app.get('/downloads/:document', function(req, res) {
    const fileName = req.params.document;
    const directoryPath = __dirname + "/downloads/";
  
    res.download(directoryPath + fileName, fileName, (err) => {
      if (err) {
        res.status(500).send({
          message: "No se puede descargar el archivo. " + err,
        })
      }
    })
})

var server = app.listen(8085, function(){
    var host = server.address().address
    var port = server.address().port

    console.log('Servidor activo en http://%s:%s', host, port)
})