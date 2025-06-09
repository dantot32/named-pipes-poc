Imports System.IO
Imports System.IO.Pipes
Imports System.Text

' payload taken from
' https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication

Public Class Form1

    Private client As NamedPipeClientStream
    Private reader As StreamReader
    Private writer As StreamWriter

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Text = "Named Pipe Client Example"

        ' chat
        Dim chat As New TextBox
        chat.ReadOnly = True
        chat.BorderStyle = BorderStyle.None
        chat.Multiline = True
        chat.Dock = DockStyle.Fill
        chat.ScrollBars = ScrollBars.Both
        Me.Controls.Add(chat)

        ' text
        Dim text As New TextBox
        text.Dock = DockStyle.Bottom
        text.Height = 30
        text.Multiline = False
        text.BorderStyle = BorderStyle.FixedSingle
        text.Text = "Type your message here..."
        Me.Controls.Add(text)

        ' button
        Dim button As New Button
        button.Text = "Send"
        button.Dock = DockStyle.Bottom
        button.Height = 30
        button.Enabled = True
        Me.Controls.Add(button)
        AddHandler button.Click, Async Sub(s, args)

                                     If String.IsNullOrWhiteSpace(text.Text) Then Exit Sub

                                     button.Enabled = False
                                     Dim command = text.Text

                                     ' send message
                                     Await writer.WriteLineAsync(command)

                                     ' read message
                                     Dim response = Await reader.ReadLineAsync()

                                     ' report message
                                     chat.Text = chat.Text & vbNewLine & response
                                     text.Clear()
                                     button.Enabled = True

                                 End Sub

        client = New NamedPipeClientStream(".", "named-pipes-poc", PipeDirection.InOut, PipeOptions.Asynchronous)

        Threading.Thread.Sleep(1000) ' Wait for the server to start
        Await client.ConnectAsync()
        chat.Text = "Connected to server!"

        reader = New StreamReader(client, Encoding.UTF8)
        writer = New StreamWriter(client, Encoding.UTF8)

    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        If client IsNot Nothing Then client.Dispose()

        If reader IsNot Nothing Then

            If reader.BaseStream IsNot Nothing Then
                reader.Close()
            End If
            reader.Dispose()

        End If

        If writer IsNot Nothing Then

            If writer.BaseStream IsNot Nothing Then
                writer.Flush()
                writer.Close()
            End If
            writer.Dispose()

        End If

    End Sub

End Class
