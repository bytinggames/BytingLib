namespace BytingLib.Test.Components.StuffTest
{
    interface IID
    {
        int GetID();
    }

    class ID : IID
    {
        private readonly int id;

        public ID(int id)
        {
            this.id = id;
        }

        public int GetID() => id;
    }
}
