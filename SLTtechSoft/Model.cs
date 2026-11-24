using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using static SLTtechSoft.Form1;
using static SLTtechSoft.ModelTable;
using static WindowsFormsApp1.Login;
using SLTtechSoft.Class;
using VMControls.Winform.Release;

namespace SLTtechSoft
{
    public partial class Model : Form
    {
        public string PathFileParameter = "D:\\InitialData\\Laser Marking\\ModelInfomation.txt";
        #region Data Declarations
        Form1 _form1;
        List<WindowsFormsApp1.Login.User> users = new List<WindowsFormsApp1.Login.User>();
        public CurrentModelInfomation currentModelInfomation = new CurrentModelInfomation();
        public ModelParameter modelParameter = new ModelParameter();
        public VmMainViewConfigControl vmProcedureConfigControl1;
        private VisionMasterProcess currentVisionProcess;
        private bool currentVisionProcessOwnedByEditor = false;
        public int IndexImageChoosing = 0;
        public ModelTable modelTable = new ModelTable();
        public FrmVisionFileSetup[] ListFrmVisionFileSetup = new FrmVisionFileSetup[4];
        private int CamSelectedIndex = 0;
        private int RowSelectedIndex = 0;
        public bool IsliveCamModel = false;

        private string DirectorImageFolder = "";
        private string[] FolderImageFiles;
        private int ImageFolderCount = 0;
        private bool ImageFolderOrFile = false;
        private int panel1FirstWidth = 0;
        private string FileImageOpening = "";
        private bool IsImageFileToolRunOnce = false;
        public LaserSettingLayout laserSettingLayout;
        #endregion

        public Model()
        {
            InitializeComponent();
            
            radioUseCam.Checked = true;
            panel1FirstWidth = panel1.Width;
            panel4.Width = panel1.Width;
            InitializeLaserSettingLayout();
        }

        private void InitializeLaserSettingLayout()
        {
            laserSettingLayout = new LaserSettingLayout
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                ShowInTaskbar = false
            };

            panel9.Controls.Add(laserSettingLayout);
            laserSettingLayout.Show();
        }
        public void InitializeUI(Form1 obj)
        {
            _form1 = obj;
            laserSettingLayout.SaveAllRequested += LaserSettingLayoutSaveAllRequested;
            laserSettingLayout.ScanImageRequested += LaserSettingLayout_ScanImageRequested;

            InitialFormVisionSetup();

            // Do not create VmMainViewConfigControl at startup. VisionMaster procedures are loaded
            // for runtime independently. The editor is created only when the user presses Load
            // on a specific vision row, so model Save/Call does not populate the editor implicitly.
            vmProcedureConfigControl1 = null;

            modelTable = new ModelTable();
            modelTable.Dock = DockStyle.Fill;
            panel4.Controls.Add(modelTable);
            modelTable.Save.Click += ModelTableSave_Click;
            modelTable.Call.Click += ModelTalbeCall_Click;
            modelTable.Delete.Click += ModelTalbeDelete_Click;
            modelTable.NewModelRequested += ModelTable_NewModelRequested;
            ReadModelInfomation();

            if (modelTable.Table.Rows.Count > 0)
            {
                int modelNo = currentModelInfomation.ModelNo;
                if (modelNo < 0 || modelNo >= modelTable.Table.Rows.Count) modelNo = 0;
                LoadAModel(modelNo);
            }
            else
            {
                modelParameter = new ModelParameter();
                ShowAModelParameter(modelParameter);
            }

        }
        private void InitialFormVisionSetup()
        {
            ListFrmVisionFileSetup = new FrmVisionFileSetup[_form1.NumberOfCamera];
            if (_form1.formOption == null) return;
            if (_form1.formOption.Parameters == null) return;
            if (_form1.formOption.Parameters.Cam == null) return;
            for (int i = 0; i < _form1.NumberOfCamera; i++)
            {
                if (_form1.formOption.Parameters.Cam[i] == null) continue;
                ListFrmVisionFileSetup[i] = new FrmVisionFileSetup
                {
                    VisionNo = i + 1,
                    VisionName = _form1.formOption.Parameters.Cam[i].Name,
                    Tag = i,
                    SaveEnable = false,
                    Width = flpVisionFileSetup.Width-10
                };
                ListFrmVisionFileSetup[i].lbName.Text = _form1.formOption.Parameters.Cam[i].Name;
                ListFrmVisionFileSetup[i].lbVisionNo.Text = $"Cam {i+1}";
                ListFrmVisionFileSetup[i].btnLoad.Tag = i;
                ListFrmVisionFileSetup[i].btnSave.Tag = i;
                ListFrmVisionFileSetup[i].btnLoad.Click += LoadVisionFile_Click;
                ListFrmVisionFileSetup[i].btnSave.Click += SaveVisionFile_Click;
                flpVisionFileSetup.Controls.Add(ListFrmVisionFileSetup[i]);
            }
        }

