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

Imports SimpleCBAIV.CBADevice
Imports System.Threading
Imports System.IO

Public Class MPUSBEngine
#Region "Constants"
    Private Const CBAIV_vid_pid As String = "vid_2405&pid_0005"

    ''' <summary>
    ''' Driver ID
    ''' </summary>
    Private Const pEP As String = "\MCHP_EP1"
#Region "Outgoing start bytes"

    Private Const Start_Get_Config As Byte = 67

    Private Const Start_Set_Status As Byte = 83

    Private Const Start_Cal_Load_Pos As Byte = 76

    Private Const Start_Cal_Voltage_Pos As Byte = 86

    Private Const Start_Reset_Pic As Byte = 82

    Private Const Start_Restart_Bootloader As Byte = 65

    Private Const Start_Send_Bootloader_Fragment As Byte = 66
#End Region
#Region "Incoming Start Bytes"

    Private Const Start_Send_Config As Byte = 99

    Private Const Start_Ack As Byte = 97

    Private Const Start_Send_Status As Byte = 115
#End Region
#Region "Fan and LED Control Positions"

    Public Const Val_Fan_LED_No_Change As Byte = 0

    Public Const Val_Fan_LED_Firmware_Control As Byte = 1

    Public Const Val_Fan_LED_Off As Byte = 2

    Public Const Val_Fan_LED_On As Byte = 3

    ''' <summary>
    ''' Only used on the data from the CBA device
    ''' </summary>
    Public Const Pos_Fan_LED_On As Byte = 7
#End Region
#Region "FLAGS"

    Private Const Pos_Flag_Change As Byte = 0

    Private Const Pos_Flag_Running As Byte = 1

    Private Const Pos_PWM_Voltage_Disabled As Byte = 2

    Private Const Pos_Calibrate As Byte = 3

    Private Const Pos_Power_Exceeded As Byte = 4

    Private Const Pos_Abort_Temperature As Byte = 5

    Private Const Pos_Use_VStop As Byte = 6

    Private Const Pos_Stop_VStop As Byte = 7
#End Region
#Region "Message Byte Positions"

    Private Const bPos_Send_Status_FLAGS As Byte = 1

    Private Const bPos_Send_Status_LOAD As Byte = 3

    Private Const bPos_Send_Status_FAN As Byte = 7

    Private Const bPos_Send_Status_LED1 As Byte = 8

    Private Const bPos_Send_Status_LED2 As Byte = 9

    Private Const bPos_Send_Status_IOTRIS As Byte = 10

    Private Const bPos_Send_Status_IOPORT As Byte = 11

    Private Const bPos_Send_Status_INT_TEMPERATURE As UInt16 = 12

    Private Const bPos_Send_Status_EXT_TEMPERATURE As UInt16 = 14

    Private Const bPos_Send_Status_DETECT As Byte = 16

    Private Const bPos_Send_Status_VOLTAGE As Byte = 20

    Private Const bPos_Send_Status_VStop As Byte = 24

    Private Const bPos_Send_Status_Time As Byte = 28
#End Region

#Region "Private consts"
    Private Const bPos_Set_Status_FLAGS As Byte = 1

    Private Const bPos_Set_Status_LOAD As Byte = 3

    Private Const bPos_Set_Status_FAN As Byte = 7

    Private Const bPos_Set_Status_LED1 As Byte = 8

    Private Const bPos_Set_Status_LED2 As Byte = 9

    Private Const bPos_Set_Status_IOTRIS As Byte = 10

    Private Const bPos_Set_Status_IOPORT As Byte = 11

    Private Const bPos_Set_Status_VSTOP As Byte = 12


    Private Const bPos_Cal_Pos_POS As Byte = 1

    Private Const bPos_Cal_Pos_READING As Byte = 2

    Private Const bPos_Send_Config_HW_Ver As Byte = 1

    Private Const bPos_Send_Config_FW_Ver_MAJ As Byte = 2

    Private Const bPos_Send_Config_FW_Ver_MIN As Byte = 3

    Private Const bPos_Send_Config_SERIAL As Byte = 4

    Private Const bPos_Send_Config_MAX_LOAD As Byte = 8

    Private Const bPos_Send_Config_MIN_LOAD As Byte = 12
#End Region

