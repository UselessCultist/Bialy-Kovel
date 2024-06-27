using Godot;
using System.Collections.Generic;

public partial class GridManager : Node
{
    private Dictionary<Vector2I, List<Character>> grid = new Dictionary<Vector2I, List<Character>>();
    private int cellSize = 16; // Размер ячейки сетки

    // Метод для добавления объекта в сетку
    public void AddToGrid(Character character)
    {
        Vector2I cellPos = GetCellPosition(character.GlobalPosition);
        if (!grid.ContainsKey(cellPos))
        {
            grid[cellPos] = new List<Character>();
        }
        grid[cellPos].Add(character);
    }

    // Метод для удаления объекта из сетки
    public void RemoveFromGrid(Character character)
    {
        Vector2I cellPos = GetCellPosition(character.GlobalPosition);
        if (grid.ContainsKey(cellPos))
        {
            grid[cellPos].Remove(character);
            if (grid[cellPos].Count == 0)
            {
                grid.Remove(cellPos);
            }
        }
    }

    // Метод для получения ячейки по позиции
    private Vector2I GetCellPosition(Vector2 position)
    {
        return new Vector2I((int)position.X / cellSize, (int)position.Y / cellSize);
    }

    public Character FindNearestResource(ResourceType type, Vector2 pos, int searchRadius)
    {
        Vector2I cellPos = GetCellPosition(pos);

        Queue<Vector2I> queue = new Queue<Vector2I>();
        HashSet<Vector2I> visited = new HashSet<Vector2I>();
        queue.Enqueue(cellPos);
        visited.Add(cellPos);

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { -1, 0, 1, 0 };

        while (queue.Count > 0)
        {
            Vector2I currentPos = queue.Dequeue();

            if (grid.ContainsKey(currentPos))
            {
                foreach (var character in grid[currentPos])
                {
                    if (character.Type == Type.Resource)
                    {
                        var ability = character.GetAbility<Resource>();
                        if (ability.ResourceType == type)
                        {
                            return character;
                        }
                    }
                }
            }

            // Добавляем соседние клетки в очередь
            for (int i = 0; i < 4; i++)
            {
                Vector2I neighborPos = new Vector2I(currentPos.X + dx[i], currentPos.Y + dy[i]);

                if (!visited.Contains(neighborPos) && Distance(cellPos, neighborPos) <= searchRadius)
                {
                    queue.Enqueue(neighborPos);
                    visited.Add(neighborPos);
                }
            }
        }

        return null;
    }

    // Вспомогательный метод для вычисления расстояния между ячейками
    private float Distance(Vector2I a, Vector2I b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.X - b.X, 2) + Mathf.Pow(a.Y - b.Y, 2));
    }

    // Метод для добавления всех существующих объектов в сетку
    public void AddAllExistingNodes()
    {
        foreach (Character character in GetTree().GetNodesInGroup("game_objects"))
        {
            AddToGrid(character);
        }
    }
}