namespace BytingLib
{
    public interface INodeContainer
    {
        public List<NodeGL> Children { get; }
    }

    public static class INodeContainerExtension
    {
        public static IEnumerable<NodeGL> GetNodes(this INodeContainer container)
        {
            for (int i = 0; i < container.Children.Count; i++)
            {
                yield return container.Children[i];

                foreach (var n in container.Children[i].GetNodes())
                    yield return n;
            }
        }

        public static IEnumerable<NodeGL> GetNodes(this INodeContainer container, Func<NodeGL, bool> goDeeper)
        {
            for (int i = 0; i < container.Children.Count; i++)
            {
                if (!goDeeper(container.Children[i]))
                    continue;

                yield return container.Children[i];

                foreach (var n in container.Children[i].GetNodes(goDeeper))
                    yield return n;
            }
        }
    }
}
