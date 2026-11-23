using FluentModbus;
using FluentModbusPLC;
using System;
using System.IO.Ports;
namespace SLTtechSoft.Class
{
    public class PLCMagager 
    {
        private ModbusManager PLCModbus = new ModbusManager();
        private volatile bool _connected;
        public event Action<bool> ConnectionChanged;
        public event Action<string> Log;
        private Protocol protocol { get; set; }

        private void WriteLog(string message)
        {
            var handler = Log;
            if (handler != null) { try { handler(DateTime.Now.ToString("HH:mm:ss.fff") + "  " + message); } catch { } }
        }
        public bool Connect(PLCConfig PLCconfig)
        {
            bool connected = false;
            try
            {
                switch (PLCconfig.Protocol)
                {
                    case Protocol.Modbus:
                        {
                            var config = new ModbusConfig();
                            if (PLCconfig.Mode == ModbusMode.Tcp)
                            {
                                config = new ModbusConfig
                                {
                                    Mode = ModbusMode.Tcp,
                                    IpAddress = PLCconfig.IpAddress,
                                    Port = PLCconfig.Port,
                                    UnitId = PLCconfig.UnitId,
                                    AutoReconnect = PLCconfig.AutoReconnect,
                                    ReconnectInterval = PLCconfig.ReconnectInterval
                                };
                            }
                            else
                            {
                                config = new ModbusConfig
                                {
                                    Mode = ModbusMode.Rtu,
                                    ComPort = PLCconfig.ComPort,
                                    BaudRate = PLCconfig.BaudRate,
                                    Parity = PLCconfig.Parity,
                                    StopBits = PLCconfig.StopBits,
                                    UnitId = PLCconfig.UnitId,
                                    AutoReconnect = PLCconfig.AutoReconnect,
                                    ReconnectInterval = PLCconfig.ReconnectInterval
                                };
                            }
                             

                            break;
                        }
                    case Protocol.XGT:
                        {

                            break;
                        }

                    default: break;
                }

            
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }


            return connected;
        }

        public void Dispose()
        { 
         
        }
        public void Disconnect()
        { 
        
        }
    }
    public enum Protocol
    { 
        Modbus,
        XGT,
        SLMP
    }
 
    public sealed class PLCConfig
    {
        public Protocol Protocol { get; set; }
        public ModbusMode Mode { get; set; } = ModbusMode.Tcp;
        //ID
        public int UnitId { get; set; } = 1;
        // TCP
        public string IpAddress { get; set; } = "192.168.0.10";
        public int Port { get; set; } = 502;

        // RTU
        public string ComPort { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public Parity Parity { get; set; } = Parity.Even;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Handshake Handshake { get; set; } = Handshake.None;

        // Common
        public ModbusEndianness Endianness { get; set; } = ModbusEndianness.BigEndian;
        public int ConnectTimeout { get; set; } = 1000;
        public int ReadTimeout { get; set; } = 1000;
        public int WriteTimeout { get; set; } = 1000;
        public bool AutoReconnect { get; set; } = true;
        public int ReconnectInterval { get; set; } = 1000;
    }


}
