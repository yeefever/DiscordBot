namespace DiscordBot
{
    public class KneeEvent
    {
        public string event_name { get; set; }
        public ulong creator_id { get; set; }

        public string event_id { get; set; }

        public string start_date { get; set; }
        public List<string> invited { get; set; }
        public string event_desc { get; set; }

        public Dictionary<string, ulong[]> userIdMessageId { get; set; } // user id, dm channel id, message id : )

        public KneeEvent()
        {
           
        }
    }
}