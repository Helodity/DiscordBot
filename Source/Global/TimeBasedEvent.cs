using System;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;

namespace DiscordBotRewrite.Global {
    public class TimeBasedEvent {
        #region Properties
        TimeSpan Duration;
        Action OnTimer;
        bool Cancelled = false;
        #endregion

        #region Constructors
        public TimeBasedEvent(TimeSpan duration, Action onEnd) {
            Duration = duration;
            OnTimer = onEnd;
        }
        #endregion

        #region Public
        public async void Start() {
            DateTime runTime = DateTime.Now + Duration;

            while(DateTime.Compare(DateTime.Now, runTime) < 0 && !Cancelled) {
                await Task.Delay(1);
            }

            if(!Cancelled) {
                OnTimer.Invoke();
            }
        }

        public void Cancel() {
            Cancelled = true;
        }
        #endregion
    }
}