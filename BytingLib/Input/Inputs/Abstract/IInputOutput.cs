namespace BytingLib
{
    public interface IInputOutput : IPointerValue
    {
        IEnumerable<InputUpdate> GetChildren();
        void Initialize(InputUpdater updater);
        void Disable();
    }

    public static class InputUpdateExtension
    {
        public static IEnumerable<IInputOutput> GetDependencies(this IInputOutput inputUpdate)
        {
            if (inputUpdate is InputUpdate i)
            {
                foreach (var d in GetDependencies(i))
                {
                    yield return d;
                }
            }
        }

        public static IEnumerable<IInputOutput> GetDependencies(this InputUpdate inputUpdate)
        {
            foreach (var child in inputUpdate.GetChildren())
            {
                if (child is IInputOutput inputOutput)
                {
                    yield return inputOutput;
                    continue;
                }

                foreach (var c in child.GetDependencies())
                {
                    yield return c;
                }
            }
        }
        public static IEnumerable<InputUpdate> GetAllRecursivelyUntilOutput(this IInputOutput inputUpdate)
        {
            if (inputUpdate is InputUpdate i)
            {
                foreach (var d in GetAllRecursivelyUntilOutput(i))
                {
                    yield return d;
                }
            }
        }
        public static IEnumerable<InputUpdate> GetAllRecursivelyUntilOutput(this InputUpdate inputUpdate)
        {
            foreach (var child in inputUpdate.GetChildren())
            {
                if (child is not IInputOutput)
                {
                    foreach (var c in child.GetAllRecursivelyUntilOutput())
                    {
                        yield return c;
                    }
                }
            }
            yield return inputUpdate;
        }

        public static IEnumerable<InputUpdate> GetAllRecursively(this IInputOutput inputUpdate)
        {
            if (inputUpdate is InputUpdate i)
            {
                foreach (var d in GetAllRecursively(i))
                {
                    yield return d;
                }
            }
        }
        public static IEnumerable<InputUpdate> GetAllRecursively(this InputUpdate inputUpdate)
        {
            foreach (var child in inputUpdate.GetChildren())
            {
                foreach (var c in child.GetAllRecursively())
                {
                    yield return c;
                }
            }
            yield return inputUpdate;
        }
    }
}
