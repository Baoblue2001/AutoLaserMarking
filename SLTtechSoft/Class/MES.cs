using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Protocol;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.Serialization.Formatters;

namespace SLTtechSoft
{
    public class MESComunication
    {
        public int tcpClient_ReceiveTimeout = 5000;
        static TcpClient tcpClient;
        public string m_ipAddress = "127.0.0.1";
        public int m_tcpPort = 3047;
        static DateTime dtDisconnect = new DateTime();
        static DateTime dtNow = new DateTime();
        public bool NetworkIsOk = false;
        public string Datasending, DataRecived;
        public int numberOfTX;
        public string ExceptionCode = "";
        public string Status = "";
        public bool ReadHoldRegisterIsError, ReadCoilsIsError, WriteSingleCoilIsError, WriteMultiRegisterIsError;
        public string TX_cmd, RX_cmd;
        public bool isGetSerialNo = false;

        public string GetSerialNo(string Lotno)
        {
            string result = "526157";
            isGetSerialNo = true;
            return result;
        }
        static readonly string Digits = "0123456789ABCDEF";
        public string ByteTo1Hex(byte b)
        {
            char[] chars = new char[2];
            chars[0] = Digits[b / 16];
            chars[1] = Digits[b % 16];
            return new string(chars);
        }
        public string IntToHex(int b)
        {

            string result = "0";
            if (b >= 0 && b < 16)
            {
                switch (b)
                {
                    case 0: { result = "0"; break; }
                    case 1: { result = "1"; break; }
                    case 2: { result = "2"; break; }
                    case 3: { result = "3"; break; }
                    case 4: { result = "4"; break; }
                    case 5: { result = "5"; break; }
                    case 6: { result = "6"; break; }
                    case 7: { result = "7"; break; }
                    case 8: { result = "8"; break; }
                    case 9: { result = "9"; break; }
                    case 10: { result = "A"; break; }
                    case 11: { result = "B"; break; }
                    case 12: { result = "C"; break; }
                    case 13: { result = "D"; break; }
                    case 14: { result = "E"; break; }
                    case 15: { result = "F"; break; }
                    default: { result = "0"; break; }
                }
            }
            return result;
        }
        public string IntToHex2(int b)
        {
            string result = "0";
            if (b >= 0 && b < 256)
            {
                int b1 = b % 16;
                int b2 = b / 16;
                result = IntToHex(b2) + IntToHex(b1);
            }
            return result;
        }
        public bool[] Int8ToBools(int value)
        {
            bool[] bits = new bool[8];
            int remainder = 0;
            int i = 0;
            while (value > 0)
            {

                remainder = value % 2;
                value /= 2;
                bits[i] = Convert.ToBoolean(remainder);
                i++;
            }
            return bits;
        }

        public bool[] Int16ToBools(int value)
        {
            bool[] bits = new bool[16];
            int remainder = 0;
            int i = 0;
            while (value > 0)
            {

                remainder = value % 2;
                value /= 2;
                bits[i] = Convert.ToBoolean(remainder);
                i++;
            }
            return bits;
        }
        public bool[] Int32ToBools(int value)
        {
            bool[] bits = new bool[32];
            int remainder = 0;
            int i = 0;
            while (value > 0)
            {

                remainder = value % 2;
                value /= 2;
                bits[i] = Convert.ToBoolean(remainder);
                i++;
            }
            return bits;
        }

        public bool[] Hex8ToBools(string value)
        {
            int decvalue = HexToInt8(value);
            bool[] result = Int8ToBools(decvalue);
            return result;
        }

        public int HexToInt8(string Hexstring)
        {
            int result;
            if (Hexstring.Length == 0 || Hexstring.Length > 2)
            {
                result = 0;
            }
            else
            {

                if (Hexstring.Length == 2)
                {
                    string highstring = Hexstring.Substring(0, 1);
                    string lowstring = Hexstring.Substring(1, 1);
                    int highword = HexToInt4(highstring);
                    int lowword = HexToInt4(lowstring);
                    result = highword * 16 + lowword;
                }
                else
                {
                    result = HexToInt4(Hexstring);
                }

            }
            return result;
        }

