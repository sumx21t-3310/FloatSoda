using System.Numerics;

namespace FloatSoda.OVR.Math
{
    public static class MatrixExtension
    {
        /// <summary>
        /// Converts a <see cref="Matrix4x4"/> to a <see cref="HmdMatrix34_t"/>.
        /// System.Numerics は行優先 (Row-major)、OpenVR は列優先 (Column-major) のため、
        /// 転置しながら3x4行列に変換する。平行移動成分は M41/M42/M43 から取得。
        /// <br/>
        /// <br/>
        /// From (Row-major 4x4): <br/>
        /// M11 M12 M13 M14 <br/>
        /// M21 M22 M23 M24 <br/>
        /// M31 M32 M33 M34 <br/>
        /// M41 M42 M43 M44 <br/>
        /// <br/>
        /// To (Column-major 3x4): <br/>
        /// M11 M12 M13 M41 <br/>
        /// M21 M22 M23 M42 <br/>
        /// M31 M32 M33 M43
        /// </summary>
        public static HmdMatrix34_t ToHmdMatrix34_t(this Matrix4x4 matrix)
        {
            return new HmdMatrix34_t
            {
                m0 = matrix.M11,
                m1 = matrix.M12,
                m2 = matrix.M13,
                m3 = matrix.M41,
                m4 = matrix.M21,
                m5 = matrix.M22,
                m6 = matrix.M23,
                m7 = matrix.M42,
                m8 = matrix.M31,
                m9 = matrix.M32,
                m10 = matrix.M33,
                m11 = matrix.M43,
            };
        }

        /// <summary>
        /// Converts a <see cref="HmdMatrix34_t"/> to a <see cref="Matrix4x4"/>.
        /// Column-major の3x4行列を Row-major の4x4行列に変換する。
        /// <br/>
        /// <br/>
        /// From (Column-major 3x4): <br/>
        /// m0 m1 m2  m3  <br/>
        /// m4 m5 m6  m7  <br/>
        /// m8 m9 m10 m11 <br/>
        /// <br/>
        /// To (Row-major 4x4): <br/>
        /// m0 m1 m2  0 <br/>
        /// m4 m5 m6  0 <br/>
        /// m8 m9 m10 0 <br/>
        /// m3 m7 m11 1
        /// </summary>
        public static Matrix4x4 ToMatrix4x4(this HmdMatrix34_t matrix)
        {
            return new Matrix4x4(
                matrix.m0, matrix.m1, matrix.m2, 0,
                matrix.m4, matrix.m5, matrix.m6, 0,
                matrix.m8, matrix.m9, matrix.m10, 0,
                matrix.m3, matrix.m7, matrix.m11, 1
            );
        }
    }
}
