using ClickableTransparentOverlay;
using ImGuiNET;
using MyGUI.Offsets;
using Swed64;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using _Offsets = MyGUI.Offsets.Offsets;
using Point = SixLabors.ImageSharp.Point;
namespace MyGUI
{
    public class SavePoint
    {
        public string Title { get; set; } = "";
        public Vector3 Position { get; set; }
    }
    public class Program : Overlay
    {
        #region Constantas
        private static Vector3 FreezeFacePoin = new Vector3(87f, 0f, 0f);
        public const string MyNick = "ZXClown";
        public static int MyIndex { get; set; } = 0;
        private static List<SavePoint> _savePoints = new List<SavePoint>();
        public static Swed _swed = new Swed("quake3e-vulkan.x64");
        public static IntPtr _moduleBase = IntPtr.Zero;
        public static Player MyPlayer;
        #endregion
        #region Main

        [STAThread]
        private static void Main(string[] args)
        {
            #region StartProgramm
            var hHook = SetHook();
            Console.WriteLine("App start. Main process = " + MainProcess.Id);
            _moduleBase = _swed.GetModuleBase("quake3e-vulkan.x64.exe");
            Program program = new Program();
            program.Start();
            Application.Run();
            Console.WriteLine("Unhook");
            UnhookWindowsHookEx(hHook);
            #endregion
        }
        #endregion
        #region Render
        protected override void Render()
        {
            ImGui.Begin("MyGUI");
            while (_Offsets.AllPlayers.Count() < 10)
            {
                Offsets.Player player = new Player();
            }
            if (ImGui.BeginMenu("Players") || true)
            {
                foreach (var player in _Offsets.AllPlayers.Select((Value, i) => new { i, Value }))
                {
                    string nana = $"{_swed.ReadString(player.Value.NickName, 7)}";
                    if (nana[0] == '\0') { continue; }
                    ImGui.Separator();
                    if (nana == MyNick)
                    {
                        MyIndex = player.i;
                        MyPlayer = (Player)player.Value;
                        ImGui.TextColored(new Vector4(1, 0, 1, 1), "It's me");
                    }
                    ImGui.Text($"Nick: {_swed.ReadString(player.Value.NickName, 7)}");
                    ImGui.Text($"\tHealth: {_swed.ReadInt(player.Value.Health)} \tAmmo: {_swed.ReadInt(player.Value.AmmoInFirstWeapon)}\n\tPosition: {_swed.ReadVec(player.Value.Position)}\n\tMove: {_swed.ReadVec(player.Value.MoveVector)}");
                    ImGui.Text($"\tLook: {_swed.ReadVec(player.Value.FaceVision)}");
                    ImGui.Text($"\tDistance: {player.Value.GetDistance(_Offsets.AllPlayers[MyIndex])}");
                }
            }
        }
        #endregion
        #region Teleport 

        private static void TeleportToPosition(Vector3 position)
        {
            _swed.WriteVec(MyPlayer.Position, position);
        }
        private static bool ListInitialize { get; set; } = false;

        private static void InitializeList()
        {
            Vector3 currentPlayerPosition = _swed.ReadVec(MyPlayer.Position);
            while (_savePoints.Count() <= 3)
            {
                SavePoint freeShit = new SavePoint { Position = currentPlayerPosition, Title = "Free slot" };
                _savePoints.Add(freeShit);
            }
            ListInitialize = true;
        }

        #endregion
        #region SaveCurrentPosition 

