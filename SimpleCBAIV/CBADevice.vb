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


Public Class CBADevice
    MustInherit Class CBAPacket
    End Class
#Region "CBAIV Packets"
#Region "CBAIVPacket"

    Class CBAIVPacket
        Inherits CBAPacket

        ''' <summary>
        ''' Returns an instance of a child of CBAIVPacket, 
        ''' depending on the data packet.
        ''' </summary>
        ''' <param name="packet">byte data from device.</param>
        ''' <returns>A new instance of a child of CBAIVPacket.</returns>
        Public Shared Function FromBytes(ByVal packet() As Byte) As CBAIVPacket
            Select Case (packet(0))
                Case CBAIVPacketSetStatus.MsgType
                    ' Report packet.
                    Return New CBAIVPacketSetStatus(packet)
                Case CBAIVPacketSendStatus.MsgType
                    ' Calibration packet.
                    Return New CBAIVPacketSendStatus(packet)
                Case CBAIVPacketCalibratePosition.MsgType
                    ' Calibration packet.
                    Return New CBAIVPacketCalibratePosition(packet)
                Case CBAIVPacketBootloaderFragment.MsgType
                    ' Calibration packet.
                    Return New CBAIVPacketBootloaderFragment(packet)
                Case CBAIVPacketAck.MsgType
                    ' Calibration packet.
                    Return New CBAIVPacketAck(packet)
                Case CBAIVPacketResetPic.MsgType
                    ' Calibration packet.
                    Return New CBAIVPacketResetPic
                Case CBAIVRestartBootloader.MsgType
                    ' Calibration packet.
                    Return New CBAIVRestartBootloader
                Case Else
                    Throw New ApplicationException("Unknown packet")
            End Select
        End Function

        Public Overridable Function DbgString(ByVal separator As String) As String
            Return Nothing
        End Function
    End Class
#End Region

#Region "CBAIVPacketSetStatus"

    Class CBAIVPacketSetStatus
        Inherits CBAIVPacket

#Region "Member Variables"
        Public Const MsgType As Byte = 83

        Private Const PacketSize As Byte = 16

        Private m_Flags As UInt16 = 0

        Private m_Load As UInt32 = 0

        Private m_Fan As Byte = 0

        Private m_LED1 As Byte = 0

        Private m_LED2 As Byte = 0

        Private m_IOTRIS As Byte = 0

        Private m_IOPORT As Byte = 0

        Private m_VSTOP As UInt32 = 0
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
#Region "Set Status byte positions"

        Private Const bPos_Set_Status_FLAGS As Byte = 1

        Private Const bPos_Set_Status_LOAD As Byte = 3

        Private Const bPos_Set_Status_FAN As Byte = 7

        Private Const bPos_Set_Status_LED1 As Byte = 8

        Private Const bPos_Set_Status_LED2 As Byte = 9

        Private Const bPos_Set_Status_IOTRIS As Byte = 10

        Private Const bPos_Set_Status_IOPORT As Byte = 11

        Private Const bPos_Set_Status_VSTOP As Byte = 12
#End Region
#Region "Enums"

        Public Enum FanLEDControl As Byte

            NoChange = Val_Fan_LED_No_Change

            FirmwareControl = Val_Fan_LED_Firmware_Control

            Off = Val_Fan_LED_Off

            Onn = Val_Fan_LED_On
        End Enum
#End Region
#Region "Constructors"

        Public Sub New(ByVal packet() As Byte)
            MyBase.New()
            m_Flags = BitConverter.ToUInt16(packet, bPos_Set_Status_FLAGS)
            m_Load = BitConverter.ToUInt32(packet, bPos_Set_Status_LOAD)
            'convert to uint32, packet, starting at 2
            m_Fan = packet(bPos_Set_Status_FAN)
            m_LED1 = packet(bPos_Set_Status_LED1)
            m_LED2 = packet(bPos_Set_Status_LED2)
            m_IOTRIS = packet(bPos_Set_Status_IOTRIS)
            m_IOPORT = packet(bPos_Set_Status_IOPORT)
            m_VSTOP = BitConverter.ToUInt32(packet, bPos_Set_Status_VSTOP)
        End Sub

        ''' <summary>
        ''' Constructor for the packet from the individual values
        ''' </summary>
        ''' <param name="Flags">Flags bytes</param>
        ''' <param name="Load">Load, in microAmps</param>
        ''' <param name="Fan">Fan control byte</param>
        ''' <param name="LED1">LED1 control byte</param>
        ''' <param name="LED2">LED2 control byte</param>
        ''' <param name="IOTRIS">IOTRIS control byte</param>
        ''' <param name="IOPORT">IOPORT values</param>
        Public Sub New(ByVal Flags As UInt16, ByVal Load As UInt32, ByVal Fan As FanLEDControl, ByVal LED1 As FanLEDControl, ByVal LED2 As FanLEDControl, ByVal IOTRIS As Byte, ByVal IOPORT As Byte, ByVal VSTOP As UInt32)
            MyBase.New()
            m_Flags = Flags
            m_Load = Load
            m_Fan = CType(Fan, Byte)
            m_LED1 = CType(LED1, Byte)
            m_LED2 = CType(LED2, Byte)
            m_IOTRIS = IOTRIS
            m_IOPORT = IOPORT
            m_VSTOP = VSTOP
        End Sub
