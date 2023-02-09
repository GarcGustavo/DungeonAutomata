using DungeonAutomata._Project.Scripts.DialogueSystem;
using DungeonAutomata._Project.Scripts.DialogueSystem.Interfaces;
using DungeonAutomata._Project.Scripts.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonAutomata._Project.Scripts.Controllers
{
    [RequireComponent(typeof(Rigidbody))]
    public class FPController : MonoBehaviour
    {
        private DialogueManager _dialogueManager;
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
            _dialogueManager = DialogueManager.Instance;
            interactionImage.enabled = false;
            _cameraTransform = _camera.transform;
            DialoguePanel.OnDialogueEnd += () => _interacting = false;
        }

        // Update is called once per frame
        public void FixedUpdate()
        {
            //move the actual player here
            if (_interacting)
                return;
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
                //negate this value so it rotates like an FPS not like a plane
                _camera.transform.Rotate(-m_cameraRotation);
            }
        }
    
        public void Update()
        {
            ReceiveInput();
            if (!_interacting)
            {
                CheckInteraction();
            }
            InternalLockUpdate();
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
    
        private void CheckInteraction()
        {
            var position = _cameraTransform.position;
            var forward = _cameraTransform.forward;
            var actor = Utils.CastInteractionRay(position, forward, interactableLayerMask);
            Debug.DrawLine(position, position + forward * 100, Color.red);
            RaycastHit hit;
            //if (Physics.Raycast(ray, out hit, interactableLayerMask))
            if (actor != null && !_interacting)
            {
                //var hitObject = hit.transform.gameObject;
                //var actor = hitObject.GetComponent<IActor>();
                Debug.DrawLine(position, position + forward * 100, Color.green);
                interactionImage.enabled = true;
                _currentActor = actor;
            }
            else
            {
                interactionImage.enabled = false;
            }
        }
    
        private void ReceiveInput()
        {
            m_MovX = Input.GetAxis("Horizontal");
            m_MovY = Input.GetAxis("Vertical");

            m_moveHorizontal = transform.right * m_MovX;
            m_movVertical = transform.forward * m_MovY;

            m_velocity = (m_moveHorizontal + m_movVertical).normalized * speed;

            //mouse movement 
            m_yRot = Input.GetAxisRaw("Mouse X");
            m_xRot = Input.GetAxisRaw("Mouse Y");
            m_rotation = new Vector3(0, m_yRot, 0) * m_lookSensitivity;
            m_cameraRotation = new Vector3(m_xRot, 0, 0) * m_lookSensitivity;
        
            if (Input.GetKeyDown(KeyCode.E) && _interacting)
            {
                Debug.Log("E pressed");
                _dialogueManager.TriggerNextLine();
            }
            else if (Input.GetKeyDown(KeyCode.E) && !_interacting && _currentActor != null)
            {
                _currentActor.TriggerDialogue();
                _interacting = true;
                interactionImage.enabled = false;
            }
        }

    }
}
