#if UNITY_EDITOR
using System.Text;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.MovementFeature.Components;
using Scellecs.Morpeh;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.EditorTools
{
    [InitializeOnLoad]
    public static class EditorPositionSynchronizer
    {
        private static Vector3 _lastPosition;
        private static Transform _selectedTransform;

        static EditorPositionSynchronizer()
        {
            EditorApplication.update += OnEditorUpdate;
            Selection.selectionChanged += OnSelectionChanged;   
        }

        private static void OnSelectionChanged()
        {
            _selectedTransform = Selection.activeTransform;
            if (_selectedTransform != null)
            {
                _lastPosition = _selectedTransform.position;
            }
        }

        private static void OnEditorUpdate()
        {
            if (_selectedTransform == null || !Application.isPlaying)
                return;

            if (_selectedTransform.position != _lastPosition)
            {
                _lastPosition = _selectedTransform.position;
                SyncPositionComponent(_selectedTransform);
            }
        }

        private static void SyncPositionComponent(Transform transform)
        {
            EntityView entityView = transform.GetComponent<EntityView>();
            if (entityView == null || entityView.Entity.IsNullOrDisposed())
                return;

            Entity entity = entityView.Entity;

            if (!entity.Has<Position>())
                return;

            bool wasControllerEnabled = false;

            bool hasController = entity.Has<CharacterControllerLink>();
            
            if (hasController)
            {
                ref CharacterControllerLink controllerLink = ref entity.GetComponent<CharacterControllerLink>();
            
                wasControllerEnabled = controllerLink.Controller.enabled; 
                controllerLink.Controller.enabled = false;
            }

            ref Position position = ref entity.GetComponent<Position>();
            position.Value = transform.position;

            if (hasController)
            {
                ref CharacterControllerLink controllerLink = ref entity.GetComponent<CharacterControllerLink>();
                controllerLink.Controller.enabled = wasControllerEnabled;
            }
        }
    }
}
#endif