using Ldb = LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LiteDBManager.Data;
using LiteDBManager.Structures;

namespace LiteDBManager.Services
{
    /// <summary>
    /// Provee una serie de métodos para el tratamiento de datos
    /// tanto de importación como de exportación.
    /// </summary>
    public class DataOperationsService
    {
        public CollectionsWrapperDO CollectionsWrapperDO
        {
            get => default;
            set
            {
            }
        }

        public DataOperations DataOperations
        {
            get => default;
            set
            {
            }
        }

        public CollectionDO CollectionDO
        {
            get => default;
            set
            {
            }
        }

        public Serializer Serializer
        {
            get => default;
            set
            {
            }
        }

        /// <summary>
        /// Importa todos los datos definidos en el archivo al que hace 
        /// referencia el parámetro <code>path</code>.
        /// </summary>
        /// <param name="path">Ruta del archivo a importar</param>
        /// <returns>true en caso de éxito o false en cualquier otro caso.</returns>
        public static bool ImportData(string path, DataOperations operation)
        {
            var extension = Path.GetExtension(path);

            if (extension != null && extension != string.Empty)
            {
                var data = LoadSavedFile(path);

                if (extension.Equals(".json"))
                {
                    var json = new JsonSerializer();
                    return ImportData(json, data, operation);
                }
                else if (extension.Equals(".xml"))
                {
                    var xml = new XmlSerializer();
                    return ImportData(xml, data, operation);
                }
            }

            return false;
        }

