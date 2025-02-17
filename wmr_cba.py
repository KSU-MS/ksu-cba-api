"""
    wmr_cba.py

    Python library for talking to West Mountain Radio battery analyzer tools,
    such as the CBA IV.
    
    Sorry there is not good documentation.  See the __test_cba4() function 
    for a good example of usage of this code.
    
    As of right now this code only supports Windows systems using the official
    WMR provided drivers (http://www.westmountainradio.com/kb_view_topic.php?id=OT41).
    In the future I would also like to add Linux support using libusb, but I have no ETA.
    
    Using this library with the official WMR drivers requires having the mpusbapi.dll
    file in the same directory as this script.  This file can be found in the CBA4 
    SDK (http://www.westmountainradio.com/zip/cba4_api_sdk.zip)
"""
# Copyright (c) 2018 - Darren Rook
# Rights to use this code is made available using the MIT license.

import threading
import time
import ctypes
import sys
from sys import exit

class cba4:
    """
    Class for talking to CBA IV
    """
    def __init__(self, serial_number=None, interface_number=0):
        if serial_number:
            devices = self.scan()
            for device in devices:
                if device == serial_number:
                    break
                interface_number += 1
                #end -for device in devices-
            #end -if serial_number-

        self.__handle_read = -1
        self.__handle_write = -1

        self.__usb = mpusbapi()
        self.__handle_read = self.__usb.MPUSBOpen(interface_number, "vid_2405&pid_0005", "\\MCHP_EP1", 1)
        self.__handle_write = self.__usb.MPUSBOpen(interface_number, "vid_2405&pid_0005", "\\MCHP_EP1", 0)
        
        if self.is_valid():
            rx = bytearray(65)
            bw = bytearray(1)
            bw[0] = 0x43
            self.__usb.MPUSBWrite(self.__handle_write, bw, 1000)
            ok = self.__wait_for(0x63, rx)
            if ok:
                self.__config_bytes = list(rx)

        self.__thread = None
        #end __init__

    def close(self):
        self.do_stop()
        if (self.__handle_read != -1):
            ret = self.__usb.MPUSBClose(self.__handle_read)
            self.__handle_read = -1
        if (self.__handle_write != -1):
            ret = self.__usb.MPUSBClose(self.__handle_write)
            self.__handle_write = -1
        #end close()

    def __del__(self):
        self.close()
        #end __del__

    class __worker_thread(threading.Thread):
        """
        Worker thread for sending a message every 333ms.  This is because the
        Send Status (0x73) command needs to be periodically sent as a watch
        dog timer, else the CBA will think the computer or software has crashed
        and will stop drawing a load from the battery.
        """
        def __init__(self, cba):
            """
            Create worker thread.

            Parameters: \n
            cba - The parent CBA object.
            transmit_bytes - the message, in bytes, to send every 333ms.  A 
            copy of this array will be copied and thus the reference doesn't
            need to be kept.
            """
            threading.Thread.__init__(self)
            self.__cba = cba
            self.__tx_bytes = bytearray(16)
            self.__tx_bytes[0] = 0x53
            self.__lock = threading.Lock()
            self.__rx_bytes_unsynced = bytearray(65)
            self.__rx_bytes_synced = bytearray(65)
            self.__run = True
            self.__temp_halted = False
            #end __init__()

        def run(self):
            while self.__run:
                time.sleep(0.75)
                self.__cba.get_status_response(self.__tx_bytes, self.__rx_bytes_unsynced)
                self.__lock.acquire()
                self.__rx_bytes_synced[:] = self.__rx_bytes_unsynced
                self.__lock.release()
            #end run()

        def stop(self):
            """
            Tell the thread to stop working.  You will still need to join()
            to wait until thread is done.
            """
            self.__run = False
            #end stop()

        def get_status_response(self, status_bytes):
            """
            Returns the latest status response message heard, as a bytearray
            """
            self.__lock.acquire()
            i = 0
            while (i < len(status_bytes)) and (i < len(self.__rx_bytes_synced)):
                status_bytes[i] = self.__rx_bytes_synced[i]
                i += 1
            status_bytes = list(self.__rx_bytes_synced)
            self.__lock.release()
            #end get_status_response()
        #end class __worker_thread

    def is_valid(self):
        """
        Checks to see if connection is valid.

        Returns:    \n
        true if connected OK, false if error.
        """
        return self.__usb and (self.__handle_write != -1) and (self.__handle_read != -1)
        #end valid()

    @staticmethod
    def scan():
        """
        Returns an array of found devices, as their serial number (integer) -
        or None if there are no devices connected.
        """
        num = mpusbapi().MPUSBGetDeviceCount("vid_2405&pid_0005")
        if not num:
            return None
        devices = []
        i = 0
        while i < num:
            cba = cba4(interface_number=i)
            i += 1
            sn = cba.get_serial_number()
            if sn:
                devices.append(sn)
            cba.close()
            #end loop
        if len(devices) == 0:
            return None
        return devices
        #end scan()

    def __wait_for(self, cmd_byte, rx_bytes, timeout_seconds=1.0):
        """
        Read USB until we get a message that starts with 'cmd_byte'.
        Result is saved to 'rx_bytes', which is an array of bytes.
        'rx_bytes' may have been modified even if the proper expected message
        was not received!

        Returns:    \n
        True if OK and self.__br was updated, False if wasn't heard within
        'timeout_seconds'
        """
        if not self.is_valid():
            return False
        t = time.time()
        remain = timeout_seconds
        while 1:
            num_read = self.__usb.MPUSBRead(self.__handle_read, rx_bytes, int(remain*1000))
            if (num_read > 0) and (rx_bytes[0]==cmd_byte):
                return True
            remain = timeout_seconds - (time.time()-t)
            if remain <= 0:
                break
            time.sleep(0.001)
            #end 1 loop
        return False
        #end __wait_for()

    def get_serial_number(self):
        """
        Returns the serial number of the device, which is written on a sticker
        on the bottom of the device.

        Returns:    \n
        The serial number, an integer.  Returns 0 if error.
        """
        if not self.__config_bytes:
            return 0
        return self.__config_bytes[4] + (self.__config_bytes[5] * 0x100) + (self.__config_bytes[6] * 0x10000) + (self.__config_bytes[7] * 0x1000000)
        #end get_serial_number

    def do_start_current(self, amps, vstop=0):
        """
        Tells the CBA to start drawing 'amps' load, in float, from it's source.  
        If voltage of supply goes below 'vstop', then the unit will stop drawing
        current (to prevent over discharing a battery).  'vstop' is a float,
        or send 0 to not use vstop.
        Use do_stop() to stop drawing current.
        """
        self.do_stop()

        amps *= 1000.0 * 1000.0
        amps = int(amps)
        tx = bytearray(16)
        tx[0] = 0x53    #CMD
        tx[1] = 0x03    #FLAGS
        if vstop:
            tx[1] |= 0x40
        tx[2] = 0
        tx[3] = (amps >> 0) & 0xff  #LOAD
        tx[4] = (amps >> 8) & 0xff
        tx[5] = (amps >> 16) & 0xff
        tx[6] = (amps >> 24) & 0xff
        tx[7] = 0   #FAN
        tx[8] = 0   #LED1
        tx[9] = 0   #LED2
        tx[10] = 0  #IOTRIS
        tx[11] = 0  #IOPORT
        vstop *= 1000.0 * 1000.0
        vstop = int(vstop)
        tx[12] = (vstop >> 0) & 0xff  #VSTOP
        tx[13] = (vstop >> 8) & 0xff
        tx[14] = (vstop >> 16) & 0xff
        tx[15] = (vstop >> 24) & 0xff

        self.get_status_response(tx)

        self.__thread = cba4.__worker_thread(self)
        self.__thread.start()
        #end do_start_draw()

    def do_stop(self):
        """
        End a running test / current draw
        """
        if (self.__thread and self.__thread.isAlive):
            self.__thread.stop()
            self.__thread.join()

        if (self.is_valid()):
            tx = bytearray(16)
            tx[0] = 0x53
            tx[1] = 1
            self.get_status_response(tx)
        #end do_stop()

    def get_status_response(self, force_xmit=None, force_rcv=None):
        """
        Read the status message from the CBA4.

        Parameters: \n
        force_xmit - If provided (bytearray), this set status (0x53) message
        is sent.  If provided, it will also ignore the previous status response
        heard by the running thread and send this message to get a new status
        response.
        force_rcv - If provided (bytearray), received status messages are saved
        into this array.

        Returns:    \n
        A bytearray of the status response, None if an error.
        """
        if not force_rcv:
            force_rcv = bytearray(65)

        if not force_xmit and self.__thread and self.__thread.isAlive():
            self.__thread.get_status_response(force_rcv)
            return force_rcv

        if not force_xmit:
            force_xmit = bytearray(16)
            force_xmit[0] = 0x53

        self.__usb.MPUSBWrite(self.__handle_write, force_xmit, 1000)

        ok = self.__wait_for(0x73, force_rcv)

        if ok:
            return force_rcv

        print("NO RESPONSE")
        return None
        #end get_status_response()

    def get_voltage(self):
        """
        Returns the measured voltage (float)
        """
        status = self.get_status_response()
        volts = status[20] + (status[21] * 0x100) + (status[22] * 0x10000) + (status[23] * 0x1000000)
        volts = float(volts)
        volts /= (1000.0 * 1000.0)
        return volts
        #end get_voltage

    def get_set_current(self):
        """
        Returns the test current (amps, as a float), or 0.0 if a test is not
        running.
        """
        status = self.get_status_response()
        flags = status[1]
        if (flags & 0x2) == 0:
            return 0.0  #test isn't running
        current = status[3] + (status[4] * 0x100) + (status[5] * 0x10000) + (status[6] * 0x1000000)
        current = float(current)
        current /= (1000.0 * 1000.0)
        return current
        #end get_set_current()

    def get_measured_current(self):
        """
        Returns the measured current (amps, as a float).  The feedback of the
        CBA is 10bits for the entire 40 Amps range, so this should not be used
        as an accurate reading.  It can be used to detect gross errors, such
        as the fuse being blown or the device power limiting the test.
        """
        status = self.get_status_response()
        current = status[16] + (status[17] * 0x100) + (status[18] * 0x10000) + (status[19] * 0x1000000)
        current = float(current)
        current /= (1000.0 * 1000.0)
        return current
        #end get_measured_current

    def is_running(self):
        """
        Returns True if a test is currently running and the CBA is drawing
        current.
        """
        status = self.get_status_response()
        return ((status[1] & 2) == 2)
        #end is_running()

    def is_power_limited(self):
        """
        Returns True if test is not running to user specified parameters because
        it has exceeded maximum power or current limits of the device.
        """
        status = self.get_status_response()
        return ((status[1] & 0x10) == 0x10)
        #end is_power_limited()

    def is_high_temp(self):
        """
        Returns True if test was aborted because the temperature of the CBA
        got too high and exceeded safety limits.
        """
        status = self.get_status_response()
        return ((status[1] & 0x20) == 0x20)
        #end is_power_limited()
    #end class cba4
    
