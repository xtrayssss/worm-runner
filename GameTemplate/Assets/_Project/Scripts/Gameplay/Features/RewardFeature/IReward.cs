namespace _Project.Scripts.Gameplay.Features.RewardFeature
{
    public interface IReward
    {
        RewardType Type { get; }
        bool IsEmpty { get; }
        IReward Combine(IReward other);
        IReward Copy();
        void GenerateFinalValues();
        IReward Multiply(float multiplier);
    }
}