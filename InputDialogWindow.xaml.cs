using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace AutoClicker
{
    public partial class InputDialogWindow : Window
    {
        public MouseButtons actionStr;
        public int posXStr;
        public int posYStr;
        public int delayStr;
        public InputDialogWindow()
        {
            InitializeComponent();
        }
        private void Integer_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            actionStr = actionBox.SelectedIndex == 1 ? MouseButtons.Right : MouseButtons.Left;
            if (!int.TryParse(posX.Text, out posXStr))
            {
                posXStr = 0;
            }
            if (!int.TryParse(posY.Text, out posYStr))
            {
                posYStr = 0;
            }
            if (!int.TryParse(delay.Text, out delayStr))
            {
                delayStr = 0;
            }
            DialogResult = true;
        }
    }
}
