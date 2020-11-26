using System;
using Ldb = LiteDB;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDBManager.Structures;
using System.Text.Json;
using System.IO;

namespace LiteDBManager.Services
{
    public class SerializeServices
    {
        public static bool SerializeToJson(string[] collections, string savePath)
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

            var serialized = JsonSerializer.Serialize(colWrapper, typeof(CollectionsWrapperDO));

            return SaveToFile(serialized, savePath);
            
            //Console.WriteLine(serialized);

            //var origWrapper = JsonSerializer.Deserialize(serialized, typeof(CollectionsWrapperDO));
            //var origBson = Ldb.JsonSerializer.Deserialize()

            //var original = Ldb.JsonSerializer.Deserialize(collectionDo.Documents[0]);

            //var json = JsonSerializer.Serialize(collectionDo, typeof(CollectionDO));
        }

        public static CollectionsWrapperDO DeserializeFromJson(string path)
        {
            var json = LoadSavedFile(path);

            if(json != null)
            {
                var origWrapper = (CollectionsWrapperDO) JsonSerializer.Deserialize(json, typeof(CollectionsWrapperDO));

                if(origWrapper != null && origWrapper.Collections.Count > 0)
                {
                    foreach(var collection in origWrapper.Collections)
                    {
                        
                    }
                }
            }

            return null;
        }

        private static bool SaveToFile(string data, string path)
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
