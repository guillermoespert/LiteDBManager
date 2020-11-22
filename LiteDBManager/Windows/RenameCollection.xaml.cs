using LiteDBManager.Services;
using System.Text.RegularExpressions;
using System.Windows;

namespace LiteDBManager.Windows
{
    /// <summary>
    /// Lógica de interacción para RenameCollection.xaml
    /// </summary>
    public partial class RenameCollection : Window
    {
        public string CurrentName { get; private set; }
        public string NewName { get; private set; }

        public RenameCollection(string currentName)
        {
            CurrentName = currentName;

            InitializeComponent();
            lblNombreActual.Content = currentName;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCollectionName.Text))
            {
                MessageBox.Show(this, "El nombre para la nueva colección no puede estar vacío.",
                    "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!Regex.IsMatch(txtCollectionName.Text, "^[a-zA-Z0-9_]+$"))
            {
                MessageBox.Show(this, "El nombre para la nueva colección solo puede contener letras, cifras y guión bajo.",
                    "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            NewName = txtCollectionName.Text;
            DialogResult = true;
        }
    }
}
