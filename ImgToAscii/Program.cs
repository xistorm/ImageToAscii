using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Windows;
using System.Windows.Interop;

namespace ImgToAscii
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //необходимые данные
            string directory = "";
            string fileName = "";
            string imagePath = "";
            string resName = "";
            string txtName = "";

            int maxWidth;
            int maxHeight;
            
            //набор символов
            string ASCII;
            double coef;
            
            //выбор дериктории и файла
            Console.WriteLine("Enter file name or ful path if it is in another directory");
            imagePath = Console.ReadLine();
            imagePath = imagePath.Replace('/', '\\');
            if (imagePath.Contains("\\"))
            {
                directory = imagePath.Remove(imagePath.LastIndexOf('\\'));
                fileName = imagePath.Substring(imagePath.LastIndexOf('\\'));
                fileName = fileName.Remove(fileName.LastIndexOf('.'));
            }
            else
            {
                directory = "";
                fileName = imagePath.Remove(imagePath.LastIndexOf('.'));
            }
            resName = directory + fileName + "_ascii" + imagePath.Substring(imagePath.LastIndexOf('.'));
            txtName = directory + fileName + "_ascii.txt";
            
            //открываем файл
            Bitmap temp = (Bitmap)Image.FromFile(imagePath);

            //выбор качества детализации результата
            Console.Clear();
            Console.WriteLine("Detalization [1] - full resolution, [2] - half-full, [3] - worst");
            {
                int option = int.Parse(Console.ReadLine());
                switch (option)
                {
                    case 1:
                        maxWidth = temp.Width;
                        maxHeight = temp.Height;
                        break;
                    case 2:
                        maxWidth = temp.Width / 2;
                        maxHeight = maxWidth * 9 / 16;
                        break;
                    case 3:
                    default:
                        maxWidth = 320;
                        maxHeight = 180;
                        break;
                }
            }

            //делаем изображение подходящих размеров
            double scale = (maxWidth + .0) / temp.Width;
            if (temp.Height * scale > maxHeight)
                scale = (maxHeight + .0) / temp.Height;

            
            if (imagePath.Contains(".gif"))
                GifConvert(imagePath, scale, resName);
            else
                SimpleImageConvert(temp, scale, txtName, resName).Save(resName);
        }

        public static void GifConvert(string imagePath, double scale, string resName)
        {
            GifBitmapEncoder encoder = new GifBitmapEncoder();
            Image tempImage = Image.FromFile(imagePath);
            FrameDimension dimension = new FrameDimension(tempImage.FrameDimensionsList[0]);
            
            for (int i = 0; i < tempImage.GetFrameCount(dimension); ++i)
            {
                tempImage.SelectActiveFrame(dimension, i);
                var frameBitmap = SimpleImageConvert((Bitmap) tempImage.Clone(), scale, "tmp.txt", "tmp.png");
                var frame = Imaging.CreateBitmapSourceFromHBitmap(
                    frameBitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                encoder.Frames.Add(BitmapFrame.Create(frame));
            }
            
            encoder.Save(new FileStream(resName, FileMode.Create));
        }

        public static Bitmap SimpleImageConvert(Bitmap tempImage, double scale, string txtName, string resName)
        {
            //для записи
            string resImagePath = "ascii.png";
            StreamWriter sw = new StreamWriter(txtName, false, System.Text.Encoding.Default);
            
            Bitmap image = new Bitmap(tempImage, new System.Drawing.Size((int)Math.Ceiling(tempImage.Width * scale), (int)Math.Ceiling(tempImage.Height * scale)));

            //делаем изображение чёрно-белым
            ToGreyScale(ref image);
                
            //подбираем
            string ASCII = " \'.\",:;!~+-xmo*W&8@";
            (int min, int max) palet = (256, 0);
            for (int y = 0; y < image.Height; ++y)
            {
                for (int x = 0; x < image.Width; ++x)
                {
                    if (image.GetPixel(x, y).R > palet.max)
                        palet.max = image.GetPixel(x, y).R;
                    if (image.GetPixel(x, y).R < palet.min)
                        palet.min = image.GetPixel(x, y).R;
                }
            }
            double coef = (ASCII.Length - 1.0) / (palet.max - palet.min);
            
            //создаём текстовый файл с символами
            for (int y = 0; y < image.Height; ++y)
            {
                for (int x = 0; x < image.Width; ++x)
                {
                    var color = image.GetPixel(x, y).R;
                    sw.Write(ASCII[(int)Math.Floor((palet.max - color) * coef)].ToString() + ASCII[(int)Math.Floor((palet.max - color) * coef)].ToString());
                }
                sw.WriteLine();
            }
            sw.Close();

            //текстовый вариант в виде картинки
            return ConvertTextToImage(new StreamReader(txtName), (int)(image.Height * 7), image.Width * 8);
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
                Font font = new Font("Anonymous Pro", 5);
                graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, width, height);
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.DrawString(sr.ReadToEnd(), font, new SolidBrush(Color.Black), 0, 0);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();
            }

            sr.Close();
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