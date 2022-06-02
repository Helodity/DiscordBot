using System;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;

namespace DiscordBotRewrite.Global {
    public class TimeBasedEvent {

        TimeSpan Duration;
        Action OnTimer;
        bool Cancelled = false;

        public TimeBasedEvent(TimeSpan duration, Action onEnd) {
            Duration = duration;
            OnTimer = onEnd;
        }

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

    }
}