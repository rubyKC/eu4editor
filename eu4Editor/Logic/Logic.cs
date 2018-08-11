using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BezierCurveSample.View.Utils;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Eu4Editor.ViewModel;
using GalaSoft.MvvmLight.Ioc;
using GeoPPT.Core;
using GeoPPT.Draw;
using Newtonsoft.Json;
using ReadEu4Config;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Path = System.Windows.Shapes.Path;
using Pen = System.Windows.Media.Pen;
using Point = System.Drawing.Point;

namespace Eu4Editor
{
    public class Logic
    {

        public static unsafe ColorRangeDict GetProvinces(string file,bool isdevert=false)
        {
            ColorRangeDict dict = new ColorRangeDict();

            var bmp = (Bitmap)System.Drawing.Bitmap.FromFile(file);
            //bmp = bmp.Scale(2);

            //var bmp1 = bmp.Scale(2);
            //var format = bmp.PixelFormat;

            var h = (ushort)bmp.Height;

            bmp.Do((p, width, height) =>
            {
                var ptr = (Int32*)p.ToPointer();
                ////////////////////////////////////

                for (UInt16 y = 0; y < height; y++)
                {
                    for (UInt16 x = 0; x < width; x++)
                    {
                        var val = *ptr;

                        Prov prov;

                        if (dict.Dict.TryGetValue(val, out prov))
                        {

                            if (!isdevert)
                                prov.Points.Add(new Point2(x, y));
                            else 
                                prov.Points.Add(new Point2(x,(ushort)(h-y)));
                        }
                        else
                        {
                            prov = new Prov();
                            prov.Color = val;
                            if (!isdevert)
                                prov.Points.Add(new Point2(x, y));
                            else
                                prov.Points.Add(new Point2(x, (ushort)(h - y)));
                            dict.Dict.Add(val, prov);
                        }

                        ptr++;
                    }
                }
            });

            foreach (var value in dict.Dict.Values)
            {
                value.CalRect();
            }


            return dict;
        }

        //public static void WriteAllProvImages(MainViewModel vm)
        //{
        //    var dir = "Provs//";

        //    if (!Directory.Exists(dir))
        //        Directory.CreateDirectory(dir);

        //    int i = 0;
        //    foreach (var prov in vm.Provs)
        //    {
        //        if (i > 1000)
        //            break;
        //        var img = prov.DrawImage();
        //        var file = dir + i + ".png";
        //        img.Save(file, ImageFormat.Png);
        //        i++;
        //    }
        //}


