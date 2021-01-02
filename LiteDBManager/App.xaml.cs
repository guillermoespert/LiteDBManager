using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace LiteDBManager
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string UPDATE_SERVER_URL = "http://localhost:8085/lastversion";
        public const string APPLICATION_VERSION = "0.1";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var main = new MainWindow();
            main.Show();
            Current.MainWindow = main;
        }
    }
}
