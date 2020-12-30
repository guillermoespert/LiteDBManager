using Ldb = LiteDB;
using LiteDBManager.Services;
using LiteDBManager.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDBManager.Data
{
    /// <summary>
    /// Provee una clase de base para el servicio de serialización y deserialización
    /// de los datos de exportación e importación.
    /// </summary>
    public abstract class Serializer
    {
        public CollectionsWrapperDO CollectionsWrapperDO
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

        public abstract StringWriter Serialize(CollectionsWrapperDO collectionsWrapper);

        public abstract CollectionsWrapperDO Deserialize(string data);

        /// <summary>
        /// Convierte una lista de colecciones a un objeto de envoltura de exportación de datos.
        /// Los documentos dentro de cada colección son serializados a JSON extendido.
        /// </summary>
        /// <param name="collections">lista de colecciones a serializar</param>
        /// <returns>Objeto de exportación de datos</returns>
        public static CollectionsWrapperDO SerializeCollectionsToInternalObject(string[] collections)
        {
            CollectionsWrapperDO colWrapper = new CollectionsWrapperDO();

            foreach (string colName in collections)
            {
                var collection = DbConnections.CurrentConnection.LiteDatabase.GetCollection(colName);
                var documents = new List<Ldb.BsonDocument>(collection.FindAll());
                var collectionDo = new CollectionDO();
                collectionDo.CollectionName = colName;

                foreach (var document in documents)
                {
                    collectionDo.Documents.Add(Ldb.JsonSerializer.Serialize(document));
                }

                colWrapper.Collections.Add(collectionDo);
            }

            return colWrapper;
        }
    }
}
