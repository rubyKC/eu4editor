using GalaSoft.MvvmLight.Ioc;
using GeoPPT.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Eu4Editor.DrawingVisual;

namespace GeoPPT.Draw
{


    internal partial class SelectCanvas
    {
     
        
        public SelectSet SelSet = new SelectSet();
        public bool IsCanRelSel = true;
        public event Func<Rect, IEnumerable<ISelected>> RegionHitTestAction;
        public event Func<Point, ISelected> PointHitTestAction;
        //private bool IsSelInThisTurn = false;//是否穿透选中；
        private FrameworkElement Ctrl;
        private bool IsStartDrag = false;
        internal MouseButtonEventHandler MouseLeftButtonUp { get; set; }
        internal MouseButtonEventHandler MouseButtonDown { get; set; }
        internal MouseEventHandler MouseMove { get; set; }

        public bool isStart = false;
        public SelectCanvas(FrameworkElement ctrl)
        {
            Ctrl = ctrl;
          
            //这个是必要的，否则很有可能运行慢的时候，down和move的点位置不一样
            Point startPoint = new Point();
           
            
            
            
            MouseButtonDown = (obj, e) =>
            {
                isStart = false;
                if (Keyboard.Modifiers != ModifierKeys.Control)
                {

                }
                if (IsStartDrag != true)
                    isStart = true;
                startPoint = e.GetPosition(ctrl);

                ////非框选下，点击执行这个 判断Ctrl和非Ctrl
                //FindSelect(this.Ctrl, startPoint);
            };

            MouseMove = (obj, e) =>
            {
                var point = e.GetPosition(ctrl);
                //去除抖动会框选的问题
                if (Distance(startPoint, point) < 4)
                    return;
                //去除滚动条点击会框选的问题
                var rect = new Rect(0, 0, ctrl.ActualWidth - 20, ctrl.ActualHeight - 25);
                if (!rect.Contains(point))
                {
                    isStart = false;
                    return;
                }
                //IsSelInThisTurn = false;
                if (isStart && IsCanRelSel && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    //Test2008.Core.Shape.SelectCollection.Clear();
                    //////////////////////////////////////////////                   
                    //注意这个，相等的情况是在容器上点击，非相等的情况是在元素上点击，当元素不可拖动的时候，才可以从元素上点击拉成方框
                    //if (this == e.Source)
                    {
                        var adornerLayer = AdornerLayer.GetAdornerLayer(ctrl);
                        adornerLayer.SnapsToDevicePixels = true;

                        var adorner = new SelRectAdorner(this, startPoint);
                        adorner.RegionHitTestAction = RegionHitTestAction;
                        adornerLayer.Add(adorner);
                    }

                    //e.Handled = true;
                }

                //不可少
                isStart = false;
            };

            MouseLeftButtonUp = (obj, e) =>
            {
                if (isStart)
                {
                    ////非框选下，点击执行这个 判断Ctrl和非Ctrl
                    FindSelect(this.Ctrl, startPoint);
                    //不可少
                    isStart = false;
                }
            };

        }

        public void Enable(bool flag)
        {
            this.Ctrl.MouseLeftButtonDown -= MouseButtonDown;
            this.Ctrl.MouseMove -= MouseMove;
            this.Ctrl.MouseLeftButtonUp -= MouseLeftButtonUp;

            if (flag)
            {
                this.Ctrl.MouseLeftButtonDown += MouseButtonDown;
                this.Ctrl.MouseMove += MouseMove;
                this.Ctrl.MouseLeftButtonUp += MouseLeftButtonUp;
            }
        }

        //public void FindSelect(Visual visual, Point startPoint)
        //{
        //    //if (PointHitTestAction != null)
        //    //{
        //        //var newpt = new Point(startPoint.X / GEConfig.Instance.PagePercent * 100,
        //        //    startPoint.Y / GEConfig.Instance.PagePercent * 100);

        //        var item = FindSelect(startPoint);
        //        if (item != null)
        //        {
        //            this.SelSet.Add(item);
                    
        //        }
        //        else
        //            this.SelSet.Clear();
        //    //}
        //    //else
        //    //    this.SelSet.Clear();

        //}

        public void FindSelect(Visual visual, Point pt, HitTestResultCallback callback=null)
        {
            var hitResultsList = new List<DependencyObject>();

            ////底板cell不参与Rect Sel的判定
            HitTestFilterCallback filter = (target) =>
            {
                if (target is FrameworkElement && ((FrameworkElement)target).DataContext is ISelected)
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                else
                    return HitTestFilterBehavior.Continue;
            };

            var hitTestPara = new PointHitTestParameters(pt);

            
            if(callback==null)
            callback = result =>
            {
                var hit = ((PointHitTestResult) result).VisualHit as DrawingVisual;
                if (hit != null)
                {
                    this.SelSet.Add(MyVisualHost1.FindDict[hit]);

                }
                else
                    this.SelSet.Clear();

                return HitTestResultBehavior.Stop;
            };


            VisualTreeHelper.HitTest(visual, null, callback, hitTestPara);

        }