#End Region
#Region "Output byte array"

        Public Function ToByteArray() As Byte()
            Dim outgoingPacket() As Byte = New Byte((PacketSize) - 1) {}
            outgoingPacket(0) = MsgType
            'insert load as a 4-byte chunk
            Dim temp() As Byte = BitConverter.GetBytes(m_Load)
            Dim i As Integer = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Set_Status_LOAD + i)) = temp(i)
                i = (i + 1)
            Loop
            temp = BitConverter.GetBytes(m_Flags)
            i = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Set_Status_FLAGS + i)) = temp(i)
                i = (i + 1)
            Loop
            outgoingPacket(bPos_Set_Status_FAN) = m_Fan
            outgoingPacket(bPos_Set_Status_LED1) = m_LED1
            outgoingPacket(bPos_Set_Status_LED2) = m_LED2
            outgoingPacket(bPos_Set_Status_IOTRIS) = m_IOTRIS
            outgoingPacket(bPos_Set_Status_IOPORT) = m_IOPORT
            temp = BitConverter.GetBytes(m_VSTOP)
            i = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Set_Status_VSTOP + i)) = temp(i)
                i = (i + 1)
            Loop
            Return outgoingPacket
        End Function
#End Region
#Region "Getters"

        Public ReadOnly Property Flags As UInt16
            Get
                Return m_Flags
            End Get
        End Property

        ''' <summary>
        ''' The desired current draw, in microamps
        ''' </summary>
        Public ReadOnly Property Load As UInt32
            Get
                Return m_Load
            End Get
        End Property

        ''' <summary>
        ''' Desired fan setting
        ''' </summary>
        Public ReadOnly Property Fan As FanLEDControl
            Get
                Return CType(m_Fan, FanLEDControl)
            End Get
        End Property

        ''' <summary>
        ''' Desired LED1 setting
        ''' </summary>
        Public ReadOnly Property LED1 As FanLEDControl
            Get
                Return CType(m_LED1, FanLEDControl)
            End Get
        End Property

        ''' <summary>
        ''' Desired LED2 setting
        ''' </summary>
        Public ReadOnly Property LED2 As FanLEDControl
            Get
                Return CType(m_LED2, FanLEDControl)
            End Get
        End Property

        ''' <summary>
        ''' Controls the input/output direction of the general purpose IO pins
        ''' </summary>
        Public ReadOnly Property IOTRIS As Byte
            Get
                Return m_IOTRIS
            End Get
        End Property

        ''' <summary>
        ''' Controls the new output state if the bit is in output mode
        ''' </summary>
        Public ReadOnly Property IOPORT As Byte
            Get
                Return m_IOPORT
            End Get
        End Property

        Public ReadOnly Property VSTOP As UInt32
            Get
                Return m_VSTOP
            End Get
        End Property
