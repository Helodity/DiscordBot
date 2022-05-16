namespace DiscordBotRewrite.Global;

public readonly struct Cooldown {
    readonly DateTime EndTime;
    public Cooldown(DateTime endTime) {
        EndTime = endTime;
    }
    public bool IsOver => DateTime.Compare(DateTime.Now, EndTime) >= 0;

    public static bool UserHasCooldown(ulong userId, ref Dictionary<ulong, Cooldown> cooldowns) {
        if(cooldowns.TryGetValue(userId, out Cooldown cooldown)) {
            if(!cooldown.IsOver)
                return true;
            cooldowns.Remove(userId);
        }
        return false;
    }
    public static TimeSpan TimeUntilExpiration(ulong userId, ref Dictionary<ulong, Cooldown> cooldowns) {
        if(cooldowns.TryGetValue(userId, out Cooldown cooldown)) {
            if(!cooldown.IsOver)
                return cooldown.TimeUntilExpiration();
            cooldowns.Remove(userId);
        }
        return TimeSpan.Zero;
    }

    public TimeSpan TimeUntilExpiration() {
        return EndTime - DateTime.Now;
    }
}
