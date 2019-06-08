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

        private static List<Keys> CantPressKeyList = new List<Keys>();
        private static List<Keys> CanPressKeyList = new List<Keys>();

        #endregion

        #region EventHandleField

        public delegate void KeybdHookDelegate(Keys keys);
        /// <summary>
        /// 키보드가 눌렸을떄 눌린 키보드의 키값을 받습니다
        /// </summary>
        public static event KeybdHookDelegate OnHookCallback;

        #endregion

        #region LowlevelField

        private static int WH_KEYBOARD_LL = 13;
        private static int WM_KEYDOWN = 0x0100;
        private static IntPtr hook = IntPtr.Zero;
        private static LowLevelKeyboardProc llkProcedure = HookCallback;

        #endregion

        #region SettingMethods

        /// <summary>
        /// 키보드 후킹을 시작하며 이벤트 핸들을 합니다
        /// </summary>
        public static void Keybd_InitHook()
        {
            hook = SetHook(llkProcedure);
        }

        /// <summary>
        /// 키보드 후킹을 종료합니다 이 메소드는 프로세스가 끝나기 전에 꼭 한번은 나와야 합니다
        /// </summary>
        public static void Keybd_UnHook()
        {
            UnhookWindowsHookEx(hook);
        }

        #endregion

        #endregion

        #region ListManage

        /// <summary>
        /// 누를수 있는 키 리스트에 아이템을 추가합니다
        /// </summary>
        /// <param name="keys"></param>
        public static void CanPressKey_add(Keys keys)
        {
            if(CanPressKeyList.Contains(keys) == false)
                CanPressKeyList.Add(keys);
        }

        /// <summary>
        /// 누를수 없는 키 리스트에 아이템을 추가합니다
        /// </summary>
        /// <param name="keys"></param>
        public static void CantPressKey_add(Keys keys)
        {
            if (CantPressKeyList.Contains(keys) == false)
                CantPressKeyList.Add(keys);
        }

        /// <summary>
        /// 누를수 있는 키 리스트에서 특정 아이템을 찾아 제거합니다
        /// </summary>
        /// <param name="keys"></param>
        public static void CanPressKey_remove(Keys keys)
        {
            if (CanPressKeyList.Contains(keys) == true)
                CanPressKeyList.Remove(keys);
        }

        /// <summary>
        /// 누를수 없는 키 리스트에서 특정 아이템을 찾아 제거합니다
        /// </summary>
        /// <param name="keys"></param>
        public static void CantPressKey_remove(Keys keys)
        {
            if (CantPressKeyList.Contains(keys) == true)
                CantPressKeyList.Remove(keys);
        }

        /// <summary>
        /// 누를수 있는 키 리스트에 특정 아이템이 있는지 확인합니다
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool CanPressKey_Contains(Keys keys)
        {
            if (CanPressKeyList.Contains(keys))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 누를수 없는 키 리스트에 특정 아이템이 있는지 확인합니다
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool CantPressKey_Contains(Keys keys)
        {
            if (CantPressKeyList.Contains(keys))
                return true;
            else
                return false;
        }

        #endregion

        #region LowlevelMethods

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                OnHookCallback?.Invoke((Keys)vkCode);

                if (CanPressKeyList.Count > 0)//화이트 리스트가 비어있지 않다면
                {
                    if (CanPressKeyList.Contains((Keys)vkCode) == false)//화이트 리스트에 존재하지 않는다면
                    {
                        return (IntPtr)1;//키 후킹 탈취
                    }
                }
                else//화이트 리스트가 비어있다면
                {
                    if (CantPressKeyList.Contains((Keys)vkCode) == true)//블랙리스트에 항목이 존재한다면
                    {
                        return (IntPtr)1;//키 후킹 탈취
                    }
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule currentModule = currentProcess.MainModule;
            String moduleName = currentModule.ModuleName;
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            return SetWindowsHookEx(WH_KEYBOARD_LL, llkProcedure, moduleHandle, 0);
        }

        #endregion

        #region DllImports

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(String lpModuleName);

        #endregion
    }
}
