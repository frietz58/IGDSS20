using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class HousingBuilding : Building
{

    public int capacity = 10; //number of works that can live at building
    public int generationInterval = 10;
    private int birthTime;  //game time when house is created
    private int currSec = 0;  // current game time second
    private int latestUpdateAt = 0; // second at which house was last updated


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        spawnWorker();
        spawnWorker();
        birthTime = (int)Time.time;
        latestUpdateAt = birthTime; //to avoid ticks when we just created the gameobject
    }

    // Update is called once per frame
    void Update()
    {
        currSec = (int)Time.time;
        // calcEfficiency();
        generateWorkers();
        latestUpdateAt = currSec;
    }

    public override void calcEfficiency()  // override empty method from base class...
    {
        float efficiency = 0.0f;
        int workerCounter = 0;

        for (int i = 0; i < _workers.Count; i++)
        {
            if (_workers[i].isRegisteredWorker) // don't consider children and elderly for efficency calculation...
            {
                efficiency += _workers[i]._happiness;
                workerCounter += 1;
            }
            
        }
        if (workerCounter != 0)  // if there are no people in the house, avoid that efficiency becomes NaN due to zero division
        {
            _efficiency = efficiency / workerCounter;
        } else
        {
            _efficiency = 1;
        }
        
        // Debug.Log(_efficiency);

    }

    private void spawnWorker()
    {
        // Worker worker = new Worker();
        GameObject prefab = GameObject.Find("GameManager").GetComponent<GameManager>().workerPrefab;
        var workerGameObject = Instantiate(prefab, _tile.transform.position, Quaternion.identity);
        Worker workerInstance = workerGameObject.GetComponent<Worker>();
        workerInstance.gameObject = workerGameObject;
        workerInstance._house = this;

        _workers.Add(workerInstance);

    }

    private void generateWorkers()
    {
        float actualGenerationInterval = generationInterval / _efficiency;
        if ((currSec - birthTime) % actualGenerationInterval == 0 && currSec != latestUpdateAt)
        {
            if (capacity > _workers.Count)
            {
                spawnWorker();
            }
        }
    }



}
