using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Text.Json;

namespace BytingLib
{
    public class AnimationData
    {
        public Dictionary<string, Frame> frames { get; set; }

        public Meta meta { get; set; }

        public int TotalDuration { get; private set; }

        public void Initialize()
        {
            int time = 0;
            foreach (var f in frames.Values)
            {
                //f.timestamp = time;
                time += f.duration;
            }

            TotalDuration = time;

            if (meta?.frameTags != null)
            {
                foreach (var tag in meta.frameTags)
                {
                    tag.TotalDuration = frames.Skip(tag.from).Take(tag.to - tag.from + 1).Sum(f => f.Value.duration);
                }
            }
        }

        private Rectangle GetSourceRectangle(long time)
        {
            time %= TotalDuration;
            foreach (var f in frames.Values)
            {
                time -= f.duration;
                if (time < 0)
                    return f.rectangle;
            }

            throw new Exception();
        }

        public Rectangle GetSourceRectangle(long time, string animationTagName)
        {
            if (animationTagName == null)
                return GetSourceRectangle(time);

            var tag = GetFrameTag(animationTagName);

            return GetSourceRectangle(time, tag);
        }

        public Rectangle GetSourceRectangle(long time, Meta.FrameTag frameTag)
        {
            if (frameTag == null)
                return GetSourceRectangle(time);

            int tagFramesCount = frameTag.to - frameTag.from + 1;
            int tagTotalDuration = frameTag.TotalDuration;

            time %= tagTotalDuration;

            if (frameTag.direction != "forward")
                time = tagTotalDuration - time; // reverse time

            foreach (var f in frames.Values.Skip(frameTag.from).Take(tagFramesCount))
            {
                time -= f.duration;
                if (time < 0)
                    return f.rectangle;
            }

            throw new Exception();
        }

        public Meta.FrameTag GetFrameTag(string animationTagName)
        {
            if (meta == null)
                throw new Exception("meta is null");
            if (meta.frameTags == null)
                throw new Exception("meta.frameTags is null");
            var tag = meta.frameTags.Find(f => f.name == animationTagName);
            if (tag == null)
                throw new Exception("couldn't find tag " + animationTagName);
            return tag;
        }

        public class Frame
        {
            public string name { get; set; }
            public Rectangle rectangle { get; set; }
            public int duration { get; set; }
            //public int timestamp { get; internal set; }

            public Rect frame
            {
                set
                {
                    rectangle = new Rectangle(value.x, value.y, value.w, value.h);
                }
            }

        }

        public class Rect
        {
            public int x { get; set; }
            public int y { get; set; }
            public int w { get; set; }
            public int h { get; set; }
        }

        public class Meta
        {
            public List<FrameTag> frameTags { get; set; }

            public class FrameTag
            {
                public string name { get; set; }
                public int from { get; set; }
                public int to { get; set; }
                public string direction { get; set; }
                public int TotalDuration { get; set; }
            }
        }


        public static AnimationData FromJson(string json)
        {
            var data = JsonSerializer.Deserialize<AnimationData>(json);
            data.Initialize();
            return data;
        }

        static Dictionary<string, AnimationData> animationDatas = new Dictionary<string, AnimationData>();

        public static AnimationData GetAnimationData(ContentManager content, string assetName)
        {
            if (animationDatas.ContainsKey(assetName))
                return animationDatas[assetName];
            
            string file = Path.Combine(content.RootDirectory, assetName.Replace('/', '\\') + ".json");
            string json = File.ReadAllText(file);
            AnimationData data = FromJson(json);
            animationDatas.Add(assetName, data);
            return data;
        }
    }

}