#Region "Send_SensorValue"

    Private Const byte_SendSensorVal_Start As Byte = 122

    Private Const bPos_SendSensorVal_FINE_PWM As Byte = 1

    Private Const bPos_SendSensorVal_COARSE_PWM As Byte = 3

    Private Const bPos_SendSensorVal_AN0_4p5V As Byte = 5

    Private Const bPos_SendSensorVal_AN1_60V As Byte = 7

    Private Const bPos_SendSensorVal_AN2_15V As Byte = 9

    Private Const bPos_SendSensorVal_AN4_40A As Byte = 11

    Private Const bPos_SendSensorVal_AN8_EXT_TH As Byte = 13

    Private Const bPos_SendSensorVal_AN9_100A As Byte = 15

    Private Const bPos_SendSensorVal_AN10_INT_TH As Byte = 17

    Private Const bPos_SendSensorVal_CALIB_BMP As Byte = 19

    Private Const bPos_SendSensorVal_MAGIC As Byte = 20

    Private Const bPos_SendSensorVal_SIZE As Byte = 21
#End Region
#Region "Message Byte Counts"

    Private Const bCount_Set_Status As Byte = 9

    Private Const bCount_Send_Status As Byte = 17

    Private Const bCount_Cal_Pos As Byte = 6

    Private Const bCount_Send_PWM_Raw As Byte = 4

    Private Const bCount_Send_Raw_Sensor As Byte = 15
#End Region

#End Region
#Region "Enums"

    Public Enum FanLEDControl As Byte

        NoChange = Val_Fan_LED_No_Change

        FirmwareControl = Val_Fan_LED_Firmware_Control

        Off = Val_Fan_LED_Off

        Onn = Val_Fan_LED_On
    End Enum
