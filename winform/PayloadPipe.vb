Imports Newtonsoft.Json
Imports System.IO
Imports System.IO.Pipes
Imports System.Security.Cryptography
Imports System.Text
Imports winform.Form1

Public Class PayloadPipe

    Public Shared Async Function SendAsync(pipeStream As PipeStream, e As Payload) As Task

        ' serialize
        Dim json = JsonConvert.SerializeObject(e)
        ' convert to bytes
        Dim jsonBytes As Byte() = Encoding.UTF8.GetBytes(json)

        ' calc SHA256 for data integrity
        Dim hash As Byte()
        Using sha256 As SHA256 = SHA256Managed.Create()
            hash = sha256.ComputeHash(jsonBytes)
        End Using

        ' concatenate hash + json
        Dim message As Byte() = New Byte(hash.Length + jsonBytes.Length - 1) {}
        Buffer.BlockCopy(hash, 0, message, 0, hash.Length)
        Buffer.BlockCopy(jsonBytes, 0, message, hash.Length, jsonBytes.Length)

        ' send message
        Await pipeStream.WriteAsync(message, 0, message.Length)

        pipeStream.WaitForPipeDrain()

    End Function

    Public Shared Async Function ReadAsync(pipe As PipeStream) As Task(Of Payload)

        ' read all message
        Dim bufferSize As Integer = 4096
        Dim buf As Byte() = New Byte(bufferSize - 1) {}
        Using ms As New MemoryStream()
            Dim bytesRead As Integer
            Do

                bytesRead = Await pipe.ReadAsync(buf, 0, buf.Length)
                If bytesRead > 0 Then ms.Write(buf, 0, bytesRead)

            Loop While bytesRead = buf.Length ' continua se c'è altro da leggere

            ' full message byte array
            Dim fullMessage As Byte() = ms.ToArray()

            ' check that message is long enough to contain the hash
            If fullMessage.Length < 32 Then Throw New InvalidDataException("Invalid message lenght")

            ' extract first 32 bytes as hash
            Dim receivedHash As Byte() = New Byte(31) {}
            Buffer.BlockCopy(fullMessage, 0, receivedHash, 0, 32)

            ' extract payload
            Dim jsonBytesLength As Integer = fullMessage.Length - 32
            Dim jsonBytes As Byte() = New Byte(jsonBytesLength - 1) {}
            Buffer.BlockCopy(fullMessage, 32, jsonBytes, 0, jsonBytesLength)

            ' compute sha256 hash of the received JSON bytes
            Dim computedHash As Byte()
            Using sha256 As SHA256 = SHA256Managed.Create()
                computedHash = sha256.ComputeHash(jsonBytes)
            End Using

            ' compare received hash with computed hash
            If Not receivedHash.SequenceEqual(computedHash) Then Throw New InvalidDataException("Payload content corrupted")

            ' deserialize JSON to Payload object
            Dim json As String = Encoding.UTF8.GetString(jsonBytes)
            Dim payload As Payload = JsonConvert.DeserializeObject(Of Payload)(json)

            Return payload
        End Using
    End Function

End Class