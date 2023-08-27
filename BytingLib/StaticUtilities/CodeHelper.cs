
namespace BytingLib
{
    public static class CodeHelper
    {
        public static void Swap<T>(ref T val1, ref T val2)
        {
            (val1, val2) = (val2, val1);
        }

        public static void ChangeVarTemporarily<T>(ref T variable, T tempValue, Action actionWhile)
        {
            T store = variable;
            variable = tempValue;
            actionWhile();
            variable = store;
        }
        public static void ChangeVarTemporarily<T>(T getVariable, Action<T> setVariable, T tempValue, Action actionWhile)
        {
            T store = getVariable;
            setVariable(tempValue);
            actionWhile();
            setVariable(store);
        }

        public static OnDispose ChangeVarTemporarily<T>(T getVariable, Action<T> setVariable, T tempValue)
        {
            T store = getVariable;
            setVariable(tempValue);

            return new OnDispose(() => setVariable(store));
        }
    }
}
