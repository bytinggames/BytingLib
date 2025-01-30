namespace BytingLib
{
    public interface IPointerValue
    {
        void SetPointerValue(object obj);
        object? GetPointerValue();
        Type GetDeclaredPointerValueType();
    }
}
