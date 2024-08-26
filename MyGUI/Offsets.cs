using System.Numerics;

namespace MyGUI.Offsets
{
    public static class Offsets
    {
        public const Int32 BaseOffset = 0x0AA5EA0;
        public static List<Player> AllPlayers = new List<Player>();
    }

    public struct Player
    {
        private const Int32 BaseOffset = 0x0AA5EA0;
        private const Int32 _offset = 0x5A8;
        public IntPtr NickName { get; set; }
        public IntPtr Health { get; set; }
        public IntPtr Position { get; set; }
        public IntPtr MoveVector { get; set; }
        public IntPtr AmmoInFirstWeapon { get; set; }
        public IntPtr FaceVision { get; set; }
        public float Distance { get; set; }
        public float GetDistance(Player MainPlayer)
        {
            return Vector3.Distance(Program._swed.ReadVec(this.Position), Program._swed.ReadVec(MainPlayer.Position));
        }
        public Player()
        {
            this.NickName = Program._swed.ReadPointer(Program._moduleBase + BaseOffset)+(0x200 + (_offset * Offsets.AllPlayers.Count()));
            this.Health = Program._swed.ReadPointer(Program._moduleBase + BaseOffset) + (0x0B8 + (_offset * Offsets.AllPlayers.Count()));
            this.Position = Program._swed.ReadPointer(Program._moduleBase + BaseOffset) + (0x14 + (_offset * Offsets.AllPlayers.Count()));
            this.MoveVector = Program._swed.ReadPointer(Program._moduleBase + BaseOffset) + (0x020 + (_offset * Offsets.AllPlayers.Count()));
            this.AmmoInFirstWeapon = Program._swed.ReadPointer(Program._moduleBase + BaseOffset) + (0x180 + (_offset * Offsets.AllPlayers.Count()));
            this.FaceVision = Program._swed.ReadPointer(Program._moduleBase + BaseOffset) + (0x098 + (_offset * Offsets.AllPlayers.Count()));
            Offsets.AllPlayers.Add(this);
        }
    }


}
