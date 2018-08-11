using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ReadEu4Config
{
    public interface INode
    {
        string Name { get; set; }
        string Remark { get; set; }
    }


    public class Config:JObject
    {

    }




    public class JObject:INode
    {
        public string Remark { get; set; }
        public string Name { get; set; }
        public List<INode> Items { get; set; }

        public JObject()
        {
            this.Items=new List<INode>();
        }
    }

    public class JProperty : INode
    {
        public string Remark { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

    }


  

    public class ParseEu
    {
        private static string reg0Str = "([\\w\\d\\.]+)" +
                                        "\\s*" +
                                        "=" +
                                        "\\s*" +
                                        "([\\w\\d]+)" +
                                        "\\s*(#[^\r]*)?";
        static Regex reg0=new Regex(reg0Str,RegexOptions.Multiline);
        static string reg1Str=
              "^([\\w\\d\\.]+)\\s*=\\s*"
                + "{"  //#普通字符“(”
                + "("                //#分组构造，用来限定量词“*”修饰范围
                + "(?<Open>\\{)"     //  #命名捕获组，遇到开括弧“Open”计数加1
                + "|"                //#分支结构
                + "(?<-Open>\\})"    //#狭义平衡组，遇到闭括弧“Open”计数减1
                + "|"                //#分支结构
                + "[^{}]+"           //#非括弧的其它任意字符
                + ")*"               //#以上子串出现0次或任意多次
                + "(?(Open)(?!))"    //#判断是否还有“Open”，有则说明不配对，什么都不匹配
                + "}"               //#普通闭括弧
                +"\\s*(#[^\r]*)?";  //注释
        static Regex reg1=new Regex(reg1Str,RegexOptions.Multiline,TimeSpan.FromSeconds(1));


        public static void Parse(string str,JObject root)
        {
            var match0 = reg1.Matches(str);
            var temp=new List<JObject>();
            foreach (Match match in match0)
            {
                var name = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                var remark = match.Groups[3].Value;

                ///////////////////////////////////
                var obj = new JObject {Name = name,Remark = remark};
                temp.Add(obj);

                Parse(value.Trim(),obj);
            }

            str = reg1.Replace(str, "");

            var match1 = reg0.Matches(str);

            foreach (Match match in match1)
            {
                var name = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                var remark = match.Groups[3].Value;

                root.Items.Add(new JProperty {Name = name,Value = value,Remark = remark});

            }


            root.Items.AddRange(temp);
        }


   

    }
}
