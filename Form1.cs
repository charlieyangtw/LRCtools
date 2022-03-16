using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LRCtools
{
    public partial class Form1 : Form
    {
        LRCdata ld;
            
        public Form1()
        {
            InitializeComponent();
            newObject();
        }

        private void 離開ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        private void 儲存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ld.lstLRC.Count > 0)
            {
                saveFileDialog1.FileName = ld.ar != null && ld.ti != null && ld.ar.Length > 0 && ld.ti.Length > 0 ?
                    string.Format("{0} - {1}.lrc", ld.ar, ld.ti) : "";
                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    System.IO.File.WriteAllText(
                        saveFileDialog1.FileName,
                        txtTarget.Text,
                        Encoding.Unicode
                        );
            }
        }

        private void 單字恢復長串ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (txtSource.Text.Trim().Length > 0)
                txtTarget.Text = funCharToLongString(txtSource.Text.Trim());
            tabControl1.SelectTab(tabControl1.TabCount - 1);
        }

        private void 關於ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox1().Show();
        }

        #region txtSource
        private void txtSource_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
                txtSource.Cursor = System.Windows.Forms.Cursors.Arrow;  //指定鼠標形狀（更好看）  
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void txtSource_DragDrop(object sender, DragEventArgs e)
        {
            string strFile = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            txtSource.Text = System.IO.File.ReadAllText(strFile, Encoding.Unicode);
            txtSource.Cursor = System.Windows.Forms.Cursors.IBeam; //還原鼠標形狀
            tabControl1.SelectTab(0);

            單字恢復長串ToolStripMenuItem_Click(sender, e);
            if (ld.bJone)
                儲存ToolStripMenuItem_Click(sender, e);
        }
        #endregion txtSource

        #region txtTarget
        private void txtTarget_TextChanged(object sender, EventArgs e)
        {
            txtTarget.SelectionLength = 0;
        }
        #endregion txtTarget

        #region 自定功能
        private void newObject()
        {
            ld = new LRCdata();
        }

        private string funCharToLongString(string strData)
        {
            newObject();
            foreach (string s in mySplit(strData))
                ld.Add(s);
            return ld.Output();
        }

        private string[] mySplit(string strData, string strSplit = "\n,\r\n")
        {
            return strData.Split(strSplit.Split(','), StringSplitOptions.RemoveEmptyEntries);
        }
        #endregion 自定功能

    }

    public class LRCdata
    {
        public string ar { get; set; }
        public string ti { get; set; }
        public string by { get; set; }

        public bool bJone { get; set; }

        public List<LineData> lstLRC = new List<LineData>();

        public void Add(string strData)
        {
            //[00:02.69]你[00:02.89]是[00:03.04]不[00:03.24]是[00:03.67]像[00:03.95]我[00:04.30]在[00:04.82]太[00:05.05]陽[00:05.41]下[00:05.73]低[00:06.16]頭[00:07.06]
            //[00:25.33][01:39.04][03:14.58]也許世界就這樣
            if (strData.Length > 0)
            {
                LineData ld = new LineData();
                string[] aryD = strData.Split('[');

                //尋找整串是不是都只有一個字, 數量超過3個就是可以儲存
                if (bJone == false)
                    bJone = aryD
                        .Where(x => x.IndexOf(']') != -1)
                        .Select(x => x.Split(']')[1])
                        .Where(x => x.Length == 1)
                        .Count() > 3;

                for (int i = 0; i < aryD.Length; i++)
                {
                    string d = aryD[i];
                    if (d.Length == 0)
                        continue;

                    if (d.StartsWith("ar:"))
                        ar = d.Replace("]", "").Substring(3);
                    else if (d.StartsWith("ti:"))
                        ti = d.Replace("]", "").Substring(3);
                    else if (d.StartsWith("by:"))
                        by = d.Replace("]", "").Substring(3);
                    else
                    {
                        if (ld.lstTime == null)
                            ld.lstTime = new List<string>();

                        string[] aryD1 = d.Split(']');
                        if (aryD1.Length > 1)
                        {
                            if (ld.lstTime.Count == 0 || (aryD1[1].Length == 0 && i != aryD.Length - 1))
                                ld.lstTime.Add(aryD1[0]);
                            else if (aryD1[1].Length > 0 && i == aryD.Length - 1)
                                ld.lstTime.Add(aryD1[0]);
                            ld.Content += aryD1[1];
                        }
                    }
                }
                if (ld.lstTime != null)
                    lstLRC.Add(ld);
            }
        }

        public string Output()
        {
            StringBuilder sb = new StringBuilder();
            if (lstLRC.Count > 0)
            {
                if (ar != null)
                    sb.AppendLine(string.Format("[ar:{0}]", ar));
                if (ti != null)
                    sb.AppendLine(string.Format("[ti:{0}]", ti));
                if (by != null)
                    sb.AppendLine(string.Format("[by:{0}]", by));

                foreach(LineData ld in lstLRC)
                    sb.AppendLine(string.Format("{0}{1}", string.Join("", ld.lstTime.Select(x => "[" + x + "]").ToArray()), ld.Content));
            }
            return sb.ToString();
        }
    }

    public class LineData
    {
        public List<string> lstTime { get; set; }
        public string Content { get; set; }
    }
}
