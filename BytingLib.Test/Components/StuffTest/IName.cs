namespace BytingLib.Test.Components.StuffTest
{
    interface IName
    {
        string GetName();
    }

    class Name : IName
    {
        private readonly string name;

        public Name(string name)
        {
            this.name = name;
        }

        public string GetName() => name;
    }
}
