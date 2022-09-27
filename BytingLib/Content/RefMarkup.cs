using BytingLib.Creation;
using BytingLib.Markup;

namespace BytingLib
{
    public class RefMarkup : RefLoca
    {
        private readonly Creator creator;
        public MarkupRoot Markup { get; private set; }

        public RefMarkup(string key, Creator creator, Localization loca) : base(key, loca)
        {
            this.creator = creator;
            Markup = GetMarkup();
        }

        protected override void InitText()
        {
            base.InitText();
            Markup = GetMarkup();
        }

        private MarkupRoot GetMarkup()
        {
            return new MarkupRoot(creator, Text);
        }
    }
}
