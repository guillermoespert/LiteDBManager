using LiteDBManager.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ms = System.Text.Json;

namespace LiteDBManager.Data
{
    public class JsonSerializer : Serializer
    {
        public override CollectionsWrapperDO Deserialize(string data)
        {
            var origWrapper = (CollectionsWrapperDO)Ms.JsonSerializer.Deserialize(data, typeof(CollectionsWrapperDO));

            if (origWrapper != null && origWrapper.Collections.Count > 0)
            {
                return origWrapper;
            }

            return null;
        }

        public override StringWriter Serialize(CollectionsWrapperDO collectionsWrapper)
        {
            return new StringWriter(new StringBuilder(Ms.JsonSerializer.Serialize(collectionsWrapper, typeof(CollectionsWrapperDO))));
        }
    }
}
