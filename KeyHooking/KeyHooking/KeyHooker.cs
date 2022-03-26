using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace KeyHooking
{
    public static class KeyHooker
    {
        #region Field

        #region White/Black_ListKeyField

        private static readonly List<Keys> CantPressKeyList = new();
        private static readonly List<Keys> CanPressKeyList = new();

        #endregion

        #region EventHandleField
        
        /// <summary>
        /// 키보드가 눌렸을떄 눌린 키보드의 키값을 받습니다
        /// </summary>
        public static event Action<Keys> OnHookCallback;

        #endregion

        #region LowlevelField

        private const int WhKeyboardLl = 13;
        private const int WmKeydown = 0x0100;
        private static IntPtr _hook = IntPtr.Zero;

        #endregion

        #region SettingMethods

        /// <summary>
        /// 키보드 후킹을 시작하며 이벤트 핸들을 합니다
        /// </summary>
        public static void KeyboardInitHook() => _hook = SetHook();

        /// <summary>
        /// 키보드 후킹을 종료합니다 이 메소드는 프로세스가 끝나기 전에 꼭 한번은 나와야 합니다
        /// </summary>
        public static void KeyboardUnHook() => UnhookWindowsHookEx(_hook);

        #endregion

        #endregion

        #region ListManage

        /// <summary>
        /// 누를수 있는 키 리스트에 아이템을 추가합니다
        /// </summary>
        /// <param name="keys"></param>
        public static void CanPressKeyAdd(Keys keys)
        {
            if(!CanPressKeyList.Contains(keys)) CanPressKeyList.Add(keys);
        }

        /// <summary>
        /// 누를수 없는 키 리스트에 아이템을 추가합니다
        /// </summary>
        /// <param name="keys"></param>
        public static void CantPressKeyAdd(Keys keys)
        {
            if (!CantPressKeyList.Contains(keys)) CantPressKeyList.Add(keys);
        }

        /// <summary>
        /// 누를수 있는 키 리스트에서 특정 아이템을 찾아 제거합니다
        /// </summary>
        /// <param name="keys"></param>
        public static void CanPressKeyRemove(Keys keys)
        {
            if (CanPressKeyList.Contains(keys)) CanPressKeyList.Remove(keys);
        }

        /// <summary>
        /// 누를수 없는 키 리스트에서 특정 아이템을 찾아 제거합니다
        /// </summary>
        /// <param name="keys"></param>
        public static void CantPressKeyRemove(Keys keys)
        {
            if (CantPressKeyList.Contains(keys)) CantPressKeyList.Remove(keys);
        }

        /// <summary>
        /// 누를수 있는 키 리스트에 특정 아이템이 있는지 확인합니다
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool CanPressKeyContains(Keys keys)
        {
            return CanPressKeyList.Contains(keys);
        }

        /// <summary>
        /// 누를수 없는 키 리스트에 특정 아이템이 있는지 확인합니다
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool CantPressKeyContains(Keys keys)
        {
            return CantPressKeyList.Contains(keys);
        }

        #endregion

        #region LowlevelMethods

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WmKeydown)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                OnHookCallback?.Invoke((Keys)vkCode);

                lock (CanPressKeyList)
                {
                    if (0 < CanPressKeyList.Count)
                    {
                        if (!CanPressKeyList.Contains((Keys) vkCode)) //화이트 리스트에 존재하지 않는다면
                        {
                            return (IntPtr) 1; //키 후킹 탈취
                        }
                    }
                    else if (0 < CantPressKeyList.Count)
                    {
                        if (CantPressKeyList.Contains((Keys) vkCode)) //블랙리스트에 항목이 존재한다면
                        {
                            return (IntPtr) 1; //키 후킹 탈취
                        }
                    }
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static IntPtr SetHook()
        {
            var currentProcess = Process.GetCurrentProcess();
            var currentModule = currentProcess.MainModule ?? throw new NullReferenceException("currentProcess.MainModule is null");
            var moduleName = currentModule.ModuleName;
            var moduleHandle = GetModuleHandle(moduleName);
            return SetWindowsHookEx(WhKeyboardLl, HookCallback, moduleHandle, 0);
        }

        #endregion

        #region DllImports

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lowLevelKeyboardProc, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }
}
