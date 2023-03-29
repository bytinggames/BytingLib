namespace BytingLib
{
    public static class PreloadAssets
    {
        public static void Preload(object obj)
        {
            var props = obj.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in props)
            {
                prop.GetValue(obj);
            }
        }
    }
}
