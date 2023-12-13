using AutoItX3Lib;
using Gma.System.MouseKeyHook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace AutoClicker
{
    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents m_GlobalHook;
        private List<MouseEventExtArgs> mouseEventExtArgs;
        private AutoItX3 au3;
        static CancellationTokenSource cancellationTokenSource;
        bool isRunning = false;
        bool isRecording = false;

        public ObservableCollection<Macros> SavedActions { get; set; }
        public Options SavedOptions { get; set; }
        private RecordingMarkerWindow markerWindow;
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = true;
            SavedOptions = new Options();
            SavedOptions.LoadSavedActions();

            au3 = new AutoItX3();

            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyDownTxt += M_GlobalHook_KeyDownTxt;

            LoadSavedActions();
        }
        private void LoadSavedActions()
        {
            string fileName = "savedActions.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            SavedActions = new();

            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        try
                        {
                            var deserializedPerson = JsonConvert.DeserializeObject<ObservableCollection<Macros>>(json);
                            Trace.WriteLine($"{deserializedPerson.Count}");
                            SavedActions = new ObservableCollection<Macros>(deserializedPerson);
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
                savedActionsBox.ItemsSource = SavedActions;
                savedActionsBox.DisplayMemberPath = "Name";
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"An error occurred while reading the file: {ex.Message}");
            }
        }

        private void SaveActionsToFile()
        {
            string fileName = "savedActions.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);


            try
            {
                string json = JsonConvert.SerializeObject(SavedActions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while writing data to the file: {ex.Message}");
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            SaveActionsToFile();
            SavedOptions.SaveActionsToFile();
        }

        private async void M_GlobalHook_KeyDownTxt(object? sender, KeyDownTxtEventArgs e)
        {
            if (e.KeyEvent.KeyCode == Keys.F3)
            {
                if (isRunning) return;
                if (isRecording)
                {
                    StopRecording();
                }
                else
                {
                    StartRecording();
                }
            }
            else if (e.KeyEvent.KeyCode == Keys.F2)
            {
                if (isRecording) return;
                if (isRunning)
                {
                    StopMacro();
                }
                else
                {
                    await StartMacro();
                }
            }
        }

        private void StopMacro()
        {
            isRunning = false;
            Visibility = Visibility.Visible;
            markerWindow.Close();
            StopLongRunningTask();
        }

        private async Task StartMacro()
        {
            if (savedActionsBox.SelectedItem == null)
            {
                ApplyErrorStyle();
                return;
            }
            isRunning = true;
            Visibility = Visibility.Hidden;
            markerWindow = new RecordingMarkerWindow(false);
            markerWindow.Show();
            int index = savedActionsBox.SelectedIndex;
            await StartActionTask(index);
        }

        private void StopRecording()
        {
            isRecording = false;
            Visibility = Visibility.Visible;
            Trace.WriteLine($"STOP RECORDING...");
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            markerWindow.Close();

            m_GlobalHook.KeyDownTxt -= M_GlobalHook_KeyDownTxt;

            ActionControlWindow actionControlWindow = new ActionControlWindow();

            actionControlWindow.SetClickActions(ConvertMouseArgs());
            actionControlWindow.Owner = this;
            actionControlWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (actionControlWindow.ShowDialog() == true)
            {
                SavedActions.Add(actionControlWindow.newMacros);
                savedActionsBox.SelectedIndex = savedActionsBox.Items.Count - 1;
            }
            m_GlobalHook.KeyDownTxt += M_GlobalHook_KeyDownTxt;
        }
        private void StartRecording()
        {
            isRecording = true;
            Visibility = Visibility.Hidden;
            Trace.WriteLine($"RECORDING START...");
            markerWindow = new RecordingMarkerWindow(true);
            markerWindow.Show();
            mouseEventExtArgs = new List<MouseEventExtArgs>();
            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
        }

        static void StopLongRunningTask()
        {
            cancellationTokenSource.Cancel();
        }
        private async Task StartActionTask(int index)
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            await Task.Run(() =>
            {
                Macros action = SavedActions[index];

                int actionsCount = action.ActionsList.Count;

                int counter = SavedOptions.RepeatCount;
                bool isRunning = true;

                while (isRunning)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    for (int a = 0; a < actionsCount; a++)
                    {

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        string mButton = action.ActionsList[a].MouseButton.ToString();
                        int posX = action.ActionsList[a].MousePosition.X;
                        int posY = action.ActionsList[a].MousePosition.Y;
                        au3.MouseClick(mButton, posX, posY);

                        Task.Delay(action.ActionsList[a].Delay).Wait();
                    }

                    if (!SavedOptions.RepeatForever)
                    {
                        counter--;
                        if (counter == 0)
                        {
                            isRunning = false;
                        }
                    }
                    Task.Delay(SavedOptions.DefaultDelay).Wait();

                }
            }, cancellationToken);

            isRunning = false;
            markerWindow?.Close();
            Visibility = Visibility.Visible;
        }

        private List<SingleActionINotify> ConvertMouseArgs()
        {
            List<SingleActionINotify> actions = new();

            for (int i = 0; i < mouseEventExtArgs.Count; i++)
            {
                int d;
                if (i == mouseEventExtArgs.Count - 1)
                    d = 0;
                else
                    d = mouseEventExtArgs[i + 1].Timestamp - mouseEventExtArgs[i].Timestamp;

                SingleActionINotify action = new(mouseEventExtArgs[i].Button, mouseEventExtArgs[i].Location, d);
                actions.Add(action);
            }
            return actions;
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Trace.WriteLine($"MouseDown: \t{e.Button}; \t System Timestamp: \t{e.Location}");

            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
            mouseEventExtArgs.Add(e);
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow optionsWindow = new OptionsWindow(this, SavedOptions);
            optionsWindow.Owner = this;
            optionsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (optionsWindow.ShowDialog() == true)
            {
                SavedOptions = Options.ConvertFromINotify(optionsWindow.OptionsModel);
            }

        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            bool check = savedActionsBox.SelectedItem == null;
            if (check) return;

            SavedActions.RemoveAt(savedActionsBox.SelectedIndex);
            if (SavedActions.Count > 0)
                savedActionsBox.SelectedIndex = 0;
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            bool check = savedActionsBox.SelectedItem == null;
            if (check) return;

            int index = savedActionsBox.SelectedIndex;
            ActionControlWindow actionControlWindow = new ActionControlWindow();

            List<SingleAction> act = SavedActions[index].ActionsList;

            actionControlWindow.SetClickActions(Macros.ConvertToINotify(act), index);
            actionControlWindow.Owner = this;
            actionControlWindow.Topmost = true;
            actionControlWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (actionControlWindow.ShowDialog() == true)
            {
                SavedActions[index].ActionsList = Macros.ConvertFromINotify(actionControlWindow.clickActions);
            }
        }
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await StartMacro();
        }
        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            StartRecording();
        }

        private void ApplyErrorStyle()
        {
            Brush defaultBorderBrush = savedActionsBoxBorder.BorderBrush;
            Thickness defaultThickness = savedActionsBoxBorder.BorderThickness;

            savedActionsBoxBorder.BorderBrush = Brushes.Red;
            savedActionsBoxBorder.BorderThickness = new Thickness(1.6);

            var timer = new System.Timers.Timer(200);
            timer.Elapsed += (source, args) =>
            {
                savedActionsBoxBorder.Dispatcher.Invoke(() =>
                {
                    savedActionsBoxBorder.BorderBrush = defaultBorderBrush;
                    savedActionsBoxBorder.BorderThickness = defaultThickness;

                    savedActionsBox.IsDropDownOpen = true;
                });
                timer.Stop();
            };
            timer.AutoReset = false;
            timer.Start();
        }
    }
}