#End Region
#Region "Strings"

        Public Overrides Function ToString() As String
            Dim retVal As String = ""
            'add things to retVal until it has everything with the given delimiter
            retVal = (retVal + ("Load: " + m_Load))
            retVal = (retVal + ("Flags: " + m_Flags))
            retVal = (retVal + ("Fan: " + m_Fan))
            retVal = (retVal + ("LED1: " + m_LED1))
            retVal = (retVal + ("LED2: " + m_LED2))
            retVal = (retVal + ("IOTRIS: " + m_IOTRIS))
            retVal = (retVal + ("IOPORT: " + m_IOPORT))
            retVal = (retVal + ("VSTOP: " + m_VSTOP))
            Return retVal
        End Function

        ''' <summary>
        ''' Generates a String that can be used for debugging purposes
        ''' </summary>
        ''' <param name="separator">character(s) to be put between values</param>
        ''' <returns>string describing the packet</returns>
        Public Overrides Function DbgString(ByVal separator As String) As String
            Dim retVal As String = ""
            'add things to retVal until it has everything with the given delimiter
            retVal = (retVal + ("Load: " + m_Load.ToString("X")))
            retVal = (retVal _
                        + (separator + ("Flags: " + m_Flags.ToString("X"))))
            retVal = (retVal _
                        + (separator + ("Fan: " + m_Fan.ToString("X"))))
            retVal = (retVal _
                        + (separator + ("LED1: " + m_LED1.ToString("X"))))
            retVal = (retVal _
                        + (separator + ("LED2: " + m_LED2.ToString("X"))))
            retVal = (retVal _
                        + (separator + ("IOTRIS: " + m_IOTRIS.ToString("X"))))
            retVal = (retVal _
                        + (separator + ("IOPORT: " + m_IOPORT.ToString("X"))))
            retVal = (retVal _
                        + (separator + ("VSTOP: " _
                        + (m_VSTOP.ToString("X") + separator))))
            Return retVal
        End Function
#End Region
    End Class
#End Region

#Region "CBAIVPacketSendStatus"

    Class CBAIVPacketSendStatus
        Inherits CBAIVPacket

#Region "Member Variables"
        Public Const MsgType As Byte = 115

        Public Const PacketSize As Byte = 32

        Private m_Flags As UInt16 = 0

        Private m_Load As UInt32 = 0

        Private m_Fan As Byte = 0

        Private m_LED1 As Byte = 0

        Private m_LED2 As Byte = 0

        Private m_IOTRIS As Byte = 0

        Private m_IOPORT As Byte = 0

        Private m_INT_TEMP As UInt16 = 0

        Private m_EXT_TEMP As UInt16 = 0

        Private m_DETECT As UInt32 = 0

        Private m_VOLTAGE As UInt32 = 0

        Private m_TIME As UInt32 = 0

        Private m_VSTOP As UInt32 = 0
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
#Region "Send Status byte position definitions"

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
#Region "Enums"

        Public Enum FanLEDControl As Byte

            NoChange = Val_Fan_LED_No_Change

            FirmwareControl = Val_Fan_LED_Firmware_Control

            Off = Val_Fan_LED_Off

            Onn = Val_Fan_LED_On
        End Enum
#End Region
#Region "constructors"

        Public Sub New(ByVal packet() As Byte)
            MyBase.New()
            m_Flags = BitConverter.ToUInt16(packet, CType(bPos_Send_Status_FLAGS, Integer))
            m_Load = BitConverter.ToUInt32(packet, bPos_Send_Status_LOAD)
            'convert to uint32, packet, starting at 2
            m_Fan = packet(bPos_Send_Status_FAN)
            m_LED1 = packet(bPos_Send_Status_LED1)
            m_LED2 = packet(bPos_Send_Status_LED2)
            m_IOTRIS = packet(bPos_Send_Status_IOTRIS)
            m_IOPORT = packet(bPos_Send_Status_IOPORT)
            m_INT_TEMP = BitConverter.ToUInt16(packet, bPos_Send_Status_INT_TEMPERATURE)
            m_EXT_TEMP = BitConverter.ToUInt16(packet, bPos_Send_Status_EXT_TEMPERATURE)
            m_DETECT = BitConverter.ToUInt32(packet, bPos_Send_Status_DETECT)
            m_VOLTAGE = BitConverter.ToUInt32(packet, bPos_Send_Status_VOLTAGE)
            m_TIME = BitConverter.ToUInt32(packet, bPos_Send_Status_Time)
            m_VSTOP = BitConverter.ToUInt32(packet, bPos_Send_Status_VStop)
        End Sub

        ''' <summary>
        ''' Creates the packet object from values for each data value.
        ''' </summary>
        ''' <param name="Flags">Flags byte</param>
        ''' <param name="Load">Load, in microamps</param>
        ''' <param name="Fan">Fan control, as defined in the enum</param>
        ''' <param name="LED1">LED control, as defined in the enum</param>
        ''' <param name="LED2">LED  control, as defined in the enum</param>
        ''' <param name="IOTRIS">IO direction of the pins</param>
        ''' <param name="IOPORT">Value to be output on the pins</param>
        ''' <param name="Ext_Temp">Temperature read in from external probe</param>
        ''' <param name="Int_Temp">Temperature read from internal probe</param>
        ''' <param name="Detect">Current detected on the CBA</param>
        ''' <param name="Voltage">Voltage detected on the CBA</param>
        Public Sub New(ByVal Flags As UInt16, ByVal Load As UInt32, ByVal Fan As FanLEDControl, ByVal LED1 As FanLEDControl, ByVal LED2 As FanLEDControl, ByVal IOTRIS As Byte, ByVal IOPORT As Byte, ByVal Ext_Temp As UInt16, ByVal Int_Temp As UInt16, ByVal Detect As UInt32, ByVal Voltage As UInt32, ByVal VStop As UInt32, ByVal Time As UInt32)
            MyBase.New()
            m_Flags = Flags
            m_Load = Load
            m_Fan = CType(Fan, Byte)
            m_LED1 = CType(LED1, Byte)
            m_LED2 = CType(LED2, Byte)
            m_IOTRIS = IOTRIS
            m_IOPORT = IOPORT
            m_INT_TEMP = Int_Temp
            m_EXT_TEMP = Ext_Temp
            m_DETECT = Detect
            m_VOLTAGE = Voltage
            m_VSTOP = VStop
            m_TIME = Time
        End Sub
#End Region
#Region "Output byte array"

        Public Function ToByteArray() As Byte()
            Dim outgoingPacket() As Byte = New Byte((PacketSize) - 1) {}
            outgoingPacket(0) = MsgType
            Dim temp() As Byte = BitConverter.GetBytes(m_Flags)
            Dim i As Integer = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Send_Status_FLAGS + i)) = temp(i)
                i = (i + 1)
            Loop
            'insert load as a 4-byte chunk
            temp = BitConverter.GetBytes(m_Load)
            i = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Send_Status_LOAD + i)) = temp(i)
                i = (i + 1)
            Loop
            outgoingPacket(bPos_Send_Status_FAN) = m_Fan
            outgoingPacket(bPos_Send_Status_LED1) = m_LED1
            outgoingPacket(bPos_Send_Status_LED2) = m_LED2
            outgoingPacket(bPos_Send_Status_IOTRIS) = m_IOTRIS
            outgoingPacket(bPos_Send_Status_IOPORT) = m_IOPORT
            'insert internal temperature as a 2-byte chunk
            temp = BitConverter.GetBytes(m_INT_TEMP)
            i = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Send_Status_INT_TEMPERATURE + i)) = temp(i)
                i = (i + 1)
            Loop
            'insert external temperature as a 2-byte chunk
            temp = BitConverter.GetBytes(m_EXT_TEMP)
            i = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Send_Status_EXT_TEMPERATURE + i)) = temp(i)
                i = (i + 1)
            Loop
            'insert detected current as a 4-byte chunk
            temp = BitConverter.GetBytes(m_DETECT)
            i = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Send_Status_DETECT + i)) = temp(i)
                i = (i + 1)
            Loop
            'insert detected voltage as a 4-byte chunk
            temp = BitConverter.GetBytes(m_VOLTAGE)
            i = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Send_Status_VOLTAGE + i)) = temp(i)
                i = (i + 1)
            Loop
            temp = BitConverter.GetBytes(m_TIME)
            i = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Send_Status_Time + i)) = temp(i)
                i = (i + 1)
            Loop
            temp = BitConverter.GetBytes(m_VSTOP)
            i = 0
            Do While (i < temp.Length)
                outgoingPacket((bPos_Send_Status_VStop + i)) = temp(i)
                i = (i + 1)
            Loop
            Return outgoingPacket
        End Function
#End Region
#Region "Strings"

        Public Overrides Function ToString() As String
            Dim retVal As String = ""
            'add things to retVal until it has everything with the given delimiter
            retVal = (retVal + (" Load: " + m_Load))
            retVal = (retVal + (" Flags: " + m_Flags))
            retVal = (retVal + (" Fan: " + m_Fan))
            retVal = (retVal + (" LED1: " + m_LED1))
            retVal = (retVal + (" LED2: " + m_LED2))
            retVal = (retVal + (" IOTRIS: " + m_IOTRIS))
            retVal = (retVal + (" IOPORT: " + m_IOPORT))
            retVal = (retVal + (" Int_Temp: " + m_INT_TEMP))
            retVal = (retVal + (" Ext_Temp: " + m_EXT_TEMP))
            retVal = (retVal + (" Detect_current: " + m_DETECT))
            retVal = (retVal + (" Voltage: " + m_VOLTAGE))
            Return retVal
        End Function

        ''' <summary>
        ''' Generates a String that can be used for debugging purposes
        ''' </summary>
        ''' <param name="separator">character(s) to be put between values</param>
        ''' <returns>string describing the packet</returns>
        Public Overrides Function DbgString(ByVal separator As String) As String
            Dim retVal As String = ""
            'add things to retVal until it has everything with the given delimiter
            retVal = (retVal _
                        + (separator + ("Load: " + m_Load)))
            retVal = (retVal _
                        + (separator + ("Flags: " + m_Flags)))
            retVal = (retVal _
                        + (separator + ("Fan: " + m_Fan)))
            retVal = (retVal _
                        + (separator + ("LED1: " + m_LED1)))
            retVal = (retVal _
                        + (separator + ("LED2: " + m_LED2)))
            retVal = (retVal _
                        + (separator + ("IOTRIS: " + m_IOTRIS)))
            retVal = (retVal _
                        + (separator + ("IOPORT: " _
                        + (m_IOPORT + separator))))
            retVal = (retVal _
                        + (separator + (" Int_Temp: " + m_INT_TEMP)))
            retVal = (retVal _
                        + (separator + (" Ext_Temp: " + m_EXT_TEMP)))
            retVal = (retVal _
                        + (separator + (" Detect_current: " + m_DETECT)))
            retVal = (retVal _
                        + (separator + (" Voltage: " + m_VOLTAGE)))
            Return retVal
        End Function
