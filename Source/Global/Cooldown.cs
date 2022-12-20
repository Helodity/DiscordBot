using SQLite;

namespace DiscordBotRewrite.Global {
    [Table("cooldowns")]
    public class Cooldown {

        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public long UserID { get; set; }

        [Column("cooldown_id")]
        public string CooldownID { get; set; }

        [Column("endtime")]
        public DateTime EndTime { get; set; }

        public bool IsOver => DateTime.Compare(DateTime.Now, EndTime) >= 0;

        public Cooldown() { }


        public Cooldown(long userID, string cooldownID) {
            UserID = userID;
            CooldownID = cooldownID;
            EndTime = DateTime.Now;
        }
        public void SetEndTime(DateTime time, bool update = true) {
            EndTime = time;
            if(update)
                Bot.Database.Update(this);
        }
        public static Cooldown GetCooldown(long userID, string cooldownID) {
            Cooldown cooldown = Bot.Database.Table<Cooldown>().FirstOrDefault(x => x.UserID == userID && x.CooldownID == cooldownID);
            if(cooldown == null) {
                cooldown = new Cooldown(userID, cooldownID);
                Bot.Database.Insert(cooldown);
            }
            return cooldown;
        }

        public static bool UserHasCooldown(long userID, string cooldownID) {
            Cooldown c = GetCooldown(userID, cooldownID);
            return c.IsOver;
        }

    }
}