using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveUnuseIDS
{
    class MyConfiguration
    {
        public bool AutoMode { get; set; }  
        public MyParams Params { get; set; }
    }
    class MyParams
    {
        public bool UseRegex { get; set; }
        public bool UseFilter { get; set; }
        public string Regex { get; set; }
        public string FindFileDir { get; set; }
        public List<string> SrcStrings_Path { get; set; }
        public List<List<string>> prefixandsuffix { get; set; }
        public List<string> FileName_Filter { get; set; }
        public List<string> ExtensionName_Filter { get; set; }
        public List<string> Regex_Filter { get; set; }
        public List<string> DeleteFilePath { get; set; }
        public List<List<string>> Delete_prefixandsuffix { get; set; }
    }

}
