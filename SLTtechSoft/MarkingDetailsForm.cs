using System;
using System.Windows.Forms;

namespace SLTtechSoft
{
    public partial class MarkingDetailsForm : Form
    {
        public string MarkingType { get; set; }
        public string ProductNumber { get; set; }
        public string Content { get; set; }

        public MarkingDetailsForm(string markingType, string productNumber, string content)
        {
            InitializeComponent();
            MarkingType = markingType ?? "CODE";
            ProductNumber = productNumber ?? "0";
            Content = content ?? "";
            typeComboBox.Text = MarkingType;
            productNumberTextBox.Text = ProductNumber;
            contentTextBox.Text = Content;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            MarkingType = typeComboBox.Text;
            ProductNumber = productNumberTextBox.Text.Trim();
            Content = contentTextBox.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