        public int HexToInt4(string HexString)
        {
            int result;
            switch (HexString)
            {
                case "0": { result = 0; break; }
                case "1": { result = 1; break; }
                case "2": { result = 2; break; }
                case "3": { result = 3; break; }
                case "4": { result = 4; break; }
                case "5": { result = 5; break; }
                case "6": { result = 6; break; }
                case "7": { result = 7; break; }
                case "8": { result = 8; break; }
                case "9": { result = 9; break; }
                case "A": { result = 10; break; }
                case "B": { result = 11; break; }
                case "C": { result = 12; break; }
                case "D": { result = 13; break; }
                case "E": { result = 14; break; }
                case "F": { result = 15; break; }
                default: { result = 0; break; }
            }
            return result;
        }

        public bool ConnectTCP(string ipAddress, int tcpPort)
        {
            return Connect1(ipAddress, tcpPort);
        }
        private static bool CheckInternet()
        {
            //http://msdn.microsoft.com/en-us/library/windows/desktop/aa384702(v=vs.85).aspx
            // InternetConnectionState flag = InternetConnectionState.INTERNET_CONNECTION_LAN;
            return true;
        }
        private bool Connect1(string ipAddress, int tcpPort)
        {
            m_ipAddress = ipAddress;
            m_tcpPort = tcpPort;
            if (tcpClient != null)
                tcpClient.Close();
            tcpClient = new TcpClient();
            if (CheckInternet())
            {
                try
                {
                    IAsyncResult asyncResult = tcpClient.BeginConnect(ipAddress, tcpPort, null, null);
                    asyncResult.AsyncWaitHandle.WaitOne(2000, true); //wait for 3 sec
                    if (!asyncResult.IsCompleted)
                    {
                        tcpClient.Close();
                        ExceptionCode = "ERROR" + DateTime.Now.ToString() + ":Cannot connect to server.";
                        OnProtocolException(this, Protocol.ExceptionCode.ConnectionFailt, ExceptionCode);
                        Console.WriteLine(ExceptionCode);
                        return false;
                    }
                    else
                    {
                        ExceptionCode = "";
                        Status = DateTime.Now.ToString() + ":Connected to server.";
                        OnTransferLogData(this, "Connected", "Done");
                        NetworkIsOk = true;
                        return true;
                    }


                }
                catch (Exception ex)
                {
                    ExceptionCode = "ERROR" + DateTime.Now.ToString() + ":Connect process " + ex.StackTrace + "==>" + ex.Message;
                    OnProtocolException(this, Protocol.ExceptionCode.ConnectionFailt, ExceptionCode);
                    NetworkIsOk = false;
                    return false;
                }
            }
            else return false;
        }
        private void Reconnect()
        {
            try
            {
                if (NetworkIsOk)
                {
                }
                else
                {
                    dtNow = DateTime.Now;
                    if ((dtNow - dtDisconnect) > TimeSpan.FromSeconds(10))
                    {
                        Console.WriteLine(DateTime.Now.ToString() + ":Start connecting");

                        NetworkIsOk = Connect1(m_ipAddress, m_tcpPort);
                        if (!NetworkIsOk)
                        {
                            ExceptionCode = "ERROR" + DateTime.Now.ToString() + ":Connecting fail. Wait for retry";
                            OnProtocolException(this, Protocol.ExceptionCode.ConnectionFailt, ExceptionCode);
                            dtDisconnect = DateTime.Now;
                        }
                    }
                    else
                    {
                        Console.WriteLine(DateTime.Now.ToString() + ":Wait for retry connecting");
                    }

                }

            }
            catch (Exception exception)
            {
                if (exception.Source.Equals("System"))
                {
                    NetworkIsOk = false;
                    OnProtocolException(this, Protocol.ExceptionCode.ConnectionFailt, exception.Message);
                    dtDisconnect = DateTime.Now;
                }
            }
        }
        public void Disconnect()
        {
            try
            {
                if (NetworkIsOk)
                {
                    NetworkIsOk = false;
                    tcpClient.Close();
                    OnTransferLogData(this, "Disconnected", "Done");
                }
            }
            catch (Exception ex)
            {
                OnProtocolException(this, Protocol.ExceptionCode.ConnectionFailt, ex.Message);
            }
               

        } //class Disconnect

