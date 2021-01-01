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
        private const string UPDATE_SERVER_URL = "http://localhost:8085/lastversion";
        private const string APPLICATION_VERSION = "0.1";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var main = new MainWindow();
            main.Show();
            Current.MainWindow = main;
            CheckForUpdates();
        }

        private void CheckForUpdates()
        {
            WebRequest webGetUrl = WebRequest.Create(UPDATE_SERVER_URL);
            Stream response = webGetUrl.GetResponse().GetResponseStream();
            StreamReader streamReader = new StreamReader(response);
            var data = JsonSerializer.Deserialize<UpdateData>(streamReader.ReadToEnd());

            if (data.Version.Equals(APPLICATION_VERSION))
                Console.WriteLine("La aplicación está actualizada");
            else
                Console.WriteLine("Existe una nueva versión");
        }

        class UpdateData
        {
            public string Version { get; set; }
            public string DownloadUrl { get; set; }
        }
    }
}
