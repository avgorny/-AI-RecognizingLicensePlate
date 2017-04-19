using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace histogramAforge.Model
{
    class GraphicProcessing
    {
        public SaveFile saveFile = new SaveFile();
        List<Bitmap> bufferForResult = new List<Bitmap>();


        /// <summary>
        /// This metod get whole plate and cut it on characters.
        /// </summary>
        /// <param name="WhieteAndBlackImage"></param>
        /// <returns>List of bitmap with characters</returns>
        public List<Bitmap> ProcesImage (Bitmap WhieteAndBlackImage)
        {
            int startX = 0;
            while (startX < WhieteAndBlackImage.Width)
            {

                startX = CreateCutedCharacters(WhieteAndBlackImage, startX);

            }
            List<Bitmap> buff = bufferForResult;
            bufferForResult = null;
            return buff;

        }

        /// <summary>
        /// This method crop an image
        /// </summary>
        /// <param name="source"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public Bitmap CropImage(Bitmap source, Rectangle section)
        {

            Bitmap bmp = new Bitmap(section.Width, section.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return bmp;
        }

        /// <summary>
        /// This metod is croping an image basis on white and balck pixels.
        /// At first it is looking for black pixels around 10% of column.
        /// If this will be checked then it is looking for an white space min 85% in column
        /// If both are founded it crops the image.
        /// </summary>
        /// <param name="gray"></param>
        /// <param name="startX"></param>
        /// <returns></returns>
        private int CreateCutedCharacters(Bitmap gray, int startX)
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
                this.saveFile.saveImages(newImage, startX);
                CreateCharacters(newImage, startX);
            }

            if (setSecond == false)
            {
                return myBitmap.Width;
            }
            return second;

        }


        /// <summary>
        /// This method cut unnecessary margins from an image.
        /// </summary>
        /// <param name="largeCharacter"></param>
        /// <param name="startX"></param>
        private void CreateCharacters(Bitmap largeCharacter, int startX)
        {
            Bitmap myBitmap = new Bitmap(largeCharacter);
            int start = 0;
            int koniec = 0;
            Boolean flagaStart = false;
            Boolean flagaKoniec = false;
            //szukanie od wysokości 1/3 obrazka w górę wiersza który nie ma czarnego pixela.
            for (int y = (largeCharacter.Height / 3); y >= 0; y--)
            {
                int blackCounter = 0;
                for (int x = 0; x <= largeCharacter.Width - 1; x++)
                {
                    Color pixelColor = myBitmap.GetPixel(x, y);
                    if (pixelColor.R + pixelColor.G + pixelColor.B <= 10)
                    {
                        blackCounter++;
                    }

                }
                if (blackCounter == 0 && flagaStart == false)
                {
                    start = y;
                    flagaStart = true;

                }
            }
            //szukanie od wysokości 2/3 obrazka wiersza który nie ma czarnego pixela.
            for (int y = 2 * (largeCharacter.Height / 3); y <= largeCharacter.Height - 1; y++)
            {
                int blackCounter = 0;
                for (int x = 0; x <= largeCharacter.Width - 1; x++)
                {
                    Color pixelColor = myBitmap.GetPixel(x, y);
                    if (pixelColor.R + pixelColor.G + pixelColor.B <= 10)
                    {
                        blackCounter++;
                    }

                }
                if (blackCounter == 0 && flagaKoniec == false)
                {
                    koniec = y;
                    flagaKoniec = true;
                }
            }
            //aprawdzenie czy mamy jakiś początek i koniec, zapisanie nowego obrazka
            if (start != 0 && koniec != 0)
            {
                Crop filter = new Crop(new Rectangle(0, start, largeCharacter.Width, koniec - start));
                myBitmap = filter.Apply(myBitmap);
                saveFile.saveImages(myBitmap, startX, "\\Etap2\\");
                bufferForResult.Add(myBitmap);

            }
            
        }

        public List<Bitmap> ProcesLearningImage(Bitmap WhieteAndBlackImage)
        {
            int startX = 0;
            while (startX < WhieteAndBlackImage.Width)
            {

                startX = CreateCutedLeraningCharacters(WhieteAndBlackImage, startX);

            }
            List<Bitmap> buff = bufferForResult;
            bufferForResult = null;
            return buff;

        }


        /// <summary>
        /// This metod is croping an image basis on white and balck pixels.
        /// At first it is looking for black pixels around 10% of column.
        /// If this will be checked then it is looking for an white space min 85% in column
        /// If both are founded it crops the image.
        /// </summary>
        /// <param name="gray"></param>
        /// <param name="startX"></param>
        /// <returns></returns>
        private int CreateCutedLeraningCharacters(Bitmap gray, int startX)
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

                    if (count > myBitmap.Height * 0.95)
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
            if (second - first >= 8)
            {
                Crop filter = new Crop(new Rectangle(first - 2, 0, second - first + 2, myBitmap.Height));
                Bitmap newImage = filter.Apply(myBitmap);
                this.saveFile.saveImages(newImage, startX);
                CreateCharacters(newImage, startX);
            }

            if (setSecond == false)
            {
                return myBitmap.Width;
            }
            return second;

        }
    }
}
