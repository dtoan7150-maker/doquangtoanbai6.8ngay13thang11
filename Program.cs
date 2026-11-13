using System;
using System.Windows.Forms;
using BilliardsManagement.GUI;

namespace BilliardsManagement
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormMain());
        }
    }
}