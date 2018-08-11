using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4Editor
{
    public enum GameModes
    {
        None,
        Vic2,
        Eu4,
        CK2,
        HOI3,
        Roma,
        Sengoku,
        MotE,
        HOI4,
    }

    public class FileProvider
    {
        public GameModes Mode { get; set; }

        public string BaseUrl { get; set; }

        public FileProvider(string url)
        {
            //var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            //var parent = Directory.GetParent(baseDir.Trim('\\'));

            var path = url;// parent.FullName;

            BaseUrl = url;

            if (Directory.GetFiles(path, "victoria2.exe").Length == 1)
            {
                Mode= GameModes.Vic2;
            }

            else if (Directory.GetFiles(path, "eu4.exe").Length == 1)
            {
                Mode=GameModes.Eu4;
            }

            else if (Directory.GetFiles(path, "CK2game.exe").Length == 1)
            {
                //Mode = GameModes.CK2;
            }

            else if (Directory.GetFiles(path, "HOI3.exe").Length == 1)
            {
                Mode = GameModes.HOI3;
            }
            else if (Directory.GetFiles(path, "Rome.exe").Length == 1)
            {
                Mode = GameModes.Roma;
            }
            else if (Directory.GetFiles(path, "Sengoku.exe").Length == 1)
            {
                //Mode = GameModes.Sengoku;
            }
            else if (Directory.GetFiles(path, "mote.exe").Length == 1)
            {
                Mode = GameModes.MotE;
            }
            else if (Directory.GetFiles(path, "HOI4.exe").Length == 1)
            {
                Mode = GameModes.HOI4;
            }
        }

        public string GetProvMap()
        {
            var str = BaseUrl + "\\Map\\provinces.bmp";
            return str;
        }

        public string GetDefinition()
        {
            var str = BaseUrl + "\\Map\\definition.csv";
            return str;
        }

        public string GetDefaultMap()
        {
            var str = BaseUrl + "\\Map\\default.map";
            return str;
        }

        public string GetCountriesDefineFile()
        {
            if (Mode == GameModes.Eu4 || Mode==GameModes.CK2)
            {
                var str = BaseUrl + "\\common\\country_tags\\00_countries.txt";
                return str;
            }
            else
            {
                var str = BaseUrl + "\\Common\\countries.txt";
                return str;
            }
            
        }

        string GetCommonPath()
        {
            var str = BaseUrl + "\\Common\\";
            return str;


        }

        public List<string> GetCountriesFiles()
        {
            if (Mode == GameModes.HOI3)
            {
                var dir0 = GetCommonPath() + "\\countries\\";
                var files0 = Directory.GetFiles(dir0);

                var dir1 = GetModOrOriFile(GetCommonPath()) + "\\countries\\";
                var files1 = Directory.GetFiles(dir1);

                var dict=new Dictionary<string,string>();
                foreach (var s1 in files1)
                {
                    var name = Path.GetFileNameWithoutExtension(s1);

                    if (!dict.ContainsKey(name))
                        dict.Add(name, s1);
                }
                foreach (var s0 in files0)
                {
                    var name = Path.GetFileNameWithoutExtension(s0);

                    if(!dict.ContainsKey(name))
                        dict.Add(name,s0);
                }

                return dict.Values.ToList();
            }
            else
            {
                var dir = GetCommonPath() + "\\countries\\";
                var files = Directory.GetFiles(dir);
                return files.ToList();
            }

            //return files.Select(GetModOrOriFile).ToList();
        }

        public string GetModOrOriFile(string str)
        {
            if (Mode == GameModes.HOI3)
            {
                var path = str.Replace(BaseUrl, "");

                var modFile = BaseUrl+ "\\tfh" + path;
                if (File.Exists(modFile) || Directory.Exists(modFile))
                    return modFile;
                else
                    return str;
            }
            else
            {
                return str;
            }
        }

        string GetProvsPath()
        {
            var str = BaseUrl + "\\history\\provinces";
            return str;
        }

        public List<string> GetProvsFiles()
        {
            if (Mode == GameModes.Eu4 || Mode==GameModes.Roma || Mode==GameModes.MotE || Mode==GameModes.CK2)
            {
                var dir = GetProvsPath();
                var files = Directory.GetFiles(dir).ToList();

                return files.Select(GetModOrOriFile).ToList();
            }
            else
            {
                //var dir = GetProvsPath();
                //var dirs = Directory.GetDirectories(dir);
                //var files = dirs.SelectMany(Directory.GetFiles).ToList();

                //return files.Select(GetModOrOriFile).ToList();


                if (Mode == GameModes.HOI3)
                {
                    var dir0 = GetProvsPath() ;
                    var dirs0 = Directory.GetDirectories(dir0);
                    var files0 = dirs0.SelectMany(Directory.GetFiles).ToList();

                    var dir1 = GetModOrOriFile(GetProvsPath());
                    var dirs1 = Directory.GetDirectories(dir1);
                    var files1 = dirs1.SelectMany(Directory.GetFiles).ToList();

                    var dict = new Dictionary<string, string>();
                    foreach (var s1 in files1)
                    {
                        var name = Path.GetFileNameWithoutExtension(s1);

                        if (!dict.ContainsKey(name))
                            dict.Add(name, s1);
                    }
                    foreach (var s0 in files0)
                    {
                        var name = Path.GetFileNameWithoutExtension(s0);

                        if (!dict.ContainsKey(name))
                            dict.Add(name, s0);
                    }

                    return dict.Values.ToList();
                }
                else
                {
                    var dir = GetProvsPath();
                    var dirs = Directory.GetDirectories(dir);
                    var files = dirs.SelectMany(Directory.GetFiles).ToList();

                    return files.Select(GetModOrOriFile).ToList();
                }
            }
        } 

        public bool IsRevert()
        {
            if (this.Mode == GameModes.Eu4 || this.Mode==GameModes.Sengoku || this.Mode== GameModes.Roma || this.Mode== GameModes.MotE || this.Mode== GameModes.CK2 || this.Mode==GameModes.HOI4)
                return false;

            return true;
        }
    }
}
