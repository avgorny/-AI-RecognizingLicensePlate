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
using AForge.Imaging.Filters;

namespace ocr
{
    public class PlateNoOcr
    {
        //instance of singelton
        private static PlateNoOcr instance;
        // pattern size
        private int patternSize = 35;
        // patterns count
        private int patterns = 9;
        //field for network
        private AForge.Neuro.ActivationNetwork Network;

        private Dictionary<int, char> resultDictionary = new Dictionary<int, char>();

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



        #region manual lerning data
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
        #endregion

        /// <summary>
        /// This method learn NN how to ocr basis on fileds placed in cals fields 
        /// </summary>
        /// <returns>Amount of learning cycles</returns>
        public int init()
        {
            double[][] learningData = CreateLearningMatrix();
            double[][] outputs = CreateExpectedResult(learningData);            
            patterns = learningData.GetLength(0);
            
            AForge.Neuro.ActivationNetwork neuralNet =
                new AForge.Neuro.ActivationNetwork(new AForge.Neuro.BipolarSigmoidFunction(0.75), patternSize, patterns, patterns);
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
                //error = teacher.RunEpoch(input, output);
                error = teacher.RunEpoch(learningData, outputs);
                Console.WriteLine(error);
                i++;
            }
            while (error > 0.05);
            Network = neuralNet;
            return i;   
        }

        /// <summary>
        /// OCR Bitmap with one character using teached neural network
        /// </summary>
        /// <param name="mBitmap"></param>
        /// <returns></returns>
        public int OcrImage(Bitmap mBitmap)
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
            return maxIndex;
           
        }

        /// <summary>
        /// This method creat table with floats [0,1] 0-black 1-white
        /// </summary>
        /// <param name="mBitmap"></param>
        /// <returns>table of values from range [0,1]</returns>
        private double[] CreteInputMatrix(Bitmap mBitmap)
        {
            double[] result = new double[35];
            int i = 0;
            List<PointsStartEnd> PointsList = CreatePointsTable(mBitmap);
            foreach (PointsStartEnd pointSet in PointsList)
            {
                //tu jest kłopot czasem dostaje za dużo punktów i wychodzi siatka 5x8 patrz CreatePointsTable
                //zdecydowanie do poprawy, powoduje błedy typu Z odczytane jako 7
                if (i < 35)
                {
                    result[i] = CountBlackWhite(pointSet, mBitmap);
                    i++;
                }

            }
            return result;
        }

        /// <summary>
        /// This method determine latice (siatke) on character image
        /// </summary>
        /// <param name="img"></param>
        /// <returns>list of points with first and last Point from space</returns>
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

        /// <summary>
        /// This method returns coverage with blacke points on one squre defined by points
        /// </summary>
        /// <param name="pointSet"></param>
        /// <param name="bitMap"></param>
        /// <returns></returns>
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
            return result;
        }

        /// <summary>
        /// This method is preparing Learning Matrix basis on image from LearningPictures
        /// </summary>
        /// <returns></returns>
        private double[][] CreateLearningMatrix()
        {
            //loading and cuting learning matrix for each character
            GraphicProcessing graph = new GraphicProcessing();
            Bitmap mBitmap;

            System.Drawing.Image img = histogramAforge.Properties.Resources.learningAll;
            mBitmap = new Bitmap(img);
            Grayscale greyScaleFilter = new Grayscale(1.0, 0.0, 0.0);
            mBitmap = greyScaleFilter.Apply(mBitmap);
            Threshold tresholdFilter = new Threshold(120);
            tresholdFilter.ApplyInPlace(mBitmap);
            List<Bitmap> listOfCharacters = graph.ProcesLearningImage(mBitmap);
            //crate input matrix for each character to learn NN
            double[][] learnigTable = new double[listOfCharacters.Count][];
            int i = 0;
            
            foreach (Bitmap map in listOfCharacters)
            {
                learnigTable[i] = CreteInputMatrix(map);
                i++;
            }
            
            return learnigTable;
        }

        /// <summary>
        /// Create expected outputs basis on learning data
        /// </summary>
        /// <param name="learningTable"></param>
        private double[][] CreateExpectedResult(double[][] learningTable)
        {
            //first dimension size
            int size1 = learningTable.GetLength(0);            
            double[][] expectedOutputs = new double[size1][];            
            for(int index = 0; index<size1;index++)
            {
                expectedOutputs[index] = new double[size1];
            }
            for(int i = 0; i<size1 ;i++)
                for(int j = 0; j<size1; j++)
                {
                    if(i==j)
                    {
                        expectedOutputs[i][j] = 1;
                    }else
                    {
                        expectedOutputs[i][j] = 0;
                    }
                    
                }
            return expectedOutputs;
        }


        /// <summary>
        /// This metod create dictinary with key with output value and as key and char ASCII as value
        /// </summary>
        /// <param name="amountOfOutputs"></param>
        private void CreateDictionary(int amountOfOutputs)
        {
            int character = 48; // 0 ASCII in decimal
            for(int i=0; i<amountOfOutputs; i++)
            {
                if(character<58)
                {
                    resultDictionary.Add(i, (char)character);
                }else
                {
                    if (character == 81-7)
                    {
                        character++;
                    }                        
                    resultDictionary.Add(i, (char)(character+7));
                }
                character++;
            }
        }

        /// <summary>
        /// Create a String format of plate basis on output from NN
        /// </summary>
        /// <param name="neuralOutputs"></param>
        /// <returns></returns>
        public string CreateFullResult(List<int> neuralOutputs)
        {
            if(resultDictionary.Count<10)
                CreateDictionary(patternSize);
            string result="";
            for(int i=0;i<neuralOutputs.Count;i++)
            {
                result += resultDictionary[neuralOutputs[i]];
            }
            return result;
        }

        #region serialization
        /// <summary>
        /// This method is saving learned NN from this class to binary file 
        /// </summary>
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

        /// <summary>
        /// This method is Loading saved and teached NN 
        /// </summary>
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
        #endregion
    }


}
