using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Xml;
using System.Collections.Generic;

// fix/stuff:
// fix tile picker/add label to show which tile selected/pointed at
// stcani.tbl
// undo buffer
// copy buffer -> auto paste on new map bug? (hyru)
// delete block
// fix deselecting when switching to right fg (heiler)
// popup with scrollable list of tiles (acht/heiler)
// fix pasting at < 0
// deleting / 0 tile causes popup exception?
// x newfile flag not set false after saving
// proper copy/cut/paste/select

namespace MapEditor
{
    public partial class Form1 : Form
    {
        //delegate void Temp();

        System.Collections.Hashtable STCforegrounds = new Hashtable();
        String darkagesPath;
        //bool newFile = false;
        //List<UndoItem> undoBuffer = new List<UndoItem>();
        string[] titles =
        {
            "This shirt is \"dry-clean only\"...which means it's dirty.",
            "If carrots got you drunk, rabbits would be fucked up.",
            "I wish I could play little league now, I'd kick some fuckin' ass.",
            "My fake plants died because I did not pretend to water them.",
            "I don't have a microwave oven, but I do have a clock that occasionally cooks shit.",
            "I had a parrot. The parrot talked, but it did not say \"I'm hungry,\" so it died.",
            "I order the club sandwich all the time, but I'm not even a member, man. I don't know how I get away with it.",
            "I saw this wino, he was eating grapes. I was like, \"Dude, you have to wait.\"",
            "I'd like to make a vending machine that sells vending machines. It'd have to be real fuckin' big!",
            "I'm against picketing, but I don't know how to show it.",
            "I haven't slept for ten days, because that would be too long.",
            "My friend asked me if I wanted a frozen banana, I said \"No, but I want a regular banana later, so ... yeah\".",
            "I used to do drugs. I still do, but I used to, too.",
            "I bought a seven-dollar pen because I always lose pens and I got sick of not caring.",
            "I like rice. Rice is great when you're hungry and you want 2,000 of something.",
            "I love my fed-ex guy cause he's a drug dealer and he doesn't even know it.",
            "I got a king sized bed. I don't know any kings, but if one came over, I guess he'd be comfortable.",
            "You know that word \"lull\"? That's four letters, three of them are L's, fuck!",
            "They say Flintstone's vitamins are chewable. All vitamins are chewable, it's just that they taste shitty.",
            "I went to a record store, they said they specialized in hard-to-find records.  Nothing was alphabetized.",
            "Imagine if the headless horseman had a headless horse.  That would be fucking chaos.",
            "A fly was very close to being called a \"land,\" cause that's what they do half the time.",
            "My belt holds up my pants and my pants have belt loops that hold up the belt. What the fuck’s really goin on down there? Who is the real hero?",
            "I can't tell you what hotel I'm stayin' in, but there are two trees involved.",
            "Fish are always eating other fish. If fish could scream, the ocean would be loud as shit.",
            "I like vending machines 'cause snacks are better when they fall.",
            "This guy handed me a picture of him; he said, \"Here's a picture of me when I was younger.\" Every picture is of you when you were younger.",
            "Snake eyes!  That's a gambling term.  Or it's an animal term too.",
            "I think Bigfoot is blurry, that's the problem. It's not the photographer's fault.",
            "I was walking down the street with my friend and he said, \"I hear music\", as if there is any other way you can take it in.",
        };
        //System.Collections.Hashtable STSforegrounds = new Hashtable();
        public Form1(string openFile)
        {
            //Temp temp = delegate()
            //{
                //MessageBox.Show("Hi!");
            //};

            //temp.Invoke();
            InitializeComponent();
            pictureBox1.Image = new Bitmap(560, 480);
            button10_Click(null, null); //yes
            pictureBox1.MouseDown += new MouseEventHandler(pictureBox1_MouseDown);
            pictureBox1.MouseLeave += new EventHandler(pictureBox1_MouseLeave);
            pictureBox1.MouseUp += new MouseEventHandler(pictureBox1_MouseUp);
            pictureBox1.MouseMove += new MouseEventHandler(pictureBox1_MouseDown);
            pictureBox2.MouseDown += new MouseEventHandler(pictureBox2_MouseDown);
            pictureBox5.MouseDown += new MouseEventHandler(pictureBox5_MouseDown);
            pictureBox5.MouseMove += new MouseEventHandler(pictureBox5_MouseDown);
            pictureBox6.MouseDown += new MouseEventHandler(pictureBox6_MouseDown);
            pictureBox6.MouseMove += new MouseEventHandler(pictureBox6_MouseDown);
            panel1.Scroll += new ScrollEventHandler(panel1_Scroll);
            Resize += new EventHandler(Form1_Resize);
            bool s = LoadXmlData();
            if (!s)
            {
                darkagesPath = "c:\\progra~1\\kru\\dark ages\\";
                WriteConfigXml();
            }

            int titleIndex = new Random((int)DateTime.Now.Ticks).Next() % titles.Length;
            this.Text = titles[titleIndex];
            checkPathsExist();

            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 500;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(button10, "Paint selection to map");
            toolTip1.SetToolTip(button9, "Select single tile");
            toolTip1.SetToolTip(button11, "Select a block of tiles");
            toolTip1.SetToolTip(button12, "Select Darkages install directory");
            toolTip1.SetToolTip(button13, "Open map file");
            toolTip1.SetToolTip(button14, "Toggle background");
            toolTip1.SetToolTip(button15, "Toggle left foreground");
            toolTip1.SetToolTip(button16, "Toggle right foreground");
            toolTip1.SetToolTip(button17, "Edit background");
            toolTip1.SetToolTip(button18, "Edit left foreground");
            toolTip1.SetToolTip(button19, "Edit right foreground");
            toolTip1.SetToolTip(button20, "Edit all layers");
            toolTip1.SetToolTip(button21, "Edit both foreground layers");
            toolTip1.SetToolTip(button22, "Return to top of map");
            toolTip1.SetToolTip(button24, "Undo [Ctrl+Z]");
            toolTip1.SetToolTip(checkBox1, "If selection includes blank foregrounds, include them when pasting to map.");
            toolTip1.SetToolTip(checkBox2, "Show collision points in map and foreground tiles list.");

            tabControl2.SelectedIndexChanged += new EventHandler(tabControl2_SelectedIndexChanged);

            DragEnter += new DragEventHandler(OnDragEnter);
            DragDrop += new DragEventHandler(OnDragDrop);
            //pictureBox1.MouseMove += new MouseEventHandler(pictureBox1_MouseOver);
            //Thread t = new Thread(new ThreadStart(LoadDat));
            //t.Start();
            if (openFile != null)
            {
                openMap(openFile, false);
            }
        }


        private void OnDragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            //Debug.WriteLine("OnDragEnter");
            string filename;
            bool validData = GetFilename(out filename, e);
            if (validData)
            {
                //if (lastFilename != filename)
                //{
                    //thumbnail.Image = null;
                    //thumbnail.Visible = false;
                    //lastFilename = filename;
                    //getImageThread = new Thread(new ThreadStart(LoadImage));
                    //getImageThread.Start();
                //}
                //else
                //{
                    //thumbnail.Visible = true;
                //}
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void OnDragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            //Debug.WriteLine("OnDragDrop");
            string filename;
            bool validData = GetFilename(out filename, e);
            if (validData)
            {
                //while (getImageThread.IsAlive)
                //{
                    //Application.DoEvents();
                    //Thread.Sleep(0);
                //}
                //thumbnail.Visible = false;
                //image = nextImage;
                //AdjustView();
                //if ((pb.Image != null) && (pb.Image != nextImage))
                //{
                    //pb.Image.Dispose();
                //}
                //pb.Image = image;
                openMap(filename, false);
            }
        }

