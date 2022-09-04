using ImageMagick;
using System;

namespace imgdiff
{
    class Program
    {
        static double distance(int r, int g, int b)
        {
            return Math.Sqrt(r * r + g * g + b * b);
        }

        static void Main(string[] args)
        {
            var root = @"c:\Users\cashto\Documents\GitHub\icfp2022\work\idiff";
            var penalty = 0.0;
            using (var old_img = new MagickImage($"{root}\\old.png"))
            {
                using (var new_img = new MagickImage($"{root}\\new.png"))
                {
                    using (var diff_img = new MagickImage(new MagickColor("#000000"), 400, 400))
                    {
                        for (var x = 0; x < 400; ++x)
                        {
                            for (var y = 0; y < 400; ++y)
                            {
                                var old_pixel = old_img.GetPixels().GetPixel(x, y);
                                var new_pixel = new_img.GetPixels().GetPixel(x, y);
                                var d = distance(old_pixel[0] - new_pixel[0], old_pixel[1] - new_pixel[1], old_pixel[2] - new_pixel[2]);
                                var diff = (byte)Math.Min((int)d, 255);
                                penalty += diff;
                                diff_img.GetPixels().SetPixel(x, y, new byte[]{ diff, diff, diff} );
                            }
                        }

                        
                        diff_img.Write($"{root}\\diff.png");
                    }
                }

                Console.WriteLine($"Penalty: {penalty * 0.005}");
            }
        }
    }
}
