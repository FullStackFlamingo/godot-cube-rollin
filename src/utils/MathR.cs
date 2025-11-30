namespace CubeWorld;

using Godot;
using Godot.Collections;

class MathR
{
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
}
