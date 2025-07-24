
namespace Flyleaf.Common
{
    public interface ILiveControl
    {
        Action<Status>? StatusAction { get; set; }
        Action OpenCompleted { get; set; }
        void PlayLive(string url);
        void StopLive();
        void SetCameraUrl(string url);

        void TakeSnapShot(string fileName);

        object GetInstance();

    }

    public enum Status
    {
        Opening,
        Failed,
        Stopped,
        Paused,
        Playing,
        Ended
    }
}
