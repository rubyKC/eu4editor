using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Eu4Editor;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace GeoPPT.Core
{
    public static class ImageHelper
    {

        public static System.Windows.Media.Color ToMeidaColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static System.Drawing.Color ToColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static BitmapImage ToBitmapImage(this BitmapSource source)
        {
            BitmapEncoder encoder = new BmpBitmapEncoder();
            var memoryStream = new MemoryStream();
            var bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(memoryStream);

            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }

        public static Bitmap BitmapImage2Bitmap(this BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative)); 

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                var bitmap = new Bitmap(outStream);

                // return bitmap; <-- leads to problems, stream is closed/closing ... 
                return new Bitmap(bitmap);
            }
        }


        //public static BitmapSource ToBitmapSource(this Bitmap _bitmap)
        //{
        //    if (_bitmap == null)
        //        return null;

        //    Rectangle destRect = new Rectangle(0, 0, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);

        //    var bitmap = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //    using (Graphics g = Graphics.FromImage(bitmap))
        //    {
        //        g.DrawImage(_bitmap, destRect, destRect, GraphicsUnit.Pixel);
        //    }
        //    return Tobitmap(_bitmap);
        //}

        //public static BitmapSource ToBitmapSource(this Bitmap bmp)
        //{
        //    if (null == bmp)
        //        return null;

        //    try
        //    {
        //        return Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
        //// at class level 


        //// at class level 
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource ToBitmapSource(this Bitmap bmp)
        {
            // your code 
            BitmapSource returnSource;  

            IntPtr hbmp = IntPtr.Zero;
            try
            {
                hbmp = bmp.GetHbitmap();

                returnSource = Imaging.CreateBitmapSourceFromHBitmap(hbmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                returnSource = null;
            }
            finally
            {
                if (hbmp != IntPtr.Zero)
                    DeleteObject(hbmp);
            }

            return returnSource;
        }



        public static RectangleF ToRectangleF(this Rect rect)
        {
            return new RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
        }

        public static Bitmap ScaleImage(this Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }

        public static Bitmap SubImage(this Image image, int x, int y, int width, int height)
        {
            var newImg = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            var grfx = Graphics.FromImage(newImg);
            grfx.DrawImage(image, new Rectangle(x, y, width, height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            grfx.Dispose();

            return newImg;
        }

        public static Bitmap SubImage(this Bitmap img, Rectangle cropArea)
        {
            var x = cropArea.X < 0 ? 0 : cropArea.X;
            var y = cropArea.Y < 0 ? 0 : cropArea.Y;
            var width = cropArea.Width + x > img.Width ? cropArea.Width-- : cropArea.Width;
            var height = cropArea.Height + y > img.Height ? cropArea.Height-- : cropArea.Height;

            var rect = new Rectangle(x, y, width, height);

            Bitmap bmpCrop = img.Clone(cropArea, PixelFormat.Format24bppRgb);
            return bmpCrop;
        }

        //public static Bitmap SubImage(this Image image,Rectangle rect)
        //{
        //    return SubImage(image, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        //}

        public static Bitmap Scale(this Image image, int ratio)
        {
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            var grfx = Graphics.FromImage(newImage);
            grfx.InterpolationMode = InterpolationMode.NearestNeighbor; ;
            grfx.DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }


        public static Bitmap GetFullScreenImage()
        {
            //var rectangle = SystemInformation.VirtualScreen;
            //var workingArea = SystemInformation.WorkingArea;
            //var bitmap = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
            //using (var g = Graphics.FromImage(bitmap))
            //{
            //    g.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0, rectangle.Size, CopyPixelOperation.SourceCopy);
            //}
            //return bitmap;

            return null;
        }

        private static BitmapSource ClipFullFromScreen(Rect rect, Bitmap fullScreen)
        {
            if (null == fullScreen)
                return null;

            var sourceRect = new System.Drawing.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            var destRect = new System.Drawing.Rectangle(0, 0, (int)rect.Width, (int)rect.Height);
            var bitmap = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(fullScreen, destRect, sourceRect, GraphicsUnit.Pixel);
            }
            return bitmap.ToBitmapSource();

        }


        public static void ToBmpFile(this BitmapSource source, string filePath)
        {
            if (null == source || string.IsNullOrEmpty(filePath))
                return;

            var bmpBitmapEncoder = new BmpBitmapEncoder();
            bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(source));

            try
            {
                using (var stream = File.Create(filePath))
                {
                    bmpBitmapEncoder.Save(stream);
                }

            }
            catch (Exception ex)
            {
                //Log.GetLogger.Info(ex.Message);
                Application.Current.GetLocator().Main.log.Error(ex.Message);
            }
        }

        public static void ToPngFile(this BitmapSource source, string filePath)
        {
            if (null == source || string.IsNullOrEmpty(filePath))
                return;

            var bmpBitmapEncoder = new PngBitmapEncoder();
            bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(source));


            try
            {
                using (var stream = File.Create(filePath))
                {
                    bmpBitmapEncoder.Save(stream);
                }
            }
            catch (Exception ex)
            {
                //Log.GetLogger.Info(ex.Message);
                Application.Current.GetLocator().Main.log.Error(ex.Message);
            }
        }




        public static void ToJpgFile(this BitmapSource source, string filePath)
        {
            if (null == source || string.IsNullOrEmpty(filePath))
                return;

            var bmpBitmapEncoder = new JpegBitmapEncoder();
            bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(source));

            try
            {
                using (var stream = File.Create(filePath))
                {
                    bmpBitmapEncoder.Save(stream);
                }
            }
            catch (Exception ex)
            {
                //Log.GetLogger.Info(ex.Message);
                Application.Current.GetLocator().Main.log.Error(ex.Message);
            }

        }

        //public static BitmapSource FromJpgFile(string filePath)
        //{
        //    if (string.IsNullOrEmpty(filePath))
        //        return null;

        //    var bmpBitmapEncoder = new JpegBitmapDecoder(new Uri(filePath), BitmapCreateOptions.None, BitmapCacheOption.Default);
        //    BitmapImage uriImage = new BitmapImage();
        //    uriImage.Width = 200;
        //    // Set image source.

        //    uriImage.Source = uriBitmap.Frames[0];


        //}


        public static RenderTargetBitmap RenderWpf(this Visual visual, int width, int height)
        {
            RenderTargetBitmap bmp=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);
            bmp.Render(visual);
            return bmp;
        }

        public static void ClipScreenToBmpFile(Rect rect, string filePath)
        {
            var bitmapSource = ImageHelper.ClipFullFromScreen(rect, GetFullScreenImage());
            bitmapSource.ToBmpFile(filePath);
        }

        public static void ClipScreen(Rectangle rectangle, string filePath)
        {
            var bitmap = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0, rectangle.Size, CopyPixelOperation.SourceCopy);
            }
            try
            {
                bitmap.Save(filePath);
            }
            catch (Exception ex)
            {
                //Log.GetLogger.Info("ImageHelper类ClipScreen方法异常,异常信息为" + ex.ToString());
                Application.Current.GetLocator().Main.log.Error(ex.Message);
            }
        }

        public static void WPFElementToBitmap(Window frameworkElement, string filePath)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)frameworkElement.ActualWidth, (int)frameworkElement.Height, 96d, 96d, PixelFormats.Default);
            renderTargetBitmap.Render(frameworkElement);
            BmpBitmapEncoder bmpBitmapEncoder = new BmpBitmapEncoder();
            bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            try
            {
                using (var stream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    bmpBitmapEncoder.Save(stream);
                }
            }
            catch (Exception ex)
            {
                Application.Current.GetLocator().Main.log.Error(ex.Message);
            }
            //DrawingVisual drawingVisual = new DrawingVisual();
            //DrawingContext drawingContext = drawingVisual.RenderOpen();
            //drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, System.Windows.Media.Pen, new Rect(0, 0, renderTargetBitmap.Width, renderTargetBitmap.Height));
            //drawingContext.Close();
            //renderTargetBitmap.Render(drawingVisual);
            //renderTargetBitmap.Render(frameworkElement);

        }
    }


}
