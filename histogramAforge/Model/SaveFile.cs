using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace histogramAforge.Model
{
    public class SaveFile
    {
        private string savePath;

        public SaveFile()
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.savePath = folderBrowserDialog1.SelectedPath;

            }
        }

        
        public void saveImages(Bitmap mbitmap, int startX)
        {
            mbitmap.Save(savePath+"\\imageSecond" + startX + ".bmp");
        }

        public void saveImages(Bitmap mbitmap, int startX, string additionalPath)
        {
            mbitmap.Save(savePath + additionalPath+ "img" + startX + ".bmp");
        }

    }
}
