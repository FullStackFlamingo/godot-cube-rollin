namespace CubeWorld;

using Godot;
using Godot.Collections;

class MathR
{
    /// <summary>
    /// Rotate a Transform3D around a pivot point, on an axis, with provided angle in Radians
    /// </summary>
    /// <param name="trans">Original Transform</param>
    /// <param name="pivotPoint">Pivot Point e.g. Vector3(1,1,1) bottom corner of cube object</param>
    /// <param name="axis">Axis, e.g. Vector3(1,0,0)</param>
    /// <param name="angle">Angle in Radians</param>
    /// <returns>Modified transform</returns>
    public static Transform3D RotateAround(Transform3D trans, Vector3 pivotPoint, Vector3 axis, float angle)
    {
        // # Rotate its basis
        Basis rotated_basis = trans.Basis.Rotated(axis, angle);

        // # Rotate its origin
        Vector3 rotated_origin = pivotPoint + (trans.Origin - pivotPoint).Rotated(axis, angle);

        // # Set the result back
        return new Transform3D(rotated_basis, rotated_origin);
    }
    public static PhysicsRayQueryParameters3D ScreenToWorldRayQueryParamters3D(Camera3D camera3D, Vector2 screenPosition, uint collisionMask = uint.MaxValue, Array<Rid> exclude = null)
    {
        Vector3 from = camera3D.ProjectRayOrigin(screenPosition);
        Vector3 to = from + camera3D.ProjectRayNormal(screenPosition) * 1000 /*Ray Length */;
        return PhysicsRayQueryParameters3D.Create(from, to, collisionMask, exclude);
    }

    /// <summary>
    /// Calculates the shortest difference between two given angles.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float DeltaAngle(float current, float target)
    {
        float delta = MathR.Repeat(target - current, Mathf.Pi * 2);
        if (delta > Mathf.Pi)
            delta -= Mathf.Pi * 2;
        return delta;
    }

    /// <summary>
    /// Calculates the shortest difference between two given angles.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float DeltaAngleDegrees(float current, float target)
    {
        float delta = MathR.Repeat(target - current, 360.0F);
        if (delta > 180.0F)
            delta -= 360.0F;
        return delta;
    }
    /// <summary>
    ///  Loops the value t, so that it is never larger than max and never smaller than 0.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float Repeat(float t, float max)
    {
        return Mathf.Clamp(t - Mathf.Floor(t / max) * max, 0.0f, max);
    }

}