        public static float count1 = 0;
        public static float count2 = 1;
        public static Contours GetContours(System.Drawing.Bitmap bmp)
        {
            //var newbmp = bmp.Scale(2);

            //StringBuilder msgBuilder = new StringBuilder("Performance: ");

            //var imgfile = AppDomain.CurrentDomain.BaseDirectory + imageUrl;


            //Load the image from file and resize it for display
            Image<Gray, Byte> img = new Image<Gray, byte>(bmp);
            //.Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);

            //Convert the image to grayscale and filter out the noise
            //UMat uimage = new UMat();
            //CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

            //var imageGray = img.Convert<Gray, Byte>();
            //var imagethreshold = img.ThresholdBinary(new Gray(1), new Gray(255));
            CvInvoke.Threshold(img, img, 1, 255, ThresholdType.Binary);
            //imagethreshold.FillConvexPoly();
            //use image pyr to remove noise
            //UMat pyrDown = new UMat();
            //CvInvoke.PyrDown(uimage, pyrDown);
            //CvInvoke.PyrUp(pyrDown, uimage);

            //Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();

            //#region circle detection
            //Stopwatch watch = Stopwatch.StartNew();
            //double cannyThreshold = 100;//180.0;
            //double cannyThresholdLinking = 100;//120.0;
            //UMat cannyEdges = new UMat();
            //CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            //int[,] tempStructure = { { 1, 1, 1 } };
            //Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new System.Drawing.Size(2, 2), new System.Drawing.Point(-1, -1));


            //CvInvoke.Dilate(img, img, structuringElement, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));

            //CvInvoke.Dilate(img, img, structuringElement, new System.Drawing.Point(-1, -1), 1, BorderType.Default,
            //    new MCvScalar(0, 0, 0));
            //CvInvoke.Erode(img, img, structuringElement, new System.Drawing.Point(-1, -1), 1, BorderType.Default,
            //    new MCvScalar(0, 0, 0));


            /////////////////////////////////////
            using (Mat hierachy = new Mat())
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {

                CvInvoke.FindContours(img, contours, hierachy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
                //CvInvoke.DrawContours();

                //////////////////////////////////////
                //多边形逼近
                //List<VectorOfPoint> results = new List<VectorOfPoint>();
                //for (int i = 0; i < contours.Size; i++)
                //{
                //    var contour = contours[i];


                //    VectorOfPoint approxContour = new VectorOfPoint();
                //    CvInvoke.ApproxPolyDP(contour, approxContour, 0.5, true);
                //    results.Add(approxContour);

                //    count1 += contour.Size;
                //    count2 += approxContour.Size;

                //    CvInvoke.Dilate(contour, contour,);
                //}





                var list = contours.ToArrayOfArray().Select(v => new Contour { Pts = v }).OrderByDescending(v => v.Pts.Length).ToList();
                //var list = results.Select(v => new Contour { Pts = v.ToArray() }).OrderByDescending(v => v.Pts.Length).ToList();
                Contours Cons = new Contours();
                Cons.Items = list;
                Cons.Width = img.Width;
                Cons.Height = img.Height;

                return Cons;
            }
        }


        public static unsafe Bitmap DrawContours(Contours cons)
        {
            Bitmap bmp = new Bitmap(cons.Width, cons.Height);

            bmp.Do((p, w, h) =>
            {
                var color = System.Drawing.Color.Blue.ToArgb();
                //var p = ptr;     
                var ptr = (Int32*)p.ToPointer();

                foreach (var contour in cons.Items)
                {
                    foreach (var pt in contour.Pts)
                    {
                        var p1 = ptr + pt.X + pt.Y * w;
                        *p1 = color;
                    }
                }


            });

            return bmp;
        }


        public static System.Windows.Shapes.Path DrawPath(Contours cons,Prov prov)
        {
            var path = new System.Windows.Shapes.Path();
            path.Stroke = System.Windows.Media.Brushes.Red;
            path.StrokeThickness = 1;
            path.Fill = System.Windows.Media.Brushes.Green;


            var group = new PathGeometry();
            path.Data = group;

            int left = 0;
            int top = 0;

            if (prov != null)
            {
                left = prov.Rectangle.Left;
                top = prov.Rectangle.Top;
            }


            int i = 0;
            foreach (var contour in cons.Items)
            {
                if(contour.Pts.Length<=1)
                    continue;
                //////////////////////////////
                //if (contour.Pts.Length <= 2)
                //    return;
                //var subPath = new PathGeometry();
                //subPath.Figures.Add(new Polygon());
                var polygon = new PathFigure();
                polygon.StartPoint = contour.Pts.First().ToPoint(left,top);
                polygon.IsClosed = true;
                //polygon.IsFilled = true;
                var seg = new PolyLineSegment(contour.Pts.Skip(1).Select(v => v.ToPoint(left,top)).ToList(), true);
                seg.IsSmoothJoin = true;
                //seg.IsStroked = false;
                polygon.Segments.Add(seg);

                //if (i == 0 || i == 2 || i == 4)
                //if(contour.Pts.Length>2 )
                group.Figures.Add(polygon);



                i++;

            }

            return path;

        }
        public static Geometry GetGeometry1(Contours cons, Prov prov)
        {
            //var path = new System.Windows.Shapes.Path();
            //path.Stroke = System.Windows.Media.Brushes.Red;
            //path.StrokeThickness = 1;
            //path.Fill = System.Windows.Media.Brushes.Green;


            ////////////////////////////////////////////////////////



            /////////////////////////////////////
            // Create a StreamGeometry to use to specify myPath.
            StreamGeometry geometry = new StreamGeometry();
            geometry.FillRule = FillRule.EvenOdd;

            // Open a StreamGeometryContext that can be used to describe this StreamGeometry 
            // object's contents.
            using (StreamGeometryContext ctx = geometry.Open())
            {

                int left = 0;
                int top = 0;

                if (prov != null)
                {
                    left = prov.Rectangle.Left;
                    top = prov.Rectangle.Top;
                }

                foreach (var contour in cons.Items)
                {
                    if (contour.Pts.Length <= 1)
                        continue;

                    if (contour.Pts.Length >= 3)
                    {
                        var firstPt = contour.Pts.First().ToPoint(left, top);
                        //////////////////////////////
                        // Begin the triangle at the point specified. Notice that the shape is set to 
                        // be closed so only two lines need to be specified below to make the triangle.
                        ctx.BeginFigure(firstPt, true /* is filled */, true /* is closed */);

                        var pts1 = contour.Pts.Select(v => v.ToPoint(left, top)).ToList();
                        /////////////////////////////////////////
                        var beizerSegments = InterpolationUtils.InterpolatePointWithBeizerCurves(pts1, true);

                        ctx.BeginFigure(firstPt, true, false);
                        List<System.Windows.Point> list = new List<System.Windows.Point>();
                        foreach (var seg in beizerSegments)
                        {
                            //var segment = new BezierSegment
                            //{
                            //    Point1 = beizerCurveSegment.FirstControlPoint,
                            //    Point2 = beizerCurveSegment.SecondControlPoint,
                            //    Point3 = beizerCurveSegment.EndPoint
                            //};

                            list.Add(seg.FirstControlPoint);
                            list.Add(seg.SecondControlPoint);
                            list.Add(seg.EndPoint);

                        }
                        //ctx.PolyQuadraticBezierTo(list, true, false);
                        ctx.PolyBezierTo(list, true, false);

                    }
                    else
                    {
                        //var firstPt = contour.Pts.First().ToPoint(left, top);
                        ////////////////////////////////
                        //// Begin the triangle at the point specified. Notice that the shape is set to 
                        //// be closed so only two lines need to be specified below to make the triangle.
                        //ctx.BeginFigure(firstPt, true /* is filled */, true /* is closed */);

                        //// Draw a line to the next specified point.
                        ////ctx.LineTo(new Point(100, 100), true /* is stroked */, false /* is smooth join */);

                        //ctx.PolyLineTo(contour.Pts.Skip(1).Select(v => v.ToPoint(left, top)).ToList(), true, false);
                    }

                }
            }

            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            geometry.Freeze();

            //// Specify the shape (triangle) of the Path using the StreamGeometry.
            //path.Data = geometry;



            return geometry;

        }
        public static Geometry GetGeometry(Contours cons, Prov prov)
        {
            //var path = new System.Windows.Shapes.Path();
            //path.Stroke = System.Windows.Media.Brushes.Red;
            //path.StrokeThickness = 1;
            //path.Fill = System.Windows.Media.Brushes.Green;


            ////////////////////////////////////////////////////////
     
 

            /////////////////////////////////////
            // Create a StreamGeometry to use to specify myPath.
            StreamGeometry geometry = new StreamGeometry();
            geometry.FillRule = FillRule.EvenOdd;

            // Open a StreamGeometryContext that can be used to describe this StreamGeometry 
            // object's contents.
            using (StreamGeometryContext ctx = geometry.Open())
            {

                int left = 0;
                int top = 0;

                if (prov != null)
                {
                    left = prov.Rectangle.Left;
                    top = prov.Rectangle.Top;
                }

                foreach (var contour in cons.Items)
                {
                    if (contour.Pts.Length <= 1)
                        continue;

                    var firstPt = contour.Pts.First().ToPoint(left, top,2);
                    //////////////////////////////
                    // Begin the triangle at the point specified. Notice that the shape is set to 
                    // be closed so only two lines need to be specified below to make the triangle.
                    ctx.BeginFigure(firstPt, true /* is filled */, true /* is closed */);

                    // Draw a line to the next specified point.
                    //ctx.LineTo(new Point(100, 100), true /* is stroked */, false /* is smooth join */);

                    ctx.PolyLineTo(contour.Pts.Skip(1).Select(v => v.ToPoint(left, top,2)).ToList(),true,false);
                }
            }

            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            geometry.Freeze();

            //// Specify the shape (triangle) of the Path using the StreamGeometry.
            //path.Data = geometry;


            
            return geometry;

        }



        public static Geometry GetGeometryByCurve(Contours cons, Prov prov)
        {
            //var path = new System.Windows.Shapes.Path();
            //path.Stroke = System.Windows.Media.Brushes.Red;
            //path.StrokeThickness = 1;
            //path.Fill = System.Windows.Media.Brushes.Green;


            ////////////////////////////////////////////////////////



            /////////////////////////////////////
            // Create a StreamGeometry to use to specify myPath.
            StreamGeometry geometry = new StreamGeometry();
            geometry.FillRule = FillRule.EvenOdd;

            // Open a StreamGeometryContext that can be used to describe this StreamGeometry 
            // object's contents.
            using (StreamGeometryContext ctx = geometry.Open())
            {

                int left = 0;
                int top = 0;

                if (prov != null)
                {
                    left = prov.Rectangle.Left;
                    top = prov.Rectangle.Top;
                }

                foreach (var contour in cons.Items)
                {
                    if (contour.Pts.Length <= 1)
                        continue;

              
                  
                    //var first = beizerSegments.First().StartPoint;
                    var firstPt = contour.Pts.First().ToPoint(left, top, 3);
                    //////////////////////////////
                    // Begin the triangle at the point specified. Notice that the shape is set to 
                    // be closed so only two lines need to be specified below to make the triangle.
                    ctx.BeginFigure(firstPt, true /* is filled */, true /* is closed */);

                    // Draw a line to the next specified point.
                    //ctx.LineTo(new Point(100, 100), true /* is stroked */, false /* is smooth join */);

                    //ctx.PolyLineTo(contour.Pts.Skip(1).Select(v => v.ToPoint(left, top, 2)).ToList(), true, false);

                    var pts1 = contour.Pts.Select(v => v.ToPoint(left,top,3)).ToList();
                    var beizerSegments = InterpolationUtils.InterpolatePointWithBeizerCurves(pts1, true);

                    if (beizerSegments != null)
                    {
                        var points =
                            beizerSegments.SelectMany(
                                v =>
                                    new List<System.Windows.Point>
                                    {
                                        v.FirstControlPoint,
                                        v.SecondControlPoint,
                                        v.EndPoint
                                    })
                                .ToList();

                        ctx.PolyBezierTo(points, true, false);

                    }
                }
            }

            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            geometry.Freeze();

            //// Specify the shape (triangle) of the Path using the StreamGeometry.
            //path.Data = geometry;



            return geometry;

        }

        public static System.Windows.Shapes.Path DrawPathByCurve(Contours cons)
        {
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            path.Stroke = System.Windows.Media.Brushes.Red;
            path.StrokeThickness = 2;
            path.Fill = System.Windows.Media.Brushes.Green;


            var group = new PathGeometry();
            path.Data = group;

            int i = 0;
            foreach (var contour in cons.Items)
            {
                //var subPath = new PathGeometry();
                //subPath.Figures.Add(new Polygon());
                var polygon = new PathFigure();
                polygon.StartPoint = contour.Pts.First().ToPoint();
                polygon.IsClosed = true;
                //polygon.IsFilled = true;
                //var seg = new PolyLineSegment(contour.Pts.Skip(1).Select(v => v.ToPoint()).ToList(), true);



                var pts1 = contour.Pts.Select(v => v.ToPoint()).ToList();
                /////////////////////////////////////////
                var beizerSegments = InterpolationUtils.InterpolatePointWithBeizerCurves(pts1, true);

                if (beizerSegments == null || beizerSegments.Count < 1)
                {
                    //Add a line segment <this is generic for more than one line>
                    //foreach (var point in points.GetRange(1, points.Count - 1))
                    //{

                    //    var myLineSegment = new LineSegment { Point = point };
                    //    myPathSegmentCollection.Add(myLineSegment);
                    //}
                }
                else
                {
                    foreach (var beizerCurveSegment in beizerSegments)
                    {
                        var segment = new BezierSegment
                        {
                            Point1 = beizerCurveSegment.FirstControlPoint,
                            Point2 = beizerCurveSegment.SecondControlPoint,
                            Point3 = beizerCurveSegment.EndPoint
                        };
                        polygon.Segments.Add(segment);
                    }
                }


                /////////////////////////////////////////
                //polygon.Segments.Add(seg);


                //if (i == 0 || i == 2 || i == 4)
                //    group.Figures.Add(polygon);

                i++;

            }

            return path;
        }

        public static System.Windows.Shapes.Path DrawPath(Contour contour)
        {
            System.Windows.Shapes.Path path = new Path();
            path.Stroke = Brushes.Red;
            path.StrokeThickness = 1;            //path.Fill = Brushes.Black;

            var group = new PathGeometry();
            path.Data = group;

            var polygon = new PathFigure();
            polygon.StartPoint = contour.Pts.First().ToPoint();
            polygon.IsClosed = true;
            //polygon.IsFilled = true;
            var seg = new PolyLineSegment(contour.Pts.Skip(1).Select(v => v.ToPoint()).ToList(), true);
            polygon.Segments.Add(seg);

            group.Figures.Add(polygon);

            return path;
        }

        public static Dictionary<int, int> LoadProvIdDict()
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            //var file = AppDomain.CurrentDomain.BaseDirectory + "files/definition.csv";
            var vm = SimpleIoc.Default.GetInstance<MainViewModel>();
            var file = vm.FileProvider.GetModOrOriFile(vm.FileProvider.GetDefinition());
            if (File.Exists(file))
            {


                var lines = File.ReadAllLines(file,Encoding.Default).Skip(1).ToList();


                //string templine = "";
                try
                {

                    foreach (var line in lines)
                    {
                        //templine = line;

                        var strs = line.Split(';');
                        var color = System.Drawing.Color.FromArgb(
                            int.Parse(strs[1].TrimEnd('.')),
                            int.Parse(strs[2].TrimEnd('.')),
                            int.Parse(strs[3].TrimEnd('.')));


                        if (!string.IsNullOrEmpty(strs[0]))
                        {
                            var id = int.Parse(strs[0]);

                            var key = color.ToArgb();

                            if (!dict.ContainsKey(key))
                                dict.Add(key, id);
                        }

                    }
                }
                catch (Exception ex)
                {

                    Application.Current.GetLocator().Main.log.Error(ex.Message);
                }

                return dict;
            }

            return null;
        }


