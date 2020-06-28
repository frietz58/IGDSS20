using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBuilding : Building
{
    JobManager _jobManager; //Reference to the JobManager
    GameManager _gameManager; //Reference to the GameManager



    #region Attributes
    public bool _scaleWithNeighbors; // Boolean whether the efficiency of the building depends on neighboring tiles
    public int _maxNeighbors; // Number of neighboring tiles of a type needed to maximize efficiency
    private int num_jobs;
    #endregion

    protected override void Start()
    {

        base.Start();
        
        _jobManager = GameObject.Find("JobManager").GetComponent<JobManager>();
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        _jobs = new List<Job>();
        num_jobs = setNumJobs(); // set number of available jobs depending on type of building

        for (int i=0; i<num_jobs; i++)
        {
            // instantiate job objects
            Job job = new Job(this);
            _jobs.Add(job);
            _jobManager._availableJobs.Add(job);
        }
    }



    public override void calcEfficiency()
    {
        List<Tile> neighbors = _tile._neighborTiles;

        // first we check the neighborhood of the tile for efficency 
        if (!_scaleWithNeighbors)
        {
            _efficiency = 1.0f;
        } else
        {
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

            //then, we further adjust the efficency considering the workers
            if (_workers.Count > 1)
            {
                efficency = efficency * _workers.Count / num_jobs;

                float happinessFactor = 0.0f;
                foreach (Worker w in _workers)
                {
                    happinessFactor += w._happiness;
                }
                happinessFactor = happinessFactor / _workers.Count;
            }
            else
            {
                efficency = 0; // zero if there are no people working there...
            }

            

            _efficiency = efficency;


        }

        
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

    private int setNumJobs()
    {
        int num = 0;
        if (_type == BuildingTypes.Fishery)
        {
            num = 25;
        } else if (_type == BuildingTypes.Lumberjack)
        {
            num = 5;
        } else if (_type == BuildingTypes.Sawmill)
        {
            num = 10;
        } else if (_type == BuildingTypes.SheepFarm)
        {
            num = 10;
        } else if (_type == BuildingTypes.Knitters)
        {
            num = 50;
        } else if (_type == BuildingTypes.PotatoFarm)
        {
            num = 20;
        } else if (_type == BuildingTypes.Distillery)
        {
            num = 50;
        }
        return num;
    }

    //This class acts as a data container and has no functionality
}
