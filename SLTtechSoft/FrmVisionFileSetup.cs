using DocumentFormat.OpenXml.Office2013.Excel;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
namespace SLTtechSoft
{
    public partial class FrmVisionFileSetup : UserControl
    {
        private bool _SaveEnable;
        private int _visionNo;
        private string _visionName = "";
        private int _selectedRowIndex = -1;
        private ContextMenuStrip dgvMenu;
        public bool SaveEnable
        {
            get => _SaveEnable;
            set
            {
                _SaveEnable = value;
                btnSave.Enabled= value;
            }
        }
        public int VisionNo
        {
            get => _visionNo;
            set
            {
                _visionNo = value;
                lbVisionNo.Text = $"Vision {value}";
            }
        }
        public int SelectedRowIndex
        {
            get => _selectedRowIndex;
            set
            {
                _selectedRowIndex = value;
                lbRowSelected.Text = value.ToString();
            }
        }
        public string VisionName
        {
            get => _visionName;
            set
            {
                _visionName = value ?? "";
                lbName.Text = _visionName;
            }
        }
        public FrmVisionFileSetup()
        {
            InitializeComponent();
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Load += FrmVisionFileSetup_Load;

            InitDataGridViewMenu();
        }
        private void UpdateFormHeight()
        {
            int rowHeight = dataGridViewVision.RowTemplate.Height;
            int rowCount = dataGridViewVision.Rows.Cast<DataGridViewRow>().Count(r => r.Visible && !r.IsNewRow);

            int gridHeight = dataGridViewVision.ColumnHeadersHeight + rowCount * rowHeight + 3;

            int minGridHeight = dataGridViewVision.ColumnHeadersHeight + rowHeight + 3;
            int maxGridHeight = Screen.FromControl(this).WorkingArea.Height - 200;

            gridHeight = Math.Max(minGridHeight, Math.Min(gridHeight, maxGridHeight));

            int diff = gridHeight - dataGridViewVision.Height;

            dataGridViewVision.Height = gridHeight;
            this.Height += diff;
        }
        private void InitDataGridViewMenu()
        {
            dgvMenu = new ContextMenuStrip();

            dgvMenu.Items.Add("Change Direction", null, ChangeDirection_Click);
            dgvMenu.Items.Add("Move Up", null, MoveUp_Click);
            dgvMenu.Items.Add("Move Down", null, MoveDown_Click);

            dgvMenu.Items.Add(new ToolStripSeparator());
            dgvMenu.Items.Add("Insert", null, Insert_Click);
            dgvMenu.Items.Add("Delete", null, Delete_Click);

            dataGridViewVision.ContextMenuStrip = dgvMenu;
            dataGridViewVision.CellMouseDown += DataGridViewVision_CellMouseDown;
            dataGridViewVision.SelectionChanged += dataGridViewVision_SelectionChanged;
            dataGridViewVision.RowsAdded += DataGridViewVision_RowsChanged;
            dataGridViewVision.RowsRemoved += DataGridViewVision_RowsChanged;
        }
        private void FrmVisionFileSetup_Load(object sender, EventArgs e)
        {
            BeginInvoke(new Action(UpdateFormHeight));
        }

        private void DataGridViewVision_RowsChanged(object sender, EventArgs e)
        {
            BeginInvoke(new Action(UpdateFormHeight));
        }
        private void dataGridViewVision_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewVision.CurrentRow == null || dataGridViewVision.CurrentRow.IsNewRow)
            {
                SelectedRowIndex = -1;
                return;
            }

