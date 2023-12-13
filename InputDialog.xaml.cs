using System.Windows;
using System.Windows.Media;

namespace AutoClicker
{
    public partial class InputDialog : Window
    {
        public string EnteredText { get; private set; }
        public InputDialog()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                InputTextBox.BorderBrush = Brushes.Red;

                var timer = new System.Timers.Timer(200);
                timer.Elapsed += (source, args) =>
                {
                    InputTextBox.Dispatcher.Invoke(() =>
                    {
                        InputTextBox.BorderBrush = Brushes.Gainsboro;
                        InputTextBox.Focus();
                    });
                    timer.Stop();
                };
                timer.AutoReset = false;
                timer.Start();
            }
            else
            {
                EnteredText = InputTextBox.Text;
                DialogResult = true;
            }
        }
    }
}
