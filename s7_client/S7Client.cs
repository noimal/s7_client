using System;
using System.Net.Sockets;

namespace s7_client {
    public class S7Client {
        public string IpAddress { get; private set; }
        public ushort Port { get; private set; }
        public ushort Rack { get; private set; }
        public ushort Slot { get; private set; }
        public bool Busy { get; private set; }
        public bool Connected { get; private set; }
        public ushort PduLength { get; private set; }
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private byte[] response = new byte[2048];
        
        public S7Client(string ipAddress, ushort port, ushort rack, ushort slot) {
            IpAddress = ipAddress;
            Port = port;
            Rack = rack;
            Slot = slot;
        }

        public bool Connect() {
            if(Busy) {
                throw new S7ClientException(10, "S7Client is busy.");
            }
            Busy = true;
            try {
                //tcp connection
                tcpClient = new TcpClient(IpAddress, Port);
                networkStream = tcpClient.GetStream();
                //iso connection
                ushort remoteTSAP = (ushort) (1 * 256 + Rack * 32 + Slot);
                byte[] isoConnectionRequest = new byte[22];
                isoConnectionRequest[0] = 3;
                isoConnectionRequest[1] = 0;
                isoConnectionRequest[2] = 0;
                isoConnectionRequest[3] = 22;
                isoConnectionRequest[4] = 17;
                isoConnectionRequest[5] = 224;
                isoConnectionRequest[6] = 0;
                isoConnectionRequest[7] = 0;
                isoConnectionRequest[8] = 0;
                isoConnectionRequest[9] = 1;
                isoConnectionRequest[10] = 0;
                isoConnectionRequest[11] = 192;
                isoConnectionRequest[12] = 1;
                isoConnectionRequest[13] = 10;
                isoConnectionRequest[14] = 193;
                isoConnectionRequest[15] = 2;
                isoConnectionRequest[16] = 1;
                isoConnectionRequest[17] = 0;
                isoConnectionRequest[18] = 194;
                isoConnectionRequest[19] = 2;
                isoConnectionRequest[20] = (byte) (remoteTSAP >> 8);
                isoConnectionRequest[21] = (byte) (remoteTSAP & 255);
                networkStream.Write(isoConnectionRequest, 0, isoConnectionRequest.Length);
                int receivedByteCount = networkStream.Read(response, 0, response.Length);
                if(receivedByteCount != 22 && response[5] != 208) {
                    Connected = false;
                    throw new S7ClientException(4, "Iso connection failed.");
                }
                //negotiate pdu length
                PduLength = 960;
                byte[] pduNegotiationRequest = new byte[25];
                pduNegotiationRequest[0] = 3;
                pduNegotiationRequest[1] = 0;
                pduNegotiationRequest[2] = 0;
                pduNegotiationRequest[3] = 25;
                pduNegotiationRequest[4] = 2;
                pduNegotiationRequest[5] = 240;
                pduNegotiationRequest[6] = 128;
                pduNegotiationRequest[7] = 50;
                pduNegotiationRequest[8] = 1;
                pduNegotiationRequest[9] = 0;
                pduNegotiationRequest[10] = 0;
                pduNegotiationRequest[11] = 4;
                pduNegotiationRequest[12] = 0;
                pduNegotiationRequest[13] = 0;
                pduNegotiationRequest[14] = 8;
                pduNegotiationRequest[15] = 0;
                pduNegotiationRequest[16] = 0;
                pduNegotiationRequest[17] = 240;
                pduNegotiationRequest[18] = 0;
                pduNegotiationRequest[19] = 0;
                pduNegotiationRequest[20] = 1;
                pduNegotiationRequest[21] = 0;
                pduNegotiationRequest[22] = 1;
                pduNegotiationRequest[23] = (byte) (PduLength >> 8);
                pduNegotiationRequest[24] = (byte) (PduLength & 255);
                networkStream.Write(pduNegotiationRequest, 0, pduNegotiationRequest.Length);
                receivedByteCount = networkStream.Read(response, 0, response.Length);
                if(receivedByteCount != 27 && response[17] != 0 && response[18] != 0) {
                    Connected = false;
                    throw new S7ClientException(5, "Pdu negotiation failed.");
                }
                byte[] bytesOfPduLength = new byte[2];
                bytesOfPduLength[0] = response[26];
                bytesOfPduLength[1] = response[25];
                PduLength = BitConverter.ToUInt16(bytesOfPduLength, 0);
                return Connected = true;
            }
            catch(ArgumentException) {
                Connected = false;
                throw new S7ClientException(1, "Hostname or port is not valid.");
            }
            catch(SocketException) {
                Connected = false;
                throw new S7ClientException(2, "Tcp connection failed.");
            }
            catch(InvalidOperationException) {
                Connected = false;
                throw new S7ClientException(3, "Network stream failed.");
            }
            finally {
                Busy = false;
            }
        }

