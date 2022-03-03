using Raylib_cs;
using System.Numerics;
namespace game {
    public struct Pos {
        public int x;
        public int y;
        public Pos(int X, int Y){
            x = X;
            y = Y;
        }
    }

    class Grid {
        types[,] grid;
        int[,] labels;
        
        Dictionary<int, Component> components;
        List<Connection> connections;
        int width, height;
        public Grid(int w, int h, int size){
            grid = new types[h,w];
            labels = new int[h,w];
            components = new Dictionary<int, Component>();
            connections = new List<Connection>();
            width = w;
            height = h;
        }

        public int toGrid(float pos, int gridsize) {
            pos =  pos/gridsize;
            return (int) pos;
        }

        public void add(Vector2 pos, types t, int gridsize) {
            int x = toGrid(pos.X, gridsize);
            int y = toGrid(pos.Y, gridsize);
            add(x, y, t);
        }
        public void add(int x, int y, types t) {
            if (x<0 || x>=width){return;}
            if (y<0 || y>=height){return;}
            if (grid[y,x] == t) {return;}
            grid[y,x] = t;
            buildObjects();
        }
        public void del(Vector2 pos, int gridsize) {
            int x = toGrid(pos.X, gridsize);
            int y = toGrid(pos.Y, gridsize);
            if (x<0 || x>=width){return;}
            if (y<0 || y>=height){return;}
            grid[y,x] = 0;
            buildObjects();
        }

        public types GetBlock(int x, int y) {
            if (x<0 || x>width || y<0 || y>height) {
                return 0;
            }
            return grid[y,x];
        }


        public int[,] changeLabel(int from, int to, int[,] labels) {
            for (int y=0; y<height; y++) {
                for (int x=0; x<width; x++) {
                    if (labels[y,x] == from){
                        labels[y,x] = to;
                    }
                }
            }
            return labels;
        }

        public void printlabels(int[,] labels){
            for (int y=0; y<height; y++) {
                for (int x=0; x<width; x++) {
                    Console.Write(labels[y,x].ToString()+", ");
                }
                Console.Write("\n");
            }
        }

        public void buildObjects(){
            connectedComponents();
            makeComponents();
            makeConnections();
        }
        public void makeComponents(){
            components = new Dictionary<int, Component>();
            for (int y=0; y<height; y++) {
                for (int x=0; x<width; x++) { 
                    if (labels[y,x] <= 0 || grid[y,x] == 0) {continue;}
                    if (components.ContainsKey(labels[y,x])) {
                        components[labels[y,x]].add(new Pos(x,y));
                    }
                    else {
                        Component c = ComponentFactory.NewComponent(grid[y,x]);
                        c.add(new Pos(x,y));
                        components.Add(labels[y,x], c);
                    }
                }
            }
        }

        public void makeConnections(){
            connections = new List<Connection>();
            for (int y=0; y<height; y++) {
                for (int x=0; x<width; x++) { 
                    if (labels[y,x] != -1) {continue;}

                    bool wiref = false;
                    bool otherf = false;
                    Pos[] neighbors = {new Pos(x-1,y), new Pos(x,y-1), new Pos(x+1,y), new Pos(x,y+1)};
                    Connection con = ComponentFactory.NewConnection(grid[y,x], new Pos(x,y));
                    foreach (Pos pos in neighbors) {
                        types block = GetBlock(pos.x, pos.y);
                        if (block == 0) {continue;}
                        if (!wiref && block == types.WIRE) {
                            con.addWire(components[labels[pos.y, pos.x]]);
                            wiref = true;
                        }
                        else if (!otherf && labels[pos.y, pos.x] >= 0 && block != types.WIRE) {
                            con.addOther(components[labels[pos.y, pos.x]]);
                            otherf = true;
                        }
                    }
                    connections.Add(con);
                }
            }
        }
        
        public void connectedComponents() {
            labels = new int[height,width];
            int label = 1;
            bool changed = true;
            while (changed){
                for (int y=0; y<height; y++) {
                    for (int x=0; x<width; x++) {
                        changed = false;
                        if (grid[y,x] == 0){
                            continue;
                        }
                        if (grid[y,x] == types.OUT || grid[y,x] == types.IN){
                            labels[y,x] = -1;
                            changed = true;
                            continue;
                        }
                        bool found = false;
                        if (GetBlock(x-1, y) == grid[y,x]){
                            labels[y,x] = labels[y,x-1];
                            found = true;
                            changed = true;
                        }
                        if (GetBlock(x, y-1) == grid[y,x]){
                            if (GetBlock(x-1, y) == grid[y,x]) {
                                labels = changeLabel(labels[y,x], labels[y-1, x], labels);
                            }
                            labels[y,x] = labels[y-1,x];
                            found = true;
                            changed = true;
                        }
                        if (!found){
                            labels[y,x] = label++;
                            changed = true;
                        }
                    }
                }
            }
            // printlabels(labels);
        }

        public void update(){
            foreach (Connection c in connections) {
                c.update();
            }
        }

        public void draw(int gridsize, int xoff, int yoff) {
            foreach(KeyValuePair<int, Component> entry in components) {
                entry.Value.draw(gridsize, xoff, yoff);
            }
            foreach (Connection c in connections) {
                c.draw(gridsize, xoff, yoff);
            }
        }
    }
}