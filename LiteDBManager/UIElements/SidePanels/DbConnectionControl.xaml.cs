using LiteDBManager.Services;
using LiteDBManager.Structures;
using LiteDB;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

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
    }
}
