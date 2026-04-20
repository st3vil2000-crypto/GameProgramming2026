using UnityEngine;

namespace AH2736
{


    public abstract class ChargeBase : MonoBehaviour
    {
        // + Charge Variables
        [Header("Charge Properties")]
        [SerializeField, Range(0, 100)] protected int m_cohesionCharge = 1; // Charge Magnitude: determines force strength 
        [SerializeField] protected bool m_positive = true; // 'positive' or 'negative' charge (opposites attract, like repels)

        //todo decay effect
        //todo change charge sign to enum instead of boolean?


        // + Public Read-Only Variables
        public virtual int CohesionCharge
        {
            get => m_cohesionCharge;
            set
            {
                m_cohesionCharge = value;
            }
        }

        public virtual bool CohesionPositive
        {
            get => m_positive;
            set
            {
                m_positive = value;
            }
        }

        // + Critical Method : taking charge
        public virtual void TakeCharge(int charge, bool positive, GameObject chargeSource)
        {
            m_cohesionCharge = charge;
            m_positive = positive;
        }

    }

}