using System;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class WorldCharacter : WorldData
    {
        public const string TABLE = "Characters";
        public int UserId { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Map { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Class { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int UnspentTalentPoints { get; set; }
        public string Sprite { get; set; }
        public int MaxInventorySlots { get; set; }
        public int Gold { get; set; }
        public string Checkpoint { get; set; }
    }
}