        public bool Close() {
            if(Busy) {
                throw new S7ClientException(0, "S7Client is busy.");
            }
            if(networkStream != null) {
                networkStream.Close();
            }
            if(tcpClient != null) {
                tcpClient.Close();
            }
            Connected = false;
            return !(Connected = false);
        }

        public byte[] Read(ushort dataBlockNumber, uint startingAddress, ushort byteCount) {
            if(Busy) {
                throw new S7ClientException(10, "S7Client is busy.");
            }
            if(!Connected) {
                throw new S7ClientException(11, "S7Client is not connected to a remote device.");
            }
            if(byteCount < 2 || byteCount > PduLength - 18 || byteCount % 2 != 0) {
                throw new S7ClientException(6, "Byte count is out of range.");
            }
            byte[] readRequest = new byte[31];
            readRequest[0] = 3;
            readRequest[1] = 0;
            readRequest[2] = 0;
            readRequest[3] = 31;
            readRequest[4] = 2;
            readRequest[5] = 240;
            readRequest[6] = 128;
            readRequest[7] = 50;
            readRequest[8] = 1;
            readRequest[9] = 0;
            readRequest[10] = 0;
            readRequest[11] = 5;
            readRequest[12] = 0;
            readRequest[13] = 0;
            readRequest[14] = 14;
            readRequest[15] = 0;
            readRequest[16] = 0;
            readRequest[17] = 4;
            readRequest[18] = 1;
            readRequest[19] = 18;
            readRequest[20] = 10;
            readRequest[21] = 16;
            readRequest[22] = 2;
            readRequest[23] = (byte) (byteCount >> 8);
            readRequest[24] = (byte) (byteCount & 255);
            readRequest[25] = (byte) (dataBlockNumber >> 8);
            readRequest[26] = (byte) (dataBlockNumber & 255);
            readRequest[27] = 132;
            readRequest[28] = (byte) ((startingAddress >> 16) & 255);
            readRequest[29] = (byte) ((startingAddress >> 8) & 255);
            readRequest[30] = (byte) (startingAddress & 255);
            networkStream.Write(readRequest, 0, readRequest.Length);
            int receivedByteCount = networkStream.Read(response, 0, response.Length);
            if(receivedByteCount < 27 || response[21] != 255) {
                throw new S7ClientException(7, "Reading failed.");
            }
            byte[] readBytes = new byte[byteCount];
            for(int byteIndex = 0; byteIndex < byteCount; byteIndex++) {
                readBytes[byteIndex] = response[25 + byteIndex];
            }
            return readBytes;
        }

