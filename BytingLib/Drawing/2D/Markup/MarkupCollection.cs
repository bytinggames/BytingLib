namespace BytingLib.Markup
{
    public class MarkupCollection : IBranch
    {
        public List<INode> Children { get; } = new List<INode>();

        public virtual IEnumerable<ILeaf> IterateOverLeaves(MarkupSettings settings)
        {
            foreach (var child in Children)
            {
                if (child is ILeaf leaf)
                {
                    yield return leaf;
                }
                else if (child is IBranch branch)
                {
                    foreach (var collectionChild in branch.IterateOverLeaves(settings))
                    {
                        yield return collectionChild;
                    }
                }
                else
                {
                    throw new Exception(child.GetType() + " node must be either leaf or branch");
                }
            }
        }

        public IEnumerable<INode> AllChildren() => AllChildren(this);
        public static IEnumerable<INode> AllChildren(MarkupCollection node)
        {
            foreach (var child in node.Children)
            {
                yield return child;
                if (child is MarkupCollection branch)
                {
                    foreach (var collectionChild in AllChildren(branch))
                    {
                        yield return collectionChild;
                    }
                }
            }
        }

        public IEnumerable<ILeaf> AllLeaves() => AllLeaves(this);
        public static IEnumerable<ILeaf> AllLeaves(MarkupCollection node)
        {
            foreach (var child in node.Children)
            {
                if (child is MarkupCollection branch)
                {
                    foreach (var leaf in AllLeaves(branch))
                    {
                        yield return leaf;
                    }
                }
                else if (child is ILeaf leaf)
                {
                    yield return leaf;
                }
            }
        }

        public MarkupCollection(Creator creator, string text)
        {
            ScriptReaderLiteral reader = new ScriptReaderLiteral(text);

            INode? element;
            while ((element = ReadElement(creator, reader)) != null)
            {
                Children.Add(element);
            }
        }

        public MarkupCollection(params INode[] children)
        {
            Children = children.ToList();
        }

        private static INode? ReadElement(Creator creator, ScriptReaderLiteral reader)
        {
            char? peek = reader.Peek();
            if (!peek.HasValue)
            {
                return null;
            }

            switch (peek)
            {
                case '#':
                    reader.ReadChar(); // read in '#'
                    return creator.CreateObject<INode>(reader);

                case '\n':
                    reader.ReadChar(); // read in '\n'
                    return new MarkupNewLine();

                default:
                    creator.AutoParameters.TryGetValue(typeof(IStringEdit), out object? editObj);
                    IStringEdit? edit = (IStringEdit?)editObj;
                    return new MarkupText(reader,  edit);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Dispose();
            }
        }

        public override string ToString()
        {
            string s = "[ ";
            if (Children.Count > 0)
            {
                s += Children[0].ToString();
                for (int i = 1; i < Children.Count; i++)
                {
                    s += ", " + Children[i].ToString();
                }
            }
            s += " ]";
            return s;
        }
    }
}
