using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LiteDBManagerUpdater
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //Debugger.Launch();

            MainWindow mainWindow = new MainWindow();

            if(e.Args.Length == 2)
            {
                mainWindow.UpdateFilePath = e.Args[0];
                mainWindow.MainFilePath = e.Args[1];
            }

            Current.MainWindow = mainWindow;
            mainWindow.Show();
            mainWindow.DoUpdate();
        }
    }
}
