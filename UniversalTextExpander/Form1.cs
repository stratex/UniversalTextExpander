using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.IO;

namespace UniversalTextExpander
{
    public partial class Form1 : Form
    {
        #region Global Variables
        private Form2 AddCodeForm;
        private KeyboardHook kbHook = new KeyboardHook();
        private ObservableCollection<KeyCodes> codes = new ObservableCollection<KeyCodes>();

        private string currentword = "";
        private bool macroRunning = false;
        private bool CTRLPressed = false;
        #endregion

        #region Initialization
        public Form1()
        {
            InitializeComponent();

            kbHook.KeyDown += KbHook_KeyDown;
            kbHook.KeyUp += KbHook_KeyUp;
            codes.CollectionChanged += Codes_CollectionChanged;

            kbHook.Install();

            AddCodeForm = new Form2();

            if (File.Exists("settings.txt"))
                ImportCodes();
        }
        #endregion

        #region Collection Events
        private void Codes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            listView1.Items.Clear();
            foreach (var item in codes)
                listView1.Items.Add(new ListViewItem(new string[] { item.Short, item.Long }));
        }
        #endregion

        #region Keyboard Events
        private void KbHook_KeyUp(KeyboardHook.VKeys key)
        {
            if (key == KeyboardHook.VKeys.CONTROL || key == KeyboardHook.VKeys.LCONTROL || key == KeyboardHook.VKeys.RCONTROL)
                CTRLPressed = false;
        }

        private void KbHook_KeyDown(KeyboardHook.VKeys key)
        {
            Console.WriteLine(key.ToString());
            if (!macroRunning)
            {
                if (key == KeyboardHook.VKeys.CONTROL || key == KeyboardHook.VKeys.LCONTROL || key == KeyboardHook.VKeys.RCONTROL)
                    CTRLPressed = true;
                if (!CTRLPressed)
                {
                    if (key == KeyboardHook.VKeys.SPACE || key == KeyboardHook.VKeys.RETURN || key == KeyboardHook.VKeys.TAB || key == KeyboardHook.VKeys.BACK)
                    {
                        foreach (var item in codes)
                            if (item.Short == currentword)
                            {
                                macroRunning = true;
                                for (int i = 0; i < currentword.Length; i++)
                                    SendKeys.SendWait("{BS}");
                                SendKeys.SendWait(ParseLongCode(item.Long));
                                macroRunning = false;
                                currentword = "";
                            }
                        currentword = "";
                    }
                    else if (key.ToString().Contains("KEY_"))
                        currentword += key.ToString().Remove(0, 4);
                }
            }
            label1.Text = currentword;
        }
        #endregion  

        #region Functions
        private void ImportCodes()
        {
            using (StreamReader sr = new StreamReader("settings.txt"))
                while(!sr.EndOfStream)
                    codes.Add(new KeyCodes(sr.ReadLine().Split(':')));
        }

        private void ExportCodes()
        {
            using (StreamWriter sw = new StreamWriter("settings.txt"))
                foreach (var item in codes)
                    sw.WriteLine(String.Format("{0}:{1}", item.Short, item.Long));
        }

        private string ParseLongCode(string toParse)
        {

            if(toParse[0] == '%' && toParse[toParse.Length - 1] == '%')
                switch (toParse.Replace("%", string.Empty))
                {
                    case "datetime":
                        return (DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
                    case "date":
                        return DateTime.Now.ToLongDateString();
                    case "time":
                        return DateTime.Now.ToLongTimeString();
                }
            return toParse;
        }
        #endregion

        #region Form Events
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExportCodes();
            kbHook.Uninstall();            
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            macroRunning = true;
            AddCodeForm.txtShort.Text = "";
            AddCodeForm.txtLong.Text = "";
            if (AddCodeForm.ShowDialog(this) == DialogResult.OK)
                codes.Add(new KeyCodes(AddCodeForm.txtShort.Text.ToUpper(), AddCodeForm.txtLong.Text));
            macroRunning = false;
        }
        #endregion

        #region ContextMenu
        private void toolAdd_Click(object sender, EventArgs e)
        {
            macroRunning = true;
            AddCodeForm.txtShort.Text = "";
            AddCodeForm.txtLong.Text = "";
            if (AddCodeForm.ShowDialog(this) == DialogResult.OK)
                codes.Add(new KeyCodes(AddCodeForm.txtShort.Text.ToUpper(), AddCodeForm.txtLong.Text));
            macroRunning = false;
        }

        private void toolEdit_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                macroRunning = true;
                KeyCodes val = codes[listView1.SelectedIndices[0]];
                AddCodeForm.txtShort.Text = val.Short;
                AddCodeForm.txtLong.Text = val.Long;
                if (AddCodeForm.ShowDialog(this) == DialogResult.OK)
                {
                    codes.Remove(val);
                    codes.Add(new KeyCodes(AddCodeForm.txtShort.Text.ToUpper(), AddCodeForm.txtLong.Text));
                }
                macroRunning = false;
            }
        }

        private void toolDelete_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                codes.RemoveAt(listView1.SelectedIndices[0]);
        }
        #endregion
    }

    #region KeyCodes class
    public class KeyCodes
    {
        public KeyCodes(string shortcode, string longcode)
        {
            Short = shortcode;
            Long = longcode;
        }
        public KeyCodes(string[] codes)
        {
            Short = codes[0];
            Long = codes[1];
        }
        public string Short { get; set; }
        public string Long { get; set; }
    }
    #endregion
}
