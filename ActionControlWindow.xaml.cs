using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoClicker
{
    public partial class ActionControlWindow : Window
    {
        public List<SingleActionINotify> clickActions = new();
        public Macros newMacros;
        private int index = -1;
        public ActionControlWindow()
        {
            InitializeComponent();
            Topmost = true;

            ActionsListView.MouseDoubleClick += ItemDouble_Click;
        }
        public void SetClickActions(List<SingleActionINotify> actions)
        {
            clickActions = new(actions);

            for (int i = 0; i < clickActions.Count; i++)
            {
                clickActions[i].Index = i + 1;
            }
            ActionsListView.ItemsSource = clickActions;
        }
        public void SetClickActions(List<SingleActionINotify> actions, int index)
        {
            clickActions = new(actions);
            this.index = index;
            for (int i = 0; i < clickActions.Count; i++)
            {
                clickActions[i].Index = i + 1;
            }
            ActionsListView.ItemsSource = clickActions;
        }
        private void MoveItem(bool up)
        {
            SingleActionINotify? selectedAction = ActionsListView.SelectedItem as SingleActionINotify;
            if (selectedAction == null) return;

            int index = ActionsListView.SelectedIndex;

            if (up && ActionsListView.SelectedIndex != 0)
            {
                (clickActions[index], clickActions[index - 1]) = (clickActions[index - 1], clickActions[index]);
            }
            else if (!up && ActionsListView.SelectedIndex != clickActions.Count - 1)
            {
                (clickActions[index], clickActions[index + 1]) = (clickActions[index + 1], clickActions[index]);
            }
            SetClickActions(clickActions);
        }
        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveItem(true);
        }
        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveItem(false);
        }
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            SingleActionINotify? selectedAction = ActionsListView.SelectedItem as SingleActionINotify;
            if (selectedAction == null) return;

            int index = ActionsListView.SelectedIndex;
            clickActions.RemoveAt(index);
            if (index != 0)
                ActionsListView.SelectedIndex = index - 1;
            SetClickActions(clickActions);
        }
        private void ItemDouble_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (sender is ListView listView)
                {
                    if (listView.SelectedItem is SingleActionINotify)
                    {
                        SingleActionINotify action = clickActions[listView.SelectedIndex];
                        InputDialogWindow inputDialog = new InputDialogWindow();
                        inputDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        inputDialog.Owner = this;

                        inputDialog.posX.Text = action.MousePosition.X.ToString();
                        inputDialog.posY.Text = action.MousePosition.Y.ToString();
                        inputDialog.delay.Text = action.DelayStr;

                        inputDialog.actionBox.SelectedIndex = action.MouseButton == System.Windows.Forms.MouseButtons.Left ? 0 : 1;


                        if (inputDialog.ShowDialog() == true)
                        {
                            Trace.WriteLine($"{inputDialog.actionStr}, {inputDialog.posXStr}, {inputDialog.posYStr}, {inputDialog.delayStr}");


                            action.MouseButton = inputDialog.actionStr;
                            action.MousePosition = new System.Drawing.Point(inputDialog.posXStr, inputDialog.posYStr);
                            action.Delay = inputDialog.delayStr;
                        }
                    }
                }
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (index == -1)
            {
                var inputDialog = new InputDialog();
                inputDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                inputDialog.Owner = this;

                if (inputDialog.ShowDialog() == true)
                {
                    string userInput = inputDialog.EnteredText;
                    newMacros = new(Macros.ConvertFromINotify(clickActions), userInput);
                }
                else
                {
                    return;
                }
            }
            DialogResult = true;
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