#End Region
#Region "Getters"

        Public ReadOnly Property Flags As UInt16
            Get
                Return m_Flags
            End Get
        End Property

        ''' <summary>
        ''' The desired current draw, in microamps
        ''' </summary>
        Public ReadOnly Property Load As UInt32
            Get
                Return m_Load
            End Get
        End Property

        ''' <summary>
        ''' Desired fan setting
        ''' </summary>
        Public ReadOnly Property Fan As FanLEDControl
            Get
                Return CType(m_Fan, FanLEDControl)
            End Get
        End Property

        ''' <summary>
        ''' Desired LED1 setting
        ''' </summary>
        Public ReadOnly Property LED1 As FanLEDControl
            Get
                Return CType(m_LED1, FanLEDControl)
            End Get
        End Property

        ''' <summary>
        ''' Desired LED2 setting
        ''' </summary>
        Public ReadOnly Property LED2 As FanLEDControl
            Get
                Return CType(m_LED2, FanLEDControl)
            End Get
        End Property

        ''' <summary>
        ''' Controls the input/output direction of the general purpose IO pins
        ''' </summary>
        Public ReadOnly Property IOTRIS As Byte
            Get
                Return m_IOTRIS
            End Get
        End Property

        ''' <summary>
        ''' Controls the new output state if the bit is in output mode
        ''' </summary>
        Public ReadOnly Property IOPORT As Byte
            Get
                Return m_IOPORT
            End Get
        End Property

        ''' <summary>
        ''' Stores the internal temperature of the CBAIV
        ''' </summary>
        Public ReadOnly Property INT_TEMP As UInt16
            Get
                Return m_INT_TEMP
            End Get
        End Property

        ''' <summary>
        ''' Stores the Temperature from the add-on temp probe
        ''' </summary>
        Public ReadOnly Property EXT_TEMP As UInt16
            Get
                Return m_EXT_TEMP
            End Get
        End Property

        ''' <summary>
        ''' Current detected on the CBA, microamps
        ''' </summary>
        Public ReadOnly Property DETECT As UInt32
            Get
                Return m_DETECT
            End Get
        End Property

        ''' <summary>
        ''' Voltage detected on the CBA, microvolts
        ''' </summary>
        Public ReadOnly Property VOLTAGE As UInt32
            Get
                Return m_VOLTAGE
            End Get
        End Property
