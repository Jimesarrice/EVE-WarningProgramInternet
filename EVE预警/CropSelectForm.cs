using System.Drawing;
using System.Windows.Forms;

namespace EVE预警
{
    /// <summary>
    /// 框选裁切窗口 - 显示完整图片并允许用户拖动框选裁切区域
    /// </summary>
    public partial class CropSelectForm : Form
    {
        private Bitmap sourceImage;
        private Point cropStartPoint;
        private Point cropEndPoint;
        private bool isDragging;
        private Rectangle selectionRect;

        // 返回的裁切结果
        public int StartX { get; private set; }
        public int StartY { get; private set; }
        public int EndX { get; private set; }
        public int EndY { get; private set; }
        public bool Confirmed { get; private set; }

        public CropSelectForm(Bitmap image)
        {
            sourceImage = image;
            InitializeComponent();
            InitializeCustomSettings();
        }

        private void InitializeComponent()
        {
            this.pictureBox = new PictureBox();
            this.panelButtons = new Panel();
            this.buttonConfirm = new Button();
            this.buttonCancel = new Button();
            this.labelInfo = new Label();
            ((System.ComponentModel.ISupportInitialize)this.pictureBox).BeginInit();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();

            // pictureBox
            this.pictureBox.BackColor = Color.Black;
            this.pictureBox.Dock = DockStyle.Fill;
            this.pictureBox.Location = new Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new Size(800, 600);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.MouseDown += PictureBox_MouseDown;
            this.pictureBox.MouseMove += PictureBox_MouseMove;
            this.pictureBox.MouseUp += PictureBox_MouseUp;
            this.pictureBox.Paint += PictureBox_Paint;

            // panelButtons
            this.panelButtons.Controls.Add(this.buttonConfirm);
            this.panelButtons.Controls.Add(this.buttonCancel);
            this.panelButtons.Controls.Add(this.labelInfo);
            this.panelButtons.Dock = DockStyle.Bottom;
            this.panelButtons.Location = new Point(0, 561);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new Size(800, 50);
            this.panelButtons.TabIndex = 1;

            // buttonConfirm
            this.buttonConfirm.Location = new Point(580, 10);
            this.buttonConfirm.Name = "buttonConfirm";
            this.buttonConfirm.Size = new Size(100, 30);
            this.buttonConfirm.TabIndex = 2;
            this.buttonConfirm.Text = "确认裁切";
            this.buttonConfirm.UseVisualStyleBackColor = true;
            this.buttonConfirm.Click += ButtonConfirm_Click;

            // buttonCancel
            this.buttonCancel.Location = new Point(690, 10);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(100, 30);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += ButtonCancel_Click;

            // labelInfo
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new Point(10, 15);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new Size(200, 20);
            this.labelInfo.TabIndex = 4;
            this.labelInfo.Text = "请拖动鼠标选择裁切区域";

            // CropSelectForm
            this.AutoScaleDimensions = new SizeF(9F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 611);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.panelButtons);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CropSelectForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "选择裁切区域";
            this.Load += CropSelectForm_Load;
            ((System.ComponentModel.ISupportInitialize)this.pictureBox).EndInit();
            this.panelButtons.ResumeLayout(false);
            this.panelButtons.PerformLayout();
            this.ResumeLayout(false);
        }

        private void InitializeCustomSettings()
        {
            selectionRect = Rectangle.Empty;
            isDragging = false;
            Confirmed = false;
            StartX = 0;
            StartY = 0;
            EndX = sourceImage.Width;
            EndY = sourceImage.Height;
        }

        private void CropSelectForm_Load(object? sender, EventArgs e)
        {
            // 根据图片大小调整窗口
            int maxWidth = 1200;
            int maxHeight = 800;

            int windowWidth = Math.Min(sourceImage.Width + 20, maxWidth);
            int windowHeight = Math.Min(sourceImage.Height + 100, maxHeight);

            this.Size = new Size(windowWidth, windowHeight);

            // 计算缩放比例以适应窗口
            DisplayImageWithScaling();
        }

