using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using PrimeTween;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.VFXFeature.Services
{
    public sealed class VFXService : IService
    {
        private readonly Transform _vfxParent;
        private readonly AudioService _audioService;

        public VFXService(AudioService audioService, Transform vfxParent = null)
        {
            _vfxParent = vfxParent;
            _audioService = audioService;
        }

        public void PlayVFX(VFXData vfxData, Vector3 position, Transform target = null, float? scale = null)
        {
            Vector3 spawnPosition = position + vfxData.SpawnOffset;

            if (vfxData.Delay > 0f)
                Tween.Delay(_vfxParent, vfxData.Delay, () => SpawnVFX(vfxData, spawnPosition, target, scale));
            else
                SpawnVFX(vfxData, spawnPosition, target, scale);
        }

        private GameObject SpawnVFX(VFXData vfxData, Vector3 position, Transform target, float? scale)
        {
            GameObject vfxObject = Object.Instantiate(vfxData.VFXPrefab, _vfxParent);

            Vector3 finalScale;

            if (scale.HasValue)
                finalScale = new Vector3(scale.Value, scale.Value, scale.Value);
            else if (vfxData.CustomScale.HasValue)
                finalScale = vfxData.CustomScale.Value;
            else
                finalScale = vfxData.VFXPrefab.transform.localScale;

            vfxObject.transform.position = position;
            vfxObject.transform.localScale = finalScale;
            vfxObject.transform.rotation = vfxData.CustomRotation.HasValue
                ? Quaternion.Euler(vfxData.CustomRotation.Value)
                : vfxData.VFXPrefab.transform.rotation;

            if (vfxData.FollowTarget && target != null)
                vfxObject.transform.SetParent(target);

            if (!string.IsNullOrEmpty(vfxData.SoundId))
                PlaySound(vfxData.SoundId, position, vfxData.Is3D);

            if (vfxData.VFXDuration > 0f)
                Object.Destroy(vfxObject, vfxData.VFXDuration);

            return vfxObject;
        }

        public void PlayVFXUI(
            VFXData vfxData,
            RectTransform parent,
            Vector2 anchoredPosition = default,
            float? scale = null)
        {
            if (vfxData.Delay > 0f)
                Tween.Delay(parent, vfxData.Delay, () => SpawnVFXUI(vfxData, parent, anchoredPosition, scale));
            else
                SpawnVFXUI(vfxData, parent, anchoredPosition, scale);
        }

        private GameObject SpawnVFXUI(
            VFXData vfxData,
            RectTransform parent,
            Vector2 anchoredPosition,
            float? scale)
        {
            GameObject vfxObject = Object.Instantiate(vfxData.VFXPrefab, parent);

            RectTransform rect = vfxObject.GetComponent<RectTransform>();

            rect.anchoredPosition = anchoredPosition + (Vector2)vfxData.SpawnOffset;

            Vector3 finalScale;
            
            if (scale.HasValue)
                finalScale = new Vector3(scale.Value, scale.Value, scale.Value);
            else if (vfxData.CustomScale.HasValue)
                finalScale = vfxData.CustomScale.Value;
            else
                finalScale = vfxData.VFXPrefab.transform.localScale;

            vfxObject.transform.localScale = finalScale;
            vfxObject.transform.localRotation = vfxData.CustomRotation.HasValue
                ? Quaternion.Euler(vfxData.CustomRotation.Value)
                : vfxData.VFXPrefab.transform.localRotation;

            if (!string.IsNullOrEmpty(vfxData.SoundId))
                PlaySound(vfxData.SoundId, parent.position, vfxData.Is3D);

            if (vfxData.VFXDuration > 0f)
                Object.Destroy(vfxObject, vfxData.VFXDuration);

            return vfxObject;
        }

        private void PlaySound(string soundId, Vector3 position, bool is3D)
        {
            if (is3D)
                _audioService.PlaySound3D(soundId, position);
            else
                _audioService.PlaySound(soundId);
        }
    }
}