        public event ProtocolExceptionEventHandler ProtocolException;

        public delegate void ProtocolExceptionEventHandler(object sender, ExceptionCode exCode, string information);
        private void OnProtocolException(object sender, ExceptionCode exCode, string information)
        {
            if (this.ProtocolException != null)
            {
                this.ProtocolException(sender, exCode, information);
            }
        }

        public event TransferLogData TransferLog;
        public delegate void TransferLogData(object sender, string Type, string Data);
        public void OnTransferLogData(object sender, string Type, string Data)
        {
            if (this.TransferLog != null)
            {
                this.TransferLog(sender, Type, Data);
            }
        }

        public DataInfomationSend DataInformationRequest(string Lotno)
        {
            if (!NetworkIsOk)
            {
                OnTransferLogData(this, "Send Fail", "Network is Closing");
                return null;
            }
            DataInfomationSend result = new DataInfomationSend();
            byte[] bytes = Encoding.ASCII.GetBytes(Lotno);
            byte[] cmd = new byte[bytes.Length + 2];
            cmd[0] = 0x02;
            for (int i = 1; i < cmd.Length - 1; i++)
            {
                cmd[i] = bytes[i - 1];
            }
            cmd[cmd.Length - 1] = 0x03;
            TX_cmd = "";
            for (int i = 0; i < cmd.Length; i++)
            {
                string hex = IntToHex2((int)cmd[i]);
                TX_cmd = TX_cmd + hex + " ";
            }

            try
            {
                tcpClient.ReceiveTimeout = tcpClient_ReceiveTimeout;
                tcpClient.Client.Send(cmd);
                OnTransferLogData(this, "Send Lot", $"<STX>{Lotno}<ETX>");
                byte[] hi = new byte[500];
                int m = tcpClient.Client.Receive(hi);
                //DataRecived = Encoding.ASCII.GetString(hi, 1, m - 1);
                RX_cmd = "";
                for (int i = 0; i < m; i++)
                {
                    string hex = IntToHex2((int)hi[i]);
                    RX_cmd = RX_cmd + hex + " ";
                }

                if (m > 2)
                {

                    if (hi[0] == 02 && hi.Length > 2)
                    {
                        int ByteRecievedCount = 0;
                        for (int i = 1; i < hi.Length; i++)
                        {
                            if (hi[i] == 03)
                            {
                                ByteRecievedCount = i - 1; break;
                            }
                        }

                        byte[] hi2 = new byte[ByteRecievedCount];

                        for (int i = 0; i < ByteRecievedCount; i++)
                        {
                            hi2[i] = hi[i + 1];
                        }
                        string Temp = Encoding.ASCII.GetString(hi2);

                        OnTransferLogData(this, "Recieved Lot", $"<STX>{Lotno}<ETX>");

                        string[] Separate = Temp.Split(',');

                        if (Separate.Length != 5)
                        {
                            ExceptionCode = $"Data Recieved Wrong {Temp}";
                            result = null;
                            ReadHoldRegisterIsError = true;
                            isGetSerialNo = false;
                            return result;
                        }

                        result.Code = Separate[0];
                        result.YMMDDN = Separate[1];
                        result.SerialNo = Separate[2];
                        result.Rank = Separate[3].Split('/');
                        result.Array = Convert.ToInt32(Separate[4]);
                        isGetSerialNo = true;
                        Status = "ReadSerialNo is OK " + numberOfTX.ToString();
                        numberOfTX++;
                        if (numberOfTX > 65535) numberOfTX = 0;
                        ReadHoldRegisterIsError = false;
                        ExceptionCode = "";
                    }
                    else
                    {
                        ExceptionCode = $"Data Recieved Wrong {RX_cmd}";
                        result = null;
                        ReadHoldRegisterIsError = true;
                        isGetSerialNo = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionCode = ex.ToString();
                OnProtocolException(this, Protocol.ExceptionCode.ConnectionFailt, ex.Message);
                result = null;
                ReadHoldRegisterIsError = true;
                isGetSerialNo = false;
            }
            return result;
        }

        public bool EndLot()
        {
            if (!NetworkIsOk)
            {
                OnTransferLogData(this, "Send Fail", "Network is Closing");
                return false;
            }
            byte[] bytes = Encoding.ASCII.GetBytes("LOT END");
            byte[] cmd = new byte[bytes.Length + 2];
            cmd[0] = 0x02;
            for (int i = 1; i < cmd.Length - 1; i++)
            {
                cmd[i] = bytes[i - 1];
            }
            cmd[cmd.Length - 1] = 0x03;
            TX_cmd = "";
            for (int i = 0; i < cmd.Length; i++)
            {
                string hex = IntToHex2((int)cmd[i]);
                TX_cmd = TX_cmd + hex + " ";
            }
            try
            {
                //tcpClient.ReceiveTimeout = tcpClient_ReceiveTimeout;
                tcpClient.Client.Send(cmd);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionCode = ex.ToString();
                OnProtocolException(this, Protocol.ExceptionCode.ConnectionFailt, ex.Message);
                return false;
            }

        }
        public bool SendScanDataToMES(string Code)
        {
            if (!NetworkIsOk)
            {
                OnTransferLogData(this, "Send Fail", "Network is Closing");
                return false;
            }
            string ContentSend = Code;
            string TargetRecieved = $"SCAN DATA,{Code}";
            byte[] bytes = Encoding.ASCII.GetBytes(ContentSend);
            byte[] cmd = new byte[bytes.Length + 2];
            cmd[0] = 0x02;
            for (int i = 1; i < cmd.Length - 1; i++)
            {
                cmd[i] = bytes[i - 1];
            }
            cmd[cmd.Length - 1] = 0x03;
            TX_cmd = "";
            for (int i = 0; i < cmd.Length; i++)
            {
                string hex = IntToHex2((int)cmd[i]);
                TX_cmd = TX_cmd + hex + " ";
            }
            try
            {
                tcpClient.ReceiveTimeout = tcpClient_ReceiveTimeout;
                tcpClient.Client.Send(cmd);
                OnTransferLogData(this, "Send Code", $"<STX>{Code}<ETX>");
                byte[] hi = new byte[500];
                int m = tcpClient.Client.Receive(hi);
                RX_cmd = "";
                for (int i = 0; i < m; i++)
                {
                    string hex = IntToHex2((int)hi[i]);
                    RX_cmd = RX_cmd + hex + " ";
                }

                if (m > 2)
                {

                    if (hi[0] == 02 && hi.Length > 2)
                    {
                        int ByteRecievedCount = 0;
                        for (int i = 1; i < hi.Length; i++)
                        {
                            if (hi[i] == 03)
                            {
                                ByteRecievedCount = i - 1; break;
                            }
                        }
                        byte[] hi2 = new byte[ByteRecievedCount];
                        for (int i = 0; i < ByteRecievedCount; i++)
                        {
                            hi2[i] = hi[i + 1];
                        }
                        string Temp = Encoding.ASCII.GetString(hi2);
                        OnTransferLogData(this, "Recieved Code", $"<STX>{Temp}<ETX>");
                        ExceptionCode = "";
                        if (Temp == TargetRecieved)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    {
                        ExceptionCode = $"ScanDataToMES Error. {RX_cmd}";
                        return false;
                    }
                }
                else
                { return false; }
            }
            catch (Exception ex)
            {
                ExceptionCode = ex.ToString();
                OnProtocolException(this, Protocol.ExceptionCode.ConnectionFailt, ex.Message);
                return false;
            }
        }

    }
    public class DataInfomationSend
    {
        public string Code { get; set; }
        public string YMMDDN { get; set; }
        public string SerialNo { get; set; }
        public string[] Rank { get; set; }
        public int Array { get; set; }

    }
}
