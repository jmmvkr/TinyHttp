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
using System.Threading;

namespace TinyHttp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, jvk.lib.ILog
    {
        private jvk.lib.TinyHttp ht = new jvk.lib.TinyHttp();
        private Action<Control,bool> actEnable;

        public MainWindow()
        {
            InitializeComponent();
            actEnable = myBtnEnable;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ht.Service = new jvk.lib.EchoService();
            ht.OnServerStart += Ht_OnServerStart;

            new Thread(tcpProc).Start();
        }

        private void myBtnEnable(Control c, bool b)
        {
            c.IsEnabled = b;
        }

        private void Ht_OnServerStart(object sender, EventArgs e)
        {
            Dispatcher.Invoke(actEnable, btnBrowse, true);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ht.stop(this);
        }

        private void tcpProc()
        {
            ht.runServerProc(this);
        }

        public void log(string msg)
        {
            Console.WriteLine(msg);
        }

        public void log(string msg, Exception ex)
        {
            Console.WriteLine(msg);
            Console.WriteLine(ex.GetType());
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            string uri = String.Format("http://127.0.0.1:{0}", ht.Port);
            System.Diagnostics.Process.Start(uri);
        }

    } // end - class MainWindow
}
