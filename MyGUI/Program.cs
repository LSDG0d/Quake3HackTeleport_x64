using ClickableTransparentOverlay;
using ImGuiNET;
using Swed64;
using System.Numerics;
namespace MyGUI
{
    public class SavePoint
    {
        public string title { get; set; } = "";
        public Vector3 position { get; set; }
    }
    public class Program : Overlay
    {
        List<SavePoint> savePoints = new List<SavePoint>();
        static public Swed swed = new Swed("quake3e-vulkan.x64");
        static public IntPtr moduleBase = IntPtr.Zero;
        static public IntPtr posAddress = IntPtr.Zero;
        private static void Main(string[] args)
        {
            moduleBase = swed.GetModuleBase("quake3e-vulkan.x64.exe");
            posAddress = swed.ReadPointer(moduleBase, 0x0AA5EA0) + 0x5BC;
            Program program = new Program();
            program.Start();
        }
        protected override void Render()
        {
            ImGui.Begin("MyGUI");
            if (ImGui.Button("First button"))
            {
                SaveCurrentPosition();
            }
            foreach (var point in savePoints)
            {
                if (ImGui.Button(point.title))
                {
                    TeleportToPosition(point.position);
                }
            }
        }
        private void TeleportToPosition(Vector3 position)
        {
            swed.WriteVec(posAddress, position);
        }
        private void SaveCurrentPosition()
        {
            Vector3 currentPlayerPosition = GetCurrentPlayerPosition();
            string savePointTitle = "Save Point " + (savePoints.Count + 1);
            SavePoint savePoint = new SavePoint { title = savePointTitle, position = currentPlayerPosition };
            savePoints.Add(savePoint);
        }
        private Vector3 GetCurrentPlayerPosition()
        {
            return swed.ReadVec(posAddress);
        }
    }
}
