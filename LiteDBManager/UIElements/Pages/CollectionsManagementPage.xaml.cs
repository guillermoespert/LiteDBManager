using LiteDBManager.Services;
using LiteDBManager.Windows;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace LiteDBManager.UIElements
{
    /// <summary>
    /// Lógica de interacción para DbManagementPage.xaml
    /// </summary>
    public partial class CollectionsManagementPage : Page
    {
        public CollectionsManagementPage()
        {
            InitializeComponent();

            LoadCollections();
        }

        /// <summary>
        /// Carga todas las colecciones contenidas en la base de datos actual.
        /// </summary>
        public void LoadCollections()
        {
            var dbConnection = DbConnections.CurrentConnection;
            tbkFilename.Text = Path.GetFileName(dbConnection.ConnectionData.Filename);
            dgCollections.ItemsSource = dbConnection.LiteDatabase.GetCollectionNames();
        }

        private void btnAddCollection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dbConnection = DbConnections.CurrentConnection;
            AddCollection adcol = new AddCollection();
            adcol.Owner = Application.Current.MainWindow;
            adcol.ShowDialog();

            MainService.UpdateCollections();
            dgCollections.ItemsSource = dbConnection.LiteDatabase.GetCollectionNames();
        }

        /// <summary>
        /// Gestiona el evento de doble clic de ratón sobre la tabla.
        /// Abre la colección que se encuentre seleccionada en ese momento.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCollections_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var collection = dgCollections.SelectedItem as string;

            if(collection != null)
            {
                PageNavigationService.OpenCollection(collection);
            }
        }

        /// <summary>
        /// Gestiona el evento eliminar colección en la tabla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgCollections.SelectedItem != null)
            {
                var collectionName = (string)dgCollections.SelectedItem;

                var result = MessageBox.Show(MainService.MainWindow, "La colección '" + collectionName + "' y todos sus documentos serán eliminados de manera definitiva. Esta acción no puede ser deshecha.\n\n¿Está seguro que desea eliminar esta colección?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SqlServices.DeleteCollection(collectionName);
                    MainService.UpdateCollections();
                    LoadCollections();
                }
            }
        }

        /// <summary>
        /// Gestiona el evento renombrar de la tabla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRenombrar_Click(object sender, RoutedEventArgs e)
        {
            if (dgCollections.SelectedItem != null)
            {
                var collectionName = (string)dgCollections.SelectedItem;

                var renameCollection = new RenameCollection(collectionName);
                var result = renameCollection.ShowDialog();

                if (result.Value)
                {
                    SqlServices.RenameCollection(collectionName, renameCollection.NewName);
                    MainService.UpdateCollections();
                    LoadCollections();
                }
            }
        }

        /// <summary>
        /// Gestiona el evento vaciar de la tabla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVaciar_Click(object sender, RoutedEventArgs e)
        {
            if (dgCollections.SelectedItem != null)
            {
                var collectionName = (string)dgCollections.SelectedItem;

                var result = MessageBox.Show(MainService.MainWindow, "Todos los documentos de la colección '" + collectionName + "' serán eliminados de manera definitiva. Esta acción no puede ser deshecha.\n\n¿Está seguro que desea vaciar esta colección?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SqlServices.EmptyCollection(collectionName);
                    MainService.UpdateCollections();
                    LoadCollections();
                    DbConnections.CurrentConnection.DocumentManagementPages[collectionName].LoadDocuments();
                }
            }
        }
    }
}
