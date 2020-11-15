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
    }
}
