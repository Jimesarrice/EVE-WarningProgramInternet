﻿namespace EVE预警
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
            labelHandle = new Label();
            pictureBox = new PictureBox();
            textBoxGalaxy = new TextBox();
            labelGalaxy = new Label();
            richTextBoxLog = new RichTextBox();
            labelLog = new Label();
            buttonCapture = new Button();
            buttonExit = new Button();
            comboBoxUpdateRate = new ComboBox();
            labelUpdateRate = new Label();
            trackBarStartX = new TrackBar();
            trackBarStartY = new TrackBar();
            trackBarEndX = new TrackBar();
            trackBarEndY = new TrackBar();
            labelStartX = new Label();
            labelStartY = new Label();
            labelEndX = new Label();
            labelEndY = new Label();
            textBoxStartX = new TextBox();
            textBoxStartY = new TextBox();
            textBoxEndX = new TextBox();
            textBoxEndY = new TextBox();
            buttonStartDetection = new Button();
            buttonStopDetection = new Button();
            checkBoxPlaySound = new CheckBox();
            buttonCropSelect = new Button();
            connectSever = new Button();
            textBoxServerIP = new TextBox();
            labelServerIP = new Label();
            buttonDisconnectServer = new Button();
            richTextBoxAlertLog = new RichTextBox();
            labelAlertLog = new Label();
            labelConnectionState = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarStartX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarStartY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarEndX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarEndY).BeginInit();
            SuspendLayout();
            // 
            // labelHandle
            // 
            labelHandle.AutoSize = true;
            labelHandle.Location = new Point(12, 9);
            labelHandle.Name = "labelHandle";
            labelHandle.Size = new Size(122, 20);
            labelHandle.TabIndex = 0;
            labelHandle.Text = "当前窗口句柄: 无";
            // 
            // pictureBox
            // 
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.Location = new Point(12, 54);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(301, 489);
            pictureBox.TabIndex = 1;
            pictureBox.TabStop = false;
            // 
            // textBoxGalaxy
            // 
            textBoxGalaxy.Location = new Point(100, 549);
            textBoxGalaxy.Name = "textBoxGalaxy";
            textBoxGalaxy.Size = new Size(98, 27);
            textBoxGalaxy.TabIndex = 2;
            // 
            // labelGalaxy
            // 
            labelGalaxy.AutoSize = true;
            labelGalaxy.Location = new Point(12, 552);
            labelGalaxy.Name = "labelGalaxy";
            labelGalaxy.Size = new Size(73, 20);
            labelGalaxy.TabIndex = 3;
            labelGalaxy.Text = "星系位置:";
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Location = new Point(319, 54);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.ReadOnly = true;
            richTextBoxLog.Size = new Size(469, 200);
            richTextBoxLog.TabIndex = 4;
            richTextBoxLog.Text = "";
            richTextBoxLog.TextChanged += richTextBoxLog_TextChanged;
            // 
            // labelLog
            // 
            labelLog.AutoSize = true;
            labelLog.Location = new Point(319, 31);
            labelLog.Name = "labelLog";
            labelLog.Size = new Size(39, 20);
            labelLog.TabIndex = 5;
            labelLog.Text = "日志";
            // 
            // buttonCapture
            // 
            buttonCapture.Location = new Point(12, 636);
            buttonCapture.Name = "buttonCapture";
            buttonCapture.Size = new Size(140, 29);
            buttonCapture.TabIndex = 6;
            buttonCapture.Text = "抓取窗口";
            buttonCapture.UseVisualStyleBackColor = true;
            buttonCapture.Click += buttonCapture_Click;
            // 
            // buttonExit
            // 
            buttonExit.Location = new Point(13, 706);
            buttonExit.Name = "buttonExit";
            buttonExit.Size = new Size(296, 29);
            buttonExit.TabIndex = 7;
            buttonExit.Text = "退出程序";
            buttonExit.UseVisualStyleBackColor = true;
            buttonExit.Click += buttonExit_Click;
            // 
            // comboBoxUpdateRate
            // 
            comboBoxUpdateRate.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxUpdateRate.FormattingEnabled = true;
            comboBoxUpdateRate.Items.AddRange(new object[] { "0.5秒", "1秒", "2秒", "3秒", "4秒", "5秒", "6秒", "7秒", "8秒", "9秒", "10秒", "15秒", "20秒" });
            comboBoxUpdateRate.Location = new Point(100, 594);
            comboBoxUpdateRate.Name = "comboBoxUpdateRate";
            comboBoxUpdateRate.Size = new Size(67, 28);
            comboBoxUpdateRate.TabIndex = 8;
            // 
            // labelUpdateRate
            // 
            labelUpdateRate.AutoSize = true;
            labelUpdateRate.Location = new Point(10, 597);
            labelUpdateRate.Name = "labelUpdateRate";
            labelUpdateRate.Size = new Size(73, 20);
            labelUpdateRate.TabIndex = 9;
            labelUpdateRate.Text = "更新频率:";
            // 
            // trackBarStartX
            // 
            trackBarStartX.Location = new Point(319, 478);
            trackBarStartX.Maximum = 1920;
            trackBarStartX.Name = "trackBarStartX";
            trackBarStartX.Size = new Size(349, 56);
            trackBarStartX.TabIndex = 10;
            trackBarStartX.Scroll += trackBar_Scroll;
            // 
            // trackBarStartY
            // 
            trackBarStartY.Location = new Point(319, 548);
            trackBarStartY.Maximum = 1080;
            trackBarStartY.Name = "trackBarStartY";
            trackBarStartY.Size = new Size(349, 56);
            trackBarStartY.TabIndex = 11;
            trackBarStartY.Scroll += trackBar_Scroll;
            // 
            // trackBarEndX
            // 
            trackBarEndX.Location = new Point(319, 618);
            trackBarEndX.Maximum = 1920;
            trackBarEndX.Name = "trackBarEndX";
            trackBarEndX.Size = new Size(349, 56);
            trackBarEndX.TabIndex = 12;
            trackBarEndX.Value = 1920;
            trackBarEndX.Scroll += trackBar_Scroll;
            // 
            // trackBarEndY
            // 
            trackBarEndY.Location = new Point(319, 688);
            trackBarEndY.Maximum = 1080;
            trackBarEndY.Name = "trackBarEndY";
            trackBarEndY.Size = new Size(349, 56);
            trackBarEndY.TabIndex = 13;
            trackBarEndY.Value = 1080;
            trackBarEndY.Scroll += trackBar_Scroll;
            // 
            // labelStartX
            // 
            labelStartX.AutoSize = true;
            labelStartX.Location = new Point(674, 478);
            labelStartX.Name = "labelStartX";
            labelStartX.Size = new Size(53, 20);
            labelStartX.TabIndex = 14;
            labelStartX.Text = "起始X:";
            // 
            // labelStartY
            // 
            labelStartY.AutoSize = true;
            labelStartY.Location = new Point(674, 548);
            labelStartY.Name = "labelStartY";
            labelStartY.Size = new Size(52, 20);
            labelStartY.TabIndex = 15;
            labelStartY.Text = "起始Y:";
            // 
            // labelEndX
            // 
            labelEndX.AutoSize = true;
            labelEndX.Location = new Point(674, 618);
            labelEndX.Name = "labelEndX";
            labelEndX.Size = new Size(53, 20);
            labelEndX.TabIndex = 16;
            labelEndX.Text = "结束X:";
            // 
            // labelEndY
            // 
            labelEndY.AutoSize = true;
            labelEndY.Location = new Point(674, 688);
            labelEndY.Name = "labelEndY";
            labelEndY.Size = new Size(52, 20);
            labelEndY.TabIndex = 17;
            labelEndY.Text = "结束Y:";
            // 
            // textBoxStartX
            // 
            textBoxStartX.Location = new Point(735, 475);
            textBoxStartX.Name = "textBoxStartX";
            textBoxStartX.Size = new Size(53, 27);
            textBoxStartX.TabIndex = 18;
            textBoxStartX.Text = "0";
            textBoxStartX.TextChanged += textBox_TextChanged;
            // 
            // textBoxStartY
            // 
            textBoxStartY.Location = new Point(735, 545);
            textBoxStartY.Name = "textBoxStartY";
            textBoxStartY.Size = new Size(53, 27);
            textBoxStartY.TabIndex = 19;
            textBoxStartY.Text = "0";
            textBoxStartY.TextChanged += textBox_TextChanged;
            // 
            // textBoxEndX
            // 
            textBoxEndX.Location = new Point(735, 615);
            textBoxEndX.Name = "textBoxEndX";
            textBoxEndX.Size = new Size(53, 27);
            textBoxEndX.TabIndex = 20;
            textBoxEndX.Text = "1920";
            textBoxEndX.TextChanged += textBox_TextChanged;
            // 
            // textBoxEndY
            // 
            textBoxEndY.Location = new Point(735, 685);
            textBoxEndY.Name = "textBoxEndY";
            textBoxEndY.Size = new Size(53, 27);
            textBoxEndY.TabIndex = 21;
            textBoxEndY.Text = "1080";
            textBoxEndY.TextChanged += textBox_TextChanged;
            // 
            // buttonStartDetection
            // 
            buttonStartDetection.Location = new Point(169, 636);
            buttonStartDetection.Name = "buttonStartDetection";
            buttonStartDetection.Size = new Size(140, 29);
            buttonStartDetection.TabIndex = 22;
            buttonStartDetection.Text = "开始检测";
            buttonStartDetection.UseVisualStyleBackColor = true;
            buttonStartDetection.Click += buttonStartDetection_Click;
            // 
            // buttonStopDetection
            // 
            buttonStopDetection.Enabled = false;
            buttonStopDetection.Location = new Point(169, 671);
            buttonStopDetection.Name = "buttonStopDetection";
            buttonStopDetection.Size = new Size(140, 29);
            buttonStopDetection.TabIndex = 23;
            buttonStopDetection.Text = "停止检测";
            buttonStopDetection.UseVisualStyleBackColor = true;
            buttonStopDetection.Click += buttonStopDetection_Click;
            // 
            // checkBoxPlaySound
            // 
            checkBoxPlaySound.AutoSize = true;
            checkBoxPlaySound.Location = new Point(173, 596);
            checkBoxPlaySound.Name = "checkBoxPlaySound";
            checkBoxPlaySound.Size = new Size(136, 24);
            checkBoxPlaySound.TabIndex = 24;
            checkBoxPlaySound.Text = "报警时播放声音";
            checkBoxPlaySound.UseVisualStyleBackColor = true;
            // 
            // buttonCropSelect
            // 
            buttonCropSelect.Location = new Point(204, 548);
            buttonCropSelect.Name = "buttonCropSelect";
            buttonCropSelect.Size = new Size(109, 29);
            buttonCropSelect.TabIndex = 25;
            buttonCropSelect.Text = "框选裁切区域";
            buttonCropSelect.UseVisualStyleBackColor = true;
            buttonCropSelect.Click += buttonCropSelect_Click;
            // 
            // connectSever
            // 
            connectSever.Location = new Point(12, 671);
            connectSever.Name = "connectSever";
            connectSever.Size = new Size(140, 29);
            connectSever.TabIndex = 26;
            connectSever.Text = "连接服务器";
            connectSever.UseVisualStyleBackColor = true;
            connectSever.Click += buttonConnectSever_Click;
            // 
            // textBoxServerIP
            // 
            textBoxServerIP.Location = new Point(488, 262);
            textBoxServerIP.Name = "textBoxServerIP";
            textBoxServerIP.Size = new Size(120, 27);
            textBoxServerIP.TabIndex = 29;
            textBoxServerIP.Text = "127.0.0.1";
            // 
            // labelServerIP
            // 
            labelServerIP.AutoSize = true;
            labelServerIP.Location = new Point(411, 265);
            labelServerIP.Name = "labelServerIP";
            labelServerIP.Size = new Size(71, 20);
            labelServerIP.TabIndex = 30;
            labelServerIP.Text = "服务器IP:";
            // 
            // buttonDisconnectServer
            // 
            buttonDisconnectServer.Enabled = false;
            buttonDisconnectServer.Location = new Point(614, 260);
            buttonDisconnectServer.Name = "buttonDisconnectServer";
            buttonDisconnectServer.Size = new Size(100, 29);
            buttonDisconnectServer.TabIndex = 31;
            buttonDisconnectServer.Text = "断开连接";
            buttonDisconnectServer.UseVisualStyleBackColor = true;
            buttonDisconnectServer.Click += buttonDisconnectServer_Click;
            // 
            // richTextBoxAlertLog
            // 
            richTextBoxAlertLog.Location = new Point(320, 295);
            richTextBoxAlertLog.Name = "richTextBoxAlertLog";
            richTextBoxAlertLog.ReadOnly = true;
            richTextBoxAlertLog.Size = new Size(469, 449);
            richTextBoxAlertLog.TabIndex = 27;
            richTextBoxAlertLog.Text = "";
            // 
            // labelAlertLog
            // 
            labelAlertLog.AutoSize = true;
            labelAlertLog.Location = new Point(320, 269);
            labelAlertLog.Name = "labelAlertLog";
            labelAlertLog.Size = new Size(69, 20);
            labelAlertLog.TabIndex = 28;
            labelAlertLog.Text = "预警日志";
            // 
            // labelConnectionState
            // 
            labelConnectionState.AutoSize = true;
            labelConnectionState.ForeColor = Color.Gray;
            labelConnectionState.Location = new Point(720, 262);
            labelConnectionState.Name = "labelConnectionState";
            labelConnectionState.Size = new Size(68, 20);
            labelConnectionState.TabIndex = 32;
            labelConnectionState.Text = "● 未连接";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 755);
            Controls.Add(labelConnectionState);
            Controls.Add(buttonDisconnectServer);
            Controls.Add(labelServerIP);
            Controls.Add(textBoxServerIP);
            Controls.Add(labelAlertLog);
            Controls.Add(richTextBoxAlertLog);
            Controls.Add(connectSever);
            Controls.Add(buttonCropSelect);
            Controls.Add(checkBoxPlaySound);
            Controls.Add(buttonStopDetection);
            Controls.Add(buttonStartDetection);
            Controls.Add(textBoxEndY);
            Controls.Add(textBoxEndX);
            Controls.Add(textBoxStartY);
            Controls.Add(textBoxStartX);
            Controls.Add(labelEndY);
            Controls.Add(labelEndX);
            Controls.Add(labelStartY);
            Controls.Add(labelStartX);
            Controls.Add(trackBarEndY);
            Controls.Add(trackBarEndX);
            Controls.Add(trackBarStartY);
            Controls.Add(trackBarStartX);
            Controls.Add(labelUpdateRate);
            Controls.Add(comboBoxUpdateRate);
            Controls.Add(buttonExit);
            Controls.Add(buttonCapture);
            Controls.Add(labelLog);
            Controls.Add(richTextBoxLog);
            Controls.Add(labelGalaxy);
            Controls.Add(textBoxGalaxy);
            Controls.Add(pictureBox);
            Controls.Add(labelHandle);
            Name = "Main";
            Text = "EVE预警 - 色块识别工具";
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarStartX).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarStartY).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarEndX).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarEndY).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelHandle;
        private PictureBox pictureBox;
        private TextBox textBoxGalaxy;
        private Label labelGalaxy;
        private RichTextBox richTextBoxLog;
        private Label labelLog;
        private Button buttonCapture;
        private Button buttonExit;
        private ComboBox comboBoxUpdateRate;
        private Label labelUpdateRate;
        private TrackBar trackBarStartX;
        private TrackBar trackBarStartY;
        private TrackBar trackBarEndX;
        private TrackBar trackBarEndY;
        private Label labelStartX;
        private Label labelStartY;
        private Label labelEndX;
        private Label labelEndY;
        private TextBox textBoxStartX;
        private TextBox textBoxStartY;
        private TextBox textBoxEndX;
        private TextBox textBoxEndY;
        private Button buttonStartDetection;
        private Button buttonStopDetection;
        private CheckBox checkBoxPlaySound;
        private Button buttonCropSelect;
        private Button connectSever;
        private TextBox textBoxServerIP;
        private Label labelServerIP;
        private Button buttonDisconnectServer;
        private RichTextBox richTextBoxAlertLog;
        private Label labelAlertLog;
        private Label labelConnectionState;
    }
}
