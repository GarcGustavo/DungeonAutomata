using System;
using System.Collections;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class FPController : MonoBehaviour
{
    private IActor _currentActor;
    private float speed = 1.0f;
    private float m_MovX;
    private float m_MovY;
    private Vector3 m_moveHorizontal;
    private Vector3 m_movVertical;
    private Vector3 m_velocity;
    private Rigidbody m_Rigid;
    private float m_yRot;
    private float m_xRot;
    private Vector3 m_rotation;
    private Vector3 m_cameraRotation;
    private float m_lookSensitivity = 1.0f;
    private bool m_cursorIsLocked = true;
    private bool _interacting = false;
    public LayerMask interactableLayerMask = 3;
    [SerializeField] private Image interactionImage;

    [Header("The Camera the player looks through")]
    private Camera _camera;
    private Transform _cameraTransform;

    private void Awake()
    {
        m_Rigid = GetComponent<Rigidbody>();
        _camera = Camera.main;
    }

    private void Start()
    {
        interactionImage.enabled = false;
        _cameraTransform = _camera.transform;
    }

    // Update is called once per frame
    public void FixedUpdate()
    {

        m_MovX = Input.GetAxis("Horizontal");
        m_MovY = Input.GetAxis("Vertical");

        m_moveHorizontal = transform.right * m_MovX;
        m_movVertical = transform.forward * m_MovY;

        m_velocity = (m_moveHorizontal + m_movVertical).normalized * speed;

        //mouse movement 
        m_yRot = Input.GetAxisRaw("Mouse X");
        m_rotation = new Vector3(0, m_yRot, 0) * m_lookSensitivity;

        m_xRot = Input.GetAxisRaw("Mouse Y");
        m_cameraRotation = new Vector3(m_xRot, 0, 0) * m_lookSensitivity;

        //move the actual player here
        if (m_velocity != Vector3.zero)
        {
            m_Rigid.MovePosition(m_Rigid.position + m_velocity * Time.fixedDeltaTime);
        }

        if (m_rotation != Vector3.zero)
        {
            //rotate the camera of the player
            m_Rigid.MoveRotation(m_Rigid.rotation * Quaternion.Euler(m_rotation));
        }

        if (_camera != null)
        {
            //negate this value so it rotates like a FPS not like a plane
            _camera.transform.Rotate(-m_cameraRotation);
        }

        InternalLockUpdate();
        CastInteractionRay();
    }

    //controls the locking and unlocking of the mouse
    private void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            m_cursorIsLocked = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_cursorIsLocked = true;
        }

        if (m_cursorIsLocked)
        {
            UnlockCursor();
        }
        else if (!m_cursorIsLocked)
        {
            LockCursor();
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void CastInteractionRay()
    {
        // Get the mouse position in screen space
        Vector3 mousePos = Input.mousePosition;
        
        // Convert the mouse position to world space
        //Ray ray = Camera.main.ScreenPointToRay(mousePos);
        //Ray ray = _camera.ViewportPointToRay(mousePos);
        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.red);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactableLayerMask))
        {
            var hitObject = hit.transform.gameObject;
            var actor = hitObject.GetComponent<IActor>();
            if (actor != null)
            {
                Debug.Log("Hit an actor");
                interactionImage.enabled = true;
                if (Input.GetKeyDown(KeyCode.E))
                {
                    actor.TriggerDialogue();
                    _currentActor = actor;
                    _interacting = true;
                }
            }
        }
        else
        {
            interactionImage.enabled = false;
        }
    }
    
    private void ReceiveInput()
    {
        
        if (Input.GetKeyDown(KeyCode.E) && !interactionImage.enabled)
        {
            Debug.Log("E pressed");
        }
        if (Input.GetKeyDown(KeyCode.E) && _interacting)
        {
            Debug.Log("E released");
            //TODO: Invoke dialogue input event
            _interacting = false;
        }
    }

}
