using System.Text.Json.Nodes;

namespace BytingLib
{
    public class AnimationGL
    {
        public readonly string? Name;
        private readonly ModelGL model;

        public override string ToString() => "Animation: " + Name;

        public Channel[] channels;

        public float AnimationStartSecond, AnimationEndSecond;
        public float TransitionSecondsBetweenLastAndFirstFrame = 0f;
        public float AnimationDuration => AnimationEndSecond - AnimationStartSecond + TransitionSecondsBetweenLastAndFirstFrame;
        public float AnimationEndSecondIncludingTransitionToBegin => AnimationEndSecond + TransitionSecondsBetweenLastAndFirstFrame;
        public WrapMode Wrap = WrapMode.Repeat;

        public enum WrapMode
        {
            Repeat,
            Clamp,
            Mirror
        }


        public AnimationGL(ModelGL model, JsonNode n)
        {
            this.model = model;
            Name = n["name"]?.GetValue<string>();
            var samplersArr = n["samplers"]!.AsArray();
            channels = n["channels"]!.AsArray().Select(c => GetChannel(c!, model, samplersArr)).ToArray();

            // get animation start and end second
            float startSecond = float.MaxValue;
            float endSecond = float.MinValue;
            for (int i = 0; i < channels.Length; i++)
            {
                float second = channels[i].sampler.KeyFrames.seconds[0];
                if (startSecond > second)
                    startSecond = second;
                second = channels[i].sampler.KeyFrames.seconds[^1];
                if (endSecond < second)
                    endSecond = second;
            }
            AnimationStartSecond = startSecond;
            AnimationEndSecond = endSecond;
        }

        private static Channel GetChannel(JsonNode channelNode, ModelGL model, JsonArray samplersArr)
        {
            var path = channelNode["target"]!["path"]!.GetValue<string>();

            Channel channel = path switch
            {
                "rotation" => new ChannelRotation(model, channelNode, samplersArr),
                "translation" => new ChannelTranslation(model, channelNode, samplersArr),
                "scale" => new ChannelScale(model, channelNode, samplersArr),
                _ => throw new NotImplementedException(),
            };
            return channel;
        }

        /// <summary>Used, when no transition happens</summary>
        public void UpdateAnimationTime(float second)
        {
            float samplerSecond = SecondToSamplerSecond(second);

            for (int i = 0; i < channels.Length; i++)
                channels[i].Apply(samplerSecond, AnimationEndSecondIncludingTransitionToBegin);
        }

        public void BlendStart(float second)
        {
            model.AnimationBlend.Begin();

            for (int i = 0; i < channels.Length; i++)
                model.AnimationBlend.AddChannelfNotAlready(channels[i]);

            UpdateAnimationTime(second);
        }

        public void BlendAdd(float second, float interpolationAmount)
        {
            if (model.AnimationBlend == null)
                throw new Exception("You first need to call BlendStart()");

            float samplerSecond = SecondToSamplerSecond(second);

            model.AnimationBlend.BeginAnimationBlend();
            for (int i = 0; i < channels.Length; i++)
            {
                if (model.AnimationBlend.AddChannelfNotAlready(channels[i]))
                {
                    // new blend, so start blending from default
                    channels[i].ApplyDefault();
                }
                channels[i].BlendTo(samplerSecond, interpolationAmount, AnimationEndSecondIncludingTransitionToBegin);
            }
            model.AnimationBlend.EndAnimationBlend(interpolationAmount);
        }

        private float SecondToSamplerSecond(float second)
        {
            switch (Wrap)
            {
                case WrapMode.Repeat:
                    second = second % AnimationDuration;
                    break;
                case WrapMode.Clamp:
                    second = Math.Clamp(second, 0, AnimationDuration);
                    break;
                case WrapMode.Mirror:
                    second = second % (AnimationDuration * 2);
                    if (second > AnimationDuration)
                        second = AnimationDuration * 2 - second;
                    break;
                default:
                    throw new NotImplementedException();
            }

            second += AnimationStartSecond;
            return second;
        }

        public void ApplyDefault()
        {
            channels.ForEvery(f => f.ApplyDefault());
        }

        abstract class Channel<T> : Channel
        {
            protected Sampler<T> samplerT;

            protected Channel(ModelGL model, JsonNode n, ISampler sampler) : base(model, n, sampler)
            {
                samplerT = (Sampler<T>)sampler;
            }

            protected abstract void Apply(T val);
            protected abstract T GetNodeValue();
            protected abstract T GetDefaultValue();

            protected void AddBlend(T val, float interpolationAmount)
            {
                val = samplerT.Output.Interpolate(GetNodeValue(), val, interpolationAmount, samplerT.interpolation);
                Apply(val);
            }

            internal override void Apply(float samplerSecond, float? endSecondForInterpolationToStart)
            {
                Apply(samplerT.GetValue(samplerSecond, endSecondForInterpolationToStart));
            }

            internal override void ApplyDefault()
            {
                Apply(GetDefaultValue());
            }

            internal override void BlendTo(float second, float interpolationAmount, float? endSecondForInterpolationToStart)
            {
                var val = samplerT.GetValue(second, endSecondForInterpolationToStart);
                AddBlend(val, interpolationAmount);
            }

            internal override void BlendToDefault(float interpolationAmount)
            {
                var val = GetDefaultValue();
                AddBlend(val, interpolationAmount);
            }
        }

        class ChannelRotation : Channel<Quaternion>
        {
            public ChannelRotation(ModelGL model, JsonNode n, JsonArray samplersArr)
                : base(model, n,
                      new Sampler<Quaternion>(model, samplersArr[n["sampler"]!.GetValue<int>()]!, new SamplerOutputQuaternion()))
            { }

