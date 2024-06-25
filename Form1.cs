using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Text.Json;
using System.Media;
using System.Configuration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Foxhole_Queue_Monitor
{
    public partial class Form1 : Form
    {
        ShardStatus shardStatus = new ShardStatus();

        Timer refreshTimer = new Timer();
        

        public Form1()
        {
            InitializeComponent();
            refreshTimer.Tick += new EventHandler(refreshBtn_Click);
            refreshTimer.Interval = Properties.Settings.Default.RefreshTimer;
            refreshTimer.Start();
            autoRefreshCB.Checked = true;

            //Load API Shard calls
            foreach (SettingsProperty currentProperty in Properties.Shards.Default.Properties)
            {
                shardCB.Items.Add(currentProperty.Name);
            }

            //Select default shard
            int index = shardCB.Items.IndexOf(Properties.Settings.Default.DefaultShard);
            shardCB.SelectedIndex = index;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            if(sender is Timer)
            {
                nextRefreshTxtB.Text = DateTime.Now.AddMilliseconds(Properties.Settings.Default.RefreshTimer).ToString();
            }

            //Clear Previous Data
            colonialQueueUpdateTxtB.Clear();
            colonialQueueWarningTxtB.Clear();
            colonialsInQueueTxtB.Clear();
            colonialQueueDropDetTxtb.Clear();
            colonialQueueDropDetTxtb.BackColor = Color.Empty;

            wardenQueueUpdateTxtB.Clear();
            wardenQueueWarningTxtB.Clear();
            wardensInQueueTxtB.Clear();
            wardenQueueDropDetTxtb.Clear();
            wardenQueueDropDetTxtb.BackColor = Color.Empty;

            colonialQueueUpdateTxtB.Text += "Queues: ";
            wardenQueueUpdateTxtB.Text += "Queues: ";

            try
            {
                using (var client = new HttpClient())
                {
                    int wardensInQueue = 0;
                    int colonialsInQueue = 0;

                    var endpoint = new Uri(apiUrlTxtB.Text);
                    var result = client.GetAsync(endpoint).Result;
                    var json = result.Content.ReadAsStringAsync().Result;

                    shardStatus = JsonSerializer.Deserialize<ShardStatus>(json);

                    //Set Queue Warnings
                    wardenQueueWarningTxtB.Text = shardStatus.bShowWardenQueueWarning.ToString();
                    colonialQueueWarningTxtB.Text = shardStatus.bShowColonialQueueWarning.ToString();

                    if (wardenQueueWarningTxtB.Text == "True")
                    {
                        wardenQueueWarningTxtB.BackColor = Color.OrangeRed;
                    }
                    else
                    {
                        wardenQueueWarningTxtB.BackColor = Color.LightGreen;
                    }
                    if (colonialQueueWarningTxtB.Text == "True")
                    {
                        colonialQueueWarningTxtB.BackColor = Color.OrangeRed;
                    }
                    else
                    {
                        colonialQueueWarningTxtB.BackColor = Color.LightGreen;
                    }

                    //Take snapshot of data before clearing.
                    List<ShardStatusSnapShot> shardStatusSnapShot = new List<ShardStatusSnapShot>();

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        shardStatusSnapShot.Add(new ShardStatusSnapShot(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString(), row.Cells[2].Value.ToString()));
                    }

                    dataGridView1.Rows.Clear();
                    dataGridView2.Rows.Clear();

                    foreach (ServerConnectionInfoList serverConnectionInfoList in shardStatus.serverConnectionInfoList)
                    {
                        if (!serverConnectionInfoList.currentMap.Contains("HomeRegion"))
                        {
                            colonialsInQueue += Convert.ToInt32(serverConnectionInfoList.colonialQueueSize.ToString());
                            wardensInQueue += Convert.ToInt32(serverConnectionInfoList.wardenQueueSize.ToString());

                            var snapShot = shardStatusSnapShot.Find(i => i.regionName == serverConnectionInfoList.currentMap.Replace("Hex", ""));

                            if (snapShot == null)
                            {
                                string[] row1 = new string[] { serverConnectionInfoList.currentMap.Replace("Hex", ""), 
                                    serverConnectionInfoList.colonialQueueSize.ToString(), 
                                    serverConnectionInfoList.wardenQueueSize.ToString(), "", "" };
                                dataGridView1.Rows.Add(row1);
                            }
                            else
                            {
                                string[] row1 = new string[] { serverConnectionInfoList.currentMap.Replace("Hex", ""), 
                                    serverConnectionInfoList.colonialQueueSize.ToString(), 
                                    serverConnectionInfoList.wardenQueueSize.ToString(),
                                    Convert.ToInt32(snapShot.previousColonialQueueSize).ToString(),
                                    Convert.ToInt32(snapShot.previousWardenQueueSize).ToString() };
                                dataGridView1.Rows.Add(row1);
                            }

                            if(Convert.ToInt32(serverConnectionInfoList.colonialQueueSize.ToString()) > 0)
                            {
                                colonialQueueUpdateTxtB.Text += "(" + serverConnectionInfoList.currentMap.Replace("Hex", "") + " - " + serverConnectionInfoList.colonialQueueSize.ToString() + ")" + " ";
                            }
                            if (Convert.ToInt32(serverConnectionInfoList.wardenQueueSize.ToString()) > 0)
                            {
                                wardenQueueUpdateTxtB.Text += "(" + serverConnectionInfoList.currentMap.Replace("Hex", "") + " - " + serverConnectionInfoList.wardenQueueSize.ToString() + ")" + " ";
                            }
                        }
                        if (serverConnectionInfoList.currentMap.Contains("HomeRegion"))
                        {
                            string[] row1 = new string[] { serverConnectionInfoList.currentMap, serverConnectionInfoList.colonialQueueSize.ToString(), serverConnectionInfoList.wardenQueueSize.ToString() };
                            dataGridView2.Rows.Add(row1);
                        }
                    }

                    colonialsInQueueTxtB.Text = colonialsInQueue.ToString();
                    wardensInQueueTxtB.Text = wardensInQueue.ToString();

                    foreach (DataGridViewRow dataGridViewRow in dataGridView1.Rows)
                    {
                        //Colonial Queue Colors
                        if (Convert.ToInt32(dataGridViewRow.Cells[1].Value.ToString()) != 0)
                        {
                            if (Convert.ToInt32(dataGridViewRow.Cells[1].Value.ToString()) >= 1)
                            {
                                dataGridViewRow.Cells[1].Style.BackColor = Color.FromArgb(237, 126, 128);
                            }
                            if (Convert.ToInt32(dataGridViewRow.Cells[1].Value.ToString()) >= 10)
                            {
                                dataGridViewRow.Cells[1].Style.BackColor = Color.FromArgb(201, 107, 109);
                            }
                            if (Convert.ToInt32(dataGridViewRow.Cells[1].Value.ToString()) >= 15)
                            {
                                dataGridViewRow.Cells[1].Style.BackColor = Color.FromArgb(168, 90, 91);
                            }
                        }

                        //Warden Queue Colors
                        if (Convert.ToInt32(dataGridViewRow.Cells[2].Value.ToString()) != 0)
                        {
                            if (Convert.ToInt32(dataGridViewRow.Cells[2].Value.ToString()) >= 1)
                            {
                                dataGridViewRow.Cells[2].Style.BackColor = Color.FromArgb(237, 126, 128);
                            }
                            if (Convert.ToInt32(dataGridViewRow.Cells[2].Value.ToString()) >= 10)
                            {
                                dataGridViewRow.Cells[2].Style.BackColor = Color.FromArgb(201, 107, 109);
                            }
                            if (Convert.ToInt32(dataGridViewRow.Cells[2].Value.ToString()) >= 15)
                            {
                                dataGridViewRow.Cells[2].Style.BackColor = Color.FromArgb(168, 90, 91);
                            }
                        }

                        //Colonial Queue Drop Detection
                        //Check if current queue size is less than old queue size
                        if(dataGridViewRow.Cells[3].Value.ToString() != "")
                        {
                            if (Convert.ToInt32(dataGridViewRow.Cells[1].Value.ToString()) < Convert.ToInt32(dataGridViewRow.Cells[3].Value.ToString()))
                            {
                                int queueDifference = Math.Abs(Convert.ToInt32(dataGridViewRow.Cells[1].Value.ToString()) - Convert.ToInt32(dataGridViewRow.Cells[3].Value.ToString()));

                                if (queueDifference >= Properties.Settings.Default.QueueDropThreshold)
                                {
                                    dataGridViewRow.Cells[3].Style.BackColor = Color.Yellow;
                                    colonialQueueDropDetTxtb.Text = "Colonial Queue Drop Detected";
                                    colonialQueueDropDetTxtb.BackColor = Color.OrangeRed;

                                    logTxtB.Text += DateTime.Now + " : " + "Colonial Queue Drop Detected: " + dataGridViewRow.Cells[0].Value.ToString() + " - Dropped by: " + queueDifference + Environment.NewLine;
                                }
                            }
                        }
                        //Warden Queue Drop Detection
                        if (dataGridViewRow.Cells[4].Value.ToString() != "")
                        {
                            if (Convert.ToInt32(dataGridViewRow.Cells[2].Value.ToString()) < Convert.ToInt32(dataGridViewRow.Cells[4].Value.ToString()))
                            {
                                int queueDifference = Math.Abs(Convert.ToInt32(dataGridViewRow.Cells[2].Value.ToString()) - Convert.ToInt32(dataGridViewRow.Cells[4].Value.ToString()));

                                if (queueDifference >= Properties.Settings.Default.QueueDropThreshold)
                                {
                                    dataGridViewRow.Cells[4].Style.BackColor = Color.Yellow;
                                    wardenQueueDropDetTxtb.Text = "Warden Queue Drop Detected";
                                    wardenQueueDropDetTxtb.BackColor = Color.OrangeRed;

                                    logTxtB.Text += DateTime.Now + " : " + "Warden Queue Drop Detected: " + dataGridViewRow.Cells[0].Value.ToString() + " - Dropped by: " + queueDifference + Environment.NewLine;
                                }
                            }
                        }
                    }

                    if (wardenQueueDropDetTxtb.Text != "" || colonialQueueDropDetTxtb.Text != "")
                    {
                        SoundPlayer sp = new SoundPlayer();
                        sp.SoundLocation = "queuedropalert.wav";
                        sp.Play();
                    }

                    dataGridView1.Sort(this.dataGridView1.Columns["Region"], ListSortDirection.Ascending);
                    dataGridView1.AutoResizeColumns();
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                    dataGridView2.Sort(this.dataGridView2.Columns["RegionHome"], ListSortDirection.Ascending);
                    dataGridView2.AutoResizeColumns();
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                    dataGridView1.Rows[0].Selected = true;
                    dataGridView2.Rows[0].Selected = true;

                    dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                    dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];

                    logTxtB.SelectionStart = logTxtB.Text.Length;
                    logTxtB.ScrollToCaret();

                    lastRefreshTxtB.Text = DateTime.Now.ToString();
                }
            }catch (Exception ex) 
            { 
                logTxtB.Text += ex.Source + Environment.NewLine;
                logTxtB.Text += ex.StackTrace + Environment.NewLine;
                logTxtB.Text += ex.Message + Environment.NewLine;
                logTxtB.SelectionStart = logTxtB.Text.Length;
                logTxtB.ScrollToCaret();
            }
        }

        private void selectedIndexChange(object sender, EventArgs e)
        {

            //Load API Shard calls
            foreach (SettingsProperty currentProperty in Properties.Shards.Default.Properties)
            {
                if(shardCB.Text == currentProperty.Name)
                {
                    apiUrlTxtB.Text = currentProperty.DefaultValue.ToString();
                }
            }
        }

        private void colonialQueueUpdateCopyBtn_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(colonialQueueUpdateTxtB.Text);
        }

        private void wardenQueueUpdateCopyBtn_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(wardenQueueUpdateTxtB.Text);
        }

        private void autoRefreshCB_CheckedChanged(object sender, EventArgs e)
        {
            if(autoRefreshCB.Checked)
            {
                refreshTimer.Start();
                nextRefreshTxtB.Text += DateTime.Now.AddMilliseconds(Properties.Settings.Default.RefreshTimer);
            }
            else
            {
                refreshTimer.Stop();
                nextRefreshTxtB.Clear();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void clearLogBtn_Click(object sender, EventArgs e)
        {
            logTxtB.Clear();
        }

        private void openQueuesBtn_Click(object sender, EventArgs e)
        {
            ExecuteProgramGrid executeProgramGrid = new ExecuteProgramGrid();
            executeProgramGrid.StartPosition = FormStartPosition.CenterParent;
            executeProgramGrid.ShowDialog();
        }
    }

    public class ShardStatus
    {
        public bool bShowColonialQueueWarning { get; set; }
        public bool bShowWardenQueueWarning { get; set; }
        public double normalizedGlobalPopulation { get; set; }
        public List<ServerConnectionInfoList> serverConnectionInfoList { get; set; }
        public string warId { get; set; }
        public int squadMaxSize { get; set; }
        public int secondsToPreConquest { get; set; }
        public bool bIsPreConquest { get; set; }
        public bool bIsVIPMode { get; set; }
    }

    public class ServerConnectionInfoList
    {
        public string currentMap { get; set; }
        public string steamId { get; set; }
        public string ipAddress { get; set; }
        public int port { get; set; }
        public int beaconPort { get; set; }
        public object packedWarStatus { get; set; }
        public int packedServerState { get; set; }
        public int colonialQueueSize { get; set; }
        public int wardenQueueSize { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public int serverType { get; set; }
        public int mapId { get; set; }
        public int openColonialSlots { get; set; }
        public int openWardenSlots { get; set; }
        public int freeDiskSpaceInMb { get; set; }
        public int totalDiskSpaceInMb { get; set; }
    }

    public class ShardStatusSnapShot
    {
        public string regionName { get; set; }
        public string previousColonialQueueSize { get; set; }
        public string previousWardenQueueSize { get; set; }
        public ShardStatusSnapShot(string regionName, string previousColonialQueueSize, string previousWardenQueueSize)
        {
            this.regionName = regionName;
            this.previousColonialQueueSize = previousColonialQueueSize;
            this.previousWardenQueueSize = previousWardenQueueSize;
        }
    }
}
