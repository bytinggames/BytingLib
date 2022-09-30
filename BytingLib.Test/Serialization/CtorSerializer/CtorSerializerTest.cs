using BytingLib.DataTypes;
using BytingLib.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytingLib.Test.CtorSerialization
{
    abstract class Entity : ICtorSerializable
    {
        public abstract (byte ID, object Value)[] Serialize();
    }

    abstract class Coin : Entity
    {
        public Vector2 pos;
        public string name;
    }

    class SomeRef
    {

    }

    class Coin1 : Coin
    {

        public Coin1([CS(0)] Vector2 pos, [CS(1)] string name)
        {
            this.pos = pos;
            this.name = name;
        }

        public override (byte ID, object Value)[] Serialize() => new (byte, object)[] { (0, pos), (1, name) };
    }
    class Coin2 : Coin
    {
        public Coin2([CS(0)] Vector2 pos, [CS(1)] string name)
        {
            this.pos = pos;
            this.name = name;
        }

        public override (byte ID, object Value)[] Serialize() => new (byte, object)[] { (1, name), (0, pos) };
    }
    class Coin3 : Coin
    {
        public Coin3([CS(1)] Vector2 pos, [CS(0)] string name)
        {
            this.pos = pos;
            this.name = name;
        }

        public override (byte ID, object Value)[] Serialize() => new (byte, object)[] { (0, name), (1, pos) };
    }
    class Coin4 : Coin
    {
        public Coin4([CS(1)] Vector2 pos, SomeRef someRef, [CS(0)] string name)
        {
            this.pos = pos;
            this.name = name;
            SomeRef = someRef;
        }

        public SomeRef SomeRef { get; }

        public override (byte ID, object Value)[] Serialize() => new (byte, object)[] { (0, name), (1, pos) };
    }

    [TestClass]
    public class CtorSerializerTest
    {
        [TestMethod]
        public void Test()
        {
            SomeRef someRef = new SomeRef();

            List<Coin> coins = new List<Coin>();
            coins.Add(new Coin1(new Vector2(1, 2), "Peter"));
            coins.Add(new Coin2(new Vector2(3, 4), "Hans"));
            coins.Add(new Coin3(new Vector2(5, 6), "Lukas"));
            coins.Add(new Coin4(new Vector2(7, 8), someRef, "Franz"));

            CtorSerializer serializer = new CtorSerializer(new()
            {
                { typeof(Coin1), 0 },
                { typeof(Coin2), 1 },
                { typeof(Coin3), 2 },
                { typeof(Coin4), 3 },
            }, new object[]
            {
                someRef
            });
            Dictionary<Type, int> forward = new()
            {
                { typeof(Vector2), 0 },
                { typeof(string), 1 },
            };
            Dictionary<int, Type> backward = new();
            foreach (var item in forward)
                backward.Add(item.Value, item.Key);

            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            using (BytingWriter bw = new BytingWriter(ms,
                BinaryObjectWriter.WriteFunctions,
                forward
                ))
            {
                serializer.Serialize(bw, coins.Cast<ICtorSerializable>().ToList());
                data = ms.ToArray();
            }

            List<Coin> coinsLoaded;

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BytingReader br = new BytingReader(ms, BinaryObjectReader.ReadFunctions,
                backward, false))
                {
                    coinsLoaded = serializer.Deserialize(br).Cast<Coin>().ToList();
                }
            }

            Assert.AreEqual(coins.Count, coinsLoaded.Count);
            for (int i = 0; i < coins.Count; i++)
            {
                Assert.AreEqual(coins[i].pos.X, coinsLoaded[i].pos.X);
                Assert.AreEqual(coins[i].pos.Y, coinsLoaded[i].pos.Y);
                Assert.AreEqual(coins[i].name, coinsLoaded[i].name);
            }

            Assert.AreEqual(((Coin4)coins[3]).SomeRef, ((Coin4)coinsLoaded[3]).SomeRef);
        }
    }
}
