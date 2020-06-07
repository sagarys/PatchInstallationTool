using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Patch_Installation_tool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MessageBox.Show("Patch Validation Tool takes 3 min to start !!!!!");
            var myMainWindow = new MainWindow();
            myMainWindow.ShowDialog();
        }
    }
}
