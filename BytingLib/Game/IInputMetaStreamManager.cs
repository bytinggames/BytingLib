namespace BytingLib
{
    public interface IInputMetaObjectManager
    {
        void WriteMetaObject(BinaryWriter writer);
        void ReadMetaObject(BinaryReader reader);
        void OnReplayEnd();
    }
}