#End Region
    End Class
#End Region

#Region "CBAIVPacketCalibratePosition"

    Class CBAIVPacketCalibratePosition
        Inherits CBAIVPacket

        Private m_IDX As Byte = 0

        Private m_Value As UInt32 = 0

        Public Const MsgType As Byte = 76

        Public Const PacketSize As Byte = 8
#Region "Calibrate Position byte positions"

        Private Const bPos_Cal_Pos_IDX As Byte = 1

        Private Const bPos_Cal_Pos_VALUE As Byte = 2
#End Region

        Public Sub New(ByVal packet() As Byte)
            MyBase.New()
            m_IDX = packet(bPos_Cal_Pos_IDX)
            m_Value = BitConverter.ToUInt32(packet, bPos_Cal_Pos_VALUE)
        End Sub

        ''' <summary>
        ''' Builds a calibratePositionPacket from the data values
        ''' </summary>
        ''' <param name="IDX">IDX, 
        ''' 0-4.5V      ->  0
        ''' 4.5-15V     ->  1
        ''' 15-60V      ->  2
        ''' Fine PWM 1  ->  3
        ''' Fine PWM 2  ->  4
        ''' Coarse PWM 1->  5
        ''' Coarse PWM 2->  6</param>
        ''' <param name="Value">Value, Real value of this position as read by the DMM</param>
        Public Sub New(ByVal IDX As Byte, ByVal Value As UInt32)
            MyBase.New()
            m_IDX = IDX
            m_Value = Value
        End Sub

        ''' <summary>
        ''' generates the byte array representing the packet
        ''' </summary>
        ''' <returns>byte array representing the packet</returns>
        Public Function ToByteArray() As Byte()
            Dim outgoingPacket() As Byte = New Byte((PacketSize) - 1) {}
            outgoingPacket(0) = MsgType
            outgoingPacket(bPos_Cal_Pos_IDX) = m_IDX
            Dim temp() As Byte = BitConverter.GetBytes(m_Value)
            Dim checkvalue As Byte = CType((CType(76, Byte) Or IDX), Byte)
            'The operator should be an XOR ^ instead of an OR, but not available in CodeDOM
            Dim i As Integer = 0
            Do While (i < 4)
                outgoingPacket((bPos_Cal_Pos_VALUE + i)) = temp(i)
                checkvalue = checkvalue Xor temp(i)
                i = (i + 1)
            Loop
            outgoingPacket(6) = checkvalue
            outgoingPacket(7) = Not checkvalue
            Return outgoingPacket
        End Function

        ''' <summary>
        ''' IDX for the load calibration
        ''' </summary>
        Public ReadOnly Property IDX As Byte
            Get
                Return m_IDX
            End Get
        End Property

        ''' <summary>
        ''' Value for the load calibration
        ''' </summary>
        Public ReadOnly Property Value As UInt32
            Get
                Return m_Value
            End Get
        End Property

        ''' <summary>
        ''' Debug string
        ''' </summary>
        ''' <param name="delimiter">String or character to put between values</param>
        ''' <returns>debug string</returns>
        Public Overrides Function DbgString(ByVal delimiter As String) As String
            Dim retVal As String = ""
            retVal = (retVal + "Calibrate Position packet: ")
            Dim temp() As Byte = ToByteArray()
            Dim i As Integer = 0
            Do While (i _
                        < (temp.Length - 1))
                retVal = (retVal _
                            + (temp(i).ToString("X") + delimiter))
                i = (i + 1)
            Loop
            retVal = (retVal + temp((temp.Length - 1)).ToString("X"))
            Return retVal
        End Function
    End Class
#End Region

#Region "CBAIVPacketAck"

    Class CBAIVPacketAck
        Inherits CBAIVPacket

        Public Const MsgType As Byte = 97

        Private m_Payload() As Byte = Nothing

        ''' <summary>
        ''' Constructor from the byte array representing the packet
        ''' </summary>
        ''' <param name="packet">Full Packet</param>
        Public Sub New(ByVal packet() As Byte)
            MyBase.New()
            m_Payload = New Byte(((packet.GetUpperBound(0) - 1)) - 1) {}
            Dim i As Integer = 1
            Do While (i < packet.GetUpperBound(0))
                m_Payload((i - 1)) = packet(i)
                i = (i + 1)
            Loop
        End Sub

        ''' <summary>
        ''' Message being acked
        ''' </summary>
        Public ReadOnly Property Payload As Byte()
            Get
                Return m_Payload
            End Get
        End Property
    End Class
