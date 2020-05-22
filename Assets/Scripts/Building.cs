using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    #region Attributes
    public BuildingTypes _type; //The type of the tile
    public int _upkeep;
    public int _costMoney;
    public int _costPlanks;
    public Tile _tile;
    public float _efficiency;
    public float _generationInterval;
    public int _outputCount;
    public Tile.TileTypes _placement;
    public bool _scaleWithNeighbors;
    public (int, int) _minMaxNeighbors;
    public GameManager.ResourceTypes input;
    #endregion

    #region Enumerations
    //Enumeration of all available building types. Can be addressed from other scripts by calling Building.BuildingTypes
    public enum BuildingTypes { Empty, Fishery, Lumberjack, Sawmill, SheepFarm, Knitters, PotatoFarm, Distillery };
    #endregion

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