#End Region
#Region "Member Vars"

    Private myMPFunctions As MPFunctions = New MPFunctions

    ''' <summary>
    ''' timer to track the data from the device
    ''' </summary>
    Private m_tmrRead As System.Windows.Forms.Timer = New System.Windows.Forms.Timer

    Private m_fanState As Boolean = False

    Private m_LED1State As Boolean = False

    Private m_LED2State As Boolean = False

    'Private m_busy As Boolean = False

    ''' <summary>
    ''' desired current, to be sent to the device, microamps
    ''' Load * 10^(-6) = amps
    ''' </summary>
    Private m_Load As UInt32

    ''' <summary>
    ''' Detected from the unit, microamps
    ''' Detect * 10^(-6) = amps
    ''' </summary>
    Private m_Detect As UInt32

    ''' <summary>
    ''' Detected from the unit, microvolts
    ''' voltage * 10^(-6) = volts
    ''' </summary>
    Private m_Voltage As UInt32

    ''' <summary>
    ''' Used to update the form
    ''' </summary>
    Private m_parent As frmMain

    ''' <summary>
    ''' Represents the device's serial number
    ''' </summary>
    Private m_serialNumber As UInt32
#End Region
#Region "Getters and Setters"

    ' Public Property Busy As Boolean
    '     Get
    '         Return m_busy
    '     End Get
    '     Set(value As Boolean)
    '         m_busy = value
    '     End Set
    ' End Property

    ''' <summary>
    ''' True = on, False = off
    ''' </summary>
    Public ReadOnly Property LED1_on As Boolean
        Get
            Return m_LED1State
        End Get
    End Property

    ''' <summary>
    ''' True = on, False = off
    ''' </summary>
    Public ReadOnly Property LED2_on As Boolean
        Get
            Return m_LED2State
        End Get
    End Property

    ''' <summary>
    ''' True = on, False = off
    ''' </summary>
    Public ReadOnly Property Fan_on As Boolean
        Get
            Return m_fanState
        End Get
    End Property

    ''' <summary>
    ''' Voltage apparent to the CBA
    ''' </summary>
    Public ReadOnly Property Voltage As UInt32
        Get
            Return m_Voltage
        End Get
    End Property

    ''' <summary>
    ''' Load the CBA reads
    ''' </summary>
    Public ReadOnly Property Detect As UInt32
        Get
            Return m_Detect
        End Get
    End Property

    ''' <summary>
    ''' Intended load for the CBA
    ''' </summary>
    Public ReadOnly Property Load As UInt32
        Get
            Return m_Load
        End Get
    End Property

    ''' <summary>
    ''' Returns whether or not the device is connected via the MPLab USB driver
    ''' </summary>
    Public Property Connected As Boolean
        Get
            Return myMPFunctions.attached
        End Get
        Set(value As Boolean)
            myMPFunctions.attached = value
        End Set
    End Property

    ''' <summary>
    ''' represents the CBA's serial number
    ''' </summary>
    Public ReadOnly Property SerialNumber As UInt32
        Get
            Return m_serialNumber
        End Get
    End Property
#End Region

#Region "Constructors"
    Public Sub New(ByVal parent As frmMain)
        m_parent = parent
    End Sub
#End Region
#Region "REad/Write"


    ''' <summary>
    ''' m_tmrRead's tick event
    ''' reads the most recent packet, and adjusts the data to match.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub m_tmrRead_Tick(ByVal sender As Object, ByVal e As EventArgs)
        dataUpdate()
    End Sub

    Private Sub m_pollConfigData(ByVal sender As Object, ByVal e As EventArgs)
        'config poll
    End Sub

    Private Sub m_pollAck(ByVal sender As Object, ByVal e As EventArgs)
        'poll for an ack
        'check to see if we have an ack, and if so, 
        'check for the code inside it, and read the whole ack.
    End Sub

    ''' <summary>
    ''' Force a timer tick event
    ''' </summary>
    Public Sub TICK()
        dataUpdate()
    End Sub

    ''' <summary>
    ''' reads the most recent data packet, and changes the data to match
    ''' </summary>
    Private Function dataUpdate() As Boolean
        Dim retVal() As Byte = New Byte((32) - 1) {}
        retVal = readBulkData(115, 60)

        Try
            Dim load As UInt32 = BitConverter.ToUInt32(retVal, bPos_Send_Status_LOAD)
            Dim detect As UInt32 = BitConverter.ToUInt32(retVal, bPos_Send_Status_DETECT)
            Dim voltage As UInt32 = BitConverter.ToUInt32(retVal, bPos_Send_Status_VOLTAGE)
            Dim extTemperature As UInt16 = BitConverter.ToUInt16(retVal, bPos_Send_Status_EXT_TEMPERATURE)
            Dim intTemperature As UInt16 = BitConverter.ToUInt16(retVal, bPos_Send_Status_INT_TEMPERATURE)
            Dim VStopVal As UInt32 = BitConverter.ToUInt32(retVal, bPos_Send_Status_VStop)
            Dim TimeVal As UInt32 = BitConverter.ToUInt32(retVal, bPos_Send_Status_Time)

            Dim fanOn As Boolean = (retVal(bPos_Send_Status_FAN) And (1 << Pos_Fan_LED_On))
            Dim LED1On As Boolean = (retVal(bPos_Send_Status_LED1) And (1 << Pos_Fan_LED_On))
            Dim LED2On As Boolean = (retVal(bPos_Send_Status_LED2) And (1 << Pos_Fan_LED_On))
            Dim chngTestState As Boolean = (retVal(bPos_Send_Status_FLAGS) And (1 << Pos_Flag_Change))
            Dim tstRun As Boolean = (retVal(bPos_Send_Status_FLAGS) And (1 << Pos_Flag_Running))
            Dim useVStop As Boolean = (retVal(bPos_Send_Status_FLAGS) And (1 << Pos_Use_VStop))
            Dim StopVStop As Boolean = (retVal(bPos_Send_Status_FLAGS) And (1 << Pos_Stop_VStop))
            Dim PowerExceeded As Boolean = (retVal(bPos_Send_Status_FLAGS) And (1 << Pos_Power_Exceeded))
            Dim PWMDisabled As Boolean = (retVal(bPos_Send_Status_FLAGS) And (1 << Pos_PWM_Voltage_Disabled))
            Dim CalibStatus As Boolean = (retVal(bPos_Send_Status_FLAGS) And (1 << Pos_Calibrate))
            Dim HighTempAbort As Boolean = (retVal(bPos_Send_Status_FLAGS) And (1 << Pos_Abort_Temperature))
            m_parent.RefreshStatus(retVal(bPos_Send_Status_FAN), retVal(bPos_Send_Status_LED1), _
                                    retVal(bPos_Send_Status_LED2), load, detect, voltage, intTemperature, _
                        extTemperature, fanOn, LED1On, LED2On, chngTestState, tstRun, _
                       retVal(bPos_Send_Status_IOTRIS), retVal(bPos_Send_Status_IOPORT), _
                      useVStop, StopVStop, VStopVal, TimeVal, PWMDisabled, CalibStatus, _
                     PowerExceeded, HighTempAbort)
            Return True
        Catch e As Exception
            'Throw
        End Try
        Return False
    End Function

    ''' <summary>
    ''' primarily used when an ack is expected.
    ''' </summary>
    ''' <param name="Type">type of message detected</param>
    ''' <param name="size">number of bytes expected</param>
    ''' <returns>message if type is correct. Empty set id the type is not correct</returns>
    Public Function readBulkData(ByVal Type As Byte, ByVal size As Byte) As Byte()
        size = 60
        Dim retVal() As Byte = New Byte((size) - 1) {}
        Dim bytesToTransfer As Integer = size
        Dim bytesTransferred() As UInt32 = New UInt32((1) - 1) {}
        Try
            'read this handle, retVal as the place to put the data, [size] bytes, 0 transferred, 100 milliseconds
            MPFunctions._MPUSBRead(myMPFunctions.EP1INHandle, retVal, bytesToTransfer, bytesTransferred, 100)
        Catch e As Exception
            'handle the exception
            'MessageBox.Show("NO");
            Return Nothing
        End Try
        If (bytesTransferred(0) = 0) Then
            Return Nothing
        End If
        Dim message As String = "Gett: "
        Dim i As Integer = 0
        Do While (i < bytesTransferred(0))
            message = (message _
                        + (retVal(i).ToString("X") + " "))
            i = (i + 1)
        Loop

        'If this is not the desired type, return the empty array.
        If (retVal(0) <> Type) Then
            Return Nothing
        End If
        Dim retVal2() As Byte = New Byte((bytesTransferred(0)) - 1) {}
        i = 0
        Do While (i < bytesTransferred(0))
            retVal2(i) = retVal(i)
            i = (i + 1)
        Loop
        retVal = retVal2
        Return retVal
    End Function

    ''' <summary>
    ''' Sends the given Byte array to the CBA device
    ''' </summary>
    ''' <param name="bulkData"></param>
    ''' <returns></returns>
    Private Function writeBulkData(ByVal bulkData() As Byte) As Boolean
        Dim message As String = "Send: "
        Dim i As Integer = 0
        Do While (i < bulkData.Length)
            message = (message _
                        + (bulkData(i).ToString("X") + " "))
            i = (i + 1)
        Loop


        Dim OutputBulkDataBuffer() As Byte = bulkData
        Dim bytesToTransfer As Integer = bulkData.Length
        Dim bytesTransferred() As UInteger = New UInteger((2) - 1) {}
        Try
            MPFunctions._MPUSBWrite(myMPFunctions.EP1OUTHandle, OutputBulkDataBuffer, bytesToTransfer, bytesTransferred, 1000)
        Catch e As Exception
            Return False
        End Try
        Return True
    End Function
#End Region
#Region "Connections"

    ''' <summary>
    ''' Uses the Microchip driver to connect to the USB board.
    ''' </summary>
    Public Function Connect() As Boolean
        If (Not myMPFunctions.attached _
                    AndAlso (MPFunctions._MPUSBGetDeviceCount(CBAIV_vid_pid) <> 0)) Then
            myMPFunctions.EP1OUTHandle = MPFunctions._MPUSBOpen(0, CBAIV_vid_pid, pEP, 0, 0)
            myMPFunctions.EP1INHandle = MPFunctions._MPUSBOpen(0, CBAIV_vid_pid, pEP, 1, 0)
            'status = connected.
            If (myMPFunctions.EP1INHandle.ToInt32 < 0 Or myMPFunctions.EP1OUTHandle.ToInt32 < 0) Then
                Return False
            End If
            myMPFunctions.attached = True
            'connected, start the timer for updates
            AddHandler m_tmrRead.Tick, AddressOf Me.m_tmrRead_Tick

            m_tmrRead.Enabled = True
            'interval in milliseconds
            m_tmrRead.Interval = 5
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub Disconnect()
        sendCustomMessage(New CBAIVPacketSetStatus(0, 0, CBAIVPacketSetStatus.FanLEDControl.FirmwareControl, CBAIVPacketSetStatus.FanLEDControl.FirmwareControl, CBAIVPacketSetStatus.FanLEDControl.FirmwareControl, 0, 0, 0).ToByteArray)
        If myMPFunctions.attached Then
            'myMPFunctions.EP1OUTHandle = 
            MPFunctions._MPUSBClose(myMPFunctions.EP1OUTHandle)
            'MPFunctions._MPUSBOpen(0, charger_vid_pid, pEP, 0, 0);
            MPFunctions._MPUSBClose(myMPFunctions.EP1INHandle)
            'myMPFunctions.EP1INHandle = MPFunctions._MPUSBOpen(0, charger_vid_pid, pEP, 1, 0);
            'status = connected.
            myMPFunctions.attached = False
            'disconnected, stop the timer updates
            RemoveHandler m_tmrRead.Tick, AddressOf Me.m_tmrRead_Tick

            m_tmrRead.Enabled = False
        End If
    End Sub
