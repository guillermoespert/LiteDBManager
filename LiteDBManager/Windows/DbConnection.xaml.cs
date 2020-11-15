using LiteDB;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace LiteDBManager.Windows
{
    /// <summary>
    /// Lógica de interacción para Connection.xaml
    /// </summary>
    public partial class DbConnection : Window
    {
        public ConnectionString ConnectionString { get; private set; }

        public DbConnection(ConnectionString dbConnection)
        {
            ConnectionString = dbConnection;
            InitializeComponent();
        }

        private void btnSearchFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;

            if(ofd.ShowDialog().Value)
            {
                txtDbPath.Text = ofd.FileName;
            }
        }

        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            DbOptions dbo = new DbOptions(DbOptionEditModes.Connect);
            dbo.DbConnection = ConnectionString;
            dbo.Owner = this;
            dbo.ShowDialog();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDbPath.Text))
            {
                MessageBox.Show(this, "Debe especificar la ruta al archivo de base de datos para continuar.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!File.Exists(txtDbPath.Text))
            {
                MessageBox.Show(this, "El archivo de base de datos especificado no existe.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ConnectionString.Filename = txtDbPath.Text;

            DialogResult = true;
        }
    }
}
