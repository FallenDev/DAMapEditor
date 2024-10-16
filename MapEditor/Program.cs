using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace MapEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string mapName = null;
            if (args.Length >= 1)
            {
                mapName = args[0];
            }

            Application.Run(new Form1(mapName));
        }
    }
}