        private void LoadVisionFile_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            int cam = (int)button.Tag;
            int index = ListFrmVisionFileSetup[cam].SelectedRowIndex;
            if (index < 0)
            {
                MessageBox.Show("Please select a vision row first.", "Load VisionMaster", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CamSelectedIndex = cam;
            RowSelectedIndex = index;
            if (_form1.formMain != null && _form1.formMain.IsUiInitialized) _form1.formMain.SwitchModeAuto(ModeMachine.Manual);
            else _form1.MachineMode = ModeMachine.Manual;
            LoadVisionMasterProcessEditor(cam, index);

            bool loaded = currentVisionProcess != null && currentVisionProcess.Procedure != null;
            for (int i = 0; i < ListFrmVisionFileSetup.Length; i++)
            {
                if (ListFrmVisionFileSetup[i] == null) continue;
                ListFrmVisionFileSetup[i].SaveEnable = loaded && i == cam;
            }
        }
        private void SaveVisionFile_Click(object sender, EventArgs e)
        {
            try
            {
                Button button = sender as Button;
                int cam = button?.Tag is int ? (int)button.Tag : CamSelectedIndex;
                int index = cam >= 0 && cam < ListFrmVisionFileSetup.Length ? ListFrmVisionFileSetup[cam].SelectedRowIndex : -1;
                if (index < 0) index = RowSelectedIndex;

                if (currentVisionProcess == null || currentVisionProcess.Procedure == null)
                {
                    MessageBox.Show("Please Load the procedure before Save.", "Save VisionMaster", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cam != CamSelectedIndex || index != RowSelectedIndex)
                {
                    MessageBox.Show("The selected row is not the procedure currently opened in the editor. Press Load first.", "Save VisionMaster", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string fileName = NormalizeVisionFileName(ListFrmVisionFileSetup[cam].GetFileName(index));
                if (string.IsNullOrWhiteSpace(fileName) || !SamePath(fileName, currentVisionProcess.FilePath))
                {
                    MessageBox.Show("The PRC path was changed after the procedure was loaded. Press Load again before Save.", "Save VisionMaster", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save the exact procedure instance currently shown in the editor. Do not reload all
                // procedures and do not rebind VmMainViewConfigControl after Save.
                _form1.RunWithVisionRuntimeLock(() => currentVisionProcess.Save());

                // Normally the editor uses the already-loaded runtime instance. If this row was not
                // available in runtime and the editor had to load a private instance, transfer that
                // same instance into the existing runtime slot instead of loading the PRC a second time.
                if (currentVisionProcessOwnedByEditor && _form1.RunWithVisionRuntimeLock(() => TryReplaceRuntimeProcess(cam, index, currentVisionProcess)))
                    currentVisionProcessOwnedByEditor = false;

                _form1.WriteLogPC(LogType.Vision, "SaveVisionMasterProcess", $"Saved Cam{cam + 1}, Index{index}: {currentVisionProcess.FilePath}");
                MessageBox.Show($"Save VisionMaster process done. Cam{cam + 1}, Index{index}.\n{currentVisionProcess.FilePath}", "Save VisionMaster", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Save VisionMaster Process Fail");
                _form1.WriteLogPC(LogType.Vision, "SaveVisionMasterProcess", ex.ToString());
            }
        }
        private void ModelTableSave_Click(object sender, EventArgs e)
        {
            if (SaveAModel()) SaveCurrentModelInfomation(true);
        }

        private void ModelTable_NewModelRequested(object sender, EventArgs e)
        {
            if (modelTable.Table.CurrentCell == null)
            {
                MessageBox.Show("Please select a model to copy first.", "Create Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sourceRow = modelTable.Table.CurrentCell.RowIndex;
            if (sourceRow < 0 || sourceRow >= modelTable.Table.Rows.Count || modelTable.Table.Rows[sourceRow].IsNewRow)
            {
                MessageBox.Show("Please select a valid model to copy.", "Create Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sourceFile = Convert.ToString(modelTable.Table.Rows[sourceRow].Cells[modelTable.Col.File].Value);
            if (string.IsNullOrWhiteSpace(sourceFile) || !File.Exists(sourceFile))
            {
                MessageBox.Show("The selected model file does not exist.", "Create Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (Form dialog = new Form())
            using (TextBox nameBox = new TextBox())
            using (Button okButton = new Button())
            using (Button cancelButton = new Button())
            {
                dialog.Text = "Create New Model";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(360, 125);

                Label label = new Label { AutoSize = true, Text = "Model name:", Location = new Point(15, 18) };
                nameBox.Location = new Point(15, 42);
                nameBox.Width = 330;
                okButton.Text = "Create";
                okButton.DialogResult = DialogResult.OK;
                okButton.Location = new Point(184, 78);
                cancelButton.Text = "Cancel";
                cancelButton.DialogResult = DialogResult.Cancel;
                cancelButton.Location = new Point(270, 78);
                dialog.Controls.AddRange(new Control[] { label, nameBox, okButton, cancelButton });
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                CreateNewModel(nameBox.Text, sourceFile);
            }
        }

        private void CreateNewModel(string modelName, string sourceFile)
        {
            modelName = (modelName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(modelName))
            {
                MessageBox.Show("Please enter a model name.", "Create Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (char invalid in Path.GetInvalidFileNameChars())
                if (modelName.IndexOf(invalid) >= 0)
                {
                    MessageBox.Show("Model name contains invalid characters.", "Create Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

            foreach (DataGridViewRow row in modelTable.Table.Rows)
                if (!row.IsNewRow && string.Equals(Convert.ToString(row.Cells[modelTable.Col.Name].Value), modelName, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("This model already exists.", "Create Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

            string rootFolder = @"D:\InitialData\Laser Marking\Model";
            string modelFolder = Path.Combine(rootFolder, modelName);
            string modelFile = Path.Combine(modelFolder, modelName + ".txt");
            if (Directory.Exists(modelFolder) || File.Exists(modelFile))
            {
                MessageBox.Show("This model already exists.", "Create Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ModelParameter newModel = JsonConvert.DeserializeObject<ModelParameter>(File.ReadAllText(sourceFile));
                if (newModel == null) throw new InvalidDataException("The selected model file is invalid or empty.");
                newModel.ModelName = modelName;

                Directory.CreateDirectory(modelFolder);
                CopyVisionFilesToModelFolder(newModel, sourceFile, modelFolder);
                CopyLaserPicturesToModelFolder(newModel, sourceFile, modelFolder);
                File.WriteAllText(modelFile, JsonConvert.SerializeObject(newModel));

                int row = modelTable.Table.Rows.Add(modelTable.Table.Rows.Count, modelName, modelFile);
                modelTable.Table.CurrentCell = modelTable.Table.Rows[row].Cells[modelTable.Col.Name];
                SetCurrentModelRow(row);
                _form1.ModelNoInternal = row;
                currentModelInfomation.ModelNo = row;
                currentModelInfomation.PathFileModel = modelFile;
                modelParameter = newModel;
                laserSettingLayout.SetModelFolderPath(modelFolder);
                ShowAModelParameter(newModel);
                SaveCurrentModelInfomation(true);
                MessageBox.Show("New model copied successfully.", "Create Model", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Create Model Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CopyLaserPicturesToModelFolder(ModelParameter model, string sourceModelFile, string targetFolder)
        {
            if (model == null || model.Cam == null) return;

            string sourceFolder = Path.GetDirectoryName(Path.GetFullPath(sourceModelFile));
            Dictionary<string, string> copiedFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int camIndex = 0; camIndex < model.Cam.Length; camIndex++)
            {
                CamPara camera = model.Cam[camIndex];
                if (camera == null) continue;

                CopyLaserSurfacePictures(camera.Top, "TOP", sourceFolder, targetFolder, copiedFiles);
                CopyLaserSurfacePictures(camera.Bot, "BOT", sourceFolder, targetFolder, copiedFiles);

                camera.LaserTopImageFile = CopyLaserImageReference(
                    camera.LaserTopImageFile, "TOP", sourceFolder, targetFolder, copiedFiles);
                camera.LaserBotImageFile = CopyLaserImageReference(
                    camera.LaserBotImageFile, "BOT", sourceFolder, targetFolder, copiedFiles);
                camera.LaserImageFile = camera.LaserTopImageFile;
            }
        }

        private void CopyLaserSurfacePictures(
            LaserSurfaceParameter surface, string surfaceName, string sourceFolder,
            string targetFolder, Dictionary<string, string> copiedFiles)
        {
            if (surface == null || surface.ScanImages == null) return;

            for (int i = 0; i < surface.ScanImages.Count; i++)
            {
                LaserScanImageParameter picture = surface.ScanImages[i];
                if (picture == null) continue;

                string copiedRelativePath = CopyLaserImageReference(
                    picture.ImagePath, surfaceName, sourceFolder, targetFolder, copiedFiles);
                picture.ImagePath = copiedRelativePath;
            }
        }

        private string CopyLaserImageReference(
            string imageReference, string surfaceName, string sourceFolder,
            string targetFolder, Dictionary<string, string> copiedFiles)
        {
            if (string.IsNullOrWhiteSpace(imageReference)) return "";

            string sourcePath = Path.IsPathRooted(imageReference)
                ? imageReference
                : Path.Combine(sourceFolder, imageReference);
            sourcePath = Path.GetFullPath(sourcePath);
            if (!File.Exists(sourcePath)) return "";

            string targetPath;
            if (!copiedFiles.TryGetValue(sourcePath, out targetPath))
            {
                string destinationFolder = Path.Combine(targetFolder, "LaserPictures", surfaceName);
                Directory.CreateDirectory(destinationFolder);
                string extension = Path.GetExtension(sourcePath);
                string fileName = Path.GetFileNameWithoutExtension(sourcePath) + "_" +
                    Guid.NewGuid().ToString("N").Substring(0, 8) + extension;
                targetPath = Path.Combine(destinationFolder, fileName);
                File.Copy(sourcePath, targetPath, false);
                copiedFiles.Add(sourcePath, targetPath);
            }

            return Path.Combine("LaserPictures", surfaceName, Path.GetFileName(targetPath));
        }

        private void CopyVisionFilesToModelFolder(ModelParameter model, string sourceModelFile, string targetFolder)
        {
            if (model.Cam == null) return;

            string sourceFolder = Path.GetDirectoryName(Path.GetFullPath(sourceModelFile));
            Dictionary<string, string> copiedFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> destinationNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int camIndex = 0; camIndex < model.Cam.Length; camIndex++)
            {
                DataTable visionTable = model.Cam[camIndex]?.VisionTable;
                if (visionTable == null || !visionTable.Columns.Contains("clDirection")) continue;

                foreach (DataRow row in visionTable.Rows)
                {
                    string referencedFile = Convert.ToString(row["clDirection"]);
                    if (string.IsNullOrWhiteSpace(referencedFile)) continue;

                    string sourcePath = Path.IsPathRooted(referencedFile)
                        ? referencedFile.Trim()
                        : Path.Combine(sourceFolder, referencedFile.Trim());
                    sourcePath = Path.GetFullPath(NormalizeVisionFileName(sourcePath));
                    if (!File.Exists(sourcePath))
                        throw new FileNotFoundException("A VisionMaster setup file referenced by the selected model was not found.", sourcePath);

                    string targetPath;
                    if (!copiedFiles.TryGetValue(sourcePath, out targetPath))
                    {
                        string fileName = Path.GetFileName(sourcePath);
                        string targetName = fileName;
                        if (destinationNames.Contains(targetName))
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                            string extension = Path.GetExtension(fileName);
                            targetName = "Cam" + (camIndex + 1) + "_" + fileName;
                            int suffix = 2;
                            while (destinationNames.Contains(targetName))
                                targetName = "Cam" + (camIndex + 1) + "_" + fileNameWithoutExtension + "_" + suffix++ + extension;
                        }

                        destinationNames.Add(targetName);
                        targetPath = Path.Combine(targetFolder, targetName);
                        VisionMasterProcess editorProcess = currentVisionProcess;
                        if (editorProcess != null && SamePath(editorProcess.FilePath, sourcePath))
                            _form1.RunWithVisionRuntimeLock(() => editorProcess.SaveAs(targetPath));
                        else
                            File.Copy(sourcePath, targetPath);

                        if (!File.Exists(targetPath) || new FileInfo(targetPath).Length == 0)
                            throw new IOException("The copied VisionMaster setup file is missing or empty: " + targetPath);
                        copiedFiles.Add(sourcePath, targetPath);
                    }

                    row["clDirection"] = targetPath;
                }
            }
        }

        private void SetCurrentModelRow(int modelNo)
        {
            for (int i = 0; i < modelTable.Table.RowCount; i++)
            {
                if (modelTable.Table.Rows[i].IsNewRow) continue;
                modelTable.Table.Rows[i].DefaultCellStyle.BackColor = i == modelNo
                    ? System.Drawing.Color.Lime
                    : System.Drawing.Color.White;
            }
        }

        private void LaserSettingLayoutSaveAllRequested(object sender, EventArgs e)
        {
            if (SaveAModel()) SaveCurrentModelInfomation(true);
        }

        private void LaserSettingLayout_ScanImageRequested(Action<Bitmap, int, int> onCaptured)
        {
            string modelFile = currentModelInfomation.PathFileModel;
            if (string.IsNullOrWhiteSpace(modelFile) || !File.Exists(modelFile))
            {
                MessageBox.Show("Please save the current Model before using Scan Image.", "Scan Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            laserSettingLayout.SetModelFolderPath(Path.GetDirectoryName(modelFile));

            // Laser Setting currently stores its parameters in Cam[0], so use Camera 1.
            if (_form1 == null || !_form1.RequestLaserSetupCapture(0, onCaptured))
                MessageBox.Show("Camera 1 is not ready, or another Scan Image request is still waiting for a frame.", "Scan Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void ModelTalbeCall_Click(object sender, EventArgs e)
        {
            if (!_form1.formOption.Parameters.Option.CallModelManual)
            {
                MessageBox.Show("Please Use call Model Manual.","Call Model Fail");
                return;
            } 
            ModelColumns Col = new ModelColumns();
            if (modelTable.Table.CurrentCell == null) return;
            int ModelNo = modelTable.Table.CurrentCell.RowIndex;
            if (ModelNo < 0 || ModelNo >= modelTable.Table.Rows.Count || modelTable.Table.Rows[ModelNo].IsNewRow) return;
            LoadAModel(ModelNo);
            string path = currentModelInfomation.PathFileModel;
            SaveCurrentModelInfomation(true);
        }
        private void ModelTalbeDelete_Click(object sender, EventArgs e)
        {
            DataGridView table = modelTable.Table;
            if (table.CurrentCell == null)
            {
                MessageBox.Show("Please select a model to delete.", "Delete Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rowIndex = table.CurrentCell.RowIndex;
            if (rowIndex < 0 || rowIndex >= table.Rows.Count || table.Rows[rowIndex].IsNewRow) return;

            string modelName = Convert.ToString(table.Rows[rowIndex].Cells[modelTable.Col.Name].Value);
            string modelFile = Convert.ToString(table.Rows[rowIndex].Cells[modelTable.Col.File].Value);
            DialogResult result = MessageBox.Show(
                "Delete model '" + modelName + "' from the list and its saved files?",
                "Delete Model", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            try
            {
                bool deleteActiveModel = _form1.ModelNoInternal == rowIndex;
                string fullModelFile = string.IsNullOrWhiteSpace(modelFile) ? "" : Path.GetFullPath(modelFile);
                string modelRoot = Path.GetFullPath(@"D:\InitialData\Laser Marking\Model");
                string modelFolder = Path.GetFullPath(Path.Combine(modelRoot, modelName ?? ""));
                string expectedModelFile = Path.GetFullPath(Path.Combine(modelFolder, (modelName ?? "") + ".txt"));
                bool isApplicationModelFolder = SamePath(Path.GetDirectoryName(modelFolder), modelRoot)
                    && SamePath(fullModelFile, expectedModelFile);

                if (isApplicationModelFolder)
                {
                    foreach (DataGridViewRow otherRow in table.Rows)
                    {
                        if (otherRow.Index == rowIndex || otherRow.IsNewRow) continue;
                        string otherModelFile = Convert.ToString(otherRow.Cells[modelTable.Col.File].Value);
                        if (!string.IsNullOrWhiteSpace(otherModelFile)
                            && IsPathWithinDirectory(otherModelFile, modelFolder))
                            throw new InvalidOperationException("Another model in the list uses files in this model folder. Delete was cancelled to protect it.");
                    }

                    if (Directory.Exists(modelFolder))
                        Directory.Delete(modelFolder, true);
                    else if (File.Exists(fullModelFile))
                        File.Delete(fullModelFile);
                }
                else if (!string.IsNullOrWhiteSpace(fullModelFile) && File.Exists(fullModelFile))
                {
                    File.Delete(fullModelFile);
                }

                table.Rows.RemoveAt(rowIndex);
                modelTable.RefreshValueTable();

                if (table.Rows.Count == 0)
                {
                    modelParameter = new ModelParameter { Cam = new CamPara[_form1.NumberOfCamera] };
                    for (int i = 0; i < modelParameter.Cam.Length; i++) modelParameter.Cam[i] = new CamPara();
                    _form1.ModelNoInternal = 0;
                    currentModelInfomation.ModelNo = 0;
                    currentModelInfomation.PathFileModel = "";
                    ShowAModelParameter(modelParameter);
                }
                else if (deleteActiveModel)
                {
                    int nextRow = Math.Min(rowIndex, table.Rows.Count - 1);
                    table.CurrentCell = table.Rows[nextRow].Cells[modelTable.Col.Name];
                    LoadAModel(nextRow);
                }
                else
                {
                    int activeRow = _form1.ModelNoInternal > rowIndex
                        ? _form1.ModelNoInternal - 1
                        : _form1.ModelNoInternal;
                    if (activeRow < 0 || activeRow >= table.Rows.Count) activeRow = 0;
                    _form1.ModelNoInternal = activeRow;
                    currentModelInfomation.ModelNo = activeRow;
                    currentModelInfomation.PathFileModel = Convert.ToString(table.Rows[activeRow].Cells[modelTable.Col.File].Value) ?? "";
                    table.CurrentCell = table.Rows[activeRow].Cells[modelTable.Col.Name];
                    SetCurrentModelRow(activeRow);
                }

                SaveCurrentModelInfomation(true);
                _form1.WriteLogPC(LogType.Main, "Model", "Deleted model '" + modelName + "' and its saved files.");
                MessageBox.Show("Model deleted successfully.", "Delete Model", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _form1.WriteLogPC(LogType.Main, "Delete Model", ex.ToString());
                MessageBox.Show(ex.ToString(), "Delete Model Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool IsPathWithinDirectory(string filePath, string directoryPath)
        {
            string fullDirectory = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string fullFile = Path.GetFullPath(filePath);
            return fullFile.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase);
        }

        #region EventHandle
        private void numbericEdit1_Click_1(object sender, EventArgs e)
        {
            NumbericEdit ThisnumbericEdit = sender as NumbericEdit;

            NumPad numPad = new NumPad();
            numPad.InitialUI(ThisnumbericEdit, this, _form1);
            numPad.Show();
        }
        private void btnModel_New_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to make new Model? Current Parameters will be clear", "New Model", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                currentModelInfomation.PathFileModel = "";
                modelParameter = new ModelParameter();
                laserSettingLayout.SetModelFolderPath("");
                ShowAModelParameter(modelParameter);
                
            }
            //LOG
            _form1.WriteLogPC(LogType.Main, "Model", "New Model is initial");
        }
        private void btnModel_Save_Click(object sender, EventArgs e)
        {
            if (SaveAModel()) SaveCurrentModelInfomation(true);
        }
        private DataTable GetDataGridViewAsDataTable(DataGridView _DataGridView)
        {
            try
            {
                if (_DataGridView.ColumnCount == 0) return null;
                DataTable dtSource = new DataTable();
                //////create columns
                foreach (DataGridViewColumn col in _DataGridView.Columns)
                {
                    if (col.ValueType == null) dtSource.Columns.Add(col.Name, typeof(string));
                    else dtSource.Columns.Add(col.Name, col.ValueType);
                    dtSource.Columns[col.Name].Caption = col.HeaderText;
                }
                ///////insert row data
                foreach (DataGridViewRow row in _DataGridView.Rows)
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
        public bool SaveAModel()
        {
            try
            {
                if (modelParameter == null) modelParameter = new ModelParameter();
                if (modelParameter.Cam == null) modelParameter.Cam = new CamPara[_form1.NumberOfCamera];
                if (modelParameter.Cam.Length != _form1.NumberOfCamera)
                {
                    CamPara[] temp = new CamPara[_form1.NumberOfCamera];
                    Array.Copy(modelParameter.Cam, temp, Math.Min(modelParameter.Cam.Length, temp.Length));
                    modelParameter.Cam = temp;
                }

                // Save only model data here. Do NOT initialize cameras, reload PRC files, rebuild MAIN,
                // or bind anything to VmMainViewConfigControl. Runtime is changed only by Call Model
                // or by explicitly saving the procedure that is currently open in the editor.
                for (int i = 0; i < _form1.NumberOfCamera; i++)
                {
                    if (modelParameter.Cam[i] == null) modelParameter.Cam[i] = new CamPara();
                    if (ListFrmVisionFileSetup[i] != null) modelParameter.Cam[i].VisionTable = ListFrmVisionFileSetup[i].GetDataTable();
                }
                if (modelParameter.Cam.Length > 0)
                    laserSettingLayout.ApplySurfaceParameters(modelParameter.Cam[0]);

                ModelColumns col = new ModelColumns();
                int modelNo = _form1.ModelNoInternal;
                if (modelNo < 0 || modelNo >= modelTable.Table.Rows.Count || modelTable.Table.Rows[modelNo].IsNewRow)
                    throw new InvalidOperationException("Current model row is invalid.");

                // The MAIN layout depends on the number/name of vision rows. Rebuild it only when
                // a model is actually called, never when the model is merely saved.
                if (_form1.formMain != null && _form1.formMain.IsUiInitialized)
                {
                    _form1.formMain.InitialCameraVisionDisplay();
                    UpdateMainDisplayFromModel();
                }

                modelParameter.ModelName = modelTable.Table.Rows[modelNo].Cells[col.Name].Value?.ToString() ?? "";
                string fileName = modelTable.Table.Rows[modelNo].Cells[col.File].Value?.ToString() ?? currentModelInfomation.PathFileModel;

                if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
                {
                    using (SaveFileDialog dlg = new SaveFileDialog())
                    {
                        dlg.Filter = "Model file (*.txt)|*.txt|All files (*.*)|*.*";
                        dlg.DefaultExt = "txt";
                        dlg.AddExtension = true;
                        dlg.FileName = string.IsNullOrWhiteSpace(modelParameter.ModelName) ? "Model.txt" : modelParameter.ModelName + ".txt";
                        if (dlg.ShowDialog() != DialogResult.OK) return false;
                        fileName = dlg.FileName;
                    }
                }

                string folder = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string json = JsonConvert.SerializeObject(modelParameter);
                File.WriteAllText(fileName, json);

                currentModelInfomation.ModelNo = modelNo;
                currentModelInfomation.PathFileModel = fileName;
                laserSettingLayout.SetModelFolderPath(folder);
                modelTable.Table.Rows[modelNo].Cells[col.Name].Value = modelParameter.ModelName;
                modelTable.Table.Rows[modelNo].Cells[col.File].Value = fileName;
                if (_form1.formMain != null) _form1.formMain.ModelName.Text = modelParameter.ModelName;

                _form1.WriteLogPC(LogType.Main, "Model", $"Model saved without reloading runtime. Model: {modelParameter.ModelName}, File: {fileName}");
                MessageBox.Show("Save Model Parameters is Successfull", "Save Model Parameter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Save Model Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void btnModel_Load_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.CheckFileExists = true;
                    dlg.CheckPathExists = true;
                    dlg.Filter = "Model file (*.txt)|*.txt|All files (*.*)|*.*";
                    if (dlg.ShowDialog() != DialogResult.OK) return;

                    int modelNo = modelTable.Table.CurrentRow != null && !modelTable.Table.CurrentRow.IsNewRow
                        ? modelTable.Table.CurrentRow.Index : _form1.ModelNoInternal;
                    if (modelNo < 0 || modelNo >= modelTable.Table.Rows.Count || modelTable.Table.Rows[modelNo].IsNewRow) return;

                    ModelColumns col = new ModelColumns();
                    modelTable.Table.Rows[modelNo].Cells[col.File].Value = dlg.FileName;
                    LoadAModel(modelNo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Load Model File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void LoadAModel(int ModelNo)
        {
            ModeMachine previousMode = _form1.MachineMode;
            _form1.MachineMode = ModeMachine.Manual;
            try { _form1.RunWithVisionRuntimeLock(() => LoadAModelCore(ModelNo)); }
            finally
            {
                _form1.MachineMode = previousMode;
                if (_form1.formMain != null && _form1.formMain.IsUiInitialized)
                    _form1.formMain.UpdateControlStatus();
            }
        }

        private void LoadAModelCore(int ModelNo)
        {
            try
            {
                ModelColumns col = new ModelColumns();
                if (ModelNo < 0 || ModelNo >= modelTable.Table.Rows.Count || modelTable.Table.Rows[ModelNo].IsNewRow) return;
                if (modelTable.Table.Rows[ModelNo].Cells[col.File].Value == null)
                {
                    MessageBox.Show("Current Model File is null");
                    return;
                }

                string fileName = modelTable.Table.Rows[ModelNo].Cells[col.File].Value.ToString();
                if (!File.Exists(fileName))
                {
                    MessageBox.Show("Model File is not exist!");
                    return;
                }

                // Close only the teaching editor before changing model. If it points to a runtime
                // procedure, do not dispose it here; InitialAllCameras owns the runtime lifecycle.
                CloseProcedureEditor();

                _form1.ModelNoInternal = ModelNo;
                currentModelInfomation.ModelNo = ModelNo;
                currentModelInfomation.PathFileModel = fileName;
                laserSettingLayout.SetModelFolderPath(Path.GetDirectoryName(fileName));

                ReadAModelParameter(fileName);
                ShowAModelParameter(modelParameter);

                // The MAIN layout depends on the number/name of vision rows. Rebuild it only when
                // a model is actually called, never when the model is merely saved.
                if (_form1.formMain != null && _form1.formMain.IsUiInitialized)
                {
                    _form1.formMain.InitialCameraVisionDisplay();
                    UpdateMainDisplayFromModel();
                }

                // Apply the called model to runtime. InitialAllCameras now reuses already-open
                // cameras and reloads only the PRC arrays, avoiding unnecessary reconnect cycles.
                _form1.InitialAllCameras();

                for (int i = 0; i < modelTable.Table.RowCount; i++)
                {
                    if (modelTable.Table.Rows[i].IsNewRow) continue;
                    modelTable.Table.Rows[i].DefaultCellStyle.BackColor = i == ModelNo ? System.Drawing.Color.Lime : System.Drawing.Color.White;
                }

                _form1.WriteLogPC(LogType.Main, "Model", $"Model called: {modelParameter.ModelName}. Runtime procedures reloaded from model file.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Load Model File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateMainDisplayFromModel()
        {
            if (_form1?.formMain?.CameraVisionDisplays == null) return;
            for (int cam = 0; cam < _form1.NumberOfCamera; cam++)
            {
                if (ListFrmVisionFileSetup[cam] == null || _form1.formMain.CameraVisionDisplays[cam] == null) continue;
                int count = Math.Min(ListFrmVisionFileSetup[cam].GetRowCount(), _form1.formMain.CameraVisionDisplays[cam].List.Count);
                for (int index = 0; index < count; index++)
                {
                    CameraVisionDisplay display = _form1.formMain.CameraVisionDisplays[cam].List[index];
                    if (display == null) continue;
                    display.Exposure.Text = ListFrmVisionFileSetup[cam].GetExposure(index).ToString();
                }
            }
        }
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            SaveCurrentModelInfomation(false);
        }
        #endregion

        #region Read and Save Parameter
        public void ReadModelToSetModelTable(DataTable dataTable)
        {
            modelTable.Table.Rows.Clear();
            if (dataTable == null) return;
            foreach (DataRow row in dataTable.Rows)
            {
                if (row.ItemArray[0] == null) return;
                string thisdata1 = row.ItemArray[0].ToString();
                if (thisdata1 == "") return;
                modelTable.Table.Rows.Add(row.ItemArray);
            }
        }
        private void ReadModelInfomation()
        {
            try
            {
                if (!File.Exists(PathFileParameter)) return;
                string text = File.ReadAllText(PathFileParameter);
                CurrentModelInfomation read = JsonConvert.DeserializeObject<CurrentModelInfomation>(text);
                if (read == null) return;
                currentModelInfomation = read;
                ShowFixModelInfomation(currentModelInfomation);
            }
            catch (Exception ex)
            {
                _form1?.WriteLogPC(LogType.Main, "Model", $"Read ModelInfomation failed: {ex.Message}");
            }
        }
        private void ShowFixModelInfomation(CurrentModelInfomation Model)
        {
            ReadModelToSetModelTable(Model?.ModelInfo);
        }
        public void SaveCurrentModelInfomation(bool silent)
        {
            try
            {
                currentModelInfomation.ModelInfo = GetDataGridViewAsDataTable(modelTable.Table);

                //string PathFileParameter = MyDirectory() + @"\ModelInfomation.txt";
                //PathFileParameter = "D:\\InitialData\\ModelInfomation.txt";
                string json = JsonConvert.SerializeObject(currentModelInfomation);
                File.WriteAllText(PathFileParameter, json);
                if (!silent)
                    MessageBox.Show("Save Current Model Infomation is Successfull", "Save Model Infomation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //LOG
                _form1.WriteLogPC(LogType.Main, "Model", "Current Model Information is Save");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "A Fix Parameter Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ReadAModelParameter(string Path)
        {
            try
            {
                if (!File.Exists(Path)) throw new Exception("Model File is not exist!");
                string json = File.ReadAllText(Path);
                ModelParameter read = JsonConvert.DeserializeObject<ModelParameter>(json);
                if (read == null) throw new Exception("Model file is invalid or empty.");
                modelParameter = read;
                currentModelInfomation.PathFileModel = Path;
                _form1.WriteLogPC(LogType.Main, "Model", $"Read OKE Model: {modelParameter.ModelName} From file: {Path}. OKE");
            }
            catch (Exception ex)
            {
                modelParameter = new ModelParameter();
                _form1.WriteLogPC(LogType.Main, "Model", $"Read Error Model From file: {Path}. Error: {ex.Message}");
                throw;
            }
        }
        public void ShowAModelParameter(ModelParameter Model)
        {
            if (Model == null) Model = new ModelParameter();
            if (Model.Cam == null) Model.Cam = new CamPara[_form1.NumberOfCamera];
            if (Model.Cam.Length != _form1.NumberOfCamera)
            {
                CamPara[] temp = new CamPara[_form1.NumberOfCamera];
                Array.Copy(Model.Cam, temp, Math.Min(Model.Cam.Length, temp.Length));
                Model.Cam = temp;
            }

            for (int i = 0; i < _form1.NumberOfCamera; i++)
            {
                if (Model.Cam[i] == null) Model.Cam[i] = new CamPara();
                if (ListFrmVisionFileSetup[i] != null) ListFrmVisionFileSetup[i].SetTable(Model.Cam[i].VisionTable);
            }

            modelParameter = Model;
            if (modelParameter.Cam.Length > 0)
                laserSettingLayout.LoadSurfaceParameters(modelParameter.Cam[0]);
            if (_form1.formMain != null)
            {
                _form1.formMain.ModelName.Text = Model.ModelName ?? "";
                UpdateMainDisplayFromModel();
            }
        }
        private void SaveAModelParameter(ModelParameter Model, string Path)
        {
            try
            {
                string json = JsonConvert.SerializeObject(Model);
                File.WriteAllText(Path, json);
                //LOG
                _form1.WriteLogPC(LogType.Main, "Model", $"Save Model: {Model.ModelName} To file: {Path}. OK");
                MessageBox.Show("Save Model is Successfull", "Save A Model", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //LOG
                _form1.WriteLogPC(LogType.Main, "Model", $"Save Model: {Model.ModelName} To file: {Path}. Fail");
                MessageBox.Show(ex.ToString(), "Read a model parameters Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    
        private string NormalizeVisionFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "";
            fileName = fileName.Trim();
            if (string.Equals(Path.GetExtension(fileName), ".vpp", StringComparison.OrdinalIgnoreCase))
            {
                string prcCandidate = Path.ChangeExtension(fileName, ".prc");
                if (File.Exists(prcCandidate)) fileName = prcCandidate;
            }
            return fileName;
        }

        private bool SamePath(string path1, string path2)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path1) || string.IsNullOrWhiteSpace(path2)) return false;
                return string.Equals(Path.GetFullPath(path1).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(path2).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return string.Equals(path1?.Trim(), path2?.Trim(), StringComparison.OrdinalIgnoreCase);
            }
        }

        public VisionMasterProcess LoadVisionMasterProcessFromFile(string fileName)
        {
            try
            {
                fileName = NormalizeVisionFileName(fileName);
                if (string.IsNullOrWhiteSpace(fileName)) return null;
                if (!File.Exists(fileName))
                {
                    _form1?.WriteLogPC(LogType.Vision, "LoadVisionMasterProcess", $"PRC file is not exist: {fileName}");
                    return null;
                }

                Cursor = Cursors.AppStarting;
                VisionMasterProcess process = VisionMasterProcess.Load(fileName);
                _form1?.WriteLogPC(LogType.Vision, "LoadVisionMasterProcess", $"Loaded PRC: {fileName}");
                return process;
            }
            catch (Exception ex)
            {
                _form1?.WriteLogPC(LogType.Vision, "LoadVisionMasterProcess", ex.ToString());
                return null;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void EnsureProcedureEditor()
        {
            if (vmProcedureConfigControl1 != null && !vmProcedureConfigControl1.IsDisposed) return;
            vmProcedureConfigControl1 = new VmMainViewConfigControl { Dock = DockStyle.Fill };
            panel3.Controls.Add(vmProcedureConfigControl1);
            vmProcedureConfigControl1.BringToFront();
        }

        private void DisposeProcedureEditorControl()
        {
            if (vmProcedureConfigControl1 == null) return;
            try { panel3.Controls.Remove(vmProcedureConfigControl1); } catch { }
            try { vmProcedureConfigControl1.Dispose(); } catch { }
            vmProcedureConfigControl1 = null;
        }

        private void ReleaseCurrentEditorProcess()
        {
            if (currentVisionProcessOwnedByEditor)
            {
                try { currentVisionProcess?.Dispose(); } catch { }
            }
            currentVisionProcess = null;
            currentVisionProcessOwnedByEditor = false;
            lbToolOpening.Text = "";
            for (int i = 0; i < ListFrmVisionFileSetup.Length; i++)
                if (ListFrmVisionFileSetup[i] != null) ListFrmVisionFileSetup[i].SaveEnable = false;
        }

        private void CloseProcedureEditor()
        {
            // Unbind/dispose the editor control before releasing an editor-owned procedure.
            // This prevents the VisionMaster UI control from retaining ModuleSource during Dispose.
            DisposeProcedureEditorControl();
            ReleaseCurrentEditorProcess();
        }

        public void CloseProcedureEditorForShutdown()
        {
            CloseProcedureEditor();
            if (laserSettingLayout != null)
            {
                laserSettingLayout.Hide();
                laserSettingLayout.Dispose();
                laserSettingLayout = null;
            }
        }

        private VisionMasterProcess GetRuntimeVisionProcess(int cam, int index, string fileName)
        {
            if (_form1?._VisionSystem == null || cam < 0 || cam >= _form1._VisionSystem.Length) return null;
            VisionSystem visionSystem = _form1._VisionSystem[cam];
            if (visionSystem?.VisionProcesses == null || index < 0 || index >= visionSystem.VisionProcesses.Length) return null;
            VisionMasterProcess process = visionSystem.VisionProcesses[index];
            if (process?.Procedure == null) return null;
            return SamePath(process.FilePath, fileName) ? process : null;
        }

        public void LoadVisionMasterProcessEditor(int cam, int index)
        {
            string fileName = NormalizeVisionFileName(ListFrmVisionFileSetup[cam].GetFileName(index));
            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            {
                MessageBox.Show($"PRC file is not exist:\n{fileName}", "Load VisionMaster", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Start with a fresh editor control so bindings from the previous procedure cannot
            // accumulate. This does not load or reload any runtime procedure.
            DisposeProcedureEditorControl();
            ReleaseCurrentEditorProcess();

            VisionMasterProcess runtimeProcess = GetRuntimeVisionProcess(cam, index, fileName);
            if (runtimeProcess != null)
            {
                currentVisionProcess = runtimeProcess;
                currentVisionProcessOwnedByEditor = false;
            }
            else
            {
                currentVisionProcess = LoadVisionMasterProcessFromFile(fileName);
                currentVisionProcessOwnedByEditor = currentVisionProcess != null;
            }

            if (currentVisionProcess == null || currentVisionProcess.Procedure == null) return;

            CamSelectedIndex = cam;
            RowSelectedIndex = index;
            lbToolOpening.Text = currentVisionProcess.FilePath;
            EnsureProcedureEditor();
            currentVisionProcess.EnableDisplayCallbacks();
            VisionMasterUiHelper.EnableAllModuleCallbacks();
            VisionMasterUiHelper.BindProcedureEditor(vmProcedureConfigControl1, currentVisionProcess.Procedure);
            _form1?.WriteLogPC(LogType.Vision, "VisionMaster Editor", $"Open Cam{cam + 1}, Index{index}: {currentVisionProcess.FilePath}. RuntimeInstance={!currentVisionProcessOwnedByEditor}");
        }

        private bool TryReplaceRuntimeProcess(int cam, int index, VisionMasterProcess process)
        {
            if (process == null || _form1?._VisionSystem == null || cam < 0 || cam >= _form1._VisionSystem.Length) return false;
            VisionSystem visionSystem = _form1._VisionSystem[cam];
            if (visionSystem?.VisionProcesses == null || index < 0 || index >= visionSystem.VisionProcesses.Length) return false;

            VisionMasterProcess old = visionSystem.VisionProcesses[index];
            if (!ReferenceEquals(old, process))
            {
                try { old?.Dispose(); } catch { }
                visionSystem.VisionProcesses[index] = process;
            }

            bool allLoaded = visionSystem.VisionProcesses.Length > 0;
            for (int i = 0; i < visionSystem.VisionProcesses.Length; i++)
                if (visionSystem.VisionProcesses[i]?.Procedure == null) { allLoaded = false; break; }
            visionSystem.IsVisionProcessInitial = allLoaded;
            return true;
        }

        public VisionMasterProcess LoadVisionMasterProcessIndex(int cam, int index)
        {
            if (cam < 0 || cam >= ListFrmVisionFileSetup.Length || ListFrmVisionFileSetup[cam] == null) return null;
            string fileName = NormalizeVisionFileName(ListFrmVisionFileSetup[cam].GetFileName(index));
            return LoadVisionMasterProcessFromFile(fileName);
        }
        #endregion
        private void txbRunTextFolder_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            System.Windows.Forms.FolderBrowserDialog fldDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (fldDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txbRunTextFolder.Text = fldDialog.SelectedPath;
            }
        }
        private void txbRunTextFolder_TextChanged(object sender, EventArgs e)
        {
            try
            {
                DirectorImageFolder = txbRunTextFolder.Text;
                if (DirectorImageFolder == "") return;
                if (!Directory.Exists(DirectorImageFolder)) return;
                FolderImageFiles = Directory.GetFiles(@DirectorImageFolder);
                ImageFolderCount = 0;
                ImageFolderOrFile = false;
            }
            catch (IOException ex)
            { 
                
            }
          
        }
        private void txbRunTestFile_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txbRunTestFile.Text = dlg.FileName;
            }
        }
        private void txbRunTestFile_TextChanged(object sender, EventArgs e)
        {
            FileImageOpening = txbRunTestFile.Text;
            IsImageFileToolRunOnce = false;
        }
        private void btnRunOnce_Click(object sender, EventArgs e)
        {
            if (radioUseCam.Checked)
            {
                _form1.MachineMode = ModeMachine.Teaching;
                _form1.formMain.UpdateControlStatus();
                _form1._VisionSystem[CamSelectedIndex].TriggerOne = true;
            }
            else
            {
                using (Bitmap image = GetImage())
                {
                    RunOneVisionMasterProcessEdit(image);
                }
            }
        }

        public void RunOneVisionMasterProcessEdit(Bitmap image)
        {
            try
            {
                if (image == null || currentVisionProcess == null) return;

                // The embedded VisionMaster editor needs module result callbacks enabled
                // in order to refresh image/result/graphics after an SDK-triggered run.
                currentVisionProcess.EnableDisplayCallbacks();
                VisionMasterUiHelper.EnableAllModuleCallbacks();

                int result = currentVisionProcess.Run(image);

                if (!string.IsNullOrWhiteSpace(currentVisionProcess.LastError))
                    _form1?.WriteLogPC(LogType.Vision, "VisionMaster Teaching", currentVisionProcess.LastError);

                // Force the embedded VisionMaster main-view to repaint after the run.
                // Do not reload the PRC here; it must stay bound to the same procedure instance.
                VisionMasterUiHelper.RefreshProcedureEditor(vmProcedureConfigControl1);
                _form1?.WriteLogPC(LogType.Vision, "VisionMaster Teaching", $"Run Done. Result={result}");
            }
            catch (Exception ex)
            {
                _form1?.WriteLogPC(LogType.Vision, "VisionMaster Teaching", ex.ToString());
            }
        }

        public Bitmap GetImage()
        {
            try
            {
                if (currentVisionProcess == null)
                {
                    MessageBox.Show("Please load VisionMaster PRC file first!");
                    return null;
                }

                // When camera mode is selected, Camera_ImageReceived supplies the image asynchronously.
                if (radioUseCam.Checked) return null;

                if (radioUseImageSetting.Checked) return null;

                if (radioUseFolder.Checked)
                {
                    if (FolderImageFiles == null || FolderImageFiles.Length == 0) return null;
                    if (ImageFolderCount >= FolderImageFiles.Length) ImageFolderCount = 0;

                    string fileName = FolderImageFiles[ImageFolderCount];
                    lbFolderImageIndex.Text = ImageFolderCount.ToString();
                    ImageFolderCount++;
                    return VisionMasterUiHelper.LoadBitmapUnlocked(fileName);
                }

                if (radioFile.Checked)
                {
                    if (string.IsNullOrWhiteSpace(FileImageOpening)) return null;
                    return VisionMasterUiHelper.LoadBitmapUnlocked(FileImageOpening);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return null;
        }

        private void btnRefreshFolderImage_Click(object sender, EventArgs e)
        {
            ImageFolderCount = 0;
        }
        public string ConvertIntToStringNumberic(int Value, int RighOfDecimal)
        {
            string Result = "0";
            string TempValue = Value.ToString();

            switch (RighOfDecimal)
            {
                case 0:
                    {
                        Result = Value.ToString();
                        break;
                    }
                case 1:
                    {
                        if (!TempValue.Contains("-"))
                        {

                            if (TempValue.Length == 1)
                            {
                                TempValue = "0" + TempValue;
                            }
                        }
                        else
                        {

                            if (TempValue.Length == 1)
                            {
                                TempValue = TempValue.Replace("-", "");
                                TempValue = "-0" + TempValue;
                            }
                        }
                        string LeftValue = TempValue.Substring(0, TempValue.Length - 1);
                        string RightValue = TempValue.Substring(TempValue.Length - 1);
                        Result = LeftValue + "." + RightValue;
                        break;
                    }
                case 2:
                    {
                        if (!TempValue.Contains("-"))
                        {
                            if (TempValue.Length == 1)
                            {
                                TempValue = "00" + TempValue;
                            }
                            else
                            {
                                if (TempValue.Length == 2)
                                {
                                    TempValue = "0" + TempValue;
                                }
                            }
                        }
                        else
                        {

                            if (TempValue.Length == 2)
                            {
                                TempValue = TempValue.Replace("-", "");
                                TempValue = "-00" + TempValue;
                            }
                            else
                            {
                                if (TempValue.Length == 3)
                                {
                                    TempValue = TempValue.Replace("-", "");
                                    TempValue = "-0" + TempValue;
                                }
                            }
                        }
                        string LeftValue = TempValue.Substring(0, TempValue.Length - 2);
                        string RightValue = TempValue.Substring(TempValue.Length - 2);
                        Result = LeftValue + "." + RightValue;
                        break;
                    }
                case 3:
                    {
                        if (!TempValue.Contains("-"))
                        {
                            if (TempValue.Length == 1)
                            {
                                TempValue = "000" + TempValue;
                            }
                            else
                            {
                                if (TempValue.Length == 2)
                                {
                                    TempValue = "00" + TempValue;
                                }
                                else
                                {
                                    if (TempValue.Length == 3)
                                    {
                                        TempValue = "0" + TempValue;
                                    }
                                }
                            }
                        }
                        else
                        {

                            if (TempValue.Length == 2)
                            {
                                TempValue = TempValue.Replace("-", "");
                                TempValue = "-000" + TempValue;
                            }
                            else
                            {
                                if (TempValue.Length == 3)
                                {
                                    TempValue = TempValue.Replace("-", "");
                                    TempValue = "-00" + TempValue;
                                }
                                else
                                {
                                    if (TempValue.Length == 4)
                                    {
                                        TempValue = TempValue.Replace("-", "");
                                        TempValue = "-0" + TempValue;
                                    }
                                }
                            }
                        }

                        string LeftValue = TempValue.Substring(0, TempValue.Length - 3);
                        string RightValue = TempValue.Substring(TempValue.Length - 3);
                        Result = LeftValue + "." + RightValue;
                        break;
                    }
            }
            return Result;

        }
        private void Model_Load(object sender, EventArgs e)
        {
            users = UserManager.LoadUsers();
        }
       
    }
}
