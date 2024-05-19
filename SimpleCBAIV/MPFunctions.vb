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

Imports System
Imports System.Runtime.InteropServices

Public Class MPFunctions
#Region "Constants"
    Public Const MPUSB_FAIL As Integer = 0

    Public Const MPUSB_SUCCESS As Integer = 1

    Public Const MP_WRITE As Integer = 0

    Public Const MP_READ As Integer = 1

    ' MAX_NUM_MPUSB_DEV is an abstract limitation.
    ' It is very unlikely that a computer system will have more
    ' then 127 USB devices attached to it. (single or multiple USB hosts)
    Public Const MAX_NUM_MPUSB_DEV As Integer = 127
#End Region
#Region "mpusbapi.dll notes"
#End Region

#Region "Member Variables"
    ''' <summary>
    ''' Outgoing usb handle
    ''' </summary>
    Friend EP1OUTHandle As IntPtr

    ''' <summary>
    ''' Incoming USB handle
    ''' </summary>
    Friend EP1INHandle As IntPtr

    ''' <summary>
    ''' True -> Device attached, False -> Device not attached
    ''' </summary>
    Friend attached As Boolean = False
#End Region

#Region "DLL Imports"
    'regarding adding to this file using the mpusbapi.cpp as a reference:
    ''' <summary>
    ''' Opens the identified device
    ''' </summary>
    ''' <param name="instance">an instance number of the device to open. 
    ''' Number of instances is indicated by the getDeviceCount call</param>
    ''' 
    ''' <param name="pVID_PID">PID and VID to indicate type of target device</param>
    ''' <param name="pEP">String of the endpoint number on the target endpoint to open</param>
    ''' <param name="dwDir">Specifies the direction of the endpoint</param>
    ''' <param name="dwReserved">Reserved for future use</param>
    ''' <returns>handle for the opened device</returns>
    Public Declare Function _MPUSBOpen Lib "mpusbapi.dll" (ByVal instance As Int32, ByVal pVID_PID As String, ByVal pEP As String, ByVal dwDir As Int32, ByVal dwReserved As Int32) As IntPtr

    'Input
    ''' <summary>
    ''' Get the count of devices with the given pVID_PID
    ''' </summary>
    ''' <param name="pVID_PID">PID and VID to indicate type of target device</param>
    ''' <returns>count of devices with that pVID_PID. This is the number of instances</returns>
    Public Declare Function _MPUSBGetDeviceCount Lib "mpusbapi.dll" (ByVal pVID_PID As String) As Int32

    ''' <summary>
    ''' Close the connection with the device with the given handle
    ''' </summary>
    ''' <param name="handle">Handle identifier for the device to be closed</param>
    ''' <returns>True => Successful</returns>
    Public Declare Function _MPUSBClose Lib "mpusbapi.dll" (ByVal handle As IntPtr) As Boolean

    ''' <summary>
    ''' Read up to a given number of bytes from the buffer
    ''' </summary>
    ''' <param name="handle">Handle indicating the device</param>
    ''' <param name="pData">Data pointer (byte[]) to store the data as it is copied from the buffer</param>
    ''' <param name="dwLen">Maximum number of bytes to read</param>
    ''' <param name="pLength">response value for bytes read</param>
    ''' <param name="dwMilliseconds">Timeout value, in milliseconds</param>
    ''' <returns>data read -> pData
    ''' bytes read(count) -> pLength
    ''' pass => 1, fail => 0</returns>
    Public Declare Function _MPUSBRead Lib "mpusbapi.dll" (ByVal handle As IntPtr, ByVal pData() As Byte, ByVal dwLen As Int32, ByVal pLength() As UInt32, ByVal dwMilliseconds As Integer) As Int32

    ''' <summary>
    ''' Write a given packet to the device
    ''' </summary>
    ''' <param name="handle">Handle indicating the device</param>
    ''' <param name="pData">Data pointer (byte[]) to read the data from as it is copied to the buffer</param>
    ''' <param name="dwLen">number of bytes to read</param>
    ''' <param name="pLength">points to the number of bytes written by this function call (byte[])</param>
    ''' <param name="dwMilliseconds">time-out interval, in milliseconds</param>
    ''' <returns>pass => 1, fail => 0</returns>
    Public Declare Function _MPUSBWrite Lib "mpusbapi.dll" (ByVal handle As IntPtr, ByVal pData() As Byte, ByVal dwLen As Int32, ByVal pLength() As UInt32, ByVal dwMilliseconds As Integer) As Int32

    'not sure if this is necessary yet.
    Public Declare Function _MPUSBGetDeviceLink Lib "mpusbapi.dll" (ByVal instance As Int32, ByVal pVID_PID As String, ByVal pPath As String, ByVal dwLen As Int32, ByVal pLength() As Byte) As Int32

    ''' <summary>
    ''' Returns the Device Descriptor Data
    ''' </summary>
    ''' <param name="handle"> Identifies the endpoint pipe to be read. The pipe handle must
    ''' have been created with MP_READ access attribute. </param>
    ''' <param name="pDevDsc">pointer (byte array) to where the resulting descriptor should be copied.</param>
    ''' <param name="dwLen"></param>
    ''' <param name="pLength"></param>
    ''' <returns>pass => 1, fail => 0</returns>
    Public Declare Function _MPUSBGetDeviceDescriptor Lib "mpusbapi.dll" (ByVal handle As IntPtr, ByVal pDevDsc() As Byte, ByVal dwLen As UInt32, ByVal pLength() As UInteger) As UInt32

    ''' <summary>
    ''' Returns the Configuration Descriptor
    ''' </summary>
    ''' <param name="handle">Identifies the endpoint pipe to be read. The pipe handle must
    ''' have been created with MP_READ access attribute.</param>
    ''' <param name="bIndex">the index of the configuration descriptor desired.  Valid input
    ''' range is 1 - 255.</param>
    ''' <param name="pDevDsc">pointer to where the resulting descriptor should be copied.</param>
    ''' <param name="dwLen">the available data in the pDevDsc buffer</param>
    ''' <param name="pLength">a pointer to a DWORD that will be updated with the amount of data 
    ''' actually written to the pDevDsc buffer.  This number will be less than or equal to dwLen.</param>
    ''' <returns>pass => 1, fail => 0</returns>
    Public Declare Function _MPUSBGetConfigurationDescriptor Lib "mpusbapi.dll" (ByVal handle As IntPtr, ByVal bIndex As Char, ByVal pDevDsc() As Byte, ByVal dwLen As UInt32, ByVal pLength() As UInt32) As UInt32

    ''' <summary>
    ''' Returns the requested string descriptor
    ''' </summary>
    ''' <param name="handle">Identifies the endpoint pipe to be read. The pipe handle must
    ''' have been created with MP_READ access attribute.</param>
    ''' <param name="bIndex">the index of the configuration descriptor desired.  Valid input
    ''' range is 0 - 255.</param>
    ''' <param name="wLangId">the language ID of the string that needs to be read</param>
    ''' <param name="pDevDsc">pointer to where the resulting descriptor should be copied.</param>
    ''' <param name="dwLen">the available data in the pDevDsc buffer</param>
    ''' <param name="pLength">a pointer to a DWORD that will be updated with the amount of data
    ''' actually written to the pDevDsc buffer.  This number will be
    ''' less than or equal to dwLen.</param>
    ''' <returns>pass => 1, fail => 0</returns>
    Public Declare Function _MPUSBGetStringDescriptor Lib "mpusbapi.dll" (ByVal handle As IntPtr, ByVal bIndex As Char, ByVal wLangId As UInt16, ByVal pDevDsc As String, ByVal dwLen As UInt32, ByVal pLength() As UInt32) As UInt32
#End Region
End Class
