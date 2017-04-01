using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Unity's Mathf and Vector3/Vector2 don't cover all the operations that need to be computed
    /// for the game; that's where Math2d comes in. Whenever a 2d computation needs to happen to
    /// help the programmer, then it goes here. Many methods will use 3d vectors as Unity forces 2d
    /// objects to use the vector. The z axis is assumed to be its layer and will be stripped away for
    /// all 2d methods.
    /// </summary>
    public static class Math2d
    {
        /// <summary>
        /// This will rotate a 2d vector by the given degree.
        /// </summary>
        /// <param name="vec">The vector to rotate</param>
        /// <param name="degree">The angle, in degrees, to rotate</param>
        /// <returns>The rotated vector</returns>
        public static Vector2 RotateVector(Vector2 vec, float degree)
        {
            float theta = Mathf.Deg2Rad * degree;
            float oldVectorX = vec.x;
            vec.x = vec.x * Mathf.Cos(theta) - vec.y * Mathf.Sin(theta);
            vec.y = oldVectorX * Mathf.Sin(theta) + vec.y * Mathf.Cos(theta);
            return vec;
        }

        /// <summary>
        /// In order to save memory, this method will rotate the
        /// vector directly.
        /// </summary>
        /// <param name="vec">The reference to the vector.</param>
        /// <param name="degree">The angle, in degrees, to rotate it</param>
        public static void RotateVector(ref Vector2 vec, float degree)
        {
            float theta = Mathf.Deg2Rad * degree;
            float oldVectorX = vec.x;
            vec.x = vec.x * Mathf.Cos(theta) - vec.y * Mathf.Sin(theta);
            vec.y = oldVectorX * Mathf.Sin(theta) + vec.y * Mathf.Cos(theta);
        }

        /// <summary>
        /// Returns a random unit vector2 between degMin and degMax where degMin
        /// and degMax are angles in degrees.
        /// </summary>
        /// <param name="degMin">The minimum possible angle of the returned vector</param>
        /// <param name="degMax">The maximum possible angle of the returned vector</param>
        public static Vector2 RandomUnitVector(float degMin, float degMax)
        {
            float angle = UnityEngine.Random.Range(degMin, degMax);
            return Quaternion.Euler(0, 0, angle) * Vector2.right;
        }

        /// <summary>
        /// This takes in an object and determines if it is in the given cone.
        /// </summary>
        /// <param name="objectPosition">position of the object</param>
        /// <param name="conePosition">origin position of the cone.</param>
        /// <param name="coneNormal">the direction of the cone.</param>
        /// <param name="coneRadius">The radius of the cone.</param>
        /// <param name="coneAngle">The angle of the cone.</param>
        /// <returns>IF he object is in the cone or not.</returns>
        public static bool ObjectInCone(Vector2 objectPosition, Vector2 conePosition, Vector2 coneNormal, float coneRadius, float coneAngle)
        {
            Vector2 coneToObject = (objectPosition - conePosition);
            float distanceFromCone = Vector2.Distance(objectPosition, conePosition);
            float angleFromConeNormal = Mathf.Abs(Vector2.Angle(coneToObject, coneNormal));

            return distanceFromCone <= coneRadius && angleFromConeNormal <= coneAngle / 2;
        }

        /// <summary>
        /// This takes in two rays (a point and a direction) and finds the point of intersection between
        /// the two, if there is an intersection. The method uses Cramer's Rule to solve the system of
        /// linear equations to find the unknown (x, y) value. The operation doesn't use Unity's Ray2d
        /// class because the direction property normalizes the vector which throws off the calculation
        /// due to floating point error.
        /// </summary>
        /// <param name="intersection">The point where the two rays intersect</param>
        /// <param name="position1">The origin position for the first ray.</param>
        /// <param name="direction1">The direction of the first ray.</param>
        /// <param name="position2">The origin position for the second ray.</param>
        /// <param name="direction2">The direction of the second ray.</param>
        /// <returns>If the two rays intersect</returns>
        public static bool RayRayIntersection(out Vector2 intersection, Vector2 position1, Vector2 direction1, Vector2 position2, Vector2 direction2)
        {
            const int VECTOR_SIZE = 2;

            var coefficient = new Matrix2x2();
            var answers = new Vector2();
            intersection = new Vector2();

            coefficient[0, 0] = direction1.x;
            coefficient[0, 1] = -direction2.x;
            coefficient[1, 0] = direction1.y;
            coefficient[1, 1] = -direction2.y;

            answers[0] = position2.x - position1.x;
            answers[1] = position2.y - position1.y;

            float det = coefficient.determinant;

            if (det == 0)
                return false;

            for (int col = 0; col < VECTOR_SIZE; col++)
            {
                Vector2 originalColumn = coefficient.GetCol(col);
                coefficient.SetCol(col, answers);
                intersection[col] = coefficient.determinant / det;
                coefficient.SetCol(col, originalColumn);
            }
            return true;
        }

        /// <summary>
        /// This will take a list of Game Objects and return the geometric
        /// mean of the swarm.
        /// </summary>
        /// <param name="gameObjects">List of Game Objects</param>
        /// <returns>The Geometric Mean of the list.</returns>
        public static Vector2 GeometricMean(List<GameObject> gameObjects)
        {
            var geometricMean = new Vector2();

            foreach (GameObject minion in gameObjects)
            {
                geometricMean.x += minion.transform.position.x / gameObjects.Count;
                geometricMean.y += minion.transform.position.y / gameObjects.Count;
            }

            return geometricMean;
        }

        /// <summary>
        /// Given two positions, this will determine if the origin position can see the
        /// target position, meaning that a raytrace can go straight from the origin to the
        /// target without hitting any colliders. The trace will only go out as far as the
        /// max distance that is allowed.
        /// </summary>
        /// <param name="distance">The distance from the origin to the target</param>
        /// <param name="maxDistance">The maximum distance that is allowed</param>
        /// <param name="originPostion">The position of the origin that will see the target</param>
        /// <param name="targetPosition">The position of the target that is suppose to be seen.</param>
        /// <returns></returns>
        public static bool CanSeePosition(float distance, float maxDistance, Vector2 originPostion, Vector2 targetPosition)
        {
            bool canSee = false;
            Vector2 direction = ((targetPosition - originPostion)).normalized;
            float clampDistance = distance < maxDistance ? distance : maxDistance;
            var hit = Physics2D.Raycast(originPostion, direction, clampDistance, 1 << LayerMask.NameToLayer("Background"));
            if (hit.collider == null)
            {
                canSee = true;
            }
            return canSee;
        }

        /// <summary>
        /// Given two ranges, that do no share the same range or the same 1:1 output, this will convert
        /// the index of the 1st range so that it outputs a number in the second range. F:R1(i) -> R2(i1)
        /// </summary>
        /// <param name="range1Start">The start value of the 1st range</param>
        /// <param name="range1End">The end value of the 1st range</param>
        /// <param name="range2Start">The start value of the 2nd range</param>
        /// <param name="range2End">The end value of the 2nd range</param>
        /// <param name="index">The number in the first range that needs to be mapped to the second range.</param>
        /// <returns>The number in the second range that was mapped from the first range.</returns>
        public static float ConvertToRange(float range1Start, float range1End, float range2Start, float range2End, float index)
        {
            if (index < range1Start || index > range1End)
                throw new ArgumentException("Index must be between the 1st range.");

            float range1 = range1End - range1Start;
            float range2 = range2End - range2Start;
            float ratio = range2 / range1;
            float newValue = index * ratio;
            return newValue + range2Start;
        }
    }
}