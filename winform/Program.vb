Imports System.IO

Friend Module Program

    <STAThread()>
    Friend Sub Main(args As String())

        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New Form1)

    End Sub

    Private Sub StartSidecar()

        Dim sidecarExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "console.exe")
        Dim alreadyRunning = Process.GetProcessesByName("console").Any()

        If Not alreadyRunning AndAlso File.Exists(sidecarExePath) Then

            Dim startInfo As New ProcessStartInfo(sidecarExePath)
            startInfo.UseShellExecute = False

#If DEBUG Then
            startInfo.CreateNoWindow = False
#Else
            startInfo.CreateNoWindow = True
#End If

            Process.Start(startInfo)
        End If

    End Sub

End Module
