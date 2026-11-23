using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using QRCoder;

namespace SLTtechSoft
{
    public partial class LaserSettingLayout : Form
    {
        public event EventHandler SaveAllRequested;

        private enum LaserSurface
        {
            Top,
            Bot
        }

        private sealed class QrMarkingLabel : Label
        {
            public Bitmap QrImage { get; set; }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(BackColor);
                if (QrImage != null)
                    e.Graphics.DrawImage(QrImage, ClientRectangle);
                else
                    TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                if (BackColor == Color.LimeGreen)
                {
                    using (Pen selectionPen = new Pen(Color.LimeGreen, 3f))
                        e.Graphics.DrawRectangle(selectionPen, 1, 1, ClientSize.Width - 3, ClientSize.Height - 3);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && QrImage != null)
                {
                    QrImage.Dispose();
                    QrImage = null;
                }
                base.Dispose(disposing);
            }
        }

        private sealed class FiducialLabel : Label
        {
            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.Red, 2f))
                using (Brush brush = new SolidBrush(Color.Red))
                using (StringFormat format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                {
                    int centerY = ClientSize.Height / 2;
                    e.Graphics.DrawLine(pen, 0, centerY, 14, centerY);
                    e.Graphics.DrawLine(pen, 7, centerY - 7, 7, centerY + 7);
                    e.Graphics.DrawString(Text, Font, brush, new RectangleF(17, 0, ClientSize.Width - 17, ClientSize.Height), format);
                }
            }
        }

        private static float ParseResolution(string text, float fallback)
        {
            float value;
            return float.TryParse(text, out value) && value > 0 ? value : fallback;
        }

        private static string NormalizeMarkingType(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return "CODE";
            string value = type.Replace(" ", "").Trim();
            if (string.Equals(value, "CODE", StringComparison.OrdinalIgnoreCase)) return "CODE";
            if (string.Equals(value, "RIGHTTEXT", StringComparison.OrdinalIgnoreCase)) return "RIGHTTEXT";
            if (string.Equals(value, "LEFTTEXT", StringComparison.OrdinalIgnoreCase)) return "LEFTTEXT";
            if (string.Equals(value, "TOPTEXT", StringComparison.OrdinalIgnoreCase)) return "TOPTEXT";
            if (string.Equals(value, "BOTTOMTEXT", StringComparison.OrdinalIgnoreCase)) return "BOTTOMTEXT";
            return type.Trim();
        }

        private static float GetMarkingRotation(string type)
        {
            switch (NormalizeMarkingType(type))
            {
                case "TOPTEXT": return 90f;
                case "BOTTOMTEXT": return 270f;
                default: return 0f;
            }
        }

        private sealed class FiducialItem
        {
            public int Id;
            public PointF Position;
            public Label View;
        }

        private sealed class DirectionalMarkingLabel : Label
        {
            public float Rotation { get; set; }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (Rotation == 0)
                {
                    base.OnPaint(e);
                    return;
                }

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.Clear(BackColor);
                e.Graphics.TranslateTransform(ClientSize.Width / 2f, ClientSize.Height / 2f);
                e.Graphics.RotateTransform(Rotation);
                e.Graphics.TranslateTransform(-ClientSize.Height / 2f, -ClientSize.Width / 2f);
                using (Brush brush = new SolidBrush(BackColor))
                using (Pen pen = new Pen(ForeColor))
                using (Brush textBrush = new SolidBrush(ForeColor))
                using (StringFormat format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                {
                    RectangleF textBounds = new RectangleF(0, 0, ClientSize.Height, ClientSize.Width);
                    e.Graphics.FillRectangle(brush, textBounds);
                    e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Height - 1, ClientSize.Width - 1);
                    e.Graphics.DrawString(Text, Font, textBrush, textBounds, format);
                }
                e.Graphics.ResetTransform();
            }
        }

        public sealed class MarkingItem
        {
            public int Id;
            public string Product;
            public string Type;
            public string PrintType;
            public string Content;
            public PointF Position;
            public Label View;
            public string Rotate;
        }

        public  List<MarkingItem> markings = new List<MarkingItem>();
        private readonly List<MarkingItem> selectedMarkings = new List<MarkingItem>();
        private readonly List<FiducialItem> fiducials = new List<FiducialItem>();
        public MarkingItem selectedMarking;
        private FiducialItem selectedFiducial;
        private int nextMarkingId;
        private int nextFiducialId;
        private float imageZoom = 1f;
        private Point imagePan;
        private Point panStart;
        private bool panning;
        private bool panMode;
        private bool selectingMarkings;
        private bool addingFiducial;
        private bool updatingMarkingSelection;
        private Point selectionStart;
        private Point selectionEnd;
        private bool updatingPositionControls;
        private LaserSurface currentSurface = LaserSurface.Top;
        private LaserSurfaceParameter topParameters = new LaserSurfaceParameter();
        private LaserSurfaceParameter botParameters = new LaserSurfaceParameter();
        private LaserSurfaceParameter savedTopParameters;
        private LaserSurfaceParameter savedBotParameters;
        private string topImageFileName = "";
        private string botImageFileName = "";
        private readonly List<MarkingItem> copiedMarkings = new List<MarkingItem>();
        private const float MarkingFontSize = 9.75f;

        public LaserSettingLayout()
        {
            InitializeComponent();
            KeyPreview = true;
            ConfigureImageViewport();
            InitializeMarkingList();
            WireEvents();
            ConfigurePanMenu();
            positionX.Maximum = 1000000;
            positionY.Maximum = 1000000;
            resolutionValue.TextChanged += ResolutionValueTextChanged;
        }

        private void ConfigurePanMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem normalItem = new ToolStripMenuItem("Normal");
            ToolStripMenuItem panItem = new ToolStripMenuItem("Pan");
            normalItem.Click += (s, e) => SetPanMode(false);
            panItem.Click += (s, e) => SetPanMode(true);
            menu.Items.Add(normalItem);
            menu.Items.Add(panItem);
            markingImage.ContextMenuStrip = menu;
            imageViewport.ContextMenuStrip = menu;
            SetPanMode(false);
        }

        private void SetPanMode(bool enabled)
        {
            panMode = enabled;
            Cursor = enabled ? Cursors.Hand : Cursors.Default;
            markingImage.Cursor = enabled ? Cursors.Hand : Cursors.Default;
            imageViewport.Cursor = enabled ? Cursors.Hand : Cursors.Default;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C))
            {
                CopySelectedMarking();
                return true;
            }

            if (keyData == (Keys.Control | Keys.V))
            {
                PasteCopiedMarking();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void ReleaseResources()
        {
            foreach (FiducialItem item in fiducials)
                if (item.View != null) markingImage.Controls.Remove(item.View);
            foreach (MarkingItem item in markings)
                if (item.View != null) imageViewport.Controls.Remove(item.View);
            markings.Clear();
            selectedMarkings.Clear();
            fiducials.Clear();

            Image oldImage = markingImage.Image;
            markingImage.Image = null;
            if (oldImage != null) oldImage.Dispose();
        }

        private void ConfigureImageViewport()
        {
            //imageViewport.AutoScroll = true;
            imageViewport.Dock = DockStyle.Fill;
            imageViewport.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            markingImage.Dock = DockStyle.None;
            markingImage.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            markingImage.AutoSize = false;
            markingImage.Location = Point.Empty;
            imageViewport.Controls.SetChildIndex(markingImage, 0);
        }

        public void LoadImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !System.IO.File.Exists(fileName))
                return;

            if (currentSurface == LaserSurface.Top) topImageFileName = fileName;
            else botImageFileName = fileName;

            Image oldImage = markingImage.Image;
            markingImage.Image = Image.FromFile(fileName);
            if (oldImage != null)
                oldImage.Dispose();

            imagePan = Point.Empty;
            FitImageClick(this, EventArgs.Empty);
        }

        private void WireEvents()
        {
            Button[] tools = { toolQr, toolRightText, toolLeftText, toolTopText, toolBottomText };
            foreach (Button tool in tools)
            {
                tool.MouseDown += ToolMouseDown;
                tool.AllowDrop = false;
            }

            markingImage.AllowDrop = true;
            imageViewport.AllowDrop = true;
            markingImage.DragEnter += MarkingImageDragEnter;
            markingImage.DragDrop += MarkingImageDragDrop;
            imageViewport.DragEnter += MarkingImageDragEnter;
            imageViewport.DragDrop += MarkingImageDragDrop;
            markingImage.MouseWheel += MarkingImageMouseWheel;
            imageViewport.MouseWheel += MarkingImageMouseWheel;
            markingImage.TabStop = true;
            imageViewport.TabStop = true;
            markingImage.MouseDown += PanMouseDown;
            markingImage.MouseMove += PanMouseMove;
            markingImage.MouseUp += PanMouseUp;
            markingImage.MouseDown += AddFiducialAtMouseDown;
            imageViewport.MouseDown += PanMouseDown;
            imageViewport.MouseMove += PanMouseMove;
            imageViewport.MouseUp += PanMouseUp;
            imageViewport.MouseDown += AddFiducialAtMouseDown;
            imageViewport.Resize += ImageViewportResize;
            markingList.SelectionChanged += MarkingListSelectionChanged;
            markingList.MultiSelect = true;
            markingList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            markingList.DefaultCellStyle.SelectionBackColor = Color.LightGreen;
            markingList.DefaultCellStyle.SelectionForeColor = Color.Black;
            markingList.RowsDefaultCellStyle.BackColor = Color.LightGray;
            positionX.ValueChanged += PositionValueChanged;
            positionY.ValueChanged += PositionValueChanged;
            imagePixelToMmValue.TextChanged += ResolutionValueTextChanged;
            //printType.SelectedIndexChanged += PrintTypeChanged;
            markingContent.TextChanged += MarkingContentChanged;
            detailsButton.Click += DetailsButtonClick;
            imageViewport.Paint += ImageViewportPaint;
            markingImage.Paint += MarkingImagePaint;
            markingImage.MouseDown += SelectionMouseDown;
            markingImage.MouseMove += SelectionMouseMove;
            markingImage.MouseUp += SelectionMouseUp;
            imageViewport.MouseDown += SelectionMouseDown;
            imageViewport.MouseMove += SelectionMouseMove;
            imageViewport.MouseUp += SelectionMouseUp;
            moveUp.Click += MoveUpClick;
            moveDown.Click += MoveDownClick;
            deleteMarking.Click += DeleteMarkingClick;
            deleteAllMarking.Click += DeleteAllMarkingClick;
            alignLeft.Click += (s, e) => AlignSelected(0);
            alignCenter.Click += (s, e) => AlignSelected(1);
            alignRight.Click += (s, e) => AlignSelected(2);
            alignTop.Click += (s, e) => AlignSelected(3);
            alignMiddle.Click += (s, e) => AlignSelected(4);
            alignBottom.Click += (s, e) => AlignSelected(5);
            distributeHorizontal.Click += (s, e) => DistributeSelected(true);
            distributeVertical.Click += (s, e) => DistributeSelected(false);
            fitImage.Click += FitImageClick;
            zoomIn.Click += ZoomInClick;
            zoomOut.Click += ZoomOutClick;
            btnloadimage.Click += LoadImageClick;
            addFiducial.Click += AddFiducialClick;
            deleteFiducial.Click += DeleteFiducialClick;
            deleteAllFiducial.Click += (s, e) => DeleteAllFiducials();
            btnTop.Click += (s, e) => SwitchSurface(LaserSurface.Top);
            btnBot.Click += (s, e) => SwitchSurface(LaserSurface.Bot);
            saveOne.Click += SaveOneClick;
            saveAll.Click += SaveAllClick;
            discard.Click += DiscardClick;
        }

        public void LoadSurfaceParameters(CamPara camera)
        {
            topImageFileName = camera == null ? "" : GetTopImageFile(camera);
            botImageFileName = camera == null ? "" : camera.LaserBotImageFile;
            topParameters = CloneSurface(camera == null ? null : camera.Top);
            botParameters = CloneSurface(camera == null ? null : camera.Bot);
            savedTopParameters = CloneSurface(topParameters);
            savedBotParameters = CloneSurface(botParameters);
            currentSurface = LaserSurface.Top;
            btnTop.BackColor = Color.LightGreen;
            btnBot.BackColor = Color.WhiteSmoke;
            LoadSurfaceImage(topImageFileName);
            LoadCurrentSurfaceToControls();
            FitImageClick(this, EventArgs.Empty);
        }

        public void ApplySurfaceParameters(CamPara camera)
        {
            if (camera == null) return;
            SaveCurrentSurfaceToMemory();
            camera.Top = CloneSurface(topParameters);
            camera.Bot = CloneSurface(botParameters);
            camera.LaserTopImageFile = topImageFileName;
            camera.LaserBotImageFile = botImageFileName;
            camera.LaserImageFile = topImageFileName;
            camera.ResolutionImage = currentSurface == LaserSurface.Top ? topParameters.ResolutionImage : botParameters.ResolutionImage;
        }

        private static LaserSurfaceParameter CloneSurface(LaserSurfaceParameter source)
        {
            LaserSurfaceParameter target = new LaserSurfaceParameter();
            if (source == null) return target;
            target.ResolutionImage = source.ResolutionImage;
            foreach (LaserMarkingParameter marking in source.Markings ?? new List<LaserMarkingParameter>())
                target.Markings.Add(new LaserMarkingParameter { Id = marking.Id, Product = marking.Product, Type = marking.Type, PrintType = marking.PrintType, Content = marking.Content, PositionX = marking.PositionX, PositionY = marking.PositionY });
            foreach (LaserFiducialParameter point in source.Fiducials ?? new List<LaserFiducialParameter>())
                target.Fiducials.Add(new LaserFiducialParameter { Id = point.Id, PositionX = point.PositionX, PositionY = point.PositionY });
            target.ImagePixelToMM = source.ImagePixelToMM > 0 ? source.ImagePixelToMM : source.ResolutionImage;
            target.ResolutionImageRobot = source.ResolutionImageRobot > 0 ? source.ResolutionImageRobot : source.ResolutionImage;
            return target;
        }

        private LaserSurfaceParameter CurrentParameters
        {
            get { return currentSurface == LaserSurface.Top ? topParameters : botParameters; }
        }
        //Lấy ảnh
        private void SwitchSurface(LaserSurface surface)
        {
            SaveCurrentSurfaceToMemory();
            currentSurface = surface;
            btnTop.BackColor = surface == LaserSurface.Top ? Color.LightGreen : Color.WhiteSmoke;
            btnBot.BackColor = surface == LaserSurface.Bot ? Color.LightGreen : Color.WhiteSmoke;
            LoadSurfaceImage(surface == LaserSurface.Top ? topImageFileName : botImageFileName);
            LoadCurrentSurfaceToControls();
        }

        private static string GetTopImageFile(CamPara camera)
        {
            if (!string.IsNullOrWhiteSpace(camera.LaserTopImageFile)) return camera.LaserTopImageFile;
            return camera.LaserImageFile;
        }

        private void LoadSurfaceImage(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) && System.IO.File.Exists(fileName))
                LoadImage(fileName);
            else
            {
                Image oldImage = markingImage.Image;
                markingImage.Image = null;
                if (oldImage != null) oldImage.Dispose();
                FitImageClick(this, EventArgs.Empty);
            }
        }
        //load&Save
        private void SaveCurrentSurfaceToMemory()
        {
            if (currentSurface == LaserSurface.Top) topParameters = CaptureCurrentSurface();
            else botParameters = CaptureCurrentSurface();
        }

        private LaserSurfaceParameter CaptureCurrentSurface()
        {
            LaserSurfaceParameter result = new LaserSurfaceParameter
            {
                ResolutionImage = ParseResolution(resolutionValue.Text, 1f),
                ImagePixelToMM = ParseResolution(imagePixelToMmValue.Text, 1f),
                ResolutionImageRobot = ParseResolution(robotResolutionValue.Text, 1f)
            };
            foreach (MarkingItem item in markings)
                result.Markings.Add(new LaserMarkingParameter { Id = item.Id, Product = item.Product, Type = item.Type, PrintType = item.PrintType, Content = item.Content, PositionX = item.Position.X, PositionY = item.Position.Y, Rotate = item.Rotate });
            foreach (FiducialItem point in fiducials)
                result.Fiducials.Add(new LaserFiducialParameter { Id = point.Id, PositionX = point.Position.X, PositionY = point.Position.Y });
            return result;
        }

        private void LoadCurrentSurfaceToControls()
        {
            DeleteAllMarkingClick(this, EventArgs.Empty);
            DeleteAllFiducials();
            resolutionValue.Text = CurrentParameters.ResolutionImage.ToString("0.########");
            imagePixelToMmValue.Text = CurrentParameters.ImagePixelToMM.ToString("0.########");
            robotResolutionValue.Text = CurrentParameters.ResolutionImageRobot.ToString("0.########");
            foreach (LaserMarkingParameter marking in CurrentParameters.Markings ?? new List<LaserMarkingParameter>())
                AddMarking(marking.Type, new PointF((float)marking.PositionX, (float)marking.PositionY), marking.Id, marking.Product, marking.PrintType, marking.Content,marking.Rotate);
            NormalizeMarkingOrder();
            RebuildMarkingList();
            foreach (LaserFiducialParameter point in CurrentParameters.Fiducials ?? new List<LaserFiducialParameter>())
                AddFiducial(new PointF((float)point.PositionX, (float)point.PositionY), point.Id);
        }

        private void SaveOneClick(object sender, EventArgs e)
        {
            SaveCurrentSurfaceToMemory();
            if (currentSurface == LaserSurface.Top) savedTopParameters = CloneSurface(topParameters);
            else savedBotParameters = CloneSurface(botParameters);
        }

        private void SaveAllClick(object sender, EventArgs e)
        {
            SaveCurrentSurfaceToMemory();
            savedTopParameters = CloneSurface(topParameters);
            savedBotParameters = CloneSurface(botParameters);
            SaveAllRequested?.Invoke(this, EventArgs.Empty);
        }

        private void DiscardClick(object sender, EventArgs e)
        {
            if (currentSurface == LaserSurface.Top) topParameters = CloneSurface(savedTopParameters);
            else botParameters = CloneSurface(savedBotParameters);
            LoadCurrentSurfaceToControls();
        }

        private void LoadImageClick(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Load Marking Image";
                dialog.Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff|All files|*.*";
                dialog.Multiselect = false;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    LoadImage(dialog.FileName);
            }
        }

        private void ToolMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ((Button)sender).DoDragDrop(((Button)sender).Text, DragDropEffects.Copy);
        }

        private void MarkingImageDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(string)) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void MarkingImageDragDrop(object sender, DragEventArgs e)
        {
            string type = e.Data.GetData(typeof(string)) as string;
            if (string.IsNullOrWhiteSpace(type)) return;

            Point clientPoint = imageViewport.PointToClient(new Point(e.X, e.Y));
            PointF imagePoint;
            if (!TryGetImagePoint(clientPoint, out imagePoint)) return;
            string Rotate =   GetMarkingRotation(type).ToString().Replace("f","");
            AddMarking(type, imagePoint, Rotate);
        }

        private bool TryGetImagePoint(Point viewportPoint, out PointF imagePoint)
        {
            imagePoint = PointF.Empty;
            if (markingImage.Image == null || markingImage.Width <= 0 || markingImage.Height <= 0) return false;
            Point point = markingImage.PointToClient(imageViewport.PointToScreen(viewportPoint));
            if (point.X < 0 || point.Y < 0 || point.X > markingImage.Width || point.Y > markingImage.Height) return false;
            imagePoint = new PointF(point.X / imageZoom, point.Y / imageZoom);
            imagePoint.X = Math.Max(0, Math.Min(markingImage.Image.Width, imagePoint.X));
            imagePoint.Y = Math.Max(0, Math.Min(markingImage.Image.Height, imagePoint.Y));
            return true;
        }
        //Thêm Label laser

        private void AddMarking(string type, PointF position,string Rotate)
        {
            string normalizedType = NormalizeMarkingType(type);
            AddMarking(normalizedType, position, nextMarkingId++, "0", normalizedType, "", Rotate);
        }

        private void AddMarking(string type, PointF position, int id, string product, string printType, string content, string Rotate)
        {
            type = NormalizeMarkingType(type);
            printType = NormalizeMarkingType(string.IsNullOrWhiteSpace(printType) ? type : printType);
            MarkingItem item = new MarkingItem
            {
                Id = id,
                Product = product ?? "0",
                Type = type,
                PrintType = printType,
                Content = content ?? "",
                Position = position,
                Rotate = Rotate
            };
            nextMarkingId = Math.Max(nextMarkingId, id + 1);
            markings.Add(item);
            int row = markingList.Rows.Add(GetMarkingNumber(item),item.Product ,item.Type, FormatCoordinate(ToMillimeter(position.X)), FormatCoordinate(ToMillimeter(position.Y)));
            markingList.Rows[row].Tag = item;
            CreateMarkingView(item);
            SelectMarking(item);
        }

        private void CreateMarkingView(MarkingItem item)
        {
            Label view = item.Type == "CODE"
                ? (Label)new QrMarkingLabel()
                : new DirectionalMarkingLabel();
            item.View = view;
            item.View.AutoSize = false;
           
            item.View.Font = new Font("Times New Roman", MarkingFontSize, FontStyle.Regular);
            item.View.Text = string.IsNullOrWhiteSpace(item.Content) ? item.Type : item.Content;
            item.View.TextAlign = ContentAlignment.MiddleLeft;
            item.View.ForeColor = Color.White;
            item.View.BackColor = Color.Violet;
            item.View.BorderStyle = BorderStyle.None;
            item.View.Tag = item;
            item.View.Cursor = Cursors.Hand;
            item.View.ContextMenuStrip = markingImage.ContextMenuStrip;
            if (item.View is DirectionalMarkingLabel directionalView)
            {
                directionalView.Rotation = GetMarkingRotation(item.Type);
            }
            UpdateQrImage(item);
            item.View.MouseDown += MarkingViewMouseDown;
            item.View.MouseWheel += MarkingImageMouseWheel;
            item.View.MouseDown += PanMouseDown;
            item.View.MouseMove += PanMouseMove;
            item.View.MouseUp += PanMouseUp;
            imageViewport.Controls.Add(item.View);
            item.View.BringToFront();
            UpdateMarkingView(item);
        }

        private void PanMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Middle) return;
            if (e.Button == MouseButtons.Left && !panMode) return;
            panning = true;
            panStart = Control.MousePosition;
            ((Control)sender).Capture = true;
            ((Control)sender).Cursor = Cursors.SizeAll;
        }

        private void PanMouseMove(object sender, MouseEventArgs e)
        {
            bool leftPan = panMode && (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;
            bool middlePan = (Control.MouseButtons & MouseButtons.Middle) == MouseButtons.Middle;
            if (!panning || !leftPan && !middlePan) return;

            Point current = Control.MousePosition;
            int deltaX = current.X - panStart.X;
            int deltaY = current.Y - panStart.Y;
            if (deltaX == 0 && deltaY == 0) return;

            Point scroll = imageViewport.AutoScrollPosition;
           imageViewport.AutoScrollPosition = new Point(-scroll.X - deltaX, -scroll.Y - deltaY);
            panStart = current;
        }

        private void PanMouseUp(object sender, MouseEventArgs e)
        {
            if (!panning || (e.Button != MouseButtons.Left && e.Button != MouseButtons.Middle)) return;
            panning = false;
            ((Control)sender).Capture = false;
            ((Control)sender).Cursor = panMode ? Cursors.Hand : Cursors.Default;
        }

        private void MarkingViewMouseDown(object sender, MouseEventArgs e)
        {
            if (panMode) return;
            MarkingItem item = ((Control)sender).Tag as MarkingItem;
            if (item == null) return;

            if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                if (selectedMarkings.Contains(item))
                    selectedMarkings.Remove(item);
                else
                    selectedMarkings.Add(item);

                selectedMarking = selectedMarkings.Count > 0 ? item : null;
                UpdateMarkingSelectionVisuals();
                return;
            }

            SelectMarking(item);
            if (e.Button != MouseButtons.Left) return;
            Point start = item.View.PointToScreen(e.Location);
            Dictionary<MarkingItem, PointF> originals = new Dictionary<MarkingItem, PointF>();
            foreach (MarkingItem selected in selectedMarkings)
                originals[selected] = selected.Position;
            if (!originals.ContainsKey(item))
                originals[item] = item.Position;
            item.View.Capture = true;
            item.View.MouseMove += MoveMarkingView;
            item.View.MouseUp += StopMoveMarkingView;
            item.View.Tag = new object[] { item, start, originals };
        }

        private void MoveMarkingView(object sender, MouseEventArgs e)
        {
            Label view = (Label)sender;
            object[] state = view.Tag as object[];
            if (state == null || e.Button == MouseButtons.None) return;
            Point start = (Point)state[1];
            Dictionary<MarkingItem, PointF> originals = (Dictionary<MarkingItem, PointF>)state[2];
            Point current = view.PointToScreen(e.Location);
            float deltaX = (current.X - start.X) / imageZoom;
            float deltaY = (current.Y - start.Y) / imageZoom;
            foreach (KeyValuePair<MarkingItem, PointF> original in originals)
            {
                MarkingItem item = original.Key;
                item.Position = new PointF(original.Value.X + deltaX, original.Value.Y + deltaY);
                ClampPosition(item);
                UpdateMarkingView(item);
                UpdateMarkingRow(item);
            }
            UpdatePositionControls(selectedMarking);
        }

        private void StopMoveMarkingView(object sender, MouseEventArgs e)
        {
            Label view = (Label)sender;
            view.Capture = false;
            view.MouseMove -= MoveMarkingView;
            view.MouseUp -= StopMoveMarkingView;
            view.Tag = view.Tag is object[] ? ((object[])view.Tag)[0] : view.Tag;
        }
        //SelectMarking
        private void SelectMarking(MarkingItem item)
        {
            selectedMarking = item;
            if (item != null && !selectedMarkings.Contains(item))
            {
                selectedMarkings.Clear();
                selectedMarkings.Add(item);
            }
            UpdateMarkingSelectionVisuals();
        }

        private void CopySelectedMarking()
        {
            copiedMarkings.Clear();
            List<MarkingItem> source = selectedMarkings.Count > 0
                ? new List<MarkingItem>(selectedMarkings)
                : selectedMarking == null
                    ? new List<MarkingItem>()
                    : new List<MarkingItem> { selectedMarking };

            foreach (MarkingItem item in source)
            {
                //item.Rotate = GetMarkingRotation(item.Type).ToString().Replace("f", "");
                copiedMarkings.Add(new MarkingItem
                {
                    Product = item.Product,
                    Type = item.Type,
                    PrintType = item.PrintType,
                    Content = item.Content,
                    Position = item.Position,
                    Rotate = item.Rotate,
                });
            }
        }

        private void PasteCopiedMarking()
        {
            if (copiedMarkings.Count == 0) return;

            float offset = 20f / Math.Max(0.1f, imageZoom);
            List<MarkingItem> pastedMarkings = new List<MarkingItem>();
            foreach (MarkingItem copiedMarking in copiedMarkings)
            {
                MarkingItem pasted = new MarkingItem
                {
                    Id = nextMarkingId++,
                    Product = copiedMarking.Product,
                    Type = copiedMarking.Type,
                    PrintType = copiedMarking.PrintType,
                    Content = copiedMarking.Content,
                    Position = new PointF(copiedMarking.Position.X + offset, copiedMarking.Position.Y + offset),
                    Rotate = copiedMarking.Rotate
                    
                };
                ClampPosition(pasted);
                markings.Add(pasted);
                pastedMarkings.Add(pasted);

                int row = markingList.Rows.Add(GetMarkingNumber(pasted),pasted.Product ,pasted.Type,
                    FormatCoordinate(ToMillimeter(pasted.Position.X)),
                    FormatCoordinate(ToMillimeter(pasted.Position.Y)));
                markingList.Rows[row].Tag = pasted;
                CreateMarkingView(pasted);
            }

            selectedMarkings.Clear();
            selectedMarkings.AddRange(pastedMarkings);
            selectedMarking = pastedMarkings[pastedMarkings.Count - 1];
            UpdateMarkingSelectionVisuals();
        }

        private void UpdateMarkingSelectionVisuals()
        {
            updatingMarkingSelection = true;
            try
            {
                markingList.ClearSelection();
                for (int i = 0; i < markingList.Rows.Count; i++)
                {
                    bool selected = selectedMarkings.Contains(markingList.Rows[i].Tag as MarkingItem);
                    markingList.Rows[i].Selected = selected;
                    if (selected) markingList.CurrentCell = markingList.Rows[i].Cells[0];
                }
            }
            finally
            {
                updatingMarkingSelection = false;
            }

            foreach (MarkingItem marking in markings)
                if (marking.View != null) marking.View.BackColor = selectedMarkings.Contains(marking) ? Color.LimeGreen : Color.FromArgb(190, Color.SkyBlue);
                 
            UpdatePositionControls(selectedMarking);
        }
        private void PrintTypeChanged(object sender, EventArgs e)
        {
            if (updatingPositionControls || selectedMarking == null) return;
          //  selectedMarking.PrintType = NormalizeMarkingType(printType.Text);
        }
        private void MarkingContentChanged(object sender, EventArgs e)
        {
            if (updatingPositionControls || selectedMarking == null) return;
            selectedMarking.Content = markingContent.Text;
            UpdateMarkingDisplay(selectedMarking);
        }

        private void DetailsButtonClick(object sender, EventArgs e)
        {
            if (selectedMarking == null)
            {
                MessageBox.Show(this, "Please select a marking item first.", "Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (MarkingDetailsForm dialog = new MarkingDetailsForm(selectedMarking.PrintType, selectedMarking.Product, selectedMarking.Content))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                string updatedType = NormalizeMarkingType(dialog.MarkingType);
                bool viewTypeChanged = (selectedMarking.Type == "CODE") != (updatedType == "CODE");
                selectedMarking.Type = updatedType;
                selectedMarking.PrintType = updatedType;
                selectedMarking.Rotate = GetMarkingRotation(updatedType).ToString();
                selectedMarking.Product = string.IsNullOrWhiteSpace(dialog.ProductNumber) ? "0" : dialog.ProductNumber;
                selectedMarking.Content = dialog.Content ?? "";

                if (viewTypeChanged)
                {
                    Label previousView = selectedMarking.View;
                    imageViewport.Controls.Remove(previousView);
                    previousView.Dispose();
                    CreateMarkingView(selectedMarking);
                }
                else if (selectedMarking.View is DirectionalMarkingLabel directionalView)
                {
                    directionalView.Rotation = GetMarkingRotation(updatedType);
                }

                UpdatePositionControls(selectedMarking);
                UpdateMarkingDisplay(selectedMarking);
                UpdateMarkingView(selectedMarking);
                RebuildMarkingList();
                SelectMarking(selectedMarking);
            }
        }
        private void SelectPrintType(string type)
        {
            string normalized = NormalizeMarkingType(type);
            //for (int i = 0; i < printType.Items.Count; i++)
            //{
            //    if (NormalizeMarkingType(Convert.ToString(printType.Items[i])) == normalized)
            //    {
            //        printType.SelectedIndex = i;
            //        return;
            //    }
            //}
            //printType.Text = normalized;
        }

        private static void UpdateMarkingDisplay(MarkingItem item)
        {
            if (item == null || item.View == null) return;
            item.View.Text = string.IsNullOrWhiteSpace(item.Content) ? item.Type : item.Content;
            UpdateQrImage(item);
            item.View.Invalidate();
        }

        private static void UpdateQrImage(MarkingItem item)
        {
            QrMarkingLabel qrView = item?.View as QrMarkingLabel;
            if (qrView == null) return;

            if (qrView.QrImage != null)
            {
                qrView.QrImage.Dispose();
                qrView.QrImage = null;
            }

            if (!string.IsNullOrWhiteSpace(item.Content))
            {
                using (QRCodeGenerator generator = new QRCodeGenerator())
                using (QRCodeData data = generator.CreateQrCode(item.Content, QRCodeGenerator.ECCLevel.Q))
                using (QRCode qrCode = new QRCode(data))
                    qrView.QrImage = qrCode.GetGraphic(5);
            }
        }

        private void MarkingListSelectionChanged(object sender, EventArgs e)
        {
            if (updatingMarkingSelection) return;
            selectedMarkings.Clear();
            foreach (DataGridViewRow row in markingList.SelectedRows)
            {
                MarkingItem item = row.Tag as MarkingItem;
                if (item != null) selectedMarkings.Add(item);
            }
            selectedMarking = selectedMarkings.Count > 0 ? selectedMarkings[0] : null;
            UpdatePositionControls(selectedMarking);
            foreach (MarkingItem marking in markings) if (marking.View != null) marking.View.BackColor = selectedMarkings.Contains(marking) ? Color.LimeGreen : Color.FromArgb(190, Color.SkyBlue);
        }

        private void PositionValueChanged(object sender, EventArgs e)
        {
            if (updatingPositionControls || selectedMarking == null || !positionX.Focused && !positionY.Focused) return;
            selectedMarking.Position = new PointF(ToPixel((float)positionX.Value), ToPixel((float)positionY.Value));
            ClampPosition(selectedMarking);
            UpdateMarkingView(selectedMarking);
            UpdateMarkingRow(selectedMarking);
        }

        private void UpdatePositionControls(MarkingItem item)
        {
            if (item == null) return;
            updatingPositionControls = true;
            try
            {
                positionX.Value = ClampValue(positionX, ToMillimeter(item.Position.X));
                positionY.Value = ClampValue(positionY, ToMillimeter(item.Position.Y));
                SelectPrintType(item.PrintType);
                markingContent.Text = item.Content ?? "";
            }
            finally
            {
                updatingPositionControls = false;
            }
        }

        private static decimal ClampValue(NumericUpDown control, float value)
        {
            return Math.Max(control.Minimum, Math.Min(control.Maximum, (decimal)value));
        }

        private void UpdateMarkingView(MarkingItem item)
        {
            if (item.View == null) return;
            bool vertical = item.Type == "TOPTEXT" || item.Type == "BOTTOMTEXT";
            string displayText = string.IsNullOrWhiteSpace(item.Content) ? item.Type : item.Content;
            item.View.Font = new Font("Times New Roman", Math.Max(1f, MarkingFontSize * imageZoom), FontStyle.Regular);
            string[] textLines = displayText.Replace("\r\n", "\n").Split('\n');
            int textWidth = 0;
            foreach (string line in textLines)
                textWidth = Math.Max(textWidth, TextRenderer.MeasureText(line, item.View.Font).Width);
            int textHeight = TextRenderer.MeasureText("Ag", item.View.Font).Height * textLines.Length;
            Size baseSize = item.Type == "CODE"
                ? new Size(35, 35)
                : vertical
                ? new Size(Math.Max(40, textHeight + 12), Math.Max(120, textWidth + 8))
                : new Size(Math.Max(120, textWidth + 8), Math.Max(40, textHeight + 12));
            item.View.Size = new Size(
                Math.Max(1, (int)Math.Round(baseSize.Width * imageZoom)),
                Math.Max(1, (int)Math.Round(baseSize.Height * imageZoom)));
            item.View.Left = markingImage.Left + (int)(item.Position.X * imageZoom) - item.View.Width / 2;
            item.View.Top = markingImage.Top + (int)(item.Position.Y * imageZoom) - item.View.Height / 2;
            item.View.BringToFront();
        }

        private void UpdateAllMarkingViews()
        {
            foreach (MarkingItem item in markings) UpdateMarkingView(item);
            UpdateAllFiducialViews();
        }

        private void ClampPosition(MarkingItem item)
        {
            if (markingImage.Image == null) return;
            item.Position = new PointF(Math.Max(0, Math.Min(markingImage.Image.Width, item.Position.X)), Math.Max(0, Math.Min(markingImage.Image.Height, item.Position.Y)));
        }

        private static string FormatCoordinate(float value) { return value.ToString("0.00"); }

        private float Resolution
        {
            get
            {
                float value;
                return float.TryParse(resolutionValue.Text, out value) && value > 0 ? value : 1f;
            }
        }

        private float ImagePixelToMM
        {
            get { return ParseResolution(imagePixelToMmValue.Text, Resolution); }
        }

        private float ToMillimeter(float pixel) { return pixel * ImagePixelToMM; }
        private float ToPixel(float millimeter) { return millimeter / ImagePixelToMM; }

        private void ResolutionValueTextChanged(object sender, EventArgs e)
        {
            foreach (MarkingItem item in markings) UpdateMarkingRow(item);
            UpdatePositionControls(selectedMarking);
        }

        private void SelectionMouseDown(object sender, MouseEventArgs e)
        {
            if (panMode || addingFiducial || e.Button != MouseButtons.Left) return;
            selectingMarkings = true;
            selectionStart = ToViewportPoint((Control)sender, e.Location);
            selectionEnd = selectionStart;
            imageViewport.Capture = true;
            InvalidateSelectionArea();
        }

        private void SelectionMouseMove(object sender, MouseEventArgs e)
        {
            if (!selectingMarkings) return;
            selectionEnd = ToViewportPoint((Control)sender, e.Location);
            InvalidateSelectionArea();
        }

        private void SelectionMouseUp(object sender, MouseEventArgs e)
        {
            if (!selectingMarkings || e.Button != MouseButtons.Left) return;
            selectingMarkings = false;
            selectionEnd = ToViewportPoint((Control)sender, e.Location);
            imageViewport.Capture = false;
            Rectangle selection = NormalizeRectangle(selectionStart, selectionEnd);
            if (selection.Width > 3 || selection.Height > 3)
            {
                if ((ModifierKeys & Keys.Shift) != Keys.Shift) selectedMarkings.Clear();
                foreach (MarkingItem item in markings)
                {
                    Rectangle bounds = new Rectangle(
                        item.View.Left,
                        item.View.Top,
                        item.View.Width,
                        item.View.Height);
                    if (selection.IntersectsWith(bounds) && !selectedMarkings.Contains(item))
                        selectedMarkings.Add(item);
                }
                selectedMarking = selectedMarkings.Count > 0 ? selectedMarkings[selectedMarkings.Count - 1] : null;
                UpdateMarkingSelectionVisuals();
            }
            InvalidateSelectionArea();
        }

        private Point ToViewportPoint(Control source, Point point)
        {
            return imageViewport.PointToClient(source.PointToScreen(point));
        }

        private void InvalidateSelectionArea()
        {
            imageViewport.Invalidate();
            markingImage.Invalidate();
        }

        private static Rectangle NormalizeRectangle(Point first, Point second)
        {
            return new Rectangle(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y), Math.Abs(first.X - second.X), Math.Abs(first.Y - second.Y));
        }

        private void ImageViewportPaint(object sender, PaintEventArgs e)
        {
            if (!selectingMarkings || markingImage.Bounds.Contains(selectionStart)) return;
            DrawSelectionRectangle(e.Graphics, NormalizeRectangle(selectionStart, selectionEnd));
        }

        private void MarkingImagePaint(object sender, PaintEventArgs e)
        {
            if (!selectingMarkings) return;
            Point start = markingImage.PointToClient(imageViewport.PointToScreen(selectionStart));
            Point end = markingImage.PointToClient(imageViewport.PointToScreen(selectionEnd));
            DrawSelectionRectangle(e.Graphics, NormalizeRectangle(start, end));
        }

        private static void DrawSelectionRectangle(Graphics graphics, Rectangle selection)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(45, Color.DodgerBlue)))
            using (Pen pen = new Pen(Color.DodgerBlue, 1))
            {
                graphics.FillRectangle(brush, selection);
                graphics.DrawRectangle(pen, selection);
            }
        }

        private void UpdateMarkingRow(MarkingItem item)
        {
            foreach (DataGridViewRow row in markingList.Rows)
                if (row.Tag == item)
                {
                    row.Cells["X"].Value = FormatCoordinate(ToMillimeter(item.Position.X));
                    row.Cells["Y"].Value = FormatCoordinate(ToMillimeter(item.Position.Y));
                    break;
                }
        }

        private void InitializeMarkingList()
        {
            if (markingList.Columns.Count == 0)
            {
                markingList.Columns.Add("Number", "No.");
                markingList.Columns.Add("Product", "Product");
                markingList.Columns.Add("Type", "Type");
                markingList.Columns.Add("X", "X (mm)");
                markingList.Columns.Add("Y", "Y (mm)");
                markingList.Columns[0].FillWeight = 35;
            }

            if (fiducialList.Columns.Count == 0)
            {
                fiducialList.Columns.Add("Point", "Point No.");
                fiducialList.Columns.Add("X", "X");
                fiducialList.Columns.Add("Y", "Y");
            }

        }

        private void MarkingImageMouseWheel(object sender, MouseEventArgs e)
        {
            Point viewportPoint = imageViewport.PointToClient(Control.MousePosition);
            ZoomAt(viewportPoint, e.Delta);
        }

        private void ImageAreaMouseEnter(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control != null && control.CanFocus)
                control.Focus();
        }

        private void ZoomAt(Point viewportPoint, int wheelDelta)
        {
            if (markingImage.Image == null || wheelDelta == 0) return;

            Point scroll = imageViewport.AutoScrollPosition;
            float imageX = (viewportPoint.X - scroll.X) / imageZoom;
            float imageY = (viewportPoint.Y - scroll.Y) / imageZoom;
            float zoomFactor = (float)Math.Pow(1.05, wheelDelta / 120.0);
            float newZoom = Math.Max(0.1f, Math.Min(8f, imageZoom * zoomFactor));
            if (Math.Abs(newZoom - imageZoom) < 0.0001f) return;

            imageZoom = newZoom;
            UpdateImageTransform();

            int targetX = (int)(imageX * imageZoom - viewportPoint.X);
            int targetY = (int)(imageY * imageZoom - viewportPoint.Y);
            int maxX = Math.Max(0, markingImage.Width - imageViewport.ClientSize.Width);
            int maxY = Math.Max(0, markingImage.Height - imageViewport.ClientSize.Height);
            targetX = Math.Max(0, Math.Min(maxX, targetX));
            targetY = Math.Max(0, Math.Min(maxY, targetY));
            imageViewport.AutoScrollPosition = new Point(-targetX, -targetY);
        }

        private void MarkingImageMouseDown(object sender, MouseEventArgs e)
        {
            panning = false;
        }

        private void MarkingImageMouseMove(object sender, MouseEventArgs e)
        {
            return;
        }

        private void MarkingImageMouseUp(object sender, MouseEventArgs e)
        {
            panning = false;
        }

        private void ZoomInClick(object sender, EventArgs e)
        {
            Point center = new Point(imageViewport.ClientSize.Width / 2, imageViewport.ClientSize.Height / 2);
            ZoomAt(center, 120);
        }

        private void ZoomOutClick(object sender, EventArgs e)
        {
            Point center = new Point(imageViewport.ClientSize.Width / 2, imageViewport.ClientSize.Height / 2);
            ZoomAt(center, -120);
        }

        private void FitImageClick(object sender, EventArgs e)
        {
            if (markingImage.Image == null)
            {
                imageZoom = 1f;
                UpdateImageTransform();
                return;
            }

            int viewportWidth = Math.Max(1, imageViewport.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            int viewportHeight = Math.Max(1, imageViewport.ClientSize.Height - SystemInformation.HorizontalScrollBarHeight);
            imageZoom = Math.Min((float)viewportWidth / markingImage.Image.Width, (float)viewportHeight / markingImage.Image.Height);
            imageZoom = Math.Max(0.1f, Math.Min(8f, imageZoom));
            imagePan = Point.Empty;
            UpdateImageTransform();
        }

        private void UpdateImageTransform()
        {
            if (markingImage.Image == null)
            {
                UpdateZoomLabel();
                return;
            }

            markingImage.SizeMode = PictureBoxSizeMode.StretchImage;
            markingImage.Width = Math.Max(100, (int)(markingImage.Image.Width * imageZoom));
            markingImage.Height = Math.Max(100, (int)(markingImage.Image.Height * imageZoom));
            int viewportWidth = imageViewport.ClientSize.Width;
            int viewportHeight = imageViewport.ClientSize.Height;
            markingImage.Left = markingImage.Width < viewportWidth ? (viewportWidth - markingImage.Width) / 2 : 0;
            markingImage.Top = markingImage.Height < viewportHeight ? (viewportHeight - markingImage.Height) / 2 : 0;
            imageViewport.AutoScrollMinSize = new Size(
                Math.Max(viewportWidth, markingImage.Width),
                Math.Max(viewportHeight, markingImage.Height));
            UpdateAllMarkingViews();
            UpdateZoomLabel();
        }

        private void UpdateZoomLabel()
        {
            zoomLabel.Text = string.Format("{0}%", (int)(imageZoom * 100));
        }

        private void ImageViewportResize(object sender, EventArgs e)
        {
            UpdateImageTransform();
        }

        private void AddFiducialClick(object sender, EventArgs e)
        {
            addingFiducial = !addingFiducial;
            addFiducial.Text = addingFiducial ? "Click Image" : "Add";
            addFiducial.BackColor = addingFiducial ? Color.LightGreen : SystemColors.Control;
            markingImage.Cursor = addingFiducial ? Cursors.Cross : Cursors.Default;
            imageViewport.Cursor = addingFiducial ? Cursors.Cross : Cursors.Default;
        }

        private void AddFiducialAtMouseDown(object sender, MouseEventArgs e)
        {
            if (!addingFiducial || e.Button != MouseButtons.Left) return;

            Point viewportPoint = imageViewport.PointToClient(((Control)sender).PointToScreen(e.Location));
            PointF imagePoint;
            if (!TryGetImagePoint(viewportPoint, out imagePoint)) return;

            AddFiducial(imagePoint, nextFiducialId++);
            addingFiducial = false;
            addFiducial.Text = "Add";
            addFiducial.BackColor = SystemColors.Control;
            markingImage.Cursor = Cursors.Default;
            imageViewport.Cursor = Cursors.Default;
        }

        private void AddFiducial(PointF position, int id)
        {
            nextFiducialId = Math.Max(nextFiducialId, id + 1);
            FiducialItem item = new FiducialItem { Id = id, Position = position };
            fiducials.Add(item);
            int row = fiducialList.Rows.Add(item.Id.ToString(), FormatCoordinate(ToMillimeter(position.X)), FormatCoordinate(ToMillimeter(position.Y)));
            fiducialList.Rows[row].Tag = item;
            item.View = new FiducialLabel { AutoSize = false, Size = new Size(48, 24), Text = "F" + item.Id, Font = new Font(Font.FontFamily, 9f, FontStyle.Bold), ForeColor = Color.Red, BackColor = Color.Transparent, Tag = item, Cursor = Cursors.Hand };
            item.View.MouseDown += FiducialMouseDown;
            markingImage.Controls.Add(item.View);
            item.View.BringToFront();
            UpdateFiducialView(item);
        }

        private static Region CreateFiducialRegion(Size size)
        {
            int arm = 3;
            int centerX = size.Width / 2;
            int centerY = size.Height / 2;
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(new[]
            {
                new Point(centerX - arm, 0),
                new Point(centerX + arm, 0),
                new Point(centerX + arm, centerY - arm),
                new Point(size.Width, centerY - arm),
                new Point(size.Width, centerY + arm),
                new Point(centerX + arm, centerY + arm),
                new Point(centerX + arm, size.Height),
                new Point(centerX - arm, size.Height),
                new Point(centerX - arm, centerY + arm),
                new Point(0, centerY + arm),
                new Point(0, centerY - arm),
                new Point(centerX - arm, centerY - arm)
            });
            Region region = new Region(path);
            path.Dispose();
            return region;
        }

        private void FiducialMouseDown(object sender, MouseEventArgs e)
        {
            FiducialItem item = ((Control)sender).Tag as FiducialItem;
            if (item == null || e.Button != MouseButtons.Left) return;
            selectedFiducial = item;
            Point start = item.View.PointToScreen(e.Location);
            PointF original = item.Position;
            item.View.Capture = true;
            MouseEventHandler move = null;
            MouseEventHandler up = null;
            move = (s, args) =>
            {
                Point current = item.View.PointToScreen(args.Location);
                item.Position = new PointF(original.X + (current.X - start.X) / imageZoom, original.Y + (current.Y - start.Y) / imageZoom);
                ClampFiducial(item);
                UpdateFiducialView(item);
                UpdateFiducialRow(item);
            };
            up = (s, args) => { item.View.Capture = false; item.View.MouseMove -= move; item.View.MouseUp -= up; };
            item.View.MouseMove += move;
            item.View.MouseUp += up;
        }

        private void UpdateFiducialView(FiducialItem item)
        {
            if (item.View == null) return;
            item.View.Left = (int)(item.Position.X * imageZoom) - item.View.Width / 2;
            item.View.Top = (int)(item.Position.Y * imageZoom) - item.View.Height / 2;
            item.View.BringToFront();
        }

        private void UpdateAllFiducialViews() { foreach (FiducialItem item in fiducials) UpdateFiducialView(item); }
        private void ClampFiducial(FiducialItem item) { if (markingImage.Image != null) item.Position = new PointF(Math.Max(0, Math.Min(markingImage.Image.Width, item.Position.X)), Math.Max(0, Math.Min(markingImage.Image.Height, item.Position.Y))); }
        private void UpdateFiducialRow(FiducialItem item)
        {
            foreach (DataGridViewRow row in fiducialList.Rows) if (row.Tag == item) { row.Cells[1].Value = FormatCoordinate(ToMillimeter(item.Position.X)); row.Cells[2].Value = FormatCoordinate(ToMillimeter(item.Position.Y)); break; }
        }
        private void DeleteAllFiducials()
        {
            foreach (FiducialItem item in fiducials) if (item.View != null) markingImage.Controls.Remove(item.View);
            fiducials.Clear();
            nextFiducialId = 0;
            fiducialList.Rows.Clear();
        }

        private void DeleteFiducialClick(object sender, EventArgs e)
        {
            if (fiducialList.CurrentRow == null || fiducialList.CurrentRow.IsNewRow) return;
            FiducialItem item = fiducialList.CurrentRow.Tag as FiducialItem;
            if (item != null) { if (item.View != null) markingImage.Controls.Remove(item.View); fiducials.Remove(item); }
            fiducialList.Rows.Remove(fiducialList.CurrentRow);
        }

        private void MoveUpClick(object sender, EventArgs e) { MoveSelected(-1); }
        private void MoveDownClick(object sender, EventArgs e) { MoveSelected(1); }

        private void MoveSelected(int direction)
        {
            if (selectedMarking == null) return;
            int index = markings.IndexOf(selectedMarking);
            int target = index + direction;
            if (index < 0 || target < 0 || target >= markings.Count) return;
            markings.RemoveAt(index);
            markings.Insert(target, selectedMarking);
            RebuildMarkingList();
            SelectMarking(selectedMarking);
        }

        private void DeleteMarkingClick(object sender, EventArgs e)
        {
            if (selectedMarking == null) return;
            if (selectedMarking.View != null) imageViewport.Controls.Remove(selectedMarking.View);
            markings.Remove(selectedMarking);
            selectedMarking = null;
            RebuildMarkingList();
        }

        private void DeleteAllMarkingClick(object sender, EventArgs e)
        {
            foreach (MarkingItem item in markings)
                if (item.View != null) imageViewport.Controls.Remove(item.View);
            markings.Clear();
            selectedMarking = null;
            RebuildMarkingList();
        }

        private void RebuildMarkingList()
        {
            NormalizeMarkingOrder();
            markingList.Rows.Clear();
            foreach (MarkingItem item in markings)
            {
                int row = markingList.Rows.Add(GetMarkingNumber(item),item.Product, item.Type, FormatCoordinate(ToMillimeter(item.Position.X)), FormatCoordinate(ToMillimeter(item.Position.Y)));
                markingList.Rows[row].Tag = item;
            }
        }

        private static string GetMarkingNumber(MarkingItem item)
        {
            return string.IsNullOrWhiteSpace(item.Product) || item.Product == "0" ? item.Id.ToString() : item.Id.ToString();
        }

        private void NormalizeMarkingOrder()
        {
            for (int i = 0; i < markings.Count; i++)
                markings[i].Id = i + 1;
            nextMarkingId = markings.Count + 1;
        }

        private void AlignSelected(int mode)
        {
            SyncSelectedMarkingsFromListIfNeeded();
            if (selectedMarkings.Count < 2) return;
            float value = mode == 0 ? selectedMarkings.Min(item => item.Position.X) : mode == 2 ? selectedMarkings.Max(item => item.Position.X) : selectedMarkings.Average(item => item.Position.X);
            float y = mode == 3 ? selectedMarkings.Min(item => item.Position.Y) : mode == 5 ? selectedMarkings.Max(item => item.Position.Y) : selectedMarkings.Average(item => item.Position.Y);
            foreach (MarkingItem item in selectedMarkings)
            {
                if (mode <= 2) item.Position = new PointF(value, item.Position.Y);
                else item.Position = new PointF(item.Position.X, y);
                UpdateMarkingView(item); UpdateMarkingRow(item);
            }
            UpdatePositionControls(selectedMarking);
        }

        private void SyncSelectedMarkingsFromListIfNeeded()
        {
            if (selectedMarkings.Count > 0) return;

            foreach (DataGridViewRow row in markingList.SelectedRows)
            {
                MarkingItem item = row.Tag as MarkingItem;
                if (item != null && !selectedMarkings.Contains(item))
                    selectedMarkings.Add(item);
            }

            if (selectedMarking == null && selectedMarkings.Count > 0)
                selectedMarking = selectedMarkings[0];
        }

        private float MinX() { float value = float.MaxValue; foreach (MarkingItem item in markings) value = Math.Min(value, item.Position.X); return value; }
        private float MaxX() { float value = float.MinValue; foreach (MarkingItem item in markings) value = Math.Max(value, item.Position.X); return value; }
        private float MinY() { float value = float.MaxValue; foreach (MarkingItem item in markings) value = Math.Min(value, item.Position.Y); return value; }
        private float MaxY() { float value = float.MinValue; foreach (MarkingItem item in markings) value = Math.Max(value, item.Position.Y); return value; }

        private void DistributeSelected(bool horizontal)
        {
            if (markings.Count < 3 || selectedMarking == null) return;
            List<MarkingItem> ordered = new List<MarkingItem>(selectedMarkings);
            ordered.Sort((a, b) => horizontal ? a.Position.X.CompareTo(b.Position.X) : a.Position.Y.CompareTo(b.Position.Y));
            float first = horizontal ? ordered[0].Position.X : ordered[0].Position.Y;
            float last = horizontal ? ordered[ordered.Count - 1].Position.X : ordered[ordered.Count - 1].Position.Y;
            for (int i = 1; i < ordered.Count - 1; i++)
            {
                float coordinate = first + (last - first) * i / (ordered.Count - 1);
                ordered[i].Position = horizontal ? new PointF(coordinate, ordered[i].Position.Y) : new PointF(ordered[i].Position.X, coordinate);
                UpdateMarkingView(ordered[i]);
                UpdateMarkingRow(ordered[i]);
            }
            UpdatePositionControls(selectedMarking);
        }
    }
}
