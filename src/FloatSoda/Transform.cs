using System.Numerics;
using Valve.VR;

namespace FloatSoda;

public class Transform
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }

    public static Transform FromHmdMatrix34_t(HmdMatrix34_t m)
    {
        // 1. 位置(Position)の抽出: 4列目 (m3, m7, m11)
        Vector3 pos = new Vector3(m.m3, m.m7, m.m11);

        // 2. 回転(Rotation)の抽出: 3x3行列からクォータニオンへ変換
        // OpenVRの行列構造:
        // [ m0, m1, m2,  m3 ]
        // [ m4, m5, m6,  m7 ]
        // [ m8, m9, m10, m11]
        
        float tr = m.m0 + m.m5 + m.m10;
        Quaternion q = new Quaternion();

        if (tr > 0)
        {
            float s = MathF.Sqrt(tr + 1.0f) * 2;
            q.W = 0.25f * s;
            q.X = (m.m9 - m.m6) / s;
            q.Y = (m.m2 - m.m8) / s;
            q.Z = (m.m4 - m.m1) / s;
        }
        else if ((m.m0 > m.m5) && (m.m0 > m.m10))
        {
            float s = MathF.Sqrt(1.0f + m.m0 - m.m5 - m.m10) * 2;
            q.W = (m.m9 - m.m6) / s;
            q.X = 0.25f * s;
            q.Y = (m.m1 + m.m4) / s;
            q.Z = (m.m2 + m.m8) / s;
        }
        else if (m.m5 > m.m10)
        {
            float s = MathF.Sqrt(1.0f + m.m5 - m.m0 - m.m10) * 2;
            q.W = (m.m2 - m.m8) / s;
            q.X = (m.m1 + m.m4) / s;
            q.Y = 0.25f * s;
            q.Z = (m.m6 + m.m9) / s;
        }
        else
        {
            float s = MathF.Sqrt(1.0f + m.m10 - m.m0 - m.m5) * 2;
            q.W = (m.m4 - m.m1) / s;
            q.X = (m.m2 + m.m8) / s;
            q.Y = (m.m6 + m.m9) / s;
            q.Z = 0.25f * s;
        }

        return new Transform 
        { 
            Position = pos, 
            Rotation = Quaternion.Normalize(q) 
        };
    }

    public HmdMatrix34_t ToHmdMatrix34_t()
    {
        // クォータニオンから4x4行列を作成
        Matrix4x4 m = Matrix4x4.CreateFromQuaternion(Rotation);
        
        return new HmdMatrix34_t
        {
            // 回転成分の代入
            m0 = m.M11, m1 = m.M12, m2 = m.M13,
            m4 = m.M21, m5 = m.M22, m6 = m.M23,
            m8 = m.M31, m9 = m.M32, m10 = m.M33,
            
            // 位置成分の代入 (4列目)
            m3 = Position.X,
            m7 = Position.Y,
            m11 = Position.Z
        };
    }

    public static implicit operator HmdMatrix34_t(Transform t) => t.ToHmdMatrix34_t();
    public static implicit operator Transform(HmdMatrix34_t matrix34T) => FromHmdMatrix34_t(matrix34T);
}