using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PaddleOCRSharp;

namespace EVE预警
{
    /// <summary>
    /// OCR文字识别处理器 - 使用PaddleOCR
    /// </summary>
    public static class OcrProcessor
    {
        private static OCRParameter _ocrParam = new OCRParameter();
        private static OCRModelConfig? _modelConfig;
        private static bool _isInitialized = false;
        private static string? _initError;
        private static readonly object _lockObj = new object();

        /// <summary>
        /// 初始化OCR引擎
        /// </summary>
        public static void Initialize()
        {
            lock (_lockObj)
            {
                try
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    string inferenceDir = Path.Combine(appDir, "inference");

                    // 检查模型目录是否存在
                    if (!Directory.Exists(inferenceDir))
                    {
                        Directory.CreateDirectory(inferenceDir);
                    }

                    string detPath = Path.Combine(inferenceDir, "ch_PP-OCRv4_det_infer");
                    string clsPath = Path.Combine(inferenceDir, "ch_ppocr_mobile_v2.0_cls_infer");
                    string recPath = Path.Combine(inferenceDir, "ch_PP-OCRv4_rec_infer");
                    string keysPath = Path.Combine(inferenceDir, "ppocr_keys.txt");

                    // 检查关键模型文件
                    if (!Directory.Exists(detPath) || !Directory.Exists(recPath) || !File.Exists(keysPath))
                    {
                        _initError = $"模型文件不完整，缺少检测或识别模型";
                        _isInitialized = false;
                        return;
                    }

                    _modelConfig = new OCRModelConfig
                    {
                        det_infer = detPath,
                        cls_infer = clsPath,
                        rec_infer = recPath,
                        keys = keysPath
                    };

                    _ocrParam = new OCRParameter
                    {
                        use_angle_cls = false,
                        max_side_len = 1024,
                        det_db_box_thresh = 0.5f,
                        det_db_thresh = 0.3f,
                        det_db_unclip_ratio = 1.6f
                    };

                    _isInitialized = true;
                    _initError = null;
                }
                catch (Exception ex)
                {
                    _initError = ex.Message;
                    _isInitialized = false;
                }
            }
        }

        /// <summary>
        /// 检查OCR引擎是否已正确初始化
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// 获取初始化错误信息
        /// </summary>
        public static string? InitError => _initError;

        /// <summary>
        /// 从Bitmap裁剪指定区域并进行OCR识别
        /// </summary>
        public static string? RecognizeText(Bitmap sourceImage, int left, int top, int right, int bottom)
        {
            if (!_isInitialized || _modelConfig == null)
            {
                return $"OCR引擎未初始化: {_initError}";
            }

            // 验证坐标
            if (left < 0 || top < 0 || right <= left || bottom <= top ||
                right > sourceImage.Width || bottom > sourceImage.Height)
            {
                return null;
            }

            // 裁剪图像
            int width = right - left;
            int height = bottom - top;

            string tempInputFile = Path.Combine(Path.GetTempPath(), $"EVE_OCR_IN_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
            string tempOutputFile = Path.Combine(Path.GetTempPath(), $"EVE_OCR_OUT_{DateTime.Now:yyyyMMdd_HHmmss_fff}.txt");

            try
            {
                // 裁剪并保存到临时文件
                using (Bitmap croppedImage = new Bitmap(width, height))
                {
                    using (Graphics g = Graphics.FromImage(croppedImage))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.DrawImage(sourceImage,
                            new Rectangle(0, 0, width, height),
                            new Rectangle(left, top, width, height),
                            GraphicsUnit.Pixel);
                    }

                    croppedImage.Save(tempInputFile, ImageFormat.Png);
                }

                // 使用PaddleOCR识别
                lock (_lockObj)
                {
                    using (var engine = new PaddleOCREngine(_modelConfig, _ocrParam))
                    {
                        OCRResult result = engine.DetectText(tempInputFile);
                        return result?.Text?.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                return $"OCR识别失败: {ex.Message}";
            }
            finally
            {
                // 清理临时文件
                try
                {
                    if (File.Exists(tempInputFile)) File.Delete(tempInputFile);
                    if (File.Exists(tempOutputFile)) File.Delete(tempOutputFile);
                }
                catch { }
            }
        }
    }
}
