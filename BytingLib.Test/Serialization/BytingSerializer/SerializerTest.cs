using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BytingLib.Serialization;

namespace BytingLib.Test.BytingSerializer
{
    [TestClass]
    public class SerializerTest
    {
        static Serializer mySerializer = new(new TypeIDs(new Dictionary<Type, int>()
        {
            { typeof(Data), 0 },
            { typeof(Point), 1 },
            { typeof(Line), 2 },
            { typeof(List<object>), 3 },
            { typeof(LineThick), 4 },
        }), false);

        public static IEnumerable<object[]> GetDataNoRef()
        {
            yield return new object[] { DataFactory.CreateSimple() };
            yield return new object[] { DataFactory.Create() };
            yield return new object[] { DataFactory.CreateDerived() };
        }

        public static IEnumerable<object[]> GetData()
        {
            foreach (var item in GetDataNoRef())
                yield return item;
            yield return new object[] { DataFactory.CreateInterlinked() };
            yield return new object[] { DataFactory.CreateCircular() };
        }

        [TestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void TestNewtonsoftJson(Data data1)
        {
            TestSerialization(data1, () =>
            {
                var settings = new Newtonsoft.Json.JsonSerializerSettings()
                {
                    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                    PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data1, settings);
                Console.WriteLine($"json size {json.Length}: " + json);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Data>(json, settings)!;

            });
        }

        [TestMethod]
        [DynamicData(nameof(GetDataNoRef), DynamicDataSourceType.Method)]
        public void TestBytingSerializer(Data data1)
        {
            mySerializer.References = false;
            TestBytingSerializerForReal(data1);
        }

        [TestMethod]
        public void TestBytingSerializerNotWorkingOnRefs()
        {
            mySerializer.References = false;
            Assert.ThrowsException<AssertFailedException>(() =>
                TestBytingSerializerForReal(DataFactory.CreateInterlinked()));
        }

        [TestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void TestBytingSerializerRef(Data data1)
        {
            mySerializer.References = true;
            TestBytingSerializerForReal(data1);
        }

        private void TestBytingSerializerForReal(Data data1)
        {
            TestSerialization(data1, () =>
            {
                byte[] data;
                using (MemoryStream stream = new())
                {
                    mySerializer.Serialize(stream, data1);
                    data = stream.ToArray();
                }
                Console.WriteLine("data size: " + data.Length);
                using (MemoryStream stream = new(data))
                {
                    return mySerializer.Deserialize<Data>(stream);
                }
            });
        }

        private void TestSerialization(Data data1, Func<Data> doSerialization)
        {
            string str1 = data1.ToString();

            Data data2 = doSerialization();

            string str2 = data2.ToString();

            Assert.AreEqual(str1, str2);

            int count1 = data1.GetAllObjects().Distinct().Count();
            int count2 = data2.GetAllObjects().Distinct().Count();
            Assert.AreEqual(count1, count2);
        }
    }
}