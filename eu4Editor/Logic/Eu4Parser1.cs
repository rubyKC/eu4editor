using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ReadEu4Config
{
    public class Config1
    {
        public List<YearPoint> Years { get; set; }
        public string ConfigFileName { get; set; }

        //public string Remark { get; set; }

        public Config1()
        {
            this.Years=new List<YearPoint>();

            this.Years.Add(new YearPoint());
        }

        public static Config1 Parse(Config config, string fileName)
        {
            var config1=new Config1(); 
            config1.ConfigFileName = fileName;

            ////////////////////////////////////////////
            foreach (var item in config.Items)
            {
                if (item is JProperty)
                {
                    //var item=jp
                }
            }

            ////////////////////////////////////////////
            return config1;
        }
    }

    public class YearPoint
    {
        public string Name { get; set; }
        public int Year { get; set; }
        public Dictionary<string, string> Values { get; set; }

        public YearPoint()
        {
            this.Year = -1;
            this.Values = new Dictionary<string, string>();
        }
    }


    
    
}
