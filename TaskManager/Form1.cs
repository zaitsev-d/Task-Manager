using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;

namespace TaskManager
{
    public partial class Form1 : Form
    {
        private List<Process> processes = null;
        private ListViewItemComparer comparer = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void GetProcesses()
        {
            processes.Clear();
            processes = Process.GetProcesses().ToList<Process>();
        }

        private void RefreshProcessesList()
        {
            listView1.Items.Clear();
            double memorySize = default;

            foreach (Process p in processes)
            {
                memorySize = default;

                PerformanceCounter performanceCounter = new PerformanceCounter();
                performanceCounter.CategoryName = "Process";
                performanceCounter.CounterName = "Working Set - Private";
                performanceCounter.InstanceName = p.ProcessName;

                memorySize = (double)performanceCounter.NextValue() / (1000 * 1000);

                string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memorySize, 1).ToString() };
                listView1.Items.Add(new ListViewItem(row));

                performanceCounter.Close();
                performanceCounter.Dispose();
            }

            Text = "Running processes: " + processes.Count.ToString();
        }

        private void RefreshProcessesList(List<Process> processes, string keyword)
        {
            try
            {
                listView1.Items.Clear();
                double memorySize = default;

                foreach (Process p in processes)
                {
                    if (p != null)
                    {
                        memorySize = default;

                        PerformanceCounter performanceCounter = new PerformanceCounter();
                        performanceCounter.CategoryName = "Process";
                        performanceCounter.CounterName = "Working Set - Private";
                        performanceCounter.InstanceName = p.ProcessName;

                        memorySize = (double)performanceCounter.NextValue() / (1000 * 1000);

                        string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memorySize, 1).ToString() };
                        listView1.Items.Add(new ListViewItem(row));

                        performanceCounter.Close();
                        performanceCounter.Dispose();
                    }
                }

                Text = $"Running processes '{keyword}': " + processes.Count.ToString();
            }
            catch(Exception) { }
        }

        private void KillProcess(Process process)
        {
            process.Kill();
            process.WaitForExit();
        }

        private void KillProcessAndChildren(int processID)
        {
            if(processID == 0)
            {
                return;
            }

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + processID);
            ManagementObjectCollection objectCollection = searcher.Get();

            foreach(ManagementObject obj in objectCollection)
            {
                KillProcessAndChildren(Convert.ToInt32(obj["ProcessID"]));
            }

            try
            {
                Process p = Process.GetProcessById(processID);
                p.Kill();
                p.WaitForExit();
            }
            catch(ArgumentException) { }
        }

        private int GetParentProcessID(Process process)
        {
            int parentID = default;

            try
            {
                ManagementObject managementObject = new ManagementObject("win32_process.handle='" + process.Id + "'");
                managementObject.Get();

                parentID = Convert.ToInt32(managementObject["ParentProcessId"]);
            }
            catch(Exception) { }

            return parentID;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            processes = new List<Process>();
            comparer = new ListViewItemComparer();
            comparer.ColumnIndex = 0;

            GetProcesses();
            RefreshProcessesList();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GetProcesses();
            RefreshProcessesList();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if(listView1.SelectedItems[0] != null)
                {
                    Process processToKill =  processes.Where((x) => x.ProcessName == 
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcess(processToKill);
                    GetProcesses();
                    RefreshProcessesList();
                }
            } 
            catch(Exception) { }
        }

        private void endTaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                   listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcess(processToKill);
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                   listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessID(processToKill));
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void endTheProcessesTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                   listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessID(processToKill));
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void runNewTaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Interaction.InputBox("Enter program name", "Run new task");

            try
            {
                Process.Start(path);
            }
            catch(Exception) { }
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            GetProcesses();

            List<Process> filteredProcesses = processes.Where((x) => 
            x.ProcessName.ToLower().Contains(toolStripTextBox1.Text.ToLower())).ToList<Process>();

            RefreshProcessesList(filteredProcesses, toolStripTextBox1.Text);
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            comparer.ColumnIndex = e.Column;
            comparer.SortDirection = comparer.SortDirection == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;

            listView1.ListViewItemSorter = comparer;
            listView1.Sort();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
