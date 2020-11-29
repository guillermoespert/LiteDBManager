using LiteDBManager.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ms = System.Xml.Serialization;

namespace LiteDBManager.Data
{
    public class XmlSerializer : Serializer
    {
        public override CollectionsWrapperDO Deserialize(string data)
        {
            var xmlSerializer = new Ms.XmlSerializer(typeof(CollectionsWrapperDO));
            return (CollectionsWrapperDO)xmlSerializer.Deserialize(new StringReader(data));
        }

        public override StringWriter Serialize(CollectionsWrapperDO collectionsWrapper)
        {
            var xmlSerializer = new Ms.XmlSerializer(typeof(CollectionsWrapperDO));
            var strWriter = new StringWriter();

            xmlSerializer.Serialize(strWriter, collectionsWrapper);

            return strWriter;
        }
    }
}
