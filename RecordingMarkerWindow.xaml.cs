using System.Windows;

namespace AutoClicker
{
    public partial class RecordingMarkerWindow : Window
    {
        public RecordingMarkerWindow(bool isRec)
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = 0;
            Topmost = true;
            if (isRec)
            {
                label.Text = "RECORDING \n Press F3 to stop";
            }
            else
            {
                label.Text = "RUNNING \n Press F2 to stop";
            }
        }
    }
}
