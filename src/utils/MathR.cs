using Godot;

namespace CubeWorld
{
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

    }
}