using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    #region Attributes
    public GameManager gameManager;
    public float mouseSpeed = 150.0f;
    public float scrollSpeed = 10.0f;

    float boundaryConstant = 50.0f;

    // params for rotation speed, would make sense to adap those based on how far zoomed in we are...
    public float rotHSpeed = 10.0f;
    public float rotVSpeed = 10.0f;

    // set in start, yaw and pitch of main camera
    float yaw = 0.0f;
    float pitch = 0.0f;

    // min and max values for camera coordinates that are checked in camera move, rotate and zoom
    float maxX = 0.0f;
    float minX = 0.0f;
    float minY = 0.0f;
    float maxY = 0.0f;
    float maxZ = 0.0f;
    float minZ = 0.0f;
    #endregion

    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {
        pitch = transform.eulerAngles[0];
        yaw = transform.eulerAngles[1];
    }

    // Update is called once per frame
    void Update()
    {
        bool verbose = false;
        // get values from game gamemanger that were not available at start time...
        if (maxX == 0.0f)
        {
            maxX = gameManager.lastRowPos + boundaryConstant;
            minX = gameManager.firstRowPos - boundaryConstant;
            maxZ = gameManager.lastColPos + boundaryConstant;
            minZ = gameManager.firstColPos - boundaryConstant;
            minY = 20f;
            maxY = 250f;
        }

        if (Input.GetMouseButton(1))
        {
            // move on xz plane on right mouse click
            transform.position = moveCamera();

        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            // zoom on scroll wheel
            Camera.main.transform.position = zoomCamera();
        }

        if (Input.GetMouseButton(2))
        {
            // rotate camera on wheel down
            transform.eulerAngles = rotateCamera();
        }

        if (Input.GetMouseButtonDown(0))
        {
            // output name of object on left mouse click
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (verbose)
                {
                    string msg0 = "Own type: {0}, own index: [{1}, {2}]";
                    Debug.LogFormat(msg0, hit.collider.GetComponent<Tile>()._type, hit.collider.GetComponent<Tile>()._coordinateWidth, hit.collider.GetComponent<Tile>()._coordinateHeight);
                }
                
                foreach (Tile t in hit.collider.GetComponent<Tile>()._neighborTiles)
                {
                    if (verbose)
                    {
                        string msg1 = "Neighbor Type: {0}, index: [{1}, {2}]";
                        Debug.LogFormat(msg1, t._type, t._coordinateWidth, t._coordinateHeight);
                    }

                    var highlighter = t.GetComponent<HighlightObject>();
                    highlighter.timedHighlight();
                }

                gameManager.TileClicked(hit.collider.GetComponent<Tile>()._coordinateHeight, hit.collider.GetComponent<Tile>()._coordinateWidth);

            }
        }
    }
    #endregion

    #region Methods
    Vector3 moveCamera()
    {

        // get current camera forward and rightway vector
        Vector3 camF = Camera.main.transform.forward;
        Vector3 camR = Camera.main.transform.right;

        camF.y = 0;
        camR.y = 0;

        camF = camF.normalized;
        camR = camR.normalized;

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSpeed;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSpeed;

        //return new Vector3(
        //        Mathf.Clamp(transform.position.x + mouseY, minX, maxX),
        //        transform.position.y,
        //        Mathf.Clamp(transform.position.z - mouseX, minZ, maxZ)
        //);

        Vector3 camPos = transform.position;

        // add mouseY & mouseX to current rotation of camera, instead of world coordinates...
        camPos -= camF * mouseY + camR * mouseX;

        camPos.x = Mathf.Clamp(camPos.x, minX, maxX);
        camPos.z = Mathf.Clamp(camPos.z, minZ, maxZ);

        return camPos;

    }

    Vector3 rotateCamera()
    {
        yaw -= rotHSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
        pitch += rotVSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;

        return new Vector3(pitch, yaw, 0.0f);
    }

    Vector3 zoomCamera()
    {
        //float fov = Camera.main.fieldOfView;
        //fov -= Input.GetAxisRaw("Mouse ScrollWheel") * scrollSpeed;
        //float minFov = 15;
        //float maxFov = 80;
        //fov = Mathf.Clamp(fov, minFov, maxFov);
        //Camera.main.fieldOfView = fov;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float zoomDistance = scrollSpeed * Input.GetAxisRaw("Mouse ScrollWheel") * Time.deltaTime;

        Transform new_cam_pos = Camera.main.transform;
        new_cam_pos.Translate(ray.direction * zoomDistance, Space.World);
        float x_clamped = Mathf.Clamp(new_cam_pos.position.x, minX, maxX);
        float y_clamped = Mathf.Clamp(new_cam_pos.position.y, minY, maxY);
        float z_clamped = Mathf.Clamp(new_cam_pos.position.z, minZ, maxZ);

        return new Vector3(x_clamped, y_clamped, z_clamped);

    }
    #endregion
}
