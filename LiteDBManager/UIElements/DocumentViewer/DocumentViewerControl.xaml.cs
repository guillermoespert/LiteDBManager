using LiteDBManager.Services;
using LiteDB;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text;

namespace LiteDBManager.UIElements.DocumentViewer
{
    /// <summary>
    /// Lógica de interacción para DocumentViewer.xaml
    /// </summary>
    public partial class DocumentViewerControl : UserControl, DocumentEditorComponent
    {
        /// <summary>
        /// Contiene una referencia al documento BSON original dado a 
        /// este control. Las modificaciones aportadas al documento se
        /// añaden a este documento.
        /// </summary>
        public BsonValue Document { get; private set; }

        /// <summary>
        /// Mantiene un contador del número de niveles de profundidad
        /// en la creación de documentos hijos.
        /// </summary>
        public int DocumentNestingLevel { get; set; } = 1;

        public int LineCounter { get; private set; } = 0;

        public bool IsEditing { get; private set; }

        public event EventHandler<EventArgs> DeleteDocument;

        public bool IsSelected { get; set; }

        public bool IsGroupCollapsed { get; set; }

        public bool IsEditingTextual { get; private set; }

        public DocumentViewerControl(BsonDocument document) : this(document as BsonValue)
        { }

        public DocumentViewerControl(BsonArray document) : this(document as BsonValue)
        { }

        private DocumentViewerControl(BsonValue document)
        {
            Document = document;

            InitializeComponent();

            grdTextCodeEditor.Visibility = Visibility.Hidden;
            stpLinesContainer.Visibility = Visibility.Visible;

            //Cargar el documento original
            LoadDocument(document);
        }

        /// <summary>
        /// Carga el documento pasado durante la creación del control.
        /// </summary>
        public void LoadDocument(BsonValue document)
        {
            if(document is BsonDocument)
            {
                var doc = document as BsonDocument;

                //Insertar la línea de comienzo
                var start = new DocumentViewerLine(LineType.Opening, ++LineCounter);
                stpLinesContainer.Children.Add(start);

                foreach (var line in doc)
                {
                    if (line.Value is BsonDocument)
                    {
                        var childDoc = new DocumentViewerLineGroup(line, DocumentNestingLevel + 1, ++LineCounter);
                        LineCounter = childDoc.LoadDocument();
                        childDoc.LinesAdded += ChildDoc_LinesAdded;
                        stpLinesContainer.Children.Add(childDoc);
                    }
                    else
                    {
                        //Insertar las líneas de datos
                        var docline = new DocumentViewerLine(line, ++LineCounter);
                        docline.AddNewLine += Children_AddNewLine;
                        docline.UpdatedLineType += Children_UpdatedLineType;
                        stpLinesContainer.Children.Add(docline);
                    }
                }

                //Insertar la línea de cierre
                DocumentViewerLine dvl = new DocumentViewerLine(LineType.Closing, ++LineCounter);
                stpLinesContainer.Children.Add(dvl);
            }
        }

        private void ChildDoc_LinesAdded(object sender, EventArgs e)
        {
            UpdateLineNumbers();
        }

        public void UpdateLineNumbers()
        {
            LineCounter = 1;

            foreach (var child in stpLinesContainer.Children)
            {
                if (child is DocumentViewerLine)
                {
                    var line = child as DocumentViewerLine;
                    line.LineNumber = LineCounter;
                    LineCounter++;
                }
                else if (child is DocumentViewerLineGroup)
                {
                    var group = child as DocumentViewerLineGroup;
                    LineCounter = group.UpdateLineNumbers(LineCounter);
                }
            }
        }

        private void StartLine_InnerDocumentDelete(object sender, LineEventArgs e)
        {
            //var line = sender as DocumentViewerLine;
            //var group = FoldingGroups[line.FoldingGroup];
            //var groupElements = stpLinesContainer.Children;
            //int i = group.LineStart - 1;
            //int end = i + group.LinesCount;

            //for (; i <= end; i++)
            //{
            //    if (i < groupElements.Count)
            //    {
            //        var element = groupElements[i] as DocumentViewerLine;

            //        element.SetAsDeleted();
            //    }
            //}
        }