        private static bool ImportData(Serializer serializer, string strData, DataOperations operation)
        {
            var data = serializer.Deserialize(strData);

            if (data != null)
            {
                if (data.Collections.Count > 0)
                {
                    //Averiguar el tipo de importación: de lista de colecciones o de lista de documentos
                    if(data.Collections.Count == 1 && string.IsNullOrWhiteSpace(data.Collections[0].CollectionName))
                    {
                        //Importación de una colección de documentos
                        if (operation == DataOperations.ImportDocuments)
                            return ImportDocumentsData(data);
                        else
                            MessageBox.Show(MainService.MainWindow, "Los datos contenidos en el archivo no son campatibles con el tipo de importación solicitada.", "Error de archivo", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        //Importación de lista de colecciones
                        if(operation == DataOperations.ImportCollections)
                            return ImportCollectionsData(data);
                    }
                }
                else
                {
                    MessageBox.Show(MainService.MainWindow, "No se han podido cargar datos del archivo seleccionado. Por favor asegúrese que el archivo seleccionado es un archivo de exportación de datos de LiteDBManager.", "Error de carga de archivo", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }

            return false;
        }

        private static bool ImportCollectionsData(CollectionsWrapperDO data)
        {
            var result = MessageBox.Show(MainService.MainWindow, "Se importarán " + data.Collections.Count + " colecciones definidas en el archivo de importación. Si la base de datos contiene alguna colección con el mismo nombre se le preguntará si quiere sobreescribirla.\n\n¿Desea continuar?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int importedCollections = 0;
                int ignoredCollections = 0;
                int errors = 0;

                foreach (var collection in data.Collections)
                {
                    if (collection.CollectionName != null)
                    {
                        var db = DbConnections.CurrentConnection.LiteDatabase;

                        if (db.CollectionExists(collection.CollectionName))
                        {
                            result = MessageBox.Show(MainService.MainWindow, "La colección '" + collection.CollectionName + "' ya existe. Puede importar los datos existentes en el archivo de importación sobreescribiendo los datos presentes en la base de datos. Esta acción no se puede deshacer.\n\n¿Desea sobreescribir la versión actual de la base de datos con los datos del archivo de importación?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                //Eliminar la colección para permitir la inserción de la nueva
                                db.DropCollection(collection.CollectionName);
                            }
                            else
                            {
                                ignoredCollections++;
                                continue;
                            }
                        }

                        //Insertar la nueva colección
                        if (SqlServices.CreateNewColection(collection.CollectionName))
                        {
                            var dbCollection = db.GetCollection(collection.CollectionName);

                            foreach (var document in collection.Documents)
                            {
                                var doc = Ldb.JsonSerializer.Deserialize(document);
                                dbCollection.Insert((Ldb.BsonDocument)doc);
                            }

                            importedCollections++;

                            //Si se ha sobreescrito una colección existente se actualiza la página de documentos para reflejar el nuevo
                            //estado de la colección.
                            if (DbConnections.CurrentConnection.DocumentManagementPages.ContainsKey(collection.CollectionName))
                            {
                                DbConnections.CurrentConnection.DocumentManagementPages[collection.CollectionName].LoadDocuments(collection.CollectionName);
                            }
                        }
                        else
                        {
                            errors++;
                        }
                    }
                    else
                    {
                        ignoredCollections++;
                    }
                }

                var message = string.Format("Se han insertado {0} colecciones, se han ignorado {1} colecciones y han habido {2} errores", importedCollections, ignoredCollections, errors);
                MessageBox.Show(MainService.MainWindow, message, "Resultado de la importación", MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }

            return false;
        }

        private static bool ImportDocumentsData(CollectionsWrapperDO data)
        {
            var result = MessageBox.Show(MainService.MainWindow, "Se importarán " + data.Collections[0].Documents.Count + " documentos definidos en el archivo de importación. Si la base de datos contiene algun documento con el mismo identificador este será sobreescrito.\n\n¿Desea continuar?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int importedDocuments = 0;

                var db = DbConnections.CurrentConnection.LiteDatabase;
                var collection = DbConnections.CurrentConnection.EditingCollection;

                var dbCollection = db.GetCollection(collection);

                foreach (var document in data.Collections[0].Documents)
                {
                    Ldb.BsonDocument doc = (Ldb.BsonDocument)Ldb.JsonSerializer.Deserialize(document);

                    if(doc.Keys.Contains("_id"))
                    {
                        var query = DbConnections.CurrentConnection.LiteDatabase.Execute(string.Format("SELECT $ FROM {0} WHERE _id = {1}", collection, doc["_id"]));

                        if(!query.HasValues)
                        {
                            dbCollection.Insert(doc);
                        }
                        else
                        {
                            dbCollection.Update(doc);
                        }
                    }
                    else
                    {
                        dbCollection.Update(doc);
                    }

                    importedDocuments++;
                }

                //Si se ha sobreescrito una colección existente se actualiza la página de documentos para reflejar el nuevo
                //estado de la colección.
                if (DbConnections.CurrentConnection.DocumentManagementPages.ContainsKey(collection))
                {
                    DbConnections.CurrentConnection.DocumentManagementPages[collection].LoadDocuments(collection);
                }


                var message = string.Format("Se han insertado {0} documentos.", importedDocuments);
                MessageBox.Show(MainService.MainWindow, message, "Resultado de la importación", MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Exporta una lista de colecciones que serán serializadas completamente.
        /// Estas colecciones están serializadas previamente.
        /// </summary>
        /// <param name="collections">Lista de colecciones a serializar</param>
        /// <param name="path">Archivo donde se alamacenará la exportación</param>
        /// <returns>true si la exportación fue correcta o false en caso contrario</returns>
        public static bool ExportData(string[] collections, string path) => ExportData(Serializer.SerializeCollectionsToInternalObject(collections), path);

        /// <summary>
        /// Serializa un objeto de serialización y lo almacena en el archivo definido
        /// por path
        /// </summary>
        /// <param name="data">Objeto de exportación</param>
        /// <param name="path">Ruta de almacenamiento</param>
        /// <returns>true si la exportación fue correcta o false en caso contrario</returns>
        public static bool ExportData(CollectionsWrapperDO data, string path)
        {
            var extension = Path.GetExtension(path);

            if (extension != null && extension != string.Empty)
            {
                if (extension.Equals(".json"))
                {
                    var json = new JsonSerializer();
                    return ExportAndSaveData(json, data, path);
                }
                else if (extension.Equals(".xml"))
                {
                    var xml = new XmlSerializer();
                    return ExportAndSaveData(xml, data, path);
                }
            }

            return false;
        }

        private static bool ExportAndSaveData(Serializer serializer, CollectionsWrapperDO data, string path)
        {
            var serialized = serializer.Serialize(data);

            if (serialized != null)
            {
                return SaveToFile(serialized, path);
            }

            return false;
        }

        private static bool SaveToFile(StringWriter data, string path)
        {
            try
            {
                var writer = File.CreateText(path);
                writer.Write(data);
                writer.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string LoadSavedFile(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch
            {
                return null;
            }
        }
    }
}
