using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;
using QRCoder;

namespace SLTtechSoft
{
    public partial class LaserSettingLayout : Form
    {
        public event EventHandler SaveAllRequested;
        public event Action<Action<Bitmap, int, int>> ScanImageRequested;

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
            if (string.IsNullOrWhiteSpace(text))
                return fallback;

            // Do not allow thousands separators here. On Vietnamese Windows, for example,
            // "." is commonly the thousands separator and "0.02" could otherwise be parsed
            // as 2 instead of 0.02. Accept either the current culture decimal separator
            // or invariant "." while keeping the entered numeric value unambiguous.
            float value;
            bool parsed = float.TryParse(text.Trim(), NumberStyles.Float,
                CultureInfo.CurrentCulture, out value);

            if (!parsed)
                parsed = float.TryParse(text.Trim(), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out value);

            return parsed && value > 0 && !float.IsNaN(value) && !float.IsInfinity(value)
                ? value
                : fallback;
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
            public string ImageId;
            public PointF LocalPosition;
            public PointF Position; // Contact-sheet canvas position used only for drawing.
            public Label View;
        }

        private sealed class ImageTileInfo
        {
            public string ImageId;
            public RectangleF DisplayBounds;
            public int SourceWidth;
            public int SourceHeight;
            // Affine mapping from source-image pixels to stitched-canvas pixels.
            public PointF CanvasOrigin;
            public PointF CanvasXAxis;
            public PointF CanvasYAxis;
            public float Scale;
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
            public string ImageId;
            public PointF LocalPosition;
            public string Product;
            public string Type;
            public string PrintType;
            public string Content;
            public PointF Position; // Contact-sheet canvas position used only for drawing.
            public Label View;
            public string Rotate;
        }

        public  List<MarkingItem> markings = new List<MarkingItem>();
        private readonly List<MarkingItem> selectedMarkings = new List<MarkingItem>();
        private readonly List<FiducialItem> fiducials = new List<FiducialItem>();
        private readonly List<ImageTileInfo> imageTiles = new List<ImageTileInfo>();
        public MarkingItem selectedMarking;
        private FiducialItem selectedFiducial;
        private int nextMarkingId;
        private int nextFiducialId;
        private float imageZoom = 1f;
        private float targetZoom = 1f;
        private Point zoomAnchor;
        private float zoomAnchorImageX;
        private float zoomAnchorImageY;
        private Timer zoomAnimationTimer;
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
        private string modelFolderPath = "";
        private bool refreshingPictureGrid;
        private bool picturePreviewActive;
        private bool refreshingAxisControls;
        private bool loadingSurfaceControls;
        private CheckBox enableAxisTransform;
        private CheckBox swapServoAxes;
        private CheckBox reverseServoX;
        private CheckBox reverseServoY;
        private readonly List<MarkingItem> copiedMarkings = new List<MarkingItem>();
        private const float MarkingFontSize = 9.75f;

        public LaserSettingLayout()
        {
            InitializeComponent();
            KeyPreview = true;
            zoomAnimationTimer = new Timer { Interval = 15 };
            zoomAnimationTimer.Tick += ZoomAnimationTick;
            FormClosed += (s, e) =>
            {
                zoomAnimationTimer.Stop();
                zoomAnimationTimer.Dispose();
            };
            ConfigurePictureGrid();
            ConfigureAxisTransformControls();
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
            // Zoom and pan are handled by imagePan/imageZoom only. Native AutoScroll must stay disabled.
            imageViewport.AutoScroll = false;
            imageViewport.Dock = DockStyle.Fill;
            imageViewport.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            markingImage.Dock = DockStyle.None;
            markingImage.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            markingImage.AutoSize = false;
            markingImage.Location = Point.Empty;
            EnableDoubleBuffering(imageViewport);
            EnableDoubleBuffering(markingImage);
            imageViewport.Controls.SetChildIndex(markingImage, 0);
        }

        private static void EnableDoubleBuffering(Control control)
        {
            if (control == null) return;
            PropertyInfo property = typeof(Control).GetProperty(
                "DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null)
                property.SetValue(control, true, null);
        }

        public void LoadImage(string fileName)
        {
            LoadImage(fileName, true);
        }

        private void LoadImage(string fileName, bool fitToViewport)
        {
            LoadImage(fileName, fitToViewport, true);
        }

        private void LoadImage(string fileName, bool fitToViewport, bool updateSurfaceFileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !System.IO.File.Exists(fileName))
                return;

            if (updateSurfaceFileName)
            {
                if (currentSurface == LaserSurface.Top) topImageFileName = fileName;
                else botImageFileName = fileName;
            }

            Bitmap loadedImage;
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (Image source = Image.FromStream(stream))
                loadedImage = new Bitmap(source);

            Image oldImage = markingImage.Image;
            markingImage.Image = loadedImage;
            if (oldImage != null)
                oldImage.Dispose();

            if (fitToViewport)
            {
                imagePan = Point.Empty;
                FitImageClick(this, EventArgs.Empty);
            }
            else
            {
                UpdateImageTransform();
            }
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
            markingImage.MouseEnter += ImageAreaMouseEnter;
            imageViewport.MouseEnter += ImageAreaMouseEnter;
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
            markingList.CellEndEdit += MarkingListCellEndEdit;
            markingList.DataError += (s, e) => { e.ThrowException = false; };
            fiducialList.CellEndEdit += FiducialListCellEndEdit;
            fiducialList.DataError += (s, e) => { e.ThrowException = false; };
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
            scanImage.Click += ScanImageClick;
            dgvListPictureSub.CellDoubleClick += PictureGridCellDoubleClick;
            dgvListPictureSub.MouseLeave += PictureGridMouseLeave;
            dgvListPictureSub.CellValueChanged += PictureGridCellValueChanged;
            dgvListPictureSub.DataError += (s, e) => { e.ThrowException = false; };
            btnAddPicture.Click += AddPicturesClick;
            btnDeletePic.Click += DeleteSelectedPicturesClick;
            btnPicDeleteAll.Click += DeleteAllPicturesClick;
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
            if (!File.Exists(topImageFileName) && topParameters.ScanImages.Count > 0)
                topImageFileName = ResolvePicturePath(topParameters.ScanImages[0]);
            if (!File.Exists(botImageFileName) && botParameters.ScanImages.Count > 0)
                botImageFileName = ResolvePicturePath(botParameters.ScanImages[0]);
            savedTopParameters = CloneSurface(topParameters);
            savedBotParameters = CloneSurface(botParameters);
            currentSurface = LaserSurface.Top;
            ListPicture.Text = "Picture - TOP";
            RebuildSurfaceImage(LaserSurface.Top);
            RebuildSurfaceImage(LaserSurface.Bot);
            btnTop.BackColor = Color.LightGreen;
            btnBot.BackColor = Color.WhiteSmoke;
            LoadSurfaceImage(topImageFileName);
            LoadCurrentSurfaceToControls();
            RefreshPictureGrid();
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
                target.Markings.Add(new LaserMarkingParameter { Id = marking.Id, ImageId = marking.ImageId ?? "", Product = marking.Product, Type = marking.Type, PrintType = marking.PrintType, Content = marking.Content, PositionX = marking.PositionX, PositionY = marking.PositionY, Rotate = marking.Rotate });
            foreach (LaserFiducialParameter point in source.Fiducials ?? new List<LaserFiducialParameter>())
                target.Fiducials.Add(new LaserFiducialParameter { Id = point.Id, ImageId = point.ImageId ?? "", PositionX = point.PositionX, PositionY = point.PositionY });
            target.ImagePixelToMM = source.ImagePixelToMM > 0 ? source.ImagePixelToMM : source.ResolutionImage;
            target.ResolutionImageRobot = source.ResolutionImageRobot > 0 ? source.ResolutionImageRobot : source.ResolutionImage;
            target.EnableServoAxisTransform = source.EnableServoAxisTransform;
            target.SwapServoAxes = source.SwapServoAxes;
            target.ReverseServoX = source.ReverseServoX;
            target.ReverseServoY = source.ReverseServoY;
            target.HasStitchCoordinateSystem = source.HasStitchCoordinateSystem;
            target.StitchOriginX = source.StitchOriginX;
            target.StitchOriginY = source.StitchOriginY;
            target.StitchPixelToMM = source.StitchPixelToMM;
            target.ImageLocalCoordinateVersion = source.ImageLocalCoordinateVersion;
            foreach (LaserScanImageParameter picture in source.ScanImages ?? new List<LaserScanImageParameter>())
                target.ScanImages.Add(new LaserScanImageParameter
                {
                    Id = string.IsNullOrWhiteSpace(picture.Id) ? Guid.NewGuid().ToString("N") : picture.Id,
                    ImagePath = picture.ImagePath,
                    ServoX = picture.ServoX,
                    ServoY = picture.ServoY,
                    CaptureTime = picture.CaptureTime
                });
            return target;
        }

        private LaserSurfaceParameter CurrentParameters
        {
            get { return currentSurface == LaserSurface.Top ? topParameters : botParameters; }
        }

        public void SetModelFolderPath(string folderPath)
        {
            modelFolderPath = folderPath ?? "";
            RefreshPictureGrid();
        }

        //Lấy ảnh
        private void SwitchSurface(LaserSurface surface)
        {
            SaveCurrentSurfaceToMemory();
            currentSurface = surface;
            ListPicture.Text = surface == LaserSurface.Top ? "Picture - TOP" : "Picture - BOT";
            RebuildSurfaceImage(surface);
            btnTop.BackColor = surface == LaserSurface.Top ? Color.LightGreen : Color.WhiteSmoke;
            btnBot.BackColor = surface == LaserSurface.Bot ? Color.LightGreen : Color.WhiteSmoke;
            LoadSurfaceImage(surface == LaserSurface.Top ? topImageFileName : botImageFileName);
            LoadCurrentSurfaceToControls();
            FitImageClick(this, EventArgs.Empty);
        }

        private static string GetTopImageFile(CamPara camera)
        {
            if (!string.IsNullOrWhiteSpace(camera.LaserTopImageFile)) return camera.LaserTopImageFile;
            return camera.LaserImageFile;
        }

