
namespace BytingLib
{
    public static class CodeHelper
    {
        public static void Swap<T>(ref T val1, ref T val2)
        {
            T val3 = val1;
            val1 = val2;
            val2 = val3;
        }

        public static void ChangeVarTemporarily<T>(ref T variable, T tempValue, Action actionWhile)
        {
            T store = variable;
            variable = tempValue;
            actionWhile();
            variable = store;
        }
    }
}
