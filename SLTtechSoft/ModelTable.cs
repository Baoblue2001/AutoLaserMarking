
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Windows.Forms;

namespace SLTtechSoft
{
    public partial class ModelTable : UserControl
    {
        public event EventHandler NewModelRequested;

        public class ModelColumns
        {
            public int No = 0;
            public int Name = 1;
            public int File = 2;
            
        }
        public ModelColumns Col = new ModelColumns();
      
        public ModelTable()
        {
            InitializeComponent();

        }
        private void Table_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            RefreshValueTable();
        }
        public void RefreshValueTable()
        {
            for (int i = 0; i < Table.RowCount; i++)
            {
                Table.Rows[i].Height = 25;
                Table.Rows[i].Cells[Col.No].Value = i.ToString();
            }
        }
        private void Table_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (Table.CurrentCell.ColumnIndex == Col.File)
            {
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                TextBox ThisTextBox = sender as TextBox;
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName;
                    fileName = dlg.FileName;
                    Table.Rows[Table.CurrentCell.RowIndex].Cells[Col.File].Value = fileName;


                }
            }
        }

        private void AddRowBelow(DataGridView dgv)
        {
            
               
            int rowIndex = dgv.CurrentCell.RowIndex;

            if (MessageBox.Show("Bạn có muốn thêm một dòng mới phía dưới không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            if (dgv.CurrentCell == null)
            {
                dgv.Rows.Insert(0, 1);
                return;
            }

            dgv.Rows.Insert(rowIndex + 1, 1);
            dgv.CurrentCell = dgv.Rows[rowIndex + 1].Cells[dgv.CurrentCell.ColumnIndex];
        }

        private void MoveRowUp(DataGridView dgv)
        {
            if (dgv.CurrentCell == null) return;
            int rowIndex = dgv.CurrentCell.RowIndex;
            int colIndex = dgv.CurrentCell.ColumnIndex;
            if (rowIndex <= 0) return;

            if (MessageBox.Show("Bạn có muốn chuyển dòng này lên trên không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            DataGridViewRow row = dgv.Rows[rowIndex];
            dgv.Rows.RemoveAt(rowIndex);
            dgv.Rows.Insert(rowIndex - 1, row);
            dgv.CurrentCell = dgv.Rows[rowIndex - 1].Cells[colIndex];
        }

        private void MoveRowDown(DataGridView dgv)
        {
            if (dgv.CurrentCell == null) return;
            int rowIndex = dgv.CurrentCell.RowIndex;
            int colIndex = dgv.CurrentCell.ColumnIndex;
            int lastIndex = dgv.AllowUserToAddRows ? dgv.Rows.Count - 2 : dgv.Rows.Count - 1;
            if (rowIndex >= lastIndex) return;

            if (MessageBox.Show("Bạn có muốn chuyển dòng này xuống dưới không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            DataGridViewRow row = dgv.Rows[rowIndex];
            dgv.Rows.RemoveAt(rowIndex);
            dgv.Rows.Insert(rowIndex + 1, row);
            dgv.CurrentCell = dgv.Rows[rowIndex + 1].Cells[colIndex];
        }
        private void DeleteSelectedRow(DataGridView dgv)
        {
            if (dgv.CurrentCell == null) return;

            int rowIndex = dgv.CurrentCell.RowIndex;
            if (dgv.Rows[rowIndex].IsNewRow) return;

            if (MessageBox.Show("Bạn có muốn xóa dòng đang chọn không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            dgv.Rows.RemoveAt(rowIndex);
            RefreshValueTable();
        }
        private void btnInsert_Click(object sender, EventArgs e)
        {
            AddRowBelow(Table);
        }

        private void btnNewModel_Click(object sender, EventArgs e)
        {
            NewModelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            MoveRowUp(Table);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            MoveRowDown(Table);
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            //DeleteSelectedRow(Table);
        }
    }
}
