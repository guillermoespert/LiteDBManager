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
using ICSharpCode.AvalonEdit.Editing;
using LiteDB;
using LiteDBManager.Services;
using LiteDBManager.UIElements.DocumentViewer;

namespace LiteDBManager.UIElements.Pages
{
    /// <summary>
    /// Lógica de interacción para CommandManagementPage.xaml
    /// </summary>
    public partial class CommandManagementPage : Page
    {
        private bool isControlPressed = false;

        public CommandManagementPage()
        {
            InitializeComponent();
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand();
        }

        private void ExecuteCommand()
        {
            var commandString = txtCodeEditor.Text;
            commandString.Trim();

            if (string.IsNullOrWhiteSpace(commandString))
                return;

            var lower = commandString.ToLower();

            if(lower.Equals("clear"))
            {
                stpCommandResults.Children.Clear();
                txtCodeEditor.Text = "";
                return;
            }

            try
            {
                var db = DbConnections.CurrentConnection.LiteDatabase;

                var result = db.Execute(txtCodeEditor.Text);
                var results = new List<BsonValue>(result.ToList());

                var command = new TextBlock();
                command.Text = txtCodeEditor.Text;
                command.FontWeight = FontWeights.Bold;
                command.MouseUp += Command_MouseUp;
                command.Margin = new Thickness(0, 10, 0, 5);
                stpCommandResults.Children.Add(command);

                if(results.Count > 0)
                {
                    foreach (var res in results)
                    {
                        if (res is BsonDocument)
                        {
                            var docControl = new DocumentViewerControl(res as BsonDocument);
                            docControl.Collection = result.Collection;
                            stpCommandResults.Children.Add(docControl);
                        }
                        else if(res is BsonValue)
                        {
                            var response = new TextBlock();

                            if(res.Type == BsonType.Int32 && res.AsInt32 > 0)
                                response.Text = string.Format("La consulta se ha ejecutado correctamente. {0} documentos afectados", res.AsInt32);
                            else if (res.Type == BsonType.Int32 && res.AsInt32 == 0)
                                response.Text = string.Format("La consulta no ha devuelto ningún resultado");

                            stpCommandResults.Children.Add(response);
                        }
                    }
                }
                else
                {
                    var response = new TextBlock();
                    response.Text = string.Format("La consulta no ha devuelto resultados");
                    stpCommandResults.Children.Add(response);
                }

                txtCodeEditor.Text = "";
            }
            catch (LiteException ex)
            {
                var response = new TextBlock();
                response.Text = string.Format("El comando introducido no es válido o su sintáxis es incorrecta: {0}", ex.Message);
                stpCommandResults.Children.Add(response);
            }
            catch
            {
                var response = new TextBlock();
                response.Text = string.Format("Error desconocido al ejecutar el comando");
                stpCommandResults.Children.Add(response);
            }
        }

        private void Command_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var command = sender as TextBlock;
            txtCodeEditor.Text = command.Text;
            ExecuteCommand();
        }

        private void executeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void txtCodeEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.RightCtrl || e.Key == Key.LeftCtrl)
                isControlPressed = true;

            if (e.Key == Key.Enter && isControlPressed)
                ExecuteCommand();
        }

        private void txtCodeEditor_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.RightCtrl || e.Key == Key.LeftCtrl)
                isControlPressed = false;
        }
    }
}
