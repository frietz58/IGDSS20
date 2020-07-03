using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{

    public List<Job> _availableJobs = new List<Job>();
    public List<Worker> _unoccupiedWorkers = new List<Worker>();



    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleUnoccupiedWorkers();
    }
    #endregion


    #region Methods

    private void HandleUnoccupiedWorkers()
    {
        if (_unoccupiedWorkers.Count > 0)
        {
            List<Worker> nowOccupied = new List<Worker>();

            foreach (Worker unoccupiedWorker in _unoccupiedWorkers)
            {
                var random = new System.Random();
                if (_availableJobs.Count > 0)
                {
                    int index = random.Next(_availableJobs.Count);
                    Job randomJob = _availableJobs[index];

                    randomJob.AssignWorker(unoccupiedWorker);
                    unoccupiedWorker.assignJob(randomJob);
                    _availableJobs.Remove(randomJob);

                    nowOccupied.Add(unoccupiedWorker); // only remove from list we are iterating over after iteration...
                }
            }

            // now remove all workers that have been assgned a job from unassigned worker list
            foreach (Worker w in nowOccupied)
            {
                _unoccupiedWorkers.Remove(w);
            }
        }
    }

    public void RegisterWorker(Worker w)
    {
        _unoccupiedWorkers.Add(w);
    }



    public void RemoveWorker(Worker w)
    {
        _unoccupiedWorkers.Remove(w);
        w.removeJob();
    }

    #endregion
}
