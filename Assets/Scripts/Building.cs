using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Building : MonoBehaviour
{

    public BuildingTypes _type; //The type of the building
    public Tile _tile; // The tile the building is placed on
    public Tile.TileTypes[] _placement; // The types of tiles the building can be built on
    public float _efficiency; // A float in [0,1] that represents the efficiency of the building
    public float _generationInterval; // The seconds interval for the building to produce resources
    public int _costMoney; // The initial cost of the building in money
    public int _costPlanks; // The initial cost of the building in planks
    public int _upkeep; // The cost for upkeep of the building per economy tick
    public int _outputCount; // How many resources are output in one generation interval
    // public GameManager.ResourceTypes input; // Resource type of input resources needed
    public GameManager.ResourceTypes[] input; // Resource type of input resources needed
    public GameManager.ResourceTypes output; // Resource type of output resources



    #region Manager References
    //public JobManager _jobManager; //Reference to the JobManager
    //public GameManager _gameManager; //Reference to the GameManager
    #endregion

    #region Workers
    public List<Worker> _workers; //List of all workers associated with this building, either for work or living
    #endregion

    #region Jobs
    public List<Job> _jobs; // List of all available Jobs. Is populated in Start()
    #endregion


    #region Enumerations
    //Enumeration of all available building types. Can be addressed from other scripts by calling Building.BuildingTypes
    public enum BuildingTypes { Empty, Fishery, Lumberjack, Sawmill, SheepFarm, Knitters, PotatoFarm, Distillery, Residency };
    #endregion

    #region Methods   

    private void Update()
    {
        calcEfficiency();
    }

    public void WorkerAssignedToBuilding(Worker w)
    {
        _workers.Add(w);
    }

    public void WorkerRemovedFromBuilding(Worker w)
    {
        _workers.Remove(w);
    }

    public virtual void calcEfficiency()
    {
        // just here for the override, see Housing and ProductionBuilding^^
    }
    #endregion
}
