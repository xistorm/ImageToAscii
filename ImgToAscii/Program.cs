using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;

namespace ImgToAscii
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //для записи
            string imagePath = "image.jpg";
            string txtPath = "ascii.txt";
            string resImagePath = "ascii.png";
            StreamWriter sw = new StreamWriter(txtPath, false, System.Text.Encoding.Default);
            
            //набор символов
            string ASCII = " \'.\",:;!~+-xmo*W&8@";
            double coef = (ASCII.Length - 1.0) / 255;
            
            //открываем файл
            Bitmap temp = (Bitmap)Image.FromFile(imagePath);
            
            //делаем изображение подходящих размеров
            int maxWidth = 120;
            int maxHeight = 60;
            double scale = (maxWidth + .0) / temp.Width;
            if (temp.Height * scale > maxHeight)
                scale = (maxHeight + .0) / temp.Height;
            
            Bitmap image = new Bitmap(temp, new Size((int)Math.Ceiling(temp.Width * scale), (int)Math.Ceiling(temp.Height * scale)));

            //делаем изображение чёрно-белым
            ToGreyScale(ref image);
            
            //создаём текстовый файл с символами
            for (int y = 0; y < image.Height; ++y)
            {
                for (int x = 0; x < image.Width; ++x)
                {
                    sw.Write(ASCII[(int)Math.Floor(image.GetPixel(x, y).R * coef)]);
                }
                sw.WriteLine();
            }
            sw.Close();

            //текстовый вариант в виде картинки
            //var res = ConvertTextToImage(new StreamReader(txtPath), image.Height * 10, image.Width * 10);
            

            //сохраняем и закрываем
            image.Save("temp.png");
            image.Dispose();
        }

        /// <summary>
        /// ASCII ТЕКСТ В КАРТИНКУ
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static Bitmap ConvertTextToImage(StreamReader sr, int height, int width)
        {
            Bitmap bmp = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                Font font = new Font("Times New Roman", 10);
                graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, width, height);
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.DrawString(sr.ReadToEnd(), font, new SolidBrush(Color.Black), 0, 0);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();
            }

            return bmp;
        }

        /// <summary>
        /// переделывает битмап изображение в чёрно-белое
        /// </summary>
        /// <param name="image"></param>
        public static void ToGreyScale(ref Bitmap image)
        {
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color c = image.GetPixel(x, y);
                    int rgb = (int) Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    image.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
            }
        }
    }
}