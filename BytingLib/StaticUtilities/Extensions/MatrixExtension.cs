
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public static class MatrixExtension
    {
        public static void MatrixToPitchYawRoll(this Matrix rotationMatrix, out float yaw, out float pitch, out float roll)
        {
            // source: https://www.dreamincode.net/forums/topic/349917-convert-from-quaternion-to-euler-angles-vector3/
            float ForwardY = -rotationMatrix.M32;
            if (ForwardY <= -1.0f)
            {
                pitch = -MathHelper.PiOver2;
            }
            else if (ForwardY >= 1.0f)
            {
                pitch = MathHelper.PiOver2;
            }
            else
            {
                pitch = MathF.Asin(ForwardY);
            }
            //Gimbal lock
            if (ForwardY > 0.9999f)
            {
                yaw = 0f;
                roll = MathF.Atan2(rotationMatrix.M13, rotationMatrix.M11);
            }
            else
            {
                yaw = MathF.Atan2(rotationMatrix.M31, rotationMatrix.M33);
                roll = MathF.Atan2(rotationMatrix.M12, rotationMatrix.M22);
            }
        }

    }
}
