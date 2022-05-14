using System.Reflection;

namespace BytingLib.Serialization
{
    public class TypeSerializer
    {
        public const BindingFlags BindingFlagsDeclaredAndInherited = BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.Instance;

        public List<List<PropInfoAndID>> PropertyLevels { get; } = new();

        public TypeSerializer(Type type)
        {
            var props = type.GetProperties(BindingFlagsDeclaredAndInherited).Where(f => Attribute.IsDefined(f, typeof(BytingPropAttribute))).ToList();
            
            Type currentLevelType = type;

            List<PropInfoAndID> currentLevel = new();
            PropertyLevels.Add(currentLevel);

            foreach (var p in props)
            {
                while (p.DeclaringType != currentLevelType)
                {
                    // go up one level
                    currentLevel = new();
                    PropertyLevels.Add(currentLevel);
                    currentLevelType = currentLevelType.BaseType!;
                }

                BytingPropAttribute storeMemberAttribute = p.GetCustomAttribute<BytingPropAttribute>()!;
                currentLevel.Add(new PropInfoAndID(p, storeMemberAttribute.ID));
            }

        }
    }
}
