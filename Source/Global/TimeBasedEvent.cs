namespace DiscordBotRewrite.Global {
    public class TimeBasedEvent {
        #region Properties
        readonly TimeSpan Duration;
        readonly Action OnTimer;
        readonly int CheckRate;
        bool Cancelled = false;
        bool Running = false;
        #endregion

        #region Constructors
        public TimeBasedEvent(TimeSpan duration, Action onEnd, int checkFrequency = 100) {
            Duration = duration;
            OnTimer = onEnd;
            CheckRate = checkFrequency;
        }
        #endregion

        #region Public
        public async void Start() {
            if(Running)
                return;

            Running = true;
            DateTime runTime = DateTime.Now + Duration;
            while(DateTime.Compare(DateTime.Now, runTime) < 0 && !Cancelled) {
                await Task.Delay(CheckRate);
            }
            //Reset variables to allow the event to restart
            Running = false;
            if(!Cancelled)
                OnTimer.Invoke();
            Cancelled = false;
        }

        public void Cancel() {
            if(Running) //Prevents us from cancelling a non existant event
                Cancelled = true;
        }
        #endregion
    }
}