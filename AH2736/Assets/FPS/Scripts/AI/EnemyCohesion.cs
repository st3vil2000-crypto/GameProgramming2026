using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Unity.FPS.Game;

namespace AH2736
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Cohesion : MonoBehaviour
    {
        // +++ Add AI Behaviour: Cohesion.
        // + Enemies act like electric charges according to type
        //  SmallBots are attracted to BigBots and repelled by SmallBots
        //  BigBots are attracted to SmallBots and repelled by BigBots
        //  Force scales with distance according to inverse square law (like gravity, electricity)
        //  Sum of forces are vectored to adjust movement direction (like object avoidance)

        // + Cohesion Behaviour Variables
        [Header("Scanning")]
        [SerializeField, Range(0f,100f)] private float m_scanRadius = 5f; // sphere of influence
        [SerializeField] private LayerMask m_scanLayer; // GameObject layer for influence (targets of scan)
        
        //hack moving variables to ChargeMarker.cs
        //[Header("Cohesion Properties")]
        //[SerializeField, Range(0,100)] private int m_cohesionCharge = 0; // Units that determine force strength - interacts with other charges in sphere of influence
        //[SerializeField] private bool m_positive = true; // 'positive' or 'negative' charge (opposites attract, like repels)
        
        [Header("Strength of Cohesion Force")]
        [SerializeField, Range(0f,100f)] private float m_forceWeight = 2f; // weight of cohesion force when setting GameObject destination
        [SerializeField, Range(0f, 100f)] private float m_forceMax = 10f; // maximum force magnitude
        [SerializeField, Range(0f, 10f)] private float m_minDistance = 1f; // minimum allowable distance to attractive object

        // + Protected Internal Variables
        private NavMeshAgent m_agent; // reference to NavMeshAgent
        private Collider[] m_scanResults = new Collider[10]; // GameObjects exerting influence
        private ChargeMarker m_myChargeMarker;

        // + Public ReadOnly Variables
        public NavMeshAgent Agent => m_agent; // Reference to NavMeshAgent to control movement in game
        public Collider[] ScanResults => m_scanResults; // List of Neighbours in ScanRadius
        
        //hack moving variables to ChargeMarker.cs
        //public int CohesionCharge => m_cohesionCharge;
        //public bool CohesionPositive => m_positive;
        

        //hack DEBUG VARIABLES
        [Header("Debug Variables")]
        public float m_debugLastForceMag;
        public Vector3 m_debugLastTotalForce;


        private void Awake()
        {
            // Initial Configuration with NavMesh
            m_agent = GetComponent<NavMeshAgent>();
            m_agent.updatePosition = true; // keep GameObject on mesh
            m_agent.updateRotation = true; // allow GameObject to turn

            m_myChargeMarker = GetComponentInChildren<AH2736.ChargeMarker>();

            if (m_myChargeMarker == null )
            {
                Debug.LogError($"ChargeMarker missing on: {gameObject.name}");
            } 

        }

        //  Update is called once per frame, after EnemyMobile and EnemyController (to allow native pathsetting)
        private void LateUpdate()
        {
            // Safety: Ensure script is running on appropriate game object
            // Return void if the GameObject has no NavMeshAgent or is not on the NavMesh
            if (m_agent == null || !m_agent.isOnNavMesh) return;

            // + 1. Get GameObject's destination point (natively set by EnemyMobile)
            Vector3 setDestination = m_agent.destination;

            // + 2. Scan for nearby influencers
            int nearbyNeighbours = Physics.OverlapSphereNonAlloc(
                transform.position,
                m_scanRadius,
                m_scanResults,
                m_scanLayer
                );

            // + 3. Calculate cohesion force from interactions with neighbours
            //  clamp magnitude of force to prevent extreme numbers at close range (asymptotic function)
            Vector3 cohesionForce = CalculateCohesionForce(nearbyNeighbours);
            cohesionForce = Vector3.ClampMagnitude(cohesionForce, m_forceMax);

            // + 4. Move GameObject according to force
            //  weighted by m_forceWeight
            m_agent.Move(cohesionForce * m_forceWeight * Time.deltaTime);
        }

        // ++ Main Method
        // Summary:
            // Calculates total force acting on GameObject 
            // resulting from charge, distance and position of neighbours 
            // within the scan radius
        // Return: 
            // totalForce: Sum of individual force vectors within scan range
        private Vector3 CalculateCohesionForce(int count)
        {
            // Define totalForce vector, defaulting to zero
            Vector3 totalForce = Vector3.zero;

            // + For each neighbour detected by the scan...
            for (int i = 0; i < count; i++)
            {
                // + Define reference to neighbour 
                GameObject neighbour = m_scanResults[i].gameObject;

                // + Safety
                // Ignore self - don't bother with the rest of the update.
                if (neighbour.transform.IsChildOf(this.transform)) continue;
                // otherwise...

                // + Define reference to neighbour cohesion variables
                ChargeMarker stats = neighbour.GetComponent<AH2736.ChargeMarker>();

                // If such information exists...
                if (stats != null)
                {   
                    // + Define neighbour variables
                    float neighbourCharge = stats.CohesionCharge;
                    bool neighbourPositive = stats.CohesionPositive; 
                    Vector3 neighbourPosition = neighbour.transform.position; 
                    float neighbourDistance = Vector3.Distance(transform.position, neighbourPosition);

                    // + Calculate strength of force from neighbour
                    float forceMagnitude = (m_myChargeMarker.CohesionCharge * neighbourCharge) / (neighbourDistance * neighbourDistance);

                    // + Safety
                    // Ignore forces from neighbours within minimum distance to avoid singularities
                    if (neighbourDistance < m_minDistance) forceMagnitude = 0;

                    // + Vector additions: Update totalForce vector with influence of neighbour

                        // If the neighbour has the same charge sign as self
                        // the force is repulsive
                    if (neighbourPositive == m_myChargeMarker.CohesionPositive) 
                    {
                        Vector3 forceDirection = (transform.position - neighbourPosition).normalized;
                        totalForce += forceDirection * forceMagnitude;
                    }

                    // If the neighbour has opposite charge sign as self
                    // the force is attractive
                    if (neighbourPositive != m_myChargeMarker.CohesionPositive) 
                    {
                        Vector3 forceDirection = (neighbourPosition - transform.position).normalized;
                        totalForce += forceDirection * forceMagnitude;
                    }

                    // Debug Information
                    m_debugLastForceMag = forceMagnitude * m_forceWeight;
                    m_debugLastTotalForce = totalForce;
                }
            }



            return totalForce;
        }


        //hack DEBUG STUFF
        private void OnDrawGizmosSelected()
        {
            // 1. Draw Scan Radius and Force Vector
            Gizmos.color = Color.rebeccaPurple;
            Gizmos.DrawWireSphere(transform.position, m_scanRadius); // Draw scan radius
            Gizmos.DrawLine(transform.position, m_debugLastTotalForce); // Draw total force vector
            Gizmos.DrawRay(transform.position, m_debugLastTotalForce * 2f);

            // 2. Draw a label with variables
            if (m_myChargeMarker == null) return;
            string debugInfo = $"Charge: {m_myChargeMarker.CohesionCharge}\nPositive: {m_myChargeMarker.CohesionPositive}\nForceExperienced: {m_debugLastForceMag}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, debugInfo);
        }
    }
}
