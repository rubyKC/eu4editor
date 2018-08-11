using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu4Editor.ViewModel;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;

namespace Eu4Editor
{
    // // Copyright (c) Microsoft. All rights reserved.
    // // Licensed under the MIT license. See LICENSE file in the project root for full license information.

    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    namespace DrawingVisual
    {
        public class MyVisualHost : FrameworkElement
        {
            // Create a collection of child visual objects.
            private readonly VisualCollection _children;

            public MyVisualHost()
            {
                _children = new VisualCollection(this)
            {
                CreateDrawingVisualRectangle(),
                CreateDrawingVisualText(),
                CreateDrawingVisualEllipses()
            };

                // Add the event handler for MouseLeftButtonUp.
                MouseLeftButtonUp += MyVisualHost_MouseLeftButtonUp;
            }

            // Capture the mouse event and hit test the coordinate point value against
            // the child visual objects.
            private void MyVisualHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                // Retreive the coordinates of the mouse button event.
                Point pt = e.GetPosition((UIElement)sender);

                // Initiate the hit test by setting up a hit test result callback method.
                VisualTreeHelper.HitTest(this, null, MyCallback, new PointHitTestParameters(pt));
            }

            // If a child visual object is hit, toggle its opacity to visually indicate a hit.
            public HitTestResultBehavior MyCallback(HitTestResult result)
            {
                if (result.VisualHit.GetType() == typeof(System.Windows.Media.DrawingVisual))
                {
                    ((System.Windows.Media.DrawingVisual)result.VisualHit).Opacity =
                        ((System.Windows.Media.DrawingVisual)result.VisualHit).Opacity == 1.0 ? 0.4 : 1.0;
                }

                // Stop the hit test enumeration of objects in the visual tree.
                return HitTestResultBehavior.Stop;
            }

            // Create a DrawingVisual that contains a rectangle.
            private System.Windows.Media.DrawingVisual CreateDrawingVisualRectangle()
            {
                System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();

                // Retrieve the DrawingContext in order to create new drawing content.
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                // Create a rectangle and draw it in the DrawingContext.
                Rect rect = new Rect(new Point(160, 100), new Size(320, 80));
                drawingContext.DrawRectangle(Brushes.LightBlue, null, rect);

                // Persist the drawing content.
                drawingContext.Close();

                return drawingVisual;
            }

            // Create a DrawingVisual that contains text.
            private System.Windows.Media.DrawingVisual CreateDrawingVisualText()
            {
                // Create an instance of a DrawingVisual.
                System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();

                // Retrieve the DrawingContext from the DrawingVisual.
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                // Draw a formatted text string into the DrawingContext.
                drawingContext.DrawText(
                    new FormattedText("Click Me!",
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        36, Brushes.Black),
                    new Point(200, 116));

                // Close the DrawingContext to persist changes to the DrawingVisual.
                drawingContext.Close();

                return drawingVisual;
            }

            // Create a DrawingVisual that contains an ellipse.
            private System.Windows.Media.DrawingVisual CreateDrawingVisualEllipses()
            {
                System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                drawingContext.DrawEllipse(Brushes.Maroon, null, new Point(430, 136), 20, 20);
                drawingContext.Close();

                return drawingVisual;
            }


            // Provide a required override for the VisualChildrenCount property.
            protected override int VisualChildrenCount => _children.Count;

            // Provide a required override for the GetVisualChild method.
            protected override Visual GetVisualChild(int index)
            {
                if (index < 0 || index >= _children.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return _children[index];
            }
        }

        public enum MapColorModes
        {
            Provs,
            Country,
        }

        public class MyVisualHost1 : FrameworkElement
        {
            // Create a collection of child visual objects.
            private VisualCollection _children;

            static MyVisualHost1()
            {
                    FindDict=new Dictionary<System.Windows.Media.DrawingVisual, Prov>();
            }
          
            public MyVisualHost1()
            {
                 _children = new VisualCollection(this);

                // Add the event handler for MouseLeftButtonUp.
                MouseLeftButtonUp += MyVisualHost_MouseLeftButtonUp;
            }
            

            public static Dictionary<System.Windows.Media.DrawingVisual, Prov> FindDict;
            public void Init(MapColorModes mapColor= MapColorModes.Provs,List<Prov> list=null )
            {
                if (list == null)
                {

                    _children.Clear();
                    FindDict = new Dictionary<System.Windows.Media.DrawingVisual, Prov>();

                    ////////////////////
                    var vm = SimpleIoc.Default.GetInstance<MainViewModel>();

                    this.BeginInit();
                    foreach (var prov in vm.Provs)
                    {
                        var visual = CreateDrawingVisualProvince(prov, mapColor);


                        _children.Add(visual);
                        FindDict.Add(visual, prov);
                        prov.Visual = visual;
                    }
                    this.EndInit();
                    ///////////////////

                    var query =
                        vm.Provs.Select(
                            v =>
                                new SimpleProv
                                {
                                    Name = v.Name,
                                    Color = v.Color,
                                    Contours = v.Contours
                                });
                    var str=JsonConvert.SerializeObject(query, Formatting.Indented);
                    System.IO.File.WriteAllText("points.json",str);

                }
                else
                {
                    var visuals = FindDict.Where(v => list.Contains(v.Value))
                        .Where(v=>!v.Value.IsLake && !v.Value.IsSea && !v.Value.IsWaste)
                        .Select(v=>v.Key);
                    visuals.ToList().ForEach(v =>
                    {

                        FindDict.Remove(v);
                        _children.Remove(v);
                    });

                    this.BeginInit();
                    foreach (var prov in list)
                    {
                        var visual = CreateDrawingVisualProvince(prov, mapColor);
                        _children.Add(visual);
                        FindDict.Add(visual, prov);
                        prov.Visual = visual;
                    }
                    this.EndInit();
                }
            }

            // Capture the mouse event and hit test the coordinate point value against
            // the child visual objects.
            private void MyVisualHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                // Retreive the coordinates of the mouse button event.
                Point pt = e.GetPosition((UIElement)sender);

                // Initiate the hit test by setting up a hit test result callback method.
                VisualTreeHelper.HitTest(this, null, MyCallback, new PointHitTestParameters(pt));
            }

            public event Action<Prov> Selected;
            private System.Windows.Media.DrawingVisual lastDrawingVisual;
            // If a child visual object is hit, toggle its opacity to visually indicate a hit.
            public HitTestResultBehavior MyCallback(HitTestResult result)
            {
                if (result.VisualHit.GetType() == typeof(System.Windows.Media.DrawingVisual))
                {
                    var visual = (System.Windows.Media.DrawingVisual) result.VisualHit;
                    //visual.Opacity = 0.4;

                    //if (lastDrawingVisual != null)
                    //    lastDrawingVisual.Opacity = 1;

                    //lastDrawingVisual = visual;


                    ////////////////////////////////
                    if (Selected != null)
                    {
                        if (FindDict.ContainsKey(visual))
                        {
                            Selected(FindDict[visual]);
                        }
                    }

                }

                // Stop the hit test enumeration of objects in the visual tree.
                return HitTestResultBehavior.Stop;
            }


            private System.Windows.Media.DrawingVisual CreateDrawingVisualProvince(Prov prov, MapColorModes mapColor = MapColorModes.Provs)
            {
                var drawingVisual = new System.Windows.Media.DrawingVisual();
                var dc = drawingVisual.RenderOpen();

                Brush brush=null;

                if (mapColor == MapColorModes.Provs)
                {
                    var color1 = System.Drawing.Color.FromArgb(prov.Color);
                    brush =new SolidColorBrush(Color.FromRgb(color1.R,color1.G,color1.B));
                }
                else if(mapColor== MapColorModes.Country)
                {
                    var color = Logic.GetCountryColor(prov.Owner);
                    if (color == null)
                    {
                        if (prov.IsLake || prov.IsSea)
                        {
                            var color1 = System.Drawing.Color.FromArgb(prov.Color);
                            brush = new SolidColorBrush(Color.FromRgb(color1.R, color1.G, color1.B));
                        }
                        else if(!prov.IsWaste)
                            brush = new SolidColorBrush(Colors.LightGray);
                        else 
                            brush = new SolidColorBrush(Colors.DimGray);
                    }
                    else
                        brush = new SolidColorBrush(color.Value);
                }
               
                //dc.DrawGeometry(brush,new Pen(Brushes.Red,0),prov.Geometry );
                
                dc.DrawGeometry(brush, new Pen(brush, 0.45), prov.Geometry);

                

                //////////////////////////////////////////
          //      FormattedText formattedText = new FormattedText(
          //"11",
          //CultureInfo.GetCultureInfo("en-us"),
          //FlowDirection.LeftToRight,
          //new Typeface(new FontFamily("宋体"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
          //12,
          //Brushes.Red);

          //      Geometry textGeometry = formattedText.BuildGeometry(new Point(0, 0));
          //      var rect = prov.Geometry.Bounds;
          //      var rect1 = textGeometry.Bounds;

          //      var w = rect.Width/rect1.Width;
          //      var h = rect.Height/rect1.Height;

          //      var scale = Math.Min(w, h);
          //      //var max=t
          //      var translateTransform = new ScaleTransform();
          //      //translateTransform.X = -textGeometry.Bounds.Left;
          //      //translateTransform.Y = -textGeometry.Bounds.Top;
          //      dc.PushTransform(translateTransform);
          //      dc.DrawGeometry(Brushes.Red, new Pen(Brushes.Red, 1.0), textGeometry);

                dc.Close();
                
                return drawingVisual; 
            }



            // Provide a required override for the VisualChildrenCount property.
            protected override int VisualChildrenCount => _children.Count;

            // Provide a required override for the GetVisualChild method.
            protected override Visual GetVisualChild(int index)
            {
                if (index < 0 || index >= _children.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return _children[index];
            }
        }
    }


    public class SimpleProv
    {
        public string Name { get; set; }
        public int Color { get; set; }

        public Contours Contours{ get; set; }
    }

}
