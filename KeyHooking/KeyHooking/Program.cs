using System;
using System.Windows.Forms;

namespace KeyHooking
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            KeyHooker.KeyboardInitHook();//필요한 준비를 합니다
            KeyHooker.OnHookCallback += KeyHooker_OnHookCallback;

            KeyHooker.CantPressKeyAdd(Keys.A);//A키를 블랙리스트에 추가하였으므로 A를 못누르게 됩니다

            Application.Run();
        }

        private static void KeyHooker_OnHookCallback(Keys keys)//눌린 키들을 이벤트 헨들로 가져옵니다
        {
            Console.Write(keys);
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            KeyHooker.KeyboardUnHook();//꺼질떄 후킹을 빼주지 않으면 원도우에 문제가 생길수 있습니다
        }
    }
}
