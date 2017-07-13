namespace Pinpoint.Agent.Network
{
    using Agent.Thrift.IO;
    using Common;
    using Control;
    using Packet;
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Threading;

    public class DefaultPinpointTcpClient
    {
        private Object locker = new Object();

        private TcpClient client = null;

        private NetworkStream networkStream = null;

        private Thread receiveThread = null;

        public DefaultPinpointTcpClient()
        {
            if (client == null)
            {
                lock (locker)
                {
                    if (client == null)
                    {
                        client = new TcpClient();
                        client.Connect("10.10.11.70", 9994);
                        if (!client.Connected)
                        {
                            throw new SocketException((int)SocketError.NotConnected);
                        }

                        receiveThread = new Thread(StartReceive);
                        receiveThread.Start();
                    }
                }
            }
        }

        public void Send(byte[] payload)
        {
            lock (locker)
            {
                networkStream.Write(payload, 0, payload.Length);
                networkStream.Flush();
            }
        }

        private void StartReceive()
        {
            if (networkStream == null)
            {
                networkStream = client.GetStream();
            }

            while (true)
            {
                Receive();
            }
        }

        public Object Receive()
        {
            var packetType = BufferHelper.ReadShort(networkStream);
            switch (packetType)
            {
                case PacketType.APPLICATION_SEND:
                    return readSend(packetType, networkStream);
                case PacketType.APPLICATION_REQUEST:
                    return readRequest(packetType, networkStream);
                case PacketType.APPLICATION_RESPONSE:
                    return readResponse(packetType, networkStream);
                case PacketType.APPLICATION_STREAM_CREATE:
                    return readStreamCreate(packetType, networkStream);
                case PacketType.APPLICATION_STREAM_CLOSE:
                    return readStreamClose(packetType, networkStream);
                case PacketType.APPLICATION_STREAM_CREATE_SUCCESS:
                    return readStreamCreateSuccess(packetType, networkStream);
                case PacketType.APPLICATION_STREAM_CREATE_FAIL:
                    return readStreamCreateFail(packetType, networkStream);
                case PacketType.APPLICATION_STREAM_RESPONSE:
                    return readStreamData(packetType, networkStream);
                case PacketType.APPLICATION_STREAM_PING:
                    return readStreamPing(packetType, networkStream);
                case PacketType.APPLICATION_STREAM_PONG:
                    return readStreamPong(packetType, networkStream);
                case PacketType.CONTROL_CLIENT_CLOSE:
                    return readControlClientClose(packetType, networkStream);
                case PacketType.CONTROL_SERVER_CLOSE:
                    return readControlServerClose(packetType, networkStream);
                case PacketType.CONTROL_PING:
                    sendPong();
                    return null;
                case PacketType.CONTROL_PONG:
                    readPong(packetType, networkStream);
                    // just also drop pong.
                    return null;
                case PacketType.CONTROL_HANDSHAKE:
                    return readEnableWorker(packetType, networkStream);
                case PacketType.CONTROL_HANDSHAKE_RESPONSE:
                    return readEnableWorkerConfirm(packetType, networkStream);
                default:
                    return null;
            }
        }

        private Object readControlClientClose(short packetType, NetworkStream stream)
        {
            return ClientClosePacket.ReadBuffer(packetType, stream);
        }

        private Object readControlServerClose(short packetType, NetworkStream stream)
        {
            return ServerClosePacket.ReadBuffer(packetType, stream);
        }

        private Object readPong(short packetType, NetworkStream buffer)
        {
            return PongPacket.ReadBuffer(packetType, buffer);
        }

        private Object readPing(short packetType, NetworkStream buffer)
        {
            return PingPacket.readBuffer(packetType, buffer);
        }

        private Object readSend(short packetType, NetworkStream stream)
        {
            throw new NotImplementedException();
            //return SendPacket.readBuffer(packetType, buffer);
        }

        private Object readRequest(short packetType, NetworkStream stream)
        {
            //TODO:complete this method
            return null;
        }

        private Object readResponse(short packetType, NetworkStream buffer)
        {
            var packet = ResponsePacket.ReadBuffer(packetType, buffer);
            var @base = new HeaderTBaseDeserializer().Deserialize(packet.Payload);
            System.Diagnostics.Debug.WriteLine("readEnableWorkerConfirm" + @base.GetType());
            return packet;
        }

        private Object readStreamCreate(short packetType, NetworkStream stream)
        {
            //TODO:complete this method
            var packet = StreamCreatePacket.ReadBuffer(packetType, stream);
            var payload = new StreamCreateSuccessPacket(packet.StreamChannelId).ToBuffer();
            networkStream.Write(payload, 0, payload.Length);
            networkStream.Flush();
            return packet;
        }


        private Object readStreamCreateSuccess(short packetType, NetworkStream stream)
        {
            throw new NotImplementedException();
            //return StreamCreateSuccessPacket.readBuffer(packetType, buffer);
        }

        private Object readStreamCreateFail(short packetType, NetworkStream stream)
        {
            throw new NotImplementedException();
            //return StreamCreateFailPacket.readBuffer(packetType, buffer);
        }

        private Object readStreamData(short packetType, NetworkStream stream)
        {
            throw new NotImplementedException();
            //return StreamResponsePacket.readBuffer(packetType, buffer);
        }

        private Object readStreamPong(short packetType, NetworkStream stream)
        {
            throw new NotImplementedException();
            //return StreamPongPacket.readBuffer(packetType, buffer);
        }

        private Object readStreamPing(short packetType, NetworkStream stream)
        {
            throw new NotImplementedException();
            //return StreamPingPacket.readBuffer(packetType, buffer);
        }

        private Object readStreamClose(short packetType, NetworkStream stream)
        {
            return StreamClosePacket.ReadBuffer(packetType, stream);
        }

        private Object readEnableWorker(short packetType, NetworkStream stream)
        {
            throw new NotImplementedException();
            //return ControlHandshakePacket.readBuffer(packetType, buffer);
        }

        private Object readEnableWorkerConfirm(short packetType, NetworkStream buffer)
        {
            var packet = ControlHandshakeResponsePacket.ReadBuffer(packetType, buffer);
            var map = (Dictionary<object, object>)(new ControlMessageDecoder().Decode(packet.Payload));
            System.Diagnostics.Debug.WriteLine("readEnableWorkerConfirm" + map.Keys.Count);
            return packet;
        }

        private void sendPong()
        {
            // a "pong" responds to a "ping" automatically.
            var payload = PongPacket.PONG_PACKET.ToBuffer();
            networkStream.Write(payload, 0, payload.Length);
            networkStream.Flush();
        }
    }
}
