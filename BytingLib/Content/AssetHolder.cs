namespace BytingLib
{
    public class AssetHolder<T>
    {
        private readonly Pointer<T> assetPointer;
        private readonly string assetName;
        private readonly Action<string> onUnusedTo0References;
        private readonly List<Ref<T>> assetReferences = new List<Ref<T>>();

        public AssetHolder(T asset, string assetName, Action<string> onUnusedTo0References)
        {
            assetPointer = new Pointer<T>(asset);
            this.assetName = assetName;
            this.onUnusedTo0References = onUnusedTo0References;
        }

        public string AssetName => assetName;

        public Ref<T> Use()
        {
            Ref<T> assetRef = new Ref<T>(assetPointer, Unuse);
            assetReferences.Add(assetRef);

            if (assetRef == null)
            {
                assetRef = new Ref<T>(assetPointer, Unuse);
            }

            return assetRef;
        }

        private void Unuse(Ref<T> asset)
        {
            assetReferences.Remove(asset);
            if (assetReferences.Count == 0)
            {
                onUnusedTo0References?.Invoke(assetName);
            }
        }

        public T Peek()
        {
            return assetPointer.Value!;
        }

        internal void Replace(T newValue)
        {
            if (newValue is Effect newEffect
                && assetPointer.Value != null)
            {
                Effect? oldEffect = assetPointer.Value as Effect;
                oldEffect?.CopyParametersTo(newEffect);
            }


            AssetDisposer.Dispose(assetPointer.Value);

            assetPointer.Value = newValue;
        }

        internal void TryTriggerOnLoad()
        {
            for (int i = 0; i < assetReferences.Count; i++)
            {
                assetReferences[i].TriggerOnReload();
            }
        }
    }
}
