using LiteDBManager.Services;
using LiteDBManager.Structures;
using LiteDB;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System;
using Microsoft.Win32;

namespace LiteDBManager.UIElements
{
    /// <summary>
    /// Lógica de interacción para DbConnexion.xaml
    /// </summary>
    public partial class DbConnectionControl : UserControl
    {
        private DbConnection DbConnection;

        public bool ConnectionOpened { get; set; }

        public DbConnectionControl(DbConnection dbconn)
        {
            DbConnection = dbconn;

            InitializeComponent();

            LoadConnection();
        }

        private void UserControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(!ConnectionOpened)
                LoadConnection();
        }

        public void LoadConnection()
        {
            MainService.UnselectAllConnections();

            if(DbConnection != null)
                DbConnections.CurrentConnection = DbConnection;

            tbkConnectionStatus.Text = "Conectado";
            tbkFilename.Text = Path.GetFileName(DbConnection.ConnectionData.Filename);
            Background = Brushes.LightGreen;
            ConnectionOpened = true;

            PageNavigationService.ShowPage(DbConnection.CollectionManagementPage);

            MainService.UpdateCollections();
        }

        private void btnCloseConnection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DbConnection?.LiteDatabase.Checkpoint();
            DbConnection?.LiteDatabase.Dispose();

            MainService.RemoveConnection(DbConnection);
        }

        private void btnDuplicate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var result = MessageBox.Show(MainService.MainWindow, "Se va a realizar una copia del archivo de base de datos.\nSi el archivo de base de datos tiene transacciones sin apuntar estas pueden no estar disponibles en el archivo duplicado.\n\n¿Desea apuntar todas las transacciones pendientes antes de copiar el archivo de base de datos?",
                "Apunte de transacciones pendientes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DbConnection.LiteDatabase.Checkpoint();
            }
            else if (result == MessageBoxResult.Cancel) return;


            var dbPath = DbConnection.ConnectionData.Filename;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = true;
            sfd.InitialDirectory = Path.GetDirectoryName(dbPath);
            sfd.Title = "Selección de la ubicación de la copia";

            if (sfd.ShowDialog().Value)
            {
                try
                {
                    DbConnection.LiteDatabase.Dispose();

                    File.Copy(dbPath, sfd.FileName);

                    result = MessageBox.Show(MainService.MainWindow, "El archivo de base de datos se ha copiado correctamente.\n\n¿Desea conectar a la nueva base de datos?",
                        "Copia terminada", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if(result == MessageBoxResult.Yes)
                    {
                        var currConnStr = DbConnection.ConnectionData;
                        ConnectionString connString = new ConnectionString();
                        connString.Collation = (currConnStr.Collation != null)? new Collation(currConnStr.Collation.LCID, currConnStr.Collation.SortOptions) : null;
                        connString.Connection = currConnStr.Connection;
                        connString.Filename = sfd.FileName;
                        connString.InitialSize = currConnStr.InitialSize;
                        connString.Password = currConnStr.Password;
                        connString.ReadOnly = currConnStr.ReadOnly;
                        connString.Upgrade = currConnStr.Upgrade;

                        MainService.AddNewConnection(connString);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(MainService.MainWindow, "Se ha producido un error: \n\n" + ex.Message,
                        "Error durante la copia de la base de datos", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DbConnection.LiteDatabase = new LiteDatabase(DbConnection.ConnectionData);
                }
            }

            
        }
    }
}
