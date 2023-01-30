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
            channels = n["channels"]!.AsArray().Select(f => new Channel(model, f!, samplersArr)).ToArray();
        }

        public class Channel
        {
            public enum TargetPath
            {
                Translation,
                Rotation,
                Scale,
                Weights
            }

            public Sampler sampler;
            public NodeGL TargetNode;
            public TargetPath targetPath;

            public Channel(ModelGL model, JsonNode n, JsonArray samplersArr)
            {
                var target = n["target"]!;
                var targetNodeIndex = target["node"]!.GetValue<int>();
                var path = target["path"]!.GetValue<string>();

                TargetNode = model.Nodes!.Get(targetNodeIndex)!;

                targetPath = path switch
                {
                    "rotation" => TargetPath.Rotation,
                    "translation" => TargetPath.Translation,
                    "scale" => TargetPath.Scale,
                    _ => throw new NotImplementedException(),
                };

                sampler = new Sampler(model, samplersArr[n["sampler"]!.GetValue<int>()]!, targetPath);
            }

            public void Apply(float second)
            {
                sampler.Apply(TargetNode, second);
            }
        }

        public class Sampler
        {
            public enum Interpolation
            {
                Linear,
                Step,
                CubicSpline
            }
            public KeyFrames KeyFrames; // refers to an accessor with floats, which are the times of the key frames of the animation
            public SamplerOutput Output; // refers to an accessor that contains the values for the animated property at the respective key frames
            public Interpolation interpolation;

            // TODO: move inside KeyFrames
            float AnimationDuration => KeyFrames.seconds[^1] - AnimationStartSecond + TransitionSecondsBetweenLastAndFirstFrame;
            public float AnimationStartSecond;
            public float TransitionSecondsBetweenLastAndFirstFrame = 0f;

            public Sampler(ModelGL model, JsonNode n, Channel.TargetPath targetPath)
            {
                int input = n["input"]!.GetValue<int>();
                int output = n["output"]!.GetValue<int>();

                byte[] bytes = model.GetBytesFromBuffer(input);
                KeyFrames = new KeyFrames(bytes); // TODO: use cached keyFrames

                bytes = model.GetBytesFromBuffer(output);
                switch (targetPath)
                {
                    case Channel.TargetPath.Translation:
                        Output = new SamplerOutputTranslation(bytes);
                        break;
                    case Channel.TargetPath.Rotation:
                        Output = new SamplerOutputRotation(bytes);
                        break;
                    case Channel.TargetPath.Scale:
                        Output = new SamplerOutputScale(bytes);
                        break;
                    case Channel.TargetPath.Weights:
                    default:
                        throw new NotImplementedException();
                }

                var interpolationStr = n["interpolation"]?.GetValue<string>();
                interpolation = interpolationStr == "STEP" ? Interpolation.Step
                    : interpolationStr == "CUBICSPLINE" ? Interpolation.CubicSpline
                    : Interpolation.Linear;

                if (KeyFrames.seconds.Length > 0)
                    AnimationStartSecond = KeyFrames.seconds[0];
            }

            internal void Apply(NodeGL targetNode, float second)
            {
                var frames = KeyFrames.seconds;
                int frame = 0;
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
                    Output.Apply(targetNode, 0);
                }
                else if (second == frames[frame])
                {
                    // no interpolation needed
                    Output.Apply(targetNode, frame);
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
                            Output.Apply(targetNode, frame0); // frame0 = frames.Length - 1 in this case
                            return;
                        }
                        frame1 = 0;
                        nextFrameSecond = frames[frame0] + TransitionSecondsBetweenLastAndFirstFrame;
                    }

                    float lerpAmount = (second - previousFrameSecond) / (nextFrameSecond - previousFrameSecond); // [0,1]

                    Output.Apply(targetNode, frame0, frame1, lerpAmount);
                }
            }
        }
    }
}
