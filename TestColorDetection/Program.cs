using System;
using System.Drawing;
using System.IO;
using EVE预警;

namespace TestColorDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== EVE预警 颜色检测测试 ===\n");

            // 使用用户提供的测试图片
            string testImagePath = @"D:\PROJECT\EVE预警\8regk28c.png";
            
            if (!File.Exists(testImagePath))
            {
                Console.WriteLine($"错误：未找到测试图片 {testImagePath}");
                return;
            }

            Console.WriteLine($"使用测试图片：{testImagePath}");

            using (Bitmap testImage = new Bitmap(testImagePath))
            {
                Console.WriteLine($"图片尺寸：{testImage.Width}x{testImage.Height}\n");

                // 查找图片中的红色和橙色区域
                Console.WriteLine("=== 分析图片中的红色区域 ===");
                FindRedRegions(testImage);
                Console.WriteLine();

                Console.WriteLine("开始检测...");
                var colorCounts = ImageProcessor.DetectAlertColorsFromTemplates(testImage);

                Console.WriteLine("\n=== 检测结果 ===");
                int totalDetected = 0;
                
                foreach (var kvp in colorCounts)
                {
                    if (kvp.Value > 0)
                    {
                        totalDetected += kvp.Value;
                        string message = ImageProcessor.GetAlertMessage(kvp.Key);
                        Console.WriteLine($"{kvp.Key}: {kvp.Value} 个 - {message}");
                    }
                }

                Console.WriteLine($"\n总计检测到：{totalDetected} 个色块");
                Console.WriteLine("\n期望结果：");
                Console.WriteLine("- 19个灰色减号 (中立声望)");
                Console.WriteLine("- 2个红色减号 (糟糕声望) - GTX 980TI 和 Microsoft");
                Console.WriteLine("- 2个蓝色加号 (良好声望) - 其他玩家");
            }
        }

        static void FindRedRegions(Bitmap image)
        {
            int redPixelCount = 0;
            int orangePixelCount = 0;
            
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    
                    // 检测红色像素 (R高，G和B低)
                    if (pixel.R > 150 && pixel.G < 80 && pixel.B < 80)
                    {
                        redPixelCount++;
                        if (redPixelCount <= 5)
                        {
                            Console.WriteLine($"红色像素 [{x},{y}]: RGB({pixel.R}, {pixel.G}, {pixel.B})");
                        }
                    }
                    // 检测橙色像素 (R和G高，B低)
                    else if (pixel.R > 180 && pixel.G > 100 && pixel.B < 80)
                    {
                        orangePixelCount++;
                        if (orangePixelCount <= 5)
                        {
                            Console.WriteLine($"橙色像素 [{x},{y}]: RGB({pixel.R}, {pixel.G}, {pixel.B})");
                        }
                    }
                }
            }
            
            Console.WriteLine($"\n总红色像素: {redPixelCount}");
            Console.WriteLine($"总橙色像素: {orangePixelCount}");
        }

        static void AnalyzeImageColors(Bitmap image)
        {
            Console.WriteLine($"图片尺寸: {image.Width}x{image.Height}");
            Console.WriteLine("像素格式: " + image.PixelFormat);
            
            // 采样分析颜色
            int sampleCount = 0;
            int redCount = 0;
            int grayCount = 0;
            int blueCount = 0;
            int otherCount = 0;
            
            for (int y = 0; y < image.Height; y += 5)
            {
                for (int x = 0; x < image.Width; x += 5)
                {
                    Color pixel = image.GetPixel(x, y);
                    sampleCount++;
                    
                    // 简化颜色分类
                    if (pixel.R > 150 && pixel.G < 80 && pixel.B < 80)
                        redCount++;
                    else if (pixel.R < 100 && pixel.G < 100 && pixel.B > 150)
                        blueCount++;
                    else if (Math.Abs(pixel.R - pixel.G) < 20 && Math.Abs(pixel.G - pixel.B) < 20)
                        grayCount++;
                    else
                        otherCount++;
                }
            }
            
            Console.WriteLine($"采样点数: {sampleCount}");
            Console.WriteLine($"红色像素: {redCount} ({redCount * 100 / sampleCount}%)");
            Console.WriteLine($"蓝色像素: {blueCount} ({blueCount * 100 / sampleCount}%)");
            Console.WriteLine($"灰色像素: {grayCount} ({grayCount * 100 / sampleCount}%)");
            Console.WriteLine($"其他像素: {otherCount} ({otherCount * 100 / sampleCount}%)");
        }

        static void AnalyzeTemplates()
        {
            string[] templateFiles = { "1.png", "2.png", "3.png", "4.png", "5.png" };
            
            foreach (string file in templateFiles)
            {
                string path = Path.Combine(@"D:\PROJECT\EVE预警", file);
                if (File.Exists(path))
                {
                    using (Bitmap bmp = new Bitmap(path))
                    {
                        Console.WriteLine($"\n{file}: {bmp.Width}x{bmp.Height}");
                        
                        // 分析主要颜色
                        int sampleCount = 0;
                        int totalR = 0, totalG = 0, totalB = 0;
                        
                        for (int y = 0; y < bmp.Height; y++)
                        {
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                Color pixel = bmp.GetPixel(x, y);
                                totalR += pixel.R;
                                totalG += pixel.G;
                                totalB += pixel.B;
                                sampleCount++;
                            }
                        }
                        
                        if (sampleCount > 0)
                        {
                            Console.WriteLine($"  平均颜色: RGB({totalR/sampleCount}, {totalG/sampleCount}, {totalB/sampleCount})");
                        }
                    }
                }
            }
        }
    }
}
