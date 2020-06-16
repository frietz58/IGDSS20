using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{

    #region Map Generation
    private Tile[,] _tileMap; //2D array of all spawned tiles
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
    public GameObject workerPrefab;

    void mapGenerationMain()
    {
        // set to true for logging in console
        bool verbose = false;

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

                    //spawn random gameobject on tile, if tile supports it
                    populateTileRandomly(tileEntity);

                    // tileEntity._neighborTiles = FindNeighborsOfTile(tileEntity);


                    // populate 2d array with the entity objects
                    _tileMap[row_ind, col_ind] = newTile.GetComponent<Tile>();

                }
            }

            // scale boundaries from indices to correct game coordinates
            lastColPos = lastColPos * col_offset_z;
            lastRowPos = lastRowPos * row_offset_x;

            if (verbose)
            {
                string msg3 = "Row range: [{0}, {1}], Col range: [{2}, {3}]";
                Debug.Log(string.Format(msg3, firstRowPos, lastRowPos, firstColPos, lastColPos));
            }

            foreach (Tile tileEntity in _tileMap)
            {
                tileEntity._neighborTiles = FindNeighborsOfTile(tileEntity);
            }
        }
    }

    void populateTileRandomly(Tile tile)
    {
        // not all tiles have random gameobjects to spawn...
        if (!(tile.randomPropPrefabs.Length == 0)){

            int spawn_tries = 0;  // number of times we try to spawn a prop
            float keep_prob = 0.0f; // probability for actually spawning the prop
            int max_random_offset = 6; // range how wide our props spread from tile center in x dir
            Vector3 random_rotation_range = new Vector3(0, 360, 0);

            //todo make probability dynamic, depending on tile types in neighborhood
            if (tile._type == Tile.TileTypes.Forest)
            {
                spawn_tries = 10;
                keep_prob = 0.5f;
                random_rotation_range.x = 20;
                random_rotation_range.z = 20;

            } else if (tile._type == Tile.TileTypes.Grass)
            {
                spawn_tries = 10;
                keep_prob = 0.8f;
            }
            else if (tile._type == Tile.TileTypes.Stone)
            {
                spawn_tries = 3;
                keep_prob = 0.8f;
            }
            else if (tile._type == Tile.TileTypes.Mountain)
            {
                Debug.Log(tile._type);
                spawn_tries = 1;
                keep_prob = 1;
                max_random_offset = 0; // spawn beacon always at center of mountain :D
            }

            //if (tile._type == Tile.TileTypes.Forest || tile._type == Tile.TileTypes.Grass)
            //{

            //Debug.Log(tile._type);
                
            // apparently we need a generator for random values ? -.-
            System.Random rand = new System.Random();

            // from QA session: n times try to generate GameObject and let probability decide
            for (int i = 0; i < spawn_tries; i++)
            {
                // take random float between 0 and 1
                double rand_val = rand.NextDouble();

                if (rand_val <= keep_prob)
                {

                    // Debug.Log(rand_val);

                    // get random position for GameObject on, based on width and height of tile
                    Vector3 prop_pos = new Vector3(
                        // UnityEngine.Random.Range only works in positive range, so we have to shift afterwards to get values +- zero...
                        UnityEngine.Random.Range(0, max_random_offset) - (max_random_offset / 2f) + tile._pos.x, 
                        tile._pos.y,
                        UnityEngine.Random.Range(0, max_random_offset) - (max_random_offset / 2f) + tile._pos.z
                        );


                    // get random gameobject (via random index) from array
                    GameObject random_gameobject = tile.randomPropPrefabs[rand.Next(0, tile.randomPropPrefabs.Length)];

                    var random_prop = Instantiate(
                        random_gameobject,
                        prop_pos, 
                        Quaternion.Euler(
                            UnityEngine.Random.Range(0, random_rotation_range.x) - random_rotation_range.x / 2,
                            UnityEngine.Random.Range(0, random_rotation_range.y),
                            UnityEngine.Random.Range(0, random_rotation_range.z) - random_rotation_range.z / 2)
                        );
                        
                    // set the correspondting tile as parent (mainly to keep hierarchy nicely structured)
                    random_prop.transform.parent = tile.transform;

                    // add the instantiated propr to the List of Gameobjects on the tile (maybe we want to do something with them later)
                    // Lumberjacks could harvest trees, that slowly regrow...
                    tile._spawnedRandomGameObjects.Add(random_prop);

                }
            }
            //}
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

    // Variables needed for economy update
    public int seconds_past = 0;
    public int playerMoney = 10000;
    public List<GameObject> upkeepBuildings = new List<GameObject>();
    private int updateAt = 0;
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

        //string msg1 = "Selected building: {0}";
        //Debug.LogFormat(string.Format(msg1, _buildingPrefabs[_selectedBuildingPrefabIndex]));

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
            //check if building can be placed and then istantiate it
            GameObject target_building = _buildingPrefabs[_selectedBuildingPrefabIndex];

            if (target_building.GetComponent<Building>()._costMoney <= playerMoney &&
                target_building.GetComponent<Building>()._costPlanks <= _resourcesInWarehouse[ResourceTypes.Planks] &&
                target_building.GetComponent<Building>()._placement.Contains(clicked_tile._type))
            {
                // instantiate building from prefab
                GameObject newBuilding = Instantiate(target_building, clicked_tile._pos, Quaternion.identity);

                // rotate building so that it faces default camera viewport
                Vector3 rot_Vec = new Vector3(0, -90, 0);
                newBuilding.transform.rotation = Quaternion.Euler(rot_Vec);

                playerMoney -= newBuilding.GetComponent<Building>()._costMoney;
                _resourcesInWarehouse[ResourceTypes.Planks] -= newBuilding.GetComponent<Building>()._costPlanks;

                newBuilding.GetComponent<Building>()._tile = clicked_tile;
                //newBuilding.GetComponent<ProductionBuilding>()._efficiency = calcEfficiency(newBuilding);

                if (newBuilding.GetComponent<Building>()._type == Building.BuildingTypes.Residency)
                {
                    //spawn workers
                    //Worker worker_a = new Worker
                    //Worker worker_b = new Worker(); 
                    //newBuilding.GetComponent<HousingBuilding>()._workers.Add(worker_a);
                    //newBuilding.GetComponent<HousingBuilding>()._workers.Add(worker_b);
                }

                if (newBuilding.GetType() == typeof(ProductionBuilding))
                {
                    newBuilding.GetComponent<ProductionBuilding>().calcEfficiency(FindNeighborsOfTile(clicked_tile));
                } else
                {
                    newBuilding.GetComponent<HousingBuilding>().calcEfficiency();
                }

                upkeepBuildings.Add(newBuilding);

                // destroy random props on tile
                foreach (GameObject prop in clicked_tile._spawnedRandomGameObjects)
                {
                    Destroy(prop);
                }
            }
        }
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
            float generationInterval = building.GetComponent<Building>()._generationInterval / building.GetComponent<Building>()._efficiency;
            if (seconds_past % generationInterval <= 0.1 && updateAt != seconds_past)
            {
                // take away resource needed for production and produce only if input resource available
                if (building.GetComponent<Building>().input.Length >= 1)
                {
                    //Debug.Log("Requires input");
                    Boolean can_produce = true;
                    foreach ( ResourceTypes resource in building.GetComponent<Building>().input)
                    {
                        if (_resourcesInWarehouse[resource] <= 1)
                        {
                            can_produce = false;
                        } else
                        {
                            // TODO: maybe balance this so that we only consume on production...
                            _resourcesInWarehouse[resource] -= 1;
                        }
                    }

                    if (can_produce)
                    {
                        if (building.GetComponent<Building>()._type == Building.BuildingTypes.Residency)
                        {

                        }
                        else
                        {
                            _resourcesInWarehouse[building.GetComponent<Building>().output] += building.GetComponent<Building>()._outputCount;
                        }
                    }
                }
                else //just produce all the time
                {
                    //Debug.Log("Doesn't require input");
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

    public bool workerConsumeRandom()
    {
        var random = new System.Random();
        var consumableResoucres = new List<ResourceTypes> { ResourceTypes.Fish, ResourceTypes.Clothes, ResourceTypes.Schnapps };
        int index = random.Next(consumableResoucres.Count);

        bool could_consume = false;
        if (_resourcesInWarehouse[consumableResoucres[index]] >=1)
        {
            _resourcesInWarehouse[consumableResoucres[index]] -= 1;
            could_consume = true;
            Debug.Log("omnomnom...");
        }

        return could_consume;
       

    }

    #endregion
}
