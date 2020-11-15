using LiteDBManager.Services;
using System.Windows;
using System.Windows.Controls;

namespace LiteDBManager.UIElements
{
    /// <summary>
    /// Lógica de interacción para DbCollection.xaml
    /// </summary>
    public partial class DbCollectionControl : UserControl
    {
        public string CollectionName 
        { 
            get { return tbkCollection.Text; }
            set { tbkCollection.Text = value; } 
        }

        public DbCollectionControl()
        {
            InitializeComponent();
        }

        public DbCollectionControl(string collectionName) : this()
        {
            CollectionName = collectionName;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(MainService.MainWindow, "La colección '" + CollectionName + "' y todos sus documentos serán eliminados de manera definitiva. Esta acción no puede ser deshecha.\n\n¿Está seguro que desea eliminar esta colección?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if(result == MessageBoxResult.Yes)
            {
                var db = DbConnections.CurrentConnection.LiteDatabase;
                db.Execute("DROP COLLECTION " + CollectionName);
                db.Execute("COMMIT");
                MainService.UpdateCollections();
            }
        }

        private void dbcollectioncontrol_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PageNavigationService.OpenCollection(CollectionName);
        }
    }
}
