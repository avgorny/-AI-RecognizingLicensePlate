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


namespace histogramAforge
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public Bitmap CropImage(Bitmap source, Rectangle section)
        {

            Bitmap bmp = new Bitmap(section.Width, section.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return bmp;
        }

      
    int createCutedCharacters(Bitmap gray, int startX)
    {
        Bitmap myBitmap = new Bitmap(gray);
        int first = 0;
        int second = 0;
        Boolean check = false;
        Boolean Bcheck = false;
        Boolean setSecond = false;
        for (int x = startX; x < myBitmap.Width; x++)
        {
            int count = 0;
            int blackCount = 0;

            for (int y = 0; y < myBitmap.Height; y++)
            {

                Color pixelColor = myBitmap.GetPixel(x, y);
                //zlicznaie baiłych pikseli
                if (pixelColor.R + pixelColor.G + pixelColor.B >= 250)
                {
                    count++;
                }
                //zliczanie czarnych pikseli
                if (pixelColor.R + pixelColor.G + pixelColor.B == 0)
                {
                    blackCount++;
                }

                if (count > myBitmap.Height * 0.85)
                {
                    Bcheck = true;
                }

                if (blackCount >= myBitmap.Height * 0.10 && !check)
                {
                    first = x;
                    check = true;
                    break;

                }
                else if (check && Bcheck)
                {
                    second = x;
                    setSecond = true;
                }


            }

            if (second != 0)
            {
                break;
            }
        }
        if (second - first >= myBitmap.Width * 0.03)
        {
            Crop filter = new Crop(new Rectangle(first - 2, 0, second - first + 2, myBitmap.Height));
            Bitmap newImage = filter.Apply(myBitmap);
            newImage.Save("C:\\Users\\Rafał Górny\\Documents\\AI_Projekt_Czytanie_Tablic\\ETAP1\\imageSecond" + startX+".bmp");
            createCharacters(newImage, startX);
        }
        
        if (setSecond == false)
        {
            return myBitmap.Width;
        }
        return second;

    }

        void createCharacters(Bitmap largeCharacter, int startX)
        {
            Bitmap myBitmap = new Bitmap(largeCharacter);
            int start = 0;
            int koniec = 0;
            Boolean flagaStart= false;
            Boolean flagaKoniec = false;
            //szukanie od wysokości 1/3 obrazka w górę wiersza który nie ma czarnego pixela.
            for (int y = (largeCharacter.Height / 3); y >= 0; y--)
            {
                int blackCounter = 0;
                for (int x = 0; x <= largeCharacter.Width -1; x++)
                {
                    Color pixelColor = myBitmap.GetPixel(x, y);
                    if (pixelColor.R + pixelColor.G + pixelColor.B <= 10)
                    {
                        blackCounter++;
                    }

                }
                if (blackCounter == 0 && flagaStart== false)
                {
                    start = y;
                    flagaStart = true;
                    
                }
            }
            //szukanie od wysokości 2/3 obrazka wiersza który nie ma czarnego pixela.
            for (int y = 2*(largeCharacter.Height / 3); y <= largeCharacter.Height -1 ; y++)
            {
                int blackCounter = 0;
                for (int x = 0; x <= largeCharacter.Width -1 ; x++)
                {
                    Color pixelColor = myBitmap.GetPixel(x, y);
                    if (pixelColor.R + pixelColor.G + pixelColor.B <= 10)
                    {
                        blackCounter++;
                    }

                }
                if (blackCounter == 0 && flagaKoniec==false)
                {
                    koniec = y;
                    flagaKoniec = true;
                }
            }
            //aprawdzenie czy mamy jakiś początek i koniec, zapisanie nowego obrazka
            if (start != 0 && koniec != 0)
            {
                Crop filter = new Crop(new Rectangle(0, start, largeCharacter.Width, koniec - start));
                Bitmap newImage = filter.Apply(myBitmap);
                newImage.Save("C:\\Users\\Rafał Górny\\Documents\\AI_Projekt_Czytanie_Tablic\\ETAP2\\imageSecond" + startX + ".bmp");
            }
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
                imageSecond.Save("C:\\Users\\Rafał Górny\\Desktop\\odKrysta\\imageSecond.bmp");

                int startX = 0;
                while (startX < imageSecond.Width)
                {
                    startX = createCutedCharacters(imageSecond, startX);

                }

            }
        }
    }

}     
