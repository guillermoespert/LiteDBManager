using LiteDBManager.UIElements;
using System.Windows;
using System.Windows.Controls;

namespace LiteDBManager.Services
{
    public class PageNavigationService
    {
        public static Page CurrentPage { get; private set; }

        /// <summary>
        /// Muestra una página dada en el marco de navegación de la 
        /// ventana principal.
        /// </summary>
        /// <param name="page">Página para mostrar</param>
        public static void ShowPage(Page page)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            mainWindow?.frmDbManager.Navigate(page);
            CurrentPage = page;
        }

        /// <summary>
        /// Abre una colección a través de una página.
        /// Para la conexión activa se busca si la colección tiene
        /// una página abierta. Si existe la página la muestra en el marco.
        /// Si la página no existe la crea y la registra en el diccionario
        /// de páginas de colección.
        /// </summary>
        /// <param name="name"></param>
        public static void OpenCollection(string name)
        {
            DbConnections.CurrentConnection.EditingCollection = name;
            DocumentManagementPage dmp;

            //Búsqueda de la colección en el diccionario de páginas activas
            if (!DbConnections.CurrentConnection.DocumentManagementPages.ContainsKey(name))
            {
                dmp = new DocumentManagementPage();
                DbConnections.CurrentConnection.DocumentManagementPages.Add(name, dmp);
            }
            else
            {
                dmp = DbConnections.CurrentConnection.DocumentManagementPages[name];
            }

            ShowPage(dmp);
        }
    }
}
