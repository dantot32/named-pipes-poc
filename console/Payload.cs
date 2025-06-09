namespace console;

public class Payload
{
    public string Command { get; set; }
    public string Data { get; set; }

    public override string ToString()
        =>  $" Command: {Command}, Data: {Data}";

}

