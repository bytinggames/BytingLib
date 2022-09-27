namespace BytingLib
{
    public class RefLoca : IDisposable
    {
        private readonly string key;
        private readonly Localization loca;

        public string Text { get; private set; }

        public RefLoca(string key, Localization loca)
        {
            this.key = key;
            this.loca = loca;
            loca.SubscribeToReload(InitText);
            Text = GetText();
        }

        public void Dispose()
        {
            loca.UnsubscribeToReload(InitText);
        }

        protected virtual void InitText()
        {
            Text = GetText();
        }
        private string GetText()
        {
            return loca.Get(key);
        }
    }
}
