namespace BytingLib.Test.Components.StuffTest
{
    class NameID : IGetName, IGetID
    {
        private readonly int id;
        private readonly string name;

        public NameID(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public string GetName() => name;

        public int GetID() => id;
    }
}