#End Region
#Region "Send messages"

    ''' <summary>
    ''' Sends the device a status command based on passed-in information
    ''' </summary>
    ''' <param name="UpdateTest">True = update TestRun and Load, False = do not update</param>
    ''' <param name="TestRun">True = run, false = stop</param>
    ''' <param name="load">Desired current in microamps</param>
    ''' <param name="fan"></param>
    ''' <param name="LED1"></param>
    ''' <param name="LED2"></param>
    ''' <returns></returns>
    Public Function SendSetStatus(ByVal UpdateTest As Boolean, ByVal TestRun As Boolean, ByVal load As UInt32, ByVal fan As Byte, ByVal LED1 As Byte, ByVal LED2 As Byte, ByVal IOTRIS As Byte, ByVal IOPORT As Byte, ByVal UseVStop As Boolean, ByVal VStop As UInt32) As Boolean
        Dim SetStatus() As Byte = New Byte((16) - 1) {}
        SetStatus(0) = Start_Set_Status
        SetStatus(bPos_Set_Status_FLAGS) = 0
        If UpdateTest Then
            SetStatus(bPos_Set_Status_FLAGS) = (SetStatus(bPos_Set_Status_FLAGS) Or (1 << Pos_Flag_Change))
        End If
        If TestRun Then
            SetStatus(bPos_Set_Status_FLAGS) = (SetStatus(bPos_Set_Status_FLAGS) Or (1 << Pos_Flag_Running))
        End If
        If UseVStop Then
            SetStatus(bPos_Set_Status_FLAGS) = (SetStatus(bPos_Set_Status_FLAGS) Or (1 << Pos_Use_VStop))
        End If
        Dim temp() As Byte = BitConverter.GetBytes(load)
        Dim i As Integer = 0
        Do While (i < temp.Length)
            SetStatus((bPos_Set_Status_LOAD + i)) = temp(i)
            i = (i + 1)
        Loop
        SetStatus(bPos_Set_Status_FAN) = fan
        SetStatus(bPos_Set_Status_LED1) = LED1
        SetStatus(bPos_Set_Status_LED2) = LED2
        SetStatus(bPos_Set_Status_IOTRIS) = IOTRIS
        SetStatus(bPos_Set_Status_IOPORT) = IOPORT
        temp = BitConverter.GetBytes(VStop)
        i = 0
        Do While (i < temp.Length)
            SetStatus((bPos_Set_Status_VSTOP + i)) = temp(i)
            i = (i + 1)
        Loop

        Return writeBulkData(SetStatus)
    End Function

    ''' <summary>
    ''' gets the raw senesor values
    ''' </summary>
    ''' <returns></returns>
    Public Function sendGetRawSensorValue() As Boolean
        Dim sendVal() As Byte = New Byte((1) - 1) {}
        sendVal(0) = 90
        writeBulkData(sendVal)
        Dim packetArray() As Byte = New Byte((21) - 1) {}
        Dim retry As Stopwatch = New Stopwatch
        retry.Reset()
        retry.Start()

        While ((packetArray Is Nothing) _
                    OrElse (packetArray(0) = 0))
            packetArray = readBulkData(byte_SendSensorVal_Start, 21)
            If (retry.ElapsedMilliseconds > 1000) Then
                writeBulkData(sendVal)
            End If
            If (retry.ElapsedMilliseconds > 10000) Then
                Return False
            End If

        End While
        '"\tAN0_4.5V: " + Bitc
        Return True
    End Function

    Public Function sendGetConfig() As Boolean
        Dim sendVal() As Byte = New Byte((1) - 1) {}
        sendVal(0) = Start_Get_Config
        writeBulkData(sendVal)
        Dim packetArray() As Byte = New Byte((4) - 1) {}
        'temporary alottment
        Dim retry As Stopwatch = New Stopwatch
        retry.Reset()
        retry.Start()
        Dim giveup As Stopwatch = New Stopwatch
        giveup.Reset()
        giveup.Start()

        While ((packetArray Is Nothing) _
                    OrElse (packetArray(0) = 0))
            packetArray = readBulkData(Start_Send_Config, 16)
            Thread.Sleep(5)
            If (retry.ElapsedMilliseconds > 500) Then
                writeBulkData(sendVal)
                retry.Reset()
                retry.Start()
            End If
            If (giveup.ElapsedMilliseconds > 10000) Then
                Return False
            End If

        End While
        'extract the serialNumber
        m_serialNumber = BitConverter.ToUInt32(packetArray, bPos_Send_Config_SERIAL)
        m_parent.Text = "Simple CBAIV " + CType(m_serialNumber, UInt32).ToString("G")
        Return True
    End Function

    Public Function sendResetPic() As Boolean
        Dim sendVal() As Byte = New Byte((1) - 1) {}
        sendVal(0) = Start_Reset_Pic
        Return writeBulkData(sendVal)
    End Function

    Public Function sendRestartBootloader() As Boolean
        Dim sendVal() As Byte = New Byte((1) - 1) {}
        sendVal(0) = Start_Restart_Bootloader
        Return writeBulkData(sendVal)
    End Function

    ''' <summary>
    ''' Sends the bootloader fragment sent in the argument
    ''' </summary>
    ''' <param name="Fragment">Bootloader Fragment, broken into bytes.</param>
    ''' <returns>false if message too big or other message error</returns>
    Public Function sendBootloaderFragment(ByVal Fragment() As Byte) As Boolean
        If (Fragment.Length >= 250) Then
            Return False
        End If
        Dim sendVal() As Byte = New Byte(((Fragment.Length + 1)) - 1) {}
        sendVal(0) = Start_Reset_Pic
        Dim i As Integer = 0
        Do While (i < Fragment.Length)
            sendVal((i + 1)) = Fragment(i)
            i = (i + 1)
        Loop
        Return writeBulkData(sendVal)
    End Function

    Public Overloads Function sendLoadCalPosition(ByVal POS As Byte, ByVal READING As UInt32) As Boolean
        Return sendLoadCalPosition(POS, BitConverter.GetBytes(READING))
    End Function

    Public Overloads Function sendLoadCalPosition(ByVal POS As Byte, ByVal READING() As Byte) As Boolean
        Dim sendVal() As Byte = New Byte((6) - 1) {}
        sendVal(0) = Start_Cal_Load_Pos
        sendVal(2) = POS
        Dim i As Integer = 0
        Do While (i < 4)
            sendVal((i + 3)) = READING(i)
            i = (i + 1)
        Loop
        Return writeBulkData(sendVal)
    End Function

    Public Overloads Function sendVoltageCalPosition(ByVal POS As Byte, ByVal READING As UInt32) As Boolean
        Return sendVoltageCalPosition(POS, BitConverter.GetBytes(READING))
    End Function

    Public Overloads Function sendVoltageCalPosition(ByVal POS As Byte, ByVal READING() As Byte) As Boolean
        Dim sendVal() As Byte = New Byte((6) - 1) {}
        sendVal(0) = Start_Cal_Load_Pos
        sendVal(2) = POS
        Dim i As Integer = 0
        Do While (i < 4)
            sendVal((i + 3)) = READING(i)
            i = (i + 1)
        Loop
        Return writeBulkData(sendVal)
    End Function

    Public Function sendCustomMessage(ByVal sendVal As Byte()) As Boolean
        Return writeBulkData(sendVal)
    End Function

    ''' <summary>
    ''' Sends the command to reset the calibration values on the CBA. use only in case of emergency.
    ''' </summary>
    Public Function SendResetCalibration() As Boolean
        Dim time_t() As Byte = New Byte((8) - 1) {}
        Dim epoch As UInt64
