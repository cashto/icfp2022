using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using IcfpUtils;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    class Block
    {
        public int x { get; set; }
        public int y { get; set; }
        public int y1 { get; set; }
        public int y2 { get; set; }
        public int block_size { get; set; }
    }

    class Image
    {
        public Image(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.values = new int[width, height, 4];
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        Int32[,,] values;

        public static Image Load(string filename)
        {
            using (var img = new MagickImage(filename))
            {
                var ans = new Image(img.Width, img.Height);
                var pixels = img.GetPixels();

                for (var y = 0; y < ans.Height; ++y)
                {
                    for (var x = 0; x < ans.Width; ++x)
                    {
                        var pixel = pixels.GetPixel(x, y);
                        ans.values[x, y, 0] = pixel[0];
                        ans.values[x, y, 1] = pixel[1];
                        ans.values[x, y, 2] = pixel[2];
                        ans.values[x, y, 3] = pixel[3];
                    }
                }

                return ans;
            }
        }

        public void Save(string filename)
        {
            using (var img = new MagickImage(new MagickColor("#000000"), Width, Height))
            {
                var pixels = img.GetPixels();
                for (var y = 0; y < Height; ++y)
                {
                    for (var x = 0; x < Width; ++x)
                    {
                        var pixel = new byte[4] {
                            (byte)values[x, y, 0],
                            (byte)values[x, y, 1],
                            (byte)values[x, y, 2],
                            (byte)values[x, y, 3]
                        };

                        pixels.SetPixel(x, y, pixel);
                    }
                }

                img.Write(filename);
            }
        }

        public Int32[] Get(int x, int y)
        {
            return new Int32[4] { values[x, y, 0], values[x, y, 1], values[x, y, 2], values[x, y, 3] };
        }

        public void Set(int x, int y, Int32[] pixel)
        {
            this.values[x, y, 0] = pixel[0];
            this.values[x, y, 1] = pixel[1];
            this.values[x, y, 2] = pixel[2];
            this.values[x, y, 3] = pixel[3];
        }

        public IEnumerable<Int32[]> Enumerate(int x, int y, int cx, int cy, Image mask = null)
        {
            for (var iy = 0; iy < cy; ++iy)
            {
                for (var ix = 0; ix < cx; ++ix)
                {
                    if (mask == null || mask.Get(ix, iy)[0] == 0)
                    {
                        yield return new Int32[] {
                            values[x + ix, y + iy, 0],
                            values[x + ix, y + iy, 1],
                            values[x + ix, y + iy, 2],
                            values[x + ix, y + iy, 3]
                        };
                    }
                }
            }
        }

        public void Fill(int x, int y, int cx, int cy, Int32[] p)
        {
            for (var iy = 0; iy < cy; ++iy)
            {
                for (var ix = 0; ix < cx; ++ix)
                {
                    values[x + ix, y + iy, 0] = p[0];
                    values[x + ix, y + iy, 1] = p[1];
                    values[x + ix, y + iy, 2] = p[2];
                    values[x + ix, y + iy, 3] = p[3];
                }
            }
        }

        public double Diff(Image other)
        {
            var penalty = 0.0;
            for (var y = 0; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    penalty += distance(Get(x, y), other.Get(x, y));
                }
            }

            return penalty * 0.005;
        }
    }

    static double distance(Int32[] p, Int32[] q)
    {
        var d0 = p[0] - q[0];
        var d1 = p[1] - q[1];
        var d2 = p[2] - q[2];
        var d3 = p[3] - q[3];
        return Math.Sqrt(d0 * d0 + d1 * d1 + d2 * d2 + d3 * d3);
    }

    struct Swap
    {
        int x1;
        int y1;
        int x2;
        int y2;
    }

    struct Paint
    {
        public int x;
        public int y;
        public Int32[] pixel;
    }

    Tuple<Image, List<Swap>> FindSwaps(Image image)
    {
        throw new NotImplementedException();
    }

    Image ApplySwaps(Image image, List<Swap> swaps)
    {
        throw new NotImplementedException();
    }

    static Int32[] Average(IEnumerable<Int32[]> pixels)
    {
        var ans = new Int32[4] { 0, 0, 0, 0 };
        var n = 0;
        foreach (var pixel in pixels)
        {
            ans[0] += pixel[0];
            ans[1] += pixel[1];
            ans[2] += pixel[2];
            ans[3] += pixel[3];
            ++n;
        }

        if (n > 0)
        {
            ans[0] /= n;
            ans[1] /= n;
            ans[2] /= n;
            ans[3] /= n;
        }

        return ans;
    }

    struct PaintOption
    {
        public int[] avgPixels;
        public int[] paintPixels;

        public PaintOption(int[] avgPixels, int[] paintPixels)
        {
            this.avgPixels = avgPixels;
            this.paintPixels = paintPixels;
        }
    }

    static readonly List<PaintOption> PaintOptions = new List<PaintOption>()
    {
        new PaintOption(new int[]{ 0, 1, 2, 3 }, new int[] {}),
        new PaintOption(new int[]{ 1, 2, 3 }, new int[] { 0 }),
        new PaintOption(new int[]{ 0, 2, 3 }, new int[] { 1 }),
        new PaintOption(new int[]{ 0, 1, 3 }, new int[] { 2 }),
        new PaintOption(new int[]{ 0, 1, 2 }, new int[] { 3 }),
        new PaintOption(new int[]{ 0, 1 }, new int[] { 2, 3 }),
        new PaintOption(new int[]{ 0, 2 }, new int[] { 1, 3 }),
        new PaintOption(new int[]{ 0, 3 }, new int[] { 1, 2 }),
        new PaintOption(new int[]{ 1, 2 }, new int[] { 0, 3 }),
        new PaintOption(new int[]{ 1, 3 }, new int[] { 0, 2 }),
        new PaintOption(new int[]{ 2, 3 }, new int[] { 0, 1 }),
        new PaintOption(new int[]{}, new int[] { 0, 1, 2, 3 })
    };

    static Tuple<Image, List<Paint>> FindPaints(Image image)
    {
        var ans = new Image(image.Width / 2, image.Height / 2);
        var paints = new List<Paint>();

        var block_size = 400 / image.Width;

        for (var y = 0; y < ans.Height; ++y)
        {
            for (var x = 0; x < ans.Width; ++x)
            {
                Int32[][] pixels = new Int32[][]
                {
                    image.Get(2 * x + 0, 2 * y + 0),
                    image.Get(2 * x + 1, 2 * y + 0),
                    image.Get(2 * x + 0, 2 * y + 1),
                    image.Get(2 * x + 1, 2 * y + 1)
                };

                var sorted_options =
                    from option in PaintOptions
                    let avg_pixel = Average(option.avgPixels.Select(idx => pixels[idx]))
                    let penalty = 0.005 * block_size * block_size * option.avgPixels.Sum(idx => distance(pixels[idx], avg_pixel)) +
                        100 * 400 * 400 / (block_size * block_size) * option.paintPixels.Length
                    orderby penalty
                    select option;

                var best_option = sorted_options.First();

                ans.Set(x, y, Average(best_option.avgPixels.Select(idx => pixels[idx])));
                foreach (var paint in best_option.paintPixels)
                {
                    var ix = 2 * x + (paint & 1);
                    var iy = 2 * y + (paint >> 1);
                    paints.Add(new Paint() { x = ix, y = iy, pixel = pixels[paint] });
                }
            }
        }

        return Tuple.Create(ans, paints);
    }

    static Image ApplyPaints(Image image, List<Paint> paints)
    {
        var ans = new Image(image.Width * 2, image.Height * 2);
        for (var y = 0; y < image.Width; ++y)
        {
            for (var x = 0; x < image.Width; ++x)
            {
                var pixel = image.Get(x, y);
                ans.Set(x * 2 + 0, y * 2 + 0, pixel);
                ans.Set(x * 2 + 1, y * 2 + 0, pixel);
                ans.Set(x * 2 + 0, y * 2 + 1, pixel);
                ans.Set(x * 2 + 1, y * 2 + 1, pixel);
            }
        }

        foreach (var paint in paints)
        {
            ans.Set(paint.x, paint.y, paint.pixel);
        }

        return ans;
    }

    struct Rectangle
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;

        public Rectangle(int x1, int y1, int x2, int y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }

        public bool IsValid()
        {
            return x1 >= 0 && y1 >= 0 && x2 < 400 && y2 < 400 && x2 > x1 && y2 > y1;
        }

        public Dictionary<string, int> ToDict()
        {
            return new Dictionary<string, int>()
            {
                { "x", x1 },
                { "y", 399 - y1 - (y2 - y1)},
                { "dx", x2 - x1 },
                { "dy", y2 - y1 }
            };
        }
    }

    static IEnumerable<Int32[]> GetRectangleColors(Image image, IEnumerable<Rectangle> rects)
    {
        var mask = new Image(image.Width, image.Height);
        var mask_pixel = new int[] { 1, 1, 1, 1 };

        foreach (var r in rects)
        {
            yield return Average(image.Enumerate(r.x1, r.y1, r.x2 - r.x1, r.y2 - r.y1, mask));
            mask.Fill(r.x1, r.y1, r.x2 - r.x1, r.y2 - r.y1, mask_pixel);
        }

        yield return Average(image.Enumerate(0, 0, image.Width, image.Height, mask));
    }

    class SearchState
    {
        public Random Random { get; set; }
        public Image OriginalImage { get; set; }
        public List<Rectangle> Rectangles { get; set; }
        public double Penalty { get; set; }
        public double PixelPenalty { get; set; }

        public SearchState(Random random, Image originalImage, IEnumerable<Rectangle> rectangles)
        {
            Random = random;
            OriginalImage = originalImage;
            Rectangles = rectangles.ToList();

            PixelPenalty = Paint().Diff(OriginalImage);
            var rectanglePenalties =
                from r in Rectangles
                let penalty = 6.0 * 400.0 * 400.0 / (r.x2 - r.x1) / (r.y2 - r.y1)
                select penalty;

            var rectanglePenalty = Rectangles.Count * 10; // rectanglePenalties.Sum() + (30 - Rectangles.Count) * 1000000;
            Penalty = PixelPenalty + rectanglePenalty;
        }

        public Image Paint()
        {
            var colors = GetRectangleColors(OriginalImage, Rectangles).ToList();
            var newImage = new Image(OriginalImage.Width, OriginalImage.Height);
            newImage.Fill(0, 0, newImage.Width, newImage.Height, colors.Last());

            for (var i = Rectangles.Count; i > 0; --i)
            {
                var r = Rectangles[i - 1];
                var pixel = colors[i - 1];
                newImage.Fill(r.x1, r.y1, r.x2 - r.x1, r.y2 - r.y1, pixel);
            }

            return newImage;
        }
    };

    static readonly NoMove NullMove = new NoMove();

    static Rectangle MutateRectagle(Rectangle r, Random random)
    {
        while (true)
        {
            int dx = random.Next(-3, 6);
            int dy = random.Next(-3, 6);
            int x = r.x1 + random.Next(-2, 5) - dx / 2;
            int y = r.y1 + random.Next(-2, 5) - dy / 2;
            int width = r.x2 - r.x1 + dx;
            int height = r.y2 - r.y1 + dy;

            var ans = new Rectangle(x, y, x + width, y + height);
            if (ans.IsValid())
            {
                return ans;
            }
        }
    }

    static readonly List<Rectangle> EmptyRectangleList = new List<Rectangle>();

    static IEnumerable<SearchNode<SearchState, NoMove>> GenerateNewStates(SearchNode<SearchState, NoMove> searchNode)
    {
        var oldState = searchNode.State;
        var random = oldState.Random;
        var newList = EmptyRectangleList;

        var iters = 10; // oldState.Random.Next(0, 25) == 0 ? 2 : 1;
        foreach (var i in Enumerable.Range(0, iters))
        {
            var newRects = new List<Rectangle>();

            if (oldState.Rectangles.Count == 0 || random.Next(0, 25) == 0)
            {
                var x = random.Next(0, 395);
                var y = random.Next(0, 395);
                newRects.Add(new Rectangle(x, y, x + 5, y + 5));
            }

            newRects.AddRange(oldState.Rectangles);

            if (newRects.Count > 30)
            {
                newRects.RemoveAt(oldState.Random.Next(0, newRects.Count));
            }

            var mutate = random.Next(0, newRects.Count);
            newRects[mutate] = MutateRectagle(newRects[mutate], random);
            
            if (random.Next(10) == 0)
            {
                var idx = random.Next(0, newRects.Count);
                var t = newRects[idx];
                newRects.RemoveAt(idx);
                newRects.Insert(0, t);
            }

            var newState = new SearchState(
                oldState.Random,
                oldState.OriginalImage,
                newRects);
            yield return searchNode.Create(newState, NullMove);
        }
    }

    static List<Rectangle> EliminateRects(Image image, List<Rectangle> rects, int howMany)
    {
        var mask = new Image(image.Width, image.Height);
        var mask_pixel = new int[] { 1, 1, 1, 1 };

        var pixelCounts = new List<Tuple<Rectangle, int>>();
        foreach (var r in rects)
        {
            pixelCounts.Add(Tuple.Create(r, image.Enumerate(r.x1, r.y1, r.x2 - r.x1, r.y2 - r.y1, mask).Count()));
            mask.Fill(r.x1, r.y1, r.x2 - r.x1, r.y2 - r.y1, mask_pixel);
        }

        var sorted_rects =
            from i in pixelCounts
            let rect = i.Item1
            let pixelCount = i.Item2 // ((rect.x2 - rect.x1) * (rect.y2 - rect.y1))
            orderby pixelCount
            select rect;

        return sorted_rects.Skip(howMany).ToList();
    }

    static void Main(string[] args)
    {
        var random = new Random();
        var src = Image.Load("input.png");
        var originalRects = new List<Rectangle>();
        if (File.Exists("output.json"))
        {
            var in_json = JToken.Parse(File.ReadAllText("output.json"));
            foreach (var r in in_json["rects"])
            {
                var x1 = (int)r["x"];
                var width = (int)r["dx"];
                var height = (int)r["dy"];
                var y1 = 399 - (int)r["y"] - height;

                originalRects.Add(new Rectangle(x1, y1, x1 + width, y1 + height));
            }
        }

        var originalState = new SearchState(random, src, originalRects);

        var searchNodes = IcfpUtils.Algorithims.Search(
            originalState,
            BestFirstSearch.Create<SearchState, NoMove>((a, b) => a.State.Penalty > b.State.Penalty, 1000),
            CancellationToken.None,
            GenerateNewStates);

        SearchState best_state = null;
        var best_penalty = Double.PositiveInfinity;

        var searchNodeEnum = searchNodes.GetEnumerator();
        for (var i = 1; i < 5000; ++i)
        {
            searchNodeEnum.MoveNext();
            if (i % 100 == 0)
            {
                //searchNodeEnum.Current.State.Paint().Save($"intermediate-{i}.png");
                Console.WriteLine($"{searchNodeEnum.Current.State.PixelPenalty} / {searchNodeEnum.Current.State.Penalty}");
            }

            if (searchNodeEnum.Current.State.Penalty < best_penalty)
            {
                best_state = searchNodeEnum.Current.State;
                best_penalty = searchNodeEnum.Current.State.Penalty;
            }

            //if (i % 1000 == 0)
            //{
            //    // Restart search
            //    searchNodes = IcfpUtils.Algorithims.Search(
            //        new SearchState(random, src, EliminateRects(src, best_state.Rectangles, 5)),
            //        BestFirstSearch.Create<SearchState, NoMove>((a, b) => a.State.Penalty > b.State.Penalty, 1000),
            //        CancellationToken.None,
            //        GenerateNewStates);
            //    searchNodeEnum = searchNodes.GetEnumerator();
            //}
        }

        best_state.Rectangles = EliminateRects(src, best_state.Rectangles, 5);
        var dst = best_state.Paint();

        dst.Save("output.png");
        var json = new Dictionary<string, object>() { { "rects", best_state.Rectangles.Reverse<Rectangle>().Select(i => i.ToDict()) } };
        File.WriteAllText("output.json", JsonConvert.SerializeObject(json));

        var img_diff = new Image(src.Width, src.Height);
        for (var y = 0; y < src.Height; ++y)
        {
            for (var x = 0; x < src.Width; ++x)
            {
                var d = (byte)Math.Min(distance(src.Get(x, y), dst.Get(x, y)) / 2, 255);
                img_diff.Set(x, y, new int[] { d, d, d, 255 });
            }
        }
        img_diff.Save("diff.png");

        Console.WriteLine($"Penalty: {src.Diff(dst)} / {best_state.Penalty}");
    }
}
