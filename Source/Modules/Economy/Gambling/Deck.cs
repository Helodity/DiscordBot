using System;
using System.Collections.Generic;
using System.Linq;
using DiscordBotRewrite.Extensions;

namespace DiscordBotRewrite.Modules.Economy.Gambling {
    public class Deck {
        List<Card> Cards;

        public Deck() {
            Cards = new();
        }
        public static Deck GetStandardDeck() {
            Deck d = new Deck();

            foreach(Card.Suit s in Enum.GetValues(typeof(Card.Suit))) {
                foreach(Card.Value v in Enum.GetValues(typeof(Card.Value))) {
                    d.AddTop(new Card(s, v));
                }
            }
            d.Shuffle();
            return d;
        }
        public Card Draw() {
            if(Cards.Count == 0)
                return null;
            Card c = Cards.First();
            Cards.RemoveAt(0);
            return c;
        }
        public List<Card> Draw(int amount) {
            List<Card> l = new List<Card>();
            for(int i = 0; i < amount; i++) {
                if(Cards.Count == 0)
                    break;
                Card c = Cards.First();
                Cards.RemoveAt(0);
                l.Add(c);
            }

            return l;
        }
        public int Size() {
            return Cards.Count;
        }

        public void Shuffle() {
            Cards.Randomize();
        }
        public void AddTop(Card c) {
            Cards.Insert(0,c);
        }
        public void AddBottom(Card c) {
            Cards.Add(c);
        }
    }
}