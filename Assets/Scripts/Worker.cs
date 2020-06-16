using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : MonoBehaviour
{
    #region Manager References
    JobManager _jobManager; //Reference to the JobManager
    GameManager _gameManager; //Reference to the GameManager
    public HousingBuilding _house; //Reference to house worker lives in
    #endregion

    public float _age; // The age of this worker
    public float _happiness; // The happiness of this worker
    public int consumeRate = 15; // rate at which worker consumes resources in seconds
    public bool isRegisteredWorker = false; // worker is registered in jobmanger
    public int workerIndex; // the index in the list of workers of the building the worker lives at

    private int birthTime;  //game time when worker is created
    private int currSec = 0;  // current game time second
    private int latestUpdateAt = 0; // second at which worker was last updated
    private bool isRetired = false; // worker is too old to work
    private bool isDead = false;
    private float nurishedCounter = 0; // gets increased when random resource was not available for consumption, decreases when consume was successful. range: [-0.5, 0.5]
    private float hasJobScore = 0; // for happiness calculation. -0.5 if jobless worker, +0.5 if worker has job

    // Start is called before the first frame update
    void Start()
    {
        _age = 0.0f;
        _happiness = 1.0f;
        birthTime = (int)Time.time;
        latestUpdateAt = birthTime; //to avoid ticks when we just created the gameobject
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _jobManager = GameObject.Find("JobManager").GetComponent<JobManager>();
    }

    // Update is called once per frame
    void Update()
    {
        currSec = (int)Time.time;
        Age();
        Consume();
        updateHappiness();
        latestUpdateAt = currSec;

    }


    private void Age()
    {
        //TODO: Implement a life cycle, where a Worker ages by 1 year every 15 real seconds.
        //When becoming of age, the worker enters the job market, and leaves it when retiring.
        //Eventually, the worker dies and leaves an empty space in his home. His Job occupation is also freed up.
        if ((currSec - birthTime) % 15 == 0 && currSec != latestUpdateAt)
        {
            _age += 1;
            //Debug.Log("I've aged!");
        }

        if (_age > 3 && !isRegisteredWorker && !isRetired)
        {
            BecomeOfAge();
            isRegisteredWorker = true;
            Debug.Log("Registering as worker");
        }

        if (_age > 5 && isRegisteredWorker && !isRetired)
        {
            Retire();
            isRetired = true;
            isRegisteredWorker = false;
            Debug.Log("Retiring");
        }

        if (_age > 7 && !isDead)
        {

            Die();
        }
    }
    // at a fixed rate, consume a random resource from the warehouse.
    // if worker couldn't consume the resource, update variable used for happiness calcualtion.
    private void Consume()
    {
        if ((currSec - birthTime) % consumeRate == 0 && currSec != latestUpdateAt)
        {
            bool could_consume = _gameManager.workerConsumeRandom();

            //update nurishedCounter depending on whether we could consume a resource or not...
            if (!could_consume)
            {
                if (nurishedCounter > -0.5f) // cap rang to [-0.5, 0.5]
                {
                    nurishedCounter -= 0.1f;
                }
                
            } else
            {
                if (nurishedCounter < 0.5f)
                {
                    nurishedCounter += 0.1f;
                }
            }
        }
    }

    private void updateHappiness()
    {
        _happiness = nurishedCounter + hasJobScore;
        //Debug.Log(_happiness);
    }

    public void setJobScore(int score)
    {
        hasJobScore = score;
    }


    public void BecomeOfAge()
    {
        _jobManager.RegisterWorker(this);
    }

    private void Retire()
    {   
        if (isRegisteredWorker)
        {
            _jobManager.RemoveWorker(this);

        }
    }

    private void Die()
    {
        //_house._workers.Remove(this);
        //_house._workers.RemoveAt(workerIndex);
        _house.WorkerRemovedFromBuilding(this);
        Destroy(this.gameObject);
        isDead = true;
    }
}