        private static void SaveCurrentPosition(int i)
        {
            if (!ListInitialize)
            {
                InitializeList();
            }

            Vector3 currentPlayerPosition = _swed.ReadVec(MyPlayer.Position);
            string savePointTitle = "Save Point " + (i + 1);
            SavePoint savePoint = new SavePoint { Title = savePointTitle, Position = currentPlayerPosition };
            if (_savePoints[i].Title == "Free slot")
            {
                _savePoints[i] = savePoint;
            }
            else
            {
                Console.WriteLine($"Teleport to {_savePoints[i].Position}");
                TeleportToPosition(_savePoints[i].Position);
            }
        }
        #endregion
        #region DllImports
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr lMod, int dwThreadId);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(IntPtr lpModuleName);
        [DllImport("user32.dll")]
        private static extern IntPtr MessageBox(IntPtr hWnd, string text, string caption, int options);
        [DllImport("user32.dll")]
        private static extern void SetWindowText(IntPtr hWnd, string text);
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point lpPoint);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        #endregion
        #region KeyBoardData
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProcDelegate _mCallback = LowLevelKeyboardHookProc;
        private static IntPtr _mHHook;
        #endregion
        #region LowLevelKeyboardProcDelegate
        private delegate IntPtr LowLevelKeyboardProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion
        #region LowLevelKeyboardHookProc
        private static IntPtr LowLevelKeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var khs = ((KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct)));
                if (khs.VirtualKeyCode == 45) //Insert
                {
                    Task.Run(() => MagnetPlayers());
                }
                if (khs.VirtualKeyCode == 46) //Delete
                {
                    UnFreezePlayers();
                }
                if (khs.VirtualKeyCode == 36) //Home
                {
                    FreezePlayers();
                }
                if (khs.VirtualKeyCode == 97) // num1
                {
                    SaveCurrentPosition(0);
                }
                if (khs.VirtualKeyCode == 98) // num2
                {
                    SaveCurrentPosition(1);
                }
                if (khs.VirtualKeyCode == 99) // num3
                {
                    SaveCurrentPosition(2);
                }
                if (khs.VirtualKeyCode == 100) // num4
                {
                    Task.Run(() => ZeroAmmoToEnemy());
                    //GodMode();
                }
                if (khs.VirtualKeyCode == 20)
                {
                    Task.Run(async () => Aim());
                }
            }
            return CallNextHookEx(_mHHook, nCode, wParam, lParam);
        }

        #region Aim

        private static async Task Aim()
        {
            Vector3 target = GetFirstEnemyPosition();
            Vector3 mousePosition = CalculateAim(target, _swed.ReadVec(MyPlayer.Position));
            _swed.WriteVec(MyPlayer.FaceVision, mousePosition);
        }

        private static Vector3 CalculateAim(Vector3 target, Vector3 myPosition)
        {
            Vector3 result = new Vector3();
            result.Y = myPosition.Y - target.Y;
            result.X = myPosition.X - target.X;
            result.Z = myPosition.Z - target.Z;
            return CalculateCelsius(result);
        }

        private static Vector3 CalculateCelsius(Vector3 rectangle)
        {
            float X = (float)(Math.Atan2(rectangle.Y,rectangle.X)*180/Math.PI);
            double tempDistance = Math.Sqrt(Math.Pow(rectangle.X, 2) + Math.Pow(rectangle.Y, 2));
            float Z = (float)(Math.Atan2(rectangle.Z,tempDistance) * 180/Math.PI);
            return new Vector3(X, Z, 0);
        }

        private static Vector3 GetFirstEnemyPosition()
        {
            Vector3 result;
            float minDistance = int.MaxValue-10;
            int i = 0;
            int id = 0;
            foreach (var player in _Offsets.AllPlayers)
            {
               if(player.Distance < 1) continue;
               if (player.Distance < minDistance)
               {
                   minDistance = player.Distance;
                   id = i;
               }
               i++;
            }
            return _swed.ReadVec(_Offsets.AllPlayers[id].Position);
        }

        #endregion
        #region ZeroAmmo
        private static bool IsZeroAmmo { get; set; } = false;

        private static async Task ZeroAmmoToEnemy()
        {
            IsZeroAmmo = !IsZeroAmmo;
            while (IsZeroAmmo)
            {
                foreach (var player in _Offsets.AllPlayers)
                {
                    if (player.NickName == MyPlayer.NickName) { continue; }
                    _swed.WriteInt(player.AmmoInFirstWeapon, 0);
                }
                await Task.Delay(50);
            }
        }
        #endregion
        #region GodMode
        private static bool IsGodMode { get; set; } = false;
        private static async Task GodMode()
        {
            IsGodMode = !IsGodMode;
            while (IsGodMode)
            {
                _swed.WriteInt(MyPlayer.Health, 101);
                await Task.Delay(50);
            }
        }
        #endregion
        #region Magnet
        private static bool IsMagnet { get; set; } = false;
        private static async Task MagnetPlayers()
        {
            IsMagnet = !IsMagnet;
            while (IsMagnet)
            {
                Vector3 playerVector3 = _swed.ReadVec(MyPlayer.Position);
                Vector3 playerVisioVector3 = _swed.ReadVec(MyPlayer.FaceVision);
                if (playerVisioVector3.Y >= 0 && playerVisioVector3.Y <= 90)
                {
                    playerVector3.X = playerVector3.X + 100 + 100 * ((float)Math.Cos((double)playerVisioVector3.Y / 25 ));
                    playerVector3.Y = playerVector3.Y + 100 + 100 * ((float)Math.Sin((double)playerVisioVector3.Y / 25 ));
                }
                else if (playerVisioVector3.Y <= -90 && playerVisioVector3.Y >= -180)
                {
                    playerVector3.X = playerVector3.X - 100 - 100 * ((float)Math.Cos((double)playerVisioVector3.Y /25 ));
                    playerVector3.Y = playerVector3.Y - 100 - 100 * ((float)Math.Sin((double)playerVisioVector3.Y / 25 ));
                }
                else if (playerVisioVector3.Y >= 90 && playerVisioVector3.Y <= 180)
                {
                    playerVector3.X = playerVector3.X - 100 - 100 * ((float)Math.Cos((double)playerVisioVector3.Y / 25 ));
                    playerVector3.Y = playerVector3.Y + 100 + 100 * ((float)Math.Sin((double)playerVisioVector3.Y / 25 ));
                }
                else if (playerVisioVector3.Y <= 0 && playerVisioVector3.Y >= -90)
                {
                    playerVector3.X = playerVector3.X + 100 + 100 * ((float)Math.Cos((double)playerVisioVector3.Y / 25 ));
                    playerVector3.Y = playerVector3.Y - 100 - 100 * ((float)Math.Sin((double)playerVisioVector3.Y / 25 ));
                }
                foreach (var player in _Offsets.AllPlayers)
                {
                    if (player.NickName == MyPlayer.NickName) { continue; }
                    _swed.WriteVec(player.Position, playerVector3);
                }
            }
        }
        #endregion
        #region Freeze 
        private static bool IsFreeze { get; set; } = false;
        private static bool FreezeStart { get; set; } = false;

        private static void UnFreezePlayers()
        {
            if (IsFreeze)
            {
                Task.Run(() => Freezing(false));
            }
        }
        private static void FreezePlayers()
        {
            if (!IsFreeze)
            {
                Task.Run(() => Freezing(true));
            }
        }
        private static async Task Freezing(bool enable)
        {
            FreezeStart = true;
            IsFreeze = enable;
            if (enable)
            {
                Dictionary<Player, Vector3> FreezePositions = new Dictionary<Player, Vector3>();
                foreach (Player player in _Offsets.AllPlayers)
                {
                    if (player.NickName == MyPlayer.NickName)
                    {
                        continue;
                    }
                    Vector3 freezePosition;
                    freezePosition = _swed.ReadVec(player.Position);
                    FreezePositions.Add(player, freezePosition);
                }
                while (IsFreeze)
                {
                    foreach (var freezePosition in FreezePositions)
                    {
                        _swed.WriteVec(freezePosition.Key.Position, freezePosition.Value);
                        _swed.WriteVec(freezePosition.Key.FaceVision, FreezeFacePoin);
                    }
                }
            }
        }
        #endregion
        #endregion
        #region SetHook
        public static nint SetHook()
        {
            try
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, _mCallback, GetModuleHandle(IntPtr.Zero), 0);
                }
            }
            catch
            {
                return nint.Zero;
            }
        }
        #endregion
        #region KeyboardHookStruct
        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardHookStruct
        {
            public readonly int VirtualKeyCode;
            public readonly int ScanCode;
            public readonly int Flags;
            public readonly int Time;
            public readonly IntPtr ExtraInfo;
        }
        #endregion
        #region Params
        private static bool IsStart { get; set; } = false;
        private static IntPtr Descriptor { get; set; } = 0;
        private static Point MousePosition { get; set; }
        private static Point MouseLastIterationPosition { get; set; }
        private static bool MainProcessIsActive { get; set; } = false;
        private static Task MainProcess
        {
            get
            {
                lock (StartProcess())
                    return Task.FromResult(Process.GetCurrentProcess());
            }
        }
        #endregion
        #region MainLogic
        private static async Task StartProcess()
        {
            if (MainProcessIsActive) { return; }
            MainProcessIsActive = true;
            //while (true)
            //{
            //    await Task.Delay(10);
            //}
        }
        #endregion
    }
}
