using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ClearStatModifiersSystem : ISystem
    {
        public World World { get; set; }

        private Request<ClearStatModifiersRequest> _clearRequests;
        private readonly StatsService _statsService;

        public ClearStatModifiersSystem(StatsService statsService) =>
            _statsService = statsService;

        public void OnAwake() =>
            _clearRequests = World.GetRequest<ClearStatModifiersRequest>();

        public void OnUpdate(float deltaTime)
        {
            foreach (ClearStatModifiersRequest clearRequest in _clearRequests.Consume())
            {
                ref readonly Stats stats = ref clearRequest.Target.GetComponent<Stats>();

                if (clearRequest.StatId == StatId.UNKNOWN)
                {
                    foreach ((_, Stat stat) in stats.Value)
                        ClearModifiersForStat(stat, clearRequest);
                }
                else
                {
                    if (stats.Value.TryGetValue(clearRequest.StatId, out Stat stat))
                        ClearModifiersForStat(stat, clearRequest);
                }
            }
        }

        private void ClearModifiersForStat(Stat stat, ClearStatModifiersRequest clearRequest)
        {
            if (clearRequest.ClearBaseModifiers)
            {
                foreach (StatModifier modifier in stat.ActiveBaseModifiers) 
                    _statsService.RemoveModifier(modifier.ModifierEntity);
            }

            if (clearRequest.ClearCurrentModifiers)
            {
                foreach (StatModifier modifier in stat.ActiveCurrentModifiers) 
                    _statsService.RemoveModifier(modifier.ModifierEntity);
            }
        }

        public void Dispose()
        {
        }
    }
}