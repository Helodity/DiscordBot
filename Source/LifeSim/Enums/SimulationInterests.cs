namespace DiscordBotRewrite.LifeSim
{
    [Flags]
    public enum SimulationInterests
    {
        None = 0,
        Sports = 1,
        Animals = 1 << 1,
        Science = 1 << 2,
        Literature = 1 << 3,
        Fashion = 1 << 4,
        Art = 1 << 5,
        Gaming = 1 << 6,
        History = 1 << 7,
        SIZE = 1 << 8
    }
}