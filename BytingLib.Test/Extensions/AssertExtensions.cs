
namespace BytingLib.Test
{
    public static class AssertExtensions
    {
        public static void AreNotEqualItems<T>(this Assert _, T[] expected, T[] actual)
        {
            Assert.IsFalse(AreEqualItems(expected, actual));
        }
        public static void AreEqualItems<T>(this Assert _, T[] expected, T[] actual)
        {
            Assert.IsTrue(AreEqualItems(expected, actual));
        }

        private static bool AreEqualItems<T>(T[] expected, T[] actual)
        {
            if (expected.Length != actual.Length)
                return false;

            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] == null)
                {
                    if (actual[i] != null)
                        return false;
                }
                else if (!expected[i]!.Equals(actual[i]))
                    return false;
            }
            return true;
        }
    }
}
