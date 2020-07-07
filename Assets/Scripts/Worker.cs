using UnityEngine;

public class Worker : MonoBehaviour
{
    #region Manager References
    JobManager _jobManager; //Reference to the JobManager
    GameManager _gameManager; //Reference to the GameManager
    public Building _house; //Reference to house worker lives in
    #endregion

    public float _age; // The age of this worker
    public float _happiness; // The happiness of this worker
    public int consumeRate = 15; // rate at which worker consumes resources in seconds
    public bool isRegisteredWorker = false; // worker is registered in jobmanger
    public GameObject gameObject;

    private int birthTime;  //game time when worker is created
    private int currSec = 0;  // current game time second
    private int latestUpdateAt = 0; // second at which worker was last updated
    private bool isRetired = false; // worker is too old to work
    private bool isDead = false;
    private float nurishedCounter = 0; // gets increased when random resource was not available for consumption, decreases when consume was successful. range: [0, 0.5]
    private float hasJobScore = 0; // for happiness calculation. 0 if jobless worker, +0.5 if worker has job
    private Job job;
    private Tile _currentTile;
    private bool isAtGoal = true;
    private float moveSpeed = 0.1f;



    // Start is called before the first frame update
    void Start()
    {
        _age = 0.0f;
        _happiness = 1.0f;
        birthTime = (int)Time.time;
        latestUpdateAt = birthTime; //to avoid ticks when we just created the gameobject
        
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _jobManager = GameObject.Find("JobManager").GetComponent<JobManager>();

        _gameManager.registerWorker(this); // add worker instance to list of all workers on game manager

        _currentTile = _house._tile;

    }

    // Update is called once per frame
    void Update()
    {
        currSec = (int)Time.time;
        Age();
        Consume();
        updateHappiness();

        if (hasJobScore == 0.5f)
        {
            int[,] potential_field = job._building._potentialField;
            GoToTile(potential_field);
        } else
        {
            int[,] potential_field = _house._potentialField;
            GoToTile(potential_field);
        }

        latestUpdateAt = currSec;

    }


    private void GoToTile(int[,] potentialField)
    {
        Tile bestTile = _currentTile;
        int best = potentialField[bestTile._coordinateWidth, bestTile._coordinateHeight] + 100;

        if (!isAtGoal)
        {
            foreach (Tile nTile in _currentTile._neighborTiles)
            {
                int val_on_field = potentialField[nTile._coordinateWidth, nTile._coordinateHeight];
                if (val_on_field < best)
                {
                    best = val_on_field;
                    bestTile = nTile;

                    
                }
            }

            Vector3 heading = bestTile.transform.position - gameObject.transform.position;
            heading = Vector3.Normalize(heading);


            gameObject.transform.position += heading * moveSpeed;
            gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.forward, heading);
            if (Vector3.Distance(gameObject.transform.position, bestTile.transform.position) < 0.1)
            {
                _currentTile = bestTile;
                if (best == 0)
                {
                    isAtGoal = true;
                }
            }

           
        }
    }


    private void Age()
    {
        //Implement a life cycle, where a Worker ages by 1 year every 15 real seconds.
        //When becoming of age, the worker enters the job market, and leaves it when retiring.
        //Eventually, the worker dies and leaves an empty space in his home. His Job occupation is also freed up.
        if ((currSec - birthTime) % 15 == 0 && currSec != latestUpdateAt)
        {
            _age += 1;
            //Debug.Log("I've aged!");
        }

        if (_age > 1 && !isRegisteredWorker && !isRetired)
        {
            BecomeOfAge();
            isRegisteredWorker = true;
            isAtGoal = false;

            //Debug.Log("Registering as worker");
        }

        if (_age > 3 && isRegisteredWorker && !isRetired)
        {
            Retire();
            isRetired = true;
            isRegisteredWorker = false;
            //Debug.Log("Retiring");
            isAtGoal = false; // so that worker moves back to home...
        }

        if (_age > 6 && !isDead)
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
                if (nurishedCounter > 0.0) // cap rang to [0, 0.5]
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

    public void assignJob(Job job)
    {
        this.job = job;
        hasJobScore = 0.5f;
    }

    public void removeJob()
    {
        hasJobScore = 0.0f;
    }


    public void BecomeOfAge()
    {
        _jobManager.RegisterWorker(this);
    }

    private void Retire()
    {   
        if (isRegisteredWorker)
        {
            _jobManager.RemoveWorker(this);  // means person will no longer look for job

            job.RemoveWorker(this); //remove worker from the job he was at
            _jobManager._availableJobs.Add(job); // make job available again for new workers on jobmanager
        }
    }

    private void Die()
    {
        //_house._workers.Remove(this);
        //_house._workers.RemoveAt(workerIndex);
        _house.WorkerRemovedFromBuilding(this);
        _gameManager.removeWorker(this); // remove worker from list of all workers in game manager

        Destroy(this.gameObject);
        isDead = true;
    }
}