class mpusbapi:
    """
    ctypes wrapper to mpusbapi.dll
    
    Several comments/documentation from Microchip's mpusabapi documentation
    has been copied/pasted into here.
    https://stackoverflow.com/questions/37888565/python-3-5-ctypes-typeerror-bytes-or-integer-address-expected-instead-of-str
    https://stackoverflow.com/questions/252417/how-can-i-use-a-dll-file-from-python
    """
    def __init__(self):
        dll = None
        self.__dll = None
        if sys.platform == "win32":
            #dll = ctypes.CDLL("mpusbapi.dll")
            dll = ctypes.cdll.LoadLibrary("mpusbapi.dll")
        else:
            return

        self.__dll = dll

        #DWORD (*MPUSBGetDLLVersion)(void);
        self.__dll._MPUSBGetDLLVersion.restype = ctypes.c_long

        #DWORD (*MPUSBGetDeviceCount)(PCHAR pVID_PID);
        self.__dll._MPUSBGetDeviceCount.argtypes = [ctypes.c_char_p]
        self.__dll._MPUSBGetDeviceCount.restype = ctypes.c_long
        
        #HANDLE (*MPUSBOpen)(DWORD instance, PCHAR pVID_PID, PCHAR pEP, DWORD dwDir, DWORD dwReserved);
        self.__dll._MPUSBOpen.argtypes = [ctypes.c_long, ctypes.c_char_p, ctypes.c_char_p, ctypes.c_long, ctypes.c_long]
        self.__dll._MPUSBOpen.restype = ctypes.c_int

        #pData and pLength are output from the function
        #DWORD (*MPUSBRead)(HANDLE handle, PVOID pData, DWORD dwLen, PDWORD pLength, DWORD dwMilliseconds);
        self.__dll._MPUSBRead.argtypes = [ctypes.c_int, ctypes.c_char_p, ctypes.c_long, ctypes.POINTER(ctypes.c_long), ctypes.c_long]
        self.__dll._MPUSBRead.restype = ctypes.c_long

        #pLength are output from the function
        #DWORD (*MPUSBWrite)(HANDLE handle, PVOID pData, DWORD dwLen, PDWORD pLength, DWORD dwMilliseconds);
        self.__dll._MPUSBWrite.argtypes = [ctypes.c_int, ctypes.c_char_p, ctypes.c_long, ctypes.POINTER(ctypes.c_long), ctypes.c_long]
        self.__dll._MPUSBWrite.restype = ctypes.c_long

        #BOOL (*MPUSBClose)(HANDLE handle);
        self.__dll._MPUSBClose.argtypes = [ctypes.c_int]
        self.__dll._MPUSBRead.restype = ctypes.c_bool
        # end __init__()
        
    def MPUSBGetDLLVersion(self):
        """
        Get the version number of the DLL.

        Returns:    \n
            32-bit revision level MMmmddii
            MM - Major release
            mm - Minor release
            dd - dot release or minor fix
            ii - test release revisions
        """
        if not self.__dll:
            return None
        return self.__dll._MPUSBGetDLLVersion()
        #end MPUSBGetDLLVersion()

    def MPUSBGetDeviceCount(self, vid_pid_string):
        """
        Get number of devices connected.

        Parameters: \n

        vid_pid_string - A string containing the PID&VID value of the target device.
        The format is "vid_xxxx&pid_yyyy". Where xxxx is the VID value
        in hex and yyyy is the PID value in hex.
        Example: If a device has the VID value of 0x04d8 and PID value
        of 0x000b, then the input string should be:
        "vid_04d8&pid_000b"

        Returns:    \n

        Number of devices attached and not connected by another instance
        of the driver.
        """
        if not self.__dll:
            return None
        return self.__dll._MPUSBGetDeviceCount(vid_pid_string.encode('utf-8'))
        #end MPUSBGetDeviceCount

    def MPUSBOpen(self, instance, vid_pid_str, ep_str, dirr):
        """
        Returns the handle to the endpoint pipe with matching VID & PID

        All pipes are opened with the FILE_FLAG_OVERLAPPED attribute.
        This allows MPUSBRead,MPUSBWrite, and MPUSBReadInt to have a time-out value.

        Note: Time-out value has no meaning for Isochronous pipes.

        instance - An instance number of the device to open.
        Typical usage is to call MPUSBGetDeviceCount first to find out
        how many instances there are.
        It is important to understand that the driver is shared among
        different devices. The number of devices returned by
        MPUSBGetDeviceCount could be equal to or less than the number
        of all the devices that are currently connected & using the
        generic driver.

        Example:
        if there are 3 device with the following PID&VID connected:
        Device Instance 0, VID 0x04d8, PID 0x0001
        Device Instance 1, VID 0x04d8, PID 0x0002
        Device Instance 2, VID 0x04d8, PID 0x0001

        If the device of interest has VID = 0x04d8 and PID = 0x0002
        Then MPUSBGetDeviceCount will only return '1'.
        The calling function should have a mechanism that attempts
        to call MPUSBOpen up to the absolute maximum of MAX_NUM_MPUSB_DEV
        (MAX_NUM_MPUSB_DEV is defined in _mpusbapi.h).
        It should also keep track of the number of successful calls
        to MPUSBOpen(). Once the number of successes equals the
        number returned by MPUSBGetDeviceCount, the attempts should
        be aborted because there will no more devices with
        a matching vid&pid left.

        vid_pid_str - A string containing the PID&VID value of the target device.
        The format is "vid_xxxx&pid_yyyy". Where xxxx is the VID value
        in hex and yyyy is the PID value in hex.
        Example: If a device has the VID value of 0x04d8 and PID value
        of 0x000b, then the input string should be:
        "vid_04d8&pid_000b"

        ep_str - A string of the endpoint number on the target endpoint to open.
        The format is "\\MCHP_EPz". Where z is the endpoint number in
        decimal.
        Example: "\\MCHP_EP1"

        This arguement can be NULL. A NULL value should be used to
        create a handles for non-specific endpoint functions.
        MPUSBRead, MPUSBWrite, MPUSBReadInt are endpoint specific
        functions.
        All others are not.
        Non-specific endpoint functions will become available in the
        next release of the DLL.

        Note: To use MPUSBReadInt(), the format of pEP has to be
        "\\MCHP_EPz_ASYNC". This option is only available for
        an IN interrupt endpoint. A data pipe opened with the
        "_ASYNC" keyword would buffer the data at the interval
        specified in the endpoint descriptor upto the maximum of
        100 data sets. Any data received after the driver buffer
        is full will be ignored.
        The user application should call MPUSBReadInt() often
        enough so that the maximum limit of 100 is never reached.

        dirr - Specifies the direction of the endpoint.
        Use 1 (MP_READ) for MPUSBRead, MPSUBReadInt
        Use 0 (MP_WRITE) for MPUSBWrite

        Returns:    \n
        -1 if there was an error opening, else the handle to use for
        future functions.
        """
        if not self.__dll:
            return -1
        ret = self.__dll._MPUSBOpen(instance, vid_pid_str.encode('utf-8'), ep_str.encode('utf-8'), dirr, 0)
        return ret
        #end MPUSBOpen()

    def MPUSBRead(self, handle, data, timeout_ms):
        """
            MPUSBRead :

            Parameters: \n

            handle - Identifies the endpoint pipe to be read. The pipe handle must
            have been created with MP_READ access attribute.

            data - A bytesarray where incoming data will be saved.

            timeout_ms
            - Specifies the time-out interval, in milliseconds. The function
            returns if the interval elapses, even if the operation is
            incomplete. If timeout_ms is zero, the function tests the
            data pipe and returns immediately. If timeout_ms is INFINITE,
            the function's time-out interval never elapses.

            Returns:    \n
            Negative number if a problem, else the number of bytes read
            and saved to data.
        """
        plen = ctypes.c_long()
        data_c_char = ctypes.c_char * len(data)
        ret = self.__dll._MPUSBRead(handle, data_c_char.from_buffer(data), len(data), ctypes.byref(plen), timeout_ms)
        if ret <= 0:
            return -1
        return plen.value
        #end MPUSBRead()

    def MPUSBWrite(self, handle, data, timeout_ms):
        """
        MPUSBWrite

        Parameters: \n

        handle - Identifies the endpoint pipe to be written to. The pipe handle
        must have been created with MP_WRITE access attribute.

        data - Points to the buffer containing the data to be written to the pipe.
            Should be a bytesarray

        dwMilliseconds
        - Specifies the time-out interval, in milliseconds. The function
        returns if the interval elapses, even if the operation is
        incomplete. If dwMilliseconds is zero, the function tests the
        data pipe and returns immediately. If dwMilliseconds is INFINITE,
        the function's time-out interval never elapses.

        Returns:    \n
        Negative number if a problem, else the number of bytes written to the
        endpoint.
        """
        plen = ctypes.c_long()
        data_c_char = ctypes.c_char * len(data)
        ret = self.__dll._MPUSBWrite(handle, data_c_char.from_buffer(data), len(data), ctypes.byref(plen), timeout_ms)
        if ret <= 0:
            return -1
        return plen.value
        #end MPUSBWrite

    def MPUSBClose(self, handle):
        """
        Close a handle.
        """
        if handle != -1:
            return self.__dll._MPUSBClose(handle)
        #end MPUSBClose
    #end mpusbapi class
    
