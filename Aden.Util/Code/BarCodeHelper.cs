using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Aden.Util.Code
{
    public static class BarCodeHelper
    {
        /// <summary>
        /// 生成条形码
        /// </summary>
        /// <param name="barNumber">条形码，位数必须为2的倍数</param>
        public static void CreateBarCode(string barNumber, Stream stream)
        {
            if (barNumber.Length % 2 == 1) barNumber = "0" + barNumber;
            Code128 code = new Code128();
            //code.ValueFont = new Font("Arial", 9); //显示条码文字字体
            code.Height = 38;
            Bitmap image = code.GetCodeImage(barNumber, Code128.Encode.Code128C);
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Gif);
        }
    }
}
