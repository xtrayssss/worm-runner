using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class CaptureZoneView : InteractiveObjectView
    {
        [Header("Capture Zone")]
        [SerializeField]
        private Transform[] _memberSpawnPoints;

        [SerializeField]
        private Transform _membersContainer;

        [SerializeField]
        private TMP_Text _statusLabel;

        public Transform[] MemberSpawnPoints => _memberSpawnPoints;
        public Transform MembersContainer => _membersContainer;

        public void UpdateStatusLabel(int currentMembers, int requiredMembers) => 
            _statusLabel.text = $"{currentMembers}/{requiredMembers} <sprite index=0>";
    }
}