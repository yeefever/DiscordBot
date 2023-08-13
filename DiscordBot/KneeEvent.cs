namespace DiscordBot
{
    public class KneeEvent
    {
        public string event_name { get; set; }
        public ulong creator_id { get; set; }

        public string start_date { get; set; }
        public List<ulong> invited { get; set; }
        public string event_desc { get; set; }

        public KneeEvent()
        {
           
        }
    }
}