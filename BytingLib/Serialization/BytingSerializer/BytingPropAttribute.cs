
namespace BytingLib
{
    public class BytingPropAttribute : Attribute
    {
        public int ID { get; }

        public BytingPropAttribute(int id)
        {
            ID = id;
        }
    }
}
