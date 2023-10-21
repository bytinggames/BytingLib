
namespace BytingLib
{
    public abstract class MeasureDurations : IDraw
    {
        Dictionary<string, MeasureDurationItem> measurements = new();
        int stack = 0;

        public IDisposable Measure(string name)
        {
            MeasureDurationItem? m;
            if (!measurements.TryGetValue(name, out m))
            {
                m = new MeasureDurationItem(stack);
                measurements.Add(name, m);
            }
            m.MeasureBegin();

            stack++;


            return new OnDispose(() => { m.MeasureEnd(); stack--; });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string textNumbers = "";
            string textNames = "";
            foreach (var m in measurements)
            {
                textNumbers += m.Value.StackStr + m.Value.GetAverageMS().ToString("N1") + "\n";
                textNames += m.Value.StackStr + "    - " + m.Key + "\n";
            }
            if (textNumbers.Length > 0)
            {
                textNumbers = textNumbers.Remove(textNumbers.Length - 1);
            }

            if (textNames.Length > 0)
            {
                textNames = textNames.Remove(textNames.Length - 1);
            }

            DrawSelf(spriteBatch, textNumbers, textNames);
        }

        protected abstract void DrawSelf(SpriteBatch spriteBatch, string textNumbers, string textNames);
    }
}