namespace App.Forms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            num_Control = new NumericUpDown();
            pictureBox_Source = new PictureBox();
            pictureBox_Result = new PictureBox();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)num_Control).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_Source).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_Result).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1230, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, saveToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(103, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(103, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // num_Control
            // 
            num_Control.Location = new Point(12, 518);
            num_Control.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            num_Control.Minimum = new decimal(new int[] { 180, 0, 0, int.MinValue });
            num_Control.Name = "num_Control";
            num_Control.Size = new Size(71, 23);
            num_Control.TabIndex = 1;
            num_Control.TextAlign = HorizontalAlignment.Center;
            num_Control.ValueChanged += num_Control_ValueChanged;
            // 
            // pictureBox_Source
            // 
            pictureBox_Source.BorderStyle = BorderStyle.FixedSingle;
            pictureBox_Source.Cursor = Cursors.Cross;
            pictureBox_Source.Location = new Point(12, 27);
            pictureBox_Source.Name = "pictureBox_Source";
            pictureBox_Source.Size = new Size(556, 485);
            pictureBox_Source.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_Source.TabIndex = 2;
            pictureBox_Source.TabStop = false;
            // 
            // pictureBox_Result
            // 
            pictureBox_Result.BorderStyle = BorderStyle.FixedSingle;
            pictureBox_Result.Location = new Point(574, 27);
            pictureBox_Result.Name = "pictureBox_Result";
            pictureBox_Result.Size = new Size(656, 526);
            pictureBox_Result.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_Result.TabIndex = 3;
            pictureBox_Result.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1230, 553);
            Controls.Add(pictureBox_Result);
            Controls.Add(pictureBox_Source);
            Controls.Add(num_Control);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Image Transformation";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)num_Control).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_Source).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_Result).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private NumericUpDown num_Control;
        private PictureBox pictureBox_Source;
        private PictureBox pictureBox_Result;
    }
}
