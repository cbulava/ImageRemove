using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageRemove
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var patternFileLocation = "";
            var baseImageFileLocation = "";


            var pattern = (Bitmap)Image.FromFile(patternFileLocation);
            var baseImage = (Bitmap)Image.FromFile(baseImageFileLocation);
            var imageToChange = baseImage;

            //replace pixels with pixels similar to the surrounding area
            //need to ignore white space

            for(int basex = 0; basex < baseImage.Width; basex++)
            {
                for(int basey = 0; basey < baseImage.Height; basey++)
                {
                    var basePixelColor = baseImage.GetPixel(basex, basey);
                    //do compare for every pixel for now
                    for(int patternx = 0; patternx < pattern.Width; patternx++)
                    {
                        for(int patterny = 0; patterny < baseImage.Height; patterny++)
                        {
                            if(basePixelColor == pattern.GetPixel(patternx, patterny) && basePixelColor != Color.White)
                            {
                                var replacement = CreateLocalColor(baseImage, basex, basey);
                                imageToChange.SetPixel(basex, basey, replacement);
                            }
                        }
                    }


                    if(baseImage.GetPixel(basex, basey) == Color.White)
                    {
                        break;
                    }
                }
            }
        }

        public static Color CreateLocalColor(Bitmap baseImage, int bx, int by)
        {
            //go one in every direction

            Color bitleft = Color.White, 
                  bitright = Color.White, 
                  bitup = Color.White,
                  bitdown = Color.White,
                  bitleftup = Color.White,
                  bitrightup = Color.White,
                  bitleftdown = Color.White,
                  bitrightdown = Color.White;

            var colors = new List<Color>(8);
            var labColors = new List<Lab>(8);

            if ((bx - 1) >= 0)
            {
                colors[0] = baseImage.GetPixel(bx - 1, by);

                if ((by + 1) < baseImage.Height)
                    colors[1] = baseImage.GetPixel(bx - 1, by + 1);
                if ((by - 1) >= 0)
                    colors[2] = baseImage.GetPixel(bx - 1, by - 1);
            }
            if ((bx + 1) < baseImage.Width)
            {
                colors[3] = baseImage.GetPixel(bx + 1, by);

                if ((by + 1) < baseImage.Height)
                    colors[4] = baseImage.GetPixel(bx - 1, by + 1);
                if ((by - 1) >= 0)
                    colors[5] = baseImage.GetPixel(bx - 1, by - 1);
            }

            if ((by - 1) >= 0)
                colors[6] = baseImage.GetPixel(bx, by - 1);
            if ((by + 1) < baseImage.Height)
                colors[7] = baseImage.GetPixel(bx, by + 1);

            colors.ForEach(color => labColors.Add(ConvertToLabSpace(color)));

            var avg = new Lab(0, 0, 0);

            labColors.ForEach(lab =>
            {
                avg = avg + lab;
            });

            avg = avg / 8.0;

            return ConvertToRgbSpace(avg);
        }

        public static Lab ConvertToLabSpace(Color rgb)
        {
            var x = rgb.R / 255.0;
            var y = rgb.G / 255.0;
            var z = rgb.B / 255.0;

            var xn = 95.0489;
            var yn = 100;
            var zn = 108.8840;

            var L = 116 * labfunc(y / yn) - 16;
            var a = 500 * (labfunc(x / xn) - labfunc(y / yn));
            var b = 200 * (labfunc(y / yn) - labfunc(z / zn));

            return new Lab(L, a, b);
        }
        public static Color ConvertToRgbSpace(Lab lab)
        {
            var xn = 95.0489;
            var yn = 100;
            var zn = 108.8840;

            var R = xn * inverselabfunc(((lab.L + 16) / 116.0) + (lab.a / 500.0));
            var G = yn * inverselabfunc((lab.L + 16) / 116.0);
            var B = zn * inverselabfunc(((lab.L + 16) / 116.0) - (lab.b / 200.0));

            return Color.FromArgb((int)(R * 255), (int)(G * 255), (int)(B * 255));
        }
        public static double labfunc(double t)
        {
            var lambda = (6.0 / 29.0);

            if (t > Math.Pow(lambda, 3))
            {
                return Math.Pow(t, (1.0 / 3.0));
            }
            else
            {
                return (t / (3 * Math.Pow(lambda, 2))) + (4.0 / 29.0);
            }
        }

        public static double inverselabfunc(double t)
        {
            var lambda = 6.0 / 29.0;

            if(t > lambda)
            {
                return Math.Pow(t, 3);
            }
            else
            {
                return (3.0 * Math.Pow(lambda, 2)) * (t - (4.0 / 29.0));
            }
        }

    }

    public class Lab
    {
        public double L;
        public double a;
        public double b;

        public Lab(double lvalue, double avalue, double bvalue)
        {
            L = lvalue;
            a = avalue;
            b = bvalue;
        }

        public static Lab operator +(Lab a, Lab b)
        {
            return new Lab(a.L + b.L, a.a + b.a, a.b + b.b);
        }

        public static Lab operator /(Lab a, double b)
        {
            return new Lab(a.L / b, a.a / b, a.b / b);
        }
    }
}
