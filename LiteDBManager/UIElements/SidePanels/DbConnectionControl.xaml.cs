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

        public DbConnectionControl(DbConnection dbconn)
        {
            DbConnection = dbconn;

            InitializeComponent();

            LoadConnection();
        }

        private void UserControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoadConnection();
        }

        public void LoadConnection()
        {
            MainService.UnselectAllConnections();

            if(DbConnection == null)
                DbConnection = DbConnections.CurrentConnection;

            tbkConnectionStatus.Text = "Conectado";
            tbkFilename.Text = Path.GetFileName(DbConnection.ConnectionData.Filename);
            Background = Brushes.LightGreen;

            PageNavigationService.ShowPage(DbConnection.CollectionManagementPage);

            MainService.UpdateCollections();
        }
    }
}
