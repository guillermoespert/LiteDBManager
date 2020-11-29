using LiteDBManager.Services;
using ICSharpCode.AvalonEdit.Editing;
using LiteDB;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;

namespace LiteDBManager.UIElements.DocumentViewer
{
    /// <summary>
    /// Lógica de interacción para DocumentViewer.xaml
    /// </summary>
    public partial class DocumentViewerControl : UserControl
    {
        /// <summary>
        /// Contiene una referencia al documento BSON que este control
        /// renderiza.
        /// </summary>
        public BsonValue Document { get; set; }

        /// <summary>
        /// Mantiene un contador del número de niveles de profundidad
        /// en la creación de documentos hijos.
        /// </summary>
        public int DocumentNestingLevel { get; set; } = 1;

        public List<FoldingGroup> FoldingGroups { get; private set; } = new List<FoldingGroup>();

        public int LineCounter { get; private set; } = 0;

        public bool IsEditing { get; private set; }

        public event EventHandler<EventArgs> DeleteDocument;

        public bool IsSelected { get; set; }

        public DocumentViewerControl(BsonValue document)
        {
            Document = document;

            InitializeComponent();

            LoadDocument();
        }

        /// <summary>
        /// Carga el documento pasado durante la creación del control.
        /// </summary>
        public void LoadDocument()
        {
            //Insertar la línea de comienzo
            stpLinesContainer.Children.Add(new DocumentViewerLine(LineType.Opening, ++LineCounter));

            var document = Document as BsonDocument;
            
            foreach(var line in document)
            {
                if(line.Value is BsonDocument)
                {
                    DocumentNestingLevel++;
                    LoadBsonDocument(line);
                }
                else
                {
                    DocumentNestingLevel = 1;
                    //Insertar las líneas de datos
                    var docline = new DocumentViewerLine(line, ++LineCounter);
                    docline.LineStartedEdition += Children_LineStartedEdition;
                    docline.AddNewLine += Children_AddNewLine;
                    stpLinesContainer.Children.Add(docline);
                }
            }

            //Insertar la línea de cierre
            DocumentViewerLine dvl = new DocumentViewerLine(LineType.Closing, ++LineCounter);
            stpLinesContainer.Children.Add(dvl);
        }

        /// <summary>
        /// Carga todos los documentos anidados que pueda tener el documento
        /// principal o envolvente.
        /// </summary>
        /// <param name="document"></param>
        public void LoadBsonDocument(KeyValuePair<string, BsonValue> document)
        {
            var bsonDoc = document.Value as BsonDocument;
            var tabLength = DocumentViewerLine.DEFAULT_TAB_LENGTH;

            if (bsonDoc != null)
            {
                int bracketsTab = (DocumentNestingLevel - 1 > 0) ? (DocumentNestingLevel - 1) * tabLength : tabLength;

                //Creación de un nuevo grupo de cierre
                FoldingGroup folding = new FoldingGroup();
                int newGroupIndex = FoldingGroups.Count;
                FoldingGroups.Add(folding); // -> Insertar para reservar el índice
                folding.LineStart = ++LineCounter;

                //Insertar la línea de comienzo
                var startLine = new DocumentViewerLine(document, LineCounter, LineType.NestedObjectOpening, bracketsTab);
                startLine.FoldingGroup = newGroupIndex;
                startLine.Visibility = (DocumentNestingLevel > 2) ? Visibility.Collapsed : Visibility.Visible;
                startLine.IsGroupCollapsed = true;
                startLine.IsNestedDocumentChildren = true;
                startLine.FoldActionRequest += FoldableStartLine_FoldActionRequest;
                startLine.LineStartedEdition += Children_LineStartedEdition;
                startLine.InnerDocumentDelete += StartLine_InnerDocumentDelete;
                startLine.AddNewLine += Children_AddNewLine;
                stpLinesContainer.Children.Add(startLine);
                
                foreach (var line in bsonDoc)
                {
                    if (line.Value is BsonDocument)
                    {
                        DocumentNestingLevel++;
                        LoadBsonDocument(line);
                    }
                    else
                    {
                        //Insertar las líneas de datos
                        var docline = new DocumentViewerLine(line, ++LineCounter, DocumentNestingLevel * tabLength);
                        docline.Visibility = Visibility.Collapsed;
                        docline.LineStartedEdition += Children_LineStartedEdition;
                        docline.AddNewLine += Children_AddNewLine;
                        docline.IsNestedDocumentChildren = true;
                        stpLinesContainer.Children.Add(docline);
                    }

                }

                //Insertar la línea de cierre 
                var endLine = new DocumentViewerLine(LineType.NestedObjectClosing, ++LineCounter, bracketsTab);
                endLine.Visibility = Visibility.Collapsed;
                stpLinesContainer.Children.Add(endLine);

                folding.LinesCount = LineCounter - folding.LineStart;
                FoldingGroups[newGroupIndex] = folding; // -> Actualizar la estructura con los cambios
            }
        }

