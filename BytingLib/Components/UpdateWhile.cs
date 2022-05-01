
namespace BytingLib
{

    public class UpdateWhile : IUpdate
    {
        private readonly IStuff parent;
        private readonly Func<bool> updateWhile;

        public UpdateWhile(IStuff parent, Func<bool> updateWhile)
        {
            this.parent = parent;
            this.updateWhile = updateWhile;
        }
        public void Update()
        {
            if (!updateWhile.Invoke())
                parent.Remove(this);
        }
    }
}
