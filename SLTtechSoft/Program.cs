using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApp1.Login;

namespace SLTtechSoft
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public static User CurrentUser { get; set; }
        public static bool IsLoggedIn { get; set; }
        [STAThread]
       
        private static void Main()
        {
            CultureInfo english = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = english;
            Thread.CurrentThread.CurrentUICulture = english;

            CultureInfo.DefaultThreadCurrentCulture = english;
            CultureInfo.DefaultThreadCurrentUICulture = english;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
        }
    }
}
