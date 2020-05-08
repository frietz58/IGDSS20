using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public GameManager gameManager;
    public float mouseSpeed = 150.0f;
    public float scrollSpeed = 50.0f;
    float boundaryConstant = 160.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSpeed;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSpeed;        

        if (Input.GetMouseButton(1))
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x + mouseY, gameManager.smallestRowPos - boundaryConstant, gameManager.largestRowPos + boundaryConstant),
                transform.position.y,
                Mathf.Clamp(transform.position.z - mouseX, gameManager.smallestColPos - boundaryConstant, gameManager.largestColPos + boundaryConstant)
            );
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            transform.position += scrollSpeed * new Vector3(0, -Input.GetAxisRaw("Mouse ScrollWheel"), 0);
        }
    }
}
