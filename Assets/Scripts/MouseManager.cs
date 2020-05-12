using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        pitch = transform.eulerAngles[0];
        yaw = transform.eulerAngles[1];
    }

    // Update is called once per frame
    void Update()
    {

        // get values from game gamemanger that were not available at start time...
        if (maxX == 0.0f)
        {
            maxX = gameManager.lastRowPos + boundaryConstant;
            minX = gameManager.firstRowPos - boundaryConstant;
            maxZ = gameManager.lastColPos + boundaryConstant;
            minZ = gameManager.firstColPos - boundaryConstant;
            minY = 90f;
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
            zoomCamera();
        }

        if (Input.GetMouseButton(2))
        {
            // rotate camera
            transform.eulerAngles = rotateCamera();
        }

        if (Input.GetMouseButtonDown(0))
        {
            // output name of object on left mouse click
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
            {
				Debug.Log(hit.collider.gameObject.name);
			}
		}
    }

    Vector3 moveCamera()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSpeed;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSpeed;

        return new Vector3(
                Mathf.Clamp(transform.position.x + mouseY, minX, maxX),
                transform.position.y,
                Mathf.Clamp(transform.position.z - mouseX, minZ, maxZ)
        );
    }

    Vector3 rotateCamera()
    {
        yaw -= rotHSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
        pitch += rotVSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;

        return new Vector3(pitch, yaw, 0.0f);
    }

    void zoomCamera()
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

        Camera.main.transform.position = new Vector3(x_clamped, y_clamped, z_clamped);        

    }
}
