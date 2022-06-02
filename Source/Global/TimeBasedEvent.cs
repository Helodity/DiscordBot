using System;
using System.Threading.Tasks;
using DiscordBotRewrite.Extensions;

namespace DiscordBotRewrite.Global {
    public class TimeBasedEvent {

        DateTime RunTime;
        Action OnTimer;
        bool RunIfTimeIsAlreadyExpired;

        bool Cancelled = false;
        public TimeBasedEvent(TimeSpan duration, Action onEnd, bool runIfAlreadyExpired = true) {
            RunTime = DateTime.Now + duration;
            OnTimer = onEnd;
            RunIfTimeIsAlreadyExpired = runIfAlreadyExpired;
        }

        public TimeBasedEvent(DateTime endTime, Action onEnd, bool runIfAlreadyExpired = true) {
            RunTime = endTime;
            OnTimer = onEnd;
            RunIfTimeIsAlreadyExpired = runIfAlreadyExpired;
        }

        public async void Start() {
            if(!RunIfTimeIsAlreadyExpired) {
                if(DateTime.Compare(DateTime.Now, RunTime) >= 0)
                    return;
            }

            while(DateTime.Compare(DateTime.Now, RunTime) < 0 && !Cancelled) {
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