            SelectedRowIndex = dataGridViewVision.CurrentRow.Index;
        }
        private void DataGridViewVision_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;

            dataGridViewVision.ClearSelection();
            dataGridViewVision.Rows[e.RowIndex].Selected = true;
            dataGridViewVision.CurrentCell = dataGridViewVision.Rows[e.RowIndex].Cells[Math.Max(e.ColumnIndex, 0)];
        }
        private void ChangeDirection_Click(object sender, EventArgs e)
        {
            var row = dataGridViewVision.CurrentRow;
            if (row == null || row.IsNewRow) return;

            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "VisionMaster Process (*.prc)|*.prc";
                dlg.Title = "Select VisionMaster Process";
                dlg.CheckFileExists = true;
                dlg.Multiselect = false;

                if (dlg.ShowDialog() == DialogResult.OK)
                    row.Cells["clDirection"].Value = dlg.FileName;
            }
        }
        private void MoveUp_Click(object sender, EventArgs e)
        {
            var row = dataGridViewVision.CurrentRow;
            if (row == null || row.IsNewRow || row.Index <= 0) return;

            int index = row.Index;

            dataGridViewVision.Rows.RemoveAt(index);
            dataGridViewVision.Rows.Insert(index - 1, row);

            dataGridViewVision.ClearSelection();
            row.Selected = true;
            dataGridViewVision.CurrentCell = row.Cells[0];
            RefreshNo();
        }
        private void MoveDown_Click(object sender, EventArgs e)
        {
            var row = dataGridViewVision.CurrentRow;
            if (row == null || row.IsNewRow) return;

            int index = row.Index;
            int lastIndex = dataGridViewVision.AllowUserToAddRows ? dataGridViewVision.Rows.Count - 2 : dataGridViewVision.Rows.Count - 1;

            if (index >= lastIndex) return;

            dataGridViewVision.Rows.RemoveAt(index);
            dataGridViewVision.Rows.Insert(index + 1, row);

            dataGridViewVision.ClearSelection();
            row.Selected = true;
            dataGridViewVision.CurrentCell = row.Cells[0];
            RefreshNo();
        }
        private void Delete_Click(object sender, EventArgs e)
        {
            var row = dataGridViewVision.CurrentRow;
            if (row == null || row.IsNewRow) return;

            int rowCount = dataGridViewVision.Rows.Cast<DataGridViewRow>().Count(r => !r.IsNewRow);

            if (rowCount <= 1)
            {
                MessageBox.Show("The table must contain at least one row.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Do you want to delete the selected row?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dataGridViewVision.Rows.Remove(row);
            }
            RefreshNo();
        }
        private void Insert_Click(object sender, EventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(
       "Nhập tên dòng cần thêm:",
       "Insert Row",
       "");

            if (string.IsNullOrWhiteSpace(name)) return;

            int newIndex;

            var row = dataGridViewVision.CurrentRow;

            if (row == null || row.IsNewRow)
            {
                newIndex = dataGridViewVision.Rows.Add();
            }
            else
            {
                newIndex = row.Index + 1;
                dataGridViewVision.Rows.Insert(newIndex, 1);
            }

            dataGridViewVision.Rows[newIndex].Cells["clName"].Value = name;

            RefreshNo();

            dataGridViewVision.ClearSelection();
            dataGridViewVision.Rows[newIndex].Selected = true;
            dataGridViewVision.CurrentCell = dataGridViewVision.Rows[newIndex].Cells["clName"];
        }
        private void RefreshNo()
        {
            int no = 0;
            foreach (DataGridViewRow row in dataGridViewVision.Rows)
            {
                if (row.IsNewRow) continue;
                row.Cells["clNo"].Value = no++;
                if (!int.TryParse(row.Cells["clExposure"].Value?.ToString(), out int Exposure))
                {
                    row.Cells["clExposure"].Value = "5000";
                }

            }
        }
        public void SetTable(DataTable dataTable)
        {
            dataGridViewVision.Rows.Clear();
            if (dataTable == null) return;
            foreach (DataRow row in dataTable.Rows)
            {
                if (row.ItemArray[0] == null) return;
                string thisdata1 = row.ItemArray[0].ToString();
                if (thisdata1 == "") return;
                dataGridViewVision.Rows.Add(row.ItemArray);
            }
        }
        public DataTable GetDataTable()
        {
            try
            {
                if (dataGridViewVision.ColumnCount == 0) return null;
                DataTable dtSource = new DataTable();
                //////create columns
                foreach (DataGridViewColumn col in dataGridViewVision.Columns)
                {
                    if (col.ValueType == null) dtSource.Columns.Add(col.Name, typeof(string));
                    else dtSource.Columns.Add(col.Name, col.ValueType);
                    dtSource.Columns[col.Name].Caption = col.HeaderText;
                }
                ///////insert row data
                foreach (DataGridViewRow row in dataGridViewVision.Rows)
                {
                    DataRow drNewRow = dtSource.NewRow();
                    foreach (DataColumn col in dtSource.Columns)
                    {
                        drNewRow[col.ColumnName] = row.Cells[col.ColumnName].Value;
                    }
                    dtSource.Rows.Add(drNewRow);
                }
                return dtSource;
            }
            catch
            {
                return null;
            }
        }
        public void SetExposure(int no, int exposure)
        {
            if (exposure <= 0) exposure = 5000;
            foreach (DataGridViewRow row in dataGridViewVision.Rows)
            {
                if (row.IsNewRow) continue;
                if (int.TryParse(row.Cells["clNo"].Value?.ToString(), out int rowNo) && rowNo == no)
                {
                    row.Cells["clExposure"].Value = exposure;
                    return;
                }
            }
        }
        public int GetExposure(int no)
        {
            foreach (DataGridViewRow row in dataGridViewVision.Rows)
            {
                if (row.IsNewRow) continue;

                if (int.TryParse(row.Cells["clNo"].Value?.ToString(), out int rowNo) && rowNo == no)
                {
                    if (int.TryParse(row.Cells["clExposure"].Value?.ToString(), out int Exposure))
                    {
                        if (Exposure <= 0) Exposure = 5000;
                    }
                    return Exposure;
                }
            }
            return 5000;
        }
        public string GetFileName(int no)
        {
            foreach (DataGridViewRow row in dataGridViewVision.Rows)
            {
                if (int.TryParse(row.Cells["clNo"].Value?.ToString(), out int rowNo) && rowNo == no)
                {
                    string Name = row.Cells["clDirection"].Value?.ToString() ?? "";
                    return Name;
                }
            }
            return "";
        }
        public string GetName(int no)
        {
            foreach (DataGridViewRow row in dataGridViewVision.Rows)
            {
                if (int.TryParse(row.Cells["clNo"].Value?.ToString(), out int rowNo) && rowNo == no)
                {
                    string Name = row.Cells["clName"].Value?.ToString() ?? "";
                    return Name;
                }
            }
            return "";
        }
        public (int No, string Name, int Exposure, string FileName) GetCurrentRowData()
        {
            var row = dataGridViewVision.CurrentRow;
            if (row == null || row.IsNewRow) return (-1, "", 5000, "");

            int no = int.TryParse(row.Cells["clNo"].Value?.ToString(), out int n) ? n : -1;
            string name = row.Cells["clName"].Value?.ToString() ?? "";
            string path = row.Cells["clDirection"].Value?.ToString() ?? "";
            if (int.TryParse(row.Cells["clExposure"].Value?.ToString(), out int Exposure))
            {
                if (Exposure <= 0) Exposure = 5000;
            }

            return (no, name, Exposure, path);
        }
        public (int No, string Name, int Exposure, string FileName) GetRowDataByNo(int no)
        {
            foreach (DataGridViewRow row in dataGridViewVision.Rows)
            {
                if (row.IsNewRow) continue;

                if (int.TryParse(row.Cells["clNo"].Value?.ToString(), out int rowNo) && rowNo == no)
                {
                    string name = row.Cells["clName"].Value?.ToString() ?? "";
                    string path = row.Cells["clDirection"].Value?.ToString() ?? "";
                    if (int.TryParse(row.Cells["clExposure"].Value?.ToString(), out int Exposure))
                    {
                        if (Exposure <= 0) Exposure = 5000;
                    }

                    return (rowNo, name, Exposure, path);
                }
            }

            return (-1, "", 5000,"");
        }
        public int GetRowCount()
        {
            return dataGridViewVision.Rows.Count;
        }
    }
}
