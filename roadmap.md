
```cs
// -------------------- SERVER (Console C# - .NET 6+) --------------------
using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private const string PipeName = "MySecurePipe";
    private const string AesKey = "12345678901234567890123456789012"; // 32 bytes
    private const string AesIV = "1234567890123456"; // 16 bytes

    public class Payload
    {
        public string Command { get; set; }
        public string Data { get; set; }
    }

    static async Task Main()
    {
        using var pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        Console.WriteLine("[Server] Waiting for connection...");
        await pipeServer.WaitForConnectionAsync();
        Console.WriteLine("[Server] Client connected.");

        var payload = await ReadMessageAsync(pipeServer);
        Console.WriteLine($"[Server] Command: {payload.Command}, Data: {payload.Data}");

        // Echo back a response
        var response = new Payload { Command = "response", Data = "Received: " + payload.Data };
        await WriteMessageAsync(pipeServer, response);

        pipeServer.WaitForPipeDrain();
    }

    static async Task<Payload> ReadMessageAsync(PipeStream stream)
    {
        var lengthBuffer = new byte[4];
        await stream.ReadAsync(lengthBuffer, 0, 4);
        int totalLength = BitConverter.ToInt32(lengthBuffer, 0);

        var buffer = new byte[totalLength];
        int read = 0;
        while (read < totalLength)
            read += await stream.ReadAsync(buffer, read, totalLength - read);

        var receivedHash = buffer[..32];
        var encryptedPayload = buffer[32..];

        using var sha256 = SHA256.Create();
        var computedHash = sha256.ComputeHash(encryptedPayload);

        if (!receivedHash.AsSpan().SequenceEqual(computedHash))
            throw new InvalidDataException("Invalid hash: data corrupted.");

        var decrypted = DecryptAES(encryptedPayload);
        return JsonSerializer.Deserialize<Payload>(decrypted);
    }

    static async Task WriteMessageAsync(PipeStream stream, Payload payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var encrypted = EncryptAES(json);

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(encrypted);

        var totalLength = hash.Length + encrypted.Length;
        var lengthPrefix = BitConverter.GetBytes(totalLength);

        await stream.WriteAsync(lengthPrefix);
        await stream.WriteAsync(hash);
        await stream.WriteAsync(encrypted);
        await stream.FlushAsync();
    }

    static byte[] EncryptAES(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(AesKey);
        aes.IV = Encoding.UTF8.GetBytes(AesIV);
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
    }

    static string DecryptAES(byte[] encrypted)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(AesKey);
        aes.IV = Encoding.UTF8.GetBytes(AesIV);
        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}

```

```vb
' -------------------- CLIENT (WinForms VB.NET - .NET Framework) --------------------
Imports System.IO.Pipes
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json

Public Class Form1
    Private Const PipeName As String = "MySecurePipe"
    Private Const AesKey As String = "12345678901234567890123456789012"
    Private Const AesIV As String = "1234567890123456"

    Private _client As NamedPipeClientStream

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _client = New NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous)
        Await _client.ConnectAsync()
        Console.WriteLine("[Client] Connected to server")
    End Sub

    Private Async Sub btnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click
        Dim payload As New Payload With {
            .Command = "launch",
            .Data = "42"
        }
        Await WriteMessageAsync(_client, payload)

        Dim response = Await ReadMessageAsync(_client)
        MessageBox.Show($"[Client] Response: {response.Command} - {response.Data}")
    End Sub

    Private Async Function WriteMessageAsync(stream As PipeStream, payload As Payload) As Task
        Dim json = JsonSerializer.Serialize(payload)
        Dim encrypted = EncryptAES(json)

        Using sha256 = SHA256.Create()
            Dim hash = sha256.ComputeHash(encrypted)
            Dim totalLength = hash.Length + encrypted.Length
            Dim lengthPrefix = BitConverter.GetBytes(totalLength)

            Await stream.WriteAsync(lengthPrefix, 0, 4)
            Await stream.WriteAsync(hash, 0, hash.Length)
            Await stream.WriteAsync(encrypted, 0, encrypted.Length)
            Await stream.FlushAsync()
        End Using
    End Function

    Private Async Function ReadMessageAsync(stream As PipeStream) As Task(Of Payload)
        Dim lengthBuffer(3) As Byte
        Await stream.ReadAsync(lengthBuffer, 0, 4)
        Dim totalLength = BitConverter.ToInt32(lengthBuffer, 0)

        Dim buffer(totalLength - 1) As Byte
        Dim read = 0
        While read < totalLength
            read += Await stream.ReadAsync(buffer, read, totalLength - read)
        End While

        Dim receivedHash = buffer.Take(32).ToArray()
        Dim encryptedPayload = buffer.Skip(32).ToArray()

        Using sha256 = SHA256.Create()
            Dim computedHash = sha256.ComputeHash(encryptedPayload)
            If Not receivedHash.SequenceEqual(computedHash) Then
                Throw New InvalidDataException("Hash mismatch")
            End If
        End Using

        Dim decrypted = DecryptAES(encryptedPayload)
        Return JsonSerializer.Deserialize(Of Payload)(decrypted)
    End Function

    Private Function EncryptAES(plainText As String) As Byte()
        Using aes = Aes.Create()
            aes.Key = Encoding.UTF8.GetBytes(AesKey)
            aes.IV = Encoding.UTF8.GetBytes(AesIV)
            Using encryptor = aes.CreateEncryptor()
                Dim plainBytes = Encoding.UTF8.GetBytes(plainText)
                Return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length)
            End Using
        End Using
    End Function

    Private Function DecryptAES(encrypted As Byte()) As String
        Using aes = Aes.Create()
            aes.Key = Encoding.UTF8.GetBytes(AesKey)
            aes.IV = Encoding.UTF8.GetBytes(AesIV)
            Using decryptor = aes.CreateDecryptor()
                Dim decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length)
                Return Encoding.UTF8.GetString(decryptedBytes)
            End Using
        End Using
    End Function

    Public Class Payload
        Public Property Command As String
        Public Property Data As String
    End Class
End Class

```

