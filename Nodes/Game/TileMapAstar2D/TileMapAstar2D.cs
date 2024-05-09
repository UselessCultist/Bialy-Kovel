using Godot;
using System;
using System.Collections.Generic;

public partial class TileMapAstar2D : TileMapLayer
{
    enum Tile { OBSTACLE, START_POINT, END_POINT }
    readonly Vector2I CELL_SIZE = new Vector2I(16, 16);
    const float BASE_LINE_WIDTH = 3.0f;
    readonly Color DRAW_COLOR = new Color(1, 1, 1, 0.5f);


    AStarGrid2D _astar = new();

    public AStarGrid2D Grid { get { return _astar; } }

	Vector2I _start_point;
	Vector2I _end_point;
	Vector2[] _path;

    public Vector2 RoundLocalPosition(Vector2 local_position) 
    {
        return MapToLocal(LocalToMap(local_position));
    }

    bool IsPointWalkable(Vector2 local_position) 
    {
        var map_position = LocalToMap(local_position);
        if (_astar.IsInBoundsv(map_position)) 
        {
            return !_astar.IsPointSolid(map_position);
        }
        return false;
    }

    public Vector2[] FindPath(Vector2 local_start_point, Vector2 local_end_point)
    {
        Vector2I start_point = LocalToMap(local_start_point);
        Vector2I end_point = LocalToMap(local_end_point);
        var path = _astar.GetPointPath(start_point, end_point);

        return path;
    }

    public List<Character> GetObjectsInNeighborCells(Vector2I cell_pos, int radius) 
    {
        List<Character> objects = new List<Character>();
        Vector2I start_cell = new(cell_pos.X-radius, cell_pos.Y-radius);
        Vector2I end_cell = new(cell_pos.X+radius, cell_pos.Y+radius);

        for (int i = start_cell.Y; i <= end_cell.Y; i++)
        {
            for (int j = start_cell.X; j <= end_cell.X; j++)
            {
                TileData data = GetCellTileData(new Vector2I(i, j));

                var path = data.GetCustomData("ObjectInCell").ToString();
                if (!String.Equals(path, ""))
                {
                    Character obj = GetNode<Character>(path);
                    objects.Add(obj);
                }
            }
        }
        return objects;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        _astar.Region = new Rect2I(-250, -250, 500, 500);   
        _astar.CellSize = CELL_SIZE;
        _astar.Offset = new(CELL_SIZE.X*0.5f, CELL_SIZE.Y*0.5f);
        _astar.DefaultComputeHeuristic = AStarGrid2D.Heuristic.Euclidean;
        _astar.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Euclidean;
        _astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.AtLeastOneWalkable;
        _astar.Update();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}




namespace AStarSharp
{
    public class Node
    {
        // Change this depending on what the desired size is for each element in the grid
        public static int NODE_SIZE = 16;
        public Node Parent;
        public Vector2 Position;
        public Vector2 Center
        {
            get
            {
                return new Vector2(Position.X + NODE_SIZE / 2, Position.Y + NODE_SIZE / 2);
            }
        }
        public float DistanceToTarget;
        public float Cost;
        public float Weight;
        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                else
                    return -1;
            }
        }
        public bool Walkable;

        public Node(Vector2 pos, bool walkable, float weight = 1)
        {
            Parent = null;
            Position = pos;
            DistanceToTarget = -1;
            Cost = 1;
            Weight = weight;
            Walkable = walkable;
        }
    }

    public class Astar
    {
        List<List<Node>> Grid;
        int GridRows
        {
            get
            {
                return Grid[0].Count;
            }
        }
        int GridCols
        {
            get
            {
                return Grid.Count;
            }
        }

        public Astar(List<List<Node>> grid)
        {
            Grid = grid;
        }

        public Stack<Node> FindPath(Vector2 Start, Vector2 End)
        {
            Node start = new Node(new Vector2((int)(Start.X / Node.NODE_SIZE), (int)(Start.Y / Node.NODE_SIZE)), true);
            Node end = new Node(new Vector2((int)(End.X / Node.NODE_SIZE), (int)(End.Y / Node.NODE_SIZE)), true);

            Stack<Node> Path = new Stack<Node>();
            PriorityQueue<Node, float> OpenList = new PriorityQueue<Node, float>();
            List<Node> ClosedList = new List<Node>();
            List<Node> adjacencies;
            Node current = start;

            // add start node to Open List
            OpenList.Enqueue(start, start.F);

            while (OpenList.Count != 0 && !ClosedList.Exists(x => x.Position == end.Position))
            {
                current = OpenList.Dequeue();
                ClosedList.Add(current);
                adjacencies = GetAdjacentNodes(current);

                foreach (Node n in adjacencies)
                {
                    if (!ClosedList.Contains(n) && n.Walkable)
                    {
                        bool isFound = false;
                        foreach (var oLNode in OpenList.UnorderedItems)
                        {
                            if (oLNode.Element == n)
                            {
                                isFound = true;
                            }
                        }
                        if (!isFound)
                        {
                            n.Parent = current;
                            n.DistanceToTarget = Math.Abs(n.Position.X - end.Position.X) + Math.Abs(n.Position.Y - end.Position.Y);
                            n.Cost = n.Weight + n.Parent.Cost;
                            OpenList.Enqueue(n, n.F);
                        }
                    }
                }
            }

            // construct path, if end was not closed return null
            if (!ClosedList.Exists(x => x.Position == end.Position))
            {
                return null;
            }

            // if all good, return path
            Node temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null) return null;
            do
            {
                Path.Push(temp);
                temp = temp.Parent;
            } while (temp != start && temp != null);
            return Path;
        }

        private List<Node> GetAdjacentNodes(Node n)
        {
            List<Node> temp = new List<Node>();

            int row = (int)n.Position.Y;
            int col = (int)n.Position.X;

            if (row + 1 < GridRows)
            {
                temp.Add(Grid[col][row + 1]);
            }
            if (row - 1 >= 0)
            {
                temp.Add(Grid[col][row - 1]);
            }
            if (col - 1 >= 0)
            {
                temp.Add(Grid[col - 1][row]);
            }
            if (col + 1 < GridCols)
            {
                temp.Add(Grid[col + 1][row]);
            }

            return temp;
        }
    }
}