10000000:
        time_t = BitConverter.GetBytes(epoch)
        Dim ResetMSG() As Byte = New Byte() {70, 100, time_t(0), time_t(1), time_t(2), time_t(3), time_t(4), 85, 170}
        'MessageBox.Show(temp.ToString() + "\r\n" + time_t[0] + "\r\n" + time_t[1] + "\r\n" + time_t[2] + "\r\n" + time_t[3] + "\r\n" + time_t[4]);
        Dim retry As Stopwatch = New Stopwatch
        retry.Reset()
        retry.Start()
        Dim giveUp As Stopwatch = New Stopwatch
        giveUp.Reset()
        giveUp.Start()
        Dim packetArray() As Byte = Nothing
        'byte[] ResetMSG = { 0x46, 0x64, 0x55, 0xAA };
        writeBulkData(ResetMSG)

        While ((packetArray Is Nothing) _
                    OrElse (packetArray(0) <> 97))
            packetArray = readBulkData(97, CType((ResetMSG.Length + 1), Byte))
            If (retry.ElapsedMilliseconds > 1000) Then
                If Not sendCustomMessage(ResetMSG) Then
                End If
                retry.Reset()
                retry.Start()
            End If
            If (giveUp.ElapsedMilliseconds > 10000) Then
                MessageBox.Show("No ACK on Message")
                giveUp.Reset()
                giveUp.Stop()
                Return False
            End If

        End While
        Return True
    End Function

    'get usb descriptor
    Public Sub Get_Descriptor()
        Dim store() As Byte = New Byte((200) - 1) {}
        'string store = "";
        Dim length() As UInteger = New UInteger((1) - 1) {}
        Dim message As String = ""
        Dim result As UInteger = 0
        Try
            'MPFunctions._MPUSBGetDeviceDescriptor(myMPFunctions.EP1OUTHandle, store, 200, length);
            Dim j As Integer = 0
            Do While (j < 2)
                'MPFunctions._MPUSBGetConfigurationDescriptor(myMPFunctions.EP1OUTHandle, (char)j, store, 200, length);
                'result = MPFunctions._MPUSBGetStringDescriptor(myMPFunctions.EP1INHandle, (char)j, (char)0, store, 200, length);
                result = MPFunctions._MPUSBGetDeviceDescriptor(myMPFunctions.EP1OUTHandle, store, 200, length)
                message = ("Descriptor count: " _
                            + (length(0) + " descriptor:"))
                Dim i As Integer = 0
                Do While (i < length(0))
                    message = (message + (" " + store(i)))
                    i = (i + 1)
                Loop
                message = (message + ("result: " + result))
                j = (j + 1)
            Loop
        Catch e As Exception
            message = "Descriptor not working"
        End Try
    End Sub
