using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SLTtechSoft.Form1;

namespace SLTtechSoft
{
    public class WordConvert
    {
        public static int UshortToSigned32(ushort lowWord, ushort highWord)
        {
            return (highWord << 16) | lowWord;
        }
        public static bool[] UshortToBools(ushort value)
        {
            bool[] result = new bool[16];
            for (int i = 0; i < 16; i++) result[i] = (value & (1 << i)) != 0;
            return result;
        }
        public static ushort[] StringToRegisters(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return new ushort[0];

                // Chuyển string sang byte ASCII
                byte[] bytes = Encoding.ASCII.GetBytes(input);

                List<ushort> regs = new List<ushort>();

                for (int i = 0; i < bytes.Length; i += 2)
                {
                    byte high = bytes[i];
                    byte low = (i + 1 < bytes.Length) ? bytes[i + 1] : (byte)0;

                    int reg = (high << 8) | low;
                    regs.Add((ushort)reg);
                }


                return regs.ToArray();
            }
            catch (Exception)
            {
                return new ushort[0];
            }
        }
        public static string RegistersToString(int[] regs)
        {
            try
            {
                if (regs == null || regs.Length == 0)
                    return string.Empty;

                List<byte> bytes = new List<byte>();

                foreach (int reg in regs)
                {
                    // Tách thành 2 byte: HighByte và LowByte
                    byte high = (byte)((reg >> 8) & 0xFF);
                    byte low = (byte)(reg & 0xFF);

                    bytes.Add(high);
                    bytes.Add(low);
                }

                // Chuyển mảng byte thành chuỗi ASCII
                string result = Encoding.ASCII.GetString(bytes.ToArray());

                // Loại bỏ các ký tự null nếu có
                return result.TrimEnd('\0');
            }
            catch (Exception ex)
            {
                return "";
            }

        }

        public static Dword IntTo2Word(int Value)
        {
            Dword result = new Dword();
            if (Value > 0)
            {
                result.Upper = (ushort)(Value / 65536);
                result.Lowwer = (ushort)(Value % 65536);
            }
            else
            {
                int ValueRev = (2147483647 + Value + 1);
                int HighWord = ValueRev / 65536 + 32768;
                int LowWord = ValueRev % 65536;

                result.Upper = (ushort)HighWord;
                result.Lowwer = (ushort)LowWord;

            }
            return result;
        }
        public static bool[] WordTo16Bit(ushort Word)
        {
            bool[] result = new bool[16];
            var bools = new BitArray(new int[] { Word }).Cast<bool>().ToArray();
            result = bools;
            return result;
        }
        public static bool[] WordTo16Bit(int Word)
        {
            bool[] result = new bool[16];
            var bools = new BitArray(new int[] { Word }).Cast<bool>().ToArray();
            result = bools;
            return result;
        }

        public static int USigned32Toint(ushort Lowwer, ushort Upper)
        {
            int result = 0;
            result = Upper * 65536 + Lowwer;
            return result;
        }
        public static int Signed32Toint(ushort Lowwer, ushort Upper)
        {
            int result = 0;
            bool[] BitUpper = WordTo16Bit(Upper);
            bool[] BitLower = WordTo16Bit(Lowwer);
            // if > 0
            if (!BitUpper[15])
            {
                result = Upper * 65536 + Lowwer;
            }
            else
            {
                int NewUpper = Upper - 32768;
                int NewValue = 2147483647 - (NewUpper * 65536) - Lowwer + 1;
                result = -NewValue;
            }
            return result;
        }
        public static int Signed32Toint(int Lowwer, int Upper)
        {
            int result = 0;
            bool[] BitUpper = WordTo16Bit(Upper);
            bool[] BitLower = WordTo16Bit(Lowwer);
            // if > 0
            if (!BitUpper[15])
            {
                result = Upper * 65536 + Lowwer;
            }
            else
            {
                int NewUpper = Upper - 32768;
                int NewValue = 2147483647 - (NewUpper * 65536) - Lowwer + 1;
                result = -NewValue;
            }
            return result;
        }
        public static ushort Bit16ToWord(bool b0, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6, bool b7, bool b8, bool b9, bool b10, bool b11, bool b12, bool b13, bool b14, bool b15)
        {
            ushort result = 0;
            int D0 = b0 ? 1 : 0;
            int D1 = b1 ? 1 : 0;
            int D2 = b2 ? 1 : 0;
            int D3 = b3 ? 1 : 0;
            int D4 = b4 ? 1 : 0;
            int D5 = b5 ? 1 : 0;
            int D6 = b6 ? 1 : 0;
            int D7 = b7 ? 1 : 0;
            int D8 = b8 ? 1 : 0;
            int D9 = b9 ? 1 : 0;
            int D10 = b10 ? 1 : 0;
            int D11 = b11 ? 1 : 0;
            int D12 = b12 ? 1 : 0;
            int D13 = b13 ? 1 : 0;
            int D14 = b14 ? 1 : 0;
            int D15 = b15 ? 1 : 0;
            int ResultInt = D0 + D1*2 + D2*4 + D3*8 + D4*16 + D5*32 + D6*64 + D7*128 + D8*256 + D9*512 + D10*1024 + D11*2048 + D12*4096 + D13*8192 + D14*16384 + D15* 32768;
            result = (ushort)ResultInt;
            return result;
        }

    }
    public class Dword
    {
        public ushort Lowwer { get; set; }
        public ushort Upper { get; set; }
    }


}
