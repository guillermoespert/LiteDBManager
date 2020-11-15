using LiteDBManager.Services;
using LiteDBManager.Windows;
using LiteDB;
using System.Windows;

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
            Connection con = new Connection(ConnectionData);
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
    }
}
