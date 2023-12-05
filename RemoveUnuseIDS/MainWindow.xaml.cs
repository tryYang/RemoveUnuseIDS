﻿using Microsoft.Win32;
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

namespace RemoveUnuseIDS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        //string pattern = @"<string\s+name=""([^""]+)"""; // 匹配 name 属性的正则表达式
        string Pattern = string.Empty; // 匹配 name 属性的正则表达式

        private List<string> SrcStrings = new List<string>();
        HashSet<string> prefixandsuffixSet = new HashSet<string>();
        HashSet<string> filterFilesSet = new HashSet<string>();
        HashSet<string> filterExtensioNameSet = new HashSet<string>();
        HashSet<string> filterRegexSet = new HashSet<string>();
        public List<string> filterRegexs = new List<string> { };
        public List<string> srcfilenames = new List<string> { };
        public event PropertyChangedEventHandler PropertyChanged;
        private List<string> filterExtensioName = new List<string>();
        string[] DeleteStringDir = Array.Empty<string>(); // 创建一个空的字符串数组

        
        private List<string> filterFiles = new List<string>();
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string filtertext = @"<string\s+name=""([^""]+)""";
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
                isuseregex = value;
                ShowFilterStrings();
                OnPropertyChanged(nameof(IsUseRegex));
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
                OnPropertyChanged(nameof(UnuseStrings));
            }
        }
        private List<List<string>> prefixandsuffix = new List<List<string>>();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            OnPropertyChanged(nameof(FliterStrings));
            OnPropertyChanged(nameof(FindStringsDemo));



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
                findStringsdemo.Add($"添加前缀:  {prefix.Text}  ,后缀:  {suffix.Text}   Demo:       " + prefix.Text + fliterStrings[0] + suffix.Text);
            }
            prefix.Text = string.Empty;
            suffix.Text = string.Empty;
        }

        private void Deleteprefixandsuffix_Click(object sender, RoutedEventArgs e)
        {
            findStringsdemo.Clear();
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
                    HashSet<string> UnuseKeys = FliterStrings.ToHashSet();
                    RecursivelySearchFiles(folderPath, UnuseKeys);
                    UnuseStrings = new ObservableCollection<string>(UnuseKeys);
                    Unusenum = UnuseStrings.Count.ToString();
                    Status1= "√";

                }
            }
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
                findStringsdemo.Add($"过滤包含{filterFile.Text}的文件");
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

                // 读取文件的所有行
                string[] lines = File.ReadAllLines(filePath);

                // 打开临时文件以便写入
                using (StreamWriter writer = new StreamWriter(tempFilePath))
                {
                    foreach (string line in lines)
                    {
                        // 检查当前行是否包含要删除的内容
                        if (!stringsToRemove.Any(s => line.Contains('"' + s + '"')))
                        {
                            // 将不包含要删除内容的行写入新文件
                            writer.WriteLine(line);
                        }
                    }
                }
                if (FuGai.IsChecked == true) {
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

        void RecursivelySearchFiles(string directory, HashSet<string> UnuseKeys)
        {
            try
            {

                string prefix = "<!--";
                string suffix = "-->";
                // 构建正则表达式
                string pattern_conment1 = $@"^\s*{Regex.Escape(prefix)}.*{Regex.Escape(suffix)}\s*$";
                string pattern_conment2 = @"^\s*//";
                foreach (string file in Directory.GetFiles(directory))
                {
                    //过滤文件
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
                                    findStringsdemo.Add($"Found '{names}' in line: {line}");

                                }
                            }
                        }
                    }
                }
                // 递归遍历子文件夹
                foreach (string subDir in Directory.GetDirectories(directory))
                {
                    RecursivelySearchFiles(subDir, UnuseKeys);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private bool IsFilterFile(string file)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
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
                foreach(var item in DeleteStringDir)
                {
                    FindStringsDemo.Add("选择删除的文件:"+item);
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
            findStringsdemo.Add($"过滤后缀名为{'.' + extensioname.Text}的文件");
            extensioname.Clear();


        }

        private void AddfilterRegex_Click(object sender, RoutedEventArgs e)
        {
            if (filterRegexSet.Contains(filterRegex.Text))
            {
                MessageBox.Show("已经过滤该后缀");
                return;
            }
            filterRegexs.Add(filterRegex.Text);
            filterRegexSet.Add(filterRegex.Text);
            findStringsdemo.Add($"过滤与正则表达式‘filterRegex.Text’匹配的行");

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
                if (Regex.IsMatch(regex, line))
                {
                    return true;
                }
            }
            return false;
        }

        private void GenenrateFile_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteStringDir.Count() <= 0)
            {
                MessageBox.Show("先选择文件");
                return;
            }
            foreach (string fileName in DeleteStringDir)
            {
                List<string> unuseStrings = new List<string>(UnuseStrings);

                RemoveLinesFromFile(fileName, unuseStrings);
            }
            if (FuGai.IsChecked==false)
            {

                MessageBox.Show("文件生成成功");
            }
            else
            {
                MessageBox.Show("成功覆盖原文件");
            }
        }
    }
}