        public bool Write(ushort dataBlockNumber, uint startingAddress, byte[] bytesToWrite) {
            int byteCount = bytesToWrite.Length;
            if(Busy) {
                throw new S7ClientException(10, "S7Client is busy.");
            }
            if(!Connected) {
                throw new S7ClientException(11, "S7Client is not connected to a remote device.");
            }
            if(byteCount < 2 || byteCount > PduLength - 35 || byteCount % 2 != 0) {
                throw new S7ClientException(8, "Byte count is out of range.");
            }
            int requestSize = 35 + byteCount;
            byte[] writeRequest = new byte[requestSize];
            writeRequest[0] = 3;
            writeRequest[1] = 0;
            writeRequest[2] = (byte) (((ushort) requestSize) >> 8);
            writeRequest[3] = (byte) (((ushort) requestSize) & 255);
            writeRequest[4] = 2;
            writeRequest[5] = 240;
            writeRequest[6] = 128;
            writeRequest[7] = 50;
            writeRequest[8] = 1;
            writeRequest[9] = 0;
            writeRequest[10] = 0;
            writeRequest[11] = 5;
            writeRequest[12] = 0;
            writeRequest[13] = 0;
            writeRequest[14] = 14;
            writeRequest[15] = (byte) (((ushort) (byteCount + 4)) >> 8);
            writeRequest[16] = (byte) (((ushort) (byteCount + 4)) & 255);
            writeRequest[17] = 5;
            writeRequest[18] = 1;
            writeRequest[19] = 18;
            writeRequest[20] = 10;
            writeRequest[21] = 16;
            writeRequest[22] = 2;
            writeRequest[23] = (byte) (((ushort) byteCount) >> 8);
            writeRequest[24] = (byte) (((ushort) byteCount) & 255);
            writeRequest[25] = (byte) (dataBlockNumber >> 8);
            writeRequest[26] = (byte) (dataBlockNumber & 255);
            writeRequest[27] = 132;
            writeRequest[28] = (byte) ((startingAddress >> 16) & 255);
            writeRequest[29] = (byte) ((startingAddress >> 8) & 255);
            writeRequest[30] = (byte) (startingAddress & 255);
            writeRequest[31] = 0;
            writeRequest[32] = 4;
            writeRequest[33] = (byte) (((ushort) (byteCount * 8)) >> 8);
            writeRequest[34] = (byte) (((ushort) (byteCount * 8)) & 255);
            for(int byteIndex = 0; byteIndex < byteCount; byteIndex++) {
                writeRequest[35 + byteIndex] = bytesToWrite[byteIndex];
            }
            networkStream.Write(writeRequest, 0, writeRequest.Length);
            int receivedByteCount = networkStream.Read(response, 0, response.Length);
            if(receivedByteCount != 22 || response[21] != 255) {
                throw new S7ClientException(9, "Writing failed.");
            }
            return true;
        }

        public ushort GetUShort(byte[] readBytes, ushort address) {
            byte[] bytesOfUShort = new byte[2];
            bytesOfUShort[0] = readBytes[address + 1];
            bytesOfUShort[1] = readBytes[address];
            return BitConverter.ToUInt16(bytesOfUShort, 0);
        }

        public void SetUshort(byte[] bytesToWrite, ushort address, ushort value) {
            bytesToWrite[address] = (byte) (value >> 8);
            bytesToWrite[address + 1] = (byte) (value & 255);
        }

        public uint GetUInt(byte[] readBytes, ushort address) {
            byte[] bytesOfUInt = new byte[4];
            bytesOfUInt[0] = readBytes[address + 3];
            bytesOfUInt[1] = readBytes[address + 2];
            bytesOfUInt[2] = readBytes[address + 1];
            bytesOfUInt[3] = readBytes[address];
            return BitConverter.ToUInt32(bytesOfUInt, 0);
        }

        public void SetUInt(byte[] bytesToWrite, ushort address, uint value) {
            bytesToWrite[address] = (byte) ((value >> 24) & 255);
            bytesToWrite[address + 1] = (byte) ((value >> 16) & 255);
            bytesToWrite[address + 2] = (byte) ((value >> 8) & 255);
            bytesToWrite[address + 3] = (byte) (value  & 255);
        }

        public float GetFloat(byte[] readBytes, ushort address) {
            byte[] bytesOfFloat = new byte[4];
            bytesOfFloat[0] = readBytes[address + 3];
            bytesOfFloat[1] = readBytes[address + 2];
            bytesOfFloat[2] = readBytes[address + 1];
            bytesOfFloat[3] = readBytes[address];
            return BitConverter.ToSingle(bytesOfFloat, 0);
        }

        public void SetFloat(byte[] bytesToWrite, ushort address, float value) {
            byte[] bytesOfFloat = BitConverter.GetBytes(value);
            bytesToWrite[address] = bytesOfFloat[3];
            bytesToWrite[address + 1] = bytesOfFloat[2];
            bytesToWrite[address + 2] = bytesOfFloat[1];
            bytesToWrite[address + 3] = bytesOfFloat[0];
        }
    }
}
