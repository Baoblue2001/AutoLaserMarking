using FluentModbusPLC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SLTtechSoft
{
    public partial class PLCConfigForm : UserControl
    {
        public Class.Protocol Protocol;
        public ModbusMode Mode;
        public PLCConfigForm()
        {
            InitializeComponent();
        }

        private void rdModbus_CheckedChanged(object sender, EventArgs e)
        {
            if (rdModbus.Checked)
            {
                Protocol = Class.Protocol.Modbus;
            }
        }

        private void rdXGT_CheckedChanged(object sender, EventArgs e)
        {
            if (rdXGT.Checked)
            {
                Protocol = Class.Protocol.XGT;
            }
        }

        private void rdSLMP_CheckedChanged(object sender, EventArgs e)
        {
            if (rdSLMP.Checked)
            {
                Protocol = Class.Protocol.SLMP;
            }
        }

        private void rdTCP_CheckedChanged(object sender, EventArgs e)
        {
            if (rdSLMP.Checked)
            {
                Mode = ModbusMode.Tcp;
            }
        }

        private void rdRTU_CheckedChanged(object sender, EventArgs e)
        {
            if (rdSLMP.Checked)
            {
                Mode = ModbusMode.Rtu;
            }
        }
    }
}
