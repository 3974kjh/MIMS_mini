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
    /// PatientSearchView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PatientSearchView : UserControl
    {
        private readonly App _app;
        public PatientSearchViewModel PV { get; private set; }

        public PatientSearchView()
        {
            InitializeComponent();

            _app = Application.Current as App;
            
            this.PV = new PatientSearchViewModel(_app.Engine);
            this.DataContext = this.PV;
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PV.Load();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PV.Unload();
        }
        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PV.SearchedPatientInfoListCommand(null);
        }
    }
}
