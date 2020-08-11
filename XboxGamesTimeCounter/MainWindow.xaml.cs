using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace XboxGamesTimeCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DateTime started;

        bool showingUserOnly = true;

        static MainWindow mw;

        Thread findNewOpenProgramsThread;

        public MainWindow()
        {
            InitializeComponent();
            //StartCode();
            mw = this;
            ListPrograms();
            //findNewOpenProgramsThread = new Thread(new ThreadStart(WaitForProcess));
            //findNewOpenProgramsThread.Start();
        }

        private void ListPrograms()
        {           

            Process[] processes;
            if (showingUserOnly)
            {
                processes = Process.GetProcesses().Where(p => p.MainWindowHandle != (IntPtr)0).ToArray();
            }
            else
            {
                processes = Process.GetProcesses();
            }

            

            foreach (Process p in processes)
            {
                //Console.WriteLine(GetProcessUser(p) + " " + p.ProcessName);
                //if (showingUserOnly && GetProcessUser(p) != Environment.UserName)
                //{
                //    //Don't do anything with it
                //}
                //else
                //{  
                if (!listbox1.Items.Contains(p.ProcessName))
                {
                    int i = listbox1.Items.Add(p.ProcessName);                    
                }
                // }


            }


        }

        void WaitForProcess()
        {
            ManagementEventWatcher startWatch = new ManagementEventWatcher(
              new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            startWatch.Start();
        }

        void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("Process started: {0}", e.NewEvent.Properties["ProcessName"].Value);

            
            
            if (listbox1.Items.Contains(e.NewEvent.Properties["ProcessName"].Value.ToString()))
            {
                //Do nothing
                Console.WriteLine("Not updating list with this one");
            }           
            else
            {
                Process[] processes = Process.GetProcesses().Where(p => p.MainWindowHandle != (IntPtr)0).ToArray();
                if(processes.Contains(processes.Where(p => p.ProcessName == e.NewEvent.Properties["ProcessName"].Value.ToString()).FirstOrDefault()))
                {
                    //Also do nothing
                }
                else {
                    this.Dispatcher.Invoke(() =>
                    {
                        ListPrograms();
                    });
                }
            }
            
            
        }

        private void Listbox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Process selection = Process.GetProcessesByName(listbox1.SelectedItem.ToString()).FirstOrDefault();

            ProcessNameLabel.Content = selection.ProcessName;

            started = selection.StartTime;

            selection.Exited += Selection_Exited;

            DisplayTimeRunning();
        }

        private void DisplayTimeRunning()
        {
            var time = DateTime.Now - started;
            TimeRunning.Content = "Open for \n" + time.Hours.ToString() + " hours, " + time.Minutes.ToString() + " minutes";
        }

        private void Selection_Exited(object sender, EventArgs e)
        {
            RecordTime();
        }

        public string GetProcessOwner(string processName)
        {
            string query = "Select * from Win32_Process Where Name = \"" + processName + "\"";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    // return DOMAIN\user
                    string owner = argList[1] + "\\" + argList[0];
                    return owner;
                }
            }

            return "NO OWNER";
        }

        private static string GetProcessUser(Process process)
        {
            IntPtr processHandle = IntPtr.Zero;
            try
            {
                OpenProcessToken(process.Handle, 8, out processHandle);
                WindowsIdentity wi = new WindowsIdentity(processHandle);
                string user = wi.Name;
                return user.Contains(@"\") ? user.Substring(user.IndexOf(@"\") + 1) : user;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                }
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private void RecordTime()
        {
            throw new NotImplementedException(); 
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ListPrograms();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //public void StartCode()
        //{
        //    //WMIEvent we = new WMIEvent();
        //    ManagementEventWatcher w = null;
        //    WqlEventQuery q;
        //    try
        //    {                
        //        q = new WqlEventQuery();
        //        q.EventClassName = "Win32_ProcessStartTrace";
        //        w = new ManagementEventWatcher(q);
        //        w.EventArrived += new EventArrivedEventHandler(ProcessStartEventArrived);
        //        w.Start();
        //        //Console.ReadLine(); // block main thread for test purposes
        //    }
        //    finally
        //    {
        //        w.Stop();
        //    }
        //}

        //public void ProcessStartEventArrived(object sender, EventArrivedEventArgs e)
        //{
        //    foreach (PropertyData pd in e.NewEvent.Properties)
        //    {
        //        Console.WriteLine("\n============================= =========");
        //        Console.WriteLine("{0},{1},{2}", pd.Name, pd.Type, pd.Value);
        //    }
        //}
    }
}
