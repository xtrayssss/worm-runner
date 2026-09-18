using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Services
{
    public static class ConeMeshGenerator
    {
        public enum ConeDirection
        {
            FORWARD,
            BACKWARD,
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        private const int DEFAULT_PRECISION = 25;

         public static Mesh CreateConeMesh(
            float range,
            float angle,
            int precision = DEFAULT_PRECISION,
            float startDistance = 0f,
            ConeDirection direction = ConeDirection.FORWARD)
        {
            if (Mathf.Approximately(angle, 360f) || angle >= 360f)
            {
                startDistance = 0f;
            }

            Mesh mesh = new Mesh();

            float halfAngle = angle * 0.5f;
            float startRadians = (360.0f - halfAngle) * Mathf.Deg2Rad;
            float span = halfAngle / precision * Mathf.Deg2Rad * 2.0f;

            int vertCount = (precision + 1) * 2;
            int triCount = precision * 2;

            Vector3[] vertices = new Vector3[vertCount];
            int[] triangles = new int[triCount * 3];
            Vector3[] normals = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];

            for (int i = 0; i < vertCount; i++)
            {
                normals[i] = Vector3.up;
            }

            float currentRadians = startRadians;
            Vector3 baseDirection = GetBaseDirection(direction);

            int vertIndex = 0;
            bool isFullCircle = Mathf.Approximately(angle, 360f) || angle >= 360f;
            
            for (int i = 0; i < precision + 1; i++)
            {
                Vector3 rotatedDirection = RotateDirection(baseDirection, currentRadians, direction);

                if (isFullCircle)
                {
                    // Для полного круга используем радиальные UV (как раньше)
                    vertices[vertIndex] = rotatedDirection * startDistance;
                    uvs[vertIndex] = new Vector2(
                        0.5f + rotatedDirection.x * (startDistance / range) * 0.5f,
                        0.5f + rotatedDirection.z * (startDistance / range) * 0.5f
                    );
                    vertIndex++;

                    vertices[vertIndex] = rotatedDirection * range;
                    uvs[vertIndex] = new Vector2(
                        0.5f + rotatedDirection.x * 0.5f,
                        0.5f + rotatedDirection.z * 0.5f
                    );
                    vertIndex++;
                }
                else
                {
                    // Для частичного конуса используем линейные UV
                    float normalizedAngle = (float)i / precision;
                    float normalizedInnerDist = startDistance / range;

                    vertices[vertIndex] = rotatedDirection * startDistance;
                    uvs[vertIndex] = new Vector2(normalizedAngle, normalizedInnerDist);
                    vertIndex++;

                    vertices[vertIndex] = rotatedDirection * range;
                    uvs[vertIndex] = new Vector2(normalizedAngle, 1.0f);
                    vertIndex++;
                }

                currentRadians += span;
            }

            int triIndex = 0;
            int localIndex = 0;

            for (int i = 0; i < triCount; i += 2)
            {
                triangles[triIndex++] = localIndex;
                triangles[triIndex++] = localIndex + 3;
                triangles[triIndex++] = localIndex + 1;

                triangles[triIndex++] = localIndex + 2;
                triangles[triIndex++] = localIndex + 3;
                triangles[triIndex++] = localIndex;

                localIndex += 2;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.name = $"ConeMesh_{direction}_R{range}_A{angle}";

            return mesh;
        }

        private static Vector3 GetBaseDirection(ConeDirection direction)
        {
            return direction switch
            {
                ConeDirection.FORWARD => Vector3.forward,
                ConeDirection.BACKWARD => Vector3.back,
                ConeDirection.LEFT => Vector3.left,
                ConeDirection.RIGHT => Vector3.right,
                ConeDirection.UP => Vector3.up,
                ConeDirection.DOWN => Vector3.down,
                _ => Vector3.forward
            };
        }

        private static Vector3 RotateDirection(Vector3 baseDir, float radians, ConeDirection direction)
        {
            // For horizontal directions (forward, back, left, right), rotate around Y axis
            if (direction == ConeDirection.FORWARD || direction == ConeDirection.BACKWARD ||
                direction == ConeDirection.LEFT || direction == ConeDirection.RIGHT)
            {
                float newX = baseDir.x * Mathf.Cos(radians) - baseDir.z * Mathf.Sin(radians);
                float newZ = baseDir.x * Mathf.Sin(radians) + baseDir.z * Mathf.Cos(radians);
                return new Vector3(newX, 0.0f, newZ).normalized;
            }
            // For vertical directions (up, down), rotate around X axis
            else
            {
                float newY = baseDir.y * Mathf.Cos(radians) - baseDir.z * Mathf.Sin(radians);
                float newZ = baseDir.y * Mathf.Sin(radians) + baseDir.z * Mathf.Cos(radians);
                return new Vector3(0.0f, newY, newZ).normalized;
            }
        }

        public static Mesh CreateCircleMesh(
            float radius,
            int precision = DEFAULT_PRECISION,
            float innerRadius = 0f,
            ConeDirection direction = ConeDirection.FORWARD)
        {
            return CreateConeMesh(radius, 360f, precision, innerRadius, direction);
        }

        public static Mesh CreateRingMesh(
            float outerRadius,
            float innerRadius,
            int precision = DEFAULT_PRECISION,
            ConeDirection direction = ConeDirection.FORWARD)
        {
            return CreateConeMesh(outerRadius, 360f, precision, innerRadius, direction);
        }

        public static void UpdateMesh(
            MeshFilter meshFilter,
            float range,
            float angle,
            int precision = DEFAULT_PRECISION,
            float startDistance = 0f,
            ConeDirection direction = ConeDirection.FORWARD)
        {
            if (meshFilter == null)
                return;

            Mesh newMesh = CreateConeMesh(range, angle, precision, startDistance, direction);

            if (meshFilter.mesh != null)
                Object.Destroy(meshFilter.mesh);

            meshFilter.mesh = newMesh;
        }
    }
}