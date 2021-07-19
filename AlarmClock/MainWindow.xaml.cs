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

// Final Project
// Nathan Fenske and Matthew Govia

namespace AlarmClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Model _model;
        public MainWindow()
        {
            InitializeComponent();
            _model = new Model();
            //set data binding context to our model
            this.DataContext = _model;

            

            
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _model.InitModel();
            H1.ItemsSource = _model.H1Collection;
            H0.ItemsSource = _model.H0Collection;
            M1.ItemsSource = _model.M1Collection;
            M0.ItemsSource = _model.M0Collection;
            S1.ItemsSource = _model.S1Collection;
            S0.ItemsSource = _model.S0Collection;
            _model.NETDispatchTimerStart(true);
            
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _model.StopTimer();
            _model.CleanUp();
        }
    }
}
