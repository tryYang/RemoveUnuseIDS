using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Path = System.IO.Path;

namespace RemoveUnuseIDS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        //string pattern = @"<string\s+name=""([^""]+)"""; // 匹配 name 属性的正则表达式
        string Pattern = string.Empty; // 匹配 name 属性的正则表达式
        long   fileCount =0;
        long  Proessfilecount=0;

        private int processbarvalue =0;
        public int ProcessBarValue
        {

            get { return processbarvalue; }
            set
            {
                processbarvalue = value;
                
                OnPropertyChanged(nameof(ProcessBarValue));
            }
        }
        private string processfilename = string.Empty;
        public string ProcessFileName
        {
            get { return processfilename; }
            set
            {
                processfilename = value;

                OnPropertyChanged(nameof(ProcessFileName));
            }
        }
        private List<string> SrcStrings = new List<string>();
        HashSet<string> prefixandsuffixSet = new HashSet<string>();
        HashSet<string> DprefixandDsuffixSet = new HashSet<string>();
        HashSet<string> filterFilesSet = new HashSet<string>();
        HashSet<string> filterExtensioNameSet = new HashSet<string>();
        HashSet<string> filterRegexSet = new HashSet<string>();
        public List<string> filterRegexs = new List<string> { };
        public List<string> srcfilenames = new List<string> { };
        public event PropertyChangedEventHandler PropertyChanged;
        private List<string> filterExtensioName = new List<string>();
        string[] DeleteStringDir = Array.Empty<string>(); // 创建一个空的字符串数组

        HashSet<string> UnuseKeys=new HashSet<string>();
        private List<string> filterFiles = new List<string>();
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //private string filtertext = @"<string\s+name=""([^""]+)""";
        private string filtertext = string.Empty;
        public bool isAuto = false;
        public string FilterText
        {

            get { return filtertext; }
            set
            {
                filtertext = value;
                OnPropertyChanged(nameof(FilterText));
            }
        }
        private string unusenum = "0";
        public string Unusenum
        {

            get { return unusenum; }
            set
            {
                unusenum = value;
                OnPropertyChanged(nameof(Unusenum));
            }
        }

        private bool isuseregex = true;
        public bool IsUseRegex
        {

            get { return isuseregex; }
            set
            {
                if (isuseregex != value) { 
                    isuseregex = value;
                    ShowFilterStrings();
                    OnPropertyChanged(nameof(IsUseRegex));
                }
            }
        }
        private string status1 = "×";
        public string Status1
        {

            get { return status1; }
            set
            {
                status1 = value;
                OnPropertyChanged(nameof(Status1));
            }
        }
        private string status2 = "×";
        public string Status2
        {

            get { return status2; }
            set
            {
                status2 = value;
                OnPropertyChanged(nameof(Status2));
            }
        }
        private string filternum = "0";
        public string Filternum
        {

            get { return filternum; }
            set
            {
                filternum = value;
                OnPropertyChanged(nameof(Filternum));
            }
        }

        private ObservableCollection<string> fliterStrings = new ObservableCollection<string>();
        public ObservableCollection<string> FliterStrings
        {
            get { return fliterStrings; }
            set
            {
                fliterStrings = value;
                OnPropertyChanged(nameof(FliterStrings));
            }
        }
        private ObservableCollection<string> findStringsdemo = new ObservableCollection<string>();
        public ObservableCollection<string> FindStringsDemo
        {
            get { return findStringsdemo; }
            set
            {
                findStringsdemo = value;
                OnPropertyChanged(nameof(FindStringsDemo));
            }
        }
        private ObservableCollection<string> unuseStrings = new ObservableCollection<string>();
        public ObservableCollection<string> UnuseStrings
        {
            get { return unuseStrings; }
            set
            {
                unuseStrings = value;
                if (value.Count > 0)
                {
                    Status1 = "√";
                }
                else
                {
                    Status1 = "×";

                }
                OnPropertyChanged(nameof(UnuseStrings));
            }
        }
        private List<List<string>> prefixandsuffix = new List<List<string>>();
        private List<List<string>> DprefixandDsuffix = new List<List<string>>();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            string jsonFilePath = @"config.json";
            if (File.Exists(jsonFilePath))
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                JObject jsonObject = JObject.Parse(jsonContent);

                MyConfiguration myConfiguration = JsonConvert.DeserializeObject<MyConfiguration>(jsonContent);
                isAuto = myConfiguration.AutoMode;
                if (isAuto)
                {
                    string jsonfiledir = System.IO.Path.Combine("JsonFile", myConfiguration.ConfigFileName);
                    MyParams myParams =null;
                    if (File.Exists(jsonfiledir))
                    {
                        string Paramconfig = File.ReadAllText(jsonfiledir);
                        myParams = JsonConvert.DeserializeObject<MyParams>(Paramconfig);
                    }
                    else
                    {
                        ExitWithTip("请检查配置文件路径");                        
                    }
                    
                    srcfilenames = myParams.SrcStrings_Path;
                    DeleteStringDir = myParams.DeleteFilePath;
                    foreach (var dir in srcfilenames)
                    {
                        if (!File.Exists(dir))
                        {
                            ExitWithTip("SrcStrings_Path中有文件路径不存在");                            
                        }
                    }
                    foreach (var dir in DeleteStringDir)
                    {
                        if (!File.Exists(dir))
                        {
                            ExitWithTip("DeleteStringDir中有文件路径不存在");
                        }
                    }

                    IsUseRegex = myParams.UseRegex;
                    FilterText = myParams.Regex;
                    ShowFilterStrings();
                    //过滤添加

                    prefixandsuffix = myParams.prefixandsuffix;
                    DprefixandDsuffix = myParams.Delete_prefixandsuffix;
                    foreach (var prefixsuffix in prefixandsuffix)
                    {
                        prefixandsuffixSet.Add(prefixsuffix[0] + "check" + prefixsuffix[1]);
                    }
                    foreach (var prefixsuffix in DprefixandDsuffix)
                    {
                        DprefixandDsuffixSet.Add(prefixsuffix[0] + "check" + prefixsuffix[1]);
                    }
                    if (myParams.UseFilter)
                    {
                        filterExtensioName = myParams.ExtensionName_Filter;
                        filterExtensioNameSet = new HashSet<string>(filterExtensioName);
                        filterFiles = myParams.FileName_Filter;
                        filterFilesSet = new HashSet<string>(filterFiles);
                        filterRegexs = myParams.Regex_Filter;
                        filterRegexSet = new HashSet<string>(filterRegexs);
                    }
                    UnuseKeys = FliterStrings.ToHashSet();
                    Task task = Task.Run(() =>
                    {
                        
                        UnuseFileFinding(myParams.FindFileDir);                        
                        Status2 = "√";
                        GenenrateFiles();
                    });
                }
            }
            //bool AutoMode = (bool)jsonObject["AutoMode"];
            //Params _params= (Params)jsonObject["Params"];
            //if (AutoMode)
            //{
            //    Params _params = 
            //}



        }
        private void ExitWithTip(string message)
        {
            MessageBox.Show(message);
            Environment.Exit(0);
        }
        private void ChoseSrcFileClicked(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.InitialDirectory = "C:\\";
            openFileDialog.Filter = "所有文件|*.*|文本文件|*.txt";
            openFileDialog.Multiselect = true;
            bool? result = openFileDialog.ShowDialog(); // 显示文件选择器
            if (result == true)
            {
                srcfilenames.Clear();
                foreach (string fileName in openFileDialog.FileNames)
                {
                    srcfilenames.Add(fileName);
                }
            }
        }


        private void ShowFilterStrings_Click(object sender, RoutedEventArgs e)
        {

            ShowFilterStrings();
        }

        string ExtractNameValue(string input)
        {
            Match match = Regex.Match(input, filtertext);

            if (match.Success)
            {
                return match.Groups[1].Value; // 提取匹配到的值
            }
            else
            {
                return string.Empty; // 如果没有匹配到，返回一个默认值
            }
        }

        private void Addprefixandsuffix_Click(object sender, RoutedEventArgs e)
        {
            if (prefixandsuffixSet.Contains(prefix.Text + "check" + suffix.Text))
            {
                MessageBox.Show("已经添加过");
                return;
            }
            prefixandsuffixSet.Add(prefix.Text + "check" + suffix.Text);
            prefixandsuffix.Add(new List<string> { prefix.Text, suffix.Text });
            if (fliterStrings.Count > 0)
            {
                FindStringsDemo.Add($"步骤2：添加前缀:  {prefix.Text}  ,后缀:  {suffix.Text}   Demo:       " + prefix.Text + fliterStrings[0] + suffix.Text);
            }
            prefix.Text = string.Empty;
            suffix.Text = string.Empty;
        }

        private void Deleteprefixandsuffix_Click(object sender, RoutedEventArgs e)
        {
            prefixandsuffix.Clear();
            prefixandsuffixSet.Clear();
        }

        private void UnuseFileFind_Click(object sender, RoutedEventArgs e)
        {
            using (Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    
                    
                    UnuseStrings.Clear();
                    string folderPath = dialog.FileName;
                    Console.WriteLine("Selected folder: " + folderPath);
                    UnuseKeys = FliterStrings.ToHashSet();
                    //RecursivelySearchFiles(folderPath);
                    //UnuseStrings = new ObservableCollection<string>(UnuseKeys);
                    //Unusenum = UnuseStrings.Count.ToString();
                    Task.Run(() =>
                    {
                        UnuseFileFinding(folderPath);

                    });
                    //Status1 = "√";
                    //UnuseFileFinding(folderPath, UnuseKeys);
                    //Task.Run(() =>
                    //{
                    //    RecursivelySearchFiles(folderPath, UnuseKeys);

                    //});

                }
            }


        }
        private async void UnuseFileFinding( string folderPath)
        {
            
            // 在界面上启动任务并不阻塞 UI
                fileCount = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Length;
                RecursivelySearchFiles(folderPath);
                if (ProcessBarValue != 100)
                {
                    ProcessBarValue = 100;
                }
                Proessfilecount = 0;
                fileCount = 0;
                UnuseStrings = new ObservableCollection<string>(UnuseKeys);
                Unusenum = UnuseStrings.Count.ToString();
                Status1 = "√";
            
        }

        private void AddfilterFile_Click(object sender, RoutedEventArgs e)
        {
            if (filterFile.Text != string.Empty)
            {
                if (filterFilesSet.Contains(filterFile.Text))
                {
                    MessageBox.Show("已经过滤该文件名");
                    return;
                }
                filterFiles.Add(filterFile.Text);
                filterFilesSet.Add(filterFile.Text);
                FindStringsDemo.Add($"过滤包含{filterFile.Text}的文件");
                filterFile.Text = string.Empty;
            }
            else
                MessageBox.Show("不能为空");
        }
        private void RemoveLinesFromFile(string filePath, List<string> stringsToRemove)
        {
            try
            {
                //string tempFilePath = "GenenrateFile" + System.IO.Path.GetExtension(filePath); // 临时文件路径
                string tempFilePath = System.IO.Path.GetFileName(filePath); // 临时文件路径
                if (tempFilePath == "config.json")
                {
                    MessageBox.Show("config.json 与本配置文件名冲突");
                    return;
                }
                // 读取文件的所有行
                string[] lines = File.ReadAllLines(filePath);

                // 打开临时文件以便写入
                using (StreamWriter writer = new StreamWriter(tempFilePath))
                {
                    foreach (string line in lines)
                    {
                        bool iswrite = true;
                        //如果没有添加前后缀就按原来的做 ，添加后 按添加后的结果删除
                        if (DprefixandDsuffixSet.Count == 0)
                        {
                            if (stringsToRemove.Any(s => line.Contains(s)))
                            {
                                // 将不包含要删除内容的行写入新文件
                                iswrite = false;
                            }
                        }
                        else
                        {
                            foreach (var keyvalue in DprefixandDsuffix)
                            {
                                if (stringsToRemove.Any(s => line.Contains(keyvalue[0] + s + keyvalue[1])))
                                {
                                    // 将不包含要删除内容的行写入新文件
                                    iswrite = false;
                                }
                            }
                        }

                        if (iswrite)
                        {
                            writer.WriteLine(line);
                        }

                    }
                }
                if (FuGai.IsChecked == true)
                {
                    // 删除原始文件
                    File.Delete(filePath);
                    // 将临时文件重命名为原始文件名
                    File.Move(tempFilePath, filePath);
                }
                Console.WriteLine("New file created without specified lines.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        void RecursivelySearchFiles(string directory )
        {
            try
            {

                string prefix = "<!--";
                string suffix = "-->";
                // 构建正则表达式
                foreach (string file in Directory.GetFiles(directory))
                {
                    //过滤文件
                     Proessfilecount++;
                    if (IsFilterFile(file))
                    {
                        
                        continue;
                    }
                    List<string> Filelines = File.ReadLines(file).ToList();
                    foreach (var preandsur in prefixandsuffix)
                    {
                        int len_pre = preandsur[0].Length;
                        int len_suf = preandsur[1].Length;
                        foreach (var line in Filelines)
                        {
                            if (line == string.Empty)
                                continue;

                            if (IsFilterRegexMatch(line))  //通过正则表达式过滤行
                                continue;
                            HashSet<string> stringnames = UnuseKeys.Select(item => preandsur[0] + item + preandsur[1]).ToHashSet();
                            foreach (var names in stringnames)
                            {
                                if (line.Contains(names))
                                {
                                    stringnames.Remove(names);
                                    string srcname = names.Substring(len_pre, names.Length - len_pre - len_suf);
                                    if (UnuseKeys.Contains(srcname))
                                    {
                                        UnuseKeys.Remove(srcname);
                                    }
                                    this.Dispatcher.Invoke(delegate
                                    {
                                        //int  value = (int)((Proessfilecount * 100) / fileCount);
                                        double value = (((double)Proessfilecount  / fileCount)*100.0);
                                        int result = (int)value;
                                        if (ProcessBarValue != result)
                                        {
                                            ProcessBarValue = result;
                                        }
                                        FindStringsDemo.Add($"Found '{names}' in line:             {{{line.Trim()}}}");
                                        scrollviewer_tip.ScrollToEnd();
                                    }
                                   );

                                }
                            }
                        }
                    }
                }
                // 递归遍历子文件夹
                foreach (string subDir in Directory.GetDirectories(directory))
                {
                    RecursivelySearchFiles(subDir);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           
        }

        private bool IsFilterFile(string file)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
            ProcessFileName = System.IO.Path.GetFileName(file);
            string extensionName = System.IO.Path.GetExtension(file);
            return filterFilesSet.Contains(fileName) || filterExtensioNameSet.Contains(extensionName);
        }

        private void DeletestringFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.InitialDirectory = "C:\\";
            openFileDialog.Filter = "所有文件|*.*|文本文件|*.txt";
            openFileDialog.Multiselect = true;

            bool? result = openFileDialog.ShowDialog(); // 显示文件选择器

            if (result == true)
            {
                if (UnuseStrings == null)
                {
                    System.Windows.MessageBox.Show("无要删除的字符串");
                }
                DeleteStringDir = openFileDialog.FileNames;
               
                foreach (var item in DeleteStringDir)
                {
                    FindStringsDemo.Add("选择删除的文件路径为:" + item);
                }

                Status2 = "√";
            }
        }

        private void ClearTip_Click(object sender, RoutedEventArgs e)
        {
            FindStringsDemo.Clear();
        }

        private void ShowFilterStrings()
        {
            if (srcfilenames.Count <= 0)
            {
                MessageBox.Show("未选择文件");
                return;
            }
            fliterStrings.Clear();
            foreach (var file in srcfilenames)
            {
                SrcStrings = File.ReadAllLines(file).ToList();
                for (int i = 0; i < SrcStrings.Count; i++)
                {
                    string temp = SrcStrings[i];
                    if (IsUseRegex)
                        temp = ExtractNameValue(temp);
                    fliterStrings.Add(temp);
                }
            }
            FliterStrings = new ObservableCollection<string>(FliterStrings.Where(s => !string.IsNullOrWhiteSpace(s)));
            Filternum = FliterStrings.Count.ToString();

            if (FliterStrings.Count <= 0)
            {
                MessageBox.Show("未有匹配字符");
                return;
            }

        }

        private void DeletefilterFile_Click(object sender, RoutedEventArgs e)
        {
            filterFiles.Clear();
            filterFilesSet.Clear();
            MessageBox.Show("清除所有通过文件名的过滤");
        }

        private void DeletefilterExtensioName_Click(object sender, RoutedEventArgs e)
        {
            filterExtensioName.Clear();
            filterExtensioNameSet.Clear();
            MessageBox.Show("清除所有通过文件后缀名的过滤");
        }

        private void AddfilterExtensioName_Click(object sender, RoutedEventArgs e)
        {
            if (filterExtensioNameSet.Contains('.' + extensioname.Text))
            {
                MessageBox.Show("已经过滤该后缀");
                return;
            }
            filterExtensioName.Add('.' + extensioname.Text);
            filterExtensioNameSet.Add('.' + extensioname.Text);
            FindStringsDemo.Add($"过滤后缀名为{'.' + extensioname.Text}的文件");
            extensioname.Clear();


        }

        private void AddfilterRegex_Click(object sender, RoutedEventArgs e)
        {
            if (filterRegexSet.Contains(filterRegex.Text))
            {
                MessageBox.Show("已经过滤该正则表达式");
                return;
            }
            filterRegexs.Add(filterRegex.Text);
            filterRegexSet.Add(filterRegex.Text);

            FindStringsDemo.Add($"过滤与正则表达式‘{filterRegex.Text}’匹配的行");
            filterRegex.Clear();


        }

        private void DeletefilterRegex_Click(object sender, RoutedEventArgs e)
        {
            filterRegexs.Clear();
            filterRegexSet.Clear();
            MessageBox.Show("清除所有正则表达式");
        }
        private bool IsFilterRegexMatch(string line)
        {
            foreach (var regex in filterRegexs)
            {
                if (Regex.IsMatch(line, regex))
                {
                    return true;
                }
            }
            return false;
        }

        private void GenenrateFile_Click(object sender, RoutedEventArgs e)
        {
            GenenrateFiles();
        }
        private void GenenrateFiles()
        {
            if (DeleteStringDir.Count() <= 0)
            {
                MessageBox.Show("先选择文件");
                return;
            }
            if (UnuseStrings.Count == 0)
            {
                MessageBox.Show("没有废弃的字符串");
                return;
            }
            foreach (string fileName in DeleteStringDir)
            {
                List<string> unuseStrings = new List<string>(UnuseStrings);

                RemoveLinesFromFile(fileName, unuseStrings);
            }
            this.Dispatcher.Invoke(delegate
            {
                if (FuGai.IsChecked == false)
                {

                    MessageBox.Show("文件生成成功");
                }
                else
                {
                    MessageBox.Show("成功覆盖原文件");
                }
            });
           
        }
        private void AddDprefixandDsuffix_Click(object sender, RoutedEventArgs e)
        {
            if (DprefixandDsuffixSet.Contains(prefix.Text + "check" + suffix.Text))
            {
                MessageBox.Show("已经添加过");
                return;
            }
            DprefixandDsuffixSet.Add(prefix.Text + "check" + suffix.Text);
            DprefixandDsuffix.Add(new List<string> { D_prefix.Text, D_suffix.Text });
            if (UnuseStrings.Count > 0)
            {
                FindStringsDemo.Add($"步骤4：添加前缀:  {D_prefix.Text}  ,后缀:  {D_suffix.Text}   Demo:       " + D_prefix.Text + UnuseStrings[0] + D_suffix.Text);
            }
            D_prefix.Text = string.Empty;
            D_suffix.Text = string.Empty;
        }

        private void DeleteDprefixandDsuffix_Click(object sender, RoutedEventArgs e)
        {
            DprefixandDsuffixSet.Clear();
            DprefixandDsuffix.Clear();
        }

        private void ClearFIlterpara_Click(object sender, RoutedEventArgs e)
        {
            filterRegexs.Clear();
            filterRegexSet.Clear();
            filterFiles.Clear();
            filterFilesSet.Clear();
            filterExtensioName.Clear();
            filterExtensioNameSet.Clear();
            MessageBox.Show("清空成功");
        }


        static long CountTotalLines(string folderPath)
        {
            long totalLines = 0;

            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    int lines = File.ReadAllLines(file).Length;
                    totalLines += lines;
                }

                string[] subDirectories = Directory.GetDirectories(folderPath);
                foreach (string subDir in subDirectories)
                {
                    // 递归调用，统计子文件夹中的行数并累加到总行数中
                    totalLines += CountTotalLines(subDir);
                }
            }
            else
            {
                Console.WriteLine("Folder not found.");
            }

            return totalLines;
        }


    }
}
