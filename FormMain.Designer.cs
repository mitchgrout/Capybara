namespace Capybara
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonRecord = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.checkBoxRepeat = new System.Windows.Forms.CheckBox();
            this.trackBarReplaySpeed = new System.Windows.Forms.TrackBar();
            this.labelReplaySpeed = new System.Windows.Forms.Label();
            this.toolTipReplaySpeed = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarReplaySpeed)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonRecord
            // 
            this.buttonRecord.Location = new System.Drawing.Point(0, 0);
            this.buttonRecord.Name = "buttonRecord";
            this.buttonRecord.Size = new System.Drawing.Size(75, 23);
            this.buttonRecord.TabIndex = 0;
            this.buttonRecord.Text = "Record";
            this.buttonRecord.UseVisualStyleBackColor = true;
            this.buttonRecord.Click += new System.EventHandler(this.buttonRecord_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(75, 0);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonPlay
            // 
            this.buttonPlay.Location = new System.Drawing.Point(150, 0);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(75, 23);
            this.buttonPlay.TabIndex = 2;
            this.buttonPlay.Text = "Play";
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // checkBoxRepeat
            // 
            this.checkBoxRepeat.AutoSize = true;
            this.checkBoxRepeat.Location = new System.Drawing.Point(0, 23);
            this.checkBoxRepeat.Name = "checkBoxRepeat";
            this.checkBoxRepeat.Size = new System.Drawing.Size(67, 17);
            this.checkBoxRepeat.TabIndex = 3;
            this.checkBoxRepeat.Text = "Repeat?";
            this.checkBoxRepeat.UseVisualStyleBackColor = true;
            // 
            // trackBarReplaySpeed
            // 
            this.trackBarReplaySpeed.Location = new System.Drawing.Point(0, 55);
            this.trackBarReplaySpeed.Maximum = 40;
            this.trackBarReplaySpeed.Minimum = -40;
            this.trackBarReplaySpeed.Name = "trackBarReplaySpeed";
            this.trackBarReplaySpeed.Size = new System.Drawing.Size(225, 45);
            this.trackBarReplaySpeed.TabIndex = 4;
            
            // 
            // labelReplaySpeed
            // 
            this.labelReplaySpeed.AutoSize = true;
            this.labelReplaySpeed.Location = new System.Drawing.Point(0, 39);
            this.labelReplaySpeed.Name = "labelReplaySpeed";
            this.labelReplaySpeed.Size = new System.Drawing.Size(77, 13);
            this.labelReplaySpeed.TabIndex = 5;
            this.labelReplaySpeed.Text = "Replay Speed:";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(225, 85);
            this.Controls.Add(this.labelReplaySpeed);
            this.Controls.Add(this.trackBarReplaySpeed);
            this.Controls.Add(this.checkBoxRepeat);
            this.Controls.Add(this.buttonPlay);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonRecord);
            this.MaximumSize = new System.Drawing.Size(241, 123);
            this.MinimumSize = new System.Drawing.Size(241, 123);
            this.Name = "FormMain";
            this.Text = "Capybara";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarReplaySpeed)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonRecord;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.CheckBox checkBoxRepeat;
        private System.Windows.Forms.TrackBar trackBarReplaySpeed;
        private System.Windows.Forms.Label labelReplaySpeed;
        private System.Windows.Forms.ToolTip toolTipReplaySpeed;
    }
}

