using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SLTtechSoft
{
    public partial class FormIO : Form
    {
        Form1 _form1;
        
        List<Label> labelInput = new List<Label>();
        List<Label> labelWordUshort = new List<Label>();

        List<Label> labelWriteAuto = new List<Label>();

        List<Label> labelInputName = new List<Label>();
        List<Label> labelOutputName = new List<Label>();

        System.Drawing.Color On = System.Drawing.Color.Lime;
        System.Drawing.Color Off = System.Drawing.Color.Coral;
        public FormIO()
        {
            InitializeComponent();
        }

        public void InitializeUI(Form1 obj_Form1)
        {
            _form1 = obj_Form1;

            for (int i = 0; i < _form1.PLC.Read.AutoBits.Count; i++)
            {
                Label label1 = new Label()
                {
                    Size = new System.Drawing.Size(99, 42),
                    AutoSize = false,
                    BackColor = _form1.PLC.Read.AutoBits[i] ? On : Off,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new System.Windows.Forms.Padding(5),
                    Text = _form1.PLC.Read.AutoBits.GetName(i)
                };
                labelInput.Add(label1);
                flowLayoutPanel1.Controls.Add(label1);
            }

            for (int i = 0; i < _form1.PLC.Read.WordUshorts.Count; i++)
            {
                Label label1 = new Label()
                {
                    Size = new System.Drawing.Size(99, 42),
                    AutoSize = false,
                    BackColor = Off,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new System.Windows.Forms.Padding(5),
                    Text = $"{_form1.PLC.Read.WordUshorts.GetName(i)}: {_form1.PLC.Read.WordUshorts[i]}" 
                };
                labelWordUshort.Add(label1);
                flowLayoutPanel1.Controls.Add(label1);
            }

            for (int i = 0; i < _form1.PLC.Write.AutoBits.Count; i++)
            {
                Label label1 = new Label()
                {
                    Size = new System.Drawing.Size(99, 42),
                    AutoSize = false,
                    BackColor = _form1.PLC.Write.AutoBits[i] ? On : Off,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new System.Windows.Forms.Padding(5),
                    Text = _form1.PLC.Write.AutoBits.GetName(i)
                };
                labelWriteAuto.Add(label1);
                flowLayoutPanel2.Controls.Add(label1);
            }
        }
        
        private void timer1_Tick(object sender, System.EventArgs e)
        {
            for (int i = 0; i < _form1.PLC.Read.AutoBits.Count; i++)
            {
                labelInput[i].BackColor = _form1.PLC.Read.AutoBits[i] ? On: Off;
            }
            for (int i = 0; i < _form1.PLC.Read.WordUshorts.Count; i++)
            {
                labelWordUshort[i].Text = $"{_form1.PLC.Read.WordUshorts.GetName(i)}: \n {_form1.PLC.Read.WordUshorts[i]}";
            }

            for (int i = 0; i < _form1.PLC.Write.AutoBits.Count; i++)
            {
                labelWriteAuto[i].BackColor = _form1.PLC.Write.AutoBits[i] ? On : Off;
            }
        }

     

     

      
    }
}
