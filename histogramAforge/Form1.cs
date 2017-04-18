using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using AForge;
using AForge.Imaging;
using AForge.Imaging.ComplexFilters;
using AForge.Imaging.ColorReduction;
using AForge.Imaging.Filters;
using histogramAforge.Model;
using ocr;



namespace histogramAforge
{
    public partial class Form1 : Form
    {

        //public Bitmap CharactersList;

        public List<Bitmap> CharactersList = new List<Bitmap>();

        public Form1()
        {
            InitializeComponent();
            
        }
        



    private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pictureBox1.Image = (Bitmap)System.Drawing.Image.FromFile(openFileDialog1.FileName);

                Bitmap imageSecond;
                imageSecond = (Bitmap)System.Drawing.Image.FromFile(openFileDialog1.FileName);
                Grayscale greyScaleFilter = new Grayscale(1.0, 0.0, 0.0);
                imageSecond = greyScaleFilter.Apply(imageSecond);
                Threshold tresholdFilter = new Threshold(120);
                tresholdFilter.ApplyInPlace(imageSecond);
                pictureBox2.Image = imageSecond;
                imageSecond.Save("imageSecond.bmp");
                GraphicProcessing graphicProcesing = new GraphicProcessing();
                CharactersList = graphicProcesing.ProcesImage(imageSecond);



            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void TeachNetworkButton_Click(object sender, EventArgs e)
        {
            PlateNoOcr plateOCR = PlateNoOcr.Instance;
            plateOCR.init();
            teachStatuslabel.Text = "teached";
        }

        private void OCRButton_Click(object sender, EventArgs e)
        {
            OcrPlatesResult.Text = " ";
            PlateNoOcr plateOCR = PlateNoOcr.Instance;
            foreach(Bitmap map in CharactersList)
            {
                OcrPlatesResult.Text += plateOCR.OcrImage(map);
            }
        }
    }

}     
