using UnityEngine;

namespace Paywall.Tools {

    public class SpriteTools {
        //float angle: the angle of rotation (if any) for our sprite. Mathf.Deg2Rad * sprite.transform.rotation.eulerAngles.z;
        //Vector 2 pivot: the pivot from which the sprite is rotating (usually the transform)
        //Vector2 point: the point (using relative sprite x and y) that we wish to calculate. 
        public static Vector2 WorldSpacePointFromSprite(Vector2 pivot, Vector2 point, float angle = 0.0f) {
            return new Vector2((point.x - pivot.x) * Mathf.Cos(angle) - (point.y - pivot.y) * Mathf.Sin(angle) + pivot.x, (point.y - pivot.y) * Mathf.Cos(angle) + (point.x - pivot.x) * Mathf.Sin(angle) + pivot.y);
        }

        public static Bounds CalculateUnrotatedBounds(Bounds rotatedBounds, Transform transform) {
            // Get the rotation angle of the sprite in radians
            float rotationRadians = Mathf.Deg2Rad * transform.eulerAngles.z;

            // Calculate cosine and sine of the rotation angle
            float cosAngle = Mathf.Cos(rotationRadians);
            float sinAngle = Mathf.Sin(rotationRadians);

            // Calculate half size after unrotation
            float halfWidth = rotatedBounds.size.x * Mathf.Abs(cosAngle) + rotatedBounds.size.y * Mathf.Abs(sinAngle);
            float halfHeight = rotatedBounds.size.y * Mathf.Abs(cosAngle) + rotatedBounds.size.x * Mathf.Abs(sinAngle);

            // Calculate unrotated bounds center
            Vector3 unrotatedCenter = transform.position + new Vector3(rotatedBounds.center.x * cosAngle - rotatedBounds.center.y * sinAngle,
                                                                       rotatedBounds.center.x * sinAngle + rotatedBounds.center.y * cosAngle,
                                                                       0f);

            // Create unrotated bounds
            Bounds unrotatedBounds = new Bounds(unrotatedCenter, new Vector3(halfWidth * 2f, halfHeight * 2f, rotatedBounds.size.z));

            return unrotatedBounds;
        }

        public static Bounds Transpose(Bounds bounds, Transform transform) {
            // Get the inverse transformation matrix
            Matrix4x4 inverseMatrix = Matrix4x4.TRS(
                -transform.position,
                Quaternion.Inverse(transform.rotation),
                Vector3.one / transform.lossyScale.magnitude
            );

            // Transform the bounds using the inverse matrix
            Vector3 min = inverseMatrix.MultiplyPoint(bounds.min);
            Vector3 max = inverseMatrix.MultiplyPoint(bounds.max);

            return new Bounds((min + max) * 0.5f, max - min);
        }
    }
}
