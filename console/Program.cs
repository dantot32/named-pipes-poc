
using System.IO.Pipes;
using System.Text;

Console.WriteLine("Starting pipe server...");

using var server = new NamedPipeServerStream("named-pipes-poc", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

// accepts a client connection asynchronously
await server.WaitForConnectionAsync();
Console.WriteLine("Client connected!");

while (true)
{

    // Create a StreamReader and StreamWriter for reading and writing to the pipe
    using var reader = new StreamReader(server, Encoding.UTF8);
    using var writer = new StreamWriter(server, Encoding.UTF8); // { AutoFlush = true };

    // Read messages from the client and respond
    var message = await reader.ReadLineAsync();
    if (message == null) break;
    Console.WriteLine($"Received: {message}");

    // Respond to the client
    await writer.WriteLineAsync("Thanks from server");
    Console.WriteLine($"Responded: Thanks from server");

    // Wait for the client to disconnect
    server.Disconnect();
}


