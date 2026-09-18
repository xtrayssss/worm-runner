using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.ToastNotificationFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Gameplay.Features.ToastNotificationFeature.Services
{
    public sealed class ToastService : IService
    {
        private readonly WindowService _windows;
        private readonly AudioService _audioService;

        public ToastService(WindowService windows, AudioService audioService)
        {
            _windows = windows;
            _audioService = audioService;
        }

        public TToast ShowToast<TToast>(WindowId toastId) where TToast : BaseToastNotification
        {
            TToast toast = _windows.OpenTemporaryPopup<TToast>(toastId);
            toast.Initialize(
                toastId,
                _audioService,
                onAnimationCompleted: () => _windows.CloseTemporaryPopup(toast).Forget());

            return toast;
        }
    }
}