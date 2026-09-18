#if UNITY_EDITOR
using System.Collections.Generic;
using Kamgam.ExcludeFromBuild;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.EditorTools
{
    public sealed class PointMarker : MonoBehaviour
    {
        [Header("Visualization")]
        [SerializeField]
        private Color _gizmoColor = Color.green;

        [SerializeField]
        private float _sphereRadius = 0.5f;

        [SerializeField]
        private bool _showLabel;

        [SerializeField]
        private bool _showDirectionArrow;

        [SerializeField]
        private float _arrowLength = 2f;

        [Header("Information")]
        [SerializeField]
        private string _markerLabel = "Spawn Point";

        private void Reset()
        {
            ExcludeFromBuildComponent exclude = GetComponent<ExcludeFromBuildComponent>();

            if (exclude == null)
                exclude = gameObject.AddComponent<ExcludeFromBuildComponent>();

            exclude.GameObject = false;
            exclude.AllGroups = true;

            exclude.Components ??= new List<Component>();

            if (!exclude.Components.Contains(this))
            {
                exclude.Components.Add(this);
                EditorUtility.SetDirty(exclude);
            }

            hideFlags = HideFlags.DontSaveInBuild;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.3f);
            Gizmos.DrawSphere(transform.position, _sphereRadius);

            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _sphereRadius);

            if (_showDirectionArrow)
            {
                DrawArrow(transform.position, transform.forward * _arrowLength, _gizmoColor);
            }

            float axisLength = _sphereRadius * 1.5f;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.right * axisLength);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * axisLength);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * axisLength);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawSphere(transform.position, _sphereRadius * 1.2f);

            Handles.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.1f);
            Handles.DrawSolidDisc(transform.position, Vector3.up, _sphereRadius * 2f);

            if (_showLabel)
            {
                GUIStyle style = new GUIStyle
                {
                    normal = { textColor = Color.white },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                Vector3 labelPosition = transform.position + Vector3.up * (_sphereRadius + 0.5f);
                Handles.Label(labelPosition, _markerLabel, style);
            }
        }

        private void DrawArrow(Vector3 start, Vector3 direction, Color color)
        {
            Gizmos.color = color;
            Vector3 end = start + direction;

            Gizmos.DrawLine(start, end);

            float arrowHeadLength = 0.3f;
            float arrowHeadAngle = 20f;

            Vector3 right = Quaternion.LookRotation(direction) *
                            Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                            Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) *
                           Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                           Vector3.forward;

            Gizmos.DrawLine(end, end + right * arrowHeadLength);
            Gizmos.DrawLine(end, end + left * arrowHeadLength);
        }
    }
}
#endif