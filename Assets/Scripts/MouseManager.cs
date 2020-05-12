using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public GameManager gameManager;
    public float mouseSpeed = 150.0f;
    public float scrollSpeed = 10.0f;
    float boundaryConstant = 50.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
        float maxX = gameManager.lastRowPos + boundaryConstant;
        float minX = gameManager.firstRowPos - boundaryConstant;
        float maxZ = gameManager.lastColPos + boundaryConstant;
        float minZ = gameManager.firstColPos - boundaryConstant;
        return new Vector3(
                Mathf.Clamp(transform.position.x + mouseY, minX, maxX),
                transform.position.y,
                Mathf.Clamp(transform.position.z - mouseX, minZ, maxZ)
        );
    }

    void zoomCamera()
    {
        float fov = Camera.main.fieldOfView;
        fov -= Input.GetAxisRaw("Mouse ScrollWheel") * scrollSpeed;
        float minFov = 15;
        float maxFov = 80;
        fov = Mathf.Clamp(fov, minFov, maxFov);
        Camera.main.fieldOfView = fov;
    }
}
