using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine.UI;

namespace AH2736
{
   // Used ObjectiveReachPoint script as basis for the new objective 
    
    // Script requires a collider to work
    [RequireComponent(typeof(Collider))]

    public class ObjectiveHoldArea : Objective
    {
       // Variables
        [Tooltip("Visible transform that will be destroyed once the objective is completed")]
        public Transform m_destroyRoot;

        [Tooltip("Image component displaying time remaining")]
        public Image m_holdTimeImage;

        [Tooltip("Floating time bar pivot transform")]
        public Transform m_holdTimeBarPivot;

        [Tooltip("Whether the bar is visible at full time")]
        public bool m_hideHoldTimeBar = true;

        [Tooltip("Time needed to hold")]
        [SerializeField] private float m_holdTimeRequired = 10f;
        private float m_elapsedTime;
        private bool m_hasStarted = false;

        [Tooltip("Whether the timer resets upon leaving and returning to the area")]
        [SerializeField] private bool m_holdTimeReset = false;

        

        void Awake()
        {
            // If there is no marker, get one
            if (m_destroyRoot == null)
                m_destroyRoot = transform;
        }

        void OnTriggerEnter(Collider other)
        {
            // Return void if the objective is completed
            if (IsCompleted)
                return;

            // Initialize player variable from the PlayerCharacterController
            var player = other.GetComponent<PlayerCharacterController>();
            // test if the other collider contains a PlayerCharacterController, then complete
            if (player != null)
            {
                if (!m_hasStarted)
                {
                    // Start counter
                    m_elapsedTime = 0f;
                    m_hasStarted = true;
                }
                
                // Log event
                Debug.Log("Hold area entered");
            }
        }

        private void OnTriggerStay(Collider other)
        {
            // Count up as long as player remains in trigger
            m_elapsedTime += Time.deltaTime;

            // When the counter is full
            if (m_elapsedTime >= m_holdTimeRequired)
            {
                // Notify for debug
                Debug.Log("Hold Timer done");
                
                // Complete objective
                CompleteObjective(string.Empty, string.Empty, "Objective complete : " + Title);

                // Destroy the transform and remove compass marker
                Destroy(m_destroyRoot.gameObject);    
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // + If timer resets, reset the timer
            
            // Initialize player variable from the PlayerCharacterController
            var player = other.GetComponent<PlayerCharacterController>();
            // test if the other collider contains a PlayerCharacterController, then complete
            if (player != null && m_holdTimeReset == true)
            {
                m_elapsedTime = 0f;
            }

            // Log event
            Debug.Log("Hold area exited");

        }

        void Update()
        {
            // Update hold time value
            m_holdTimeImage.fillAmount = (m_holdTimeRequired - m_elapsedTime) / m_holdTimeRequired;

            // Rotate hold time bar to face the camera/player
            m_holdTimeBarPivot.LookAt(Camera.main.transform.position);

            // Hide bar if needed
            if (m_hideHoldTimeBar)
                m_holdTimeBarPivot.gameObject.SetActive(m_holdTimeImage.fillAmount != 1);
        }


    }
}

