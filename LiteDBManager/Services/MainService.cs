using LiteDBManager.Structures;
using LiteDBManager.UIElements;
using LiteDB;
using System.Windows;
using System.Windows.Media;

namespace LiteDBManager.Services
{
    public class MainService
    {
        public static MainWindow MainWindow { get { return Application.Current.MainWindow as MainWindow; } }

        /// <summary>
        /// Realiza todas las operaciones para crear una nueva conexión con una base de datos.
        /// Actualiza todos los elementos de la IU así como cualquier estructura de datos.
        /// Este método necesita un ConnectionString para añadir la nueva conexión.
        /// No importa si la operación es de apertura de archivo existente o bien de
        /// la creación de una nueva base de datos.
        /// </summary>
        /// <param name="connString"></param>
        public static void AddNewConnection(ConnectionString connString)
        {
            var main = Application.Current.MainWindow as MainWindow;

            DbConnection dbConnection = new DbConnection(connString);

            DbConnections.Connections.Add(dbConnection);
            DbConnections.CurrentConnection = dbConnection;

            DbConnectionControl dbcon = new DbConnectionControl(dbConnection);

            main.stpConnections.Children.Add(dbcon);
            main.expConnections.IsExpanded = true;
        }

        /// <summary>
        /// Actualiza el control de lista de conexiones presente en la
        /// ventana principal. Refleja los cambios realizados en las 
        /// colecciones de una conexión.
        /// </summary>
        public static void UpdateCollections()
        {
            var main = Application.Current.MainWindow as MainWindow;

            main?.stpCollections.Children.Clear();
            
            foreach(var collection in DbConnections.CurrentConnection.LiteDatabase.GetCollectionNames())
            {
                DbCollectionControl dbcol = new DbCollectionControl(collection);

                main.stpCollections.Children.Add(dbcol);

                main.expCollections.IsExpanded = true;
            }
        }

        /// <summary>
        /// Pone todas la conexiones con fondo blanco.
        /// Un fondo verde significa conexión activa. Sin embargo,
        /// este fondo diferenciado solo tiene una utilidad visual
        /// para marcar la conexión activa en ese momento.
        /// </summary>
        public static void UnselectAllConnections() //-> Visual
        {
            var main = Application.Current.MainWindow as MainWindow;

            foreach(var child in main.stpConnections.Children )
            {
                var connection = child as DbConnectionControl;
                
                if(connection != null)
                {
                    connection.Background = Brushes.White;
                }
            }
        }
    }
}