        protected bool GetFilename(out string filename, DragEventArgs e)
        {
            bool ret = false;
            filename = String.Empty;

            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = ((IDataObject)e.Data).GetData("FileName") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is String))
                    {
                        filename = ((string[])data)[0];
                        string ext = Path.GetExtension(filename).ToLower();
                        if ((ext == ".map"))
                        {
                            ret = true;
                        }
                    }
                }
            }
            return ret;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData)
            {
                case (Keys.Control | Keys.F4):
                    closeMap();
                    return true; // Tell the caller that the key has been handled
                //case (Keys.Control | Keys.F1):
                    //MessageBox.Show("You pressed Crtl-F1");
                    //return true;
                case (Keys.Control | Keys.N):
                    newToolStripMenuItem_Click(null, null);
                    return true; // Tell the caller that the key has been handled
                case (Keys.Control | Keys.T):
                    newToolStripMenuItem_Click(null, null);
                    return true; // Tell the caller that the key has been handled
                case (Keys.Control | Keys.Z):
                    button24_Click(null, null);
                    return true; // Tell the caller that the key has been handled

            };
            // If the key hasn't been used by you then pass it to the base class
            return base.ProcessDialogKey(keyData);
        }

        private void closeMap()
        {
            if (map == null)
            {
                return;
            }

            maps.RemoveAt(selectedTabIndex);
            tabControl2.TabPages.RemoveAt(selectedTabIndex);
        }
        void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (maps.Count == 0)
            {
                return;
            }
            if (selectedTabIndex >= 0 && selectedTabIndex < maps.Count)
            {
                maps[selectedTabIndex].vscroll = Capricorn.Drawing.DAGraphics.vscroll;
                maps[selectedTabIndex].hscroll = Capricorn.Drawing.DAGraphics.hscroll;
            }
            selectedTabIndex = tabControl2.SelectedIndex;
            //MessageBox.Show("Fuck off " + selectedTabIndex + " " + maps.Count);
            Capricorn.Drawing.DAGraphics.vscroll = maps[selectedTabIndex].vscroll;
            Capricorn.Drawing.DAGraphics.hscroll = maps[selectedTabIndex].hscroll;

            listBox1.Items.Clear();
            // Add the dimensions to the GUI thingy and pick item #0 by default
            for (int i = 0; i < widths.Count; i++)
            {
                string s = "Width: " + widths[i] + " Height: " + heights[i];
                listBox1.Items.Add(s);
            }
            RefreshMap();
            tabControl2.TabPages[tabControl2.SelectedIndex].Controls.Add(pictureBox1);
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Visible = true;
        }

        void Form1_Resize(object sender, EventArgs e)
        {
            if (map == null)
            {
                return;
            }
            RefreshMap();
            if (Capricorn.Drawing.DAGraphics.editMode == 0)
            {
                RefreshTilesetBg();
            }
            else if (Capricorn.Drawing.DAGraphics.editMode == 1 || Capricorn.Drawing.DAGraphics.editMode == 2)
            {
                RefreshTilesetHpf();
            }
        }

        void panel1_Scroll(object sender, ScrollEventArgs e)
        {
            if (map == null)
            {
                return;
            }
            if (Capricorn.Drawing.DAGraphics.editMode == 1 || Capricorn.Drawing.DAGraphics.editMode == 2)
            {
                double percent = 1.05 * (panel1.VerticalScroll.Value / (double)panel1.VerticalScroll.Maximum);//e.NewValue / 10000.0;
                
                int newStart = (int)(maxHpf * percent);
                newStart = (newStart / 6) * 6;
                tileset_index = newStart;
//                MessageBox.Show("Hi " + percent + " - " + maxHpf);
                RefreshTilesetHpf();
            }
            else if (Capricorn.Drawing.DAGraphics.editMode == 0)
            {
                double percent = 1.05 * (panel1.VerticalScroll.Value / (double)panel1.VerticalScroll.Maximum);//e.NewValue / 10000.0;

                int newStart = (int)(tiles.TileCount * percent);
                newStart = (newStart / 3) * 3;
                tileset_index = newStart;
                //                MessageBox.Show("Hi " + percent + " - " + maxHpf);
                RefreshTilesetBg();
            }
        }

        void checkPathsExist()
        {
            if (!darkagesPath.EndsWith("\\"))
            {
                darkagesPath = darkagesPath + "\\";
            }

            if (!File.Exists(darkagesPath + "ia.dat") || !File.Exists(darkagesPath + "seo.dat"))
            {
                button13.Enabled = false;
                openToolStripMenuItem.Enabled = false;
            }
            else
            {
                button13.Enabled = true;
                openToolStripMenuItem.Enabled = true;
            }
        }

        public class LoadedMapInfo
        {
            public Capricorn.Drawing.MAPFile map;
            public ArrayList widths = new ArrayList();
            public ArrayList heights = new ArrayList();
            public List<UndoItem> undoBuffer = new List<UndoItem>();
            public int vscroll = 0;
            public int hscroll = 0;
            public bool newMap;
        }
        public Capricorn.IO.DATArchive seodat;
        public Capricorn.IO.DATArchive iadat;
        //lod3079.map
        public List<LoadedMapInfo> maps = new List<LoadedMapInfo>();

        public Capricorn.Drawing.MAPFile map
        {
            get {
                if (selectedTabIndex >= maps.Count)
                    return null;
                return maps[selectedTabIndex].map;
            }
            //set { this.m_LevPoints = value; }
        }

        public ArrayList widths
        {
            get
            {
                if (selectedTabIndex >= maps.Count)
                    return null;
                return maps[selectedTabIndex].widths;
            }
        }

        public ArrayList heights
        {
            get
            {
                if (selectedTabIndex >= maps.Count)
                    return null;
                return maps[selectedTabIndex].heights;
            }
        }

        public bool newFile
        {
            get
            {
                if (selectedTabIndex >= maps.Count)
                    return false;
                return maps[selectedTabIndex].newMap;
            }

            set
            {
                if (selectedTabIndex >= maps.Count)
                    return;
                maps[selectedTabIndex].newMap = value;
            }
        }

        public List<UndoItem> undoBuffer
        {
            get
            {
                if (selectedTabIndex >= maps.Count)
                    return null;
                return maps[selectedTabIndex].undoBuffer;
            }
        }
        
        int selectedTabIndex = 0;
        public Capricorn.Drawing.Tileset tiles;
        public Capricorn.Drawing.Tileset tileas;
        public Capricorn.Drawing.PaletteTable bgtable;
        public Capricorn.Drawing.PaletteTable fgtable;
        public int tileset_index = 0;
        //now for the right menu only
        //bool fgedit = false;
        //0=paint, 1=drop, 2=select
        //int selectedtileBG = 0;
        //int selectedtileFGleft = 0;
        //int selectedtileFGright = 0;

        int scrollx = 0;
        int scrolly = 0;


        int panXStart = -1;
        int panYStart = -1;
        //int panXEnd = -1;
        //int panYEnd = -1;

        int panXScrollStart = -1;
        int panYScrollStart = -1;


        bool snowy = false;

        //private Capricorn.Drawing.MAPFile getMap()
        //{
            //if (selectedTabIndex >= maps.Count)
                //return null;
            //return maps[selectedTabIndex];
        //}

        public bool LoadXmlData()
        {
            XmlDocument doc = new XmlDocument();
            string dir = Path.GetDirectoryName(Application.ExecutablePath);
            if (!dir.EndsWith("\\"))
            {
                dir = dir + "\\";
            }

            try
            {
                doc.Load(dir + "Config.xml");
            }
            catch (Exception)
            {
                return false;
            }

            XmlNodeList nodes = doc.SelectNodes("//Config");
            if (nodes.Count == 0)
            {
                return false;
            }
            XmlNode config = nodes[0];
            foreach (XmlNode childNode in config.ChildNodes)
            {
                if (childNode.Name == "DarkagesPath")
                {
                    darkagesPath = childNode.InnerText;
                }

            }

            return true;
        }


        public void WriteConfigXml()
        {
            string dir = Path.GetDirectoryName(Application.ExecutablePath);
            if (!dir.EndsWith("\\"))
            {
                dir = dir + "\\";
            }

            FileStream stream = File.Create(dir + "Config.xml");
            XmlTextWriter xml = new XmlTextWriter(stream, Encoding.ASCII);

            xml.Formatting = Formatting.Indented;
            xml.Indentation = 2;
            xml.IndentChar = ' ';

            xml.WriteStartDocument(true);
            xml.WriteStartElement("Config");
            xml.WriteStartElement("DarkagesPath");
            xml.WriteString(darkagesPath);
            xml.WriteEndElement();
            xml.WriteEndElement();
            xml.WriteEndDocument();

            xml.Flush();
            xml.Close();

        }





        int maxHpf;

        //public void LoadFilesFromDat(string datfilename)
        //{
            
        //    Capricorn.IO.DATArchive dat = Capricorn.IO.DATArchive.FromFile(datfilename);
        //    foreach (Capricorn.IO.DATFileEntry df in dat.Files)
        //    {
        //        if (df.Name.Contains(".hpf"))
        //        {
        //            byte[] thisHpf = dat.ExtractFile(df);
        //            Capricorn.Drawing.HPFImage hpf = Capricorn.Drawing.HPFImage.FromRawData(thisHpf);
        //            int filenum = int.Parse(df.Name.Substring(3, 5));
        //            if (df.Name.Contains("stc"))
        //            {
        //                STCforegrounds.Add(filenum, hpf);
        //                //if (filenum > maxHpf)
        //                //{
        //                    //maxHpf = filenum;
        //                //}
        //            }
                    
        //        }
        //    }
        //}
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        public bool IsWhole(double value)
        {
            return value == (double)((int)value);
        }

        private void openMap(string filename, bool newMap)
        {
            //MessageBox.Show("Opening " + newFile);
            String seoFile = darkagesPath.EndsWith("\\") ? darkagesPath + "seo.dat" : darkagesPath + "\\seo.dat";
            String iaFile = darkagesPath.EndsWith("\\") ? darkagesPath + "ia.dat" : darkagesPath + "\\ia.dat";
            if (!File.Exists(seoFile))
            {
                MessageBox.Show(seoFile + " not found.");
                return;
            }
            if (!File.Exists(iaFile))
            {
                MessageBox.Show(iaFile + " not found.");
                return;
            }

                if (!stuffLoaded)
                {
                    seodat = Capricorn.IO.DATArchive.FromFile(seoFile);
                    iadat = Capricorn.IO.DATArchive.FromFile(iaFile);
                }

                
                //lod3079.map
                if (newMap)
                    tabControl2.TabPages.Add("[New]");
                else
                {
                    //string smallFn = filename;
                    string smallFn = filename.Substring(1 + filename.LastIndexOfAny(new char[]{'\\', '/'}));
                    tabControl2.TabPages.Add(smallFn);
                }
                tabControl2.TabPages[tabControl2.TabPages.Count - 1].Controls.Add(pictureBox1);
                pictureBox1.Location = new Point(0, 0);
                pictureBox1.Visible = true;
                LoadedMapInfo info = new LoadedMapInfo();
                info.newMap = newMap;
                info.map = (Capricorn.Drawing.MAPFile.FromFile(filename));
                maps.Add(info);
                tabControl2.SelectedIndex = tabControl2.TabPages.Count - 1;

                //map = Capricorn.Drawing.MAPFile.FromFile(filename);
                double sq = Math.Sqrt((double)(map.Tiles.Length));
                int start = (int)sq;
                info.widths = new ArrayList();
                info.heights = new ArrayList();
                if (IsWhole(sq))
                {
                    info.widths.Add((int)sq);
                    info.heights.Add((int)sq);
                    map.Height = (int)sq;
                    map.Width = (int)sq;
                    start--;
                }

                //get some other dimensions, start at the squarest
                for (int i = start; i >= 2; i--)
                {
                    double result = (double)(map.Tiles.Length) / (double)i;
                    if (i < 256 && result < 256)
                    {
                        if (IsWhole(result))
                        {
                            // add both combos
                            widths.Add((int)i);
                            heights.Add((int)result);
                            widths.Add((int)result);
                            heights.Add((int)i);
                        }
                    }
                }

                listBox1.Items.Clear();
                // Add the dimensions to the GUI thingy and pick item #0 by default
                for (int i = 0; i < widths.Count; i++)
                {
                    string s = "Width: " + widths[i] + " Height: " + heights[i];
                    listBox1.Items.Add(s);
                }

                selectedTabIndex = tabControl2.TabPages.Count - 1;

                map.Name = filename;
                if (!stuffLoaded)
                {
                    tiles = Capricorn.Drawing.Tileset.FromArchive("TILEA.BMP", seodat);
                    tileas = Capricorn.Drawing.Tileset.FromArchive("TILEAS.BMP", seodat);
                    bgtable = new Capricorn.Drawing.PaletteTable();
                    fgtable = new Capricorn.Drawing.PaletteTable();
                    fgtable.LoadPalettes("stc", iadat);
                    fgtable.LoadTables("stc", iadat);
                    bgtable.LoadPalettes("mpt", seodat);
                    bgtable.LoadTables("mpt", seodat);
                    Capricorn.Drawing.DAGraphics.sotp = iadat.ExtractFile("sotp.dat");

                    maxHpf = 50000;
                    double guessModifier = 40000;
                    while (guessModifier > 1)
                    {
                        Capricorn.Drawing.HPFImage hpf = Capricorn.Drawing.HPFImage.FromArchive(
                            "stc" + maxHpf.ToString().PadLeft(5, '0') + ".hpf", true, iadat);
                        if (hpf == null)
                        {
                            maxHpf -= (int)guessModifier;
                        }
                        else
                        {
                            maxHpf += (int)guessModifier;
                        }
                        guessModifier /= 1.25;
                    }
                    
                }

                stuffLoaded = true;
                Bitmap b = null;
                if(snowy)
                    b = Capricorn.Drawing.DAGraphics.RenderMap(map, tileas, bgtable, fgtable, iadat);
                else
                    b = Capricorn.Drawing.DAGraphics.RenderMap(map, tiles, bgtable, fgtable, iadat);
                pictureBox1.Image = b;

                RefreshTilesetBg();
                listBox1.SelectedIndex = 0;
            
            
        }
        bool stuffLoaded = false;
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "Darkages map files|*.map";

            DialogResult ok = of.ShowDialog();

            if (ok == DialogResult.OK)
            {
                newFile = false;
                openMap(of.FileName, false);
            }
        }

        public void RefreshMap()
        {
            if (map == null)
                return;
            Capricorn.Drawing.DAGraphics.displayheight = pictureBox1.Height;
            Capricorn.Drawing.DAGraphics.displaywidth = pictureBox1.Width;
            Bitmap b = null;
            if(snowy)
                b = Capricorn.Drawing.DAGraphics.RenderMap(map, tileas, bgtable, fgtable, iadat);
            else
                b = Capricorn.Drawing.DAGraphics.RenderMap(map, tiles, bgtable, fgtable, iadat);

            pictureBox1.Image = b;
            //pictureBox1.Refresh();
            //scrollbars

            //int totalcolumns = map.Width + map.Height - 1;
            //double vssize = ((double)pictureBox1.Height) / (totalcolumns * 14);
            //double hssize = ((double)pictureBox1.Width) / (totalcolumns * 28);
            //int vp = (int)(vssize * pictureBox1.Height);
            //int hp = (int)(hssize * pictureBox1.Width);
            //int vtotal = pictureBox1.Height + vp / 4;
            //int htotal = pictureBox1.Width + hp / 4;
            //double hpercent = ((double)(Capricorn.Drawing.DAGraphics.hscroll/*+(totalcolumns * 28)*/) / 28.0) / (totalcolumns + 1);
            //double vpercent = ((double)Capricorn.Drawing.DAGraphics.vscroll / 14.0) / (totalcolumns + 1);
            //int centerx = (int)(hp / 2 + hpercent * htotal) + pictureBox1.Height / 2;
            //int centery = (int)(vp / 2 + vpercent * vtotal);


            //Bitmap h = new Bitmap(pictureBox1.Width, 10);
            //Bitmap v = new Bitmap(10, pictureBox1.Height);
            //Graphics gh = Graphics.FromImage(h);
            //gh.FillRectangle(Brushes.Blue, centerx - hp, 0, hp, 10);
            //Graphics gv = Graphics.FromImage(v);
            //gv.FillRectangle(Brushes.Blue, 0, centery - vp / 2, 10, vp);
            //pictureBox5.Image = h;
            //pictureBox5.Refresh();
            //pictureBox6.Image = v;
            //pictureBox6.Refresh();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.hscroll += 56;
            RefreshMap();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.vscroll += 28;
            RefreshMap();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.hscroll -= 56;
            RefreshMap();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.vscroll -= 28;
            RefreshMap();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }
        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            int mx = e.X;
            int my = e.Y;
            if (Capricorn.Drawing.DAGraphics.editMode == 0)
            {
                Capricorn.Drawing.DAGraphics.selection = new Capricorn.Drawing.MapTile[1];
                Capricorn.Drawing.DAGraphics.selectionWidth = 1;
                ushort selectedtileBG = (ushort)(tileset_index + (my / 28) * 3 + mx / 56 + 1);
                Capricorn.Drawing.DAGraphics.selection[0] = new Capricorn.Drawing.MapTile(selectedtileBG, 0, 0);
                label4.Text = (selectedtileBG - 1).ToString();
                RefreshTilesetBg();
            }
            else if (Capricorn.Drawing.DAGraphics.editMode == 1)
            {
                int xIndex = mx / 28;
                int yIndex = tileset_index / 6;
                int y = 0;
                foreach (int yy in hpfHeights)
                {
                    y += yy;
                    if (my > y)
                    {
                        yIndex ++;
                    }
                }

                Capricorn.Drawing.DAGraphics.selection = new Capricorn.Drawing.MapTile[1];
                Capricorn.Drawing.DAGraphics.selectionWidth = 1;
                ushort index = (ushort)(yIndex * 6 + xIndex);
                Capricorn.Drawing.DAGraphics.selection[0] = new Capricorn.Drawing.MapTile(0, index, 0);
                label4.Text = index.ToString();
                RefreshTilesetHpf();
            }
            else if (Capricorn.Drawing.DAGraphics.editMode == 2)
            {
                int xIndex = mx / 28;
                int yIndex = tileset_index / 6;
                int y = 0;
                foreach (int yy in hpfHeights)
                {
                    y += yy;
                    if (my > y)
                    {
                        yIndex++;
                    }
                }

                Capricorn.Drawing.DAGraphics.selection = new Capricorn.Drawing.MapTile[1];
                Capricorn.Drawing.DAGraphics.selectionWidth = 1;
                ushort index = (ushort)(yIndex * 6 + xIndex);
                Capricorn.Drawing.DAGraphics.selection[0] = new Capricorn.Drawing.MapTile(0, 0, index);
                label4.Text = index.ToString();
                RefreshTilesetHpf();
            }
            // ~~~ here - add click processing for foregrounds
        }

        void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if ((Capricorn.Drawing.DAGraphics.clickmode == 2 || Capricorn.Drawing.DAGraphics.clickmode == 3) && Capricorn.Drawing.DAGraphics.selectionXStart != -1)
            {
                int sxs = Capricorn.Drawing.DAGraphics.selectionXStart;
                int sys = Capricorn.Drawing.DAGraphics.selectionYStart;
                int sxe = Capricorn.Drawing.DAGraphics.selectionXEnd;
                int sye = Capricorn.Drawing.DAGraphics.selectionYEnd;
                // set selection
                int xMin = sxs < sxe ? sxs : sxe;
                int yMin = sys < sye ? sys : sye;
                int xMax = sxs > sxe ? sxs : sxe;
                int yMax = sys > sye ? sys : sye;
                Capricorn.Drawing.DAGraphics.selectionWidth = xMax - xMin + 1;
                int selectionHeight = yMax - yMin + 1;
                if (Capricorn.Drawing.DAGraphics.clickmode == 2)
                {
                    Capricorn.Drawing.DAGraphics.selection = new Capricorn.Drawing.MapTile[Capricorn.Drawing.DAGraphics.selectionWidth * selectionHeight];
                }

                // For delete
                UndoItem undo = new UndoItem();
                int blockWidth = (xMax - xMin + 1);
                int blockHeight = (yMax - yMin + 1);
                undo.bgData = new ushort[blockHeight * blockWidth];
                undo.fglData = new ushort[blockHeight * blockWidth];
                undo.fgrData = new ushort[blockHeight * blockWidth];
                undo.width = (xMax - xMin + 1);
                undo.x = xMin;
                undo.y = yMin;
                for (int y = yMin; y <= yMax; y++)
                {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        if (Capricorn.Drawing.DAGraphics.clickmode == 2)
                        {
                            Capricorn.Drawing.DAGraphics.selection[(y - yMin) * Capricorn.Drawing.DAGraphics.selectionWidth + (x - xMin)] = new Capricorn.Drawing.MapTile(0, 0, 0);
                            if (x < map.Width && x >= 0 && y < map.Height && y >= 0)
                            {
                                Capricorn.Drawing.DAGraphics.selection[(y - yMin) * Capricorn.Drawing.DAGraphics.selectionWidth + (x - xMin)].FloorTile = map.Tiles[y * map.Width + x].FloorTile;
                                Capricorn.Drawing.DAGraphics.selection[(y - yMin) * Capricorn.Drawing.DAGraphics.selectionWidth + (x - xMin)].LeftWall = map.Tiles[y * map.Width + x].LeftWall;
                                Capricorn.Drawing.DAGraphics.selection[(y - yMin) * Capricorn.Drawing.DAGraphics.selectionWidth + (x - xMin)].RightWall = map.Tiles[y * map.Width + x].RightWall;
                            }
                        }
                        else
                        {
                            if (x < map.Width && x >= 0 && y < map.Height && y >= 0)
                            {
                                if (Capricorn.Drawing.DAGraphics.editMode == 0 || Capricorn.Drawing.DAGraphics.editMode == 3)
                                {
                                    undo.bgData[(y - yMin) * blockWidth + (x - xMin)] = map.Tiles[y * map.Width + x].FloorTile;
                                    map.Tiles[y * map.Width + x].FloorTile = 1;
                                    undo.bg = true;
                                }
                                if (Capricorn.Drawing.DAGraphics.editMode == 1 || Capricorn.Drawing.DAGraphics.editMode == 3 || Capricorn.Drawing.DAGraphics.editMode == 4)
                                {
                                    undo.fglData[(y - yMin) * blockWidth + (x - xMin)] = map.Tiles[y * map.Width + x].LeftWall;
                                    map.Tiles[y * map.Width + x].LeftWall = 0;
                                    undo.fgl = true;
                                }
                                if (Capricorn.Drawing.DAGraphics.editMode == 2 || Capricorn.Drawing.DAGraphics.editMode == 3 || Capricorn.Drawing.DAGraphics.editMode == 4)
                                {
                                    undo.fgrData[(y - yMin) * blockWidth + (x - xMin)] = map.Tiles[y * map.Width + x].RightWall;
                                    map.Tiles[y * map.Width + x].RightWall = 0;
                                    undo.fgr = true;
                                }
                            }
                        }
                    }
                }

                if (Capricorn.Drawing.DAGraphics.clickmode == 3)
                {
                    undoBuffer.Add(undo);
                }
            }

            panXStart = -1;
            panYStart = -1;
            Capricorn.Drawing.DAGraphics.selectionXStart = -1;
            Capricorn.Drawing.DAGraphics.selectionYStart = -1;
            Capricorn.Drawing.DAGraphics.selectionXEnd = -1;
            Capricorn.Drawing.DAGraphics.selectionYEnd = -1;
            Capricorn.Drawing.DAGraphics.MouseHoverX = -1;
            Capricorn.Drawing.DAGraphics.MouseHoverY = -1;

            panXScrollStart = -1;
            panYScrollStart = -1;
            prev_mx = -1;
            prev_my = -1;
            label5.Text = "";
            RefreshMap();
        }

        void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            if ((Capricorn.Drawing.DAGraphics.clickmode == 2 || Capricorn.Drawing.DAGraphics.clickmode == 3) && Capricorn.Drawing.DAGraphics.selectionXStart != -1)
            {
                int sxs = Capricorn.Drawing.DAGraphics.selectionXStart;
                int sys = Capricorn.Drawing.DAGraphics.selectionYStart;
                int sxe = Capricorn.Drawing.DAGraphics.selectionXEnd;
                int sye = Capricorn.Drawing.DAGraphics.selectionYEnd;
                // set selection
                int xMin = sxs < sxe ? sxs : sxe;
                int yMin = sys < sye ? sys : sye;
                int xMax = sxs > sxe ? sxs : sxe;
                int yMax = sys > sye ? sys : sye;
                Capricorn.Drawing.DAGraphics.selectionWidth = xMax - xMin + 1;
                int selectionHeight = yMax - yMin + 1;
                if (Capricorn.Drawing.DAGraphics.clickmode == 2)
                {
                    Capricorn.Drawing.DAGraphics.selection = new Capricorn.Drawing.MapTile[Capricorn.Drawing.DAGraphics.selectionWidth * selectionHeight];
                }
                for (int y = yMin; y <= yMax; y++)
                {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        if (Capricorn.Drawing.DAGraphics.clickmode == 2)
                        {
                            Capricorn.Drawing.DAGraphics.selection[(y - yMin) * Capricorn.Drawing.DAGraphics.selectionWidth + (x - xMin)] = new Capricorn.Drawing.MapTile(0, 0, 0);
                            if (x < map.Width && x >= 0 && y < map.Height && y >= 0)
                            {
                                Capricorn.Drawing.DAGraphics.selection[(y - yMin) * Capricorn.Drawing.DAGraphics.selectionWidth + (x - xMin)].FloorTile = map.Tiles[y * map.Width + x].FloorTile;
                                Capricorn.Drawing.DAGraphics.selection[(y - yMin) * Capricorn.Drawing.DAGraphics.selectionWidth + (x - xMin)].LeftWall = map.Tiles[y * map.Width + x].LeftWall;
                                Capricorn.Drawing.DAGraphics.selection[(y - yMin) * Capricorn.Drawing.DAGraphics.selectionWidth + (x - xMin)].RightWall = map.Tiles[y * map.Width + x].RightWall;
                            }
                        }
                        else
                        {
                            if (x < map.Width && x >= 0 && y < map.Height && y >= 0)
                            {
                                if (Capricorn.Drawing.DAGraphics.editMode == 0 || Capricorn.Drawing.DAGraphics.editMode == 3)
                                {
                                    map.Tiles[y * map.Width + x].FloorTile = 1;
                                }
                                if (Capricorn.Drawing.DAGraphics.editMode == 1 || Capricorn.Drawing.DAGraphics.editMode == 3 || Capricorn.Drawing.DAGraphics.editMode == 4)
                                {
                                    map.Tiles[y * map.Width + x].LeftWall = 0;
                                }
                                if (Capricorn.Drawing.DAGraphics.editMode == 2 || Capricorn.Drawing.DAGraphics.editMode == 3 || Capricorn.Drawing.DAGraphics.editMode == 4)
                                {
                                    map.Tiles[y * map.Width + x].RightWall = 0;
                                }
                            }
                        }
                    }
                }
            }

            panXStart = -1;
            panYStart = -1;
            Capricorn.Drawing.DAGraphics.selectionXStart = -1;
            Capricorn.Drawing.DAGraphics.selectionYStart = -1;
            Capricorn.Drawing.DAGraphics.selectionXEnd = -1;
            Capricorn.Drawing.DAGraphics.selectionYEnd = -1;
            Capricorn.Drawing.DAGraphics.MouseHoverX = -1;
            Capricorn.Drawing.DAGraphics.MouseHoverY = -1;

            panXScrollStart = -1;
            panYScrollStart = -1;
            prev_mx = -1;
            prev_my = -1;
            label5.Text = "";
            RefreshMap();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (map == null)
                return;
            pictureBox1_MouseOver(sender, e);
            //if (e.Button != MouseButtons.Left)
                //return;
            if (e.Button == MouseButtons.Right)
            {
                if (panXStart == -1)
                {
                    panXStart = e.X;
                    panYStart = e.Y;
                    panXScrollStart = Capricorn.Drawing.DAGraphics.hscroll;
                    panYScrollStart = Capricorn.Drawing.DAGraphics.vscroll;
                }
                else
                {
                    int xdiff = (e.X - panXStart)*2;
                    int ydiff = (e.Y - panYStart)*2;
                    Capricorn.Drawing.DAGraphics.hscroll = panXScrollStart - xdiff;
                    Capricorn.Drawing.DAGraphics.vscroll = panYScrollStart - ydiff;
                    RefreshMap();
                }
            }
            if (e.Button == MouseButtons.Left)
            {
                    int mx = e.X;// + Capricorn.Drawing.DAGraphics.hscroll;// +pictureBox3.Location.X - pictureBox1.Location.X;
                    int my = e.Y;// + Capricorn.Drawing.DAGraphics.vscroll;// +pictureBox3.Location.Y - pictureBox1.Location.Y;
                    //width of the bitmap ~~~
                    int fullwidth = (Capricorn.Drawing.Tileset.TileWidth * (map.Width));
                    //height of the bitmap ~~~
                    int fullheight = ((Capricorn.Drawing.Tileset.TileHeight + 1) * (map.Height));

                    //leftmost corner = column 0, total columns = (map width + map height) - 1 ~~~
                    //if hscroll == tilewidth * ((mapwidth+1)/2-1), columnindex at 320 = ((mapwidth+1)/2-1)
                    int totalcolumns = map.Width + map.Height - 1;
                    //how many columns right of screen center we clicked
                    int relativecolumn = (mx - 14) / 28;//-(320 - (mx)) / 28;
                    //how many columns south of screen top we clicked
                    int relativerow = (my - 7) / 14;

                    int scrolledcolumn = (int)(Capricorn.Drawing.DAGraphics.hscroll / 28 + map.Width) - 10;
                    int scrolledrow = (int)(Capricorn.Drawing.DAGraphics.vscroll / 14);

                    //if hscroll == 0, columnindex at 320 = 0
                    int columnindex = scrolledcolumn + relativecolumn;//(int)((map.Width * 2.0 - 1) * mx / (double)fullwidth) + map.Width - (int)(640.0 / Capricorn.Drawing.Tileset.TileWidth) +1;
                    //top corner = row 0, total rows = map height*2 + 1 ~~~
                    int rowindex = scrolledrow + relativerow;//(int)((map.Height*2.0 - 1) * my / (double)fullheight);
                    //x index into map data.  as row increases, or column increases, it increases.
                    //tc = 4, this = 1
                    //tc = 5, this = 2
                    //tc=199, this = 98
                    int columnat00 = (totalcolumns - 1) / 2;

                    //OLD CODE
                    /*
                    int da_mapx = (int)(rowindex * .5 + (columnindex - columnat00) * .5);
                    //y index into map data.  as row increases, or column decreases, it increases.
                    int da_mapy = (int)(rowindex * .5 - (columnindex - columnat00) * .5);
                    */

                    // 0,0 = center x..
                    // # tiles
                    double shift = ((map.Height/* + map.Width*/) / 2.0/*4*/);
                    int clickx = (int)(e.X + Capricorn.Drawing.DAGraphics.hscroll + shift * Capricorn.Drawing.Tileset.TileWidth - pictureBox1.Width / 2);
                    int clicky = e.Y + Capricorn.Drawing.DAGraphics.vscroll;
                    int intercept_x = (int)(((Capricorn.Drawing.Tileset.TileHeight + 1) * map.Height) / 2.0);
                    int y0_x = (int)(intercept_x - .5 * clickx);
                    int intercept_y = (int)((-(Capricorn.Drawing.Tileset.TileHeight + 1) * map.Height) / 2.0);
                    int y0_y = (int)(intercept_y + .5 * clickx);
                    int da_mapx = (clicky - y0_x) / (Capricorn.Drawing.Tileset.TileHeight + 1);
                    int da_mapy = (clicky - y0_y) / (Capricorn.Drawing.Tileset.TileHeight + 1);

                    if (Capricorn.Drawing.DAGraphics.clickmode == 2 || Capricorn.Drawing.DAGraphics.clickmode == 3)
                    {
                        if (Capricorn.Drawing.DAGraphics.selectionXStart == -1)
                        {
                            Capricorn.Drawing.DAGraphics.selectionXStart = da_mapx;
                            Capricorn.Drawing.DAGraphics.selectionYStart = da_mapy;
                            Capricorn.Drawing.DAGraphics.selectionXEnd = da_mapx;
                            Capricorn.Drawing.DAGraphics.selectionYEnd = da_mapy;
                            label5.Text = "x = " + da_mapx + " y = " + da_mapy;
                            RefreshMap();
                            //panXScrollStart = Capricorn.Drawing.DAGraphics.hscroll;
                            //panYScrollStart = Capricorn.Drawing.DAGraphics.vscroll;
                        }
                        else
                        {
                            //fsd
                            //int xdiff = (e.X - panXStart) * 2;
                            //int ydiff = (e.Y - panYStart) * 2;
                            bool refresh = false;
                            if (da_mapx != Capricorn.Drawing.DAGraphics.selectionXEnd || da_mapy != Capricorn.Drawing.DAGraphics.selectionYEnd)
                            {
                                refresh = true;
                            }
                            Capricorn.Drawing.DAGraphics.selectionXEnd = da_mapx;
                            Capricorn.Drawing.DAGraphics.selectionYEnd = da_mapy;
                            if (refresh)
                            {
                                label5.Text = "x = " + da_mapx + " y = " + da_mapy;
                                RefreshMap();
                            }
                            //Capricorn.Drawing.DAGraphics.hscroll = panXScrollStart - xdiff;
                            //Capricorn.Drawing.DAGraphics.vscroll = panYScrollStart - ydiff;
                            //RefreshMap();
                        }

                    }
                    else
                    {

                        //MessageBox.Show("x = " + da_mapx + " y = " + da_mapy + " y0x = " + y0_x + " y0y = " + y0_y + " intx = " + intercept_x + " inty = " + intercept_y + " cx = " + clickx + " cy = " + clicky);
                        bool changed = false;

                        //MessageBox.Show("column = " + columnat00.ToString() + " mx = " + mx.ToString() + " my = " + my.ToString() + " row = " + rowindex.ToString() + "column = " + columnindex.ToString() + " x = " + da_mapx.ToString() + " y = " + da_mapy.ToString());
                        if (da_mapx >= 0 && da_mapx < map.Width && da_mapy >= 0 && da_mapy < map.Height)
                        {
                            if (Capricorn.Drawing.DAGraphics.clickmode == 0)
                            { //paint tile
                                //if (Capricorn.Drawing.DAGraphics.editMode == 0)
                                //{
                                    // check whether we should refresh -- it's slow as all hell
                                    //if (map.Tiles[da_mapy * map.Width + da_mapx].FloorTile != (ushort)(selectedtileBG + 1))
                                        //changed = true;
                                    //map.Tiles[da_mapy * map.Width + da_mapx].FloorTile = (ushort)(selectedtileBG + 1);
                                //}
                                //else if (Capricorn.Drawing.DAGraphics.editMode == 1)
                                //{

                                    //if (map.Tiles[da_mapy * map.Width + da_mapx].LeftWall != (ushort)(selectedtileFGleft + 1))
                                        //changed = true;
                                    //map.Tiles[da_mapy * map.Width + da_mapx].LeftWall = (ushort)(selectedtileFGleft + 1);
                                //}
                                //else if (Capricorn.Drawing.DAGraphics.editMode == 2)
                                //{
                                    //if (map.Tiles[da_mapy * map.Width + da_mapx].RightWall != (ushort)(selectedtileFGright + 1))
                                        //changed = true;
//
                                    //map.Tiles[da_mapy * map.Width + da_mapx].RightWall = (ushort)(selectedtileFGright + 1);
                                //}
                                //else
                                //{
                                if (Capricorn.Drawing.DAGraphics.selection != null)
                                {
                                    UndoItem undo = new UndoItem();
                                    undo.bgData = new ushort[Capricorn.Drawing.DAGraphics.selection.Length];
                                    undo.fglData = new ushort[Capricorn.Drawing.DAGraphics.selection.Length];
                                    undo.fgrData = new ushort[Capricorn.Drawing.DAGraphics.selection.Length];
                                    undo.width = Capricorn.Drawing.DAGraphics.selectionWidth;
                                    undo.x = da_mapx;
                                    undo.y = da_mapy;
                                    for (int y = da_mapy; y < da_mapy + Capricorn.Drawing.DAGraphics.selection.Length / Capricorn.Drawing.DAGraphics.selectionWidth; y++)
                                    {
                                        for (int x = da_mapx; x < da_mapx + Capricorn.Drawing.DAGraphics.selectionWidth; x++)
                                        {
                                            Capricorn.Drawing.MapTile selectionTile = Capricorn.Drawing.DAGraphics.selection[(y - da_mapy) * Capricorn.Drawing.DAGraphics.selectionWidth + (x - da_mapx)];
                                            if (selectionTile != null)
                                            {
                                                if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
                                                {
                                                    if (Capricorn.Drawing.DAGraphics.editMode == 0 || Capricorn.Drawing.DAGraphics.editMode == 3)
                                                    {
                                                        undo.bg = true;
                                                        undo.bgData[(y - da_mapy) * undo.width + (x - da_mapx)] = map.Tiles[y * map.Width + x].FloorTile;
                                                        map.Tiles[y * map.Width + x].FloorTile = selectionTile.FloorTile;
                                                    }
                                                    if (Capricorn.Drawing.DAGraphics.editMode == 1 || Capricorn.Drawing.DAGraphics.editMode == 3 || Capricorn.Drawing.DAGraphics.editMode == 4)
                                                    {
                                                        undo.fgl = true;
                                                        if(selectionTile.LeftWall > 0 || Capricorn.Drawing.DAGraphics.drawEmptyWalls)
                                                        {
                                                            undo.fglData[(y - da_mapy) * undo.width + (x - da_mapx)] = map.Tiles[y * map.Width + x].LeftWall;
                                                            map.Tiles[y * map.Width + x].LeftWall = selectionTile.LeftWall;
                                                        }
                                                    }
                                                    if (Capricorn.Drawing.DAGraphics.editMode == 2 || Capricorn.Drawing.DAGraphics.editMode == 3 || Capricorn.Drawing.DAGraphics.editMode == 4)
                                                    {
                                                        undo.fgr = true;
                                                        if (selectionTile.RightWall > 0 || Capricorn.Drawing.DAGraphics.drawEmptyWalls)
                                                        {
                                                            undo.fgrData[(y - da_mapy) * undo.width + (x - da_mapx)] = map.Tiles[y * map.Width + x].RightWall;
                                                            map.Tiles[y * map.Width + x].RightWall = selectionTile.RightWall;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    undoBuffer.Add(undo);
                                    RefreshMap();
                                }
                                //}
                            }
                            else if (Capricorn.Drawing.DAGraphics.clickmode == 1)
                            { //suction
                                ushort selectedtileBG = map.Tiles[da_mapy * map.Width + da_mapx].FloorTile;
                                ushort selectedtileFGleft = map.Tiles[da_mapy * map.Width + da_mapx].LeftWall;
                                ushort selectedtileFGright = map.Tiles[da_mapy * map.Width + da_mapx].RightWall;

                                if (Capricorn.Drawing.DAGraphics.editMode == 0)
                                {
                                    tileset_index = selectedtileBG - 1;
                                    selectedtileFGleft = 0;
                                    selectedtileFGright = 0;
                                    label4.Text = tileset_index.ToString();
                                    //RefreshTilesetBg();
                                }
                                else if (Capricorn.Drawing.DAGraphics.editMode == 1)
                                {
                                    tileset_index = (selectedtileFGleft / 6) * 6;
                                    selectedtileBG = 0;
                                    selectedtileFGright = 0;
                                    label4.Text = tileset_index.ToString();
                                    //RefreshTilesetHpf();
                                }
                                else if (Capricorn.Drawing.DAGraphics.editMode == 2)
                                {
                                    tileset_index = (selectedtileFGright / 6) * 6;
                                    selectedtileBG = 0;
                                    selectedtileFGleft = 0;
                                    label4.Text = tileset_index.ToString();
                                    //RefreshTilesetHpf();
                                }

                                Capricorn.Drawing.DAGraphics.selection = new Capricorn.Drawing.MapTile[1];
                                Capricorn.Drawing.DAGraphics.selectionWidth = 1;
                                Capricorn.Drawing.DAGraphics.selection[0] = new Capricorn.Drawing.MapTile(selectedtileBG, selectedtileFGleft, selectedtileFGright);

                                if (Capricorn.Drawing.DAGraphics.editMode == 0)
                                {
                                    RefreshTilesetBg();
                                }
                                else if (Capricorn.Drawing.DAGraphics.editMode == 1 || Capricorn.Drawing.DAGraphics.editMode == 2)
                                {
                                    RefreshTilesetHpf();
                                }
                            }
                            if (changed)
                                RefreshMap();

                        }
                    }
            }
        }

        int prev_mx = -1;
        int prev_my = -1;

        long debug_totalTime = 0;
        int debug_num = 0;
        private void pictureBox1_MouseOver(object sender, MouseEventArgs e)
        {
            long debug_start = DateTime.Now.Ticks / 10000;

            if (map == null)
                return;
            int mx = e.X + 20;// + Capricorn.Drawing.DAGraphics.hscroll;
            int my = e.Y - 10;// + Capricorn.Drawing.DAGraphics.vscroll;
            mx -= (mx % 56);
            my -= (my % 28);

            double shift = ((map.Height/* + map.Width*/) / 2.0/*4*/);
            int clickx = (int)(e.X + Capricorn.Drawing.DAGraphics.hscroll + shift * Capricorn.Drawing.Tileset.TileWidth - pictureBox1.Width / 2);
            int clicky = e.Y + Capricorn.Drawing.DAGraphics.vscroll;
            int intercept_x = (int)(((Capricorn.Drawing.Tileset.TileHeight + 1) * map.Height) / 2.0);
            int y0_x = (int)(intercept_x - .5 * clickx);
            int intercept_y = (int)((-(Capricorn.Drawing.Tileset.TileHeight + 1) * map.Height) / 2.0);
            int y0_y = (int)(intercept_y + .5 * clickx);
            int da_mapx = (clicky - y0_x) / (Capricorn.Drawing.Tileset.TileHeight + 1);
            int da_mapy = (clicky - y0_y) / (Capricorn.Drawing.Tileset.TileHeight + 1);

            Capricorn.Drawing.DAGraphics.MouseHoverX = da_mapx;
            Capricorn.Drawing.DAGraphics.MouseHoverY = da_mapy;
            if (Capricorn.Drawing.DAGraphics.clickmode == 1)
            {
                Capricorn.Drawing.DAGraphics.selectionXStart = da_mapx;
                Capricorn.Drawing.DAGraphics.selectionYStart = da_mapy;
                Capricorn.Drawing.DAGraphics.selectionXEnd = da_mapx;
                Capricorn.Drawing.DAGraphics.selectionYEnd = da_mapy;
            }

            //if (panXStart != -1 || Capricorn.Drawing.DAGraphics.selectionXStart != -1 || Capricorn.Drawing.DAGraphics.editMode == 0)
            //{
                if (da_mapx != prev_mx || da_mapy != prev_my || (panXStart != -1))
                {
                    debug_num++;
                    prev_mx = da_mapx;
                    prev_my = da_mapy;
                    label5.Text = "x = " + da_mapx + " y = " + da_mapy;
                    RefreshMap();
                    //long debug_tb = DateTime.Now.Ticks;
                    pictureBox1.Refresh();
                    long debug_end = DateTime.Now.Ticks / 10000;

                    debug_totalTime += (debug_end - debug_start);
                    if ((debug_num % 100) == 99)
                    {
                        //MessageBox.Show("Total time = " + (debug_totalTime / debug_num) + " ms.");
                    }
                    //long debug_ta = DateTime.Now.Ticks;
                    //MessageBox.Show("Refresh time = " + ((debug_ta - debug_tb) / 10000) + " ms");
                    //    //mx += 28;
                    //    //my += 14;
                    //    Bitmap b = (Bitmap)pictureBox1.Image;
                    //    Bitmap b2 = (Bitmap)pictureBox4.Image;
                    //    Bitmap b3 = (Bitmap)pictureBox3.Image;
                    //    //for (int y = 0; y < 28; y++)
                    //    //{
                    //    //    for (int x = 0; x < 56; x++)
                    //    //    {
                    //    //        Color c = b2.GetPixel(x, y);
                    //    //        if (x + mx < b.Width && y + my < b.Height && x + mx >= 0 && y + my >= 0 && c.B > 200)
                    //    //        {
                    //    //            Color c2 = b.GetPixel(x + mx, y + my);
                    //    //            b3.SetPixel(x, y, c2);
                    //    //        }
                    //    //    }
                    //    //}
                    //    for (int y = 0; y < 28; y++)
                    //    {
                    //        for (int x = 0; x < 56; x++)
                    //        {
                    //            Color c = b2.GetPixel(x, y);
                    //            if (x + mx < b.Width && y + my < b.Height && x + mx >= 0 && y + my >= 0 && c.B < 200)
                    //            {
                    //                //Color c2 = b.GetPixel(x + mx, y + my);
                    //                //b3.SetPixel(x, y, c2);
                    //                b.SetPixel(x + mx, y + my, c);
                    //            }
                    //        }
                    //    }

                    //    //Point p = new Point(mx + pictureBox1.Location.X+1, my + pictureBox1.Location.Y+1);
                    //    //pictureBox3.Location = p;
                    //    //pictureBox3.Image = b3;
                    //    //pictureBox3.Refresh();

                    //    pictureBox1.Refresh();
                    //    //int fullwidth = (Capricorn.Drawing.Tileset.TileWidth * map.Width);
                    //    //int fullheight = ((Capricorn.Drawing.Tileset.TileHeight + 1) * (map.Height + 1));
                    //    //int columnindex = (int)((map.Width * 2 + 1) * mx / fullwidth) + map.Width - (int)(640.0 / Capricorn.Drawing.Tileset.TileWidth) + 1;
                    //    //int rowindex = (int)((map.Height * 2 + 1) * my / fullheight);
                    //    //int da_mapx = (int)(rowindex * .5 + (columnindex - (map.Width + 1)) * .5);
                    //    //int da_mapy = (int)(rowindex * .5 - (columnindex - (map.Height + 1)) * .5);
                    //    //MessageBox.Show("row = " + rowindex.ToString() + "column = " + columnindex.ToString() + "x = " + da_mapx.ToString() + " y = " + da_mapy.ToString());
                    //    //if (da_mapx >= 0 && da_mapx < map.Width && da_mapy >= 0 && da_mapy < map.Height)
                    //    //{
                    //    //map.Tiles[da_mapy * map.Width + da_mapx].FloorTile = 0;
                    //    //RefreshMap();

                    //    //}
                }
           // }
        }

        List<int> hpfHeights = new List<int>();

        public void RefreshTilesetHpf()
        {
            if (map == null)
            {
                return;
            }
            lock (this)
            {
                hpfHeights.Clear();
                int ydraw = 0;
                int index = tileset_index;
                /*pictureBox2.Image*/
                Bitmap b = new Bitmap(168, pictureBox2.Height);
                Graphics g = Graphics.FromImage(b);
                while (ydraw < pictureBox2.Height)
                {
                    int ymax = 20;
                    for (int i = 0; i < 6; i++)
                    {
                        int index2 = index + i;
                        Bitmap wall = null;
                        int height = 0;
                        if (!Capricorn.Drawing.DAGraphics.cachedWalls.ContainsKey(index2))
                        {
                            if (index2 == 19583)
                            {
                                int fff = 0;
                            }
                            Capricorn.Drawing.HPFImage hpf = Capricorn.Drawing.HPFImage.FromArchive("stc" + index2.ToString().PadLeft(5, '0') + ".hpf", true, iadat);
                            if (hpf != null)
                            {
                                height = hpf.Height;
                                wall = Capricorn.Drawing.DAGraphics.RenderImage(hpf, fgtable[index2 + 1]);
                                Capricorn.Drawing.DAGraphics.cachedWalls.Add(index2, wall);
                            }
                        }
                        else
                        {
                            wall = Capricorn.Drawing.DAGraphics.cachedWalls[index2];
                            height = wall.Height;
                        }
                        Point p = new Point(28 * i, ydraw);
                        if (wall != null)
                        {
                            g.DrawImageUnscaled(wall, p);
                        }
                        if (height > ymax)
                            ymax = height;

                        if (Capricorn.Drawing.DAGraphics.editMode == 1)
                        {
                            if (Capricorn.Drawing.DAGraphics.selection != null && Capricorn.Drawing.DAGraphics.selection.Length == 1 && index2 == Capricorn.Drawing.DAGraphics.selection[0].LeftWall)
                            {
                                g.DrawRectangle(new Pen(Color.Red), 28 * i, ydraw + 1, 27, height);
                            }
                        }

                        if (Capricorn.Drawing.DAGraphics.editMode == 2)
                        {
                            if (Capricorn.Drawing.DAGraphics.selection != null && Capricorn.Drawing.DAGraphics.selection.Length == 1 && index2 == Capricorn.Drawing.DAGraphics.selection[0].RightWall)
                            {
                                g.DrawRectangle(new Pen(Color.Red), 28 * i, ydraw + 1, 27, height);
                            }
                        }

                    }
                    g.DrawLine(new Pen(Color.Black), new Point(0, ydraw), new Point(168, ydraw));
                    ydraw += ymax;
                    hpfHeights.Add(ymax);
                    index += 6;
                }
                pictureBox2.Image = b;
                pictureBox2.Refresh();
                System.GC.Collect();
            }
        }

        public void RefreshTilesetBg()
        {
            if (map == null)
            {
                return;
            }

            int ydraw = 0;
            int index = tileset_index;
            /*pictureBox2.Image*/
            Bitmap b = new Bitmap(168, pictureBox2.Height);
            Graphics g = Graphics.FromImage(b);
            Capricorn.Drawing.Tileset t = tiles;
            if (snowy)
                t = tileas;
            while (ydraw < pictureBox2.Height)
            {
                //g.DrawLine(new Pen(Color.Black), new Point(0, ydraw), new Point(168, ydraw));
                for (int i = 0; i < 3; i++)
                {
                    int index2 = index + i;
                    if (index2 >= 0 && index2 < t.TileCount)
                    {
                        //Capricorn.Drawing.HPFImage hpf = Capricorn.Drawing.HPFImage.FromArchive("stc" + index2.ToString().PadLeft(5, '0') + ".hpf", true, iadat);
                        Bitmap floorTile = Capricorn.Drawing.DAGraphics.RenderTile(t[index2], bgtable[index2 + 2]);
                        //Bitmap wall = Capricorn.Drawing.DAGraphics.RenderImage(hpf, fgtable[index + 1]);
                        Point p = new Point(56 * i, ydraw);
                        g.DrawImageUnscaled(floorTile, p);
                        if (Capricorn.Drawing.DAGraphics.selection != null && Capricorn.Drawing.DAGraphics.selection.Length == 1 && index2 + 1 == Capricorn.Drawing.DAGraphics.selection[0].FloorTile)
                        {
                            g.DrawRectangle(new Pen(Color.Red), 56 * i + 1, ydraw, 54, 26);
                        }
                        g.DrawRectangle(new Pen(Color.Black), 56 * i, ydraw - 1, 56, 28);
                    }
                }
                ydraw += 28;
                index += 3;
            }
            pictureBox2.Image = b;
            pictureBox2.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Capricorn.Drawing.DAGraphics.editMode != 0)
            {
                tileset_index+=6;
                RefreshTilesetHpf();
            }
            else
            {
                tileset_index+=3;
                RefreshTilesetBg();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Capricorn.Drawing.DAGraphics.editMode != 0)
            {
                tileset_index -= 6;
                if (tileset_index < 0)
                    tileset_index = 0;
                RefreshTilesetHpf();
            }
            else
            {
                tileset_index -= 3;
                if (tileset_index < 0)
                    tileset_index = 0;
                RefreshTilesetBg();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //fgedit = false;
            //tileset_index = 0;
            //RefreshTilesetBg();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            //fgedit = true;
            //tileset_index = 0;
            //RefreshTilesetHpf();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Capricorn.Drawing.DAGraphics.editMode != 0)
            {
                tileset_index -= 6 * hpfHeights.Count;
            }
            else
            {
                tileset_index -= 3 * (pictureBox2.Height / 28);
            }
            if (tileset_index < 0)
                tileset_index = 0;
            if (Capricorn.Drawing.DAGraphics.editMode != 0)
                RefreshTilesetHpf();
            else
                RefreshTilesetBg();
  
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Capricorn.Drawing.DAGraphics.editMode != 0)
                tileset_index += 6 * hpfHeights.Count;
            else
                tileset_index += 3 * (pictureBox2.Height / 28);
            if (Capricorn.Drawing.DAGraphics.editMode != 0)
                RefreshTilesetHpf();
            else
                RefreshTilesetBg();
        }
        private void pictureBox5_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            int totalcolumns = map.Width + map.Height - 1;

            double vssize = ((double)pictureBox1.Height) / (totalcolumns * 14);
            int vp = (int)(vssize * pictureBox1.Height);
            int vtotal = pictureBox1.Height - vp;
            double vpercent = ((double)Capricorn.Drawing.DAGraphics.vscroll / 14.0) / (totalcolumns + 1);
            int centery = (int)(vp / 2 + vpercent * vtotal);

            int x = (int)(((e.X - (pictureBox1.Width / 2)) / ((double)pictureBox1.Width)) * 28 * (totalcolumns + 1));
            int y = centery;//(int)((scrolly / 480.0) * 14 * (totalcolumns + 1));
            x /= 56;
            x *= 56;
            y /= 28;
            y *= 28;
            Capricorn.Drawing.DAGraphics.hscroll = x;// -56 * (totalcolumns / 2); ;
            //Capricorn.Drawing.DAGraphics.vscroll = y;
            RefreshMap();
        }
        private void pictureBox6_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            int totalcolumns = map.Width + map.Height - 1;
            double hssize = ((double)pictureBox1.Width) / (totalcolumns * 28);
            int hp = (int)(hssize * pictureBox1.Width);
            int htotal = pictureBox1.Width - hp;
            double hpercent = ((double)(Capricorn.Drawing.DAGraphics.hscroll/*+(totalcolumns * 28)*/) / 56.0) / (totalcolumns + 1);
            int centerx = (int)(hp / 2 + hpercent * htotal) + pictureBox1.Width / 2;

            int x = centerx;//(int)((scrollx / 560.0) * 56 * (totalcolumns + 1));
            int y = (int)(((e.Y / (double)pictureBox1.Height)) * 14 * (totalcolumns + 1)) - 56 * 3;
            x /= 56;
            x *= 56;
            y /= 28;
            y *= 28;
            //Capricorn.Drawing.DAGraphics.hscroll = x;// -56 * (totalcolumns / 2);
            Capricorn.Drawing.DAGraphics.vscroll = y;
            RefreshMap();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.clickmode = 0;
            button10.BackColor = Color.DarkBlue;
            button9.BackColor = System.Drawing.SystemColors.Control;
            button11.BackColor = System.Drawing.SystemColors.Control;
            button23.BackColor = System.Drawing.SystemColors.Control;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.clickmode = 1;
            button10.BackColor = System.Drawing.SystemColors.Control;
            button9.BackColor = Color.DarkBlue;
            button11.BackColor = System.Drawing.SystemColors.Control;
            button23.BackColor = System.Drawing.SystemColors.Control;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.clickmode = 2;
            button10.BackColor = System.Drawing.SystemColors.Control;
            button9.BackColor = System.Drawing.SystemColors.Control;
            button23.BackColor = System.Drawing.SystemColors.Control;
            button11.BackColor = Color.DarkBlue;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = listBox1.SelectedIndex;
            if (i >= listBox1.Items.Count || i < 0)
                return;
            map.Width = (int)widths[i];
            map.Height = (int)heights[i];
            RefreshMap();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (map == null)
                return;

            if (newFile)
            {
                saveAsToolStripMenuItem_Click(sender, e);
                return;
            }
            //string s = map.Name;
            //SaveFileDialog of = new SaveFileDialog();
            //DialogResult ok = of.ShowDialog();
            //if (ok == DialogResult.OK)
            //{
                StreamWriter w = new StreamWriter(new FileStream(/*of.FileName*/map.Name,FileMode.OpenOrCreate));
                for (int i = 0; i < map.Tiles.Length; i++)
                {
                    w.BaseStream.WriteByte((byte)(map.Tiles[i].FloorTile % 256));
                    w.BaseStream.WriteByte((byte)((map.Tiles[i].FloorTile>>8) % 256));
                    w.BaseStream.WriteByte((byte)(map.Tiles[i].LeftWall % 256));
                    w.BaseStream.WriteByte((byte)((map.Tiles[i].LeftWall >> 8) % 256));
                    w.BaseStream.WriteByte((byte)(map.Tiles[i].RightWall % 256));
                    w.BaseStream.WriteByte((byte)((map.Tiles[i].RightWall >> 8) % 256));

                }
                    //of.FileName
                w.Close();
            //}
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = darkagesPath;
            fbd.ShowDialog();
            darkagesPath = fbd.SelectedPath;
            checkPathsExist();
            WriteConfigXml();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.bgShow = !Capricorn.Drawing.DAGraphics.bgShow;
            button14.BackColor = Capricorn.Drawing.DAGraphics.bgShow ? Color.DarkBlue : System.Drawing.SystemColors.Control;
            RefreshMap();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.fg1Show = !Capricorn.Drawing.DAGraphics.fg1Show;
            button15.BackColor = Capricorn.Drawing.DAGraphics.fg1Show ? Color.DarkBlue : System.Drawing.SystemColors.Control;
            RefreshMap();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.fg2Show = !Capricorn.Drawing.DAGraphics.fg2Show;
            button16.BackColor = Capricorn.Drawing.DAGraphics.fg2Show ? Color.DarkBlue : System.Drawing.SystemColors.Control;
            RefreshMap();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (Capricorn.Drawing.DAGraphics.editMode != 0)
            {
                tileset_index = 0;
            }
            Capricorn.Drawing.DAGraphics.editMode = 0;
            button17.BackColor = Color.DarkRed;
            button18.BackColor = System.Drawing.SystemColors.Control;
            button19.BackColor = System.Drawing.SystemColors.Control;
            button20.BackColor = System.Drawing.SystemColors.Control;
            button21.BackColor = System.Drawing.SystemColors.Control;
            tabControl1.SelectedIndex = 0;
            RefreshTilesetBg();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (Capricorn.Drawing.DAGraphics.editMode == 0)
            {
                tileset_index = 0;
            }
            Capricorn.Drawing.DAGraphics.editMode = 1;
            button18.BackColor = Color.DarkRed;
            button17.BackColor = System.Drawing.SystemColors.Control;
            button19.BackColor = System.Drawing.SystemColors.Control;
            button20.BackColor = System.Drawing.SystemColors.Control;
            button21.BackColor = System.Drawing.SystemColors.Control;
            tabControl1.SelectedIndex = 0;
            RefreshTilesetHpf();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (Capricorn.Drawing.DAGraphics.editMode == 0)
            {
                tileset_index = 0;
            }
            Capricorn.Drawing.DAGraphics.editMode = 2;
            button19.BackColor = Color.DarkRed;
            button17.BackColor = System.Drawing.SystemColors.Control;
            button18.BackColor = System.Drawing.SystemColors.Control;
            button20.BackColor = System.Drawing.SystemColors.Control;
            button21.BackColor = System.Drawing.SystemColors.Control;
            tabControl1.SelectedIndex = 0;
            RefreshTilesetHpf();
        }

        private void button20_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.editMode = 3;
            button17.BackColor = System.Drawing.SystemColors.Control;
            button18.BackColor = System.Drawing.SystemColors.Control;
            button19.BackColor = System.Drawing.SystemColors.Control;
            button21.BackColor = System.Drawing.SystemColors.Control;
            tabControl1.SelectedIndex = 2;
            button20.BackColor = Color.DarkRed;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.editMode = 4;
            button17.BackColor = System.Drawing.SystemColors.Control;
            button18.BackColor = System.Drawing.SystemColors.Control;
            button19.BackColor = System.Drawing.SystemColors.Control;
            button20.BackColor = System.Drawing.SystemColors.Control;
            tabControl1.SelectedIndex = 2;
            button21.BackColor = Color.DarkRed;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.drawEmptyWalls = checkBox1.Checked;
        }

        private void button22_Click(object sender, EventArgs e)
        {
            if(map == null)
                return;
            int totalcolumns = map.Width + map.Height - 1;
            double vssize =  (totalcolumns * 14);
            double hssize = (totalcolumns * 14);

            Capricorn.Drawing.DAGraphics.vscroll = 0;// pictureBox1.Height / 2;// (int)(vssize / 2 - pictureBox1.Height / 2);
            Capricorn.Drawing.DAGraphics.hscroll = 0;// (int)(hssize / 2);
            RefreshMap();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.tabMap = checkBox2.Checked;
            RefreshMap();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (map == null)
                return;

            //string s = map.Name;
            SaveFileDialog of = new SaveFileDialog();
            DialogResult ok = of.ShowDialog();
            if (ok == DialogResult.OK)
            {
            StreamWriter w = new StreamWriter(new FileStream(of.FileName, FileMode.OpenOrCreate));
            for (int i = 0; i < map.Tiles.Length; i++)
            {
                w.BaseStream.WriteByte((byte)(map.Tiles[i].FloorTile % 256));
                w.BaseStream.WriteByte((byte)((map.Tiles[i].FloorTile >> 8) % 256));
                w.BaseStream.WriteByte((byte)(map.Tiles[i].LeftWall % 256));
                w.BaseStream.WriteByte((byte)((map.Tiles[i].LeftWall >> 8) % 256));
                w.BaseStream.WriteByte((byte)(map.Tiles[i].RightWall % 256));
                w.BaseStream.WriteByte((byte)((map.Tiles[i].RightWall >> 8) % 256));

            }
            //of.FileName
            w.Close();

            newFile = false;
            map.Name = of.FileName;
            string smallFn = map.Name.Substring(1 + map.Name.LastIndexOfAny(new char[] { '\\', '/' }));
            tabControl2.TabPages[selectedTabIndex].Text = smallFn;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewMap f = new NewMap();
            f.ShowDialog();
            int w = f.width;
            int h = f.height;
            //MessageBox.Show("w = " + w + " h = " + h);
            string tempDir = Environment.GetEnvironmentVariable("TEMP");
            if (!tempDir.EndsWith("\\"))
            {
                tempDir = tempDir + "\\";
            }

            string mapFile = tempDir + "lod55555.map";
            byte[] data = new byte[h * w * 6];
            for(int i=0; i < h * w * 6; i++)
            {
                if ((i % 6) == 0)
                    data[i] = 1;
                else
                    data[i] = 0;
            }
            BinaryWriter bw = new BinaryWriter(new FileStream(mapFile, FileMode.Create));
            bw.Write(data);
            bw.Close();
            openMap(mapFile, true);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.transparency = checkBox3.Checked;
            RefreshMap();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            Capricorn.Drawing.DAGraphics.clickmode = 3;
            button10.BackColor = System.Drawing.SystemColors.Control;
            button9.BackColor = System.Drawing.SystemColors.Control;
            button11.BackColor = System.Drawing.SystemColors.Control;
            button23.BackColor = Color.DarkBlue;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            snowy = checkBox4.Checked;
            RefreshMap();
            if (Capricorn.Drawing.DAGraphics.editMode == 0)
            {
                RefreshTilesetBg();
            }
            if (Capricorn.Drawing.DAGraphics.editMode == 1 || Capricorn.Drawing.DAGraphics.editMode == 2)
            {
                RefreshTilesetHpf();
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeMap();
        }

        private void button24_Click(object sender, EventArgs e)
        {
            if (map == null)
                return;
            if (undoBuffer.Count > 0)
            {
                UndoItem undo = undoBuffer[undoBuffer.Count - 1];
                int width = undo.width;
                int height = undo.bgData.Length / width;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (undo.bg)
                        {
                            map.Tiles[(undo.y + y) * map.Width + undo.x + x].FloorTile = undo.bgData[y * width + x];
                        }
                        if (undo.fgl)
                        {
                            map.Tiles[(undo.y + y) * map.Width + undo.x + x].LeftWall = undo.fglData[y * width + x];
                        }
                        if (undo.fgr)
                        {
                            map.Tiles[(undo.y + y) * map.Width + undo.x + x].RightWall = undo.fgrData[y * width + x];
                        }
                    }
                }
                undoBuffer.RemoveAt(undoBuffer.Count - 1);
            }

            //if (undoBuffer.Count == 0)
                //button24.Enabled = false;
            RefreshMap();
        }

    }
}