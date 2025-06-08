Imports System.IO.Pipes
Imports System.Text

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Text = "Named Pipe Server Example"

        Dim c As New TextBox
        c.ReadOnly = True
        c.Multiline = True
        c.Dock = DockStyle.Fill
        c.ScrollBars = ScrollBars.Both
        Me.Controls.Add(c)

        Dim server = New NamedPipeServerStream("mypipe", PipeDirection.InOut, 1)
        c.Text = "Waiting for client connection..."
        server.WaitForConnection()
        c.Text = c.Text & vbNewLine & "Client connected."

        Dim buffer(255) As Byte
        Dim bytesRead = server.Read(buffer, 0, buffer.Length)
        c.Text = c.Text & vbNewLine & "Received: " & Encoding.UTF8.GetString(buffer, 0, bytesRead)

        server.Write(Encoding.UTF8.GetBytes("Ok"), 0, 2)
        server.Dispose()

    End Sub

End Class
