using BytingLibGame.IngameSpline;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BytingLib
{
    public class SamplerOutputQuaternion : SamplerOutput<Quaternion>
    {
        public override Quaternion InterpolateLinear(Quaternion value0, Quaternion value1, float interpolationAmount)
        {
            Quaternion result;
            Quaternion.Slerp(ref value0, ref value1, interpolationAmount, out result);
            return result;
        }

        public override Quaternion InterpolateCubicSpline(Quaternion[] values4, float[] weights)
        {
            Quaternion result = new Quaternion();
            for (int i = 0; i < 4; i++)
            {
                result += values4[i] * weights[i];
            }
            return result;
        }

        protected override Quaternion[] BytesToValues(byte[] bytes, int keyFrameCount)
        {
            const int structSize = 4 * 4;
            int outputCount = bytes.Length / structSize;
            if (outputCount != keyFrameCount)
            {
                string warnMessage = "key frame count is unequal to output count: " + keyFrameCount + " != " + outputCount;

                if (outputCount == keyFrameCount * 3)
                {
                    Debug.WriteLine("Warning: " + warnMessage + ". The output count is exactly three times as large as the key frame count, " +
                    "so I'm guessing this is the bug I discovered, which I wrote a fix for. Hopefully this works!" +
                    "This probably has to do with exporting your animation without having 'Always Sample Animations' activated. In this scenario, somehow three times as many" +
                    "output bytes are written, but 2/3s of those are 0s, which I dump in my fix. The same goes for Vectors", "warning");

                    // this seems to be a bug in the blender gltf exporter, but this fixes it:
                    // the structs contained in the byte array look like the following
                    // 0 x 0   0 x 0   0 x 0  
                    // where x is the actual data, we want. one 0/x displayed here represents the whole struct (16 bytes)

                    Quaternion[] q = new Quaternion[keyFrameCount];

                    var pData = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                    IntPtr addr = pData.AddrOfPinnedObject();
                    addr = IntPtr.Add(addr, structSize); // skip the first struct which is 0

                    for (int i = 0; i < keyFrameCount; i++)
                    {
                        q[i] = Marshal.PtrToStructure<Quaternion>(addr);
                        addr = IntPtr.Add(addr, structSize * 3); // increment by 3 structs
                    }
                    pData.Free();
                    return q;
                }
            }
            return ByteExtension.ByteArrayToStructArray<Quaternion>(bytes, structSize);
        }
    }
}
