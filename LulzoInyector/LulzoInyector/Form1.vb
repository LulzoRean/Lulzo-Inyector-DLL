Public Class Form1
    Private TargetProcessHandle As Integer
    Private pfnStartAddr As Integer
    Private pszLibFileRemote As String
    Private TargetBufferSize As Integer

    Public Const PROCESS_VM_READ = &H10
    Public Const TH32CS_SNAPPROCESS = &H2
    Public Const MEM_COMMIT = 4096
    Public Const PAGE_READWRITE = 4
    Public Const PROCESS_CREATE_THREAD = (&H2)
    Public Const PROCESS_VM_OPERATION = (&H8)
    Public Const PROCESS_VM_WRITE = (&H20)
    Dim DLLFileName As String
    Public Declare Function ReadProcessMemory Lib "kernel32" ( _
    ByVal hProcess As Integer, _
    ByVal lpBaseAddress As Integer, _
    ByVal lpBuffer As String, _
    ByVal nSize As Integer, _
    ByRef lpNumberOfBytesWritten As Integer) As Integer

    Public Declare Function LoadLibrary Lib "kernel32" Alias "LoadLibraryA" ( _
    ByVal lpLibFileName As String) As Integer

    Public Declare Function VirtualAllocEx Lib "kernel32" ( _
    ByVal hProcess As Integer, _
    ByVal lpAddress As Integer, _
    ByVal dwSize As Integer, _
    ByVal flAllocationType As Integer, _
    ByVal flProtect As Integer) As Integer

    Public Declare Function WriteProcessMemory Lib "kernel32" ( _
    ByVal hProcess As Integer, _
    ByVal lpBaseAddress As Integer, _
    ByVal lpBuffer As String, _
    ByVal nSize As Integer, _
    ByRef lpNumberOfBytesWritten As Integer) As Integer

    Public Declare Function GetProcAddress Lib "kernel32" ( _
    ByVal hModule As Integer, ByVal lpProcName As String) As Integer

    Private Declare Function GetModuleHandle Lib "Kernel32" Alias "GetModuleHandleA" ( _
    ByVal lpModuleName As String) As Integer

    Public Declare Function CreateRemoteThread Lib "kernel32" ( _
    ByVal hProcess As Integer, _
    ByVal lpThreadAttributes As Integer, _
    ByVal dwStackSize As Integer, _
    ByVal lpStartAddress As Integer, _
    ByVal lpParameter As Integer, _
    ByVal dwCreationFlags As Integer, _
    ByRef lpThreadId As Integer) As Integer

    Public Declare Function OpenProcess Lib "kernel32" ( _
    ByVal dwDesiredAccess As Integer, _
    ByVal bInheritHandle As Integer, _
    ByVal dwProcessId As Integer) As Integer

    Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" ( _
    ByVal lpClassName As String, _
    ByVal lpWindowName As String) As Integer

    Private Declare Function CloseHandle Lib "kernel32" Alias "CloseHandleA" ( _
    ByVal hObject As Integer) As Integer

    Dim ExeName As String = IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath)

    Dim dllproc As String = "0"
    Dim aa As String = "0"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ListBox1.Items.Add("Lulzo Dll Inyector iniciado.")
        cargarconfig()

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        OpenFileDialog1.ShowDialog()
        If OpenFileDialog1.FileName.Length > 0 Then
            TextBox1.Text = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        If TextBox2.Text.Length > 0 Then
            Button2.Enabled = True
        Else
            Button2.Enabled = False
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If IO.File.Exists(dllproc) Then
            Dim TargetProcess As Process() = Process.GetProcessesByName(TextBox2.Text)
            If TargetProcess.Length = 0 Then
                If aa = "0" Then
                    ListBox1.Items.Add("Esperando a: " + TextBox2.Text + ".exe")
                    Button2.Text = "Cancelar"
                    aa = "1"
                End If
            Else
                Timer1.Stop()
                ListBox1.Items.Add("Injectando..")
                Call Inject()
                End
                End
            End If
        Else
            ListBox1.Items.Add("Error: Dll no encontrado")
        End If
    End Sub

    Private Sub Inject()
        On Error GoTo 1
        Dim TargetProcess As Process() = Process.GetProcessesByName(TextBox2.Text)
        TargetProcessHandle = OpenProcess(PROCESS_CREATE_THREAD Or PROCESS_VM_OPERATION Or PROCESS_VM_WRITE, False, TargetProcess(0).Id)
        pszLibFileRemote = dllproc
        pfnStartAddr = GetProcAddress(GetModuleHandle("Kernel32"), "LoadLibraryA")
        TargetBufferSize = 1 + Len(pszLibFileRemote)
        Dim Rtn As Integer
        Dim LoadLibParamAdr As Integer
        LoadLibParamAdr = VirtualAllocEx(TargetProcessHandle, 0, TargetBufferSize, MEM_COMMIT, PAGE_READWRITE)
        Rtn = WriteProcessMemory(TargetProcessHandle, LoadLibParamAdr, pszLibFileRemote, TargetBufferSize, 0)
        CreateRemoteThread(TargetProcessHandle, 0, 0, pfnStartAddr, LoadLibParamAdr, 0, 0)
        CloseHandle(TargetProcessHandle)
1:      End
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If Button2.Text = "Cancelar" Then
            Button2.Text = "Inyectar!"
            Timer1.Stop()
            ListBox1.Items.Add("Espera cancelada.")
            aa = 0
            Exit Sub
        End If

        dllproc = OpenFileDialog1.FileName
        If TextBox2.Text.Length = 0 Then
            ListBox1.Items.Add("Por favor seleccione un proceso..")
        Else
            If dllproc = "0" Then
                ListBox1.Items.Add("No se ha encontrado el DLL..")
            Else
                ListBox1.Items.Add("Dll Encontrado..")
                guardarconfig()
                dllproc = TextBox1.Text
                Timer1.Start()
            End If
        End If
    End Sub

    Sub guardarconfig()
        Try
            Dim FILE_NAME As String = "InyectConfig.ini"
            Dim objWriter As New System.IO.StreamWriter(FILE_NAME)
            objWriter.WriteLine(TextBox1.Text)
            objWriter.WriteLine(TextBox2.Text)
            objWriter.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub cargarconfig()
        Try
            Dim FILE_NAME As String = "InyectConfig.ini"
            Dim TextLine As String
            Dim list As New ArrayList
            If System.IO.File.Exists(FILE_NAME) = True Then
                Dim objReader As New System.IO.StreamReader(FILE_NAME)
                Do While objReader.Peek() <> -1
                    TextLine = objReader.ReadLine()
                    list.Add(TextLine)
                Loop
                TextBox1.Text = list(0)
                TextBox2.Text = list(1)
                ListBox1.Items.Add("Configuracion cargada.")
                objReader.Close()
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
End Class
