using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotentialField : MonoBehaviour
{
    static Dictionary<Tile.TileTypes, int> weights = new Dictionary<Tile.TileTypes, int>
    {
        {Tile.TileTypes.Empty, 1000},
        {Tile.TileTypes.Water, 30},
        {Tile.TileTypes.Sand, 2},
        {Tile.TileTypes.Grass, 1},
        {Tile.TileTypes.Forest, 2},
        {Tile.TileTypes.Stone, 1},
        {Tile.TileTypes.Mountain, 3}
    };

    public static int[,] createPotentialField(Tile[,] tileMap, Tile goal)
    {
        int[,] potentialField = new int[tileMap.GetLength(0), tileMap.GetLength(1)];
        bool[,] visited = new bool[tileMap.GetLength(0), tileMap.GetLength(1)];
        Queue<Tile> toVisit = new Queue<Tile>();

        for (int i = 0; i < potentialField.GetLength(0); i++)
        {
            for (int j = 0; j < potentialField.GetLength(1); j++)
            {
                potentialField[i,j] = weights[Tile.TileTypes.Empty];
            }
        }

        potentialField[goal._coordinateWidth, goal._coordinateHeight] = 0;
        visited[goal._coordinateWidth, goal._coordinateHeight] = true;
        
        List<Tile> neighbors = goal._neighborTiles;
        foreach (Tile n in neighbors)
        {
            toVisit.Enqueue(n);
        }

        void _createPotentialField(Tile currentTile)
        {
            if(visited[currentTile._coordinateWidth, currentTile._coordinateHeight] == true)
            {
                return;
            }
            List<Tile> currNeighbors = currentTile._neighborTiles;
            int minNeighbor = weights[Tile.TileTypes.Empty];
            foreach (Tile n in currNeighbors)
            {
                if(potentialField[n._coordinateWidth, n._coordinateHeight] < minNeighbor)
                {
                    minNeighbor = potentialField[n._coordinateWidth, n._coordinateHeight];
                }
            }

            potentialField[currentTile._coordinateWidth, currentTile._coordinateHeight] = minNeighbor + weights[currentTile._type];
            visited[currentTile._coordinateWidth, currentTile._coordinateHeight] = true;

            foreach (Tile n in currNeighbors)
            {
                toVisit.Enqueue(n);
            }
        }

        while (toVisit.Count != 0)
        {
            Tile currTile = toVisit.Dequeue();
            _createPotentialField(currTile);
        }

        return potentialField;
    }

    public static void Print2DArray<T>(T[,] matrix)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            string str = "";
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                str += matrix[i,j].ToString();
                str += " ";
            }
            // Debug.Log(str);
        }
    }
}
