using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace AutoClicker
{
    /// <summary>
    /// Логика взаимодействия для OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsINotify OptionsModel { get; set; }

        public MainWindow WindowMain { get; set; }


        public OptionsWindow(MainWindow mainWindow, Options options)
        {
            InitializeComponent();

            WindowMain = mainWindow;
            OptionsModel = Options.ConvertToINotify(options);
            DataContext = OptionsModel;

        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        private void Integer_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

        }
    }
    public class Options
    {
        public bool RepeatForever { get; set; }
        public int RepeatCount { get; set; }
        public int DefaultDelay { get; set; }
        public Options() { }

        private void SetDefault()
        {
            RepeatForever = true;
            DefaultDelay = 300;
            RepeatCount = 1;
        }
        public void LoadSavedActions()
        {
            string fileName = "options.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        try
                        {
                            var options = JsonConvert.DeserializeObject<Options>(json);

                            RepeatCount = options.RepeatCount;
                            RepeatForever = options.RepeatForever;
                            DefaultDelay = options.DefaultDelay;
                            return;
                        }
                        catch (JsonReaderException)
                        {
                            Trace.WriteLine("The file contains invalid JSON data.");
                        }
                    }
                    else
                    {
                        Trace.WriteLine("The file is empty.");
                    }
                }
                else
                {
                    Trace.WriteLine("File not found.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"An error occurred while reading the file: {ex.Message}");
            }
            SetDefault();
        }
        public void SaveActionsToFile()
        {
            string fileName = "options.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            try
            {
                string json = JsonConvert.SerializeObject(this);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while writing data to the file: {ex.Message}");
            }
        }
        public static OptionsINotify ConvertToINotify(Options opt)
        {
            OptionsINotify res = new();
            res.RepeatForever = opt.RepeatForever;
            res.RepeatCount = opt.RepeatCount;
            res.DefaultDelay = opt.DefaultDelay;
            return res;
        }
        public static Options ConvertFromINotify(OptionsINotify opt)
        {
            Options res = new();
            res.RepeatForever = opt.RepeatForever;
            res.RepeatCount = opt.RepeatCount;
            res.DefaultDelay = opt.DefaultDelay;
            return res;
        }
    }

    public class OptionsINotify : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private bool _repeatForever = true;
        public bool RepeatForever
        {
            get { return _repeatForever; }
            set
            {
                if (_repeatForever != value)
                {
                    _repeatForever = value;
                    OnPropertyChanged(nameof(RepeatForever));
                }
            }
        }
        public int _repeatCount = 1;
        public int RepeatCount
        {
            get { return _repeatCount; }
            set
            {
                if (_repeatCount != value)
                {
                    _repeatCount = value;
                    OnPropertyChanged(nameof(RepeatCount));
                }
            }
        }
        public int _defaultDelay = 300;
        public int DefaultDelay
        {
            get { return _defaultDelay; }
            set
            {
                if (_defaultDelay != value)
                {
                    _defaultDelay = value;
                    OnPropertyChanged(nameof(DefaultDelay));
                }
            }
        }
    }
}