        //public static Dictionary<string, System.Windows.Media.Color> LoadCountryColorDict()
        //{
        //    var dict = new Dictionary<string, System.Windows.Media.Color>();
        //    var file = AppDomain.CurrentDomain.BaseDirectory + "files/country_colors.txt";
        //    if (File.Exists(file))
        //    {
        //        var regStr = "^(\\w+)\\s+=\\s+\\{\\s+color1=\\s+\\{([^}]+)";

        //        var str = File.ReadAllText(file);

        //        var matches = Regex.Matches(str, regStr,RegexOptions.Multiline);

        //        try
        //        {
        //            foreach (Match match in matches)
        //            {
        //                var val = match.Groups[2].Value;
        //                var strs = val.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
        //                var color = System.Windows.Media.Color.FromRgb(
        //                    byte.Parse(strs[0]),
        //                    byte.Parse(strs[1]),
        //                    byte.Parse(strs[2]));

        //                var key = match.Groups[1].Value;

        //                if (!dict.ContainsKey(key))
        //                    dict.Add(key, color);
        //            }

        //        }
        //        catch (Exception ex)
        //        {


        //        }

        //        return dict;
        //    }

        //    return null;
        //}


        public static List<Country> LoadCountryColorDict1()
        {
            var vm = SimpleIoc.Default.GetInstance<MainViewModel>();
            List<Country> countries=new List<Country>();
            //var dir = @"H:\XUN\vic2 3.03\Victoria 2\mod\NWO\common\";
            //var dir =vm.FileProvider.GetModOrOriFile(vm.FileProvider.GetCommonPath());

            //var tagFile = AppDomain.CurrentDomain.BaseDirectory + "files/countries.txt";
            var tagFile = vm.FileProvider.GetModOrOriFile(vm.FileProvider.GetCountriesDefineFile());

            var files = vm.FileProvider.GetCountriesFiles();

            if (File.Exists(tagFile))
            {
                var regStr0= "^([\\w\\d]+)\\s*=\\s*\"([^\"]+)";

                var tagStr = File.ReadAllText(tagFile);

                var matches = Regex.Matches(tagStr, regStr0, RegexOptions.Multiline);

                //string lastColor = null;
                foreach (Match match in matches)
                {
                    try
                    {
                        var val = match.Groups[2].Value;
                        var key = match.Groups[1].Value;

                        ////////////////////////////////
                        var fileName = val;
                        val=val.Replace("/","\\");
                        var find = files.FirstOrDefault(v => v.ToLower().Contains(val.ToLower()));
                        if (find!=null)
                        {
                            var regStr = "^color = {([\\d\\s]+)";

                            var str = File.ReadAllText(find);

                            var match1 = Regex.Match(str, regStr, RegexOptions.Multiline);

                            if (match.Groups.Count > 1)
                            {
                                var val1 = match1.Groups[1].Value.Trim();
                                var strs1 = val1.Split(new[] { " ","\t" }, StringSplitOptions.RemoveEmptyEntries);
                                //lastColor = val1;
                                var color = System.Windows.Media.Color.FromRgb(
                                    byte.Parse(strs1[0]),
                                    byte.Parse(strs1[1]),
                                    byte.Parse(strs1[2]));


                                var country = new Country();
                                country.Tag = key;
                                country.Name = System.IO.Path.GetFileNameWithoutExtension(fileName);
                                country.Color = color;

                                countries.Add(country);

                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Application.Current.GetLocator().Main.log.Error(ex.Message);
                    }



                }

           
 
            }

            return countries;

        }

        public static void SetWater(List<Prov> Provs )
        {
            var dict = Provs.Where(v=>v.Id!=-1).ToDictionary(v => v.Id, v => v);

            var vm = SimpleIoc.Default.GetInstance<MainViewModel>();
            //var file = AppDomain.CurrentDomain.BaseDirectory + "files/default.map";
            var file = vm.FileProvider.GetModOrOriFile(vm.FileProvider.GetDefaultMap());
            if (File.Exists(file))
            {
                var regStr1 = "^sea_starts = {([^}]+)";
                var regStr2 = "^lakes = {([^}]+)";

                var str = File.ReadAllText(file);


                str=Regex.Replace(str, "#[^\r]*","");

                try
                {
                    var match1 = Regex.Match(str, regStr1, RegexOptions.Multiline);
                    var match2 = Regex.Match(str, regStr2, RegexOptions.Multiline);

                    if (match1.Groups.Count > 1)
                    {
                        var val = match1.Groups[1].Value;
                        var vals = val.Split(new string[] {" ","\r\n","\t"}, StringSplitOptions.RemoveEmptyEntries);

                        var list = vals.Select(v => int.Parse(v)).ToList();
                        foreach (var i in list)
                        {
                            if (dict.ContainsKey(i))
                                dict[i].IsSea = true;
                        }

                    }

                    if (match2.Groups.Count > 1)
                    {
                        var val = match2.Groups[1].Value;
                        var vals = val.Split(new string[] { " ", "\r\n", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                        var list = vals.Select(v => int.Parse(v)).ToList();
                        foreach (var i in list)
                        {
                            if (dict.ContainsKey(i))
                                dict[i].IsLake = true;
                        }

                    }

                    var mayWasteLands = Provs.Where(v => v.Owner == null);
                    foreach (var mayWasteLand in mayWasteLands)
                    {
                        /////////////////////////////////////////////////////////
                        //判断是否死地;

                        if (mayWasteLand.Config != null)
                        {
                            if (mayWasteLand.Config.Items.FirstOrDefault(v => v.Name.ToLower() == "base_tax") != null ||
                                mayWasteLand.IsLake || mayWasteLand.IsSea)
                            {
                                
                            }
                            else
                            {
                                mayWasteLand.IsWaste = true;
                            }
                        }
                        else
                        {
                            mayWasteLand.IsWaste = true;
                        }
                
                    }
                }

               
              

                catch (Exception ex)
                {

                    Application.Current.GetLocator().Main.log.Error(ex.Message);
                }

            
            }

      
        }

        public static Dictionary<string, System.Windows.Media.Color> CountryColorDict;
        public static System.Windows.Media.Color? GetCountryColor(string countryName)
        {
            if (CountryColorDict == null)
            {
                var vm = SimpleIoc.Default.GetInstance<MainViewModel>();
                CountryColorDict = vm.Countries.ToDictionary(v => v.Tag, v => v.Color);
            }

            if (countryName!=null && CountryColorDict.ContainsKey(countryName))
                return CountryColorDict[countryName];
            else
                return null;
        }


    }

    public class ColorRangeDict
    {
        public Dictionary<Int32, Prov> Dict;

        public ColorRangeDict()
        {
            this.Dict = new Dictionary<int, Prov>();
        }


    }


    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Tag { get; set; }

        public System.Windows.Media.Color Color { get; set; }
    }

    public class Prov:ISelected,INotifyPropertyChanged
    {
        private Config _config;

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string name)
        {
            if(PropertyChanged!=null)
                PropertyChanged(this,new PropertyChangedEventArgs(name));
        }

        public int Id { get; set; }
        //public Path DrawPath { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public Config Config
        {
            get { return _config; }
            set
            {
                _config = value;
                RaisePropertyChanged("Config");
            }
        }

        [JsonIgnore]
        public Config1 Config1 { get; set; }
        public string ConfigFileName { get; set; }
        [JsonIgnore]
        public Geometry Geometry { get; set; }
        [JsonIgnore]
        public System.Windows.Media.DrawingVisual Visual { get; set; }
        public int Color { get; set; }

        [JsonIgnore]
        public Contours Contours { get; set; }
        //public List<Point2> Points { get; set; }

        public List<Point2> Points { get; set; }


        public string Owner { get; set; }

        public bool IsSea { get; set; }
        public bool IsLake { get; set; }

        public bool IsWaste { get; set; }

        public System.Drawing.Rectangle Rectangle { get; set; }

        public string Region
        {
            get { return $"{Rectangle.X}:{Rectangle.Y} - {Rectangle.Right}:{Rectangle.Bottom}"; }
        }
        public Prov()
        {
            this.Points = new List<Point2>();
        }

        public void CalRect()
        {
            UInt16 minx = Points.First().X;
            UInt16 miny = Points.First().Y;
            UInt16 maxx = Points.First().X;
            UInt16 maxy = Points.First().Y;

            foreach (var point in Points)
            {
                if (point.X < minx)
                    minx = point.X;

                if (point.Y < miny)
                    miny = point.Y;

                if (point.X > maxx)
                    maxx = point.X;

                if (point.Y > maxy)
                    maxy = point.Y;
            }

            this.Rectangle = new System.Drawing.Rectangle(minx, miny, maxx - minx, maxy - miny);

            ConvToAllRelPos(minx, miny);
        }

        public void ConvToAllRelPos(ushort minx, ushort miny)
        {

            //将绝对位置变成相对位置
            foreach (var point2 in Points)
            {
                point2.X -= minx;
                point2.Y -= miny;
            }
        }

        public unsafe Bitmap DrawImage()
        {
            if(this.Points.Count==0)
                return new Bitmap(1,1);

            var bmp = new System.Drawing.Bitmap(this.Rectangle.Width + 4, this.Rectangle.Height + 4);

            //var color = System.Drawing.Color.AliceBlue.ToArgb();

            var set = new HashSet<uint>(this.Points.Select(v=>v.Key));
            var prov = this;
            //var grfx = Graphics.FromImage(bmp);
            bmp.Do((p, width, height) =>
            {
                var ptr = (Int32*)p.ToPointer();
                
                ////////////////////////////////////

                for (UInt16 y = 0; y < height; y++)
                {
                    for (UInt16 x = 0; x < width; x++)
                    {
                        //var val = *ptr;

                        var key = (UInt32)((UInt32)(x - 2) << 16 | (y - 2));

                        if (set.Contains(key))
                            *ptr = prov.Color;
                        //else
                        //    *ptr = color;
    
                        ptr++;

                    }
                }

                

                ////////////////////////////////////////////////
                ////增加border
                //var ptr1 = (Int32*)p.ToPointer();

                //List<System.Drawing.Point> borders=new List<Point>();

                //////////////////////////////////////

                //for (UInt16 y = 1; y < height-1; y++)
                //{
                //    for (UInt16 x = 1; x < width-1; x++)
                //    {
                //        var cur = (ptr1+x+y*width);

                //        //var left = *(cur - 1);
                //        var right = *(cur + 1);
                //        //var top = *(cur -width);
                //        var bottom = *(cur +width);

                //        if(*cur==0 && (right!=0 || bottom!=0))
                //            borders.Add(new Point(x,y));

                //    }
                //}

                ///////////////////////////////////////////////////////
                //var ptr2 = (Int32*)p.ToPointer();
                //foreach (var border in borders)
                //{
                //    *(ptr2 + border.X + border.Y*width) = prov.Color;
                //}

            });

            

            return bmp;
        }

        public bool IsSelected { get; set; }

    }

    public class Point2
    {
        public UInt16 X { get; set; }
        public UInt16 Y { get; set; }

        public Point2(UInt16 x, UInt16 y)
        {
            this.X = x;
            this.Y = y;
        }

        public UInt32 Key
        {
            get
            {
                return (UInt32)((UInt32)X << 16 | Y);
            }
        }

        public override string ToString()
        {
            return $"{X}:{Y}";
        }
    }




    public class Contours
    {
        public List<Contour> Items { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public Contours()
        {
            this.Items = new List<Contour>();
        }
    }

    public class Contour
    {
        public System.Drawing.Point[] Pts { get; set; }



    }



    public static class Extend
    {
        public static System.Windows.Point ToPoint(this System.Drawing.Point pt)
        {
            return new System.Windows.Point(pt.X, pt.Y);
        }

        public static System.Windows.Point ToPoint(this System.Drawing.Point pt,int left,int top)
        {
            return new System.Windows.Point(pt.X+left, pt.Y+top);
        }

        public static System.Windows.Point ToPoint(this System.Drawing.Point pt, int left, int top,int mul)
        {
            return new System.Windows.Point((pt.X + left)*mul, (pt.Y + top)*mul);
        }
    }

    public class MyCanvas : FrameworkElement
    {
        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Brushes.Red,new Pen(Brushes.Black,2),new Rect(200,200,400,400));
        }
    }
}
