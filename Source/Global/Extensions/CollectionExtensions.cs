﻿namespace DiscordBotRewrite.Global.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(
            this Dictionary<TKey, TValue> dict,
            TKey key,
            TValue value)
        {
            if (dict.TryGetValue(key, out _))
            {
                dict.Remove(key);
            }
            dict.Add(key, value);
        }
        public static void Randomize<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = GenerateRandomNumber(0, n);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}