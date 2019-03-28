# Siemens S7 Client Library For .Net

![GitHub Downloads](https://img.shields.io/github/downloads/ermanimer/s7_client/total.svg?color=%4caf50&label=GitHub%20Downloads&style=flat-square)
![Nuget Total Downloads](https://img.shields.io/nuget/dt/s7_client.svg?color=%4caf50&label=Nuget%20Total%20Downloads&style=flat-square)

## Overview
s7_client is a simple and fast .Net library which communicates with Siemens S7 devices using Siemens S7 Protocol.

s7_client reads and writes data to the specified data block. The data block's protection settings must allow full access and the optimized acces option must be disabled.

s7client is the synchronous version of [async_s7_client](https://github.com/ermanimer/async_s7_client) and it is designed to be used with Timers and BackgroundWorkers.

## Supported Data Types
| Data Type | Description |
| :-------- | :---------- |
| ushort | Unsigned 16-bit integer |
| uint | Unsigned 32-bit integer |
| float | 32-bit floating point number |

## Installation
Download the latest release from [here](https://github.com/ermanimer/s7_client/releases "Releases") and add reference to your project or run the following command in Nuget Package Manager Console.

    PM> Install-Package s7_client

## Constructor
* #### S7Client(string ipAddress, ushort port, ushort rack, ushort slot)
    * ##### Parameters:
        * **ipAddress**: Ip address of the remote device.
        * **port**: Port of the remote device. The default port is 102 for Siemens S7 Protocol.
        * **rack**: Rack number of the remote device.
        * **slot** : Slot number of the remote device.
        
        Rack and slot numbers for supported Cpus:
        
        | Cpu | Rack | Slot | Description |
        | :---| :--: | :--: | :---------- |
        | S7 300 | 0 | 2 | Always |
        | S7 400 | - | - | Follow the hardware configuration |
        | S7 1200 | 0 | 0 | Or 0, 1 |
        | S7 1500 | 0 | 0 | Or 0, 1 |
        
    * ##### Example:
        ```c#
        S7Client s7Client = new S7Client("192.168.0.1", 102, 0, 0);
        ```
 
 ## Properties
* #### Busy:
    Gets a bool value indicating whether the S7Client is running a task.
    
* #### Connected:
    Gets a bool value indicating whether the S7Client is connected to a remote device.

* #### PduLength:
    Gets a ushort value indicating the process data unit length of the remote device.
    
## Methods
* #### Connect()
    Connects to the remote device. Returns **Connected** property.
    * ##### Example:
        ```c#
        private void buttonConnect_Click(object sender, EventArgs e) {
            try {
                //connect to remote device
                bool result = s7Client.Connect();

                //print result
                Debug.WriteLine(result.ToString());
            }
            catch(S7ClientException s7ClientException) {
                Debug.WriteLine(s7ClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only S7ClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | Hostname or port is not valid. |
        | 2 | Tcp connection failed. |
        | 3 | Network stream failed. |
        | 4 | Iso connection failed. |
        | 5 | Pdu negotiation failed. |
        | 9 | S7Client is busy. |

* #### Close()
    Disposes the tcp client instance and requests that the underlying tcp connection be closed. Returns a bool indicating whether the task is successfully completed.
    * ##### Example:
        ```c#
        private void buttonClose_Click(object sender, EventArgs e) {
            try {
                //close s7 client
                bool result = modbusTcpClient.Close();

                //print result
                Debug.WriteLine(result.ToString());
            }
            catch(S7ClientException s7ClientException) {
                Debug.WriteLine(s7ClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only S7ClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 9 | S7Client is busy. |

* #### Read(ushort dataBlockNumber, uint startingAddress, ushort byteCount)
    Reads data from the remote device's specified data block. Returns a byte array indicating each data starting from the first data.
    * ##### Parameters:
        * **dataBlockNumber**: Number of the data block.
        * **startingAddress**: Starting address of the data.
        * **byteCount**: Byte count of the data.
    * ##### Example:
        ```c#
        private void buttonRead_Click(object sender, EventArgs e) {
            try {
                //read ten bytes from data block 1, starting from address 0. Ten bytes include 
                //one ushort (16-bit unsigned integer), one uint (32-bit unsigned integer) and 
                //one float (32-bit floating point number)
                byte[] readBytes = s7Client.Read(1, 0, 10);

                //print ushort
                ushort _ushort = s7Client.GetUShort(readBytes, 0);
                Debug.WriteLine($"ushort : {_ushort.ToString()}");

                //print uint
                uint _uint = s7Client.GetUInt(readBytes, 2);
                Debug.WriteLine($"uint : {_uint.ToString()}");

                //print float
                float _float = s7Client.GetFloat(readBytes, 6);
                Debug.WriteLine($"float : {_float.ToString()}");
            }
            catch(S7ClientException s7ClientException) {
                Debug.WriteLine(s7ClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only S7ClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 6 | Byte count is out of range. |
        | 7 | Reading failed. |
        | 9 | S7Client is busy. |
        | 10 | S7Client is not connected to a remote device. |
    
* #### Write(ushort dataBlockNumber, uint startingAddress, byte[] bytesToWrite)
    Writes data to the remote device's specified data block. Returns a bool indicating whether the task is successfully completed.
    * ##### Parameters:
        * **dataBlockNumber**: Number of the data block.
        * **startingAddress**: Starting address of the data.
        * **bytesToWrite**: Bytes of data.
    * ##### Example:
        ```c#
        private void buttonWrite_Click(object sender, EventArgs e) {
            try {
                //write ten bytes to data block 1, starting from address 0. Ten bytes include 
                //one ushort (16-bit unsigned integer), one uint (32-bit unsigned integer) and 
                //one float (32-bit floating point number)
                byte[] bytesToWrite = new byte[10];

                //bytes of ushort
                ushort _ushort = 1000;
                s7Client.SetUshort(bytesToWrite, 0, _ushort);

                //bytes of uint
                uint _uint = 100000;
                s7Client.SetUInt(bytesToWrite, 2, _uint);

                //bytes of float
                float _float = 3.14f;
                s7Client.SetFloat(bytesToWrite, 6, _float);

                bool result = s7Client.Write(1, 0, bytesToWrite);

                //print result
                Debug.WriteLine(result.ToString());
            }
            catch(S7ClientException s7ClientException) {
                Debug.WriteLine(s7ClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only S7ClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 6 | Byte count is out of range. |
        | 8 | Writing failed. |
        | 9 | S7Client is busy. |
        | 10 | S7Client is not connected to a remote device. |
  
## Exceptions
All S7ClientException codes and messages.

| Exception Code | Exception Message |
|:--------------:| :---------------- |
| 1 | Hostname or port is not valid. |
| 2 | Tcp connection failed.  |
| 3 | Network stream failed. |
| 4 | Iso connection failed. |
| 5 | Pdu negotiation failed. |
| 6 | Byte count is out of range. |
| 7 | Reading failed. |
| 8 | Writing failed. |
| 9 | S7Client is busy. |
| 10 | S7Client is not connected to a remote device. |

## Tests
* #### Test 01:
    | Cpu | Model | CPU Version | Test Date |
    | :-- | :---: | :---------: | :-------: |
    | S7 1200 | 1214C | V4.2 | 27.03.2019 |
    
    | Function Name | Result |
    | :------------ | :----- |
    | Read | Reads 222 bytes below 10 milliseconds. |
    | Write | Writes 204 bytes below 10 milliseconds. |
    
 ## References
 * **Snap7** : This code implements a small amount of S7 protocol which is completely covered in Snap7. Snap7 is a great open soruce project created by Dave Nardella. You can visit [Snap7 Homepage](http://snap7.sourceforge.net/) for more information.
