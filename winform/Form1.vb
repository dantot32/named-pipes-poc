Imports System.IO
Imports System.IO.Pipes
Imports System.Runtime.Serialization
Imports System.Security.Cryptography
Imports System.Security.Policy
Imports System.Text
Imports Newtonsoft.Json

' payload taken from
' https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication

Public Class Form1

    Private _client As NamedPipeClientStream
    Private _reader As StreamReader
    Private _writer As StreamWriter
    Private WithEvents _chat As TextBox
    Private WithEvents _textEditor As TextBox
    Private WithEvents _button As Button

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Text = "Named Pipe Client Example"

        ' chat
        _chat = New TextBox
        With _chat
            .ReadOnly = True
            .BorderStyle = BorderStyle.None
            .Multiline = True
            .Dock = DockStyle.Fill
            .ScrollBars = ScrollBars.Both
        End With
        Me.Controls.Add(_chat)

        ' text
        _textEditor = New TextBox
        With _textEditor
            .Dock = DockStyle.Bottom
            .Height = 30
            .Multiline = False
            .BorderStyle = BorderStyle.FixedSingle
            .Text = "Type your message here..."
        End With
        Me.Controls.Add(_textEditor)

        ' button
        _button = New Button
        With _button
            .Text = "Send"
            .Dock = DockStyle.Bottom
            .Height = 30
            .Enabled = True
        End With
        Me.Controls.Add(_button)

        _client = New NamedPipeClientStream(".", "named-pipes-poc", PipeDirection.InOut, PipeOptions.Asynchronous)

        Threading.Thread.Sleep(1000) ' Wait for the server to start
        Await _client.ConnectAsync()
        _chat.Text = "Connected to server!"

        _reader = New StreamReader(_client)
        _writer = New StreamWriter(_client)

    End Sub

    Private Async Sub ButtonClick() Handles _button.Click

        If String.IsNullOrWhiteSpace(_textEditor.Text) Then Exit Sub

        _button.Enabled = False
        Dim command = _textEditor.Text

        ' create payload
        Dim p As New Payload With {.Command = command, .Data = "43"}

        Await PayloadPipe.SendAsync(_writer.BaseStream, p)

        ' report message
        _chat.Text = _chat.Text & vbNewLine & p.ToString

        _textEditor.Clear()
        _button.Enabled = True

    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        If _client IsNot Nothing Then _client.Dispose()

        If _reader IsNot Nothing Then

            If _reader.BaseStream IsNot Nothing Then
                _reader.Close()
            End If
            _reader.Dispose()

        End If

        If _writer IsNot Nothing Then

            _writer.Dispose()

        End If

    End Sub

End Class
