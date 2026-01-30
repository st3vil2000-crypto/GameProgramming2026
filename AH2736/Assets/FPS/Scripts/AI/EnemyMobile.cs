using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyMobile : MonoBehaviour
    {
        
        // Add AI State: Cohesion.
        // Enemies act like electric charges according to type
        // - SmallBots are attracted to BigBots and repelled by SmallBots
        // - vice versa for BigBots
        public enum AIState
        {
            Cohesion,
            Patrol,
            Follow,
            Attack,
        }

        public Animator Animator;

        [Tooltip("Fraction of the enemy's attack range at which it will stop moving towards target while attacking")]
        [Range(0f, 1f)]
        public float AttackStopDistanceRatio = 0.5f;

        [Tooltip("The random hit damage effects")]
        public ParticleSystem[] RandomHitSparks;

        public ParticleSystem[] OnDetectVfx;
        public AudioClip OnDetectSfx;

        [Header("Sound")] public AudioClip MovementSound;
        public MinMaxFloat PitchDistortionMovementSpeed;

        public AIState AiState { get; private set; }
        EnemyController m_EnemyController;
        AudioSource m_AudioSource;

        const string k_AnimMoveSpeedParameter = "MoveSpeed";
        const string k_AnimAttackParameter = "Attack";
        const string k_AnimAlertedParameter = "Alerted";
        const string k_AnimOnDamagedParameter = "OnDamaged";


        // Cohesion Behaviour Variables and Methods
        [Header("Cohesion")]
        [SerializeField] public float m_scanRadius = 5f;
        [SerializeField] public LayerMask m_ScanLayer; // Maybe not serialized
        [SerializeField] public int m_CohesionCharge = 0;

        public Collider[] m_scanResults = new Collider[10]; // Nearby allies to dictate forces


        // Each frame, enemies scan for allies within a radius. 
        // Allies exert movement forces, which are resolved as a movement vector
        // Sets movement target to outcome
        // < Maybe so that scans only happen after reaching destination >
        // < or maybe not - whichever is cooler/faster >
        public Vector3 CohesionUpdate()
        {
            int nearbyNeighbours = Physics.OverlapSphereNonAlloc(
                transform.position,
                m_scanRadius,
                m_scanResults,
                m_ScanLayer
                );

            if (nearbyNeighbours > 0)
            { 
                Vector3 steeringForce = CalculateCohesionForce( nearbyNeighbours );
                return steeringForce;
            } 
            else
            {
                return Vector3.zero;
                // find nearest neighbours of each type, then calculate cohesionForce
            }

            

        }

        // Moves through results of scan (nearby allies)
        // For each object in the scan, calculate 'force' exerted on this object
        public Vector3 CalculateCohesionForce(int count)
        {
            Vector3 totalForce = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                GameObject neighbour = m_scanResults[i].gameObject;

                // Ignore self
                if (neighbour == this.gameObject) continue;

                // Get details of target
                if (neighbour.TryGetComponent<EnemyMobile>(out EnemyMobile stats))
                {
                    // Charge Variable
                    float neighbourCharge = stats.m_CohesionCharge;

                    // Distance
                    Vector3 diff = transform.position - neighbour.transform.position;
                    float distance = diff.magnitude;

                    // Vector addition
                    // Cohesion rules follow inverse square law
                    if (distance > 0)
                    {
                        float forceMagnitude = (m_CohesionCharge * neighbourCharge) / (distance * distance);
                        totalForce += diff.normalized * forceMagnitude;
                    }
                }
            }

            return totalForce;
        }

        void Start()
        {
            m_EnemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyMobile>(m_EnemyController, this,
                gameObject);

            m_EnemyController.onAttack += OnAttack;
            m_EnemyController.onDetectedTarget += OnDetectedTarget;
            m_EnemyController.onLostTarget += OnLostTarget;
            m_EnemyController.SetPathDestinationToClosestNode();
            m_EnemyController.onDamaged += OnDamaged;

            // Start patrolling
            //AiState = AIState.Patrol;
            AiState = AIState.Cohesion;

            // adding a audio source to play the movement sound on it
            m_AudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, EnemyMobile>(m_AudioSource, this, gameObject);
            m_AudioSource.clip = MovementSound;
            m_AudioSource.Play();
        }

        void Update()
        {
            UpdateAiStateTransitions();
            UpdateCurrentAiState();

            float moveSpeed = m_EnemyController.NavMeshAgent.velocity.magnitude;

            // Update animator speed parameter
            Animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);

            // changing the pitch of the movement sound depending on the movement speed
            m_AudioSource.pitch = Mathf.Lerp(PitchDistortionMovementSpeed.Min, PitchDistortionMovementSpeed.Max,
                moveSpeed / m_EnemyController.NavMeshAgent.speed);
        }

        void UpdateAiStateTransitions()
        {
            // Handle transitions 
            switch (AiState)
            {
                case AIState.Follow:
                    // Transition to attack when there is a line of sight to the target
                    if (m_EnemyController.IsSeeingTarget && m_EnemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Attack;
                        m_EnemyController.SetNavDestination(transform.position);
                    }

                    break;
                case AIState.Attack:
                    // Transition to follow when no longer a target in attack range
                    if (!m_EnemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Follow;
                    }

                    break;
            }
        }

        void UpdateCurrentAiState()
        {
            // Handle logic 
            switch (AiState)
            {
                case AIState.Cohesion:
                    Vector3 steeringForce = CohesionUpdate();
                    m_EnemyController.SetNavDestination(steeringForce);
                    break;
                case AIState.Patrol:
                    m_EnemyController.UpdatePathDestination();
                    m_EnemyController.SetNavDestination(m_EnemyController.GetDestinationOnPath());
                    break;
                case AIState.Follow:
                    m_EnemyController.SetNavDestination(m_EnemyController.KnownDetectedTarget.transform.position);
                    m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);
                    m_EnemyController.OrientWeaponsTowards(m_EnemyController.KnownDetectedTarget.transform.position);
                    break;
                case AIState.Attack:
                    if (Vector3.Distance(m_EnemyController.KnownDetectedTarget.transform.position,
                            m_EnemyController.DetectionModule.DetectionSourcePoint.position)
                        >= (AttackStopDistanceRatio * m_EnemyController.DetectionModule.AttackRange))
                    {
                        m_EnemyController.SetNavDestination(m_EnemyController.KnownDetectedTarget.transform.position);
                    }
                    else
                    {
                        m_EnemyController.SetNavDestination(transform.position);
                    }

                    m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);
                    m_EnemyController.TryAtack(m_EnemyController.KnownDetectedTarget.transform.position);
                    break;
            }
        }

        void OnAttack()
        {
            Animator.SetTrigger(k_AnimAttackParameter);
        }

        void OnDetectedTarget()
        {
            if (AiState == AIState.Patrol)
            {
                AiState = AIState.Follow;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Play();
            }

            if (OnDetectSfx)
            {
                AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
            }

            Animator.SetBool(k_AnimAlertedParameter, true);
        }

        void OnLostTarget()
        {
            if (AiState == AIState.Follow || AiState == AIState.Attack)
            {
                AiState = AIState.Patrol;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Stop();
            }

            Animator.SetBool(k_AnimAlertedParameter, false);
        }

        void OnDamaged()
        {
            if (RandomHitSparks.Length > 0)
            {
                int n = Random.Range(0, RandomHitSparks.Length - 1);
                RandomHitSparks[n].Play();
            }

            Animator.SetTrigger(k_AnimOnDamagedParameter);
        }
    }
}