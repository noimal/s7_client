# Siemens S7 Client Library For .Net

## Overview
s7_client is a simple and fast .Net library which communicates with Siemens S7 devices using Siemens S7 Protocol.

## Installation
Download the latest release from [here](https://github.com/ermanimer/s7_client/releases "Releases") and add reference to your project or run the following command in Nuget Package Manager Console.

    PM> Install-Package s7_client

## Constructor
* #### S7Client(string ipAddress, ushort port, ushort rack, ushort slot)
    * ##### Parameters:
        * **ipAddress**: Ip address of the remote device.
        * **port**: Port of the remote device. The default port is 102 for S7 Protocol.
        * **rack**: Rack number of the remote device.
        * **slot** : Slot number of the remote device.
        
        Rack and slot numbers for supported CPUs:
        
        | CPU | Rack | Slot | Description |
        | :---| :--: | :--: | :---------- |
        | S7 300 | 0 | 2 | Always |
        | S7 400 | - | - | Follow the hardware configuration. |
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
                //connect to the remote device
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
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | Hostname or port is not valid. |
        | 2 | Tcp connection failed. |
        | 3 | Network stream failed. |
        | 4 | Iso connection failed. |
        | 5 | Pdu negotiation failed. |
        | 10 | S7Client is busy. |

* #### Close()
    Disposes the tcp client instance and requests that the underlying tcp connection be closed. Returns a bool indicating if the task is successfully completed.
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
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 10 | ModbusTcpClient is busy. |
    
