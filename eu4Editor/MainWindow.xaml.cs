using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Eu4Editor.DrawingVisual;
using Eu4Editor.ViewModel;
using GalaSoft.MvvmLight.Ioc;
using GeoPPT.Core;
using GeoPPT.Draw;
using ReadEu4Config;
using Application = System.Windows.Application;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using ToolTip = System.Windows.Controls.ToolTip;

namespace Eu4Editor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
          
            InitializeComponent();

            //Application.Current.GetLocator().Main.log.Warn("警告：warn");

            this.Loaded += MainWindow_Loaded;


            

            //ILog log = log4net.LogManager.GetLogger(typeof(Program));

            ////记录错误日志

            //log.Error("error", new Exception("在这里发生了一个异常,Error Number:" + random.Next()));

            //记录严重错误

            //log.Fatal("fatal", new Exception("在发生了一个致命错误，Exception Id："+random.Next())); 

            //记录一般信息

            //log.Info("提示：系统正在运行");

            //记录调试信息

            //log.Debug("调试信息：debug");

            //记录警告信息

          
        }

        private SelectCanvas mySelectCanvas;
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //this.DataContext = new MainViewModel();
            //Open open=new Open();
            //open.Owner = this;
            //open.WindowStartupLocation= WindowStartupLocation.CenterOwner;
            //open.WindowStyle= WindowStyle.None;
            //open.ShowDialog();




            var ofg = new System.Windows.Forms.FolderBrowserDialog();

            if (File.Exists("LastDir.save"))
            {
                var path = File.ReadAllText("LastDir.save");
                if (Directory.Exists(path))
                    ofg.SelectedPath = path;
            }

            var result = ofg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var vm = SimpleIoc.Default.GetInstance<MainViewModel>();
                vm.FileProvider=new FileProvider(ofg.SelectedPath);
                if (vm.FileProvider.Mode != GameModes.None)
                {
                    Load();
                    File.WriteAllText("LastDir.save", ofg.SelectedPath);
                }
                else
                {
                    System.Windows.MessageBox.Show("不是有效P社游戏目录,无法进行分析和渲染地图");
                    File.WriteAllText("LastDir.save", ofg.SelectedPath);
                    Application.Current.Shutdown(0);
                }
            }
            else
                Application.Current.Shutdown(0);
        }

        private void Load()
        {
            this.MouseRightButtonDown += MainWindow_MouseRightButtonDown;


            mySelectCanvas = new SelectCanvas(this.canvas);
            mySelectCanvas.Enable(true);
            //this.SelectCanvas = canvas;
            //SimpleIoc.Default.Register(() => canvas.SelSet);


            //canvas.RegionHitTestAction += canvas_RegionHitTestAction;
            //canvas.PointHitTestAction += canvas_PointHitTestActionWithMultiSelected;


            mySelectCanvas.SelSet.CollectionChanged += (o, e1) =>
            {
                if (e1.Action == NotifyCollectionChangedAction.Remove)
                {
                    var list = e1.OldItems.OfType<Prov>().ToList();
                    foreach (var prov in list)
                    {
                        if (prov.IsLake || prov.IsSea || prov.IsWaste)
                            continue;

                        if (prov.Visual != null)
                            prov.Visual.Opacity = 1;
                    }
                }

                else if (e1.Action == NotifyCollectionChangedAction.Add)
                {
                    var list = e1.NewItems.OfType<Prov>().ToList();
                    foreach (var prov in list)
                    {
                        if (prov.IsLake || prov.IsSea || prov.IsWaste)
                            continue;

                        if (prov.Visual != null)
                            prov.Visual.Opacity = 0.4;
                    }
                }
            };

            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    var vm = SimpleIoc.Default.GetInstance<MainViewModel>();

                    var provs = LoadProvs();
                    LoadProvConfig(provs);
                    Logic.SetWater(provs);

                    var countries = Logic.LoadCountryColorDict1();
                    vm.Countries = countries;

                    vm.Provs = provs.OrderBy(v => v.Id).ToList();
                }
                catch (Exception ex)
                {
                    Application.Current.GetLocator().Main.log.Error(ex.Message);
                }

            });

            task.ContinueWith(v =>
            {
                //Draw
                this.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        //MyVisualHost1.Init();
                        this.MyVisualHost1.Init(MapColorModes.Country);

                        var vm = SimpleIoc.Default.GetInstance<MainViewModel>();
                        vm.Percent = 100;
                        vm.Write = "";
                    }
                    catch (Exception ex)
                    {

                        Application.Current.GetLocator().Main.log.Error(ex.Message);
                    }


                });
            });
        }

        private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //var pt = e.GetPosition(MyVisualHost1);

            //this.MyVisualHost1.ContextMenu = null;


            //var menu=new System.Windows.Controls.ContextMenu();

            //var menuItem = new System.Windows.Controls.MenuItem();
            //menuItem.Header = "选择该省的Tag";
            //menuItem.Click+=(o,e1)=>
            //{

            //};

            //menu.Items.Add(menuItem);

            //toolTip.PlacementRectangle = new Rect(pt, new System.Windows.Size(100, 100));
            //toolTip.IsOpen = false;

            //this.mySelectCanvas.FindSelect(MyVisualHost1, pt, result =>
            //{
            //    var hit = ((PointHitTestResult)result).VisualHit as System.Windows.Media.DrawingVisual;
            //    if (hit != null)
            //    {
            //        var prov = MyVisualHost1.FindDict[hit];

            //        if (!prov.IsLake && !prov.IsSea && !prov.IsWaste)
            //        {
            //            menu.PlacementTarget = this.MyVisualHost1;
            //            menu.IsOpen = true;

            //        }
            //    }
            //    else
            //    {

            //    }

            //    return HitTestResultBehavior.Stop;
            //});
        }

        private List<Prov> LoadProvs()
        {
            var vm = SimpleIoc.Default.GetInstance<MainViewModel>();

            var idDict = Logic.LoadProvIdDict();

            vm.Percent = 5;
            Console.WriteLine("Loaded ProvIds");
            vm.Write="Loaded ProvIds";

            //var file = AppDomain.CurrentDomain.BaseDirectory + "files/provinces.bmp";
            var file = vm.FileProvider.GetModOrOriFile(vm.FileProvider.GetProvMap());

            if (File.Exists(file))
            {
                var revert = vm.FileProvider.IsRevert();
                var dict = Logic.GetProvinces(file,revert);

                var provs = dict.Dict.Values.ToList();//.OrderBy(v => v.Points.Count).ToList();

                ////////////////////////////////////////
                //Cut and Draw Prov 
                //CvInvoke.UseOpenCL = true;
                //CvInvoke.UseOptimized = true;

                int i = 0;
                foreach (var prov in provs)
                {
                    var img = prov.DrawImage();

                    ///////////////////////////////
                    //clear point;
                    prov.Points.Clear();


                    try
                    {
                        if (i == 3250)
                        {
                            
                        }


                        var cons = Logic.GetContours(img);
                        //prov.Id = i;

                        if (idDict.ContainsKey(prov.Color))
                            prov.Id = idDict[prov.Color];
                        else
                            prov.Id = -1;

                        var geometry = Logic.GetGeometry(cons, prov);
                        //var geometry = Logic.GetGeometryByCurve(cons, prov);
                        prov.Contours = cons;
                        prov.Geometry = geometry;
                    }
                    catch (Exception ex)
                    {

                        Application.Current.GetLocator().Main.log.Error(ex.Message);
                    }
               

                    //if (i % 100 == 0)
                    {
                        vm.Percent = ((float)i/provs.Count)*60+5;
                        Console.WriteLine(i);
                        vm.Write = i.ToString();
                    }

                    i++;
                  
                }

                Console.WriteLine((float)Logic.count1 / Logic.count2);
                vm.Write = ((float) Logic.count1/Logic.count2).ToString();

                return provs;

                
            }

            return null;
        }




        #region Zoom

        private void MasterImage_MouseWheel(object sender, MouseWheelEventArgs e)

        {

            var image = sender as FrameworkElement;

            TransformGroup group = this.MasterImage.RenderTransform as TransformGroup;

            Debug.Assert(group != null, "Can't find transform group from image compare panel resource");

            TranslateTransform transform = group.Children[1] as TranslateTransform;

            var point = e.GetPosition(image);

            double scale = e.Delta * 0.001 * 1.5;

            ZoomImage(group, point, scale);

        }

        private static void ZoomImage(TransformGroup group, Point point, double scale)

        {

            Point pointToContent = group.Inverse.Transform(point);

            ScaleTransform transform = group.Children[0] as ScaleTransform;

            if (transform.ScaleX + scale < 0.05)
                return;


            if (transform.ScaleX + scale > 2.5)
                return;


            transform.ScaleX += scale;

            transform.ScaleY += scale;

            TranslateTransform transform1 = group.Children[1] as TranslateTransform;

            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);

            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);

        }

        ToolTip toolTip = new ToolTip();
        private bool isMouseLeftButtonDown;
        private void MasterImage_MouseMove(object sender, MouseEventArgs e)
        {
            var pt = e.GetPosition(MyVisualHost1);
            toolTip.PlacementRectangle = new Rect(pt, new System.Windows.Size(100, 100));
            toolTip.IsOpen = false;

            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var image = sender as FrameworkElement;

                if (this.isMouseLeftButtonDown && e.LeftButton == MouseButtonState.Pressed)

                {
                    toolTip.IsOpen = false;
                    this.DoImageMove(image, e.GetPosition(image));

                }
                else
                {
                  

                    this.mySelectCanvas.FindSelect(MyVisualHost1,pt, result =>
                    {
                        var hit = ((PointHitTestResult)result).VisualHit as System.Windows.Media.DrawingVisual;
                        if (hit != null)
                        {
                            var prov = MyVisualHost1.FindDict[hit];

                            if (!prov.IsLake && !prov.IsSea && !prov.IsWaste)
                            {
                                toolTip.Content = prov.Name;
                                toolTip.IsOpen = true;
                            }



                        }
                        else
                        {
                            
                        }

                        
  
                        return HitTestResultBehavior.Stop;
                    });
                }

                mySelectCanvas.isStart = false;
            }
            else
            {
               
            }
        }

        private void MasterImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)

        {

            isMouseLeftButtonDown = false;

        }

        private void MasterImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

        {
            isMouseLeftButtonDown = true;

            previousMousePoint = e.GetPosition(sender as FrameworkElement);

        }
        private Point previousMousePoint;
        private void DoImageMove(FrameworkElement image, Point position)

        {
      

            TransformGroup group = this.MasterImage.RenderTransform as TransformGroup;

            Debug.Assert(group != null, "Can't find transform group from image compare panel resource");

            TranslateTransform transform = group.Children[1] as TranslateTransform;

            transform.X += position.X - this.previousMousePoint.X;

            transform.Y += position.Y - this.previousMousePoint.Y;

            this.previousMousePoint = position;

        }




        #endregion

        private void MyVisualHost1_OnSelected(Prov obj)
        {
            var vm = SimpleIoc.Default.GetInstance<MainViewModel>();
            vm.SelectedProv = obj;
            vm.Root=new List<Prov> {obj};
        }

        private void CountryMap_OnClick(object sender, RoutedEventArgs e)
        {
            this.MyVisualHost1.Init(MapColorModes.Country);

        }

        private void LoadProvConfig(List<Prov> provs )
        {
            var vm = SimpleIoc.Default.GetInstance<MainViewModel>();

            var dict = provs.Where(v=>v.Id!=-1).ToDictionary(v => v.Id, v => v);

            //var dir = @"H:\XUN\vic2 3.03\Victoria 2\mod\NWO\history\provinces";
            //var dir = Application.Current.GetLocator().Main.FileProvider.GetProvsPath();
            //var dir = @"C:\GameEditor\Europa Universalis IV\Europa Universalis IV\history\provinces";

            ////////////////////////////////////////
            //var files = Directory.GetFiles(dir);
            //var dirs = Directory.GetDirectories(dir);
            //var files = dirs.SelectMany(v => Directory.GetFiles(v));
            ///////////////////////////////////////////

            //var all = new ReadEu4Config.JObject();
            var files = vm.FileProvider.GetProvsFiles();
            int i = 0;
            foreach (var file in files)
            {
                //if (file.Contains("2836"))
                //{
                    
                //}

                var str = File.ReadAllText(file, Encoding.Default);

                var root = LoadProvConfig(str, file, dict);


                //if (i%100 == 0)
                {
                    vm.Percent = ((float) i/provs.Count)*35+65;
                    Console.WriteLine(root.Name);
                    vm.Write = root.Name;
                }

                i++;
            }

           
            //this.lb.ItemsSource = vm.Provs.OrderBy(v => v.Id);
        }

        private static Config LoadProvConfig(string str, string file, Dictionary<int, Prov> dict)
        {
            var root = new ReadEu4Config.Config();
            ParseEu.Parse(str, root);
            //all.Items.Add(root);
            root.Name = System.IO.Path.GetFileNameWithoutExtension(file);

            var strs = root.Name.Split(new string[] {"-", " "}, StringSplitOptions.RemoveEmptyEntries);

            var id = int.Parse(strs[0].Trim());
            if (dict.ContainsKey(id))
            {
                var prov = dict[id];
                prov.Config = root;
                prov.ConfigFileName = file;
                prov.Name = string.Join(" ", strs.Skip(1));

                /////////////////////////////////////////////////////////

                //prov.Config1 = Config1.Parse(root, file);

                /////////////////////////////////////////////////////////
                //设置Owner;
                var owner = root.Items.FirstOrDefault(v => v.Name.ToLower() == "owner") as JProperty;
                if (owner != null)
                    prov.Owner = owner.Value.ToUpper();
            }
            return root;
        }

        private void ChangeSelectsTag_OnClick(object sender, RoutedEventArgs e)
        {
            if (mySelectCanvas == null)
                return;

            foreach (var sel in mySelectCanvas.SelSet)
            {
                

                var prov = sel as Prov;

                if(prov.IsLake || prov.IsSea || prov.IsWaste)
                    continue;

                var fileName = prov.ConfigFileName;

                if (string.IsNullOrEmpty(fileName))
                    continue;

                var str = File.ReadAllText(fileName);

                var countryTag = this.tbTag.Text.ToUpper();

                //                    owner = USA
                //controller = USA
                //add_core = USA

                var vm = SimpleIoc.Default.GetInstance<MainViewModel>();
                if (vm.SelectModeObject.MapMode == MapModes.All)
                {
                    str = Regex.Replace(str, "owner\\s*=\\s*\\w+", $"owner = {countryTag}");
                    str = Regex.Replace(str, "controller\\s*=\\s*\\w+", $"controller = {countryTag}");
                    str = Regex.Replace(str, "add_core\\s*=\\s*\\w+", $"add_core = {countryTag}");
                }
                else
                {
                    var oritagMatches = Regex.Matches(str, "owner\\s*=\\s*(\\w+)");
                    if (oritagMatches.Count >= 1)
                    {
                        var last = oritagMatches.OfType<Match>().First().Groups[1].Value;


                        str = Regex.Replace(str, "owner\\s*=\\s*"+last, $"owner = {countryTag}");
                        str = Regex.Replace(str, "controller\\s*=\\s*"+last, $"controller = {countryTag}");
                        str = Regex.Replace(str, "add_core\\s*=\\s*"+last, $"add_core = {countryTag}");
                    }

                }

                File.WriteAllText(prov.ConfigFileName,str);

                {
                    var provs = Application.Current.GetLocator().Main.Provs;
                    var dict = provs.Where(v => v.Id != -1).ToDictionary(v => v.Id, v => v);

                    LoadProvConfig(str, prov.ConfigFileName, dict);
                }

                ////////////////////////////////////////////////
                //change Color 
                var color = Logic.GetCountryColor(countryTag);
                if (color != null)
                {
                    prov.Color = color.Value.ToColor().ToArgb();
                    prov.Owner = countryTag;
                }

            }

            //redraw
            this.MyVisualHost1.Init(MapColorModes.Country,mySelectCanvas.SelSet.Items.OfType<Prov>().ToList());
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            //var ctrl = sender as System.Windows.Controls.TreeView;
            //if (ctrl != null)
            {
                var data = tree.ItemsSource as List<Prov>;

                if (data != null && data.Count==1)
                {
                    this.tbTag.Text = data[0].Owner;
                }
            }
        }

        private void MenuItem_OnClick1(object sender, RoutedEventArgs e)
        {
            //var ctrl = sender as System.Windows.Controls.TreeView;
            //if (ctrl != null)
            {
                var data = tree.ItemsSource as List<Prov>;

                if (data != null && data.Count == 1)
                {
                    System.Diagnostics.Process.Start(data[0].ConfigFileName);
                }
            }

            
        }

        private void MenuItem_OnClick2(object sender, RoutedEventArgs e)
        {
            var data = tree.ItemsSource as List<Prov>;

            if (data != null && data.Count == 1)
            {
                var prov = data[0];

                var provs = Application.Current.GetLocator().Main.Provs;
                var dict = provs.Where(v => v.Id != -1).ToDictionary(v => v.Id, v => v);

                var str = File.ReadAllText(prov.ConfigFileName);
                LoadProvConfig(str, prov.ConfigFileName, dict);

                ////////////////////////////////////////////////
                //change Color 
                var color = Logic.GetCountryColor(prov.Owner);
                if (color != null)
                {
                    prov.Color = color.Value.ToColor().ToArgb();
                    prov.Owner = prov.Owner;
                }

                this.MyVisualHost1.Init(MapColorModes.Country, new List<Prov> { prov});
            }
        }

        private void MenuItem_OnClick4(object sender, RoutedEventArgs e)
        {
            var data = tree.ItemsSource as List<Prov>;

            if (data != null && data.Count == 1)
            {
                var path = Path.GetDirectoryName(data[0].ConfigFileName);
                System.Diagnostics.Process.Start(path);
            }
        }
    }




    public static class ExtendClass
    {
        public static ViewModelLocator GetLocator(this Application App)
        {
            return new ViewModelLocator();
        }
    }


}
