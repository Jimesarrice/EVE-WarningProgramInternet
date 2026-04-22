using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace EVE预警
{
    public class ImageProcessor
    {
        private static readonly object lockObj = new object();
        
        // 缓存已加载的模板
        private static readonly Dictionary<AlertType, Bitmap> TemplateCache = new Dictionary<AlertType, Bitmap>();
        
        /// <summary>
        /// 从嵌入资源加载色块模板
        /// </summary>
        private static Bitmap? LoadTemplateFromResource(AlertType alertType)
        {
            if (TemplateCache.ContainsKey(alertType))
            {
                return TemplateCache[alertType];
            }

            string resourceName = $"{(int)alertType}.png";
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string name in resourceNames)
            {
                if (name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase))
                {
                    using (Stream? stream = assembly.GetManifestResourceStream(name))
                    {
                        if (stream != null)
                        {
                            Bitmap template = new Bitmap(stream);
                            TemplateCache[alertType] = template;
                            return template;
                        }
                    }
                }
            }

            return null;
        }

        private static Bitmap LoadImageFromResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            string[] resourceNames = assembly.GetManifestResourceNames();
            
            foreach (string name in resourceNames)
            {
                if (name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase))
                {
                    using (Stream? stream = assembly.GetManifestResourceStream(name))
                    {
                        if (stream != null)
                        {
                            return new Bitmap(stream);
                        }
                    }
                }
            }
            
            throw new FileNotFoundException($"资源文件 {resourceName} 未找到。可用资源: {string.Join(", ", resourceNames)}");
        }

        /// <summary>
        /// 将Bitmap转换为灰度Mat
        /// </summary>
        private static Mat BitmapToGrayMat(Bitmap bitmap)
        {
            Mat bgraMat = new Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, 4);
            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                int stride = bmpData.Stride;
                int size = stride * bitmap.Height;
                byte[] data = new byte[size];
                Marshal.Copy(bmpData.Scan0, data, 0, size);
                
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    using (Mat tempMat = new Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, 4, handle.AddrOfPinnedObject(), stride))
                    {
                        tempMat.CopyTo(bgraMat);
                    }
                }
                finally
                {
                    handle.Free();
                }

                Mat grayMat = new Mat();
                CvInvoke.CvtColor(bgraMat, grayMat, ColorConversion.Bgra2Gray);
                bgraMat.Dispose();
                return grayMat;
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
        }

        /// <summary>
        /// 将Bitmap转换为BGR Mat
        /// </summary>
        private static Mat BitmapToBgrMat(Bitmap bitmap)
        {
            Mat bgraMat = new Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, 4);
            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                int stride = bmpData.Stride;
                int size = stride * bitmap.Height;
                byte[] data = new byte[size];
                Marshal.Copy(bmpData.Scan0, data, 0, size);
                
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    using (Mat tempMat = new Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, 4, handle.AddrOfPinnedObject(), stride))
                    {
                        tempMat.CopyTo(bgraMat);
                    }
                }
                finally
                {
                    handle.Free();
                }

                Mat bgrMat = new Mat();
                CvInvoke.CvtColor(bgraMat, bgrMat, ColorConversion.Bgra2Bgr);
                bgraMat.Dispose();
                return bgrMat;
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
        }

        /// <summary>
        /// 基于嵌入资源中的模板文件(1~5.png)检测色块 - 使用Emgu.CV模板匹配
        /// 新策略：对每个模板单独进行灰度匹配，然后验证颜色
        /// </summary>
        public static Dictionary<AlertType, int> DetectAlertColorsFromTemplates(Bitmap image)
        {
            Dictionary<AlertType, int> colorCounts = new Dictionary<AlertType, int>();
            
            if (image == null)
                return colorCounts;

            foreach (AlertType type in Enum.GetValues(typeof(AlertType)))
            {
                if (type != AlertType.None)
                {
                    colorCounts[type] = 0;
                }
            }

            // 转换为灰度和BGR Mat
            using (Mat sourceGray = BitmapToGrayMat(image))
            using (Mat sourceBgr = BitmapToBgrMat(image))
            {
                // 对每个模板进行灰度匹配+颜色验证
                foreach (AlertType alertType in Enum.GetValues(typeof(AlertType)))
                {
                    if (alertType == AlertType.None)
                        continue;
                    
                    Bitmap? template = LoadTemplateFromResource(alertType);
                    if (template == null)
                        continue;
                    
                    if (template.Width > image.Width || template.Height > image.Height)
                        continue;
                    
                    // 使用当前模板进行灰度匹配
                    using (Mat templateGray = BitmapToGrayMat(template))
                    using (Mat templateBgr = BitmapToBgrMat(template))
                    using (Mat result = new Mat())
                    {
                        CvInvoke.MatchTemplate(sourceGray, templateGray, result, TemplateMatchingType.CcoeffNormed);

                        int templateWidth = template.Width;
                        int templateHeight = template.Height;
                        int minDist = Math.Max(templateWidth, templateHeight) / 2;

                        // 查找所有匹配位置
                        using (Mat resultCopy = result.Clone())
                        {
                            while (true)
                            {
                                double minVal = 0, maxVal = 0;
                                Point minLoc = new Point(), maxLoc = new Point();
                                CvInvoke.MinMaxLoc(resultCopy, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                                if (maxVal >= 0.85)
                                {
                                    // 验证颜色
                                    if (VerifyColorMatch(sourceBgr, templateBgr, maxLoc))
                                    {
                                        colorCounts[alertType]++;
                                    }

                                    // 将此区域设为0，继续查找
                                    int xStart = Math.Max(0, maxLoc.X - minDist);
                                    int yStart = Math.Max(0, maxLoc.Y - minDist);
                                    int xEnd = Math.Min(resultCopy.Cols - 1, maxLoc.X + minDist);
                                    int yEnd = Math.Min(resultCopy.Rows - 1, maxLoc.Y + minDist);

                                    using (Mat roi = new Mat(resultCopy, new Rectangle(xStart, yStart, xEnd - xStart + 1, yEnd - yStart + 1)))
                                    {
                                        roi.SetTo(new MCvScalar(0));
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    // 注意：不要 Dispose template，它是缓存的
                }
            }

            return colorCounts;
        }

        /// <summary>
        /// 验证匹配位置的颜色是否与模板颜色一致
        /// </summary>
        private static bool VerifyColorMatch(Mat sourceBgr, Mat templateBgr, Point position)
        {
            // 将Mat转换为Bitmap以便使用GetPixel
            Bitmap templateBitmap = MatToBitmap(templateBgr);
            Bitmap sourceBitmap = MatToBitmap(sourceBgr);
            
            try
            {
                int templateWidth = templateBitmap.Width;
                int templateHeight = templateBitmap.Height;
                
                // 计算模板的主要颜色（排除白色）
                int totalR = 0, totalG = 0, totalB = 0;
                int colorPixelCount = 0;
                
                for (int y = 0; y < templateHeight; y++)
                {
                    for (int x = 0; x < templateWidth; x++)
                    {
                        Color pixel = templateBitmap.GetPixel(x, y);
                        
                        // 排除白色像素（R,G,B都大于200）
                        if (pixel.R < 200 || pixel.G < 200 || pixel.B < 200)
                        {
                            totalR += pixel.R;
                            totalG += pixel.G;
                            totalB += pixel.B;
                            colorPixelCount++;
                        }
                    }
                }
                
                if (colorPixelCount == 0)
                    return true;
                    
                int templateAvgR = totalR / colorPixelCount;
                int templateAvgG = totalG / colorPixelCount;
                int templateAvgB = totalB / colorPixelCount;
                
                // 计算源图像对应区域的平均颜色
                totalR = 0;
                totalG = 0;
                totalB = 0;
                int sourceColorPixelCount = 0;
                
                for (int y = 0; y < templateHeight; y++)
                {
                    for (int x = 0; x < templateWidth; x++)
                    {
                        int srcX = position.X + x;
                        int srcY = position.Y + y;
                        
                        if (srcX < 0 || srcY < 0 || srcX >= sourceBitmap.Width || srcY >= sourceBitmap.Height)
                            continue;
                        
                        Color pixel = sourceBitmap.GetPixel(srcX, srcY);
                        
                        // 排除白色像素
                        if (pixel.R < 200 || pixel.G < 200 || pixel.B < 200)
                        {
                            totalR += pixel.R;
                            totalG += pixel.G;
                            totalB += pixel.B;
                            sourceColorPixelCount++;
                        }
                    }
                }
                
                if (sourceColorPixelCount == 0)
                    return true;
                    
                int sourceAvgR = totalR / sourceColorPixelCount;
                int sourceAvgG = totalG / sourceColorPixelCount;
                int sourceAvgB = totalB / sourceColorPixelCount;
                
                // 比较颜色差异（使用较大的容差）
                int tolerance = 80;
                bool match = Math.Abs(sourceAvgR - templateAvgR) < tolerance &&
                             Math.Abs(sourceAvgG - templateAvgG) < tolerance &&
                             Math.Abs(sourceAvgB - templateAvgB) < tolerance;
                
                return match;
            }
            finally
            {
                templateBitmap.Dispose();
                sourceBitmap.Dispose();
            }
        }

        /// <summary>
        /// 将BGR Mat转换为Bitmap（RGB格式）
        /// </summary>
        private static Bitmap MatToBitmap(Mat bgrMat)
        {
            // 先将BGR转换为RGB格式，以便Bitmap正确解释
            Mat rgbMat = new Mat();
            CvInvoke.CvtColor(bgrMat, rgbMat, ColorConversion.Bgr2Rgb);
            
            var result = new Bitmap(rgbMat.Cols, rgbMat.Rows, PixelFormat.Format24bppRgb);
            BitmapData bmpData = result.LockBits(
                new Rectangle(0, 0, result.Width, result.Height),
                ImageLockMode.WriteOnly,
                result.PixelFormat);

            try
            {
                int stride = bmpData.Stride;
                int size = stride * result.Height;
                byte[] data = new byte[size];

                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    using (Mat tempMat = new Mat(result.Height, result.Width, DepthType.Cv8U, 3, handle.AddrOfPinnedObject(), stride))
                    {
                        rgbMat.CopyTo(tempMat);
                    }
                    Marshal.Copy(data, 0, bmpData.Scan0, size);
                }
                finally
                {
                    handle.Free();
                }
            }
            finally
            {
                result.UnlockBits(bmpData);
                rgbMat.Dispose();
            }

            return result;
        }

        public static Bitmap? CaptureRegionBelowIcon(Bitmap sourceImage, Point iconPosition, int iconWidth)
        {
            if (sourceImage == null)
                return null;

            int startX = iconPosition.X;
            int startY = iconPosition.Y + 1;
            int width = iconWidth;
            int height = sourceImage.Height - startY;

            if (startX < 0 || startY < 0 || width <= 0 || height <= 0 ||
                startX + width > sourceImage.Width || startY + height > sourceImage.Height)
            {
                return null;
            }

            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(sourceImage, new Rectangle(0, 0, width, height),
                           new Rectangle(startX, startY, width, height),
                           GraphicsUnit.Pixel);
            }

            return result;
        }


        public class ProcessResult
        {
            public bool FoundLocal { get; set; }
            public bool FoundIcon { get; set; }
            public Bitmap? ResultImage { get; set; }
        }

        public enum AlertType
        {
            None,
            RedStar,
            OrangeCross,
            DarkRedSquare,
            DarkOrangePlus,
            GraySquare
        }

        private static readonly Dictionary<AlertType, Color[]> AlertColors = new Dictionary<AlertType, Color[]>
        {
            { AlertType.RedStar, new[] { Color.FromArgb(255, 180, 0, 0), Color.FromArgb(255, 200, 20, 20), Color.FromArgb(255, 220, 40, 40) } },
            { AlertType.OrangeCross, new[] { Color.FromArgb(255, 255, 120, 0), Color.FromArgb(255, 255, 140, 20), Color.FromArgb(255, 255, 160, 40) } },
            { AlertType.DarkRedSquare, new[] { Color.FromArgb(255, 100, 0, 0), Color.FromArgb(255, 120, 10, 10), Color.FromArgb(255, 140, 20, 20) } },
            { AlertType.DarkOrangePlus, new[] { Color.FromArgb(255, 200, 80, 0), Color.FromArgb(255, 220, 100, 20), Color.FromArgb(255, 230, 120, 40) } },
            { AlertType.GraySquare, new[] { Color.FromArgb(255, 80, 80, 80), Color.FromArgb(255, 100, 100, 100), Color.FromArgb(255, 120, 120, 120) } }
        };

        public static List<AlertType> DetectAlertColors(Bitmap image)
        {
            List<AlertType> detectedColors = new List<AlertType>();

            if (image == null)
                return detectedColors;

            byte[] pixels = GetPixelData(image);
            int width = image.Width;
            int height = image.Height;
            int bytesPerPixel = 4;

            Dictionary<AlertType, int> colorCounts = new Dictionary<AlertType, int>();
            foreach (AlertType type in Enum.GetValues(typeof(AlertType)))
            {
                if (type != AlertType.None)
                {
                    colorCounts[type] = 0;
                }
            }

            int sampleStep = Math.Max(1, Math.Min(width, height) / 20);
            int totalSamples = 0;

            for (int y = 0; y < height; y += sampleStep)
            {
                for (int x = 0; x < width; x += sampleStep)
                {
                    int index = (y * width + x) * bytesPerPixel;
                    if (index + 2 < pixels.Length)
                    {
                        byte r = pixels[index + 2];
                        byte g = pixels[index + 1];
                        byte b = pixels[index];

                        foreach (var kvp in AlertColors)
                        {
                            if (MatchesColorGroup(r, g, b, kvp.Value))
                            {
                                colorCounts[kvp.Key]++;
                                break;
                            }
                        }
                        totalSamples++;
                    }
                }
            }

            int threshold = Math.Max(3, totalSamples / 20);

            foreach (var kvp in colorCounts)
            {
                if (kvp.Value >= threshold)
                {
                    detectedColors.Add(kvp.Key);
                }
            }

            return detectedColors;
        }

        private static bool MatchesColorGroup(byte r, byte g, byte b, Color[] targetColors)
        {
            int tolerance = 40;
            foreach (Color target in targetColors)
            {
                if (Math.Abs(r - target.R) < tolerance &&
                    Math.Abs(g - target.G) < tolerance &&
                    Math.Abs(b - target.B) < tolerance)
                {
                    return true;
                }
            }
            return false;
        }

        private static byte[] GetPixelData(Bitmap bitmap)
        {
            lock (lockObj)
            {
                BitmapData bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    bitmap.PixelFormat);

                int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int byteCount = bmpData.Stride * bitmap.Height;
                byte[] pixels = new byte[byteCount];

                Marshal.Copy(bmpData.Scan0, pixels, 0, byteCount);
                bitmap.UnlockBits(bmpData);

                return pixels;
            }
        }

        public static string GetAlertMessage(AlertType alertType)
        {
            switch (alertType)
            {
                case AlertType.RedStar:
                    return "【警报】宣战状态！";
                case AlertType.OrangeCross:
                    return "【警告】战争状态！";
                case AlertType.DarkRedSquare:
                    return "【警报】糟糕声望！";
                case AlertType.DarkOrangePlus:
                    return "【警告】不良声望！";
                case AlertType.GraySquare:
                    return "【信息】中立声望";
                default:
                    return string.Empty;
            }
        }
    }
}
