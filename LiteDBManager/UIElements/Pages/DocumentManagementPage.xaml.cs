using LiteDB;
using LiteDBManager.Services;
using LiteDBManager.Structures;
using LiteDBManager.Windows;
using Microsoft.Win32;
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

        /// <summary>
        /// Carga en la página todos los documentos de la colección data
        /// </summary>
        /// <param name="collection"></param>
        public void LoadDocuments(string collection)
        {
            documentsContainer.ClearAllDocuments();
            var query = string.Format("SELECT $ FROM {0}", collection);
            var value = DbConnections.CurrentConnection.LiteDatabase.Execute(query).ToList();
            documentsContainer.LoadDocuments(value);
        }

        /// <summary>
        /// Carga en la página todos los documentos de la colección en edición
        /// </summary>
        public void LoadDocuments() => LoadDocuments(DbConnections.CurrentConnection.EditingCollection);

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

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if(documentsContainer.CountSelectedDocuments > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.CheckPathExists = true;
                sfd.Filter = "Archivos JSON (*.json)|*.json|Archivos XML (*.xml)|*.xml";

                if (sfd.ShowDialog().Value)
                {
                    var documents = documentsContainer.GetSelectedDocuments();
                    var documentWrapper = new CollectionsWrapperDO();
                    var collection = new CollectionDO();

                    collection.CollectionName = null;

                    foreach (BsonValue doc in documents)
                    {
                        collection.Documents.Add(JsonSerializer.Serialize(doc));
                    }

                    documentWrapper.Collections.Add(collection);
                    DataOperationsService.ExportData(documentWrapper, sfd.FileName);
                }
            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckPathExists = true;
            ofd.CheckFileExists = true;
            ofd.Filter = "Archivos JSON (*.json)|*.json|Archivos XML (*.xml)|*.xml";

            if (ofd.ShowDialog().Value)
            {
                DataOperationsService.ImportData(ofd.FileName, DataOperations.ImportDocuments);
            }
        }
    }
}