        public double Distance(Point p1, Point p2)
        {
            var dis1 = Math.Pow(Math.Abs(p1.X - p2.X), 2);
            var dis2 = Math.Pow(Math.Abs(p1.Y - p2.Y), 2);

            return Math.Sqrt(dis1 + dis2);
        }

        /// <summary>
        /// 框选框
        /// </summary>
        public class SelRectAdorner : Adorner
        {
            private Point Location;
            private Point startPoint;
            private SelectCanvas canvas;

            public Func<Rect, IEnumerable<ISelected>> RegionHitTestAction { get; set; }


            public SelRectAdorner(SelectCanvas canvas, Point dragStartPoint)
                : base(canvas.Ctrl)
            {
                this.canvas = canvas;
                this.startPoint = dragStartPoint;
                this.Location = dragStartPoint;

                this.SnapsToDevicePixels = true;
                RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

                this.MouseLeftButtonUp += new MouseButtonEventHandler(SelRect_MouseLeftButtonUp);
                this.MouseMove += new MouseEventHandler(SelRect_MouseMove);
            }

            private void SelRect_MouseMove(object sender, MouseEventArgs e)
            {
                Location = e.GetPosition(this);
                this.InvalidateVisual();
            }

            private void SelRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                // release mouse capture
                if (this.IsMouseCaptured)
                    this.ReleaseMouseCapture();

                //方框选中。。。
                this.FindSelect(canvas.Ctrl);

                /////////////////////////////////////////
                //Add 2013.9
                if (RegionHitTestAction != null)
                {
                    var list = RegionHitTestAction(new Rect(this.startPoint, this.Location));
                    canvas.SelSet.Clear();
                    canvas.SelSet.AddRange(list);

                    //弹出快捷工具条
                    if (list!=null && list.Any())
                    {
                        //var tools = SimpleIoc.Default.GetInstance<ToolGroupViewModel>();
                        //if (tools != null)
                        //{
                        //    var canvasView = SimpleIoc.Default.GetInstance<CanvasView>();
                        //    var quickOper = SimpleIoc.Default.GetInstance<QuickOperBarViewModel>();
                        //    if (quickOper != null && canvasView != null)
                        //    {
                        //        quickOper.DealCanvasMouseUp(canvasView.Part_ViewBox, e);
                        //    }
                        //}
                    }
                }


                /////////////////////////////////////////

                // remove adorner (=this) from adorner layer
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas.Ctrl);
                if (adornerLayer != null)
                    adornerLayer.Remove(this);

                //this.canvas.Ctrl.Focus();
                //this.canvas.Ctrl.RaiseEvent(e);

                //var ctrl = SimpleIoc.Default.GetInstance<CanvasView>();
                //ctrl.Focusable = true;
                //ctrl.Focus();
                //FocusManager.SetFocusedElement(ctrl,ctrl.PART_DesignView);
                //Keyboard.Focus(ctrl);
                //e.Handled = true;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                Color color = Color.FromArgb(40, 0, 40, 0);
                Pen pen = new Pen(Brushes.SkyBlue, 1);

                Brush brush = new SolidColorBrush(color);

                // without a background the OnMouseMove event would not be fired !
                drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

                drawingContext.DrawRectangle(brush, pen, new Rect(this.startPoint, this.Location));
            }

            public void FindSelect(Visual visual)
            {
                var hitResultsList = new List<DependencyObject>();

                ////底板cell不参与Rect Sel的判定
                HitTestFilterCallback filter = (target) =>
                {
                    if (target is FrameworkElement && ((FrameworkElement)target).DataContext is ISelected)
                        return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                    else
                        return HitTestFilterBehavior.Continue;
                };

                var hitTestPara = new GeometryHitTestParameters(new RectangleGeometry(new Rect(this.startPoint, this.Location)));


                HitTestResultCallback callback = result =>
                {
                    switch (((GeometryHitTestResult)result).IntersectionDetail)
                    {
                        case IntersectionDetail.FullyContains:
                            return HitTestResultBehavior.Continue;

                        case IntersectionDetail.Intersects:
                            hitResultsList.Add(result.VisualHit);
                            return HitTestResultBehavior.Continue;

                        case IntersectionDetail.FullyInside:
                            hitResultsList.Add(result.VisualHit);
                            return HitTestResultBehavior.Continue;
                        default:
                            return HitTestResultBehavior.Stop;
                    }
                };


                VisualTreeHelper.HitTest(visual, null, callback, hitTestPara);

                /////////////////
                canvas.SelSet.Clear();
                /////////////////

       

                //var query = from item in hitResultsList
                //            let obj = item as DrawingVisual
                //            where obj != null
                //            let data = obj.DataContext as ISelected
                //            where data != null
                //            select data;

                var query1 = hitResultsList
                    .OfType<DrawingVisual>()
                    .Where(v => v != null)
                    .Select(v => MyVisualHost1.FindDict[v])
                    .ToList();


                var list = query1.ToList();

                if (list.Count > 0)
                    //等会修改成集群添加模式，大大减少刷新
                    canvas.SelSet.AddRange(query1);
                //data.DrawObj = obj;
            }
        }
    }

}

