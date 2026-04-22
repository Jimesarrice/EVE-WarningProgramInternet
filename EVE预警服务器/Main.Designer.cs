namespace EVE预警服务器
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            splitContainer = new SplitContainer();
            listViewOnlineUsers = new ListView();
            labelOnlineUsers = new Label();
            richTextBoxLog = new RichTextBox();
            labelLog = new Label();
            textBoxPort = new TextBox();
            labelPort = new Label();
            buttonStart = new Button();
            buttonStop = new Button();
            labelStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // SplitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            // 
            // SplitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(labelOnlineUsers);
            splitContainer.Panel1.Controls.Add(listViewOnlineUsers);
            // 
            // SplitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(labelLog);
            splitContainer.Panel2.Controls.Add(richTextBoxLog);
            splitContainer.Size = new Size(900, 520);
            splitContainer.SplitterDistance = 250;
            splitContainer.TabIndex = 0;
            // 
            // labelOnlineUsers
            // 
            labelOnlineUsers.AutoSize = true;
            labelOnlineUsers.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelOnlineUsers.Location = new Point(10, 10);
            labelOnlineUsers.Name = "labelOnlineUsers";
            labelOnlineUsers.Size = new Size(80, 19);
            labelOnlineUsers.TabIndex = 1;
            labelOnlineUsers.Text = "在线用户:";
            // 
            // listViewOnlineUsers
            // 
            listViewOnlineUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewOnlineUsers.FullRowSelect = true;
            listViewOnlineUsers.Location = new Point(10, 35);
            listViewOnlineUsers.Name = "listViewOnlineUsers";
            listViewOnlineUsers.Size = new Size(880, 200);
            listViewOnlineUsers.TabIndex = 0;
            listViewOnlineUsers.UseCompatibleStateImageBehavior = false;
            listViewOnlineUsers.View = View.Details;
            // 
            // labelLog
            // 
            labelLog.AutoSize = true;
            labelLog.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelLog.Location = new Point(10, 10);
            labelLog.Name = "labelLog";
            labelLog.Size = new Size(50, 19);
            labelLog.TabIndex = 1;
            labelLog.Text = "日志:";
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBoxLog.Location = new Point(10, 35);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.ReadOnly = true;
            richTextBoxLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBoxLog.Size = new Size(880, 220);
            richTextBoxLog.TabIndex = 0;
            richTextBoxLog.Text = "";
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new Point(85, 535);
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new Size(80, 27);
            textBoxPort.TabIndex = 1;
            textBoxPort.Text = "2424";
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(10, 538);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(69, 20);
            labelPort.TabIndex = 2;
            labelPort.Text = "监听端口:";
            // 
            // buttonStart
            // 
            buttonStart.Location = new Point(180, 534);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(100, 29);
            buttonStart.TabIndex = 3;
            buttonStart.Text = "开始监听";
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += buttonStart_Click;
            // 
            // buttonStop
            // 
            buttonStop.Enabled = false;
            buttonStop.Location = new Point(290, 534);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(100, 29);
            buttonStop.TabIndex = 4;
            buttonStop.Text = "停止监听";
            buttonStop.UseVisualStyleBackColor = true;
            buttonStop.Click += buttonStop_Click;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.ForeColor = Color.Gray;
            labelStatus.Location = new Point(410, 538);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(94, 20);
            labelStatus.TabIndex = 5;
            labelStatus.Text = "状态: 未启动";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 580);
            Controls.Add(labelStatus);
            Controls.Add(buttonStop);
            Controls.Add(buttonStart);
            Controls.Add(labelPort);
            Controls.Add(textBoxPort);
            Controls.Add(splitContainer);
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "EVE预警服务器";
            FormClosing += Main_FormClosing;
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel1.PerformLayout();
            splitContainer.Panel2.ResumeLayout(false);
            splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SplitContainer splitContainer;
        private ListView listViewOnlineUsers;
        private Label labelOnlineUsers;
        private RichTextBox richTextBoxLog;
        private Label labelLog;
        private TextBox textBoxPort;
        private Label labelPort;
        private Button buttonStart;
        private Button buttonStop;
        private Label labelStatus;
    }
}