        private void StartLine_InnerDocumentDelete(object sender, LineEventArgs e)
        {
            var line = sender as DocumentViewerLine;
            var group = FoldingGroups[line.FoldingGroup];
            var groupElements = stpLinesContainer.Children;
            int i = group.LineStart - 1;
            int end = i + group.LinesCount;

            for (; i <= end; i++)
            {
                if (i < groupElements.Count)
                {
                    var element = groupElements[i] as DocumentViewerLine;

                    element.SetAsDeleted();
                }
            }
        }

        private void Children_LineStartedEdition(object sender, System.EventArgs e)
        {
            ActivateEditionMode();
        }

        public void ActivateEditionMode()
        {
            //Activar el modo edición
            foreach (var child in stpLinesContainer.Children)
            {
                var line = child as DocumentViewerLine;
                line.IsInEditionMode = true;
            }

            IsEditing = true;
            stpLinesContainer.Margin = new Thickness(5, 2, 0, 32);
            paneEdition.Visibility = Visibility.Visible;
            btnDeleteDocument.Visibility = Visibility.Hidden;
        }

        private void FoldableStartLine_FoldActionRequest(object sender, FoldEventArgs e)
        {
            var line = sender as DocumentViewerLine;
            
            if(line != null && FoldingGroups.Count > e.FoldingGroup)
            {
                var group = FoldingGroups[e.FoldingGroup];
                var groupElements = stpLinesContainer.Children;
                int i = group.LineStart;
                int end = (i - 1) + group.LinesCount;
                Visibility newStatus = line.IsGroupCollapsed ? Visibility.Visible : Visibility.Collapsed;

                for (; i <= end; i++)
                {
                    if(i < groupElements.Count)
                    {
                        var element = groupElements[i] as DocumentViewerLine;

                        element.Visibility = newStatus;
                        element.IsGroupCollapsed = !line.IsGroupCollapsed;
                    }
                }

                line.IsGroupCollapsed = !line.IsGroupCollapsed;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Desactivar el modo edición
            foreach (var child in stpLinesContainer.Children)
            {
                var line = child as DocumentViewerLine;
                line.IsInEditionMode = false;
                line.CancelEdition();
            }

            IsEditing = false;
            stpLinesContainer.Margin = new Thickness(0, 0, 0, 0);
            paneEdition.Visibility = Visibility.Collapsed;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, BsonValue> docLines = new Dictionary<string, BsonValue>();

            //Desactivar el modo edición
            foreach (var child in stpLinesContainer.Children)
            {
                var line = child as DocumentViewerLine;

                line.CommitEdition();

                if (line.LineType == LineType.DataDisplay && !line.IsNestedDocumentChildren)
                {
                    docLines.Add(line.LineKey, line.LineValue);
                }
                else if(line.LineType == LineType.NestedObjectOpening)
                {
                    var document = ParseChildrenDocument(line.FoldingGroup);
                    docLines.Add(line.LineKey, document);
                }

                line.IsInEditionMode = false;
                line.CancelEdition();
            }

            BsonDocument doc = new BsonDocument(docLines);

            var collection = DbConnections.CurrentConnection.LiteDatabase.GetCollection(DbConnections.CurrentConnection.EditingCollection);
            collection.Update(doc);
            DbConnections.CurrentConnection.LiteDatabase.Commit();

            IsEditing = false;
            stpLinesContainer.Margin = new Thickness(0, 0, 0, 0);
            paneEdition.Visibility = Visibility.Collapsed;

            ReloadDocument();
        }

        private BsonDocument ParseChildrenDocument(int foldingGroup)
        {
            Dictionary<string, BsonValue> docLines = new Dictionary<string, BsonValue>();

            if (FoldingGroups.Count > foldingGroup)
            {
                var group = FoldingGroups[foldingGroup];
                var groupElements = stpLinesContainer.Children;
                int i = group.LineStart;
                int end = (i - 1) + group.LinesCount;

                //Desactivar el modo edición
                for (; i <= end; i++)
                {
                    var line = groupElements[i] as DocumentViewerLine;
                    line.CommitEdition();

                    if (line.LineType == LineType.DataDisplay)
                    {
                        docLines.Add(line.LineKey, line.LineValue);
                    }
                    else if(line.LineType == LineType.NestedObjectOpening)
                    {
                        var document = ParseChildrenDocument(line.FoldingGroup);
                        docLines.Add(line.LineKey, document);
                    }
                }
            }

            BsonDocument doc = new BsonDocument(docLines);

            return doc;
        }

        private void Children_AddNewLine(object sender, LineEventArgs e)
        {
            if(stpLinesContainer.Children.Count > e.LineNumber)
            {
                var parent = sender as DocumentViewerLine;
                BsonValue value = new BsonValue("");
                DocumentViewerLine newLine = new DocumentViewerLine("", value, e.LineNumber + 1, parent.LineTabLength);
                newLine.IsInEditionMode = true;

                if (parent.LineType == LineType.NestedObjectOpening || parent.IsNestedDocumentChildren)
                    newLine.IsNestedDocumentChildren = true;

                stpLinesContainer.Children.Insert(e.LineNumber, newLine);
                var lineNumber = e.LineNumber + 1;

                for(int i = e.LineNumber; i < stpLinesContainer.Children.Count; i++)
                {
                    var line = stpLinesContainer.Children[i] as DocumentViewerLine;
                    line.LineNumber = lineNumber++;
                }

                //Recalcular los grupos de cierre
                for(int i = 0; i < FoldingGroups.Count; i++)
                {
                    if(e.LineNumber + 1 <= FoldingGroups[i].LineStart)
                    {
                        var group = FoldingGroups[i];
                        group.LineStart++;
                        FoldingGroups[i] = group;
                    }
                    else if (e.LineNumber + 1 > FoldingGroups[i].LineStart && FoldingGroups[i].LineStart + FoldingGroups[i].LinesCount >= e.LineNumber)
                    {
                        var group = FoldingGroups[i];
                        group.LinesCount++;
                        FoldingGroups[i] = group;
                    }
                }

                LineCounter++;
            }
        }

        private void ReloadDocument()
        {
            if (Document is BsonDocument)
            {
                var doc = Document as BsonDocument;

                //Buscar el id del documento
                foreach (var line in doc)
                {
                    if(line.Key.Equals("_id"))
                    {
                        var newDoc = DbConnections.CurrentConnection.LiteDatabase.GetCollection(DbConnections.CurrentConnection.EditingCollection).FindById(line.Value);

                        if(newDoc != null)
                        {
                            Document = newDoc;
                            stpLinesContainer.Children.Clear();
                            FoldingGroups.Clear();
                            LineCounter = 0;
                            DocumentNestingLevel = 1;
                            LoadDocument();
                        }

                        break;
                    }
                }
            }
        }

        private void Border_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(!IsEditing && !btnDeleteDocument.IsMouseOver)
            {
                brdControlBorder.BorderBrush = Brushes.CornflowerBlue;
                btnDeleteDocument.Visibility = Visibility.Visible;
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

            btnDeleteDocument.Visibility = Visibility.Hidden;
        }

        private void UserControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ActivateEditionMode();
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
    }
}
