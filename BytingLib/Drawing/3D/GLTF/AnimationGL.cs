using System.Text.Json.Nodes;

namespace BytingLib
{
    public class AnimationGL
    {
        private readonly ModelGL model;
        public Channel[] Channels { get; }

        public string? Name { get; }
        public float AnimationStartSecond { get; set; }
        public float AnimationEndSecond { get; set; }
        public float TransitionSecondsBetweenLastAndFirstFrame { get; set; } = 0f; // TODO: probably pass this value by paramter, instead of storing it in this raw animation representation
        public bool Looped { get; set; } = true; // TODO: probably pass this value by paramter, instead of storing it in this raw animation representation
        
        public float AnimationDuration => AnimationEndSecond - AnimationStartSecond + TransitionSecondsBetweenLastAndFirstFrame;
        public float AnimationEndSecondIncludingTransitionToBegin => AnimationEndSecond + TransitionSecondsBetweenLastAndFirstFrame;

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
            Channels = n["channels"]!.AsArray().Select(c => GetChannel(c!, model, samplersArr)).ToArray();

            // get animation start and end second
            float startSecond = float.MaxValue;
            float endSecond = float.MinValue;
            for (int i = 0; i < Channels.Length; i++)
            {
                float second = Channels[i].Sampler.KeyFrames.Seconds[0];
                if (startSecond > second)
                {
                    startSecond = second;
                }

                second = Channels[i].Sampler.KeyFrames.Seconds[^1];
                if (endSecond < second)
                {
                    endSecond = second;
                }
            }
            AnimationStartSecond = startSecond;
            AnimationEndSecond = endSecond;
        }

        public override string ToString() => "Animation: " + Name;

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
        public void UpdateAnimationTime(float second, WrapMode wrap)
        {
            float samplerSecond = SecondToSamplerSecond(second, wrap);

            for (int i = 0; i < Channels.Length; i++)
            {
                Channels[i].Apply(samplerSecond, AnimationEndSecondIncludingTransitionToBegin, Looped);
            }
        }

        public void BlendStart(float second, WrapMode wrap)
        {
            model.AnimationBlend.Begin();

            for (int i = 0; i < Channels.Length; i++)
            {
                model.AnimationBlend.AddChannelfNotAlready(Channels[i]);
            }

            UpdateAnimationTime(second, wrap);
        }

        public void BlendAdd(float second, float interpolationAmount, WrapMode wrap)
        {
            if (model.AnimationBlend == null)
            {
                throw new Exception("You first need to call BlendStart()");
            }

            float samplerSecond = SecondToSamplerSecond(second, wrap);

            model.AnimationBlend.BeginAnimationBlend();
            for (int i = 0; i < Channels.Length; i++)
            {
                if (model.AnimationBlend.AddChannelfNotAlready(Channels[i]))
                {
                    // new blend, so start blending from default
                    Channels[i].ApplyDefault();
                }
                Channels[i].BlendTo(samplerSecond, interpolationAmount, AnimationEndSecondIncludingTransitionToBegin, Looped);
            }
            model.AnimationBlend.EndAnimationBlend(interpolationAmount);
        }

        private float SecondToSamplerSecond(float second, WrapMode wrap)
        {
            if (AnimationDuration > 0)
            {
                switch (wrap)
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
                        {
                            second = AnimationDuration * 2 - second;
                        }

                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                second = 0f;
            }

            second += AnimationStartSecond;
            return second;
        }

        public void ApplyDefault()
        {
            Channels.ForEvery(f => f.ApplyDefault());
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
                val = samplerT.Output.InterpolateLinear(GetNodeValue(), val, interpolationAmount);
                Apply(val);
            }

            internal override void Apply(float samplerSecond, float? endSecondForInterpolationToStart, bool looped)
            {
                Apply(samplerT.GetValue(samplerSecond, endSecondForInterpolationToStart, looped));
            }

            internal override void ApplyDefault()
            {
                Apply(GetDefaultValue());
            }

            internal override void BlendTo(float second, float interpolationAmount, float? endSecondForInterpolationToStart, bool looped)
            {
                var val = samplerT.GetValue(second, endSecondForInterpolationToStart, looped);
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
            public NodeGL Node { get; }
            internal int AnimationBlendId { get; set; }
            public int AnimationBlendStep { get; set; }

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

            public ISampler Sampler { get; }
            public ChannelTarget Target { get; }


            public Channel(ModelGL model, JsonNode n, ISampler sampler)
            {
                Sampler = sampler;

                var target = n["target"]!;
                var targetNodeIndex = target["node"]!.GetValue<int>();
                var path = target["path"]!.GetValue<string>();
                Target = model.ChannelTargets!.Get(targetNodeIndex, path);
            }

            internal abstract void Apply(float samplerSecond, float? endSecondForInterpolationToStart, bool looped);
            internal abstract void ApplyDefault();
            internal abstract void BlendTo(float samplerSecond, float interpolationAmount, float? endSecondForInterpolationToStart, bool looped);
            internal abstract void BlendToDefault(float interpolationAmount);
        }

        public interface ISampler
        {
            public KeyFrames KeyFrames { get; }
        }

        public class Sampler<T> : ISampler
        {
            public KeyFrames KeyFrames { get; } // refers to an accessor with floats, which are the times of the key frames of the animation
            public SamplerOutput<T> Output { get; } // refers to an accessor that contains the values for the animated property at the respective key frames
            public SamplerFramesInterpolation Interpolation { get; }

            public Sampler(ModelGL model, JsonNode n, SamplerOutput<T> samplerOutput)
            {
                int input = n["input"]!.GetValue<int>();
                int output = n["output"]!.GetValue<int>();

                KeyFrames = model.KeyFrames!.Get(input)!;

                byte[] bytes = model.GetBytesFromBuffer(output);
                Output = samplerOutput;
                Output.Initialize(bytes, KeyFrames.Seconds.Length);

                var interpolationStr = n["interpolation"]?.GetValue<string>();
                Interpolation = interpolationStr == "STEP" ? SamplerFramesInterpolation.Step
                    : interpolationStr == "CUBICSPLINE" ? SamplerFramesInterpolation.CubicSpline
                    : SamplerFramesInterpolation.Linear;
            }

            internal T GetValue(float samplerSecond, float? endSecondForInterpolationToStart, bool looped)
            {
                float[] frames = KeyFrames.Seconds;
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
                else if (samplerSecond == frames[frame]
                    || Interpolation == SamplerFramesInterpolation.Step)
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


                    switch (Interpolation)
                    {
                        case SamplerFramesInterpolation.Linear:
                            return Output.GetValueLinear(frame0, frame1, lerpAmount);

                        case SamplerFramesInterpolation.CubicSpline:
                            int[] frames4 = CatmullRomSpline.GetIndices(frame0 + lerpAmount, frames.Length, looped);
                            return Output.GetValueCubicSpline(frames4, lerpAmount);

                        default:
                            throw new NotImplementedException();
                    }

                }
            }
        }
    }
}