#End Region
#Region "Receive messages"
    Public Function rxvConfig() As Byte()
        Dim retVal() As Byte = New Byte((16) - 1) {}
        retVal = readBulkData(Start_Send_Config, 16)
        Dim message As String = "Config: "
        Dim i As Integer = 0
        Do While (i < retVal.Length)
            message = (message _
                        + (retVal(i).ToString("X") + " "))
            i = (i + 1)
        Loop
        Return retVal
    End Function

    Public Function rxvAck() As Byte()
        Dim retVal() As Byte = New Byte((2) - 1) {}
        retVal = readBulkData(Start_Ack, 2)
        Select Case (retVal(1))
        End Select
        Return retVal
    End Function
#End Region
#Region "Bootloader"

    Public Function sendBootLoaderFile(ByVal Filename As String) As Boolean
        Dim readStream As Stream
        Try
            readStream = New FileStream(Filename, FileMode.Open)
            Dim reader As BinaryReader = New BinaryReader(readStream)
            Dim i As Integer = 0
            Dim buffer() As Byte = New Byte((50) - 1) {}
            '240 bytes at a time, because max. message size is 60
            i = 0
            Do While (i < readStream.Length)
                If (i _
                            > (readStream.Length - 50)) Then
                    buffer = New Byte(((readStream.Length - i)) - 1) {}
                    reader.Read(buffer, i, CType((readStream.Length - i), Integer))
                    sendBootloaderFragment(buffer)
                Else
                    reader.Read(buffer, i, 50)
                    sendBootloaderFragment(buffer)
                End If
                i = (i + 50)
            Loop
        Catch e As Exception
            Throw
        End Try
        Return True
    End Function
#End Region
End Class
