using System.Collections.Generic;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using log4net;

namespace Eu4Editor.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private List<Prov> _provs;
        private BitmapSource _source;
        private BitmapSource _source1;
        private int _provWidth;
        private int _provHeight;
        private double _percent;
        private Prov _selectedProv;
        private List<Country> _countries;
        private List<Prov> _root;
        private MapModeObject _selectModeObject;
        private List<MapModeObject> _mapModeObjects;
        private string _write;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            /// 

            MapModeObjects = new List<MapModeObject>
            {
                new MapModeObject { Name = "修改所有(排他)",Description = "修改该省份文件中所有的Tag为指定Tag",MapMode = ViewModel.MapModes.All},
                new MapModeObject { Name = "修改拥有者",Description = "修改该省份文件中所有的拥有者的Tag为指定Tag",MapMode = ViewModel.MapModes.Owner}
            };

        }


        public List<Prov> Provs
        {
            get { return _provs; }
            set
            {
                _provs = value;
                RaisePropertyChanged("Provs");
            }
        }

        public List<Country> Countries
        {
            get { return _countries; }
            set
            {
                _countries = value;
                RaisePropertyChanged("Countries");
            }
        }

        public BitmapSource Source
        {
            get { return _source; }
            set
            {
                _source = value;
                RaisePropertyChanged("Source");
            }
        }

        public BitmapSource Source1
        {
            get { return _source1; }
            set
            {
                _source1 = value;
                RaisePropertyChanged("Source1");
            }
        }

        public double Percent
        {
            get { return _percent; }
            set
            {
                _percent = value;
                RaisePropertyChanged("Percent");
            }
        }

        public string Write
        {
            get { return _write; }
            set
            {
                _write = value;
                RaisePropertyChanged("Write");
            }
        }

        public Prov SelectedProv
        {
            get { return _selectedProv; }
            set
            {
                _selectedProv = value;
                RaisePropertyChanged("SelectedProv");
            }
        }

        public List<Prov> Root
        {
            get { return _root; }
            set
            {
                _root = value;
                RaisePropertyChanged("Root");
            }
        }

        public FileProvider FileProvider { get; set; }

        public List<MapModeObject> MapModeObjects
        {
            get { return _mapModeObjects; }
            set
            {
                _mapModeObjects = value;
                RaisePropertyChanged("MapModeObjects");
            }
        }

        public MapModeObject SelectModeObject
        {
            get { return _selectModeObject; }
            set
            {
                _selectModeObject = value;
                RaisePropertyChanged("SelectModeObject");
            }
        }

        public ILog log = log4net.LogManager.GetLogger("Main");
    }

    public enum MapModes
    {
        All,
        Owner,
    }

    public class MapModeObject
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public MapModes MapMode { get; set; }
    }
}