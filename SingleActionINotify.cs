using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AutoClicker
{
    public class SingleActionINotify : INotifyPropertyChanged
    {
        public SingleActionINotify() { }
        public SingleActionINotify(MouseButtons mouseButton, Point mousePosition, int delay)
        {
            MouseButton = mouseButton;
            MousePosition = mousePosition;
            Delay = delay;
        }
        private MouseButtons _mouseButton;
        public MouseButtons MouseButton
        {
            get { return _mouseButton; }
            set
            {
                if (_mouseButton != value)
                {
                    _mouseButton = value;
                    OnPropertyChanged(nameof(MouseButton));
                    OnPropertyChanged(nameof(MouseButtonStr));
                }
            }
        }

        private Point _mousePosition;
        public Point MousePosition
        {
            get { return _mousePosition; }
            set
            {
                if (_mousePosition != value)
                {
                    _mousePosition = value;
                    OnPropertyChanged(nameof(MousePosition));
                    OnPropertyChanged(nameof(MousePositionStr));
                }
            }
        }

        private int _delay;
        public int Delay
        {
            get { return _delay; }
            set
            {
                if (_delay != value)
                {
                    _delay = value;
                    OnPropertyChanged(nameof(Delay));
                    OnPropertyChanged(nameof(DelayStr));
                }
            }
        }

        private int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }

        public string MouseButtonStr => MouseButton.ToString();
        public string MousePositionStr => $"{MousePosition.X} : {MousePosition.Y}";
        public string DelayStr => Delay.ToString();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable]
    public class Macros
    {
        public string Name { get; set; }
        public List<SingleAction> ActionsList { get; set; }

        public Macros(List<SingleAction> list, string name)
        {
            Name = name;
            ActionsList = new(list);
        }
        public Macros() { }

        public static List<SingleActionINotify> ConvertToINotify(List<SingleAction> list)
        {
            List<SingleActionINotify> result = new();
            foreach (var a in list)
            {
                var sain = new SingleActionINotify(a.MouseButton, a.MousePosition, a.Delay);
                result.Add(sain);
            }
            return result;
        }
        public static List<SingleAction> ConvertFromINotify(List<SingleActionINotify> list)
        {
            List<SingleAction> result = new();
            foreach (var a in list)
            {
                var sain = new SingleAction(a.MouseButton, a.MousePosition, a.Delay);
                result.Add(sain);
            }
            return result;
        }
    }

    public class SingleAction
    {
        public SingleAction() { }
        public SingleAction(MouseButtons mouseButton, Point mousePosition, int delay)
        {
            MouseButton = mouseButton;
            MousePosition = mousePosition;
            Delay = delay;
        }

        public MouseButtons MouseButton { get; set; }
        public Point MousePosition { get; set; }


        public int Delay { get; set; }
        public int Index { get; set; }

        public SingleActionINotify ConvertToClickAction()
        {
            return new SingleActionINotify(MouseButton, MousePosition, Delay);
        }
    }
}