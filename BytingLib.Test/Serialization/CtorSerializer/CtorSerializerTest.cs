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
        public abstract CtorData Serialize();
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

        public override CtorData Serialize() => new((0, pos), (1, name));
        public Coin1([CS(0)] Vector2 pos, [CS(1)] string name)
        {
            this.pos = pos;
            this.name = name;
        }

    }
    class Coin2 : Coin
    {
        public override CtorData Serialize() => new((1, name), (0, pos));
        public Coin2([CS(0)] Vector2 pos, [CS(1)] string name)
        {
            this.pos = pos;
            this.name = name;
        }

    }
    class Coin3 : Coin
    {
        public override CtorData Serialize() => new((0, name), (1, pos));
        public Coin3([CS(1)] Vector2 pos, [CS(0)] string name)
        {
            this.pos = pos;
            this.name = name;
        }

    }
    class Coin4 : Coin
    {
        public override CtorData Serialize() => new ((0, name), (1, pos));
        public Coin4([CS(1)] Vector2 pos, SomeRef someRef, [CS(0)] string name)
        {
            this.pos = pos;
            this.name = name;
            SomeRef = someRef;
        }

        public SomeRef SomeRef { get; }

    }
    class Coin1_1 : Coin1
    {
        public override CtorData Serialize() => new((0, pos), (1, name));
        public Coin1_1([CS(0)] Vector2 pos, [CS(1)] string name) : base(pos, name)
        {
        }
    }
    class Coin1_2 : Coin1
    {
        public readonly string surname;

        public override CtorData Serialize() => new((0, pos), (1, name), (2, surname));
        public Coin1_2([CS(0)] Vector2 pos, [CS(1)] string name, [CS(2)] string surname) : base(pos, name)
        {
            this.surname = surname;
        }
    }
    class Coin1_3 : Coin1
    {
        public override CtorData Serialize() => new((0, pos));
        public Coin1_3([CS(0)] Vector2 pos) : base(pos, "Always Coin1_3")
        {
        }
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
            coins.Add(new Coin1_1(new Vector2(9, 10), "Jens"));
            coins.Add(new Coin1_2(new Vector2(9, 10), "Jens", "Maier"));
            coins.Add(new Coin1_3(new Vector2(9, 10)));

            CtorSerializer serializer = new CtorSerializer(new()
            {
                { typeof(Coin1), 0 },
                { typeof(Coin2), 1 },
                { typeof(Coin3), 2 },
                { typeof(Coin4), 3 },
                { typeof(Coin1_1), 4 },
                { typeof(Coin1_2), 5 },
                { typeof(Coin1_3), 6 },
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

                if (coins[i] is Coin4)
                    Assert.AreEqual(((Coin4)coins[i]).SomeRef, ((Coin4)coinsLoaded[i]).SomeRef);
                if (coins[i] is Coin1_2)
                    Assert.AreEqual(((Coin1_2)coins[i]).surname, ((Coin1_2)coinsLoaded[i]).surname);
            }
        }
    }
}
