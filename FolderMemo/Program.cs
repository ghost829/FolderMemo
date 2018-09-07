using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace FolderMemo
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new SystemTray());

            bool bNew;
            Mutex mutex = new Mutex(true, "FolderMemoMutex", out bNew);
            if (bNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new SystemTray());
                mutex.ReleaseMutex();
            }
            else
            {
                //소유권 없음
                Application.Exit();
            }
        }
    }
}
