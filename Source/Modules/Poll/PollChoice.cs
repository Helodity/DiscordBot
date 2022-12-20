using SQLite;

namespace DiscordBotRewrite.Modules {

    [Table("choices")]
    public class PollChoice {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        //Id of the message this poll belongs to
        [Column("poll_id")]
        public long PollId { get; set; }
        //What the voter chose
        [Column("name")]
        public string Name { get; set; }

        #region Constructor
        public PollChoice() { }
        public PollChoice(long pollId, string name) {
            PollId = pollId;
            Name = name;
        }
        #endregion
    }
}