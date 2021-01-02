using LiteDBManager.Services;
using LiteDBManager.Windows;
using LiteDB;
using System.Windows;
using LiteDBManager.UIElements.Pages;
using LiteDBManager.UIElements;
using System.Net;
using System.IO;
using MSJson = System.Text.Json;
using System;
using System.Timers;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace LiteDBManager
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer downloadTimer = new Timer(1000);
        private UpdateData updateData;
        private string updateLocation;

        public MainWindow()
        {
            InitializeComponent();

            frmDbManager.Content = new WelcomePage();
            CheckForUpdates();
        }

        private void btnCreateDB_Click(object sender, RoutedEventArgs e)
        {
            ConnectionString ConnectionData = new ConnectionString();
            CreateDb cdb = new CreateDb(ConnectionData);
            cdb.Owner = this;
            
            if(cdb.ShowDialog().Value)
            {
                AddNewConnection(ConnectionData);
            }
        }

        private void btnOpenDB_Click(object sender, RoutedEventArgs e)
        {
            ConnectionString ConnectionData = new ConnectionString();
            DbConnection con = new DbConnection(ConnectionData);
            con.Owner = this;

            if(con.ShowDialog().Value)
            {
                AddNewConnection(ConnectionData);
            }
        }

        private void AddNewConnection(ConnectionString conndata)
        {
            MainService.AddNewConnection(conndata);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                foreach (var connection in DbConnections.Connections)
                {
                    connection?.LiteDatabase.Checkpoint();
                    connection?.LiteDatabase.Dispose();
                }
            }
            catch { }

            if(updateData != null && !string.IsNullOrWhiteSpace(updateLocation))
            {
                string appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                try
                {
                    File.Copy(Path.Combine(appPath, "LiteDBManagerUpdater.exe"), Path.Combine(appPath, "updLiteDBManagerUpdater.exe"));

                    StringBuilder stb = new StringBuilder();
                    stb.Append('"').Append(updateLocation).Append('"').Append(' ').Append('"').Append(appPath).Append('"');

                    Process.Start(Path.Combine(appPath, "updLiteDBManagerUpdater.exe"), stb.ToString());
                }
                catch { }
            }
        }

        private void btnAboutUs_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutUs();
            about.Owner = this;
            about.ShowDialog();
        }

        private void frmDbManager_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (DbConnections.CurrentConnection != null)
            {
                btnShowCollections.IsEnabled = true;
                btnShowCommandsPage.IsEnabled = true;
            }
            else
            {
                btnShowCollections.IsEnabled = false;
                btnShowCommandsPage.IsEnabled = false;
            }

            if(frmDbManager.Content is CollectionsManagementPage)
            {
                btnAddCollection.IsEnabled = true;
                btnAddDocument.IsEnabled = false;
            }
            else if (frmDbManager.Content is DocumentManagementPage)
            {
                btnAddCollection.IsEnabled = false;
                btnAddDocument.IsEnabled = true;
            }

            if(frmDbManager.Content is DataManipulation)
            {
                btnImportSelected.IsEnabled = true;
                btnExportSelected.IsEnabled = true;
            }
            else
            {
                btnImportSelected.IsEnabled = false;
                btnExportSelected.IsEnabled = false;
            }
        }

        private void btnShowCollections_Click(object sender, RoutedEventArgs e)
        {
            if (DbConnections.CurrentConnection != null)
            {
                var connection = DbConnections.CurrentConnection;

                PageNavigationService.ShowPage(connection.CollectionManagementPage);
            }
        }

        private void btnExportSelected_Click(object sender, RoutedEventArgs e)
        {
            if(frmDbManager.Content != null && frmDbManager.Content is DataManipulation)
            {
                ((DataManipulation)frmDbManager.Content).ExportSelected();
            }
        }

        private void btnImportSelected_Click(object sender, RoutedEventArgs e)
        {
            if (frmDbManager.Content != null && frmDbManager.Content is DataManipulation)
            {
                ((DataManipulation)frmDbManager.Content).ImportSelected();
            }
        }

        private void btnAddCollection_Click(object sender, RoutedEventArgs e)
        {
            if (frmDbManager.Content != null && frmDbManager.Content is DataManipulation)
            {
                ((DataManipulation)frmDbManager.Content).AddItem();
            }
        }

        private void btnAddDocument_Click(object sender, RoutedEventArgs e)
        {
            if (frmDbManager.Content != null && frmDbManager.Content is DataManipulation)
            {
                ((DataManipulation)frmDbManager.Content).AddItem();
            }
        }

        private void btnShowCommandsPage_Click(object sender, RoutedEventArgs e)
        {
            if (DbConnections.CurrentConnection != null)
            {
                PageNavigationService.ShowPage(DbConnections.CurrentConnection.CommandManagementPage);
            }
        }

        private void CheckForUpdates()
        {
            string appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            /*
             * Operaciones de finalización de actualización previa:
             * -Elimina el directorio temporal de descarga.
             * -Elimina el ejecutable de actualización usado anteriormente.
             * -Renombra el archivo de base de datos de depuración del ejecutable de actualización.
             */
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "LiteDBManager");
                string tempUpdater = Path.Combine(appPath, "updLiteDBManagerUpdater.exe");
                string newPdb = Path.Combine(appPath, "updLiteDBManagerUpdater.pdb");
                string renamePdb = Path.Combine(appPath, "LiteDBManagerUpdater.pdb");

                //Debugger.Launch();

                if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);

                if (File.Exists(tempUpdater)) File.Delete(tempUpdater);

                if (File.Exists(newPdb))
                {
                    if (File.Exists(renamePdb)) File.Delete(renamePdb);

                    File.Copy(newPdb, renamePdb);

                    File.Delete(newPdb);
                }
            }
            catch
            {
                tbkStatusMessage.Text = "Error eliminando datos de actualización obsoletos.";

                return;
            }

            /*
             * Comprobar nuevas actualizaciones.
             */
            try 
            {
                WebRequest webGetUrl = WebRequest.Create(App.UPDATE_SERVER_URL);
                Stream response = webGetUrl.GetResponse().GetResponseStream();
                StreamReader streamReader = new StreamReader(response);
                var data = MSJson.JsonSerializer.Deserialize<UpdateData>(streamReader.ReadToEnd());

                if (data != null)
                {
                    if (data.Version.Equals(App.APPLICATION_VERSION))
                        tbkStatusMessage.Text = "La aplicación está actualizada";
                    else
                    {
                        tbkStatusMessage.Text = "Existe una nueva versión";
                        downloadTimer.AutoReset = false;
                        downloadTimer.Elapsed += DownloadTimer_Elapsed;
                        downloadTimer.Start();
                        updateData = data;
                    }
                }
            }
            catch
            {
                tbkStatusMessage.Text = "Error verificando actualizaciones.";
            }
        }

        private void DownloadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            downloadTimer.Stop();

            if(updateData != null)
            {
                Dispatcher.Invoke(() =>
                {
                    tbkStatusMessage.Text = "Descargando actualización...";
                    pgrbDownload.Visibility = Visibility.Visible;
                });

                string path = Path.Combine(Path.GetTempPath(), "LiteDBManager");
                updateLocation = Path.Combine(path, Path.GetFileName(updateData.DownloadUrl));

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += updateDownload_DownloadProgressChanged;
                    wc.DownloadFileAsync(new Uri(updateData.DownloadUrl),
                        updateLocation
                    );
                }
            }
        }

        private void updateDownload_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                pgrbDownload.Value = e.ProgressPercentage;

                if (e.ProgressPercentage >= 100)
                {
                    pgrbDownload.Visibility = Visibility.Collapsed;
                    tbkStatusMessage.Text = "Descarga finalizada. La actualización se instalará cuando cierre la aplicación.";
                }
            });
        }

        class UpdateData
        {
            public string Version { get; set; }
            public string DownloadUrl { get; set; }
        }
    }
}
