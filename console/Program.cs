
using console;
using Newtonsoft.Json;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

Console.WriteLine("Starting pipe server...");

using var server = new NamedPipeServerStream("named-pipes-poc", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

// accepts a client connection asynchronously
await server.WaitForConnectionAsync();
Console.WriteLine("Client connected!");

var reader = new StreamReader(server);
var writer = new StreamWriter(server);

while (true)
{
    PipeStream pipe = (PipeStream)reader.BaseStream;

    Console.WriteLine("Waiting for new messages ...");
    // get the message
    var p = await PayloadPipe.ReadAsync(pipe);
    Console.WriteLine(p);

}




