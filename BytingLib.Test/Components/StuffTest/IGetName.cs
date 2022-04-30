namespace BytingLib.Test.Components.StuffTest
{
    interface IGetName
    {
        string GetName();
    }

    class Name : IGetName
    {
        private readonly string name;

        public Name(string name)
        {
            this.name = name;
        }

        public string GetName() => name;
    }
}
