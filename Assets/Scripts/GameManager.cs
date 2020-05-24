using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{

    #region Map generation
    private Tile[,] _tileMap; //2D array of all spawned tiles

    bool verbose = false;
    Texture2D heightmap = null;

    // heightmap contains values between 0 and 1, we want greater height differences, so we scale the height values
    public int heightmapScaleFactor = 50;

    public float firstRowPos = 0f;
    public float lastRowPos = 0f;
    public float firstColPos = 0f;
    public float lastColPos = 0f;

    // how much we have to move the tiles, for the next position
    public float row_offset_x = 8.65f;
    public int col_offset_z = 10;

    //references to all the prefabs
    public GameObject waterPrefab;
    public GameObject sandPrefab;
    public GameObject grassPrefab;
    public GameObject ForestPrefab;
    public GameObject stonePrefab;
    public GameObject mountainPrefab;

    //economy tick
    public int seconds_past = 0;
    public int playerMoney = 100;
    public List<GameObject> upkeepBuildings = new List<GameObject>();
    private int updateAt = 0;

    void mapGenerationMain()
    {
        byte[] fileData;
        if (File.Exists("Assets/Textures/Heightmap_16.png"))
        {
            fileData = File.ReadAllBytes("Assets/Textures/Heightmap_128.png");
            heightmap = new Texture2D(2, 2);
            heightmap.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            // instanciate 2d array of empty Tile object with dims of the heightmap
            _tileMap = new Tile[heightmap.width, heightmap.height];

            // iter over image width and height and instanciate tile accordingly
            for (int row_ind = 0; row_ind < heightmap.height; row_ind++)
            {
                if (row_ind > lastRowPos)
                {
                    // keep these up to date so that they can be used as boundaries for camera script
                    lastRowPos = row_ind;
                }

                for (int col_ind = 0; col_ind < heightmap.width; col_ind++)
                {
                    if (col_ind > lastColPos)
                    {
                        lastColPos = col_ind;
                    }

                    // rgba color at each index, we use the maximun color value as height
                    UnityEngine.Color pixel_val = heightmap.GetPixel(row_ind, col_ind);
                    float intensity = pixel_val.maxColorComponent;
                    if (verbose)
                    {
                        string msg1 = "x: {0}, y: {1}, pixel: {2}";
                        Debug.LogFormat(string.Format(msg1, row_ind, col_ind, intensity));
                    }


                    UnityEngine.Vector3 pos_vec = offsetToPos(row_ind, col_ind, intensity * heightmapScaleFactor);

                    if (verbose)
                    {
                        string msg2 = "row: {0}, col: {1}, height: {2}, pos_vec: {3}";
                        Debug.LogFormat(string.Format(msg2, row_ind, col_ind, intensity * heightmapScaleFactor, pos_vec));
                    }

                    var newTile = Instantiate(getTileFromPixelVal(intensity), pos_vec, Quaternion.identity);
                    newTile.transform.parent = GameObject.Find("SpawnedTiles").transform;

                    // set the position of the Tile entity object of of the spawned prefab
                    Tile tileEntity = newTile.GetComponent<Tile>();
                    tileEntity._coordinateWidth = col_ind;
                    tileEntity._coordinateHeight = row_ind;
                    tileEntity._pos = pos_vec;

                    // tileEntity._neighborTiles = FindNeighborsOfTile(tileEntity);


                    // populate 2d array with the entity objects
                    _tileMap[row_ind, col_ind] = newTile.GetComponent<Tile>();

                }
            }

            // scale boundaries from indices to correct game coordinates
            lastColPos = lastColPos * col_offset_z;
            lastRowPos = lastRowPos * row_offset_x;
            string msg3 = "Row range: [{0}, {1}], Col range: [{2}, {3}]";
            Debug.Log(string.Format(msg3, firstRowPos, lastRowPos, firstColPos, lastColPos));

            foreach (Tile tileEntity in _tileMap)
            {
                tileEntity._neighborTiles = FindNeighborsOfTile(tileEntity);
            }
        }
    }

    Vector3 offsetToPos(int row_ind, int col_ind, float height)
    {
        // we need to convert the x,y indices from the heightmap to game coordinates
        float x_pos = row_offset_x * row_ind;
        float z_pos = col_offset_z * col_ind;

        if (row_ind % 2 == 1)
        {
            // if this is an odd row number, we need to shift the tile by half a tile width
            z_pos -= col_offset_z * 0.5f;
        }

        Vector3 pos_vec = new Vector3(x_pos, height, z_pos);
        return pos_vec;
    }

    GameObject getTileFromPixelVal(float pixelVal)
    {
        // return the gameobject to clone with instantiate, based on values given in moodle
        string tileName = "";
        GameObject prefab;
        if (pixelVal == 0)
        {
            //tileName = "WaterTile";
            prefab = waterPrefab;

        }
        else if (pixelVal > 0.0 && pixelVal <= 0.2)
        {
            //tileName = "SandTile";
            prefab = sandPrefab;

        }
        else if (pixelVal > 0.2 && pixelVal <= 0.4)
        {
            //tileName = "GrassTile";
            prefab = grassPrefab;

        }
        else if (pixelVal > 0.4 && pixelVal <= 0.6)
        {
            //tileName = "ForestTile";
            prefab = ForestPrefab;

        }
        else if (pixelVal > 0.6 && pixelVal <= 0.8)
        {
            //tileName = "StoneTile";
            prefab = stonePrefab;

        }
        else
        {
            //tileName = "MountainTile";
            prefab = mountainPrefab;
        }

        //return GameObject.Find(tileName);
        return prefab;
    }
    #endregion

    #region Buildings
    public GameObject[] _buildingPrefabs; //References to the building prefabs
    public int _selectedBuildingPrefabIndex = 0; //The current index used for choosing a prefab to spawn from the _buildingPrefabs list
    #endregion

    #region Resources
    private Dictionary<ResourceTypes, float> _resourcesInWarehouse = new Dictionary<ResourceTypes, float>(); //Holds a number of stored resources for every ResourceType

    //A representation of _resourcesInWarehouse, broken into individual floats. Only for display in inspector, will be removed and replaced with UI later
    [SerializeField]
    private float _ResourcesInWarehouse_Fish;
    [SerializeField]
    private float _ResourcesInWarehouse_Wood;
    [SerializeField]
    private float _ResourcesInWarehouse_Planks;
    [SerializeField]
    private float _ResourcesInWarehouse_Wool;
    [SerializeField]
    private float _ResourcesInWarehouse_Clothes;
    [SerializeField]
    private float _ResourcesInWarehouse_Potato;
    [SerializeField]
    private float _ResourcesInWarehouse_Schnapps;
    #endregion

    #region Enumerations
    public enum ResourceTypes { None, Fish, Wood, Planks, Wool, Clothes, Potato, Schnapps }; //Enumeration of all available resource types. Can be addressed from other scripts by calling GameManager.ResourceTypes
    #endregion

    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {
        PopulateResourceDictionary();
        mapGenerationMain();
    }

    // Update is called once per frame
    void Update()
    {
        HandleKeyboardInput();
        UpdateInspectorNumbersForResources();
        updateEconomy();
    }
    #endregion

    #region Methods
    //Makes the resource dictionary usable by populating the values and keys
    void PopulateResourceDictionary()
    {
        _resourcesInWarehouse.Add(ResourceTypes.None, 100);
        _resourcesInWarehouse.Add(ResourceTypes.Fish, 100);
        _resourcesInWarehouse.Add(ResourceTypes.Wood, 100);
        _resourcesInWarehouse.Add(ResourceTypes.Planks, 100);
        _resourcesInWarehouse.Add(ResourceTypes.Wool, 100);
        _resourcesInWarehouse.Add(ResourceTypes.Clothes, 100);
        _resourcesInWarehouse.Add(ResourceTypes.Potato, 100);
        _resourcesInWarehouse.Add(ResourceTypes.Schnapps, 100);
    }

    //Sets the index for the currently selected building prefab by checking key presses on the numbers 1 to 0
    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _selectedBuildingPrefabIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _selectedBuildingPrefabIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _selectedBuildingPrefabIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _selectedBuildingPrefabIndex = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _selectedBuildingPrefabIndex = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            _selectedBuildingPrefabIndex = 5;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            _selectedBuildingPrefabIndex = 6;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            _selectedBuildingPrefabIndex = 7;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            _selectedBuildingPrefabIndex = 8;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _selectedBuildingPrefabIndex = 9;
        }
    }

    //Updates the visual representation of the resource dictionary in the inspector. Only for debugging
    void UpdateInspectorNumbersForResources()
    {
        _ResourcesInWarehouse_Fish = _resourcesInWarehouse[ResourceTypes.Fish];
        _ResourcesInWarehouse_Wood = _resourcesInWarehouse[ResourceTypes.Wood];
        _ResourcesInWarehouse_Planks = _resourcesInWarehouse[ResourceTypes.Planks];
        _ResourcesInWarehouse_Wool = _resourcesInWarehouse[ResourceTypes.Wool];
        _ResourcesInWarehouse_Clothes = _resourcesInWarehouse[ResourceTypes.Clothes];
        _ResourcesInWarehouse_Potato = _resourcesInWarehouse[ResourceTypes.Potato];
        _ResourcesInWarehouse_Schnapps = _resourcesInWarehouse[ResourceTypes.Schnapps];
    }

    //Checks if there is at least one material for the queried resource type in the warehouse
    public bool HasResourceInWarehoues(ResourceTypes resource)
    {
        return _resourcesInWarehouse[resource] >= 1;
    }

    //Is called by MouseManager when a tile was clicked
    //Forwards the tile to the method for spawning buildings
    public void TileClicked(int height, int width)
    {
        Tile t = _tileMap[height, width];

        PlaceBuildingOnTile(t);
    }

    //Checks if the currently selected building type can be placed on the given tile and then instantiates an instance of the prefab
    private void PlaceBuildingOnTile(Tile clicked_tile)
    {

        //if there is building prefab for the number input
        if (_selectedBuildingPrefabIndex < _buildingPrefabs.Length)
        {
            //TODO: check if building can be placed and then istantiate it
            GameObject target_building = _buildingPrefabs[_selectedBuildingPrefabIndex];

            if (target_building.GetComponent<Building>()._costMoney <= playerMoney && 
                target_building.GetComponent<Building>()._costPlanks <= _resourcesInWarehouse[ResourceTypes.Planks] &&
                target_building.GetComponent<Building>()._placement.Contains(clicked_tile._type))
            {
                GameObject newBuilding = Instantiate(target_building, clicked_tile._pos, Quaternion.identity);
                
                playerMoney -= newBuilding.GetComponent<Building>()._costMoney;
                _resourcesInWarehouse[ResourceTypes.Planks] -= newBuilding.GetComponent<Building>()._costPlanks;

                newBuilding.GetComponent<Building>()._tile = clicked_tile;
                newBuilding.GetComponent<Building>()._efficiency = calcEfficiency(newBuilding);

                upkeepBuildings.Add(newBuilding);
            }
        }
    }

    private float calcEfficiency(GameObject b)
    {
        //float efficency = 0.0f;

        if (!b.GetComponent<Building>()._scaleWithNeighbors)
        {
            return 1.0f;
        }

        List<Tile> neighbors = FindNeighborsOfTile(b.GetComponent<Building>()._tile);
        List<Tile> scalingNeighbors = new List<Tile>();

        if (b.GetComponent<Building>()._type == Building.BuildingTypes.Fishery)
        {
            scalingNeighbors = FindScalingNeighbors(Tile.TileTypes.Water, neighbors);
        }
        else if (b.GetComponent<Building>()._type == Building.BuildingTypes.Lumberjack)
        {
            scalingNeighbors = FindScalingNeighbors(Tile.TileTypes.Forest, neighbors);
        }
        else if (b.GetComponent<Building>()._type == Building.BuildingTypes.SheepFarm)
        {
            scalingNeighbors = FindScalingNeighbors(Tile.TileTypes.Grass, neighbors);
        }
        else if (b.GetComponent<Building>()._type == Building.BuildingTypes.PotatoFarm)
        {
            scalingNeighbors = FindScalingNeighbors(Tile.TileTypes.Grass, neighbors);
        }

        float efficency = (float)scalingNeighbors.Count / (float)b.GetComponent<Building>()._maxNeighbors;
        Debug.Log(scalingNeighbors.Count);
        if (efficency > 1)
        {
            efficency = 1;
        }

        return efficency;
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

    //Returns a list of all neighbors of a given tile
    private List<Tile> FindNeighborsOfTile(Tile t)
    {
        List<Tile> result = new List<Tile>();

        // the indices for accessing the six neighbors on the 2d array...
        // because of the hexagonal offset, these are different depending on whether tile is in even or odd row on latice
        var neighborhood_access_even_row = new List<Tuple<int, int>> {
            Tuple.Create(-1, 0),
            Tuple.Create(-1, +1),
            Tuple.Create(0, -1),
            Tuple.Create(0, +1),
            Tuple.Create(+1, 0),
            Tuple.Create(+1, +1)
        };

        var neighborhood_access_odd_row = new List<Tuple<int, int>> {
            Tuple.Create(-1, -1),
            Tuple.Create(-1, +0),
            Tuple.Create(0, -1),
            Tuple.Create(0, +1),
            Tuple.Create(+1, -1),
            Tuple.Create(+1, 0)
        };

        int[] neighborhood_vals;

        // create list we will iterate over and bind list object dynamically
        List<Tuple<int, int>> neighborhood_access;

        if (t._coordinateHeight % 2 == 0)
        {
            neighborhood_access = neighborhood_access_even_row;
        }
        else
        {
            neighborhood_access = neighborhood_access_odd_row;
        }


        foreach (Tuple<int, int> neighbor_inds in neighborhood_access)
        {
            int neighbor_row_ind = t._coordinateHeight + neighbor_inds.Item1;
            int neighbor_col_ind = t._coordinateWidth + neighbor_inds.Item2;

            if (isValidTileIndex(neighbor_row_ind, neighbor_col_ind))
            {
                Tile neighbor_tile = _tileMap[neighbor_row_ind, neighbor_col_ind];
                result.Add(neighbor_tile);
            }

        }

        //TODO: put all neighbors in the result list

        return result;
    }

    // return boolean indicating whether a a combination of row & col indices lies on the 2d lattice of tiles that form our map
    private Boolean isValidTileIndex(int row_ind, int col_ind)
    {
        if (row_ind < 0 || col_ind < 0)
        {
            return false;
        }
        else if (row_ind >= heightmap.width || col_ind >= heightmap.height)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void updateEconomy()
    {
        seconds_past = (int)Time.time;

        // generate resources produced by buildings
        foreach (GameObject building in upkeepBuildings)
        {
            if ((seconds_past % (building.GetComponent<Building>()._generationInterval / building.GetComponent<Building>()._efficiency)) <= 0.1 && updateAt != seconds_past)
            {
                Debug.Log("Generating resource");

                // take away resource needed for production and produce only if input resource available
                if (building.GetComponent<Building>().input != ResourceTypes.None)
                {
                    Debug.Log("Requires input");
                    if (_resourcesInWarehouse[building.GetComponent<Building>().input] >= 1)
                    {
                        _resourcesInWarehouse[building.GetComponent<Building>().input] -= 1;
                        _resourcesInWarehouse[building.GetComponent<Building>().output] += building.GetComponent<Building>()._outputCount;
                    }
                }
                else //just produce all the time
                {
                    Debug.Log("Doesn't require input");
                    _resourcesInWarehouse[building.GetComponent<Building>().output] += building.GetComponent<Building>()._outputCount;
                }

            }
        }

        // generate income and pay upkeep of buildings
        if (seconds_past % 10 == 0 && updateAt != seconds_past)
        {
            playerMoney += 100;
            foreach (GameObject building in upkeepBuildings)
            {
                playerMoney -= building.GetComponent<Building>()._upkeep;
            }
        }

        // makes sure we produce at most once epr second
        updateAt = seconds_past;

    }
    #endregion
}
