using UnityEngine;

namespace AH2736
{


    public class ChargeMarker : ChargeBase
    {
        // + Inherits from ChargeBase
        // + Adds effects to visualise charge variables
        
        
        // + Effect Variables
        [Header("Visuals")]
        public Light m_chargeLight;
        public ParticleSystem m_chargeParticles;

        // + Public Read-Only Variables
        public override int CohesionCharge
        {
            get => m_cohesionCharge;
            set
            {
                m_cohesionCharge = value;
                UpdateVisuals();
            }
        }
        
        public override bool CohesionPositive
        {
            // Allows visuals to be updated if sign changes
            get => m_positive;
            set
            {
                m_positive = value;
                UpdateVisuals();
            }
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            // Visualise charge properties: red = positive, blue = negative, intensity = charge magnitude
        
            Color effectColour = m_positive ? Color.red : Color.blue;

            if (m_chargeLight != null)
            {
                m_chargeLight.color = effectColour;
                m_chargeLight.intensity = m_cohesionCharge *1.0f;
            }

            if (m_chargeParticles != null)
            {
                var main = m_chargeParticles.main;
                main.startColor = effectColour;
            }
        }

        public override void TakeCharge(int charge, bool positive, GameObject chargeSource)
        {
            m_cohesionCharge = charge;
            m_positive = positive;

            UpdateVisuals();
        }

    }

}