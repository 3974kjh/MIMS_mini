using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows; // TextWrapping
using System.Windows.Controls; // ListBox
using System.Windows.Media; // SolidColorBrush
using System.Windows.Threading; // DispatcherPriority

namespace MIMS.Mini.Foundation
{
    public class LogListBox : SimpleLogger
    {
        protected static volatile ListBox _listBox;
        protected readonly int MAX_LOG_COUNT;

        public LogListBox(int maxLogCount = 5000)
        {
            MAX_LOG_COUNT = maxLogCount;

            _listBox = new ListBox();
        }

        public LogListBox(ListBox listBox, int maxLogCount = 5000)
        {
            _listBox = listBox;
            MAX_LOG_COUNT = maxLogCount;
        }

        ~LogListBox()
        {
            _listBox = null;
        }

        public ListBox ListBox { get { return _listBox; } }

        public void SetLogListBox(ListBox listBox)
        {
            _listBox = listBox;
        }

        public override void _OutputMsg(SimpleLogger.LOG_LEVEL level, string msg)
        {
            base._OutputMsg(level, msg);

            switch (level)
            {
                case LOG_LEVEL.DEBUG:
                    {
                        _listBox.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                        new Action(delegate () { AddItem("DEBUG", msg, Brushes.LightGray); }));
                    }
                    break;
                case LOG_LEVEL.INFO:
                    {
                        _listBox.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                         new Action(delegate () { AddItem("INFO", msg, Brushes.LightBlue); }));
                    }
                    break;
                case LOG_LEVEL.WARN:
                    {
                        _listBox.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                       new Action(delegate () { AddItem("WARN", msg, Brushes.MediumVioletRed); }));
                    }
                    break;
                case LOG_LEVEL.ERROR:
                    {
                        _listBox.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                        new Action(delegate () { AddItem("Error", msg, Brushes.OrangeRed); }));
                    }
                    break;
                case LOG_LEVEL.FATAL:
                    {
                        _listBox.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                            new Action(delegate () { AddItem("FATAL", msg, Brushes.Red); }));
                    }
                    break;
            }
        }

        private void AddItem(string level, string msg, SolidColorBrush brush)
        {
            if (_listBox.Items.Count >= MAX_LOG_COUNT)
            {
                for (int i = MAX_LOG_COUNT / 3; i >= 0; i--)
                {
                    _listBox.Items.RemoveAt(i);
                }
            }

            string timeMark = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ";

            Label lb = new Label();

            lb.Content = new TextBlock
            {
                Text = "[" + level + " : " + timeMark + "] " + msg,
                TextWrapping = TextWrapping.Wrap
            };

            lb.Foreground = brush;

            _listBox.Items.Add(lb);

            _listBox.SelectedIndex = _listBox.Items.Count - 1;
            _listBox.ScrollIntoView(lb);
        }
    }
}
