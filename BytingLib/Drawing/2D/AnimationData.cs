using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Text.Json;

namespace BytingLib
{
    public class AnimationData
    {
        public Dictionary<string, Frame>? frames { get; set; }

        public Meta? meta { get; set; }

        public int TotalDuration { get; private set; }

        public void Initialize()
        {
            int ms = 0;
            foreach (var f in frames!.Values)
            {
                //f.timestamp = ms;
                ms += f.duration;
            }

            TotalDuration = ms;

            if (meta?.frameTags != null)
            {
                foreach (var tag in meta.frameTags)
                {
                    tag.TotalDuration = frames.Skip(tag.from).Take(tag.to - tag.from + 1).Sum(f => f.Value.duration);
                }

                meta.InitializeFrameTagsDictionary();
            }
        }

        private Rectangle GetSourceRectangle(long ms)
        {
            ms %= TotalDuration;
            foreach (var f in frames!.Values)
            {
                ms -= f.duration;
                if (ms < 0)
                    return f.rectangle;
            }

            throw new Exception();
        }

        public Rectangle GetSourceRectangle(double ms, string? animationTagName)
            => GetSourceRectangle(Convert.ToInt64(ms), animationTagName);
        public Rectangle GetSourceRectangle(long ms, string? animationTagName)
        {
            if (animationTagName == null)
                return GetSourceRectangle(ms);

            var tag = GetFrameTag(animationTagName);

            return GetSourceRectangle(ms, tag);
        }


        public Rectangle GetSourceRectangle(double ms, Meta.FrameTag frameTag)
            => GetSourceRectangle(Convert.ToInt64(ms), frameTag);
        public Rectangle GetSourceRectangle(long ms, Meta.FrameTag frameTag)
        {
            if (frameTag == null)
                return GetSourceRectangle(ms);

            int tagFramesCount = frameTag.to - frameTag.from + 1;
            int tagTotalDuration = frameTag.TotalDuration;

            ms %= tagTotalDuration;

            if (frameTag.direction != "forward")
                ms = tagTotalDuration - ms; // reverse time

            foreach (var f in frames!.Values.Skip(frameTag.from).Take(tagFramesCount))
            {
                ms -= f.duration;
                if (ms < 0)
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
            if (meta.frameTagsDictionary == null)
                throw new Exception("meta.frameTagsDictionary is null");
            if (!meta.frameTagsDictionary.TryGetValue(animationTagName, out var tag))
                throw new Exception("couldn't find tag " + animationTagName);
            return tag;
        }

        public class Frame
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public string name { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public Rectangle rectangle { get; set; }
            public int duration { get; set; }
            //public int timestamp { get; internal set; }

            public _Rect frame
            {
                set
                {
                    rectangle = new Rectangle(value.x, value.y, value.w, value.h);
                }
            }

        }

        public class _Rect
        {
            public int x { get; set; }
            public int y { get; set; }
            public int w { get; set; }
            public int h { get; set; }
        }

        public class Meta
        {
            public List<FrameTag>? frameTags { get; set; }
            public Dictionary<string, FrameTag>? frameTagsDictionary { get; private set; }

            internal void InitializeFrameTagsDictionary()
            {
                frameTagsDictionary = new Dictionary<string, FrameTag>();
                foreach (var tag in frameTags!)
                {
                    frameTagsDictionary.Add(tag.name, tag);
                }
            }

            public class FrameTag
            {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
                public string name { get; set; }
                public int from { get; set; }
                public int to { get; set; }
                public string direction { get; set; }
                public int TotalDuration { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            }
        }


        public static AnimationData FromJson(string json)
        {
            var data = JsonSerializer.Deserialize<AnimationData>(json);
            data!.Initialize();
            return data;
        }

        static Dictionary<string, AnimationData> animationDatas = new Dictionary<string, AnimationData>();

        public static AnimationData GetAnimationData(ContentManager content, string assetName)
        {
            if (animationDatas.ContainsKey(assetName))
                return animationDatas[assetName];

            string file = Path.Combine(content.RootDirectory, assetName.Replace('/', Path.DirectorySeparatorChar) + ".ani");
            string json = File.ReadAllText(file);
            AnimationData data = FromJson(json);
            animationDatas.Add(assetName, data);
            return data;
        }
    }

}
