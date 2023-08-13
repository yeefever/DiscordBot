namespace DiscordBot
{
    public class ULongPair
    {
        public ulong First { get; set; }
        public ulong Second { get; set; }

        public ULongPair(ulong first, ulong second)
        {
            First = first;
            Second = second;
        }
    }
}