        private void LoadSurfaceImage(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) && System.IO.File.Exists(fileName))
                LoadImage(fileName, false);
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
            if (dgvListPictureSub != null && dgvListPictureSub.IsCurrentCellInEditMode)
                dgvListPictureSub.EndEdit();

            if (currentSurface == LaserSurface.Top) topParameters = CaptureCurrentSurface();
            else botParameters = CaptureCurrentSurface();
        }

        private LaserSurfaceParameter CaptureCurrentSurface()
        {
            LaserSurfaceParameter result = new LaserSurfaceParameter
            {
                ResolutionImage = ParseResolution(resolutionValue.Text, 1f),
                ImagePixelToMM = ParseResolution(imagePixelToMmValue.Text, 1f),
                ResolutionImageRobot = ParseResolution(robotResolutionValue.Text, 1f),
                EnableServoAxisTransform = enableAxisTransform != null && enableAxisTransform.Checked,
                SwapServoAxes = swapServoAxes != null && swapServoAxes.Checked,
                ReverseServoX = reverseServoX != null && reverseServoX.Checked,
                ReverseServoY = reverseServoY != null && reverseServoY.Checked,
                HasStitchCoordinateSystem = CurrentParameters.HasStitchCoordinateSystem,
                StitchOriginX = CurrentParameters.StitchOriginX,
                StitchOriginY = CurrentParameters.StitchOriginY,
                StitchPixelToMM = CurrentParameters.StitchPixelToMM,
                ImageLocalCoordinateVersion = 1
            };
            foreach (LaserScanImageParameter picture in CurrentParameters.ScanImages ?? new List<LaserScanImageParameter>())
                result.ScanImages.Add(new LaserScanImageParameter
                {
                    Id = picture.Id,
                    ImagePath = picture.ImagePath,
                    ServoX = picture.ServoX,
                    ServoY = picture.ServoY,
                    CaptureTime = picture.CaptureTime
                });
            foreach (MarkingItem item in markings)
                result.Markings.Add(new LaserMarkingParameter { Id = item.Id, ImageId = item.ImageId ?? "", Product = item.Product, Type = item.Type, PrintType = item.PrintType, Content = item.Content, PositionX = string.IsNullOrEmpty(item.ImageId) ? item.Position.X : item.LocalPosition.X, PositionY = string.IsNullOrEmpty(item.ImageId) ? item.Position.Y : item.LocalPosition.Y, Rotate = item.Rotate });
            foreach (FiducialItem point in fiducials)
                result.Fiducials.Add(new LaserFiducialParameter { Id = point.Id, ImageId = point.ImageId ?? "", PositionX = string.IsNullOrEmpty(point.ImageId) ? point.Position.X : point.LocalPosition.X, PositionY = string.IsNullOrEmpty(point.ImageId) ? point.Position.Y : point.LocalPosition.Y });
            return result;
        }

        private void LoadCurrentSurfaceToControls()
        {
            MigrateNormalizedImageLocalCoordinates();
            loadingSurfaceControls = true;
            DeleteAllMarkingClick(this, EventArgs.Empty);
            DeleteAllFiducials();
            resolutionValue.Text = CurrentParameters.ResolutionImage.ToString("0.########");
            imagePixelToMmValue.Text = CurrentParameters.ImagePixelToMM.ToString("0.########");
            robotResolutionValue.Text = CurrentParameters.ResolutionImageRobot.ToString("0.########");
            if (enableAxisTransform != null)
            {
                refreshingAxisControls = true;
                enableAxisTransform.Checked = CurrentParameters.EnableServoAxisTransform;
                swapServoAxes.Checked = CurrentParameters.SwapServoAxes;
                reverseServoX.Checked = CurrentParameters.ReverseServoX;
                reverseServoY.Checked = CurrentParameters.ReverseServoY;
                swapServoAxes.Enabled = enableAxisTransform.Checked;
                reverseServoX.Enabled = enableAxisTransform.Checked;
                reverseServoY.Enabled = enableAxisTransform.Checked;
                refreshingAxisControls = false;
            }
            foreach (LaserMarkingParameter marking in CurrentParameters.Markings ?? new List<LaserMarkingParameter>())
            {
                PointF storedPosition = new PointF((float)marking.PositionX, (float)marking.PositionY);
                PointF displayPosition = string.IsNullOrWhiteSpace(marking.ImageId) ? storedPosition : SourceToCanvas(marking.ImageId, storedPosition);
                AddMarking(marking.Type, displayPosition, marking.Id, marking.Product, marking.PrintType, marking.Content,
                    marking.Rotate, marking.ImageId, storedPosition);
            }
            NormalizeMarkingOrder();
            RebuildMarkingList();
            foreach (LaserFiducialParameter point in CurrentParameters.Fiducials ?? new List<LaserFiducialParameter>())
            {
                PointF storedPosition = new PointF((float)point.PositionX, (float)point.PositionY);
                PointF displayPosition = string.IsNullOrWhiteSpace(point.ImageId) ? storedPosition : SourceToCanvas(point.ImageId, storedPosition);
                AddFiducial(displayPosition, point.Id, point.ImageId, storedPosition);
            }
            RefreshPictureGrid();
            loadingSurfaceControls = false;
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

        private void ConfigureAxisTransformControls()
        {
            enableAxisTransform = new CheckBox
            {
                Name = "enableAxisTransform",
                Text = "Enable",
                AutoSize = true,
                Location = new Point(5, 34),
                BackColor = Color.Transparent
            };
            swapServoAxes = new CheckBox
            {
                Name = "swapServoAxes",
                Text = "Swap X/Y",
                AutoSize = true,
                Location = new Point(72, 34),
                BackColor = Color.Transparent
            };
            reverseServoX = new CheckBox
            {
                Name = "reverseServoX",
                Text = "Flip X",
                AutoSize = true,
                Location = new Point(145, 34),
                BackColor = Color.Transparent
            };
            reverseServoY = new CheckBox
            {
                Name = "reverseServoY",
                Text = "Flip Y",
                AutoSize = true,
                Location = new Point(215, 34),
                BackColor = Color.Transparent
            };

            panel7.Controls.Add(enableAxisTransform);
            panel7.Controls.Add(swapServoAxes);
            panel7.Controls.Add(reverseServoX);
            panel7.Controls.Add(reverseServoY);

            dgvListPictureSub.Location = new Point(5, 57);
            dgvListPictureSub.Size = new Size(214, 103);
            btnAddPicture.Location = new Point(227, 57);
            btnDeletePic.Location = new Point(227, 91);
            btnPicDeleteAll.Location = new Point(227, 125);

            enableAxisTransform.CheckedChanged += AxisTransformChanged;
            swapServoAxes.CheckedChanged += AxisTransformChanged;
            reverseServoX.CheckedChanged += AxisTransformChanged;
            reverseServoY.CheckedChanged += AxisTransformChanged;
        }

        private void AxisTransformChanged(object sender, EventArgs e)
        {
            if (refreshingAxisControls) return;
            bool enabled = enableAxisTransform.Checked;
            swapServoAxes.Enabled = enabled;
            reverseServoX.Enabled = enabled;
            reverseServoY.Enabled = enabled;
            SaveCurrentSurfaceToMemory();
            RebuildSurfaceImage(currentSurface);
        }

        private void ConfigurePictureGrid()
        {
            dgvListPictureSub.Columns.Clear();
            dgvListPictureSub.AutoGenerateColumns = false;
            dgvListPictureSub.AllowUserToAddRows = false;
            dgvListPictureSub.AllowUserToDeleteRows = false;
            dgvListPictureSub.ReadOnly = false;
            dgvListPictureSub.MultiSelect = true;
            dgvListPictureSub.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvListPictureSub.RowHeadersVisible = false;
            dgvListPictureSub.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvListPictureSub.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPicturePath", HeaderText = "Ảnh",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 80, ReadOnly = true
            });
            dgvListPictureSub.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colServoX", HeaderText = "Servo X (mm)", Width = 75, ReadOnly = false, ValueType = typeof(string)
            });
            dgvListPictureSub.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colServoY", HeaderText = "Servo Y (mm)", Width = 75, ReadOnly = false, ValueType = typeof(string)
            });
        }

        private string ResolvePicturePath(LaserScanImageParameter picture)
        {
            if (picture == null || string.IsNullOrWhiteSpace(picture.ImagePath)) return "";
            if (Path.IsPathRooted(picture.ImagePath)) return picture.ImagePath;
            if (string.IsNullOrWhiteSpace(modelFolderPath)) return picture.ImagePath;
            return Path.GetFullPath(Path.Combine(modelFolderPath, picture.ImagePath));
        }

        private void RefreshPictureGrid()
        {
            if (dgvListPictureSub == null) return;
            refreshingPictureGrid = true;
            try
            {
                dgvListPictureSub.Rows.Clear();
                foreach (LaserScanImageParameter picture in CurrentParameters.ScanImages ?? new List<LaserScanImageParameter>())
                {
                    string fullPath = ResolvePicturePath(picture);
                    int rowIndex = dgvListPictureSub.Rows.Add(
                        fullPath,
                        picture.ServoX.HasValue ? picture.ServoX.Value.ToString("0.###", CultureInfo.CurrentCulture) : "",
                        picture.ServoY.HasValue ? picture.ServoY.Value.ToString("0.###", CultureInfo.CurrentCulture) : "");
                    dgvListPictureSub.Rows[rowIndex].Tag = picture;
                    dgvListPictureSub.Rows[rowIndex].Cells["colPicturePath"].ToolTipText = fullPath;
                    if (!File.Exists(fullPath))
                        dgvListPictureSub.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
            finally
            {
                refreshingPictureGrid = false;
            }
        }

        private void PictureGridCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvListPictureSub.Rows.Count) return;
            LaserScanImageParameter picture = dgvListPictureSub.Rows[e.RowIndex].Tag as LaserScanImageParameter;
            string path = ResolvePicturePath(picture);
            if (!File.Exists(path)) return;

            picturePreviewActive = true;
            LoadImage(path, true, false); // Fit the temporary single-image preview without replacing the stitched path.
        }

        private void PictureGridMouseLeave(object sender, EventArgs e)
        {
            if (!picturePreviewActive) return;
            picturePreviewActive = false;

            string stitchedPath = currentSurface == LaserSurface.Top ? topImageFileName : botImageFileName;
            if (File.Exists(stitchedPath))
                LoadImage(stitchedPath, true, false);
            else
                RebuildSurfaceImage(currentSurface);
        }

        private void AddPicturesClick(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Add pictures to " + (currentSurface == LaserSurface.Top ? "TOP" : "BOT");
                dialog.Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff|All files|*.*";
                dialog.Multiselect = true;
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                if (string.IsNullOrWhiteSpace(modelFolderPath))
                {
                    MessageBox.Show("Please call or save a Model before adding pictures.", "Add Pictures",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string surfaceFolder = currentSurface == LaserSurface.Top ? "TOP" : "BOT";
                string relativeFolder = Path.Combine("LaserPictures", surfaceFolder);
                string targetFolder = Path.Combine(modelFolderPath, relativeFolder);
                try
                {
                    Directory.CreateDirectory(targetFolder);
                    foreach (string sourcePath in dialog.FileNames)
                    {
                        if (!File.Exists(sourcePath)) continue;
                        string extension = Path.GetExtension(sourcePath);
                        string fileName = Path.GetFileNameWithoutExtension(sourcePath) + "_" +
                            Guid.NewGuid().ToString("N").Substring(0, 8) + extension;
                        string destination = Path.Combine(targetFolder, fileName);
                        File.Copy(sourcePath, destination, false);

                        CurrentParameters.ScanImages.Add(new LaserScanImageParameter
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            ImagePath = Path.Combine(relativeFolder, fileName),
                            ServoX = null,
                            ServoY = null,
                            CaptureTime = DateTime.Now
                        });
                    }

                    SaveCurrentSurfaceToMemory();
                    RefreshPictureGrid();
                    RebuildSurfaceImage(currentSurface);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not add pictures: " + ex.Message, "Add Pictures",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteSelectedPicturesClick(object sender, EventArgs e)
        {
            if (dgvListPictureSub.SelectedRows.Count == 0) return;
            List<LaserScanImageParameter> selected = new List<LaserScanImageParameter>();
            foreach (DataGridViewRow row in dgvListPictureSub.SelectedRows)
            {
                LaserScanImageParameter picture = row.Tag as LaserScanImageParameter;
                if (picture != null && !selected.Contains(picture)) selected.Add(picture);
            }
            RemovePictures(selected);
        }

        private void DeleteAllPicturesClick(object sender, EventArgs e)
        {
            if (CurrentParameters.ScanImages == null || CurrentParameters.ScanImages.Count == 0) return;
            DialogResult answer = MessageBox.Show(
                "Delete all pictures from the " + (currentSurface == LaserSurface.Top ? "TOP" : "BOT") + " list?",
                "Delete All Pictures", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;
            RemovePictures(new List<LaserScanImageParameter>(CurrentParameters.ScanImages));
        }

        private void RemovePictures(List<LaserScanImageParameter> pictures)
        {
            if (pictures == null || pictures.Count == 0) return;
            HashSet<string> removedImageIds = new HashSet<string>(
                pictures.Where(p => !string.IsNullOrWhiteSpace(p.Id)).Select(p => p.Id),
                StringComparer.OrdinalIgnoreCase);

            foreach (LaserScanImageParameter picture in pictures)
            {
                CurrentParameters.ScanImages.Remove(picture);
                DeleteModelPictureFile(picture);
            }

            foreach (MarkingItem item in markings.Where(x => removedImageIds.Contains(x.ImageId ?? "")).ToList())
            {
                if (item.View != null) imageViewport.Controls.Remove(item.View);
                markings.Remove(item);
                selectedMarkings.Remove(item);
                if (selectedMarking == item) selectedMarking = null;
            }
            foreach (FiducialItem item in fiducials.Where(x => removedImageIds.Contains(x.ImageId ?? "")).ToList())
            {
                if (item.View != null) markingImage.Controls.Remove(item.View);
                fiducials.Remove(item);
            }
            RebuildMarkingList();
            RefreshFiducialRows();

            SaveCurrentSurfaceToMemory();
            RefreshPictureGrid();
            RebuildSurfaceImage(currentSurface, CurrentParameters.ScanImages.Count == 0);
        }

        private void DeleteModelPictureFile(LaserScanImageParameter picture)
        {
            try
            {
                string path = ResolvePicturePath(picture);
                if (string.IsNullOrWhiteSpace(modelFolderPath) || !File.Exists(path)) return;

                string modelRoot = Path.GetFullPath(modelFolderPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                string fullPath = Path.GetFullPath(path);
                string allowedRoot = Path.GetFullPath(Path.Combine(modelFolderPath, "LaserPictures")).
                    TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

                if (fullPath.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase) &&
                    fullPath.StartsWith(modelRoot, StringComparison.OrdinalIgnoreCase))
                    File.Delete(fullPath);
            }
            catch
            {
                // Removing a list entry must not fail just because an old image file is locked.
            }
        }

        private void PictureGridCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (refreshingPictureGrid || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            string columnName = dgvListPictureSub.Columns[e.ColumnIndex].Name;
            if (columnName != "colServoX" && columnName != "colServoY") return;

            DataGridViewRow row = dgvListPictureSub.Rows[e.RowIndex];
            LaserScanImageParameter picture = row.Tag as LaserScanImageParameter;
            if (picture == null) return;

            double? coordinate;
            string raw = Convert.ToString(row.Cells[e.ColumnIndex].Value).Trim();
            if (string.IsNullOrWhiteSpace(raw))
                coordinate = null;
            else
            {
                double parsed;
                bool valid = double.TryParse(raw, NumberStyles.Float,
                    CultureInfo.CurrentCulture, out parsed) ||
                    double.TryParse(raw, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out parsed);
                if (!valid || double.IsNaN(parsed) || double.IsInfinity(parsed))
                {
                    MessageBox.Show("Please enter a valid coordinate in millimetres, or leave the cell blank.",
                        "Invalid Coordinate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RefreshPictureGrid();
                    return;
                }
                coordinate = parsed;
            }

            if (columnName == "colServoX") picture.ServoX = coordinate;
            else picture.ServoY = coordinate;

            SaveCurrentSurfaceToMemory();
            RebindPictureGridTags();
            RebuildSurfaceImage(currentSurface);
        }

        private void RebindPictureGridTags()
        {
            List<LaserScanImageParameter> pictures = CurrentParameters.ScanImages ??
                new List<LaserScanImageParameter>();
            int count = Math.Min(dgvListPictureSub.Rows.Count, pictures.Count);
            for (int i = 0; i < count; i++)
            {
                dgvListPictureSub.Rows[i].Tag = pictures[i];
                dgvListPictureSub.Rows[i].Cells["colPicturePath"].ToolTipText =
                    ResolvePicturePath(pictures[i]);
            }
        }

        private bool TryMapLegacyContactSheetPoint(PointF oldCanvasPoint, out string imageId, out PointF sourcePoint)
        {
            imageId = "";
            sourcePoint = PointF.Empty;
            const int oldThumbWidth = 320;
            const int oldThumbHeight = 240;
            const int oldCellWidth = 344;
            const int oldCellHeight = 288;
            int columns = Math.Min(3, imageTiles.Count);

            for (int i = 0; i < imageTiles.Count; i++)
            {
                ImageTileInfo tile = imageTiles[i];
                double scale = Math.Min(oldThumbWidth / (double)tile.SourceWidth, oldThumbHeight / (double)tile.SourceHeight);
                int drawWidth = Math.Max(1, (int)Math.Round(tile.SourceWidth * scale));
                int drawHeight = Math.Max(1, (int)Math.Round(tile.SourceHeight * scale));
                int cellX = 12 + (i % columns) * oldCellWidth;
                int cellY = 12 + (i / columns) * oldCellHeight;
                int drawX = cellX + (oldCellWidth - drawWidth) / 2;
                int drawY = cellY + 4 + (oldThumbHeight - drawHeight) / 2;
                RectangleF oldBounds = new RectangleF(drawX, drawY, drawWidth, drawHeight);
                if (!oldBounds.Contains(oldCanvasPoint)) continue;

                imageId = tile.ImageId;
                sourcePoint = new PointF(
                    (float)((oldCanvasPoint.X - drawX) / scale),
                    (float)((oldCanvasPoint.Y - drawY) / scale));
                return true;
            }

            return false;
        }

        private void MigrateLegacyPointsToImageTiles(bool hadLegacyStitch, double originX, double originY, double oldMmPerPixel)
        {
            if (imageTiles.Count == 0) return;
            double mmPerPixel = Math.Max(0.0000001, CurrentParameters.ImagePixelToMM);

            Func<PointF, Tuple<string, PointF>> findSourcePoint = oldPixel =>
            {
                double machineX;
                double machineY;
                if (hadLegacyStitch && oldMmPerPixel > 0)
                {
                    machineX = originX + oldPixel.X * oldMmPerPixel;
                    machineY = originY - oldPixel.Y * oldMmPerPixel;
                }
                else
                {
                    string legacyImageId;
                    PointF legacyLocal;
                    if (TryMapLegacyContactSheetPoint(oldPixel, out legacyImageId, out legacyLocal))
                        return Tuple.Create(legacyImageId, legacyLocal);
                    if (imageTiles.Count == 1)
                        return Tuple.Create(imageTiles[0].ImageId, oldPixel);
                    return null; // Legacy multi-image data without a known tile is ambiguous.
                }

                ImageTileInfo bestTile = null;
                PointF bestLocal = PointF.Empty;
                double bestDistance = double.MaxValue;
                foreach (ImageTileInfo tile in imageTiles)
                {
                    LaserScanImageParameter picture = (CurrentParameters.ScanImages ?? new List<LaserScanImageParameter>())
                        .FirstOrDefault(x => string.Equals(x.Id, tile.ImageId, StringComparison.OrdinalIgnoreCase));
                    if (picture == null || !picture.ServoX.HasValue || !picture.ServoY.HasValue) continue;

                    PointF center = TransformServoAxes(picture.ServoX.Value, picture.ServoY.Value);
                    PointF offset = InverseTransformServoAxes(machineX - center.X, machineY - center.Y);
                    PointF local = new PointF(
                        (float)(tile.SourceWidth / 2.0 + offset.X / mmPerPixel),
                        (float)(tile.SourceHeight / 2.0 - offset.Y / mmPerPixel));
                    if (local.X < 0 || local.Y < 0 || local.X > tile.SourceWidth || local.Y > tile.SourceHeight) continue;

                    double distance = offset.X * offset.X + offset.Y * offset.Y;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestTile = tile;
                        bestLocal = local;
                    }
                }

                if (bestTile == null && imageTiles.Count == 1)
                    return Tuple.Create(imageTiles[0].ImageId, oldPixel);
                return bestTile == null ? null : Tuple.Create(bestTile.ImageId, bestLocal);
            };

            bool migratedAny = false;
            foreach (LaserMarkingParameter marking in CurrentParameters.Markings ?? new List<LaserMarkingParameter>())
            {
                if (!string.IsNullOrWhiteSpace(marking.ImageId)) continue;
                Tuple<string, PointF> migrated = findSourcePoint(new PointF((float)marking.PositionX, (float)marking.PositionY));
                if (migrated == null) continue;
                marking.ImageId = migrated.Item1;
                marking.PositionX = migrated.Item2.X;
                marking.PositionY = migrated.Item2.Y;
                migratedAny = true;
            }

            foreach (LaserFiducialParameter point in CurrentParameters.Fiducials ?? new List<LaserFiducialParameter>())
            {
                if (!string.IsNullOrWhiteSpace(point.ImageId)) continue;
                Tuple<string, PointF> migrated = findSourcePoint(new PointF((float)point.PositionX, (float)point.PositionY));
                if (migrated == null) continue;
                point.ImageId = migrated.Item1;
                point.PositionX = migrated.Item2.X;
                point.PositionY = migrated.Item2.Y;
                migratedAny = true;
            }

            foreach (MarkingItem item in markings.Where(x => string.IsNullOrWhiteSpace(x.ImageId)))
            {
                Tuple<string, PointF> migrated = findSourcePoint(item.Position);
                if (migrated == null) continue;
                item.ImageId = migrated.Item1;
                item.LocalPosition = migrated.Item2;
                item.Position = SourceToCanvas(item.ImageId, item.LocalPosition);
                migratedAny = true;
            }

            foreach (FiducialItem item in fiducials.Where(x => string.IsNullOrWhiteSpace(x.ImageId)))
            {
                Tuple<string, PointF> migrated = findSourcePoint(item.Position);
                if (migrated == null) continue;
                item.ImageId = migrated.Item1;
                item.LocalPosition = migrated.Item2;
                item.Position = SourceToCanvas(item.ImageId, item.LocalPosition);
                migratedAny = true;
            }

            if (migratedAny)
            {
                if (currentSurface == LaserSurface.Top) savedTopParameters = CloneSurface(topParameters);
                else savedBotParameters = CloneSurface(botParameters);
            }
        }

        private void MigrateNormalizedImageLocalCoordinates()
        {
            if (CurrentParameters.ImageLocalCoordinateVersion >= 1)
                return;

            bool changed = false;
            bool allReferencedTilesAvailable = true;
            foreach (LaserMarkingParameter marking in CurrentParameters.Markings ?? new List<LaserMarkingParameter>())
            {
                if (string.IsNullOrWhiteSpace(marking.ImageId)) continue;
                ImageTileInfo tile = FindImageTile(marking.ImageId);
                if (tile == null)
                {
                    allReferencedTilesAvailable = false;
                    continue;
                }

                // The buggy mapping inverted full-image edge vectors, so saved local points
                // from that build are normalized fractions instead of source pixels.
                if (Math.Abs(marking.PositionX) <= 1.001 && Math.Abs(marking.PositionY) <= 1.001)
                {
                    marking.PositionX *= tile.SourceWidth;
                    marking.PositionY *= tile.SourceHeight;
                    changed = true;
                }
            }

            foreach (LaserFiducialParameter point in CurrentParameters.Fiducials ?? new List<LaserFiducialParameter>())
            {
                if (string.IsNullOrWhiteSpace(point.ImageId)) continue;
                ImageTileInfo tile = FindImageTile(point.ImageId);
                if (tile == null)
                {
                    allReferencedTilesAvailable = false;
                    continue;
                }

                if (Math.Abs(point.PositionX) <= 1.001 && Math.Abs(point.PositionY) <= 1.001)
                {
                    point.PositionX *= tile.SourceWidth;
                    point.PositionY *= tile.SourceHeight;
                    changed = true;
                }
            }

            if (allReferencedTilesAvailable)
                CurrentParameters.ImageLocalCoordinateVersion = 1;
            if (changed)
            {
                if (currentSurface == LaserSurface.Top) topParameters = CurrentParameters;
                else botParameters = CurrentParameters;
            }
        }

        private void RepositionImageBoundItems()
        {
            foreach (MarkingItem item in markings)
                if (!string.IsNullOrWhiteSpace(item.ImageId) && FindImageTile(item.ImageId) != null)
                    item.Position = SourceToCanvas(item.ImageId, item.LocalPosition);

            foreach (FiducialItem item in fiducials)
                if (!string.IsNullOrWhiteSpace(item.ImageId) && FindImageTile(item.ImageId) != null)
                    item.Position = SourceToCanvas(item.ImageId, item.LocalPosition);

            UpdateAllMarkingViews();
            UpdateAllFiducialViews();
        }

        private void RebuildSurfaceImage(LaserSurface surface, bool clearWhenNoPictures = false)
        {
            LaserSurfaceParameter parameters = surface == LaserSurface.Top ? topParameters : botParameters;
            bool hadLegacyStitch = parameters.HasStitchCoordinateSystem && parameters.StitchPixelToMM > 0;
            double legacyOriginX = parameters.StitchOriginX;
            double legacyOriginY = parameters.StitchOriginY;
            double legacyPixelToMM = parameters.StitchPixelToMM;
            if (surface == currentSurface) imageTiles.Clear();

            parameters.HasStitchCoordinateSystem = false;
            parameters.StitchPixelToMM = 0;

            List<Tuple<LaserScanImageParameter, Bitmap>> loaded =
                new List<Tuple<LaserScanImageParameter, Bitmap>>();
            try
            {
                foreach (LaserScanImageParameter picture in parameters.ScanImages ?? new List<LaserScanImageParameter>())
                {
                    string path = ResolvePicturePath(picture);
                    if (!File.Exists(path)) continue;
                    try
                    {
                        using (Image source = Image.FromFile(path))
                            loaded.Add(Tuple.Create(picture, new Bitmap(source)));
                    }
                    catch { }
                }

                string outputPath = "";
                if (loaded.Count == 0)
                {
                    if (!clearWhenNoPictures && (parameters.ScanImages == null || parameters.ScanImages.Count == 0))
                        outputPath = surface == LaserSurface.Top ? topImageFileName : botImageFileName;
                }
                else
                {
                    double mmPerPixel = parameters.ImagePixelToMM;
                    if (double.IsNaN(mmPerPixel) || double.IsInfinity(mmPerPixel) || mmPerPixel <= 0)
                        mmPerPixel = 0.02;

                    // Only images with a known capture position can participate in a coordinate-based stitch.
                    List<Tuple<LaserScanImageParameter, Bitmap>> positioned = loaded
                        .Where(x => x.Item1.ServoX.HasValue && x.Item1.ServoY.HasValue)
                        .ToList();

                    if (positioned.Count > 0 && !string.IsNullOrWhiteSpace(modelFolderPath))
                    {
                        double minX = double.MaxValue;
                        double maxX = double.MinValue;
                        double minY = double.MaxValue;
                        double maxY = double.MinValue;

                        foreach (Tuple<LaserScanImageParameter, Bitmap> entry in positioned)
                        {
                            LaserScanImageParameter picture = entry.Item1;
                            Bitmap image = entry.Item2;
                            PointF[] corners =
                            {
                                GetServoImageMachinePoint(parameters, picture, image.Width, image.Height, mmPerPixel, 0, 0),
                                GetServoImageMachinePoint(parameters, picture, image.Width, image.Height, mmPerPixel, image.Width, 0),
                                GetServoImageMachinePoint(parameters, picture, image.Width, image.Height, mmPerPixel, 0, image.Height),
                                GetServoImageMachinePoint(parameters, picture, image.Width, image.Height, mmPerPixel, image.Width, image.Height)
                            };

                            foreach (PointF corner in corners)
                            {
                                minX = Math.Min(minX, corner.X);
                                maxX = Math.Max(maxX, corner.X);
                                minY = Math.Min(minY, corner.Y);
                                maxY = Math.Max(maxY, corner.Y);
                            }
                        }

                        const float margin = 20f;
                        double physicalWidth = Math.Max(0.001, maxX - minX);
                        double physicalHeight = Math.Max(0.001, maxY - minY);
                        double rawWidth = physicalWidth / mmPerPixel;
                        double rawHeight = physicalHeight / mmPerPixel;

                        // Preserve the physical layout while downsampling only the preview when the
                        // servo span would otherwise create an impractically large bitmap.
                        double renderScale = Math.Min(1.0, Math.Min(28000.0 / rawWidth, 28000.0 / rawHeight));
                        double maxAreaScale = Math.Sqrt(100000000.0 / Math.Max(1.0, rawWidth * rawHeight));
                        renderScale = Math.Min(renderScale, maxAreaScale);
                        double pixelsPerMM = renderScale / mmPerPixel;
                        double canvasWidthValue = Math.Ceiling(physicalWidth * pixelsPerMM + margin * 2);
                        double canvasHeightValue = Math.Ceiling(physicalHeight * pixelsPerMM + margin * 2);
                        int canvasWidth = Math.Max(1, (int)canvasWidthValue);
                        int canvasHeight = Math.Max(1, (int)canvasHeightValue);
                        List<ImageTileInfo> newTiles = new List<ImageTileInfo>();

                        using (Bitmap canvas = new Bitmap(canvasWidth, canvasHeight, PixelFormat.Format24bppRgb))
                        using (Graphics graphics = Graphics.FromImage(canvas))
                        using (Pen tilePen = new Pen(Color.FromArgb(90, 130, 150), 1f))
                        {
                            graphics.Clear(Color.Black);
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;

                            foreach (Tuple<LaserScanImageParameter, Bitmap> entry in positioned)
                            {
                                LaserScanImageParameter picture = entry.Item1;
                                Bitmap image = entry.Item2;
                                if (string.IsNullOrWhiteSpace(picture.Id))
                                    picture.Id = Guid.NewGuid().ToString("N");

                                PointF topLeftMachine = GetServoImageMachinePoint(
                                    parameters, picture, image.Width, image.Height, mmPerPixel, 0, 0);
                                PointF topRightMachine = GetServoImageMachinePoint(
                                    parameters, picture, image.Width, image.Height, mmPerPixel, image.Width, 0);
                                PointF bottomLeftMachine = GetServoImageMachinePoint(
                                    parameters, picture, image.Width, image.Height, mmPerPixel, 0, image.Height);
                                PointF bottomRightMachine = GetServoImageMachinePoint(
                                    parameters, picture, image.Width, image.Height, mmPerPixel, image.Width, image.Height);

                                PointF p00 = MachineToStitchPixel(topLeftMachine.X, topLeftMachine.Y, minX, maxY, pixelsPerMM, margin);
                                PointF p10 = MachineToStitchPixel(topRightMachine.X, topRightMachine.Y, minX, maxY, pixelsPerMM, margin);
                                PointF p01 = MachineToStitchPixel(bottomLeftMachine.X, bottomLeftMachine.Y, minX, maxY, pixelsPerMM, margin);
                                PointF p11 = MachineToStitchPixel(bottomRightMachine.X, bottomRightMachine.Y, minX, maxY, pixelsPerMM, margin);

                                PointF[] destination = { p00, p10, p01 };
                                graphics.DrawImage(image, destination);
                                graphics.DrawPolygon(tilePen, new[] { p00, p10, p11, p01 });

                                float left = Math.Min(Math.Min(p00.X, p10.X), Math.Min(p01.X, p11.X));
                                float top = Math.Min(Math.Min(p00.Y, p10.Y), Math.Min(p01.Y, p11.Y));
                                float right = Math.Max(Math.Max(p00.X, p10.X), Math.Max(p01.X, p11.X));
                                float bottom = Math.Max(Math.Max(p00.Y, p10.Y), Math.Max(p01.Y, p11.Y));

                                newTiles.Add(new ImageTileInfo
                                {
                                    ImageId = picture.Id,
                                    DisplayBounds = RectangleF.FromLTRB(left, top, right, bottom),
                                    SourceWidth = image.Width,
                                    SourceHeight = image.Height,
                                    CanvasOrigin = p00,
                                    // Store the canvas displacement of ONE source pixel, not the full image edge.
                                    // CanvasToSource then returns real source-image pixel coordinates.
                                    CanvasXAxis = new PointF((p10.X - p00.X) / image.Width, (p10.Y - p00.Y) / image.Width),
                                    CanvasYAxis = new PointF((p01.X - p00.X) / image.Height, (p01.Y - p00.Y) / image.Height),
                                    Scale = 1f
                                });
                            }

                            string surfaceFolder = surface == LaserSurface.Top ? "TOP" : "BOT";
                            string targetFolder = Path.Combine(modelFolderPath, "LaserPictures", surfaceFolder);
                            Directory.CreateDirectory(targetFolder);
                            outputPath = Path.Combine(targetFolder, "Stitched_" + surfaceFolder + ".png");
                            string temp = outputPath + "." + Guid.NewGuid().ToString("N") + ".tmp.png";
                            canvas.Save(temp, ImageFormat.Png);
                            if (File.Exists(outputPath)) File.Delete(outputPath);
                            File.Move(temp, outputPath);
                        }

                        parameters.HasStitchCoordinateSystem = true;
                        // The stitched bitmap includes a visual margin around the machine-coordinate bounds.
                        // Shift the stored origin so canvas pixel (0,0) still maps to the correct machine point.
                        parameters.StitchOriginX = minX - margin / pixelsPerMM;
                        parameters.StitchOriginY = maxY + margin / pixelsPerMM;
                        parameters.StitchPixelToMM = 1.0 / pixelsPerMM;

                        if (surface == currentSurface)
                        {
                            imageTiles.Clear();
                            imageTiles.AddRange(newTiles);
                            MigrateLegacyPointsToImageTiles(hadLegacyStitch, legacyOriginX, legacyOriginY, legacyPixelToMM);
                        }
                    }
                    else if (positioned.Count == 0)
                    {
                        // Keep the images visible in a contact sheet until their Servo X/Y values are entered.
                        string surfaceFolder = surface == LaserSurface.Top ? "TOP" : "BOT";
                        string targetFolder = Path.Combine(modelFolderPath, "LaserPictures", surfaceFolder);
                        if (!string.IsNullOrWhiteSpace(modelFolderPath))
                        {
                            Directory.CreateDirectory(targetFolder);
                            const int thumbWidth = 480;
                            const int thumbHeight = 360;
                            int columns = Math.Min(3, loaded.Count);
                            int rows = (int)Math.Ceiling(loaded.Count / (double)columns);
                            int cellWidth = thumbWidth + 32;
                            int cellHeight = thumbHeight + 58;
                            using (Bitmap canvas = new Bitmap(columns * cellWidth + 24, rows * cellHeight + 24))
                            using (Graphics graphics = Graphics.FromImage(canvas))
                            using (Font font = new Font("Segoe UI", 9f))
                            using (Brush brush = new SolidBrush(Color.White))
                            {
                                graphics.Clear(Color.FromArgb(32, 32, 32));
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                for (int i = 0; i < loaded.Count; i++)
                                {
                                    Bitmap image = loaded[i].Item2;
                                    LaserScanImageParameter picture = loaded[i].Item1;
                                    if (string.IsNullOrWhiteSpace(picture.Id))
                                        picture.Id = Guid.NewGuid().ToString("N");
                                    int col = i % columns;
                                    int row = i / columns;
                                    int x = 12 + col * cellWidth;
                                    int y = 12 + row * cellHeight;
                                    double factor = Math.Min(thumbWidth / (double)image.Width, thumbHeight / (double)image.Height);
                                    int width = Math.Max(1, (int)Math.Round(image.Width * factor));
                                    int height = Math.Max(1, (int)Math.Round(image.Height * factor));
                                    int drawX = x + (cellWidth - width) / 2;
                                    int drawY = y + 4 + (thumbHeight - height) / 2;
                                    graphics.DrawImage(image, new Rectangle(drawX, drawY, width, height));
                                    graphics.DrawString(Path.GetFileName(picture.ImagePath) + "   (Servo X/Y not set)",
                                        font, brush, x + 6, y + thumbHeight + 12);
                                }
                                outputPath = Path.Combine(targetFolder, "Preview_" + surfaceFolder + ".png");
                                string temp = outputPath + "." + Guid.NewGuid().ToString("N") + ".tmp.png";
                                canvas.Save(temp, ImageFormat.Png);
                                if (File.Exists(outputPath)) File.Delete(outputPath);
                                File.Move(temp, outputPath);
                            }
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(modelFolderPath))
                    {
                        // Some images have positions and others do not: only positioned images are stitched.
                        // The unpositioned images remain available in the picture grid.
                    }
                }

                if (surface == LaserSurface.Top) topImageFileName = outputPath;
                else botImageFileName = outputPath;

                if (currentSurface == surface)
                {
                    LoadSurfaceImage(outputPath);
                    RepositionImageBoundItems();
                    RefreshMarkingRows();
                    RefreshFiducialRows();
                    UpdatePositionControls(selectedMarking);
                    UpdateAllMarkingViews();
                }
            }
            catch (Exception ex)
            {
                if (currentSurface == surface)
                    MessageBox.Show("Could not rebuild the " + (surface == LaserSurface.Top ? "TOP" : "BOT") +
                        " image: " + ex.Message, "Image Stitching", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                foreach (var item in loaded) item.Item2.Dispose();
            }
        }


        private void ScanImageClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(modelFolderPath) || !Directory.Exists(modelFolderPath))
            {
                MessageBox.Show("Please save or call a Model before scanning images.", "Scan Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Action<Action<Bitmap, int, int>> requestHandler = ScanImageRequested;
            if (requestHandler == null)
            {
                MessageBox.Show("Camera scan is not connected to Form1.", "Scan Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LaserSurface surfaceAtRequest = currentSurface;
            string folderAtRequest = modelFolderPath;
            Action<Bitmap, int, int> onCaptured = (capturedImage, servoX, servoY) =>
                SaveScannedImage(capturedImage, servoX, servoY, surfaceAtRequest, folderAtRequest);
            requestHandler(onCaptured);
        }

        private void SaveScannedImage(Bitmap capturedImage, int servoX, int servoY, LaserSurface surface, string modelFolder)
        {
            if (capturedImage == null) return;
            string fullPath = "";
            try
            {
                string surfaceFolder = surface == LaserSurface.Top ? "TOP" : "BOT";
                string relativeFolder = Path.Combine("LaserPictures", surfaceFolder);
                string targetFolder = Path.Combine(modelFolder, relativeFolder);
                Directory.CreateDirectory(targetFolder);

                string fileName = "Scan_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 5) + ".jpg";
                fullPath = Path.Combine(targetFolder, fileName);
                capturedImage.Save(fullPath, ImageFormat.Jpeg);

                LaserScanImageParameter picture = new LaserScanImageParameter
                {
                    ImagePath = Path.Combine(relativeFolder, fileName),
                    ServoX = servoX,
                    ServoY = servoY,
                    CaptureTime = DateTime.Now
                };

                Action updateUi = () =>
                {
                    LaserSurfaceParameter targetParameters = surface == LaserSurface.Top ? topParameters : botParameters;
                    if (targetParameters.ScanImages == null) targetParameters.ScanImages = new List<LaserScanImageParameter>();
                    targetParameters.ScanImages.Add(picture);

                    if (currentSurface == surface)
                    {
                        LoadImage(fullPath);
                        RefreshPictureGrid();
                    }

                    SaveCurrentSurfaceToMemoryIfSelected(surface);
                    SaveOneSnapshotForSurface(surface);
                };

                if (IsDisposed) return;
                if (InvokeRequired) BeginInvoke(updateUi);
                else updateUi();
            }
            catch (Exception ex)
            {
                if (!IsDisposed && IsHandleCreated)
                    BeginInvoke((Action)(() => MessageBox.Show("Could not save scanned image: " + ex.Message, "Scan Image", MessageBoxButtons.OK, MessageBoxIcon.Error)));
            }
            finally
            {
                capturedImage.Dispose();
            }
        }

        private void SaveCurrentSurfaceToMemoryIfSelected(LaserSurface surface)
        {
            if (currentSurface == surface)
            {
                SaveCurrentSurfaceToMemory();
                RebindPictureGridTags();
            }
        }

        private void SaveOneSnapshotForSurface(LaserSurface surface)
        {
            if (surface == LaserSurface.Top) savedTopParameters = CloneSurface(topParameters);
            else savedBotParameters = CloneSurface(botParameters);
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
            string imageId;
            PointF localPoint;
            if (!TryGetImageTilePoint(clientPoint, out imagePoint, out imageId, out localPoint)) return;
            string Rotate = GetMarkingRotation(type).ToString().Replace("f", "");
            AddMarking(type, imagePoint, Rotate, imageId, localPoint);
        }

        private bool TryGetImagePoint(Point viewportPoint, out PointF imagePoint)
        {
            string imageId;
            PointF localPoint;
            return TryGetImageTilePoint(viewportPoint, out imagePoint, out imageId, out localPoint);
        }

        private bool TryGetImageTilePoint(Point viewportPoint, out PointF canvasPoint, out string imageId, out PointF localPoint)
        {
            canvasPoint = PointF.Empty;
            imageId = "";
            localPoint = PointF.Empty;
            if (markingImage.Image == null || markingImage.Width <= 0 || markingImage.Height <= 0) return false;

            Point point = markingImage.PointToClient(imageViewport.PointToScreen(viewportPoint));
            if (point.X < 0 || point.Y < 0 || point.X > markingImage.Width || point.Y > markingImage.Height) return false;
            canvasPoint = new PointF(point.X / imageZoom, point.Y / imageZoom);

            PointF lookupPoint = canvasPoint;
            ImageTileInfo tile = imageTiles.LastOrDefault(x => x.DisplayBounds.Contains(lookupPoint));
            if (tile == null) return false;

            PointF candidate = CanvasToSource(tile.ImageId, canvasPoint);
            if (candidate.X < 0 || candidate.Y < 0 ||
                candidate.X > tile.SourceWidth || candidate.Y > tile.SourceHeight)
                return false;

            imageId = tile.ImageId;
            localPoint = candidate;
            return true;
        }

        private bool TryFindTileAtCanvasPoint(PointF canvasPoint, out ImageTileInfo destination, out PointF localPoint)
        {
            destination = null;
            localPoint = PointF.Empty;

            // Iterate backwards because later tiles are painted on top of earlier ones.
            for (int i = imageTiles.Count - 1; i >= 0; i--)
            {
                ImageTileInfo tile = imageTiles[i];
                if (!tile.DisplayBounds.Contains(canvasPoint))
                    continue;

                PointF candidate = CanvasToSource(tile.ImageId, canvasPoint);
                if (candidate.X < 0 || candidate.Y < 0 ||
                    candidate.X > tile.SourceWidth || candidate.Y > tile.SourceHeight)
                    continue;

                destination = tile;
                localPoint = candidate;
                return true;
            }

            return false;
        }

        private void UpdateMarkingImageBinding(MarkingItem item)
        {
            if (item == null) return;

            ImageTileInfo destination;
            PointF local;
            if (TryFindTileAtCanvasPoint(item.Position, out destination, out local))
            {
                item.ImageId = destination.ImageId;
                item.LocalPosition = local;
            }
            else
            {
                // A point in the gap between tiles belongs to the stitched canvas itself.
                // GetMarkingCoordinates(item.Position) converts it through the stitch origin.
                item.ImageId = "";
                item.LocalPosition = item.Position;
            }
        }

        private void UpdateFiducialImageBinding(FiducialItem item)
        {
            if (item == null) return;

            ImageTileInfo destination;
            PointF local;
            if (TryFindTileAtCanvasPoint(item.Position, out destination, out local))
            {
                item.ImageId = destination.ImageId;
                item.LocalPosition = local;
            }
            else
            {
                item.ImageId = "";
                item.LocalPosition = item.Position;
            }
        }

        private ImageTileInfo FindImageTile(string imageId)
        {
            if (string.IsNullOrWhiteSpace(imageId)) return null;
            return imageTiles.FirstOrDefault(x => string.Equals(x.ImageId, imageId, StringComparison.OrdinalIgnoreCase));
        }

        private PointF SourceToCanvas(string imageId, PointF sourcePoint)
        {
            ImageTileInfo tile = FindImageTile(imageId);
            if (tile == null) return sourcePoint;
            return new PointF(
                tile.CanvasOrigin.X + sourcePoint.X * tile.CanvasXAxis.X + sourcePoint.Y * tile.CanvasYAxis.X,
                tile.CanvasOrigin.Y + sourcePoint.X * tile.CanvasXAxis.Y + sourcePoint.Y * tile.CanvasYAxis.Y);
        }

        private PointF CanvasToSource(string imageId, PointF canvasPoint)
        {
            ImageTileInfo tile = FindImageTile(imageId);
            if (tile == null) return canvasPoint;

            double dx = canvasPoint.X - tile.CanvasOrigin.X;
            double dy = canvasPoint.Y - tile.CanvasOrigin.Y;
            double determinant = tile.CanvasXAxis.X * tile.CanvasYAxis.Y -
                                 tile.CanvasYAxis.X * tile.CanvasXAxis.Y;
            if (Math.Abs(determinant) < 0.0000001) return PointF.Empty;

            double sourceX = (dx * tile.CanvasYAxis.Y - dy * tile.CanvasYAxis.X) / determinant;
            double sourceY = (tile.CanvasXAxis.X * dy - tile.CanvasXAxis.Y * dx) / determinant;
            return new PointF((float)sourceX, (float)sourceY);
        }
        //Thêm Label laser

        private void AddMarking(string type, PointF position, string Rotate, string imageId, PointF localPosition)
        {
            string normalizedType = NormalizeMarkingType(type);
            AddMarking(normalizedType, position, nextMarkingId++, "0", normalizedType, "", Rotate, imageId, localPosition);
        }

        private void AddMarking(string type, PointF position, int id, string product, string printType, string content, string Rotate)
        {
            AddMarking(type, position, id, product, printType, content, Rotate, "", position);
        }

        private void AddMarking(string type, PointF position, int id, string product, string printType, string content, string Rotate, string imageId, PointF localPosition)
        {
            type = NormalizeMarkingType(type);
            printType = NormalizeMarkingType(string.IsNullOrWhiteSpace(printType) ? type : printType);
            MarkingItem item = new MarkingItem
            {
                Id = id,
                ImageId = imageId ?? "",
                LocalPosition = localPosition,
                Position = string.IsNullOrWhiteSpace(imageId) ? position : SourceToCanvas(imageId, localPosition),
                Product = product ?? "0",
                Type = type,
                PrintType = printType,
                Content = content ?? "",
                Rotate = Rotate
            };
            nextMarkingId = Math.Max(nextMarkingId, id + 1);
            markings.Add(item);
            PointF machinePosition = GetMarkingCoordinates(item);
            int row = markingList.Rows.Add(GetMarkingNumber(item), item.Product, item.Type,
                FormatCoordinate(machinePosition.X), FormatCoordinate(machinePosition.Y));
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

            imagePan = new Point(imagePan.X + deltaX, imagePan.Y + deltaY);
            UpdateImagePanOnly();
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
                // Keep the label in stitched-canvas coordinates while dragging.
                // Do not clamp it to the source image that originally owned it.
                item.Position = new PointF(original.Value.X + deltaX, original.Value.Y + deltaY);
                UpdateMarkingImageBinding(item);
                UpdateMarkingView(item);
                UpdateMarkingRow(item);
            }
            UpdatePositionControls(selectedMarking);
        }

        private void StopMoveMarkingView(object sender, MouseEventArgs e)
        {
            Label view = (Label)sender;
            object[] state = view.Tag as object[];
            Dictionary<MarkingItem, PointF> originals = state != null && state.Length > 2
                ? state[2] as Dictionary<MarkingItem, PointF>
                : null;

            view.Capture = false;
            view.MouseMove -= MoveMarkingView;
            view.MouseUp -= StopMoveMarkingView;
            view.Tag = state != null && state.Length > 0 ? state[0] : view.Tag;

            if (originals != null)
            {
                foreach (MarkingItem item in originals.Keys)
                {
                    UpdateMarkingImageBinding(item);
                    UpdateMarkingView(item);
                    UpdateMarkingRow(item);
                }
                UpdatePositionControls(selectedMarking);
                SaveCurrentSurfaceToMemory();
            }
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
                    ImageId = item.ImageId,
                    LocalPosition = item.LocalPosition,
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
                    ImageId = copiedMarking.ImageId,
                    LocalPosition = copiedMarking.LocalPosition,
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

                int row = markingList.Rows.Add(GetMarkingNumber(pasted), pasted.Product, pasted.Type,
                    FormatCoordinate(GetMarkingCoordinates(pasted).X),
                    FormatCoordinate(GetMarkingCoordinates(pasted).Y));
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
            SetMarkingMachineCoordinates(selectedMarking, (float)positionX.Value, (float)positionY.Value);
            ClampPosition(selectedMarking);
            UpdateMarkingView(selectedMarking);
            UpdateMarkingRow(selectedMarking);
        }

        private void UpdatePositionControls(MarkingItem item)
        {
            bool hasMachineOrigin = item == null || string.IsNullOrWhiteSpace(item.ImageId);
            if (item != null && !string.IsNullOrWhiteSpace(item.ImageId))
            {
                LaserScanImageParameter picture = (CurrentParameters.ScanImages ?? new List<LaserScanImageParameter>())
                    .FirstOrDefault(x => string.Equals(x.Id, item.ImageId, StringComparison.OrdinalIgnoreCase));
                hasMachineOrigin = picture != null && picture.ServoX.HasValue && picture.ServoY.HasValue;
            }

            positionX.Enabled = hasMachineOrigin;
            positionY.Enabled = hasMachineOrigin;
            if (item == null) return;

            updatingPositionControls = true;
            try
            {
                PointF machinePosition = GetMarkingCoordinates(item);
                positionX.Value = ClampValue(positionX, machinePosition.X);
                positionY.Value = ClampValue(positionY, machinePosition.Y);
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
            if (float.IsNaN(value) || float.IsInfinity(value)) return control.Minimum;
            return Math.Max(control.Minimum, Math.Min(control.Maximum, (decimal)value));
        }

        private void UpdateMarkingView(MarkingItem item)
        {
            if (item.View == null) return;
            bool vertical = item.Type == "TOPTEXT" || item.Type == "BOTTOMTEXT";
            string displayText = string.IsNullOrWhiteSpace(item.Content) ? item.Type : item.Content;
            Font previousFont = item.View.Font;
            item.View.Font = new Font("Times New Roman", Math.Max(1f, MarkingFontSize * imageZoom), FontStyle.Regular);
            if (previousFont != null && !ReferenceEquals(previousFont, Control.DefaultFont))
                previousFont.Dispose();
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
            if (!string.IsNullOrWhiteSpace(item.ImageId))
            {
                ImageTileInfo tile = FindImageTile(item.ImageId);
                if (tile != null)
                {
                    item.Position = new PointF(Math.Max(tile.DisplayBounds.Left, Math.Min(tile.DisplayBounds.Right, item.Position.X)),
                                               Math.Max(tile.DisplayBounds.Top, Math.Min(tile.DisplayBounds.Bottom, item.Position.Y)));
                    item.LocalPosition = CanvasToSource(item.ImageId, item.Position);
                    item.LocalPosition = new PointF(Math.Max(0, Math.Min(tile.SourceWidth, item.LocalPosition.X)),
                                                    Math.Max(0, Math.Min(tile.SourceHeight, item.LocalPosition.Y)));
                    item.Position = SourceToCanvas(item.ImageId, item.LocalPosition);
                    return;
                }
            }
            item.Position = new PointF(Math.Max(0, Math.Min(markingImage.Image.Width, item.Position.X)), Math.Max(0, Math.Min(markingImage.Image.Height, item.Position.Y)));
        }

        private static string FormatCoordinate(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? "" : value.ToString("0.00");
        }

        // Image-local source pixels are the persistent coordinates for each image.
        // Servo X/Y identify the image centre at capture time; ImagePixelToMM is the sole scale.
        private PointF TransformServoAxes(double x, double y)
        {
            return TransformServoAxes(CurrentParameters, x, y);
        }

        private static PointF TransformServoAxes(LaserSurfaceParameter parameters, double x, double y)
        {
            if (parameters != null && parameters.EnableServoAxisTransform)
            {
                if (parameters.SwapServoAxes) { double temp = x; x = y; y = temp; }
                if (parameters.ReverseServoX) x = -x;
                if (parameters.ReverseServoY) y = -y;
            }
            return new PointF((float)x, (float)y);
        }

        private static PointF GetServoImageMachinePoint(
            LaserSurfaceParameter parameters, LaserScanImageParameter picture,
            int imageWidth, int imageHeight, double mmPerPixel, double sourceX, double sourceY)
        {
            PointF center = TransformServoAxes(parameters, picture.ServoX.Value, picture.ServoY.Value);
            PointF offset = TransformServoAxes(parameters,
                (sourceX - imageWidth / 2.0) * mmPerPixel,
                -(sourceY - imageHeight / 2.0) * mmPerPixel);
            return new PointF(center.X + offset.X, center.Y + offset.Y);
        }

        private static PointF MachineToStitchPixel(
            double machineX, double machineY, double minX, double maxY,
            double pixelsPerMM, float margin)
        {
            return new PointF(
                (float)(margin + (machineX - minX) * pixelsPerMM),
                (float)(margin + (maxY - machineY) * pixelsPerMM));
        }

        private PointF InverseTransformServoAxes(double x, double y)
        {
            LaserSurfaceParameter parameters = CurrentParameters;
            if (parameters.EnableServoAxisTransform)
            {
                if (parameters.ReverseServoX) x = -x;
                if (parameters.ReverseServoY) y = -y;
                if (parameters.SwapServoAxes) { double temp = x; x = y; y = temp; }
            }
            return new PointF((float)x, (float)y);
        }

        private PointF GetImageMachineCoordinates(string imageId, PointF sourcePixel)
        {
            LaserScanImageParameter picture = (CurrentParameters.ScanImages ?? new List<LaserScanImageParameter>())
                .FirstOrDefault(x => string.Equals(x.Id, imageId, StringComparison.OrdinalIgnoreCase));
            ImageTileInfo tile = FindImageTile(imageId);
            if (picture == null || tile == null || !picture.ServoX.HasValue || !picture.ServoY.HasValue)
                return new PointF(float.NaN, float.NaN);

            double mmPerPixel = Math.Max(0.0000001, CurrentParameters.ImagePixelToMM);
            PointF center = TransformServoAxes(picture.ServoX.Value, picture.ServoY.Value);
            PointF offset = TransformServoAxes(
                (sourcePixel.X - tile.SourceWidth / 2.0) * mmPerPixel,
                -(sourcePixel.Y - tile.SourceHeight / 2.0) * mmPerPixel);
            return new PointF(center.X + offset.X, center.Y + offset.Y);
        }

        private PointF MachineToImageSourcePixel(string imageId, float machineX, float machineY)
        {
            LaserScanImageParameter picture = (CurrentParameters.ScanImages ?? new List<LaserScanImageParameter>())
                .FirstOrDefault(x => string.Equals(x.Id, imageId, StringComparison.OrdinalIgnoreCase));
            ImageTileInfo tile = FindImageTile(imageId);
            if (picture == null || tile == null || !picture.ServoX.HasValue || !picture.ServoY.HasValue)
                return new PointF(ToPixel(machineX), ToPixel(machineY));

            double mmPerPixel = Math.Max(0.0000001, CurrentParameters.ImagePixelToMM);
            PointF center = TransformServoAxes(picture.ServoX.Value, picture.ServoY.Value);
            PointF offset = InverseTransformServoAxes(machineX - center.X, machineY - center.Y);
            return new PointF((float)(tile.SourceWidth / 2.0 + offset.X / mmPerPixel),
                              (float)(tile.SourceHeight / 2.0 - offset.Y / mmPerPixel));
        }

        private PointF GetMarkingCoordinates(MarkingItem item)
        {
            if (item == null) return PointF.Empty;
            return string.IsNullOrWhiteSpace(item.ImageId)
                ? GetMarkingCoordinates(item.Position)
                : GetImageMachineCoordinates(item.ImageId, item.LocalPosition);
        }

        private PointF GetFiducialCoordinates(FiducialItem item)
        {
            if (item == null) return PointF.Empty;
            return string.IsNullOrWhiteSpace(item.ImageId)
                ? GetMarkingCoordinates(item.Position)
                : GetImageMachineCoordinates(item.ImageId, item.LocalPosition);
        }

        private void SetMarkingMachineCoordinates(MarkingItem item, float machineX, float machineY)
        {
            if (item == null) return;
            if (string.IsNullOrWhiteSpace(item.ImageId))
            {
                item.Position = MachineToImagePixel(machineX, machineY);
                return;
            }
            item.LocalPosition = MachineToImageSourcePixel(item.ImageId, machineX, machineY);
            item.Position = SourceToCanvas(item.ImageId, item.LocalPosition);
        }

        private void SetFiducialMachineCoordinates(FiducialItem item, float machineX, float machineY)
        {
            if (item == null) return;
            if (string.IsNullOrWhiteSpace(item.ImageId))
            {
                item.Position = MachineToImagePixel(machineX, machineY);
                return;
            }
            item.LocalPosition = MachineToImageSourcePixel(item.ImageId, machineX, machineY);
            item.Position = SourceToCanvas(item.ImageId, item.LocalPosition);
        }

        private PointF GetMarkingCoordinates(PointF imagePixel)
        {
            LaserSurfaceParameter parameters = CurrentParameters;
            if (parameters.HasStitchCoordinateSystem && parameters.StitchPixelToMM > 0)
                return new PointF((float)(parameters.StitchOriginX + imagePixel.X * parameters.StitchPixelToMM),
                                  (float)(parameters.StitchOriginY - imagePixel.Y * parameters.StitchPixelToMM));
            return new PointF(ToMillimeter(imagePixel.X), ToMillimeter(imagePixel.Y));
        }

        private PointF MachineToImagePixel(float machineX, float machineY)
        {
            LaserSurfaceParameter parameters = CurrentParameters;
            if (parameters.HasStitchCoordinateSystem && parameters.StitchPixelToMM > 0)
                return new PointF((float)((machineX - parameters.StitchOriginX) / parameters.StitchPixelToMM),
                                  (float)((parameters.StitchOriginY - machineY) / parameters.StitchPixelToMM));
            return new PointF(ToPixel(machineX), ToPixel(machineY));
        }

        private static bool TryParseCoordinate(object value, out float coordinate)
        {
            string text = Convert.ToString(value).Trim();
            double parsed;
            bool valid = double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed) ||
                         double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);
            coordinate = valid && !double.IsNaN(parsed) && !double.IsInfinity(parsed) ? (float)parsed : 0f;
            return valid && !double.IsNaN(parsed) && !double.IsInfinity(parsed) &&
                   parsed <= float.MaxValue && parsed >= float.MinValue;
        }

        private void MarkingListCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (loadingSurfaceControls || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            DataGridViewRow row = markingList.Rows[e.RowIndex];
            MarkingItem item = row.Tag as MarkingItem;
            if (item == null) return;

            string column = markingList.Columns[e.ColumnIndex].Name;
            if (column != "X" && column != "Y") return;

            float value;
            if (!TryParseCoordinate(row.Cells[e.ColumnIndex].Value, out value))
            {
                UpdateMarkingRow(item);
                return;
            }

            PointF machine = GetMarkingCoordinates(item);
            if (float.IsNaN(machine.X) || float.IsNaN(machine.Y))
            {
                MessageBox.Show("Set Servo X and Servo Y for the source picture before editing machine coordinates.",
                    "Servo Position Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateMarkingRow(item);
                return;
            }
            if (column == "X") machine.X = value;
            else machine.Y = value;

            SetMarkingMachineCoordinates(item, machine.X, machine.Y);
            ClampPosition(item);
            UpdateMarkingView(item);
            UpdatePositionControls(item);
            UpdateMarkingRow(item);
            SaveCurrentSurfaceToMemory();
        }

        private void FiducialListCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (loadingSurfaceControls || e.RowIndex < 0 || e.ColumnIndex < 1 || e.ColumnIndex > 2) return;
            DataGridViewRow row = fiducialList.Rows[e.RowIndex];
            FiducialItem item = row.Tag as FiducialItem;
            if (item == null) return;

            float value;
            if (!TryParseCoordinate(row.Cells[e.ColumnIndex].Value, out value))
            {
                UpdateFiducialRow(item);
                return;
            }

            PointF machine = GetFiducialCoordinates(item);
            if (float.IsNaN(machine.X) || float.IsNaN(machine.Y))
            {
                MessageBox.Show("Set Servo X and Servo Y for the source picture before editing machine coordinates.",
                    "Servo Position Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateFiducialRow(item);
                return;
            }
            if (e.ColumnIndex == 1) machine.X = value;
            else machine.Y = value;

            SetFiducialMachineCoordinates(item, machine.X, machine.Y);
            ClampFiducial(item);
            UpdateFiducialView(item);
            UpdateFiducialRow(item);
            SaveCurrentSurfaceToMemory();
        }

        private void RefreshFiducialRows()
        {
            foreach (FiducialItem item in fiducials)
                UpdateFiducialRow(item);
        }

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
            RefreshFiducialRows();
            UpdatePositionControls(selectedMarking);

            if (ReferenceEquals(sender, imagePixelToMmValue) && !loadingSurfaceControls)
            {
                double scale;
                if (double.TryParse(imagePixelToMmValue.Text, NumberStyles.Float,
                    CultureInfo.CurrentCulture, out scale) ||
                    double.TryParse(imagePixelToMmValue.Text, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out scale))
                {
                    if (scale > 0 && !double.IsNaN(scale) && !double.IsInfinity(scale))
                    {
                        SaveCurrentSurfaceToMemory();
                        RebuildSurfaceImage(currentSurface);
                    }
                }
            }
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
                    PointF machinePosition = GetMarkingCoordinates(item);
                    row.Cells["X"].Value = FormatCoordinate(machinePosition.X);
                    row.Cells["Y"].Value = FormatCoordinate(machinePosition.Y);
                    break;
                }
        }

        private void RefreshMarkingRows()
        {
            foreach (MarkingItem item in markings)
                UpdateMarkingRow(item);
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

            // Keep the pixel under the cursor fixed throughout the animation.
            Point imagePoint = markingImage.PointToClient(imageViewport.PointToScreen(viewportPoint));
            zoomAnchorImageX = imagePoint.X / imageZoom;
            zoomAnchorImageY = imagePoint.Y / imageZoom;
            zoomAnchor = viewportPoint;

            float startingZoom = zoomAnimationTimer.Enabled ? targetZoom : imageZoom;
            float zoomFactor = (float)Math.Pow(1.15, wheelDelta / 120.0);
            targetZoom = Math.Max(0.05f, Math.Min(12f, startingZoom * zoomFactor));
            if (Math.Abs(targetZoom - imageZoom) < 0.0001f)
            {
                zoomAnimationTimer.Stop();
                return;
            }

            if (!zoomAnimationTimer.Enabled)
                zoomAnimationTimer.Start();
        }

        private void ZoomAnimationTick(object sender, EventArgs e)
        {
            float difference = targetZoom - imageZoom;
            float nextZoom = Math.Abs(difference) < 0.001f
                ? targetZoom
                : imageZoom + difference * 0.28f;

            ApplyZoomAt(zoomAnchor, zoomAnchorImageX, zoomAnchorImageY, nextZoom);

            if (Math.Abs(targetZoom - imageZoom) < 0.001f)
            {
                ApplyZoomAt(zoomAnchor, zoomAnchorImageX, zoomAnchorImageY, targetZoom);
                zoomAnimationTimer.Stop();
            }
        }

        private void ApplyZoomAt(Point viewportPoint, float imageX, float imageY, float newZoom)
        {
            imageZoom = newZoom;

            // Keep the image pixel under the cursor at exactly the same viewport position.
            int scaledWidth = Math.Max(100, (int)(markingImage.Image.Width * imageZoom));
            int scaledHeight = Math.Max(100, (int)(markingImage.Image.Height * imageZoom));
            int baseLeft = (imageViewport.ClientSize.Width - scaledWidth) / 2;
            int baseTop = (imageViewport.ClientSize.Height - scaledHeight) / 2;
            imagePan = new Point(
                (int)Math.Round(viewportPoint.X - imageX * imageZoom - baseLeft),
                (int)Math.Round(viewportPoint.Y - imageY * imageZoom - baseTop));
            UpdateImageTransform();
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
            if (zoomAnimationTimer != null)
                zoomAnimationTimer.Stop();

            if (markingImage.Image == null)
            {
                imageZoom = 1f;
                targetZoom = imageZoom;
                UpdateImageTransform();
                return;
            }

            int viewportWidth = Math.Max(1, imageViewport.ClientSize.Width);
            int viewportHeight = Math.Max(1, imageViewport.ClientSize.Height);
            imageZoom = Math.Min((float)viewportWidth / markingImage.Image.Width, (float)viewportHeight / markingImage.Image.Height);
            imageZoom = Math.Max(0.05f, Math.Min(12f, imageZoom));
            targetZoom = imageZoom;
            imagePan = Point.Empty;
            UpdateImageTransform();
        }

        private void UpdateImageTransform()
        {
            if (markingImage.Image == null)
            {
                markingImage.Location = Point.Empty;
                imageViewport.Invalidate();
                UpdateZoomLabel();
                return;
            }

            markingImage.SizeMode = PictureBoxSizeMode.StretchImage;
            int scaledWidth = Math.Max(100, (int)Math.Round(markingImage.Image.Width * imageZoom));
            int scaledHeight = Math.Max(100, (int)Math.Round(markingImage.Image.Height * imageZoom));
            if (markingImage.Width != scaledWidth) markingImage.Width = scaledWidth;
            if (markingImage.Height != scaledHeight) markingImage.Height = scaledHeight;

            int centeredLeft = (imageViewport.ClientSize.Width - markingImage.Width) / 2;
            int centeredTop = (imageViewport.ClientSize.Height - markingImage.Height) / 2;
            markingImage.Location = new Point(centeredLeft + imagePan.X, centeredTop + imagePan.Y);
            UpdateAllMarkingViews();
            UpdateZoomLabel();
        }

        // Panning only changes the canvas origin. Do not resize the bitmap or
        // recalculate fonts/QR controls on every mouse-move message.
        private void UpdateImagePanOnly()
        {
            if (markingImage.Image == null) return;

            int centeredLeft = (imageViewport.ClientSize.Width - markingImage.Width) / 2;
            int centeredTop = (imageViewport.ClientSize.Height - markingImage.Height) / 2;
            markingImage.Location = new Point(centeredLeft + imagePan.X, centeredTop + imagePan.Y);

            foreach (MarkingItem item in markings)
                UpdateMarkingViewPositionOnly(item);
        }

        private void UpdateMarkingViewPositionOnly(MarkingItem item)
        {
            if (item == null || item.View == null) return;
            item.View.Left = markingImage.Left + (int)Math.Round(item.Position.X * imageZoom) - item.View.Width / 2;
            item.View.Top = markingImage.Top + (int)Math.Round(item.Position.Y * imageZoom) - item.View.Height / 2;
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
            string imageId;
            PointF localPoint;
            if (!TryGetImageTilePoint(viewportPoint, out imagePoint, out imageId, out localPoint)) return;

            AddFiducial(imagePoint, nextFiducialId++, imageId, localPoint);
            addingFiducial = false;
            addFiducial.Text = "Add";
            addFiducial.BackColor = SystemColors.Control;
            markingImage.Cursor = Cursors.Default;
            imageViewport.Cursor = Cursors.Default;
        }

        private void AddFiducial(PointF position, int id)
        {
            AddFiducial(position, id, "", position);
        }

        private void AddFiducial(PointF position, int id, string imageId, PointF localPosition)
        {
            nextFiducialId = Math.Max(nextFiducialId, id + 1);
            FiducialItem item = new FiducialItem
            {
                Id = id,
                ImageId = imageId ?? "",
                LocalPosition = localPosition,
                Position = string.IsNullOrWhiteSpace(imageId) ? position : SourceToCanvas(imageId, localPosition)
            };
            fiducials.Add(item);
            PointF servoPosition = GetFiducialCoordinates(item);
            int row = fiducialList.Rows.Add(item.Id.ToString(), FormatCoordinate(servoPosition.X), FormatCoordinate(servoPosition.Y));
            fiducialList.Rows[row].Tag = item;
            item.View = new FiducialLabel { AutoSize = false, Size = new Size(48, 24), Text = "F" + item.Id, Font = new Font(Font.FontFamily, 9f, FontStyle.Bold), ForeColor = Color.Red, BackColor = Color.Transparent, Tag = item, Cursor = Cursors.Hand };
            item.View.MouseDown += FiducialMouseDown;
            item.View.MouseWheel += MarkingImageMouseWheel;
            item.View.MouseEnter += ImageAreaMouseEnter;
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
                // Fiducials can also move freely between stitched image tiles.
                item.Position = new PointF(original.X + (current.X - start.X) / imageZoom, original.Y + (current.Y - start.Y) / imageZoom);
                UpdateFiducialImageBinding(item);
                UpdateFiducialView(item);
                UpdateFiducialRow(item);
            };
            up = (s, args) =>
            {
                item.View.Capture = false;
                item.View.MouseMove -= move;
                item.View.MouseUp -= up;
                UpdateFiducialImageBinding(item);
                UpdateFiducialView(item);
                UpdateFiducialRow(item);
                SaveCurrentSurfaceToMemory();
            };
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
        private void ClampFiducial(FiducialItem item)
        {
            if (markingImage.Image == null) return;
            if (!string.IsNullOrWhiteSpace(item.ImageId))
            {
                ImageTileInfo tile = FindImageTile(item.ImageId);
                if (tile != null)
                {
                    item.Position = new PointF(Math.Max(tile.DisplayBounds.Left, Math.Min(tile.DisplayBounds.Right, item.Position.X)),
                                               Math.Max(tile.DisplayBounds.Top, Math.Min(tile.DisplayBounds.Bottom, item.Position.Y)));
                    item.LocalPosition = CanvasToSource(item.ImageId, item.Position);
                    item.LocalPosition = new PointF(Math.Max(0, Math.Min(tile.SourceWidth, item.LocalPosition.X)),
                                                    Math.Max(0, Math.Min(tile.SourceHeight, item.LocalPosition.Y)));
                    item.Position = SourceToCanvas(item.ImageId, item.LocalPosition);
                    return;
                }
            }
            item.Position = new PointF(Math.Max(0, Math.Min(markingImage.Image.Width, item.Position.X)),
                                       Math.Max(0, Math.Min(markingImage.Image.Height, item.Position.Y)));
        }
        private void UpdateFiducialRow(FiducialItem item)
        {
            PointF servoPosition = GetFiducialCoordinates(item);
            foreach (DataGridViewRow row in fiducialList.Rows)
                if (row.Tag == item)
                {
                    row.Cells[1].Value = FormatCoordinate(servoPosition.X);
                    row.Cells[2].Value = FormatCoordinate(servoPosition.Y);
                    break;
                }
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
                PointF machinePosition = GetMarkingCoordinates(item);
                int row = markingList.Rows.Add(GetMarkingNumber(item), item.Product, item.Type,
                    FormatCoordinate(machinePosition.X), FormatCoordinate(machinePosition.Y));
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
                ClampPosition(item);
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
                ClampPosition(ordered[i]);
                UpdateMarkingView(ordered[i]);
                UpdateMarkingRow(ordered[i]);
            }
            UpdatePositionControls(selectedMarking);
        }
    }
}
