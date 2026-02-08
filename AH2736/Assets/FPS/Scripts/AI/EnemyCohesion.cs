using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

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
        [Header("Cohesion")]
        [SerializeField] public float m_scanRadius = 5f; // sphere of influence
        [SerializeField] public LayerMask m_scanLayer; // GameObject layer for influence (targets of scan)
        [SerializeField] public int m_cohesionCharge = 0; // Units that determine force strength - interacts with other charges in sphere of influence
        [SerializeField] public bool m_positive = true; // 'positive' or 'negative' charge (opposites attract, like repels)
        [SerializeField] public float m_forceWeight = 2f; // weight of cohesion force when setting GameObject destination

        public NavMeshAgent m_agent; // reference to NavMeshAgent
        public Collider[] m_scanResults = new Collider[10]; // GameObjects exerting influence

        //hack DEBUG VARIABLES
        [SerializeField] public float m_debugLastForceMag;
        [SerializeField] public Vector3 m_debugLastTotalForce;


        //  Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            m_agent = GetComponent<NavMeshAgent>(); // Define reference to NavMeshAgent to control movement in game
            m_agent.updatePosition = true; // keep GameObject on mesh
            m_agent.updateRotation = true; // allow GameObject to turn
            m_agent.acceleration = 100f; // responsiveness to velocity adjustments
        }

        //  Update is called once per frame, after EnemyMobile and EnemyController (to allow native pathsetting)
        void LateUpdate()
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
            cohesionForce = Vector3.ClampMagnitude(cohesionForce, 10.0f);

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
        public Vector3 CalculateCohesionForce(int count)
        {
            // Define totalForce vector, defaulting to zero
            Vector3 totalForce = Vector3.zero;

            // + For each neighbour detected by the scan...
            for (int i = 0; i < count; i++)
            {
                // + Define reference to neighbour 
                GameObject neighbour = m_scanResults[i].gameObject;

                // Ignore self - don't bother with the rest of the update.
                if (neighbour == this.gameObject) continue;
                // otherwise...

                // + Define reference to neighbour cohesion variables
                Cohesion stats = neighbour.GetComponentInParent<AH2736.Cohesion>();
                
                // If such information exists...
                if (stats != null)
                {
                    // + Define neighbour variables
                    float neighbourCharge = stats.m_cohesionCharge;
                    bool neighbourPositive = stats.m_positive; 
                    Vector3 neighbourPosition = neighbour.transform.position; 
                    float neighbourDistance = Vector3.Distance(transform.position, neighbourPosition);

                    // + Calculate strength of force from neighbour
                    float forceMagnitude = (m_cohesionCharge * neighbourCharge) / (neighbourDistance * neighbourDistance);

                    // + Vector additions: Update totalForce vector with influence of neighbour

                    // If the neighbour has the same charge sign as self
                    // the force is repulsive
                    if (neighbourPositive == m_positive) 
                    {
                        Vector3 forceDirection = (transform.position - neighbourPosition).normalized;
                        totalForce += forceDirection * forceMagnitude;
                    }

                    // If the neighbour has opposite charge sign as self
                    // the force is attractive
                    if (neighbourPositive != m_positive) 
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
        private void OnDrawGizmos()
        {
            // 1. Draw Scan Radius and Force Vector
            Gizmos.color = Color.rebeccaPurple;
            Gizmos.DrawWireSphere(transform.position, m_scanRadius); // Draw scan radius
            Gizmos.DrawLine(transform.position, m_debugLastTotalForce); // Draw total force vector
            Gizmos.DrawRay(transform.position, m_debugLastTotalForce * 2f);

            // 2. Draw a label with variables
            string debugInfo = $"Charge: {m_cohesionCharge}\nPositive: {m_positive}\nForceExperienced: {m_debugLastForceMag}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, debugInfo);
        }
    }
}
