using LiteDB;
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
    public partial class DocumentManagementPage : Page
    {
        //private FoldingManager foldingManager;
        //private InlineDisplayFoldingStrategy foldingStrategy;

        public DocumentManagementPage()
        {
            InitializeComponent();

            var dbConnection = DbConnections.CurrentConnection;
            tbkFilename.Text = Path.GetFileName(dbConnection.ConnectionData.Filename);
            tbkDocument.Text = dbConnection.EditingCollection;

            LoadDocuments();
        }

        private void textEditor_TextChanged(object sender, System.EventArgs e)
        {
            //foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
        }

        public void LoadDocuments()
        {
            documentsContainer.ClearAllDocuments();
            var query = string.Format("SELECT $ FROM {0}", DbConnections.CurrentConnection.EditingCollection);
            var value = DbConnections.CurrentConnection.LiteDatabase.Execute(query).ToList();
            documentsContainer.LoadDocuments(value);
        }

        private void btnAddDocument_Click(object sender, RoutedEventArgs e)
        {
            AddNewDocument and = new AddNewDocument();
            and.Owner = MainService.MainWindow;
            var result = and.ShowDialog();

            if (result.Value)
            {
                SqlServices.AddEmptyDocument(and.IdType);
                documentsContainer.ClearAllDocuments();
                LoadDocuments();
            }
        }
    }
}
