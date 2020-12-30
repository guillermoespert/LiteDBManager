using LiteDBManager.Services;
using LiteDBManager.Windows;
using LiteDB;
using System.Windows;
using LiteDBManager.UIElements.Pages;
using LiteDBManager.UIElements;

namespace LiteDBManager
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            frmDbManager.Content = new WelcomePage();
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
            foreach(var connection in DbConnections.Connections)
            {
                connection?.LiteDatabase.Checkpoint();
                connection?.LiteDatabase.Dispose();
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
            if(DbConnections.CurrentConnection != null)
                btnShowCollections.IsEnabled = true;
            else
                btnShowCollections.IsEnabled = false;

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
    }
}