#End Region

#Region "CBAIVPacketSendConfig"

    Class CBAIVPacketSendConfig
        Inherits CBAIVPacket

        Public Const MsgType As Byte = 99

        Public Const PacketSize As Byte = 16
#Region "Send Config byte positions"

        Private Const bPos_Send_Config_HW_Ver As Byte = 1

        Private Const bPos_Send_Config_FW_Ver_MAJ As Byte = 2

        Private Const bPos_Send_Config_FW_Ver_MIN As Byte = 3

        Private Const bPos_Send_Config_SERIAL As Byte = 4

        Private Const bPos_Send_Config_MAX_LOAD As Byte = 8

        Private Const bPos_Send_Config_MIN_LOAD As Byte = 12
#End Region
#Region "member vars"

        Private m_HW_VER As Byte = 0

        Private m_FW_VER_MAJ As Byte = 0

        Private m_FW_VER_MIN As Byte = 0

        Private m_SerialNumber As UInt32 = 0

        Private m_MAX_LOAD As UInt32 = 0

        Private m_MIN_LOAD As UInt32 = 0
#End Region
#Region "Constructors"

        Public Sub New(ByVal packet() As Byte)
            MyBase.New()
            m_HW_VER = packet(bPos_Send_Config_HW_Ver)
            m_FW_VER_MAJ = packet(bPos_Send_Config_FW_Ver_MAJ)
            m_FW_VER_MIN = packet(bPos_Send_Config_FW_Ver_MIN)
            m_SerialNumber = BitConverter.ToUInt32(packet, bPos_Send_Config_SERIAL)
            'convert to uint32, packet, starting at bPos_Send_Config_SERIAL
            m_MAX_LOAD = BitConverter.ToUInt32(packet, bPos_Send_Config_MAX_LOAD)
            m_MIN_LOAD = BitConverter.ToUInt32(packet, bPos_Send_Config_MIN_LOAD)
        End Sub

        ''' <summary>
        ''' Constructor from base values
        ''' </summary>
        ''' <param name="HW_VER">Hardware Version</param>
        ''' <param name="FW_VER_MAJ">Major Firmware version</param>
        ''' <param name="FW_VER_MIN">Minor Firmware Version</param>
        ''' <param name="SerialNumber">Serial number for the device</param>
        ''' <param name="MAX_LOAD">Maximum load for the device, microamps</param>
        ''' <param name="MIN_LOAD">minimum load for the device, microamps</param>
        Public Sub New(ByVal HW_VER As Byte, ByVal FW_VER_MAJ As Byte, ByVal FW_VER_MIN As Byte, ByVal SerialNumber As UInt32, ByVal MAX_LOAD As UInt32, ByVal MIN_LOAD As UInt32)
            MyBase.New()
            m_HW_VER = HW_VER
            m_FW_VER_MAJ = FW_VER_MAJ
            m_FW_VER_MIN = FW_VER_MIN
            m_SerialNumber = SerialNumber
            m_MAX_LOAD = MAX_LOAD
            m_MIN_LOAD = MIN_LOAD
        End Sub
#End Region
#Region "Output byte array"

        Public Function ToByteArray() As Byte()
            Dim outPacket() As Byte = New Byte((PacketSize) - 1) {}
            outPacket(bPos_Send_Config_HW_Ver) = m_HW_VER
            outPacket(bPos_Send_Config_FW_Ver_MAJ) = m_FW_VER_MAJ
            outPacket(bPos_Send_Config_FW_Ver_MIN) = m_FW_VER_MIN
            'insert Serial Number as a 4-byte chunk
            Dim temp() As Byte = BitConverter.GetBytes(m_SerialNumber)
            Dim i As Integer = 0
            Do While (i < temp.Length)
                outPacket((bPos_Send_Config_SERIAL + i)) = temp(i)
                i = (i + 1)
            Loop
            'insert Maximum Load as a 4-byte chunk
            temp = BitConverter.GetBytes(m_MAX_LOAD)
            i = 0
            Do While (i < temp.Length)
                outPacket((bPos_Send_Config_MAX_LOAD + i)) = temp(i)
                i = (i + 1)
            Loop
            'insert Minimum Load as a 4-byte chunk
            temp = BitConverter.GetBytes(m_MIN_LOAD)
            i = 0
            Do While (i < temp.Length)
                outPacket((bPos_Send_Config_MIN_LOAD + i)) = temp(i)
                i = (i + 1)
            Loop
            Return outPacket
        End Function
#End Region
#Region "Strings"

        Public Overrides Function ToString() As String
            Dim retVal As String = ""
            'add things to retVal until it has everything with the given delimiter
            retVal = (retVal + (" HW_VER: " + m_HW_VER))
            retVal = (retVal + (" FW_VER_MAJ: " + m_FW_VER_MAJ))
            retVal = (retVal + (" FW_VER_MIN: " + m_FW_VER_MIN))
            retVal = (retVal + (" Serial_Number: " + m_SerialNumber))
            retVal = (retVal + (" MAX_LOAD: " + m_MAX_LOAD))
            retVal = (retVal + (" MIN_LOAD: " + m_MIN_LOAD))
            Return retVal
        End Function

        ''' <summary>
        ''' Generates a String that can be used for debugging purposes
        ''' </summary>
        ''' <param name="separator">character(s) to be put between values</param>
        ''' <returns>string describing the packet</returns>
        Public Overrides Function DbgString(ByVal separator As String) As String
            Dim retVal As String = ""
            'add things to retVal until it has everything with the given delimiter
            retVal = (retVal _
                        + (separator + (" HW_VER: " + m_HW_VER)))
            retVal = (retVal _
                        + (separator + (" FW_VER_MAJ: " + m_FW_VER_MAJ)))
            retVal = (retVal _
                        + (separator + (" FW_VER_MIN: " + m_FW_VER_MIN)))
            retVal = (retVal _
                        + (separator + (" Serial_Number: " + m_SerialNumber)))
            retVal = (retVal _
                        + (separator + (" MAX_LOAD: " + m_MAX_LOAD)))
            retVal = (retVal _
                        + (separator + (" MIN_LOAD: " + m_MIN_LOAD)))
            Return retVal
        End Function
