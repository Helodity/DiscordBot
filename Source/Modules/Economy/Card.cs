using System.Collections.Generic;

namespace DiscordBotRewrite.Modules {
    public class Card {
        public enum Suit {
            Spades,
            Hearts,
            Diamonds,
            Clubs
        }
        public enum Value {
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Ten,
            Jack,
            Queen,
            King,
            Ace
        }

        public readonly Suit suit;
        public readonly Value value;
        public Card(Suit s, Value v) {
            suit = s;
            value = v;
        }

        public override string ToString() {
            string output = string.Empty;

            output += value switch {
                Value.Two => "2",
                Value.Three => "3",
                Value.Four => "4",
                Value.Five => "5",
                Value.Six => "6",
                Value.Seven => "7",
                Value.Eight => "8",
                Value.Nine => "9",
                Value.Ten => "10",
                Value.Jack => "J",
                Value.Queen => "Q",
                Value.King => "K",
                Value.Ace => "A",
                _ => "",
            };
            output += suit switch {
                Suit.Spades => "♤",
                Suit.Hearts => "♡",
                Suit.Diamonds => "♢",
                Suit.Clubs => "♧",
                _ => "",
            };


            return output;
        }
        public static string ListToString(List<Card> cards) {
            string str = string.Empty;
            foreach(Card c in cards) {
                str += c.ToString() + " ";
            }
            return str.Trim();
        }
    }
}