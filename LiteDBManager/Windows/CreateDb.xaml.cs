using System.IO;
using System.Windows;
using System.Windows.Forms;
using LiteDB;
using MessageBox = System.Windows.MessageBox;

namespace LiteDBManager.Windows
{
    /// <summary>
    /// Lógica de interacción para CreateDb.xaml
    /// </summary>
    public partial class CreateDb : Window
    {
        public ConnectionString DbConnection { get; private set; }

        public CreateDb(ConnectionString constring)
        {
            DbConnection = constring;
            InitializeComponent();
        }

        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            DbOptions dbo = new DbOptions(DbOptionEditModes.Create);
            dbo.DbConnection = DbConnection;
            dbo.Owner = this;
            dbo.ShowDialog();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtFileName.Text))
            {
                MessageBox.Show(this, "El nombre de la base de datos no puede estar vacío.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDbPath.Text))
            {
                MessageBox.Show(this, "El ruta de la base de datos no puede estar vacía.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if(!Directory.Exists(txtDbPath.Text))
            {
                MessageBox.Show(this, "El ruta especificada no existe. Por favor elija una ubicación para guardar la base de datos.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string fullPath = Path.Combine(txtDbPath.Text, txtFileName.Text);

            if(File.Exists(fullPath))
            {
                MessageBox.Show(this, "Ya existe una base de datos con el mismo nombre en esa ubicación. Elija un nombre o ubicación diferentes.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            DbConnection.Filename = fullPath;

            DialogResult = true;
        }

        private void btnSearchFile_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Seleccione la carpeta donde desea guardar la nueva base de datos";
            fbd.ShowNewFolderButton = true;
            var reponse = fbd.ShowDialog();

            if(reponse == System.Windows.Forms.DialogResult.OK)
            {
                txtDbPath.Text = fbd.SelectedPath;
            }
        }
    }
}
