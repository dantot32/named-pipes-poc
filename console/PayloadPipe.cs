using Newtonsoft.Json;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace console;

public class PayloadPipe
{
    public static async Task SendAsync(PipeStream pipeStream, Payload e)
    {

        // serialize
        var json = JsonConvert.SerializeObject(e);
        // convert to bytes
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        // calc SHA256 for data integrity
        byte[] hash;
        using (SHA256 sha256 = SHA256Managed.Create())
        {
            hash = sha256.ComputeHash(jsonBytes);
        }

        // concatenate hash + json
        byte[] message = new byte[hash.Length + jsonBytes.Length - 1 + 1];
        Buffer.BlockCopy(hash, 0, message, 0, hash.Length);
        Buffer.BlockCopy(jsonBytes, 0, message, hash.Length, jsonBytes.Length);

        // send message
        await pipeStream.WriteAsync(message, 0, message.Length);

        pipeStream.WaitForPipeDrain();
    }

    public static async Task<Payload> ReadAsync(PipeStream pipe)
    {

        // read all message
        int bufferSize = 4096;
        byte[] buf = new byte[bufferSize];
        using (MemoryStream ms = new MemoryStream())
        {
            int bytesRead;
            do
            {
                bytesRead = await pipe.ReadAsync(buf, 0, buf.Length);
                if (bytesRead > 0)
                    ms.Write(buf, 0, bytesRead);
            }
            while (bytesRead == buf.Length); // continua se c'è altro da leggere

            // full message byte array
            byte[] fullMessage = ms.ToArray();

            // check that message is long enough to contain the hash
            if (fullMessage.Length < 32)
                throw new InvalidDataException("Invalid message lenght");

            // extract first 32 bytes as hash
            byte[] receivedHash = new byte[32];
            Buffer.BlockCopy(fullMessage, 0, receivedHash, 0, 32);

            // extract payload
            int jsonBytesLength = fullMessage.Length - 32;
            byte[] jsonBytes = new byte[jsonBytesLength - 1 + 1];
            Buffer.BlockCopy(fullMessage, 32, jsonBytes, 0, jsonBytesLength);

            // compute sha256 hash of the received JSON bytes
            byte[] computedHash;
            using (SHA256 sha256 = SHA256Managed.Create())
            {
                computedHash = sha256.ComputeHash(jsonBytes);
            }

            // compare received hash with computed hash
            if (!receivedHash.SequenceEqual(computedHash))
                throw new InvalidDataException("Payload content corrupted");

            // deserialize JSON to Payload object
            string json = Encoding.UTF8.GetString(jsonBytes);
            Payload payload = JsonConvert.DeserializeObject<Payload>(json);

            return payload;
        }
    }
}
