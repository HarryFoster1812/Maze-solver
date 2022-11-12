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
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Windows.Threading;

// Extention ideas make the ui look better (boring)
// Add an option for the user to select from pre generated mazes
// Allow the player to solve the maze by hand like an absolute idiot (maybe add flags so that they know not to go back so mabe different colours)

// Make a formula that changes the size of each rectangle based off the dimensions of the maze

namespace Maze_solver_sheet_5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public static class file_handler
    {
        public static int[] endco = new int[2];

        public static int[] startco = new int[2];

        public static char[,] readfile(string file1)
        {
            string[] filecontent = System.IO.File.ReadAllLines(file1);

            char[,] maze = new char[filecontent.GetLength(0), filecontent[0].Length];

            for (int i = 0; i < filecontent.GetLength(0); i++) {
                for (int j = 0; j < filecontent[i].Length; j++) {
                    maze[i,j] = (char)filecontent[i][j];
                }
            }
            return maze;
        }
    }
    struct node {


        public int x;
        public int y;
        public int value;
        public int fcost;
        public int gcost;
        public int hcost;
        public int[] parent;
        public bool start;
        public bool end;
        public node(int x, int y, int value , int gcost, int hcost, int[] parent, bool start, bool end)
        {
            this.x = x;
            this.y = y;
            this.value= value;
            this.start = start;
            this.end = end;
            this.hcost = hcost;
            this.gcost = gcost;
            this.fcost = gcost + hcost;
            this.parent = parent;

        }
    }
  
    public partial class MainWindow : Window
    {
        List<node> nodes = new List<node>();
        List<node> openset = new List<node>();
        List<node> closedset = new List<node>();
        node current;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        
        TextBox timeDelay = new TextBox();

        static Canvas Maze = new Canvas();
        static char[,] MazeAsText;
        const string mazeFile = "C:\\Users\\harry\\Documents\\Programming\\C#\\Maze solver sheet 5\\Maze solver sheet 5\\Mazes\\";

        static string[] dimensions;
        static int[] userloc = new int[2]; 
        static int height;
        static int width;
        ComboBox mazePicker;
        public bool solved = false;

        static Border[,] MazeGrid;

        public MainWindow()
        {
            InitializeComponent();
            initCanvas();
            loadmaze("20x20(0)");
            loadbuttons();
            makeItLookNice();
            dispatcherTimer.Tick += new EventHandler(solve);
        }

        void initCanvas() {
            Viewbox dynamicViewbox = new Viewbox();
            // Set StretchDirection and Stretch properties  
            dynamicViewbox.StretchDirection = StretchDirection.Both;
            dynamicViewbox.Stretch = Stretch.Fill;

            Maze.Height = RootWindow.Height;

            Maze.Width = RootWindow.Width;

            Maze.Background = Brushes.Black;


            RootWindow.Content = dynamicViewbox;
            dynamicViewbox.Child = Maze;
            RootWindow.Arrange(new Rect(0, 0, Width, Height));
            MazeGrid = new Border[width, height];


        }

        void resetmazegrid() {
            nodes.Clear();
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)

                {
                    Maze.Children.Remove(MazeGrid[row, col]);
                }

            }
        }

        void loadmaze(string maze) {
            

            dimensions = System.IO.File.ReadAllLines(mazeFile + maze + ".txt");
            height = dimensions.GetLength(0);
            width = dimensions[0].Length;
            // Create 400 rectangles to be squares in maze
            MazeGrid = new Border[width, height];

            MazeAsText = file_handler.readfile(mazeFile+maze+".txt");

            
            // Set the default properties of the 400 rectangles and put them

            // onto the canvas

            for (int row = 0; row < height; row++) { 

                for (int col = 0; col < width; col++)

                {

                    MazeGrid[row, col] = new Border();

                    MazeGrid[row, col].Height = Math.Floor((double)380/width);

                    MazeGrid[row, col].Width = Math.Floor((double)380 / width);
                    if (MazeAsText[row,col] == '#') {
                        MazeGrid[row, col].Background = Brushes.DarkGray;
                        nodes.Add(new node(row, col, -1, 0, 0, new int[2],false, false));
                    }
                    else if (MazeAsText[row, col] == 'S')
                    {
                        MazeGrid[row, col].Background = Brushes.Yellow;
                        nodes.Add(new node(col, row, 1, 0, 0, new int[]{ -1,-1} , true, false));
                        file_handler.startco[0] = col;
                        file_handler.startco[1] = row;
                        userloc[0] = col;
                        userloc[1] = row;

                    }
                    else if (MazeAsText[row, col] == 'E')
                    {
                        MazeGrid[row, col].Background = Brushes.DeepPink;
                        nodes.Add(new node(col, row, 1, 0, 0,new int[2] ,false, true));
                            file_handler.endco[0] = col;
                            file_handler.endco[1] = row;
                    }
                    else
                    {
                        MazeGrid[row, col].Background = Brushes.Black;
                        nodes.Add(new node(col, row, 1, 0, 0,new int[2], false, false));
                    }

                    MazeGrid[row, col].BorderBrush = Brushes.Black;

                    // Put the rectangle onto the Maze canvas

                    Canvas.SetTop(MazeGrid[row, col], row * MazeGrid[row, col].Height);

                    Canvas.SetLeft(MazeGrid[row, col], col * MazeGrid[row, col].Width);

                    Maze.Children.Add(MazeGrid[row, col]);


                }
            }
        }

        void makeItLookNice() {
            TextBlock title = new TextBlock();
            title.Text = "Welcome to the maze solver!";
            title.Foreground = Brushes.White;
            title.FontSize = 30;
            Canvas.SetTop(title, 0);
            Canvas.SetLeft(title, (width * MazeGrid[0, 0].Width) + 25);
            Maze.Children.Add(title);

            TextBlock size = new TextBlock();
            size.Text = "Size:";
            size.Foreground = Brushes.White;
            size.FontSize = 20;
            Canvas.SetTop(size, MazeGrid[0, 0].Height * 3);
            Canvas.SetLeft(size, (width * MazeGrid[0, 0].Width) + 55);
            Maze.Children.Add(size);

            TextBlock key = new TextBlock();
            key.Text = "Key:";
            key.Foreground = Brushes.White;
            key.FontSize = 20;
            Canvas.SetTop(key, MazeGrid[0, 0].Height * 3);
            Canvas.SetLeft(key, (width * MazeGrid[0, 0].Width) + 300);
            Maze.Children.Add(key);

            // Start key
            TextBlock yellowtxt = new TextBlock();
            yellowtxt.Text = "Start";
            yellowtxt.Foreground = Brushes.White;
            yellowtxt.FontSize = 10;
            Canvas.SetTop(yellowtxt, (MazeGrid[0, 0].Height * 3) + 20);
            Canvas.SetLeft(yellowtxt, (width * MazeGrid[0, 0].Width) + 260);
            Maze.Children.Add(yellowtxt);

            Rectangle yellowrec = new Rectangle();
            yellowrec.Fill = Brushes.Yellow;
            yellowrec.Height = 10;
            yellowrec.Width = 10;
            Canvas.SetTop(yellowrec, (MazeGrid[0, 0].Height * 3) +20);
            Canvas.SetLeft(yellowrec, (width * MazeGrid[0, 0].Width) + 250);
            Maze.Children.Add(yellowrec);


            // End key
            TextBlock pinktxt = new TextBlock();
            pinktxt.Text = "End";
            pinktxt.Foreground = Brushes.White;
            pinktxt.FontSize = 10;
            Canvas.SetTop(pinktxt, (MazeGrid[0, 0].Height * 3) + 40);
            Canvas.SetLeft(pinktxt, (width * MazeGrid[0, 0].Width) + 260);
            Maze.Children.Add(pinktxt);
           
            Rectangle pinkrec = new Rectangle();
            pinkrec.Fill = Brushes.DeepPink;
            pinkrec.Height = 10;
            pinkrec.Width = 10;
            Canvas.SetTop(pinkrec, (MazeGrid[0, 0].Height * 3) + 40);
            Canvas.SetLeft(pinkrec, (width * MazeGrid[0, 0].Width) + 250);
            Maze.Children.Add(pinkrec);

            // possible node
            TextBlock gtxt = new TextBlock();
            gtxt.Text = "Node that can be explored";
            gtxt.Foreground = Brushes.White;
            gtxt.FontSize = 10;
            Canvas.SetTop(gtxt, (MazeGrid[0, 0].Height * 3) + 60);
            Canvas.SetLeft(gtxt, (width * MazeGrid[0, 0].Width) + 260);
            Maze.Children.Add(gtxt);

            Rectangle grec = new Rectangle();
            grec.Fill = Brushes.Green;
            grec.Height = 10;
            grec.Width = 10;
            Canvas.SetTop(grec, (MazeGrid[0, 0].Height * 3) + 60);
            Canvas.SetLeft(grec, (width * MazeGrid[0, 0].Width) + 250);
            Maze.Children.Add(grec);
            // explored node
            TextBlock rtxt = new TextBlock();
            rtxt.Text = "Node that has been explored";
            rtxt.Foreground = Brushes.White;
            rtxt.FontSize = 10;
            Canvas.SetTop(rtxt, (MazeGrid[0, 0].Height * 3) + 80);
            Canvas.SetLeft(rtxt, (width * MazeGrid[0, 0].Width) + 260);
            Maze.Children.Add(rtxt);

            Rectangle rrec = new Rectangle();
            rrec.Fill = Brushes.Red;
            rrec.Height = 10;
            rrec.Width = 10;
            Canvas.SetTop(rrec, (MazeGrid[0, 0].Height * 3) + 80);
            Canvas.SetLeft(rrec, (width * MazeGrid[0, 0].Width) + 250);
            Maze.Children.Add(rrec);

            // optimal path
            TextBlock btxt = new TextBlock();
            btxt.Text = "Optimal path";
            btxt.Foreground = Brushes.White;
            btxt.FontSize = 10;
            Canvas.SetTop(btxt, (MazeGrid[0, 0].Height * 3) + 100);
            Canvas.SetLeft(btxt, (width * MazeGrid[0, 0].Width) + 260);
            Maze.Children.Add(btxt);

            Rectangle brec = new Rectangle();
            brec.Fill = Brushes.Blue;
            brec.Height = 10;
            brec.Width = 10;
            Canvas.SetTop(brec, (MazeGrid[0, 0].Height * 3) + 100);
            Canvas.SetLeft(brec, (width * MazeGrid[0, 0].Width) + 250);
            Maze.Children.Add(brec);
            // human path
            TextBlock sbtxt = new TextBlock();
            sbtxt.Text = "Human generated path";
            sbtxt.Foreground = Brushes.White;
            sbtxt.FontSize = 10;
            Canvas.SetTop(sbtxt, (MazeGrid[0, 0].Height * 3) + 120);
            Canvas.SetLeft(sbtxt, (width * MazeGrid[0, 0].Width) + 260);
            Maze.Children.Add(sbtxt);

            Rectangle sbrec = new Rectangle();
            sbrec.Fill = Brushes.SaddleBrown;
            sbrec.Height = 10;
            sbrec.Width = 10;
            Canvas.SetTop(sbrec, (MazeGrid[0, 0].Height * 3) + 120);
            Canvas.SetLeft(sbrec, (width * MazeGrid[0, 0].Width) + 250);
            Maze.Children.Add(sbrec);

            // current user location
            TextBlock otxt = new TextBlock();
            otxt.Text = "Current location of the user";
            otxt.Foreground = Brushes.White;
            otxt.FontSize = 10;
            Canvas.SetTop(otxt, (MazeGrid[0, 0].Height * 3) + 140);
            Canvas.SetLeft(otxt, (width * MazeGrid[0, 0].Width) + 260);
            Maze.Children.Add(otxt);

            Rectangle orec = new Rectangle();
            orec.Fill = Brushes.Orange;
            orec.Height = 10;
            orec.Width = 10;
            Canvas.SetTop(orec, (MazeGrid[0, 0].Height * 3) + 140);
            Canvas.SetLeft(orec, (width * MazeGrid[0, 0].Width) + 250);
            Maze.Children.Add(orec);

            //add instructions
            TextBlock instructxt = new TextBlock();
            instructxt.Text = "WASD for movement";
            instructxt.Foreground = Brushes.White;
            instructxt.FontSize = 20;
            Canvas.SetTop(instructxt, (MazeGrid[0, 0].Height * 3) + 160);
            Canvas.SetLeft(instructxt, (width * MazeGrid[0, 0].Width)+20);
            Maze.Children.Add(instructxt);
        }

        void loadbuttons() {
            Button DownButton = new Button();

            DownButton.Content = "Solve";

            DownButton.Width = 80;

            DownButton.Height = 30;

            // Set the name of the function to call when the button is clicked

            DownButton.Click += new RoutedEventHandler(OnDownButtonClick);

            Canvas.SetTop(DownButton, (height * MazeGrid[0, 0].Width)-5);

            Canvas.SetLeft(DownButton, 0);

            Maze.Children.Add(DownButton);
            int index=0;
            bool flag = false;

            mazePicker = new ComboBox();
            mazePicker.KeyDown += new KeyEventHandler(keydown);
            DirectoryInfo d = new DirectoryInfo(mazeFile);
            FileInfo[] Files = d.GetFiles("*.txt");
            foreach (FileInfo file in Files) {
                mazePicker.Items.Add(file.Name.Replace(".txt", "").Trim());
                if (file.Name.Replace(".txt", "").Trim() == "20x20(0)") {
                    flag = true;
                
                }
                if (flag == false) {
                    index += 1;
                }
            }
            mazePicker.SelectionChanged += new SelectionChangedEventHandler(OnSelectionchanged);
            mazePicker.SelectedIndex = index;

            Canvas.SetTop(mazePicker, (MazeGrid[0, 0].Height * 5) + 10);

            Canvas.SetLeft(mazePicker, (width * MazeGrid[0,0].Width)+50);

            TextBlock timeDelayTextBox = new TextBlock();
            timeDelayTextBox.Text = "Solver time delay (ms)";
            timeDelayTextBox.Foreground = Brushes.White;
            timeDelayTextBox.FontSize = 20;
            Canvas.SetTop(timeDelayTextBox, (MazeGrid[0, 0].Height * 15));
            Canvas.SetLeft(timeDelayTextBox, (width * MazeGrid[0, 0].Width) + 50);
            Maze.Children.Add(timeDelayTextBox);

            timeDelay.Width = 100;
            timeDelay.Foreground = Brushes.Black;
            timeDelay.IsReadOnly = false;
            timeDelay.Text = "10";
            Canvas.SetTop(timeDelay, (MazeGrid[0, 0].Height * 15) + 30);

            Canvas.SetLeft(timeDelay, (width * MazeGrid[0, 0].Width) + 50);
            Maze.Children.Add(timeDelay);

            Maze.Children.Add(mazePicker);

            
            
       }

        int CalculateGcost(node current) { 
        // start node
        int g = (int)Math.Ceiling((Math.Sqrt(Math.Pow(current.x - file_handler.startco[0], 2) + Math.Pow((current.y - file_handler.startco[1]), 2))));
            return g;
        }

        int CalculateHcost(node current) { 
        // end node
        int h = (int)Math.Floor(Math.Sqrt((Math.Pow(current.x - file_handler.endco[0], 2) + Math.Pow((current.y - file_handler.endco[1]), 2))*10));
            return h;
        }

        void solve(object sender, EventArgs e) 
        { 
                current = GetNodeWithLowestFcost(openset);
                openset.Remove(current);
                closedset.Add(current);
                int[] currentco = { current.x, current.y };
                updatescreen(currentco[1], currentco[0], true);
                // check if it is the end
                if ((current.x == file_handler.endco[0]) && (current.y == file_handler.endco[1]))
                {
                    dispatcherTimer.Stop();
                    SetBestPath(currentco[0], currentco[1]);
                    solved = true;
                    return;
                }

                // find neighbours
                int[] neighbours = FindNeighbors(current);

                for (int i = 0; i < neighbours.Length; i++)
                {

                    if (neighbours[i] == -1) { }

                    else if (nodes[neighbours[i]].value != -1 && closedset.Contains(nodes[neighbours[i]]) == false)
                    {
                        updatescreen(nodes[neighbours[i]].y, nodes[neighbours[i]].x, false);

                        node tempnode = new node(nodes[neighbours[i]].x, nodes[neighbours[i]].y, 1, CalculateGcost(nodes[neighbours[i]]), CalculateHcost(nodes[neighbours[i]]), currentco, nodes[neighbours[i]].start, nodes[neighbours[i]].end);

                        MazeGrid[tempnode.y, tempnode.x].Child = new TextBlock() { Text = tempnode.fcost.ToString() };

                        nodes[neighbours[i]] = tempnode;
                        if (openset.Contains(nodes[neighbours[i]]) == false)
                        {
                            updatescreen(nodes[neighbours[i]].y, nodes[neighbours[i]].x, false);
                            openset.Add(nodes[neighbours[i]]);
                        }
                    }
                }
        }

        node GetNodeWithLowestFcost(List<node> _openset) { 

            int lowest = 1000;
            int highestHcost = 1000; 
            int index =0;
            List<int> repeatedLowest = new List<int>();
            

            for (int i=0; i<_openset.Count; i++) {
                if(_openset[i].fcost < lowest) {
                    lowest = _openset[i].fcost;
                    index = i;
            }
        }

            for (int i=0; i<_openset.Count; i++) {
                if(_openset[i].fcost == lowest) {
                    repeatedLowest.Add(i);
                }
            }
            if (repeatedLowest.Count > 1)
            {
                for (int i = 0; i < repeatedLowest.Count; i++)
                {
                    if (_openset[repeatedLowest[i]].hcost < highestHcost)
                    {
                        highestHcost = _openset[repeatedLowest[i]].hcost;
                        index = repeatedLowest[i];
                    }
                }
            }
      


            return _openset[index];
            }

        int[] FindNeighbors(node current) {


                int[] neighbours= new int[4];
                if(current.y == 0) {
                    neighbours[0] = -1; 
                    neighbours[2] = ((current.y+1)*height+(current.x));
                    
                }
                else if(current.y == (height-1)) {
                    neighbours[0] = ((current.y-1)*height+(current.x));

                    neighbours[2] = -1;
                    
                }
                else {
                
                    neighbours[0] = ((current.y-1)*height+(current.x));

                    neighbours[2] = ((current.y+1)*height+(current.x));
                }


                if(current.x == 0) {
                    neighbours[1] = (current.y*height+(current.x+1));

                    neighbours[3] = -1;
                    
                }
                
                else if(current.x == (width-1)) {
                    neighbours[1] = -1;

                    neighbours[3] =  (current.y*height+(current.x-1));
                    
                }
                else { 
                    neighbours[1] = (current.y*height+(current.x+1));

                    neighbours[3] =  (current.y*height+(current.x-1));
            }

                return neighbours;
                
        }

        void updatescreen(int row, int column, bool state) { // true would mean that the node is closed, false if it is open
            if (state == true) {
                MazeGrid[row, column].Background = Brushes.Red;
                return;
            }
            MazeGrid[row, column].Background = Brushes.Green;
        }

        void updatescreen(int row, int column, int state)
        { // true would mean that the node is closed, false if it is open
            if (state == 1)
            {
                MazeGrid[row, column].Background = Brushes.SaddleBrown;
                return;
            }
            MazeGrid[row, column].Background = Brushes.Orange;
        }

        int updatescreen(int row, int column)
        {
            if (row == -1 && column == -1)
            {
                return -1;
            }
            MazeGrid[row, column].Background = Brushes.Blue;
            return updatescreen(nodes[row*height + column].parent[1], nodes[row*height + column].parent[0]);
        }

        int SetBestPath(int x, int y)
        {
            return updatescreen(y, x);
        }

        void OnDownButtonClick(object sender, EventArgs e) {
            if (solved == false) {
                openset.Clear();
                closedset.Clear();
                openset.Add(nodes[file_handler.startco[0] +file_handler.startco[1]*height]);
                current = openset[0];
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds(double.Parse(timeDelay.Text));
                dispatcherTimer.Start();
            }
            
        }

        void OnSelectionchanged(object sender, EventArgs e) {
            resetmazegrid();
            solved = false;
            loadmaze(mazePicker.SelectedItem.ToString());
        }

        private void keydown(object sender, KeyEventArgs e)
        {
            if (solved == false) {
                int[] neighbours = FindNeighbors(nodes[userloc[1] * height + userloc[0]]);
                if (e.Key == Key.A  || e.Key == Key.Left) {
                    if (neighbours[3] == -1 || nodes[neighbours[3]].value == -1) { }
                    else {
                        updatescreen(userloc[1], userloc[0], 1);

                        userloc[0] = nodes[neighbours[3]].x;
                        userloc[1] = nodes[neighbours[3]].y;
                        updatescreen(userloc[1], userloc[0], 0);

                    }
                }
                if (e.Key == Key.W || e.Key == Key.Up)
                {
                    if (neighbours[0] == -1 || nodes[neighbours[0]].value == -1) { }
                    else
                    {
                        updatescreen(userloc[1], userloc[0], 1);

                        userloc[0] = nodes[neighbours[0]].x;
                        userloc[1] = nodes[neighbours[0]].y;
                        updatescreen(userloc[1], userloc[0], 0);

                    }
                }
                if (e.Key == Key.D || e.Key == Key.Right)
                {
                    if (neighbours[1] == -1 || nodes[neighbours[1]].value == -1) { }
                    else
                    {
                        updatescreen(userloc[1], userloc[0], 1);
                        userloc[0] = nodes[neighbours[1]].x;
                        userloc[1] = nodes[neighbours[1]].y;
                        updatescreen(userloc[1], userloc[0], 0);

                    }
                }
                if (e.Key == Key.S || e.Key == Key.Down)
                {
                    if (neighbours[2] == -1 || nodes[neighbours[2]].value == -1) { }
                    else
                    {
                        updatescreen(userloc[1], userloc[0], 1);
                        userloc[0] = nodes[neighbours[2]].x;
                        userloc[1] = nodes[neighbours[2]].y;
                        updatescreen(userloc[1], userloc[0], 0);

                    }
                }
                if (nodes[neighbours[2]].end == true) {
                    solved = true;
                    MessageBox.Show("You won, but really i won since this code actually works. I am the best programmer in the world!");
                }
            }
            if (timeDelay.IsFocused == true) {

                if (((decimal)e.Key >= 34 && (decimal)e.Key <= 43))
                {
                    timeDelay.Text += ((decimal)e.Key-34).ToString();
                    timeDelay.SelectionStart = timeDelay.Text.Length;
                    timeDelay.SelectionLength = 0;

                }
                else if (e.Key == Key.Back)
                {
                    timeDelay.Text = timeDelay.Text.Remove(timeDelay.Text.Length - 1, 1);
                }
                else if (e.Key == Key.OemPeriod) {
                    timeDelay.Text += ".";
                    timeDelay.SelectionStart = timeDelay.Text.Length;
                    timeDelay.SelectionLength = 0;
                }
                
            }
            e.Handled = true;

        }
    }
}