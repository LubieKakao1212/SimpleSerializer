using NUnit.Framework;
using NUnit.Framework.Internal.Commands;
using SimpleSerializer;
using System;
using System.Drawing.Printing;
using System.IO;

namespace Tests
{
    public class SerializeDeserialize
    {
        private delegate SerializerStream Serialization<T>(ref T val);
        private delegate SerializerStream ResSerialization<T>(ref T val, T resolution);

        SerializerStream serializer;
        
        [SetUp]
        public void Setup()
        {
            var buffer = new MemoryStream();
            serializer = new SerializerStream(buffer, SerializationMode.Serialize);
        }

        #region Integers
        #region 1
        [Test]
        public void Byte()
        {
            byte value = 0b10101001;

            DoTestSimple(value, serializer.Byte);
        }

        [Test]
        public void SByte()
        {
            sbyte value = -0b00101001;
            
            DoTestSimple(value, serializer.SByte);
        }
        #endregion

        #region 2
        [Test]
        public void UShort()
        {
            ushort value = 0xff1c;

            DoTestSimple(value, serializer.UShort);
        }

        [Test]
        public void Short()
        {
            short value = -0x7f1c;

            DoTestSimple(value, serializer.Short);
        }
        #endregion

        #region 4
        [Test]
        public void UInt()
        {
            uint value = 0xff1c2d47;


            DoTestSimple(value, serializer.UInt);
        }

        [Test]
        public void Int()
        {
            int value = -0x7f1c2d47;

            DoTestSimple(value, serializer.Int);
        }
        #endregion

        #region 8
        [Test]
        public void ULong()
        {
            ulong value = 0xff1c2d47dfc82baa;

            DoTestSimple(value, serializer.ULong);
        }

        [Test]
        public void Long()
        {
            long value = -0x7f1c2d47dfc82baa;

            DoTestSimple(value, serializer.Long);
        }
        #endregion
        #endregion

        #region Floats
        #region Single
        private const float valF = 16f / 3f;
        private const double valD = 16d / 3d;

        [TestCase(valF)]
        [TestCase(-valF)]
        public void Float(float value)
        {
            DoTestSimple(value, serializer.Float);
        }

        [TestCase(valF, 1f)]
        [TestCase(valF, 10f)]
        [TestCase(valF, 16f)]
        [TestCase(valF, 30.3f)]
        [TestCase(-valF, 1f)]
        [TestCase(-valF, 10f)]
        [TestCase(-valF, 16f)]
        [TestCase(-valF, 30.3f)]
        public void IFloat16(float value, float resolution)
        {
            DoResolutionTest(value, resolution, serializer.IFloat16);
        }

        [TestCase(valF, 1f)]
        [TestCase(valF, 10f)]
        [TestCase(valF, 16f)]
        [TestCase(valF, 30.3f)]
        [TestCase(-valF, 1f)]
        [TestCase(-valF, 10f)]
        [TestCase(-valF, 16f)]
        [TestCase(-valF, 30.3f)]
        public void IFloat8(float value, float resolution)
        {
            DoResolutionTest(value, resolution, serializer.IFloat8);
        }
        #endregion

        #region Double
        [TestCase(valD)]
        [TestCase(-valD)]
        [Test]
        public void Double(double value)
        {
            DoTestSimple(value, serializer.Double);
        }

        [TestCase(valD, 1f)]
        [TestCase(valD, 10f)]
        [TestCase(valD, 16f)]
        [TestCase(valD, 30.3f)]
        [TestCase(-valD, 1f)]
        [TestCase(-valD, 10f)]
        [TestCase(-valD, 16f)]
        [TestCase(-valD, 30.3f)]
        public void IDouble32(double value, double resolution)
        {
            DoResolutionTest(value, resolution, serializer.IDouble32);
        }
        #endregion
        #endregion

        [TearDown]
        public void TearDown()
        {
            serializer.Dispose();
        }

        private T Resolution<T>(T value, T resolution) where T : struct, IConvertible
        {
            var v = Convert.ToDouble(value);
            var r = Convert.ToDouble(resolution);
            var v1 = Math.Floor(v * r);
            v1 += v1 < 0 ? 1 : 0;
            var result = v1 / r;
            return (T)Convert.ChangeType(result, typeof(T));
        }

        private void DoTestSimple<T>(T value, Serialization<T> serialize) where T : struct
        {
            serialize(ref value);
            var hash = serializer.SignatureHash;
            serializer.ResetHard(SerializationMode.Deserialize);

            var result = default(T);
            serialize(ref result);
            Assert.AreEqual(hash, serializer.SignatureHash);
            Assert.AreEqual(value, result);
        }

        private void DoResolutionTest<T>(T value, T resolution, ResSerialization<T> serilaize) where T : struct, IConvertible
        {
            serilaize(ref value, resolution);
            var hash = serializer.SignatureHash;
            serializer.ResetHard(SerializationMode.Deserialize);

            var result = default(T);
            serilaize(ref result, resolution);
            Assert.AreEqual(hash, serializer.SignatureHash);
            
            Assert.AreEqual(Resolution(value, resolution), result);
        }
    }
}