        public void ActivateEdition()
        {
            if (!IsEditingTextual)
            {
                //Activar el modo edición
                foreach (var child in stpLinesContainer.Children)
                {
                    if (child is DocumentEditorComponent)
                    {
                        var line = child as DocumentEditorComponent;
                        line.ActivateEdition();
                    }
                }

                txtCodeEditor.IsEnabled = false;
            }
            else
            {
                txtCodeEditor.IsEnabled = true;
            }

            IsEditing = true;
            stpLinesContainer.Margin = new Thickness(5, 2, 0, 32);
            txtCodeEditor.Margin = new Thickness(0, 2, 0, 32);
            paneEdition.Visibility = Visibility.Visible;
            grdButtons.Visibility = Visibility.Hidden;        }

        public void CancelEdition()
        {
            // Desactivar el modo edición
            foreach (var child in stpLinesContainer.Children)
            {
                if(child is DocumentEditorComponent)
                {
                    var line = child as DocumentEditorComponent;
                    line.CancelEdition();
                }
            }

            IsEditing = false;
            stpLinesContainer.Margin = new Thickness(5, 2, 0, 0);
            txtCodeEditor.Margin = new Thickness(0, 2, 0, 0);
            paneEdition.Visibility = Visibility.Collapsed;

            txtCodeEditor.IsEnabled = false;

            if(IsEditingTextual)
                txtCodeEditor.Text = SerializeJson(Document);
            else
                ReloadDocument(Document as BsonDocument);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelEdition();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            BsonDocument doc;

            if (IsEditingTextual)
            {
                doc = JsonSerializer.Deserialize(txtCodeEditor.Text) as BsonDocument;
            }
            else
            {
                doc = ParseDocument();
            }

            var collection = DbConnections.CurrentConnection.LiteDatabase.GetCollection(DbConnections.CurrentConnection.EditingCollection);
            collection.Update(doc);
            DbConnections.CurrentConnection.LiteDatabase.Commit();

            IsEditing = false;
            txtCodeEditor.IsEnabled = false;
            stpLinesContainer.Margin = new Thickness(0, 0, 0, 0);
            paneEdition.Visibility = Visibility.Collapsed;

            Document = doc;
            txtCodeEditor.Text = SerializeJson(doc);
            ReloadDocument(Document as BsonDocument);
        }

        /// <summary>
        /// Parsea el contenedor de edición interactiva y convierte el contenido
        /// a un documento BSON.
        /// </summary>
        /// <returns></returns>
        private BsonDocument ParseDocument()
        {
            Dictionary<string, BsonValue> docLines = new Dictionary<string, BsonValue>();

            //Desactivar el modo edición
            foreach (var child in stpLinesContainer.Children)
            {
                if (child is DocumentViewerLine)
                {
                    var line = child as DocumentViewerLine;

                    line.CommitEdition();

                    if (line.LineType == LineType.DataDisplay)
                    {
                        docLines.Add(line.LineKey, line.LineValue);
                    }

                    line.CancelEdition();
                }
                else if (child is DocumentViewerLineGroup)
                {
                    var group = child as DocumentViewerLineGroup;
                    var childDoc = group.ParseDocument();
                    docLines.Add(childDoc.Key, childDoc.Value);
                }
            }

             return new BsonDocument(docLines);
        }

        private void Children_AddNewLine(object sender, LineEventArgs e)
        {
            var line = sender as DocumentViewerLine;

            if (line != null)
            {
                var index = stpLinesContainer.Children.IndexOf(line);

                if (index > -1)
                {
                    BsonValue value = new BsonValue("");
                    DocumentViewerLine newLine = new DocumentViewerLine("newVariable", value, e.LineNumber + 1, DocumentNestingLevel * DocumentViewerLine.DEFAULT_TAB_LENGTH);
                    newLine.UpdatedLineType += Children_UpdatedLineType;
                    newLine.AddNewLine += Children_AddNewLine;
                    ((DocumentEditorComponent)newLine).ActivateEdition();

                    stpLinesContainer.Children.Insert(index + 1, newLine);

                    UpdateLineNumbers();
                }
            }
        }

