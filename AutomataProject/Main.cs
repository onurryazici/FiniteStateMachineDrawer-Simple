using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Xml;
using System.Diagnostics;

namespace AutomataProject
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }
        #region graphDrawer
        public class Node
        {
            public string Text { get; set; }
            public List<Node> PrevNodes { get; set; }
            public List<Node> NextNodes { get; set; }
            public float NodeX { get; set; }
            public float NodeY { get; set; }
            public string nodeString;
            public string prevNodeNames;
            public string selfLoopString;
            public string targetLoopPathString;
            public string targetLoopNodeName;
            public bool isFinal;
            public Node(string nodeName, string prevNodeNames, string pathStrings, string selfLoopPathString, string targetLoopPathString, string targetLoopNodeName, bool isFinal)
            {
                this.prevNodeNames = prevNodeNames;
                this.nodeString = pathStrings;
                this.selfLoopString = selfLoopPathString;
                this.targetLoopPathString = targetLoopPathString;
                this.targetLoopNodeName = targetLoopNodeName;
                this.isFinal = isFinal;
                PrevNodes = new List<Node>();
                NextNodes = new List<Node>();

                Text = nodeName;
                NodeX = -1;
                NodeY = -1;
            }

            public void Draw(Graphics g, float scale, float size)
            {
                RectangleF rf = new RectangleF(NodeX * scale, NodeY * scale, size, size);
                if (isFinal)
                {
                    // Double Circle drawing for final states
                    g.FillEllipse(Brushes.Beige, rf);
                    RectangleF finalRF = new RectangleF((NodeX * scale) + 4 , (NodeY * scale ) + 4, size - 7 , size-7 );
                    g.DrawEllipse(Pens.Black, rf);
                    g.DrawEllipse(Pens.Black, finalRF);
                }
                else
                {
                    g.FillEllipse(Brushes.Beige, rf);
                    g.DrawEllipse(Pens.Blue, rf);
                }
               
                using (StringFormat fmt = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                using (Font f = new Font("Consolas", 20f))
                    g.DrawString(Text, f, Brushes.Blue, rf, fmt);

                foreach (var nn in NextNodes)
                {
                    AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
                    Pen pen = new Pen(Color.Black, 2f);
                    pen.CustomEndCap = bigArrow;

                    /////////////////////////////////////////////////////////////////////////////
                    /////////////////////////////////////////////////////////////////////////////
                    //////////////////// This stage written from me /////////////////////////////
                    /////////////////////////////////////////////////////////////////////////////
                    /////////////////////////////////////////////////////////////////////////////
                    int objectX = Convert.ToInt16(GetConnector(this, scale, false, size).X);
                    int objectY = Convert.ToInt16(GetConnector(this, scale, false, size).Y);
                    int targetX = Convert.ToInt16(GetConnector(nn, scale, true, size).X - 20);
                    int targetY = Convert.ToInt16(GetConnector(nn, scale, true, size).Y);

                    PointF p2 = new PointF(objectX + 10, objectY + 100);
                    PointF p3 = new PointF(objectX - 50, objectY + 20);
                    PointF p4 = new PointF(objectX - 30, objectY + 20);
                    if (selfLoopString != string.Empty)
                    {
                        PointF textPoint = p4;
                        textPoint.X += 10;
                        g.DrawBezier(pen,
                        GetConnector(this, scale, false, size), p2, p3, p4
                        );
                        g.DrawString(selfLoopString, new Font("Tahoma", 12, FontStyle.Bold), Brushes.Black,
                        textPoint);
                    }

                    if (targetLoopPathString != string.Empty)
                    {
                        if (nn.PrevNodes.Count > 0)
                        {
                            var temp = nn.PrevNodes[0];
                            while (temp.Text != targetLoopNodeName)
                            {
                                if (temp.PrevNodes.Count != 0)
                                    temp = temp.PrevNodes[0];
                                else
                                    break;
                            }

                            if (temp.Text != "")
                            {
                                PointF pp1 = GetConnector(this, scale, false, size);
                                PointF pp2 = new PointF(objectX + 10, objectY + 70);
                                PointF pp3 = new PointF(objectX - 160, objectY + 20);
                                PointF pp4 = GetConnector(temp, scale, false, size);
                                if (nn.NodeY == 0)
                                {
                                    pp1 = new PointF(objectX + 60, objectY);
                                    pp2 = new PointF(objectX + 10, objectY + 70);
                                    pp3 = new PointF(objectX - 160, objectY + 20);
                                }
                                else if (nn.NodeY == 1)
                                {
                                    pp1 = new PointF(objectX + 60, objectY);
                                    pp2 = new PointF(objectX + 10, objectY + 70);
                                    pp3 = new PointF(objectX - 160, objectY + 80);
                                }
                                else
                                {
                                    pp1 = new PointF(objectX + 60, objectY);
                                    pp2 = new PointF(objectX + 10, objectY + 70);
                                    pp3 = new PointF(objectX - 160, objectY + 180);
                                }
                                PointF textPoint = GetConnector(this, scale, false, size);
                                textPoint = new PointF(textPoint.X + 40, textPoint.Y + 20);
                                g.DrawBezier(pen,
                                    pp1, pp2, pp3, pp4
                                    );
                                g.DrawString(targetLoopPathString, new Font("Tahoma", 10, FontStyle.Bold), Brushes.Black,
                                textPoint);
                            }
                        }
                    }
                    g.DrawString(nodeString, new Font("Tahoma", 10, FontStyle.Bold), Brushes.Black,
                        targetX, targetY);
                    g.DrawLine(pen, GetConnector(this, scale, false, size), GetConnector(nn, scale, true, size));
                }
            }

            PointF GetConnector(Node n, float scale, bool left, float size)
            {
                RectangleF r = new RectangleF(n.NodeX * scale, n.NodeY * scale, size, size);
                float x = left ? r.Left : r.Right;
                float y = r.Top + r.Height / 2;
                return new PointF(x, y);
            }
        }

        public class NodeChart
        {
            public List<Node> TheNodes { get; set; }
            public List<Node> StartNodes { get; set; }
            public NodeChart()
            {
                TheNodes = new List<Node>();
                StartNodes = new List<Node>();
            }

            public void FillPrevNodes()
            {
                foreach (var n in TheNodes)
                {
                    var pn = n.prevNodeNames.Split(',');
                    foreach (var p in pn)
                    {
                        var hit = TheNodes.Where(x => x.Text == p);
                        if (hit.Count() == 1)
                            n.PrevNodes.Add(hit.First());
                        else if (hit.Count() == 0)
                            StartNodes.Add(n);
                        else
                            Console.WriteLine(n.Text + ": prevNodeName '" + p + "' not found for unique!");
                    }
                }
            }

            public void FillNextNodes()
            {
                foreach (var n in TheNodes)
                {
                    foreach (var pn in n.PrevNodes)
                    {
                        pn.NextNodes.Add(n);
                    }
                }
            }

            public void LayoutNodeX()
            {
                foreach (Node n in StartNodes) LayoutNodeX(n, n.NodeX + 1);
            }

            public void LayoutNodeX(Node n, float vx)
            {
                n.NodeX = vx;
                foreach (Node nn in n.NextNodes) LayoutNodeX(nn, vx + 1);
            }

            public void LayoutNodeY()
            {
                Node n1 = StartNodes.First();
                n1.NodeY = 0;
                Dictionary<float, List<Node>> nodes =
                                  new Dictionary<float, List<Node>>();

                foreach (var n in TheNodes)
                {
                    if (nodes.Keys.Contains(n.NodeX)) nodes[n.NodeX].Add(n);
                    else nodes.Add(n.NodeX, new List<Node>() { n });
                }

                for (int i = 0; i < nodes.Count; i++)
                {
                    int c = nodes[i].Count;
                    for (int j = 0; j < c; j++)
                    {
                        nodes.Values.ElementAt(i)[j].NodeY = 1f * j - c / 2;
                    }
                }

                float min = TheNodes.Select(x => x.NodeY).Min();
                foreach (var n in TheNodes) n.NodeY -= min;
            }
        }
        #endregion

        
        public LinkedNodeList list;
        public NodeChart NCG;
        public class SpecialNode
        {
            public List<SpecialNode> next;
            public SpecialNode prev;
            public string path;
            public string name;
            public string selfLoopPS;
            public string targetLoopPS;
            public string targetLoopName;
            public bool isFinal;
        }
        public class LinkedNodeList
        {
            public SpecialNode head = null;
            public SpecialNode current = null;
            public int Yrow = 0;
            public string[] parents = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L","M","N","O","P","R","S","T","U","V","Y" };
            public int parentIndex = 0;
            public int currentRow = 0;
            public NodeChart NCGG;
            public List<string> lastStates;
            public LinkedNodeList()
            {
                head = new SpecialNode();
                head.prev = new SpecialNode();
                head.prev.name = "";
                head.path = "€";
                head.name = parents[parentIndex++];
                head.selfLoopPS = string.Empty;
                head.targetLoopPS = string.Empty;
                head.targetLoopName = string.Empty;
                head.next = new List<SpecialNode>();
                NCGG = new NodeChart();
                lastStates = new List<string>();
                current = head;
            }

            public void AddChild(SpecialNode n,bool gotoNext)
            {
                if(gotoNext)
                {
                    current.next.Add(n);
                    if(current==head)
                    {
                        current = current.next[Yrow];
                    }
                    else
                    {
                        current = current.next[0];
                    }
                    current.next = new List<SpecialNode>();
                }
                else
                {
                    current.next.Add(n);
                    if (current == head)
                    {
                        current = current.next[Yrow];
                    }
                    else
                    {
                        current = current.next[0];
                    }

                    SpecialNode garbageNode = new SpecialNode();
                    garbageNode.name = parents[parentIndex++];
                    garbageNode.next = null;
                    garbageNode.prev = current;
                    garbageNode.path = "€";
                    garbageNode.targetLoopName = string.Empty;
                    garbageNode.targetLoopPS = string.Empty;
                    garbageNode.selfLoopPS = "";
                    garbageNode.isFinal = false;
                    current.next = new List<SpecialNode>();
                    current.next.Add(garbageNode);
                    lastStates.Add(garbageNode.name);
                }
            }
            public void SynchroniseToRenderLibrary()
            {
                SpecialNode temp = head;
                while (temp.next != null)
                {
                        NCGG.TheNodes.Add(new Node(
                            temp.name, 
                            temp.prev.name, 
                            temp.path.ToString(),
                            temp.selfLoopPS,
                            temp.targetLoopPS,
                            temp.targetLoopName,
                            temp.isFinal
                            ));

                    //MessageBox.Show(
                    //    "\"" + temp.name + "\"," +
                    //    "\"" + temp.prev.name + "\"," +
                    //    "\"" + temp.path.ToString() + "\"," +
                    //    "\"" + temp.selfLoopPS + "\"," +
                    //    "\"" + temp.targetLoopPS + "\"," +
                    //    "\"" + temp.targetLoopName + "\" 1");

                    if (temp.next != null)
                    {
                        temp = temp.next[0];
                        if(temp.next==null)
                        {
                            currentRow++;
                            NCGG.TheNodes.Add(new Node(
                            temp.name,
                            temp.prev.name,
                            temp.path.ToString(),
                            temp.selfLoopPS,
                            temp.targetLoopPS,
                            temp.targetLoopName,
                            temp.isFinal
                            ));

                        //    MessageBox.Show(
                        //"\"" + temp.name + "\"," +
                        //"\"" + temp.prev.name + "\"," +
                        //"\"" + temp.path.ToString() + "\"," +
                        //"\"" + temp.selfLoopPS + "\"," +
                        //"\"" + temp.targetLoopPS + "\"," +
                        //"\"" + temp.targetLoopName + "\" 2");

                            if (currentRow <= Yrow)
                            {
                                temp = head;
                                temp = temp.next[currentRow];
                            }
                            else
                            {
                                break;
                            }
                        }
                       
                    }
                }
                string states = "";
                for (int i = 0; i < lastStates.Count; i++)
                {
                    states += lastStates[i].ToString() + ",";
                }
                int length = states.Length;
                if (states[length - 1].ToString() == ",")
                {
                    states = states.Remove(length-1);
                }
                 NCGG.TheNodes.Add(new Node(
                            "Z",
                            states,
                            "€",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            true
                            ));
                //MessageBox.Show(
                //        "\"" + "Z" + "\"," +
                //        "\"" + states + "\"," +
                //        "\"" + "€" + "\"," +
                //        "\"" + string.Empty+ "\"," +
                //        "\"" + string.Empty + "\"," +
                //        "\"" + string.Empty + "\" 2");
            }
            
        }

        public void ParseRegularExpression(string regularExp)
        {
            int length = regularExp.Length;

            for (int i = 0; i < length; i++)
            {
                // a
                if (Regex.IsMatch(regularExp[i].ToString(), @"^[a-zA-Z]+$"))
                {
                    SpecialNode newNode = new SpecialNode();
                    newNode.name = list.parents[list.parentIndex++];
                    newNode.next = null;
                    newNode.prev = list.current;
                    newNode.path = regularExp[i].ToString();
                    newNode.targetLoopName = string.Empty;
                    newNode.targetLoopPS = string.Empty;
                    newNode.isFinal = false;
                    bool gotoNext = false;
                    bool selfLoopDetected = false;
                    if (i < length - 1)
                    {
                        // is self loop ? e.g : b*
                        if (regularExp[i + 1].ToString() == "*")
                        {
                            if (i < length - 2)
                            {
                                if(Regex.IsMatch(regularExp[i + 2].ToString(), @"^[a-zA-Z]+$"))
                                {
                                    gotoNext = true;
                                    selfLoopDetected = true;
                                }
                            }
                            newNode.selfLoopPS = regularExp[i].ToString();
                            newNode.path = "€";
                        }
                        else
                        {
                            newNode.selfLoopPS = string.Empty;
                        }

                        // is it have neighbour ?  e.g : ab
                        if(selfLoopDetected)
                        {
                            list.AddChild(newNode, gotoNext);
                        }
                        else
                        {
                            if (Regex.IsMatch(regularExp[i + 1].ToString(), @"^[a-zA-Z]+$"))
                            {
                                gotoNext = true;
                            }
                            else
                            {
                                gotoNext = false;
                            }
                            list.AddChild(newNode, gotoNext);
                        }

                    }
                    else
                    {
                        newNode.selfLoopPS = string.Empty;
                        list.AddChild(newNode, false);
                    }
                }
                if (regularExp[i].ToString() == "+")
                {
                    list.current = list.head;
                    list.Yrow++;
                }

            }

        }
        private void DrawButton_Click(object sender, EventArgs e)
        {
            try
            {

                string regularExpString = regularExpTextbox.Text;
                regularExpString = regularExpString.Replace(" ", "");
                if(regularExpString == "")
                {
                    // nothing :)
                }
                else
                {
                    list = new LinkedNodeList();
                    ParseRegularExpression(regularExpString);
                    list.SynchroniseToRenderLibrary();
                    RenderENFA();
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("RE çözümlenirken bir hata oluştu!\n\n" + ex.ToString(), "Hata", MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void RenderENFA()
        {
            list.NCGG.FillPrevNodes();
            list.NCGG.FillNextNodes();
            list.NCGG.LayoutNodeX();
            list.NCGG.LayoutNodeY();
            pictureBox.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel.Text = "Waiting";
            this.MinimumSize = new Size(850,300);

        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (list == null) return;
            else if (list.NCGG.TheNodes.Count <= 0) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (var n in list.NCGG.TheNodes) n.Draw(e.Graphics, 80, 40);
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel.Text = " X : " + MousePosition.X.ToString() + " Y : " + MousePosition.Y.ToString();
        }
    

        private void clearButton_Click(object sender, EventArgs e)
        {
            regularExpTextbox.Clear();
            list.NCGG = new NodeChart();
            pictureBox.Invalidate();
        }

        private void forkButton_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("http://github.com/onurryazici");
            Process.Start(sInfo);
        }
    }
}
