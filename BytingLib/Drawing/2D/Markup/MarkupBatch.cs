using System;
using System.Collections.Generic;
using System.Text;

namespace BytingLib.Markup
{
    class MarkupBatch
    {
        public MarkupRoot Root { get; }
        public MarkupSettings Settings { get; }

        public MarkupBatch(MarkupRoot root, MarkupSettings settings)
        {
            Root = root;
            Settings = settings;
        }

        public void Draw()
        {
            Root.Draw(Settings);
        }
    }
}
