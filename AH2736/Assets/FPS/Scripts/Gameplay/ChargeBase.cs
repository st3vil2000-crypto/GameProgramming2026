using UnityEngine;

namespace AH2736
{

    // ++ Abstract Class and Virtual Method for Charge-Carriers
    // ChargeBase defines an object's intrinsic charge. 
    // It is instantiated as an invisible object attached to a GameObject. 
    // It goes where they go. If anyone asks what the holding GameObject's charge is,
    // this magic box has the answer.
    // The abstract base class ensures a strict default behaviour.
    // Basically, establishing that 'charge coming in equals charge coming out'.
    // This is to mimic the quantized and conserved nature of electric charge. 
    // Establishing an inescapable Natural Law.
    // However, a virtual method allows sub-types in ChargeMarkers
    // that vary in how a charge-carrier reacts to changes in charge.
    public abstract class ChargeBase : MonoBehaviour
    {
        // +++ Charge Variables
        [Header("Charge Properties")]
        [SerializeField, Range(0, 100)] protected int m_cohesionCharge = 1; // Charge Magnitude
        [SerializeField] protected bool m_positive = true; // 'positive' or 'negative' charge (opposites attract, like repels)

        //todo decay effect
        //todo change charge sign to enum instead of boolean?
        //todo rework charge to be on linear scale?


        // +++ Public Read-Only Properties
        public int CohesionCharge
        {
            get => m_cohesionCharge;
            set
            {
                m_cohesionCharge = value;
                OnChargeChanged();
            }
        }

        public bool CohesionPositive
        {
            get => m_positive;
            set
            {
                m_positive = value;
                OnChargeChanged();
            }
        }

        // +++ Critical Method : taking charge
        public void TakeCharge(int charge, bool positive, GameObject chargeSource)
        {
            m_cohesionCharge = charge;
            m_positive = positive;

            OnChargeChanged();
        }

        // + Optional trigger for behaviour when charge changes
        // virtual method that can be overriden to implement charge markers 
        // that vary in their response to changes in charge.
        protected virtual void OnChargeChanged()
        {
            // Default: Nothing
        }


    }

}