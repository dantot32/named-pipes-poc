Public Class Payload
    Public Property Command As String
    Public Property Data As String

    Public Overrides Function ToString() As String
        Return $" Command: {Command}, Data: {Data}"
    End Function

End Class
