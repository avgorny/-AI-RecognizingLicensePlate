using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using histogramAforge.Model;
using System.IO;
using System.Xml.Serialization;

namespace ocr
{
    public class PlateNoOcr
    {

        private static PlateNoOcr instance;
        // pattern size
        private int patternSize = 35;
        // patterns count
        private int patterns = 9;
        //
        AForge.Neuro.ActivationNetwork Network;

        public PlateNoOcr()
        {

        }
        

        public static PlateNoOcr Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlateNoOcr();
                }
                return instance;
            }
        }




        // learning input vectors
        double[][] input = new double[9][]
        {
                new double [] {
                0, 0, 1, 0, 0,
                0, 1, 1, 1, 0,
                0, 1, 0, 1, 0,
                0, 1, 0, 1, 0,
                1, 1, 1, 1, 1,
                1, 0, 0, 0, 1,
                1, 0, 0, 0, 1}, // Letter A

                new double [] {
                1, 0, 0, 1, 1,
                1, 0, 0, 1, 0,
                1, 0, 1, 0, 0,
                1, 1, 0, 0, 0,
                1, 0, 1, 0, 0,
                1, 0, 0, 1, 0,
                1, 0, 0, 0, 1}, // Letter K
                new double [] {
                1, 1, 1, 1, 1,
                1, 0, 0, 0, 0,
                1, 0, 0, 0, 0,
                1, 1, 1, 1, 1,
                1, 0, 0, 0, 0,
                1, 0, 0, 0, 0,
                1, 1, 1, 1, 1}, // Letter E
                new double [] {
                1, 1, 1, 1, 0,
                1, 0, 0, 0, 1,
                1, 0, 0, 1, 0,
                1, 1, 1, 0, 0,
                1, 0, 0, 1, 0,
                1, 0, 0, 0, 1,
                1, 0, 0, 0, 1}, // Letter R
                new double [] {
                1, 1, 1, 1, 1,
                0, 0, 1, 0, 0,
                0, 0, 1, 0, 0,
                0, 0, 1, 0, 0,
                0, 0, 1, 0, 0,
                0, 0, 1, 0, 0,
                0, 0, 1, 0, 0}, // Letter T
                new double [] {
                1, 0, 0, 0, 1,
                1, 1, 0, 1, 1,
                1, 0, 1, 0, 1,
                1, 0, 1, 0, 1,
                1, 0, 1, 0, 1,
                1, 0, 0, 0, 1,
                1, 0, 0, 0, 1}, // Letter M
                new double [] {
                1, 0, 0, 0, 0,
                1, 0, 0, 0, 0,
                1, 0, 0, 0, 0,
                1, 0, 0, 0, 0,
                1, 0, 0, 0, 0,
                1, 0, 0, 0, 0,
                1, 1, 1, 1, 1}, // Letter L
                new double[] {
                    1,1,1,1,1,
                    1,0,0,0,0,
                    1,0,0,0,0,
                    1,1,1,1,1,
                    0,0,0,0,1,
                    0,0,0,0,1,
                    1,1,1,1,1 }, //letter 5
                 new double[] {
                    1,1,1,1,1,
                    1,1,1,1,1,
                    0,0,0,1,1,
                    0,0,1,0,0,
                    0,0,1,0,0,
                    0,1,0,0,0,
                    1,0,0,0,0, } //letter 7
                





        };
        // learning ouput vectors
        double[][] output = new double[9][]
        {

            new double [] {
            1,0,0,0,0,0,0,0,0

            },
             new double [] {
             0,1,0,0,0,0,0,0,0

            },
            new double [] {
             0,0,1,0,0,0,0,0,0

            },
            new double [] {
             0,0,0,1,0,0,0,0,0

            },
            new double [] {
             0,0,0,0,1,0,0,0,0

            },
            new double [] {
             0,0,0,0,0,1,0,0,0

            },
            new double [] {
             0,0,0,0,0,0,1,0,0

            },
            new double [] {
             0,0,0,0,0,0,0,1,0

            },
            new double [] {
             0,0,0,0,0,0,0,0,1

            },
        };
        public int init()
        {
            AForge.Neuro.ActivationNetwork neuralNet =
                new AForge.Neuro.ActivationNetwork(new AForge.Neuro.BipolarSigmoidFunction(0.5), patternSize, patterns, patterns);
            // randomize network`s weights
            neuralNet.Randomize();

            // create network teacher
            AForge.Neuro.Learning.BackPropagationLearning teacher = new AForge.Neuro.Learning.BackPropagationLearning(neuralNet);
            teacher.Momentum = 0.2f;
            teacher.LearningRate = 0.55f;
            
            
            // teach the network
            int i = 0;
            Random rand = new Random();
            double error;
            do
            {            
                error = teacher.RunEpoch(input, output);
                Console.WriteLine(error);
                i++;
            }
            while (error > 0.1);
            Network = neuralNet;
            return i;
            //
            
        }

        public String OcrImage(Bitmap mBitmap)
        {
                                   
            CreteInputMatrix(mBitmap);
            double[] output2 = Network.Compute(CreteInputMatrix(mBitmap));

            int j, n, maxIndex = 0;

            // find the maximum from output
            double max = output2[0];
            for (j = 0, n = output2.Length; j < n; j++)
            {
                if (output2[j] > max)
                {
                    max = output2[j];
                    maxIndex = j;
                }
            }
            return maxIndex.ToString();
           
        }

        private double[] CreteInputMatrix(Bitmap mBitmap)
        {
            double[] result = new double[35];
            int i = 0;
            List<PointsStartEnd> PointsList = CreatePointsTable(mBitmap);
            foreach (PointsStartEnd pointSet in PointsList)
            {

                result[i] = CountBlackWhite(pointSet, mBitmap);

            }
            return result;
        }

        private List<PointsStartEnd> CreatePointsTable(Bitmap img)
        {
            List<PointsStartEnd> list = new List<PointsStartEnd>();
            int imgHeight = img.Height;
            int imgWidth = img.Width;
            int imgWidthDivided = imgWidth / 5;
            int imgHeightDivided = imgHeight / 7;

            for (int i = 0; i < imgHeight; i += imgHeightDivided)
            {
                for (int j = 0; j < imgWidth; j += imgWidthDivided)
                {
                    PointsStartEnd buff = new PointsStartEnd();
                    if (j + imgWidthDivided <= imgWidth && i + imgHeightDivided <= imgHeight)
                    {
                        buff.strat = new System.Drawing.Point(j, i);
                        buff.end = new System.Drawing.Point(j + imgWidthDivided - 1, i + imgHeightDivided - 1);
                        list.Add(buff);
                    }

                }
            }

            return list;
        }

        private double CountBlackWhite(PointsStartEnd pointSet, Bitmap bitMap)
        {
            double result = 0;
            int whiteCount = 0;
            int blackCount = 0;
            for (int x = pointSet.strat.X; x < pointSet.end.X; x++)
            {
                for (int y = pointSet.strat.Y; y < pointSet.end.Y; y++)
                {
                    Color pixelColor = bitMap.GetPixel(x, y);
                    //zlicznaie baiłych pikseli
                    if (pixelColor.R + pixelColor.G + pixelColor.B >= 250)
                    {
                        whiteCount++;
                    }
                    //zliczanie czarnych pikseli
                    if (pixelColor.R + pixelColor.G + pixelColor.B == 0)
                    {
                        blackCount++;
                    }
                }
            }
            int height = pointSet.end.Y - pointSet.strat.Y;
            int width = pointSet.end.X - pointSet.strat.X;
            int content = height * width;
            result = (double)blackCount / (double)content;
            //if (result >0.4)
            //{
            //    result = 1;
            //}else
            //{
            //    result = 0;
            //}
            return result;
        }

        private void CreateLearningMatrix()
        {
            GraphicProcessing graph = new GraphicProcessing();
            Bitmap mBitmap;
            using (Stream BitmapStream = System.IO.File.OpenRead("\\LearningPictures\\learningmatrix.png"))
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(BitmapStream);
                mBitmap = new Bitmap(img);
            }
            graph.ProcesImage(mBitmap);


        }

        public void SerializeNetwork()
        {          
            try
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "BIN|*.bin";
                saveFileDialog1.Title = "Save an bin File";
                saveFileDialog1.ShowDialog();

                // If the file name is not an empty string open it for saving.
                if (saveFileDialog1.FileName != "")
                {
                    Network.Save(saveFileDialog1.FileName);
                }
            }
            catch (Exception oException)
            {
                Console.WriteLine("Aplikacja wygenerowała następujący wyjątek: " + oException.Message);
            }
        }

        public void LoadSerializedNetwork()
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "BIN|*.bin";
                openDialog.Title = "Save an bin File";
                openDialog.ShowDialog();

                // If the file name is not an empty string open it for saving.
                if (openDialog.FileName != "")
                {
                    AForge.Neuro.Network NetworkBuf = AForge.Neuro.ActivationNetwork.Load(openDialog.FileName);
                    Network = (AForge.Neuro.ActivationNetwork)NetworkBuf;
                }
            }
            catch (Exception oException)
            {
                Console.WriteLine("Aplikacja wygenerowała następujący wyjątek: " + oException.Message);
            }
        }

    }
        // create neural network
       
}
