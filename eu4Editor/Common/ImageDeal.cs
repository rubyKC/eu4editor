using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;

namespace GeoPPT.Core
{
    public static class ImageDeal
    {
        public static unsafe Bitmap MergeImage(this Bitmap bmp, Bitmap bmp1) 
        {
            int height = bmp.Height;
            int width = bmp.Width;

            int height1 = bmp1.Height;
            int width1 = bmp1.Width;

            var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);;
            var data1 = bmp1.LockBits(new Rectangle(0, 0, width1, height1), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            

            var ptr = (PixelColor*)data.Scan0.ToPointer();
            var ptr1 = (PixelColor*)data1.Scan0.ToPointer();
            for (int y = 0; y < height && y<height1; y++)
            {
                for (int x = 0; x < width && x<width1; x++)
                {
                    var v1 = *ptr;
                    var v2 = ptr1[y*width1 + x];

                    var alpha1 = v2.Alpha/255.0;
                    
                    if (v2.Green != 0 || v2.Blue != 0 || v2.Red != 0)
                    {
                        (*ptr).Green = (byte) (v2.Green*alpha1*0.5 + v1.Green*0.5);
                        (*ptr).Blue = (byte) (v2.Blue*alpha1*0.5 + v1.Blue*0.5);
                        (*ptr).Red = (byte) (v2.Red*alpha1*0.5 + v1.Red*0.5);
                    }

                    ptr++;
                    //ptr1++;
                }
            }

            bmp.UnlockBits(data);
            bmp1.UnlockBits(data1);

            return bmp;
        }


        public static unsafe Image Do(this Bitmap bmp,Action<IntPtr,int,int> action)
        {
 
            int height = bmp.Height;
            int width = bmp.Width;

            var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            if (action != null)
                action(data.Scan0, width, height);
            //var ptr = (PixelColor*)data.Scan0.ToPointer();

            //DoGray(height, width, ptr);
       
            //DoSimpleMatch(height,width,ptr);
            bmp.UnlockBits(data);

            return bmp;
        }

        private static unsafe void Gray(this Bitmap bmp)
        {
            bmp.Do((p, width, height) =>
            {
                var ptr = (PixelColor*)p.ToPointer();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var c = *ptr;
                        var gray = ((c.Red * 38 + c.Green * 75 + c.Blue * 15) >> 7);
                        (*ptr).Green = (*ptr).Red = (*ptr).Blue = (byte)gray;

                        ptr++;
                    }
                }
            });

        }


        private static unsafe Bitmap Invert(this Bitmap bmp)
        {
            ////////////////////////////
            bmp.Do((p, width, height) =>
            {
                var ptr = (Int32*)p.ToPointer();
                ////////////////////////////////////

                var white = Color.White.ToArgb();
                var black = Color.Black.ToArgb();
 
                var area = width * height;
                for (int i = 0; i < area; i++)
                {
                    var val = *ptr;

                    if (val == white)
                        *ptr = black;
                    else
                        *ptr = white;

                    ptr++;
                }

            });

            return bmp;
        }

        private static unsafe Bitmap Shink(this Bitmap bmp,Color color)
        {
            var rect=Rectangle.Empty;

            var minX = bmp.Width;
            var minY = bmp.Height;
            int maxX = 0;
            int maxY = 0;
            bmp.Do((p, width, height) =>
            {
                var ptr = (Int32*)p.ToPointer();
                var backGround = color.ToArgb();

                //var xArray = new bool[width];
                //var yArray = new bool[height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var c = *ptr;
                        if(c!=backGround)
                        {
                            if (x < minX)
                                minX = x;
                            if (x > maxX)
                                maxX = x;
                            if (y < minY)
                                minY = y;
                            if (y > maxY)
                                maxY = y;
                        }

                        ptr++;
                    }
                }

                if ((minX == width) && (minY == height) && (maxX == 0) && (maxY == 0))
                {
                    minX = minY = 0;
                }

                var size= new Size(maxX - minX+1 , maxY - minY+1 );
                rect = new Rectangle(minX, minY,size.Width, size.Height);
                
            });

            //目的是转换图片格式
            var newbmp = (Bitmap)bmp.Clone(rect, PixelFormat.Format32bppArgb);
            //var newbmp=new Bitmap(rect.Width,rect.Height,PixelFormat.Format24bppRgb);
            //var grfx = Graphics.FromImage(newbmp);
            //grfx.Clear(Color.White);
            //grfx.DrawImage(bmp,0,0,rect,GraphicsUnit.Pixel);
            return newbmp;
        }

        //public static  unsafe double[] ToTrainData(this Bitmap bmp)
        //{
        //    //TODO:改成最小边框
        //    var array = new double[bmp.Width * bmp.Height];

        //    bmp.Do((p, width, height) =>
        //    {
        //        var ptr = (Int32*)p.ToPointer();
        //        ////////////////////////////////////

        //        var black = Color.Black.ToArgb();

        //        for (int y = 0; y < height; y++)
        //        {
        //            for (int x = 0; x < width; x++)
        //            {
        //                var c = *ptr;

        //                if (c == black)
        //                    array[y*width + x] = 1;
  
        //                ptr++;
        //            }

        //        }
        //    });

        //    return array;
        //}


        private static unsafe IEnumerable<PixelGroup> GroupByValue(this Bitmap bmp)
        {
            var dict = new Dictionary<int, PixelGroup>();

            bmp.Do((p, width, height) =>
            {
                var ptr = (Int32*)p.ToPointer();
                ////////////////////////////////////

                //var black = Color.Black.ToArgb();
                var area = width*height;

                for (int i = 0; i < area;i++ )
                {
                    var c = *ptr;
              
                    PixelGroup val;
                    if (dict.TryGetValue(c, out val))
                        val.Count++;
                    else
                        dict.Add(c, new PixelGroup { Count = 1, Key = c });

                    ptr++;

                }


            });

            return dict.Select(v=>v.Value);
        }
          
        //private static unsafe Dictionary<byte,int> DoSimpleMatch(int height, int width, PixelColor* ptr)
        //{
        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            var c = *ptr;
        //            var gray = ((c.Red * 38 + c.Green * 75 + c.Blue * 15) >> 7);
        //            (*ptr).Green = (*ptr).Red = (*ptr).Blue = (byte)gray;

                    

        //            ptr++;
        //        }
        //    }
        //}
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor
    {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;

    } 

    public class PixelGroup
    {
        public int Key;
        public int Count;

        public override string ToString()
        {
            return string.Format("{0} - {1}",Key,Count);
        }
    }
}
