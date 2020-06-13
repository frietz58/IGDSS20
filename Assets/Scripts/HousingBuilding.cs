using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingBuilding : Building
{

    public int capacity = 10; //number of works that can live at building


    // Start is called before the first frame update
    void Start()
    {
        generateWorker();
        generateWorker();
    }

    // Update is called once per frame
    void Update()
    {
        calcEfficiency();
    }

    public void calcEfficiency()
    {
        float efficiency = 0.0f;
        for (int i = 0; i < _workers.Count; i++)
        {
            efficiency += _workers[i]._happiness;
        }

        _efficiency = efficiency / _workers.Count;
        Debug.Log(_efficiency);

    }

    public void generateWorker()
    {
        Worker worker = new Worker();
        GameObject prefab = GameObject.Find("GameManager").GetComponent<GameManager>().workerPrefab;
        var workerPrefab = Instantiate(prefab, _tile.transform.position, Quaternion.identity);
        _workers.Add(worker);
    }



}
