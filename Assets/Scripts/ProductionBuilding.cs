using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBuilding : Building
{
    #region Attributes
    public bool _scaleWithNeighbors; // Boolean whether the efficiency of the building depends on neighboring tiles
    public int _maxNeighbors; // Number of neighboring tiles of a type needed to maximize efficiency
    #endregion

    public void calcEfficiency(List<Tile> neighbors)
    {
        if (!_scaleWithNeighbors)
        {
            _efficiency = 1.0f;
            return;
        }

        //List<Tile> neighbors = FindNeighborsOfTile(_tile);
        List<Tile> scalingNeighbors = new List<Tile>();

        if (_type == ProductionBuilding.BuildingTypes.Fishery)
        {
            scalingNeighbors = FindScalingNeighbors(Tile.TileTypes.Water, neighbors);
        }
        else if (_type == ProductionBuilding.BuildingTypes.Lumberjack)
        {
            scalingNeighbors = FindScalingNeighbors(Tile.TileTypes.Forest, neighbors);
        }
        else if (_type == ProductionBuilding.BuildingTypes.SheepFarm)
        {
            scalingNeighbors = FindScalingNeighbors(Tile.TileTypes.Grass, neighbors);
        }
        else if (_type == ProductionBuilding.BuildingTypes.PotatoFarm)
        {
            scalingNeighbors = FindScalingNeighbors(Tile.TileTypes.Grass, neighbors);
        }

        float efficency = (float)scalingNeighbors.Count / (float)_maxNeighbors;
        if (efficency > 1)
        {
            efficency = 1;
        }

        _efficiency = efficency;
    }

    private List<Tile> FindScalingNeighbors(Tile.TileTypes requireTileType, List<Tile> neighbors)
    {
        List<Tile> result = new List<Tile>();

        foreach (Tile tile in neighbors)
        {
            if (tile._type == requireTileType)
            {
                result.Add(tile);
            }
        }

        return result;
    }

    //This class acts as a data container and has no functionality
}