def __test_mpusbapi():
    vid_pid_str = "vid_2405&pid_0005"
    ep_str = "\\MCHP_EP1"

    mp = mpusbapi()

    ret = mp.MPUSBGetDLLVersion()
    print("DLL version is " + str(ret) + ".")

    ret = mp.MPUSBGetDeviceCount(vid_pid_str)
    print("Found " + str(ret) + " number of devices.")

    if ret == 0:
        exit()

    handle_read = mp.MPUSBOpen(0, vid_pid_str, ep_str, 1)
    if handle_read == -1:
        print("Error opening read endpoint!")
        exit()

    handle_write = mp.MPUSBOpen(0, vid_pid_str, ep_str, 0)
    if handle_write == -1:
        print("Error opening write endpoint!")
        exit()

    br = bytearray(65)
    ret = mp.MPUSBRead(handle_read, br, 1000)
    print("Read " + str(ret) + " bytes.  br[0]="+str(br[0]))

    bw = bytearray(1)
    bw[0] = 0x43
    ret = mp.MPUSBWrite(handle_write, bw, 1000)
    print("Wrote " + str(ret) + " bytes.  bw[0]="+str(bw[0]))

    ret = mp.MPUSBRead(handle_read, br, 1000)
    print("Read " + str(ret) + " bytes.  br[0]="+str(br[0]))
    ret = mp.MPUSBRead(handle_read, br, 1000)
    print("Read " + str(ret) + " bytes.  br[0]="+str(br[0]))
    ret = mp.MPUSBRead(handle_read, br, 1000)
    print("Read " + str(ret) + " bytes.  br[0]="+str(br[0]))

    ret = mp.MPUSBClose(handle_write)

    ret = mp.MPUSBClose(handle_read)

    mp = None

    print("Done")
    #end __test_mpusbapi()

def __test_cba4():
    def show_status():
        disp = "Volts=" + str(cba.get_voltage()) + "V"
        disp += " Load=" + str(cba.get_set_current()) + "A"
        disp += " Feedback=" + str(cba.get_measured_current()) + "A"
        disp += " Running=" + str(cba.is_running())
        disp += " PLim=" + str(cba.is_power_limited())
        print(disp)
        #end show_status()

    devices = cba4.scan()
    print("Found "+str(len(devices))+" devices.")

    cba = cba4()

    if not cba.is_valid():
        print("ERROR!  Couldn't open a device!")
        exit()
    
    print("Opened CBA4, serial #" + str(cba.get_serial_number()))

    show_status()

    load = 0.2
    print("Starting load = " + str(load))
    cba.do_start_current(load)

    reads = 10
    while reads:
        time.sleep(1)
        show_status()
        reads -= 1
    
    cba.do_stop()
    print("Stopped test")

    reads = 5
    while reads:
        time.sleep(1)
        show_status()
        reads -= 1

    print("Done")
    #end __test_cba4

if __name__ == "__main__":
    __test_cba4()