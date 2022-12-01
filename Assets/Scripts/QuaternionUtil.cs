using UnityEngine;

/// <summary>
/// Utility class for functions with quaternions
/// </summary>
public static class QuaternionUtil
{
    public static Quaternion AngVelToDeriv(Quaternion Current, Vector3 AngVel)
    {
        var Spin = new Quaternion(AngVel.x, AngVel.y, AngVel.z, 0f);
        var Result = Spin * Current;
        return new Quaternion(0.5f * Result.x, 0.5f * Result.y, 0.5f * Result.z, 0.5f * Result.w);
    }

    public static Vector3 DerivToAngVel(Quaternion Current, Quaternion Deriv)
    {
        var Result = Deriv * Quaternion.Inverse(Current);
        return new Vector3(2f * Result.x, 2f * Result.y, 2f * Result.z);
    }

    public static Quaternion IntegrateRotation(Quaternion Rotation, Vector3 AngularVelocity, float DeltaTime)
    {
        if (DeltaTime < Mathf.Epsilon) return Rotation;
        var Deriv = AngVelToDeriv(Rotation, AngularVelocity);
        var Pred = new Vector4(
                Rotation.x + Deriv.x * DeltaTime,
                Rotation.y + Deriv.y * DeltaTime,
                Rotation.z + Deriv.z * DeltaTime,
                Rotation.w + Deriv.w * DeltaTime
        ).normalized;
        return new Quaternion(Pred.x, Pred.y, Pred.z, Pred.w);
    }

    public static Quaternion QuaternionFromMatrix(double[,] m)
    {
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion
        {
            w = Mathf.Sqrt(Mathf.Max(0, (float)(1 + m[0, 0] + m[1, 1] + m[2, 2]))) / 2,
            x = Mathf.Sqrt(Mathf.Max(0, (float)(1 + m[0, 0] - m[1, 1] - m[2, 2]))) / 2,
            y = Mathf.Sqrt(Mathf.Max(0, (float)(1 - m[0, 0] + m[1, 1] - m[2, 2]))) / 2,
            z = Mathf.Sqrt(Mathf.Max(0, (float)(1 - m[0, 0] - m[1, 1] + m[2, 2]))) / 2
        };
        q.x *= Mathf.Sign((float)(q.x * (m[2, 1] - m[1, 2])));
        q.y *= Mathf.Sign((float)(q.y * (m[0, 2] - m[2, 0])));
        q.z *= Mathf.Sign((float)(q.z * (m[1, 0] - m[0, 1])));
        return q;
    }


    public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
    {
        // account for double-cover
        var Dot = Quaternion.Dot(rot, target);
        var Multi = Dot > 0f ? 1f : -1f;
        target.x *= Multi;
        target.y *= Multi;
        target.z *= Multi;
        target.w *= Multi;
        // smooth damp (nlerp approx)
        var Result = new Vector4(
            Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
            Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
            Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
            Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
        ).normalized;

        // ensure deriv is tangent
        var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
        deriv.x -= derivError.x;
        deriv.y -= derivError.y;
        deriv.z -= derivError.z;
        deriv.w -= derivError.w;

        return new Quaternion(Result.x, Result.y, Result.z, Result.w);
    }

    public static Quaternion DerivateQuaternion(Quaternion initial, Quaternion final, float time)
    {
        return new Quaternion
        {
            x = (final.x - initial.x) / time,
            y = (final.y - initial.y) / time,
            z = (final.z - initial.z) / time,
            w = (final.w - initial.w) / time,
        };

    }

    public static Quaternion EstimateNewRot(Quaternion initial, Quaternion derivative, float time)
    {
        Quaternion newRot = new Quaternion
        {
            x = initial.x + derivative.x * time,
            y = initial.y + derivative.y * time,
            z = initial.z + derivative.z * time,
            w = initial.w + derivative.w * time

        };

        newRot.Normalize();

        return newRot;
    }

    public static float Magnitude(Quaternion quaternion)
    {
        return Mathf.Sqrt(Mathf.Pow(quaternion.x, 2) + Mathf.Pow(quaternion.y, 2) + Mathf.Pow(quaternion.z, 2) + Mathf.Pow(quaternion.w, 2));
    }
}