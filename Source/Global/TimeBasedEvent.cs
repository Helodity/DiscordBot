namespace DiscordBotRewrite.Global;

public class TimeBasedEvent {

    DateTime RunTime;

    Action OnTimer;

    bool RunIfTimeIsAlreadyExpired;

    public TimeBasedEvent(int duration, TimeUnit unit, Action onEnd, bool runIfAlreadyExpired = true) {
        RunTime = DateTime.Now.AddTime(duration, unit);
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

        while(DateTime.Compare(DateTime.Now, RunTime) < 0) {
            await Task.Delay(1);
        }

        OnTimer.Invoke();
    }
}
