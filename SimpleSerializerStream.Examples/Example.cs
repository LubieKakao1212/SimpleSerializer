using SimpleSerializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSerializerStream.Examples
{
    public static class Example
    { 
        static void Main(string[] args)
        {
            var data =  new ExampleStruct() { apple = 42, aFloat = 4.4f };

            //Create the stream in Serializ
            var stream = new SerializerStream(new MemoryStream(), SerializationMode.Serialize);
            //Do serialization
            stream.ExampleStruct(ref data);

            //Rewind and reset the stream to Deserialize Mode
            stream.ResetHard(SerializationMode.Deserialize);
            //Do serialization
            stream.ExampleStruct(ref data);
            
            Console.WriteLine(stream.SignatureHash);
        }

        private static SerializerStream ExampleStruct(this SerializerStream stream, ref ExampleStruct data)
        {
            return stream
                .Int(ref data.apple)
                .Float(ref data.aFloat);
        }
    }

    public struct ExampleStruct
    {
        public int apple;
        public float aFloat;
    }

}
