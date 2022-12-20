using SQLite;

namespace DiscordBotRewrite.Modules {

    [Table("votes")]
    public class Vote {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        //Id of the message this poll belongs to
        [Column("poll_id")]
        public long PollId { get; set; }
        //Id of the voter
        [Column("voter_id")]
        public long VoterId { get; set; }
        //What the voter chose
        [Column("choice")]
        public string Choice { get; set; }

        #region Constructor
        public Vote() { }
        public Vote(long pollId, long voterId, string choice) {
            PollId = pollId;
            VoterId = voterId;
            Choice = choice;
        }
        #endregion
    }
}