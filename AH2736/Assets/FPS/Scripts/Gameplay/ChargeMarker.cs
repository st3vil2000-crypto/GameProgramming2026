using UnityEngine;

namespace AH2736
{

    // ++ Basic Charge Marker
    //  Inherits from ChargeBase
    //  Adds light-based effects to visualise charge variables
    public class ChargeMarker : ChargeBase
    {
        // +++ Effect Variables
        [Header("Visuals")]
        public Light m_chargeLight; // Reference to a light component (contained in prefab)
        public ParticleSystem m_chargeParticles; // Reference to particle component (in prefab)

        // ++ Set visuals at start
        void Start()
        {
            UpdateVisuals();
        }

        // ++ Inject call to Effects Method through the virtual override
        // + "When there is a change in charge, change the visual effects"
        protected override void OnChargeChanged()
        {
            UpdateVisuals();
        }

        // ++ Set visual effect variables to reflect charge values
        private void UpdateVisuals()
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

    }

}