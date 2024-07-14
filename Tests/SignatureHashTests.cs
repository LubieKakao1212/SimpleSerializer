
//TODO Unfinished

using NUnit.Framework;
using SimpleSerializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class SignatureHashTests
    {
        SerializerStream serializer;
        
        [SetUp] 
        public void Setup()
        {
            var buffer = new MemoryStream();
            serializer = new SerializerStream(buffer, SerializationMode.Serialize);
        }

        [TearDown]
        public void TearDown()
        {
            serializer.Dispose();
        }
    }
}
