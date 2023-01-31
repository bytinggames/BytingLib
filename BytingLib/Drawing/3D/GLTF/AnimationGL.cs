using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Nodes;

namespace BytingLib
{
    public class AnimationGL
    {
        public readonly string? Name;
        public override string ToString() => "Animation: " + Name;

        public Channel[] channels;
        //public Sampler[] samplers;

        public AnimationGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();
            var samplersArr = n["samplers"]!.AsArray();
            //samplers = n["samplers"]!.AsArray().Select(f => new Sampler(model, f!)).ToArray();
            channels = n["channels"]!.AsArray().Select(c => GetChannel(c!, model, samplersArr)).ToArray();
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

            public override void Apply(float second)
            {
                Apply(samplerT.GetValue(second));
            }

            public override void ApplyDefault()
            {
                Apply(GetDefaultValue());
            }

            public override void ApplyBlendAdd(float second, float interpolationAmount)
            {
                var val = samplerT.GetValue(second);
                AddBlend(val, interpolationAmount);
            }

            public override void ApplyBlendAddDefault(float interpolationAmount)
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
            public int InterpolationStep { get; set; } = -1;

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
            public ChannelTarget Target;


            public Channel(ModelGL model, JsonNode n, ISampler sampler)
            {
                this.sampler = sampler;

                var target = n["target"]!;
                var targetNodeIndex = target["node"]!.GetValue<int>();
                var path = target["path"]!.GetValue<string>();
                Target = model.ChannelTargets!.Get(targetNodeIndex, path);
            }

            public abstract void Apply(float second);
            public abstract void ApplyDefault();
            public abstract void ApplyBlendAdd(float second, float interpolationAmount);
            public abstract void ApplyBlendAddDefault(float interpolationAmount);


            public void BlendStart(float second, int interpolationStep)
            {
                Target.InterpolationStep = interpolationStep;
                Apply(second);
            }

            public void BlendAdd(float second, float interpolationAmount, int interpolationStep)
            {
                // is unset? then set it to the default
                if (Target.InterpolationStep == -1)
                    ApplyDefault();
                Target.InterpolationStep = interpolationStep;
                ApplyBlendAdd(second, interpolationAmount);
            }

            public void BlendAddDefault(float interpolationAmount, int interpolationStep)
            {
                Target.InterpolationStep = interpolationStep;
                ApplyBlendAddDefault(interpolationAmount);
            }
        }

        public interface ISampler
        {

        }

        public class Sampler<T> : ISampler
        {
            public KeyFrames KeyFrames; // refers to an accessor with floats, which are the times of the key frames of the animation
            public SamplerOutput<T> Output; // refers to an accessor that contains the values for the animated property at the respective key frames
            public SamplerFramesInterpolation interpolation;

            // TODO: move inside KeyFrames. Wait... this should be placed at a higher level!
            float AnimationDuration => KeyFrames.seconds[^1] - AnimationStartSecond + TransitionSecondsBetweenLastAndFirstFrame;
            public float AnimationStartSecond;
            public float TransitionSecondsBetweenLastAndFirstFrame = 0.5f;
            public bool ReverseAnimationOnWrap = false;

            public Sampler(ModelGL model, JsonNode n, SamplerOutput<T> samplerOutput)
            {
                int input = n["input"]!.GetValue<int>();
                int output = n["output"]!.GetValue<int>();

                byte[] bytes = model.GetBytesFromBuffer(input);
                KeyFrames = new KeyFrames(bytes); // TODO: use cached keyFrames

                bytes = model.GetBytesFromBuffer(output);

                Output = samplerOutput;
                Output.Initialize(bytes);

                var interpolationStr = n["interpolation"]?.GetValue<string>();
                interpolation = interpolationStr == "STEP" ? SamplerFramesInterpolation.Step
                    : interpolationStr == "CUBICSPLINE" ? SamplerFramesInterpolation.CubicSpline
                    : SamplerFramesInterpolation.Linear;

                if (KeyFrames.seconds.Length > 0)
                    AnimationStartSecond = KeyFrames.seconds[0];
            }

            internal T GetValue(float second)
            {
                var frames = KeyFrames.seconds;
                int frame = 0;
                if (ReverseAnimationOnWrap)
                {
                    second = second % (AnimationDuration * 2);
                    if (second > AnimationDuration)
                        second = AnimationDuration * 2 - second;
                }
                else
                    second = second % AnimationDuration;
                second += AnimationStartSecond;

                while (frame < frames.Length
                    && second >= frames[frame])
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
                else if (second == frames[frame])
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
                        if (TransitionSecondsBetweenLastAndFirstFrame == 0)
                        {
                            // no interpolation needed, just use the last frame
                            return Output.GetValue(frame0); // frame0 = frames.Length - 1 in this case
                        }
                        frame1 = 0;
                        nextFrameSecond = frames[frame0] + TransitionSecondsBetweenLastAndFirstFrame;
                    }

                    float lerpAmount = (second - previousFrameSecond) / (nextFrameSecond - previousFrameSecond); // [0,1]

                    return Output.GetValue(frame0, frame1, lerpAmount, interpolation);
                }
            }
        }
    }
}
