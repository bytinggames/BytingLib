namespace BytingLib
{
    public static class BinaryWriterReaderExtension
    {
        public static void Write(this BinaryWriter writer, Vector2 vec)
        {
            writer.Write(vec.X);
            writer.Write(vec.Y);
        }
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector3 vec)
        {
            writer.Write(vec.X);
            writer.Write(vec.Y);
            writer.Write(vec.Z);
        }
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Color color)
        {
            writer.Write(color.PackedValue);
        }
        public static Color ReadColor(this BinaryReader reader)
        {
            Color color = new Color(reader.ReadUInt32());
            return color;
        }

        public static void Write(this BinaryWriter writer, Rect rect)
        {
            writer.Write(rect.X);
            writer.Write(rect.Y);
            writer.Write(rect.Size.X);
            writer.Write(rect.Size.Y);
        }
        public static Rect ReadRectangle(this BinaryReader reader)
        {
            return new Rect(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

    }
}
