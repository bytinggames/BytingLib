using System.Collections.Generic;

namespace BytingLib.Test.BytingSerializer
{
    class DataFactory
    {
        public static Data CreateSimple()
        {
            return new Data()
            {
                Objects = new List<object>()
                {
                    new Point(){Pos = new Vector2(2,3) },
                }
            };
        }

        public static Data Create()
        {
            return new Data()
            {
                Objects = new List<object>()
                {
                    new Point(){Pos = new Vector2(2,3) },
                    new Point(){Pos = new Vector2(4,5) },
                    new Line(){Pos1 = new Vector2(6,7), Pos2 = new Vector2(8,9) },
                    new Data()
                    {
                        Objects = new List<object>()
                        {
                            new Point(){Pos = new Vector2(10,11) }
                        }
                    }
                }
            };
        }

        public static Data CreateInterlinked()
        {
            Point p1 = new Point() { Pos = new Vector2(1, 2) };

            return new Data()
            {
                Objects = new List<object>()
                {
                    p1,
                    p1,
                    new Data()
                    {
                        Objects = new List<object>()
                        {
                            p1,
                        }
                    }
                }
            };
        }

        public static Data CreateCircular()
        {
            Data data = new Data();
            data.Objects = new List<object>()
            {
                data
            };
            return data;
        }

        public static Data CreateDerived()
        {
            return new Data()
            {
                 Objects = new()
                 {
                      new LineThick() { Pos1 = new Vector2(2, 3), Pos2 = new Vector2(4, 5), Thickness = 6f }
                 }
            };
        }

        public static Data CreatePrivateProp()
        {
            return new Data()
            {
                Objects = new()
                {
                    new PrivateProp(1)
                }
            };
        }
        public static Data CreatePublicField()
        {
            return new Data()
            {
                Objects = new()
                {
                    new PublicField() { Field = 1 }
                }
            };
        }
        public static Data CreatePrivateField()
        {
            return new Data()
            {
                Objects = new()
                {
                    new PrivateField(1)
                }
            };
        }
    }
}