#End Region
#Region "Getters"

        Public ReadOnly Property HW_VER As Byte
            Get
                Return m_HW_VER
            End Get
        End Property

        ''' <summary>
        ''' Major Firmware version
        ''' </summary>
        Public ReadOnly Property FW_VER_MAJ As Byte
            Get
                Return m_FW_VER_MAJ
            End Get
        End Property

        ''' <summary>
        ''' Minor firmware version
        ''' </summary>
        Public ReadOnly Property FE_VER_MIN As Byte
            Get
                Return m_FW_VER_MIN
            End Get
        End Property

        ''' <summary>
        ''' Serial Number for the device
        ''' </summary>
        Public ReadOnly Property Serial_Number As UInt32
            Get
                Return m_SerialNumber
            End Get
        End Property

        ''' <summary>
        ''' Maximum load
        ''' </summary>
        Public ReadOnly Property MAX_Load As UInt32
            Get
                Return m_MAX_LOAD
            End Get
        End Property

        ''' <summary>
        ''' Minimum load
        ''' </summary>
        Public ReadOnly Property MIN_Load As UInt32
            Get
                Return m_MIN_LOAD
            End Get
        End Property
#End Region
    End Class
#End Region

#Region "CBAIVPacketBootloaderFragment"

    Class CBAIVPacketBootloaderFragment
        Inherits CBAIVPacket

        Private m_Fragment() As Byte = Nothing

        Public Const MsgType As Byte = 66

        ''' <summary>
        ''' Creates the BootloaderFragment packet from the passed-in array
        ''' </summary>
        ''' <param name="packet"></param>
        Public Sub New(ByVal packet() As Byte)
            MyBase.New()
            m_Fragment = New Byte(((packet.GetUpperBound(0) - 1)) - 1) {}
            Dim i As Integer = 1
            Do While (i < packet.GetUpperBound(0))
                Fragment((i - 1)) = packet(i)
                i = (i + 1)
            Loop
        End Sub

        ''' <summary>
        ''' Bootloader Fragment
        ''' </summary>
        Public ReadOnly Property Fragment As Byte()
            Get
                Return m_Fragment
            End Get
        End Property
    End Class
#End Region

#Region "CBAIVPacketResetPic"

    Class CBAIVPacketResetPic
        Inherits CBAIVPacket

        Public Const MsgType As Byte = 82

        ''' <summary>
        ''' Makes a packet based on the passed-in array
        ''' </summary>
        Public Sub New()
            MyBase.New()

        End Sub

        ''' <summary>
        ''' Generates the byte array for the packet
        ''' </summary>
        ''' <returns>Byte array for the packet</returns>
        Public Function ToByteArray() As Byte()
            Dim outgoingPacket() As Byte = New Byte((1) - 1) {}
            outgoingPacket(0) = MsgType
            Return outgoingPacket
        End Function
    End Class
#End Region

#Region "CBAIVPacketRestartBootloader"

    Class CBAIVRestartBootloader
        Inherits CBAIVPacket

        Public Const MsgType As Byte = 65

        ''' <summary>
        ''' makes the restartbootloader packet
        ''' </summary>
        Public Sub New()
            MyBase.New()

        End Sub

        ''' <summary>
        ''' Generates the byte array for the packet
        ''' </summary>
        ''' <returns>Byte array for the packet</returns>
        Public Function ToByteArray() As Byte()
            Dim outgoingPacket() As Byte = New Byte((1) - 1) {}
            outgoingPacket(0) = MsgType
            Return outgoingPacket
        End Function
    End Class
#End Region

#Region "CBAIVPacketSetPWMRaw"

    Class CBAIVPacketSetPWMRaw
        Inherits CBAIVPacket
#Region "Constants"
        Public Const MsgType As Byte = 80

        Private Const p_Channel As Byte = 1

        Private Const p_Duty As Byte = 2
