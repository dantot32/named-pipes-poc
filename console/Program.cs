// See https://aka.ms/new-console-template for more information
using System.IO.Pipes;
using System.Text;

var client = new NamedPipeClientStream(".", "mypipe", PipeDirection.InOut);
client.Connect();
Console.WriteLine("Connected to server.");
Console.WriteLine("Sending: Hello, server! ");

client.Write(Encoding.UTF8.GetBytes("Hello, server!"), 0, 14);

byte[] buffer = new byte[256];
var bytesRead = client.Read(buffer, 0, buffer.Length);
Console.WriteLine("Server replied: " + Encoding.UTF8.GetString(buffer, 0, bytesRead));

client.Dispose();