            protected override void Apply(Quaternion val)
            {
                Target.Node.JointTransform!.Rotation = val;
            }

            protected override Quaternion GetNodeValue() => Target.Node.JointTransform!.Rotation;

            protected override Quaternion GetDefaultValue() => Target.Node.JointTransform!.RotationDefault;
        }
        class ChannelTranslation : Channel<Vector3>
        {
            public ChannelTranslation(ModelGL model, JsonNode n, JsonArray samplersArr)
                : base(model, n,
                      new Sampler<Vector3>(model, samplersArr[n["sampler"]!.GetValue<int>()]!, new SamplerOutputVector3()))
            {
            }
            protected override void Apply(Vector3 val)
            {
                Target.Node.JointTransform!.Translation = val;
            }

            protected override Vector3 GetNodeValue() => Target.Node.JointTransform!.Translation;

            protected override Vector3 GetDefaultValue() => Target.Node.JointTransform!.TranslationDefault;
        }
        class ChannelScale : Channel<Vector3>
        {
            public ChannelScale(ModelGL model, JsonNode n, JsonArray samplersArr)
                : base(model, n,
                      new Sampler<Vector3>(model, samplersArr[n["sampler"]!.GetValue<int>()]!, new SamplerOutputVector3()))
            {
            }
            protected override void Apply(Vector3 val)
            {
                Target.Node.JointTransform!.Scale = val;
            }

            protected override Vector3 GetNodeValue() => Target.Node.JointTransform!.Scale;

            protected override Vector3 GetDefaultValue() => Target.Node.JointTransform!.ScaleDefault;
        }

        public class ChannelTarget
        {
            public NodeGL Node;
            internal int AnimationBlendId;
            public int AnimationBlendStep;

            public ChannelTarget(ModelGL model, int nodeIndex)
            {
                Node = model.Nodes!.Get(nodeIndex)!;

                Node.InitializeForAnimation();
            }
        }

        public abstract class Channel
        {
            public enum TargetPath
            {
                Translation,
                Rotation,
                Scale,
                Weights
            }

            public ISampler sampler;
            public readonly ChannelTarget Target;


            public Channel(ModelGL model, JsonNode n, ISampler sampler)
            {
                this.sampler = sampler;

                var target = n["target"]!;
                var targetNodeIndex = target["node"]!.GetValue<int>();
                var path = target["path"]!.GetValue<string>();
                Target = model.ChannelTargets!.Get(targetNodeIndex, path);
            }

            internal abstract void Apply(float samplerSecond, float? endSecondForInterpolationToStart);
            internal abstract void ApplyDefault();
            internal abstract void BlendTo(float samplerSecond, float interpolationAmount, float? endSecondForInterpolationToStart);
            internal abstract void BlendToDefault(float interpolationAmount);
        }

        public interface ISampler
        {
            public KeyFrames KeyFrames { get; }
        }

        public class Sampler<T> : ISampler
        {
            public KeyFrames KeyFrames { get; } // refers to an accessor with floats, which are the times of the key frames of the animation
            public SamplerOutput<T> Output; // refers to an accessor that contains the values for the animated property at the respective key frames
            public SamplerFramesInterpolation interpolation;

            public Sampler(ModelGL model, JsonNode n, SamplerOutput<T> samplerOutput)
            {
                int input = n["input"]!.GetValue<int>();
                int output = n["output"]!.GetValue<int>();

                KeyFrames = model.KeyFrames!.Get(input)!;

                byte[] bytes = model.GetBytesFromBuffer(output);
                Output = samplerOutput;
                Output.Initialize(bytes, KeyFrames.seconds.Length);

                var interpolationStr = n["interpolation"]?.GetValue<string>();
                interpolation = interpolationStr == "STEP" ? SamplerFramesInterpolation.Step
                    : interpolationStr == "CUBICSPLINE" ? SamplerFramesInterpolation.CubicSpline
                    : SamplerFramesInterpolation.Linear;
            }

            internal T GetValue(float samplerSecond, float? endSecondForInterpolationToStart)
            {
                float[] frames = KeyFrames.seconds;
                int frame = 0;
                // TODO: make more efficient search
                while (frame < frames.Length
                    && samplerSecond >= frames[frame])
                {
                    frame++;
                }
                frame--;
                // animationSecond is >= frames[frame]
                // interpolate between frame - 1 and frame

                if (frame < 0)
                {
                    // before start, just use the first frame
                    return Output.GetValue(0);
                }
                else if (samplerSecond == frames[frame])
                {
                    // no interpolation needed
                    return Output.GetValue(frame);
                }
                else
                {
                    int frame0 = frame;
                    int frame1 = frame + 1;

                    float previousFrameSecond = frames[frame0];

                    float nextFrameSecond;
                    if (frame1 < frames.Length)
                    {
                        nextFrameSecond = frames[frame1];
                    }
                    else
                    {
                        if (endSecondForInterpolationToStart == null)
                        {
                            // no interpolation needed, just use the last frame
                            return Output.GetValue(frame0); // frame0 is the same as (frames.Length - 1) in this case
                        }
                        frame1 = 0; // interpolation to start
                        nextFrameSecond = endSecondForInterpolationToStart.Value;
                    }

                    float lerpAmount = (samplerSecond - previousFrameSecond) / (nextFrameSecond - previousFrameSecond); // [0,1]

                    return Output.GetValue(frame0, frame1, lerpAmount, interpolation);
                }
            }
        }
    }
}