#End Region
#Region "Member vars"

        Private m_Channel As Byte = 0

        Private m_Duty As UInt16 = 0
#End Region
#Region "Constructors"

        Public Sub New(ByVal packet() As Byte)
            MyBase.New()
            m_Channel = packet(p_Channel)
            m_Duty = BitConverter.ToUInt16(packet, p_Duty)
        End Sub

        ''' <summary>
        ''' Creates the packet from the passed-in array
        ''' </summary>
        ''' <param name="channel">Channel to be set</param>
        ''' <param name="duty">Duty value to set the channel to.</param>
        Public Sub New(ByVal channel As Byte, ByVal duty As UInt16)
            MyBase.New()
            m_Channel = channel
            m_Duty = duty
        End Sub
#End Region
#Region "Getters"

        Public ReadOnly Property Channel As Byte
            Get
                Return m_Channel
            End Get
        End Property

        ''' <summary>
        ''' Duty value to be changed
        ''' </summary>
        Public ReadOnly Property Duty As UInt16
            Get
                Return m_Duty
            End Get
        End Property
#End Region
#Region "ToByteArray"

        Public Function ToByteArray() As Byte()
            Dim retVal() As Byte = New Byte((6) - 1) {}
            retVal(0) = MsgType
            retVal(1) = m_Channel
            'Duty
            Dim temp() As Byte = BitConverter.GetBytes(m_Duty)
            Dim checkvalue As Byte = CType((CType(80, Byte) Or m_Channel), Byte)
            'The operator should be an XOR ^ instead of an OR, but not available in CodeDOM
            Dim i As Integer = 0
            Do While (i < temp.Length)
                retVal((i + 2)) = temp(i)
                checkvalue = checkvalue Xor temp(i)
                i = (i + 1)
            Loop
            retVal(4) = checkvalue
            retVal(5) = Not checkvalue
            Return retVal
        End Function
#End Region
#Region "Strings"

        Public Overrides Function DbgString(ByVal delimiter As String) As String
            Dim retVal As String = ""
            retVal = (retVal + "Set PWM Raw packet: ")
            Dim temp() As Byte = ToByteArray()
            Dim i As Integer = 0
            Do While (i _
                        < (temp.Length - 1))
                retVal = (retVal _
                            + (temp(i).ToString("X") + delimiter))
                i = (i + 1)
            Loop
            retVal = (retVal + temp((temp.Length - 1)).ToString("X"))
            Return retVal
        End Function
#End Region
    End Class
#End Region

#End Region

#Region "CBADevice base class"

    MustInherit Class CBADevice
        Implements IDisposable

        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                ' Dispose managed members.
            End If
            ' Dispose non-managed members.
            Disconnect()
        End Sub

        ''' <summary>
        ''' Destructor.
        ''' </summary>
        Private Sub New()
            MyBase.New()
            Dispose(False)
        End Sub


        Public MustOverride Sub Connect(ByVal DeviceID As Integer, ByVal DeviceSerial As String)

        ''' <summary>
        ''' Disconnect from the CBA device.
        ''' </summary>
        Public MustOverride Sub Disconnect()

        ''' <summary>
        ''' Set the timeouts for the CBA device.
        ''' </summary>
        ''' <param name="ReadTimeout">Read timeout, ms.</param>
        ''' <param name="WriteTimeout">Write timeout, ms.</param>
        Public MustOverride Sub SetTimeouts(ByVal ReadTimeout As Integer, ByVal WriteTimeout As Integer)

        ''' <summary>
        ''' True if connected.
        ''' </summary>
        Public MustOverride Property Connected As Boolean
        'End Property

        ''' <summary>
        ''' Device index in devices array.
        ''' </summary>
        Public MustOverride Property DeviceIndex As Integer
        'End Property

        ''' <summary>
        ''' USB serial number.
        ''' </summary>
        Public MustOverride Property DeviceSerial As String
        'End Property

        ''' <summary>
        ''' Checks the RX queue for available data.
        ''' </summary>
        ''' <returns>true if bytes exist in the RX queue to be read.</returns>
        Public MustOverride Function BytesAvailable() As Boolean

        ''' <summary>
        ''' Read state from the CBA device. Call BytesAvailable() before this
        ''' to make sure data exists, otherwise this call may block until
        ''' data is available.
        ''' </summary>
        ''' <returns>An object inheriting from CBARpt</returns>
        Public MustOverride Function ReadState() As CBAPacket

        ''' <summary>
        ''' Set the current.
        ''' </summary>
        ''' <param name="Load">Load.</param>
        Public MustOverride Sub SetCurrent(ByVal Load As UInt32)

        ''' <summary>
        ''' Turn an output on.
        ''' </summary>
        ''' <param name="SwitchSelect">byte value determining the proper output.</param>
        Public MustOverride Sub SwitchOn(ByVal SwitchSelect As Byte)

        ''' <summary>
        ''' Turn an output off.
        ''' </summary>
        ''' <param name="SwitchSelect">byte value determining the proper output.</param>
        Public MustOverride Sub SwitchOff(ByVal SwitchSelect As Byte)
    End Class
#End Region
End Class