        private void Children_UpdatedLineType(object sender, LineEventArgs e)
        {
            var index = stpLinesContainer.Children.IndexOf(sender as UIElement);

            if (index > -1)
            {
                var newDoc = new BsonDocument();
                var value = new BsonValue("");
                newDoc.Add("newValue", value);
                var newLine = new KeyValuePair<string, BsonValue>("newDocument", newDoc);
                var childDoc = new DocumentViewerLineGroup(newLine, DocumentNestingLevel + 1, ++LineCounter);
                LineCounter = childDoc.LoadDocument();
                childDoc.LinesAdded += ChildDoc_LinesAdded;
                ((DocumentEditorComponent)childDoc).ActivateEdition();
                stpLinesContainer.Children.Remove(sender as UIElement);
                stpLinesContainer.Children.Insert(index, childDoc);

                UpdateLineNumbers();
            }
        }

        /// <summary>
        /// Recarga el documento dado
        /// </summary>
        /// <param name="document"></param>
        private void ReloadDocument(BsonDocument document)
        {
            if (document != null)
            {
                stpLinesContainer.Children.Clear();
                LineCounter = 0;
                DocumentNestingLevel = 1;
                LoadDocument(document);
            }
        }

        private void Border_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(!IsEditing && !btnDeleteDocument.IsMouseOver)
            {
                brdControlBorder.BorderBrush = Brushes.CornflowerBlue;
                grdButtons.Visibility = Visibility.Visible;
            }
        }

        private void brdControlBorder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsSelected)
            {
                brdControlBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(199, 191, 191)); //-> Gris
            }
            else
            {
                brdControlBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(219, 116, 7)); //-> Naranja
            }

            grdButtons.Visibility = Visibility.Hidden;
        }

        private void UserControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ActivateEdition();
        }

        private void btnDeleteDocument_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            brdControlBorder.BorderBrush = Brushes.Red;
        }

        private void btnDeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            DeleteDocument?.Invoke(this, new EventArgs());
        }

        private void UserControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(!IsEditing && !IsSelected)
            {
                IsSelected = true;
                brdControlBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(219, 116, 7)); //-> Naranja
                brdControlBorder.BorderThickness = new Thickness(3);
            }
            else
            {
                IsSelected = false;
                brdControlBorder.BorderThickness = new Thickness(1);

                if (IsMouseOver)
                    brdControlBorder.BorderBrush = Brushes.CornflowerBlue;
                else
                    brdControlBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(199, 191, 191)); //-> Gris
            }
        }

        private void btnToggleEditor_Click(object sender, RoutedEventArgs e)
        {
            if(IsEditingTextual)
            {
                imgToggleButton.Source = new BitmapImage(new Uri(@"/LiteDBManager;component/Resources/icons8-json-80.png", UriKind.Relative));
                IsEditingTextual = false;
                Document = JsonSerializer.Deserialize(txtCodeEditor.Text);
                ReloadDocument(Document as BsonDocument);
                grdTextCodeEditor.Visibility = Visibility.Collapsed;
                stpLinesContainer.Visibility = Visibility.Visible;
                txtCodeEditor.Text = "";
            }
            else
            {
                imgToggleButton.Source = new BitmapImage(new Uri(@"/LiteDBManager;component/Resources/icons8-tree-structure-96.png", UriKind.Relative));
                IsEditingTextual = true;
                
                txtCodeEditor.Text = SerializeJson(Document);

                grdTextCodeEditor.Visibility = Visibility.Visible;
                stpLinesContainer.Visibility = Visibility.Collapsed;
            }
        }

        private string SerializeJson(BsonValue value)
        {
            StringBuilder sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            {
                var w = new JsonWriter(writer);
                w.Pretty = true;

                w.Serialize(value ?? BsonValue.Null);

                return sb.ToString();
            }
        }
    }
}
