using LiteDBManager.Structures;
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
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class AddNewDocument : Window
    {
        public DbIdTypes IdType { get; private set; }

        public AddNewDocument()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnAddDocument_Click(object sender, RoutedEventArgs e)
        {
            if(cbxIdType.SelectedItem != null)
            {
                IdType = (DbIdTypes)cbxIdType.SelectedItem;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(this, "Debe seleccionar un tipo para el índice de la nueva colección", "Indice no seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
