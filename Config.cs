namespace UNO
{
    public static class Config
    {
        /// <summary>
        /// Should the bot connect to the Top.GG API?
        /// </summary>
        public static bool USE_TOP_GG_API = false;

        /// <summary>
        /// Are we in debug mode? (Register commands to guild and use test bot token)
        /// </summary>
        public static bool IS_DEBUG = true;

        /// <summary>
        /// The Guild to register guild commands to when debugging
        /// </summary>
        public static ulong DEBUG_GUILD_ID = 811398463685918790;

        /// <summary>
        /// The ID of the bot
        /// </summary>
        public static ulong BOT_CLIENT_ID = 1108623413855141908;

        /// <summary>
        /// It's recommended to have 1 shard per 1500-2000 guilds your bot is in.
        /// </summary>
        public static int DISCORD_SHARD_COUNT = 1;
    }
}
