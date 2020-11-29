using LiteDBManager.UIElements.DocumentViewer;
using LiteDB;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiteDBManager.Services;

namespace LiteDBManager.UIElements
{
    /// <summary>
    /// Lógica de interacción para DocumentViewerContainer.xaml
    /// </summary>
    public partial class DocumentViewerContainer : UserControl
    {
        public DocumentViewerContainer()
        {
            InitializeComponent();
        }

        public int CountSelectedDocuments
        {
            get { return SelectedDocumentsCounter(); }
        }

        public void LoadDocuments(IList<BsonValue> values)
        {
            foreach(var value in values)
            {
                DocumentViewerControl dv = new DocumentViewerControl(value);
                dv.DeleteDocument += DocumentViewer_DeleteDocument;
                dv.Margin = new Thickness(5, 10, 5, 0);
                stpDocumentsContainer.Children.Add(dv);
            }
        }

        private void DocumentViewer_DeleteDocument(object sender, EventArgs e)
        {
            var result = MessageBox.Show(MainService.MainWindow, "El documento seleccionado será eliminado de forma permanente.\n¿Esta seguro que desea eliminar este documento?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var document = (DocumentViewerControl)sender;
                SqlServices.DeleteDocument((BsonDocument)document.Document);
            }
        }

        public void ClearAllDocuments()
        {
            stpDocumentsContainer.Children.Clear();
        }

        private int SelectedDocumentsCounter()
        {
            int count = 0;

            foreach(DocumentViewerControl child in stpDocumentsContainer.Children)
            {
                if (child.IsSelected)
                    count++;
            }

            return count;
        }

        public List<BsonValue> GetSelectedDocuments()
        {
            List<BsonValue> documents = new List<BsonValue>();

            foreach (DocumentViewerControl child in stpDocumentsContainer.Children)
            {
                if (child.IsSelected)
                {
                    if(!child.IsEditing)
                    {
                        documents.Add(child.Document);
                    }
                    else
                    {
                        MessageBox.Show(MainService.MainWindow, "No se pueden importar colecciones que estén siendo editadas. Por favor termine la edición antes de importar", "Acción no permitida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }

            return documents;
        }
    }
}
