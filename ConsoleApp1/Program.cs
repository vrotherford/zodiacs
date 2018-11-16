using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using TheArtOfDev.HtmlRenderer;
using TheArtOfDev.HtmlRenderer.WinForms;
using System.Drawing.Imaging;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using System.IO;
using TheArtOfDev.HtmlRenderer.Core;
using Pechkin;
using System.Drawing.Printing;
using Pechkin.Synchronized;
using System.Drawing.Drawing2D;
using System.Threading;

namespace ConsoleApp1
{
    class Zotiyacs
    {
        public string zRu { get; set; }
        public string zEn { get; set; }

        private string[] zListEn = { "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo", "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces" };
        private string[] zListRu = { "Овен", "Телец", "Близнецы", "Рак", "Лев", "Дева", "Весы", "Скорпион", "Стрелец", "Козерог", "Водолей", "Рыбы" };

        public Zotiyacs(int number)
        {
            this.zEn = this.zListEn[number];
            this.zRu = this.zListRu[number];
        }

    }
    class HtmlPage
    {
        public string htmlText;

        public HtmlPage(string zotiyac, string sentence, string zEn, string date)
        {
            this.htmlText = renderHtmlCode(zotiyac, sentence, zEn, date);
        }

        public string renderHtmlCode(string zotiyac, string sentence, string zEn, string date)
        {
            string textFromFile;
            using (FileStream fstream = File.OpenRead(@"C:\Users\Boris\source\repos\ConsoleApp1\ConsoleApp1\templates\Templates1.html"))
            {
                byte[] array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                textFromFile = System.Text.Encoding.Default.GetString(array);
            }
            return string.Format(textFromFile, zotiyac, sentence, zEn, "Гороскоп на " + date);
        }

        public string getCssCode()
        {
            string textFromFile;
            using (FileStream fstream = File.OpenRead(@"C:\Users\Boris\source\repos\ConsoleApp1\ConsoleApp1\src\styles\bootstrap.css"))
            {
                byte[] array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                textFromFile = System.Text.Encoding.Default.GetString(array);
            }
            return textFromFile;
        }

    }
    class Program
    {
        public string dayPath { get; set; } 
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static string generateText()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            string[] fSlines = File.ReadAllLines(@"C:\Users\Boris\source\repos\ConsoleApp1\ConsoleApp1\src\texts\FirstSentence.txt");
            string[] sSLines = File.ReadAllLines(@"C:\Users\Boris\source\repos\ConsoleApp1\ConsoleApp1\src\texts\Sentenences.txt");
            string generatedText = fSlines[rand.Next(0, fSlines.Length)];
            for (int i= 0; i < rand.Next(2,4); i++)
            {
                int sNum = rand.Next(0, sSLines.Length);
                if (sSLines[sNum] != null)
                {
                    generatedText += sSLines[sNum];
                    sSLines[sNum] = null;
                }
                else
                    --i;
            }
            return generatedText;
        }

        public static void ConvertHtmlToImage(string htmlCode, string zName, string path)
        {
            var htmlToImageConv = new NReco.ImageGenerator.HtmlToImageConverter();
            var jpegBytes = htmlToImageConv.GenerateImage(htmlCode, "png");
            Image newImage = (Bitmap)((new ImageConverter()).ConvertFrom(jpegBytes));
            Rectangle cropRect = new Rectangle(0, 24, 600,600);
            Bitmap src = newImage as Bitmap;
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 24, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }
            target.Save(Path.Combine(path, zName + "_" + DateTime.Now.ToString("MM-dd") +".png"), ImageFormat.Png);
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            while (true)
            {
                DateTime curDate = DateTime.Now;
                string curDateStr = String.Format("{0:dd MMMM}", curDate);
                program.dayPath = Path.Combine(@"C:\Users\Boris\source\repos\ConsoleApp1\ConsoleApp1\output\images\", DateTime.Now.ToString("MM-dd"));
                Directory.CreateDirectory(program.dayPath);
                for (int i = 0; i < 12; i++)
                {
                    Random rand = new Random((int)DateTime.Now.Ticks);
                    Zotiyacs zotiyac = new Zotiyacs(i);
                    HtmlPage template = new HtmlPage(zotiyac.zRu, generateText(),zotiyac.zEn,curDateStr);
                    ConvertHtmlToImage(template.htmlText, zotiyac.zEn, program.dayPath);
                    Console.WriteLine("Image for {0} generated", zotiyac.zEn);
                    Thread.Sleep(1000);
                }
                Thread.Sleep(24*60*60000);
            }

        }
    }
}
