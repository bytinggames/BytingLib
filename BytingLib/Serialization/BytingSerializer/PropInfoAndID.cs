using System.Reflection;

namespace BytingLib.Serialization
{
    public struct PropInfoAndID
    {
        public PropertyInfo Prop;
        public int ID;

        public PropInfoAndID(PropertyInfo prop, int iD)
        {
            Prop = prop;
            ID = iD;
        }
    }
}
