using Core;
using ImageTransformation.Core;
using System.Drawing.Imaging;

namespace App.Forms
{
    public partial class Form1 : Form
    {
        // fields
        Bitmap bitmapSrc;
        Bitmap bitmapDst;
        Matrix transformation;

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.bitmapSrc = new Bitmap(openFileDialog.FileName);
                this.bitmapDst = new Bitmap(this.bitmapSrc);
                this.pictureBox_Source.Image = this.bitmapSrc;
                this.pictureBox_Result.Image = this.bitmapDst;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.bitmapDst.Save(saveFileDialog.FileName);
            }
        }

        private async void num_Control_ValueChanged(object sender, EventArgs e)
        {
            if(this.bitmapSrc == null) return;
            
            this.transformation = Transformations.Rotation((double)num_Control.Value);
            TransformBitmap.ExecuteForward(this.bitmapSrc, ref this.bitmapDst, this.transformation);
            this.pictureBox_Result.Image = this.bitmapDst;
        }
    }
}
