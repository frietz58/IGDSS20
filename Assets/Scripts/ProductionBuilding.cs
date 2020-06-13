using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBuilding : Building
{
    #region Attributes
    public BuildingTypes _type; //The type of the building
    public int _upkeep; // The cost for upkeep of the building per economy tick
    public int _costMoney; // The initial cost of the building in money
    public int _costPlanks; // The initial cost of the building in planks
    public Tile _tile; // The tile the building is placed on
    public float _efficiency; // A float in [0,1] that represents the efficiency of the building
    public float _generationInterval; // The seconds interval for the building to produce resources
    public int _outputCount; // How many resources are output in one generation interval
    public Tile.TileTypes[] _placement; // The types of tiles the building can be built on
    public bool _scaleWithNeighbors; // Boolean whether the efficiency of the building depends on neighboring tiles
    public int _maxNeighbors; // Number of neighboring tiles of a type needed to maximize efficiency
    public GameManager.ResourceTypes input; // Resource type of input resources needed
    public GameManager.ResourceTypes output; // Resource type of output resources
    #endregion

    #region Enumerations
    //Enumeration of all available building types. Can be addressed from other scripts by calling Building.BuildingTypes
    public enum BuildingTypes { Empty, Fishery, Lumberjack, Sawmill, SheepFarm, Knitters, PotatoFarm, Distillery };
    #endregion

    //This class acts as a data container and has no functionality
}
