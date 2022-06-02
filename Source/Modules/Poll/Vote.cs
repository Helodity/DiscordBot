namespace DiscordBotRewrite.Modules {
    public readonly struct Vote {
        #region Properties
        public readonly ulong VoterId;
        public readonly string Choice;
        #endregion

        #region Constructor
        public Vote(ulong id, string choice) {
            VoterId = id;
            Choice = choice;
        }
        #endregion
    }
}