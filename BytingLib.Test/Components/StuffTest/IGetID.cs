namespace BytingLib.Test.Components.StuffTest
{
    interface IGetID
    {
        int GetID();
    }

    class ID : IGetID
    {
        private readonly int id;

        public ID(int id)
        {
            this.id = id;
        }

        public int GetID() => id;
    }
}
