using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField, Range(0.1f, 10.0f)] float CameraSpeed = 10.0f;
    [SerializeField, Range(100.0f, 1000.0f)] float MouseSensitivity = 100.0f;


    private KeyCode _cameraMoveKey = KeyCode.Mouse1;
    private KeyCode _forwardMoveKey = KeyCode.W;
    private KeyCode _backMoveKey = KeyCode.S;
    private KeyCode _leftMoveKey = KeyCode.A;
    private KeyCode _rightMoveKey = KeyCode.D;
    private KeyCode _upMoveKey = KeyCode.E;
    private KeyCode _downMoveKey = KeyCode.Q;


    void Update()
    {
        if (GameManager.Instance.GetIsPaused())
        {
            return;
        }

        if (Input.GetKey(_cameraMoveKey))
        {
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            x *= MouseSensitivity * Time.deltaTime;
            y *= MouseSensitivity * Time.deltaTime;
            transform.RotateAround(transform.position, Vector3.up, x);
            transform.RotateAround(transform.position, transform.right, -y);
            Vector3 moveDir = new Vector3(0, 0, 0);
            if (Input.GetKey(_forwardMoveKey))
            {
                moveDir += new Vector3(0, 0, 1);
            }
            if (Input.GetKey(_backMoveKey))
            {
                moveDir += new Vector3(0, 0, -1);
            }
            if (Input.GetKey(_leftMoveKey))
            {
                moveDir += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey(_rightMoveKey))
            {
                moveDir += new Vector3(1, 0, 0);
            }
            if (Input.GetKey(_upMoveKey))
            {
                moveDir += new Vector3(0, 1, 0);
            }
            if (Input.GetKey(_downMoveKey))
            {
                moveDir += new Vector3(0, -1, 0);
            }
            transform.Translate(moveDir * CameraSpeed * Time.deltaTime * 10);
        }
    }
}
