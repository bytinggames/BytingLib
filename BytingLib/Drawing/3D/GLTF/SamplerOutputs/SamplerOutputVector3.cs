using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BytingLib
{
    public class SamplerOutputVector3 : SamplerOutput<Vector3>
    {
        public override Vector3 Interpolate(Vector3 value0, Vector3 value1, float interpolationAmount, SamplerFramesInterpolation interpolation)
        {
            switch (interpolation)
            {
                case SamplerFramesInterpolation.Linear:
                    Vector3.Lerp(ref value0, ref value1, interpolationAmount, out Vector3 result);
                    return result;
                case SamplerFramesInterpolation.Step:
                    return value0;
                case SamplerFramesInterpolation.CubicSpline:
                default:
                    throw new NotImplementedException();
            }
        }

        protected override Vector3[] BytesToValues(byte[] bytes, int keyFrameCount)
        {
            const int structSize = 3 * 4;
            int outputCount = bytes.Length / structSize;
            if (outputCount != keyFrameCount)
            {
                string warnMessage = "key frame count is unequal to output count: " + keyFrameCount + " != " + outputCount;

                if (outputCount == keyFrameCount * 3)
                {
                    Debug.WriteLine("Warning: " + warnMessage + ". The output count is exactly three times as large as the key frame count, " +
                        "so I'm guessing this is the bug I discovered, which I wrote a fix for. Hopefully this works!" +
                        "This probably has to do with exporting your animation without having 'Always Sample Animations' activated. In this scenario, somehow three times as many" +
                        "output bytes are written, but 2/3s of those are 0s, which I dump in my fix. The same goes for Quaternions", "warning");

                    // this seems to be a bug in the blender gltf exporter, but this fixes it:
                    // the structs contained in the byte array look like the following
                    // 0 x 0   0 x 0   0 x 0  
                    // where x is the actual data, we want. one 0/x displayed here represents the whole struct (12 bytes)

                    Vector3[] v = new Vector3[keyFrameCount];

                    var pData = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                    IntPtr addr = pData.AddrOfPinnedObject();
                    addr = IntPtr.Add(addr, structSize); // skip the first struct which is 0

                    for (int i = 0; i < keyFrameCount; i++)
                    {
                        v[i] = Marshal.PtrToStructure<Vector3>(addr);
                        addr = IntPtr.Add(addr, structSize * 3); // increment by 3 structs
                    }
                    pData.Free();
                    return v;
                }

                throw new BytingException();
            }
            return ByteExtension.ByteArrayToStructArray<Vector3>(bytes, structSize);
        }
    }
}
