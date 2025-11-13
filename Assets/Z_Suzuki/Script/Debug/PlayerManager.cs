using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private KeyCode LeftMoveKey = KeyCode.A;
    [SerializeField] private KeyCode RightMoveKey = KeyCode.D;

    private Movement m_playerMovement;
    private GroundChecker m_groundChecker;
    private Jump Jump;

    void Start()
    {
        m_groundChecker = GetComponent<GroundChecker>();
        m_playerMovement = GetComponent<Movement>();
        Jump = GetComponent<Jump>();

        m_playerMovement.MainspringStageUp();
    }

    void Update()
    {
        if (!m_groundChecker ||
            !m_playerMovement ||
            GameManager.Instance.GetIsPaused())
        {
            return;
        }

        m_playerMovement.SetIsGround(m_groundChecker.GetIsGround());
        
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Vector3.forward.z;
        
        m_playerMovement.Move(new Vector3(moveX, 0, moveZ));

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_playerMovement.MainspringStageUp();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            m_playerMovement.MainspringStageDown();
        }

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Jump.StartJump();
        //}
    }
}
