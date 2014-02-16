using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace LittleBin
{
    public partial class LittleBin : Form
    {
        public static LittleBin staticVar = null;
        
        // SHEmptyRecycleBin
        // http://msdn.microsoft.com/en-us/library/windows/desktop/bb762160(v=vs.85).aspx
        enum RecycleFlags : uint
        {
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        }
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
            static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);

        // SHObjectProperties
        public enum ShOP : uint
        {
            SHOP_PRINTERNAME = 0x00000001, // lpObject points to a printer friendly name
            SHOP_FILEPATH = 0x00000002, // lpObject points to a fully qualified path+file name
            SHOP_VOLUMEGUID = 0x00000004 // lpObject points to a Volume GUID
        };
        [DllImport("shell32.dll", EntryPoint = "SHObjectProperties", CharSet = CharSet.Unicode)]
        public static extern bool SHObjectProperties(IntPtr hwnd, ShOP dwType, string szObject, string szPage);

        // SHQueryRecycleBin
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct SHQUERYRBINFO
        {
            public Int32 cbSize;
            public UInt64 i64Size;
            public UInt64 i64NumItems;
        }
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);
        
        public LittleBin()
        {
            InitializeComponent();
            notifyIcon1.ContextMenu = contextMenu1;
        }

        private void OnOpen(object sender, EventArgs e)
        {
            Process.Start("shell:RecycleBinFolder");
        }

        private void OnEmpty(object sender, EventArgs e)
        {
            SHEmptyRecycleBin(IntPtr.Zero, null, 0);
        }

        private void OnProperties(object sender, EventArgs e)
        {
            SHObjectProperties(Handle, ShOP.SHOP_FILEPATH, "shell:RecycleBinFolder", null);
        }

        private void OnExit(object sender, EventArgs e) 
        { 
            Application.Exit(); 
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Process.Start("shell:RecycleBinFolder");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            SHQUERYRBINFO query = new SHQUERYRBINFO();
            query.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));

            try
            {
                if (SHQueryRecycleBin(null, ref query) == 0)
                {
                    int num_files = (int)query.i64NumItems;
                    string total_size = (Convert.ToInt64(query.i64Size)).ToFileSize();

                    if (num_files > 0)
                    {
                        menuItem2.Enabled = true;
                        notifyIcon1.Icon = Properties.Resources.FullIcon;
                        notifyIcon1.Text = "Recycle Bin\n" + total_size + " in " + num_files + (num_files > 1 ? " items" : " item");
                    }
                    else
                    {
                        menuItem2.Enabled = false;
                        notifyIcon1.Icon = Properties.Resources.EmptyIcon;
                        notifyIcon1.Text = "Recycle Bin";
                    }
                }
                else
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            catch (Exception ex)
            {
                notifyIcon1.Text = string.Format("Error accessing Recycle Bin: {0}", ex.Message);
            }

        }


    }
}
