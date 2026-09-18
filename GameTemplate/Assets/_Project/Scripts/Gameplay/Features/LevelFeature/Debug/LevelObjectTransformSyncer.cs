#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [ExecuteAlways]
    public class LevelObjectTransformSyncer : SerializedMonoBehaviour
    {
        [NonSerialized]
        [OdinSerialize]
        [ReadOnly]
        private LevelObjectData _linkedData;

        public LevelObjectData LinkedData
        {
            get => _linkedData;
            set => _linkedData = value;
        }

        private void Update()
        {
            if (_linkedData == null || !Application.isEditor || Application.isPlaying)
                return;

            bool changed = false;

            if (transform.position != _linkedData.Position)
            {
                _linkedData.Position = transform.position;
                changed = true;
            }

            if (changed)
            {
                UnityEditor.EditorUtility.SetDirty(this);

                LevelEditor editor = GetComponentInParent<LevelEditor>();

                if (editor != null)
                {
                    LevelObjectData obj = editor.LevelObjects
                        .Find(obj => obj.View?.gameObject == gameObject);

                    if (obj != null)
                    {
                        obj.Position = transform.position;
                        UnityEditor.EditorUtility.SetDirty(editor);
                    }
                }
            }
        }
    }
}
#endif