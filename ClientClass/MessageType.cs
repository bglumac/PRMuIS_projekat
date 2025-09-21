using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ClientClass
{
    [Serializable]
    public class MessageType
    {
        public string Username {  get; set; }
        public DateTime TimeSent { get; set; }
        public string Channel {  get; set; }
        public string Content { get; set; }


        //Serialize
        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        //Deserialize
        public static MessageType ToString(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                return (MessageType)bf.Deserialize(ms);
            }
        }
    }
}
