using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace MIMS.Mini
{
    /// <summary>
    /// ImageListView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageListView : UserControl
    {
        private readonly App _app;
        public ImageListViewModel IV { get; private set; }
        public ImageListView()
        {
            InitializeComponent();

            _app = Application.Current as App;

            this.IV = new ImageListViewModel(_app.Engine);
            IV.OwnerView = this;
            this.DataContext = this.IV;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            IV.Load();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IV.Unload();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IV.ShowImageViewer();
            e.Handled = true;
        }
    }
}
