using SQLite;

namespace DiscordBotRewrite.Quote
{
    [Table("quotes")]
    public class Quote
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        //The message that the bot sends
        [Column("quote_message")]
        public long QuoteMessage { get; set; }

        //The message being quoted by the bot
        [Column("original_message")]
        public long OriginalMessage { get; set; }

        public Quote() { }

        public Quote(long quote, long original)
        {
            QuoteMessage = quote;
            OriginalMessage = original;
        }
    }
}