        private void DisplayImageWithScaling()
        {
            if (sourceImage == null) return;

            // 计算可用区域
            int availableWidth = this.ClientSize.Width;
            int availableHeight = this.ClientSize.Height - panelButtons!.Height;

            // 计算缩放比例
            double scaleX = (double)availableWidth / sourceImage.Width;
            double scaleY = (double)availableHeight / sourceImage.Height;
            double scale = Math.Min(scaleX, scaleY);

            // 限制最大缩放为1（不放大，只缩小）
            scale = Math.Min(scale, 1.0);

            int displayWidth = (int)(sourceImage.Width * scale);
            int displayHeight = (int)(sourceImage.Height * scale);

            // 设置pictureBox大小
            pictureBox!.Size = new Size(displayWidth, displayHeight);
            pictureBox.Location = new Point(
                (this.ClientSize.Width - displayWidth) / 2,
                (availableHeight - displayHeight) / 2
            );

            // 绘制图像
            Bitmap displayImage = new Bitmap(displayWidth, displayHeight);
            using (Graphics g = Graphics.FromImage(displayImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(sourceImage, 0, 0, displayWidth, displayHeight);
            }

            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            pictureBox.Image = displayImage;

            // 存储缩放比例
            ImageScale = scale;
        }

        public double ImageScale { get; private set; } = 1.0;

        private void PictureBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                cropStartPoint = e.Location;
                cropEndPoint = e.Location;
                selectionRect = new Rectangle(e.Location, new Size(0, 0));
            }
        }

        private void PictureBox_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!isDragging) return;

            cropEndPoint = e.Location;

            // 计算矩形
            int x = Math.Min(cropStartPoint.X, cropEndPoint.X);
            int y = Math.Min(cropStartPoint.Y, cropEndPoint.Y);
            int width = Math.Abs(cropEndPoint.X - cropStartPoint.X);
            int height = Math.Abs(cropEndPoint.Y - cropStartPoint.Y);

            selectionRect = new Rectangle(x, y, width, height);

            // 更新信息标签
            int actualWidth = (int)(width / ImageScale);
            int actualHeight = (int)(height / ImageScale);
            labelInfo!.Text = $"框选区域: ({x}, {y}) - ({x + width}, {y + height}) | 实际尺寸: {actualWidth}x{actualHeight}";

            // 重绘
            pictureBox!.Invalidate();
        }

        private void PictureBox_MouseUp(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;

                // 如果矩形太小，忽略
                if (selectionRect.Width < 5 || selectionRect.Height < 5)
                {
                    selectionRect = Rectangle.Empty;
                    labelInfo!.Text = "请拖动鼠标选择裁切区域";
                    pictureBox!.Invalidate();
                    return;
                }

                // 转换为原始图像坐标
                StartX = (int)(selectionRect.X / ImageScale);
                StartY = (int)(selectionRect.Y / ImageScale);
                EndX = (int)((selectionRect.X + selectionRect.Width) / ImageScale);
                EndY = (int)((selectionRect.Y + selectionRect.Height) / ImageScale);

                // 确保坐标在有效范围内
                StartX = Math.Max(0, Math.Min(StartX, sourceImage.Width - 1));
                StartY = Math.Max(0, Math.Min(StartY, sourceImage.Height - 1));
                EndX = Math.Max(StartX + 1, Math.Min(EndX, sourceImage.Width));
                EndY = Math.Max(StartY + 1, Math.Min(EndY, sourceImage.Height));

                labelInfo!.Text = $"已选择: ({StartX}, {StartY}) - ({EndX}, {EndY}) | 尺寸: {EndX - StartX}x{EndY - StartY}";
            }
        }

        private void PictureBox_Paint(object? sender, PaintEventArgs e)
        {
            if (selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                // 绘制矩形边框
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }

                // 绘制半透明填充
                using (Brush brush = new SolidBrush(Color.FromArgb(30, Color.Red)))
                {
                    e.Graphics.FillRectangle(brush, selectionRect);
                }

                // 显示尺寸信息
                int actualWidth = (int)(selectionRect.Width / ImageScale);
                int actualHeight = (int)(selectionRect.Height / ImageScale);
                string sizeText = $"{actualWidth}x{actualHeight}";

                using (Font font = new Font("Arial", 10, FontStyle.Bold))
                {
                    SizeF textSize = e.Graphics.MeasureString(sizeText, font);
                    PointF textPos = new PointF(
                        selectionRect.X + (selectionRect.Width - textSize.Width) / 2,
                        selectionRect.Y + (selectionRect.Height - textSize.Height) / 2
                    );

                    // 绘制文字背景
                    using (Brush bgBrush = new SolidBrush(Color.FromArgb(128, Color.Black)))
                    {
                        e.Graphics.FillRectangle(bgBrush, textPos.X - 2, textPos.Y - 2, textSize.Width + 4, textSize.Height + 4);
                    }

                    using (Brush brush = new SolidBrush(Color.Yellow))
                    {
                        e.Graphics.DrawString(sizeText, font, brush, textPos);
                    }
                }
            }
        }

        private void ButtonConfirm_Click(object? sender, EventArgs e)
        {
            if (selectionRect.Width < 5 || selectionRect.Height < 5)
            {
                MessageBox.Show("请先拖动鼠标选择裁切区域", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Confirmed = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object? sender, EventArgs e)
        {
            Confirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private PictureBox? pictureBox;
        private Panel? panelButtons;
        private Button? buttonConfirm;
        private Button? buttonCancel;
        private Label? labelInfo;
    }
}
