using System;
using System.IO;
using System.Collections.Generic;

namespace SimpleSerializer.Channel {
    public class SerializationChannel {

        public delegate void SerializationCallback<in T>(T obj, SerializerStream stream);

        private SerializerStream checksumStream = new SerializerStream(new MemoryStream(), SerializationMode.Serialize);
    
        private Dictionary<Type, (Entry entry, int checksum)> entries = new Dictionary<Type, (Entry entry, int checksum)>();
    
        public void Register<T>(SerializationCallback<T> serializerIn) where T :  class, new() {
            if (entries.ContainsKey(typeof(T))) {
                throw new ArgumentException("Duplicate serialization type");
            }

            var serializer = ConvertSerializer(serializerIn);
            var entry = new Entry(typeof(T), serializer, () => new T());
        
            var dummyValue = new T();
            serializer(dummyValue, checksumStream);
            
            var checksum = checksumStream.SignatureHash;
            checksumStream.ResetHard(SerializationMode.Serialize);
            entries.Add(typeof(T), (entry, checksum));
        }

        public void GetHandshakeData(Stream outputStream) { 
            var serStream = new SerializerStream(outputStream, SerializationMode.Serialize);
            var entryCount = (short) entries.Count;
            serStream.Short(ref entryCount);
        
            foreach (var entry in entries) {
                var name = entry.Key.FullName;
                serStream.String(ref name);
                var checksum = entry.Value.checksum;
                serStream.Int(ref checksum);
            }
        }

        public void ValidateHandshakeData(Stream inputStream) {
            var serStream = new SerializerStream(inputStream, SerializationMode.Deserialize);
            var count = (short)-1;
            serStream.Short(ref count);

            if (entries.Count != count) {
                throw new ApplicationException("Failed to validate checksum: invalid type count");
            }

            for (int i = 0; i < count; i++) {
                var name = "";
                serStream.String(ref name);
                var checksum = 0;
                serStream.Int(ref checksum);

                var type = GetLoadedType(name);
                if (type !=  null && entries.TryGetValue(type, out var entry)) {
                    if (checksum != entry.checksum) {
                        throw new ApplicationException($"Failed to validate checksum: mismatch for { type }");
                    }
                }
                else {
                    throw new ApplicationException("Failed to validate checksum: missing or not registered type");
                }
            }

        }

        private Type GetLoadedType(string typename) {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                var type = assembly.GetType(typename);
                if (type != null) {
                    return type;
                }
            }

            return null;
        }

        public void Serialize<T>(in T value, Stream stream) where T : class {
            if (entries.TryGetValue(typeof(T), out var entry)) {
                var serializationStream = new SerializerStream(stream, SerializationMode.Serialize);
                entry.entry.serializer(value, serializationStream);
            }
            else {
                throw new ApplicationException("Type cannot be (de)serialized by this channel");
            }
        }

        public void Deserialize<T>(out T value, Stream stream) where T : class, new() {
            if (entries.TryGetValue(typeof(T), out var entry)) {
                var serializationStream = new SerializerStream(stream, SerializationMode.Deserialize);
                value = (T)entry.entry.constructor();
                entry.entry.serializer(value, serializationStream);
            }
            else {
                throw new ApplicationException("Type cannot be (de)serialized by this channel");
            }
        }

        private SerializationCallback<object> ConvertSerializer<T>(SerializationCallback<T> serializer) where T : class {
            return (value, stream) => {
                var obj = (T)value;
                serializer.Invoke(obj, stream);
            };
        }
    
        private readonly struct Entry {
            public readonly Type targetType;
            public readonly Func<object> constructor;
            public readonly SerializationCallback<object> serializer;

            public Entry(Type targetType, SerializationCallback<object> serializer, Func<object> constructor) {
                this.targetType = targetType;
                this.constructor = constructor;
                this.serializer = serializer;
            }

            public override int GetHashCode() {
                return targetType.GetHashCode();
            }

            public override bool Equals(object obj) {
                return targetType.Equals(obj);
            }
        }
    }
}