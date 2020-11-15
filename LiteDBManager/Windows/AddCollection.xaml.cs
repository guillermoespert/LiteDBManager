using LiteDBManager.Services;
using LiteDBManager.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LiteDBManager.Windows
{
    /// <summary>
    /// Lógica de interacción para AddCollection.xaml
    /// </summary>
    public partial class AddCollection : Window
    {
        public AddCollection()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtCollectionName.Text))
            {
                MessageBox.Show(this, "El nombre para la nueva colección no puede estar vacío.",
                    "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var ldb = DbConnections.CurrentConnection.LiteDatabase;

            string create = string.Format("INSERT INTO {0}:INT VALUES {{}}", txtCollectionName.Text); //Crea una colección con un registro
            string delete = string.Format("DELETE {0}", txtCollectionName.Text); //->Vacíar la colección creada con INSERT
            string commit = "COMMIT"; //->Valida los cambios y los vuelve permanentes

            ldb.Execute(create);
            ldb.Execute(delete);
            ldb.Execute(commit);

            DialogResult = true;
        }
    }
}
