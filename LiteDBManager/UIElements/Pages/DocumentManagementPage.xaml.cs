using LiteDBManager.Services;
using LiteDBManager.Windows;
using ICSharpCode.AvalonEdit.Folding;
using LiteDB;
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

            //foldingManager = FoldingManager.Install(textEditor.TextArea);
            //foldingStrategy = new InlineDisplayFoldingStrategy();

            var query = string.Format("SELECT $ FROM {0}", dbConnection.EditingCollection);
            var value = DbConnections.CurrentConnection.LiteDatabase.Execute(query).ToList();
            documentsContainer.LoadCollections(value);

            //textEditor.Text = "{\n\ttest : \"string\"\n\tobject : {\n\t\ttext : value\n\t}\n}";

            //foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
        }

        private void btnAddCollection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddCollection adcol = new AddCollection();
            adcol.Owner = Application.Current.MainWindow;
            adcol.ShowDialog();

            MainService.UpdateCollections();
        }

        private void textEditor_TextChanged(object sender, System.EventArgs e)
        {
            //foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
        }
    }
}
