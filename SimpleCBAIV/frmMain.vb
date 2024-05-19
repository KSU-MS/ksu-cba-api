'Liability Disclaimer
'-------------------------------
'West Mountain Radio will not be liable to you (whether under the law of contract,
'       the law of torts or otherwise) in relation to the contents of, or use of,
'       or otherwise in connection with, the CBA  ("product"): 
'   * To the extent that the product may cause any bodily or equipment damage; 
'   * For any indirect, special or consequential loss of any kind;
'   * For any business losses, loss of revenue, income, profits or anticipated savings,
'     loss of contracts or business relationships, loss of reputation or goodwill, 
'     or loss or corruption of information or data.
'
'By using this product, you agree to the limitations of liability set forth in this disclaimer 
'   are reasonable. 
'
'Copyright
'-------------------------------
'West Mountain Radio CBA © 2012 by Custom Computer Services, Inc.

Imports System.Runtime.InteropServices
Imports SimpleCBAIV.CBADevice

Public Class frmMain

#Region "Consts"
    Private Const Set_Status As String = "Set Status (0x53)"

    Private Const Get_Config As String = "Get Config (0x43)"

    Private Const Calibrate_Load_Position As String = "Calibrate Load Position (0x4C)"

    Private Const Calibrate_Voltage_Position As String = "Calibrate Voltage Position (0x56)"

    Private Const Reset_Pic As String = "Reset PIC (0x52)"

    Private Const Restart_Bootloader As String = "Restart Bootloader (0x41)"

    Private Const Send_Bootloader_Fragment As String = "Send Bootloader Fragment (0x42)"

    Private Const DBT_DEVICEARRIVAL As Int32 = 32768

    Private Const DBT_DEVICEREMOVECOMPLETE As Int32 = 32772

    Private Const DBT_DEVTYP_DEVICEINTERFACE As Int32 = 5

    Private Const DBT_DEVTYP_HANDLE As Int32 = 6

    Private Const DEVICE_NOTIFY_ALL_INTERFACE_CLASSES As Int32 = 4

    Private Const DEVICE_NOTIFY_SERVICE_HANDLE As Int32 = 1

    Private Const DEVICE_NOTIFY_WINDOW_HANDLE As Int32 = 0

    Private Const WM_DEVICECHANGE As Int32 = 537

    ''' <summary>
    ''' Conversion factor for double values to CBA values.
    ''' </summary>
    Private Const Million As Double = 1000000

    Private Const IDX_vcal_60v As Byte = 2

    Private Const IDX_vcal_15v As Byte = 1

    Private Const IDX_vcal_4p5v As Byte = 0

    Private Const PWM_CHAN_FINE As Byte = 0

    Private Const PWM_CHAN_COARSE As Byte = 1

    Private m_NotificationHandle As IntPtr = IntPtr.Zero
#End Region
#Region "Member vars"

    Private m_MPUSBEngine As MPUSBEngine

    Private m_timers As List(Of System.Windows.Forms.Timer) = New List(Of System.Windows.Forms.Timer)

    Private m_HasResponded As Boolean = False

    Dim m_Fan As Byte
    Dim m_LED1 As Byte
    Dim m_LED2 As Byte
    Dim m_IOTRIS As Byte
    Dim m_IOPORT As Byte
    Dim m_Load As UInt32
    Dim m_Detect As UInt32
    Dim m_Voltage As UInt32
    Dim m_SerialNumber As UInt32
    Dim m_IntTemperature As UInt16
    Dim m_ExtTemperature As UInt16
    Dim m_fanRunning As Boolean
    Dim m_LED1Lit As Boolean
    Dim m_LED2Lit As Boolean
    Dim m_ChangeTestState As Boolean
    Dim m_TestRunning As Boolean
    Dim m_AccumulatedAmpHrs As Double
    Dim m_LastTime As UInt32
#End Region

#Region "Auto-Connect CBA"

    Protected Overrides Sub WndProc(ByRef m As Message)
        Try
            ' The OnDeviceChange routine processes WM_DEVICECHANGE messages.
            Select Case (m.Msg)
                Case WM_DEVICECHANGE
                    If (m.WParam.ToInt32 <> 7) Then
                        OnDeviceChange(m)
                    End If
            End Select
            ' Let the base form process the message.
            MyBase.WndProc(m)
        Catch e As System.Exception

        End Try
    End Sub

    '''  <summary>
    '''  Requests to receive a notification when a USB device is attached or removed.
    '''  </summary>
    '''  <param name="formHandle"> handle to the window that will receive device events. </param>
    Public Sub RegisterForDeviceNotifications(ByVal formHandle As IntPtr)
        If (m_NotificationHandle <> IntPtr.Zero) Then
            Throw New ApplicationException("Already registered for notifications.")
        End If
        Dim di As DEV_BROADCAST_DEVICEINTERFACE = New DEV_BROADCAST_DEVICEINTERFACE
        Dim diBuffer As IntPtr = IntPtr.Zero
        ' Specify the USB interface class to receive notifications about.
        di.dbcc_classguid = New Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED")
        m_NotificationHandle = RegisterDeviceNotification_di(formHandle, di, DEVICE_NOTIFY_WINDOW_HANDLE)
    End Sub

    Public Sub OnDeviceChange(ByRef m As Message)
        Select Case (m.WParam.ToInt32)
            Case DBT_DEVICEARRIVAL
                ' USB device inserted.
                OnDeviceArrival(m)
            Case DBT_DEVICEREMOVECOMPLETE
                ' USB device removed.
                DisconnectCBA()
        End Select
    End Sub

    Private Sub OnDeviceArrival(ByRef m As Message)
        Dim di As DEV_BROADCAST_DEVICEINTERFACE = New DEV_BROADCAST_DEVICEINTERFACE
        Marshal.PtrToStructure(m.LParam, di)
        If Not ConnectCBA() Then
            Return
        End If
    End Sub

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)> _
    Public Class DEV_BROADCAST_DEVICEINTERFACE

        Public dbcc_size As Integer

        Public dbcc_devicetype As Integer

        Public dbcc_reserved As Integer

        Public dbcc_classguid As Guid

        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=255)> _
        Public dbcc_name As String

        Public Sub New()
            MyBase.New()
            ' Populate the class instance.
            dbcc_size = Marshal.SizeOf(Me)
            dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE
        End Sub
    End Class

    ' Use this to read the dbcc_name String and classguid:
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)> _
    Private Class DEV_BROADCAST_DEVICEINTERFACE_1

        Public dbcc_size As Int32

        Public dbcc_devicetype As Int32

        Public dbcc_reserved As Int32

        ' [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        ' public Byte[] dbcc_classguid;
        Public dbcc_classguid As Guid

        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=255)> _
        Public dbcc_name As String

        Public Sub New()
            MyBase.New()
            ' Populate the class instance.
            'dbcc_name = new StringBuilder(255);
            'dbcc_size = Marshal.SizeOf(this);
            'dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            'dbcc_name = new string('\0', 255);
        End Sub
    End Class

    <StructLayout(LayoutKind.Sequential)> _
    Private Class DEV_BROADCAST_HDR

        Friend dbch_size As Integer

        Friend dbch_devicetype As Integer

        Friend dbch_reserved As Integer
    End Class

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Private Shared Function RegisterDeviceNotification(hRecipient As IntPtr, NotificationFilter As IntPtr, Flags As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True, EntryPoint:="RegisterDeviceNotification")> _
    Private Shared Function RegisterDeviceNotification_di(hRecipient As IntPtr, <MarshalAs(UnmanagedType.LPStruct)> NotificationFilter As DEV_BROADCAST_DEVICEINTERFACE, Flags As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)> _
    Private Shared Function UnregisterDeviceNotification(Handle As IntPtr) As [Boolean]
    End Function

#End Region
#Region "Main Logic"

    Public Sub New()
        MyBase.New()
        'InitializeComponent()
        'load the xml setings file
        m_MPUSBEngine = New MPUSBEngine(Me)
        RegisterForDeviceNotifications(Me.Handle)
        InitializeComponent()
        ConnectCBA()
    End Sub


    ''' <summary>
    ''' Updates the GUI with the relevant information from the CBA.
    ''' </summary>
    ''' <param name="Fan">Command State of the fan</param>
    ''' <param name="LED1">Command State of the LED 1</param>
    ''' <param name="LED2">Command State of the LED 2</param>
    ''' <param name="Load">Intended current draw for the CBA</param>
    ''' <param name="Detect">Present apparent current draw of the CBA</param>
    ''' <param name="Voltage">Present voltage apparent to the CBA</param>
    ''' <param name="fanRunning">true = fan running, false = fan stopped</param>
    ''' <param name="LED1Lit">true = on, false = off</param>
    ''' <param name="LED2Lit">true = on, false = off</param>
    ''' <param name="ChangeTestState">true = change, false = no change</param>
    ''' <param name="TestRunning">true = running, false = not running</param>
    Public Sub RefreshStatus( _
                ByVal Fan As Byte, _
                ByVal LED1 As Byte, _
                ByVal LED2 As Byte, _
                ByVal Load As UInt32, _
                ByVal Detect As UInt32, _
                ByVal Voltage As UInt32, _
                ByVal IntTemperature As UInt16, _
                ByVal ExtTemperature As UInt16, _
                ByVal fanRunning As Boolean, _
                ByVal LED1Lit As Boolean, _
                ByVal LED2Lit As Boolean, _
                ByVal ChangeTestState As Boolean, _
                ByVal TestRunning As Boolean, _
                ByVal IOTRIS As Byte, _
                ByVal IOPORT As Byte, _
                ByVal useVstop As Boolean, _
                ByVal StopVStop As Boolean, _
                ByVal VSTOP As UInt32, _
                ByVal TIME As UInt32, _
                ByVal PWMDisabled As Boolean, _
                ByVal CalibrationStatus As Boolean, _
                ByVal CurrentLimit As Boolean, _
                ByVal AbortTemp As Boolean)
        'store the relevant data/update the fact that this has happened.
        m_Fan = Fan
        m_LED1 = LED1
        m_LED1Lit = LED1Lit
        m_LED2 = LED2
        m_LED2Lit = LED2Lit
        m_Load = Load
        m_Detect = Detect
        m_Voltage = Voltage
        m_IntTemperature = IntTemperature
        m_ExtTemperature = ExtTemperature
        m_ChangeTestState = ChangeTestState
        m_TestRunning = TestRunning
        m_IOTRIS = IOTRIS
        m_IOPORT = IOPORT
        m_HasResponded = True
        txtVoltage.Text = Math.Round(m_Voltage / Million, 3).ToString("0.000")

        txtTime.Text = (TIME \ 60).ToString + ":" + (TIME Mod 60).ToString("00")
        If (Not TIME = m_LastTime) Then
            If m_LastTime > TIME Then
                m_LastTime = TIME
            End If
            m_AccumulatedAmpHrs = CType(m_AccumulatedAmpHrs + (Load / Million * (TIME - m_LastTime) / 3600), Double)
            m_LastTime = TIME
        End If
        txtAmpHrs.Text = m_AccumulatedAmpHrs.ToString("0.000")
        If TestRunning Then
            txtStatus.Text = "Test Running"
            btnStop.Enabled = True
        End If

        If StopVStop Then
            txtStatus.Text = "Test Stopped:" & vbCrLf & "Voltage Cutoff"
        ElseIf AbortTemp Then
            txtStatus.Text = "Test Stopped:" & vbCrLf & "High Temp"
        ElseIf CurrentLimit Then
            txtStatus.Text = txtStatus.Text + ":" & vbCrLf & "Current Limited"
        ElseIf Not TestRunning Then
            txtStatus.Text = "Idle"
        End If
    End Sub

#End Region
#Region "Event Handlers"
    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles Me.FormClosing
        m_MPUSBEngine.SendSetStatus(True, False, 0, MPUSBEngine.FanLEDControl.FirmwareControl,
                                    MPUSBEngine.FanLEDControl.FirmwareControl,
                                    MPUSBEngine.FanLEDControl.FirmwareControl, 0, 0, True, 0)
        m_MPUSBEngine.Disconnect()
    End Sub

    Private Sub btnStop_Click(sender As System.Object, e As System.EventArgs) Handles btnStop.Click
        m_MPUSBEngine.SendSetStatus(True, False, 0, MPUSBEngine.FanLEDControl.FirmwareControl,
                                    MPUSBEngine.FanLEDControl.FirmwareControl,
                                    MPUSBEngine.FanLEDControl.FirmwareControl, 0, 0, True, 0)
    End Sub

    Private Sub btnSetStatus_Click(sender As System.Object, e As System.EventArgs) Handles btnSetStatus.Click
        If Not m_TestRunning Then
            m_AccumulatedAmpHrs = 0.0
            m_LastTime = 0
        End If
        m_MPUSBEngine.SendSetStatus(True, True, nUDLoad.Value * Million,
                                    MPUSBEngine.FanLEDControl.FirmwareControl,
                                    MPUSBEngine.FanLEDControl.FirmwareControl,
                                    MPUSBEngine.FanLEDControl.FirmwareControl,
                                    0, 0, True, nUDCutoff.Value * Million)
    End Sub

#End Region
#Region "Connect/Communications"

    Private Function ConnectCBA() As Boolean
        Dim connected As Boolean = m_MPUSBEngine.Connect
        If (connected _
                    AndAlso Not m_MPUSBEngine.sendGetConfig) Then
            connected = False
        End If
        Me.btnSetStatus.Enabled = True
        txtStatus.Text = "Idle"
        Return connected
    End Function

    Private Sub DisconnectCBA()
        m_MPUSBEngine.SendSetStatus(True, False, 0, MPUSBEngine.FanLEDControl.FirmwareControl,
                            MPUSBEngine.FanLEDControl.FirmwareControl,
                            MPUSBEngine.FanLEDControl.FirmwareControl, 0, 0, True, 0)
        Me.Text = "Simple CBAIV"
        txtStatus.Text = "Not Connected"
        Me.btnSetStatus.Enabled = False
        m_MPUSBEngine.Disconnect()
    End Sub

    ''' <summary>
    ''' Sends the given message, checking for an acknowledgment
    ''' </summary>
    ''' <param name="message"></param>
    ''' <param name="timeoutMS"></param>
    ''' <param name="retryMS"></param>
    ''' <returns></returns>
    Private Function SendWithAck(ByVal message() As Byte, ByVal timeoutMS As UInt32, ByVal retryMS As UInt32) As Boolean
        Dim packetArray() As Byte = New Byte((4) - 1) {}
        'temporary alottment
        Dim retry As Stopwatch = New Stopwatch
        retry.Reset()
        retry.Start()
        Dim giveUp As Stopwatch = New Stopwatch
        giveUp.Reset()
        giveUp.Start()
        If Not m_MPUSBEngine.sendCustomMessage(message) Then
            Return False
        End If

        While ((packetArray Is Nothing) _
                    OrElse (packetArray(0) <> 97))
            packetArray = m_MPUSBEngine.readBulkData(97, CType((message.Length + 1), Byte))
            If (retry.ElapsedMilliseconds > retryMS) Then
                If Not m_MPUSBEngine.sendCustomMessage(message) Then
                    Return False
                End If
                retry.Reset()
                retry.Start()
            End If
            If (giveUp.ElapsedMilliseconds > timeoutMS) Then
                MessageBox.Show("No ACK on Message")
                DisconnectCBA()
                Return False
            End If

        End While
        Return True
    End Function
#End Region
End Class
