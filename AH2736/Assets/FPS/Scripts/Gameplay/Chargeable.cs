using AH2736;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AH2736
{
    public class Chargeable : MonoBehaviour
    {
        [Tooltip("Multiplier to apply to the received charge")]
        public int ChargeMultiplier = 1;

        [Tooltip("The charge prefab to spawn if not native")]
        [SerializeField] private GameObject chargeMarkerPrefab;

        public ChargeMarker ChargeMarker { get; private set; }

        void Awake()
        {
            // Find the charge component
            ChargeMarker = GetComponent<ChargeMarker>();
            if (!ChargeMarker)
            {
                ChargeMarker = GetComponentInParent<ChargeMarker>();
            }
            if (!ChargeMarker)
            {
                ChargeMarker = GetComponentInChildren<ChargeMarker>();
            }
        }

        public void InflictCharge(int charge, bool positive, GameObject chargeSource)
        {
            // If ChargeMarker already exists, modify it
            if (ChargeMarker)
            {
                // take original values and combine to put on the number line
                var totalCharge = ChargeMarker.CohesionCharge;
                var chargePositive = ChargeMarker.CohesionPositive;

                if (chargePositive == true) totalCharge = totalCharge * 1;
                if (chargePositive == false) totalCharge = totalCharge * -1;

                // combine charge and sign of infliction to put on the number line
                if (positive == true) charge = charge * 1;
                if (positive == false) charge = charge * -1;

                // perform numerical calculations
                totalCharge += charge * ChargeMultiplier;

                // Set positive or negative state based on numerical charge value
                if (totalCharge <= 0) chargePositive = false;
                if (totalCharge > 0) chargePositive = true;

                // convert charge value back to absolute magnitude
                totalCharge = Mathf.Abs(totalCharge);

                // Apply the charge
                ChargeMarker.TakeCharge(totalCharge, chargePositive, chargeSource);

            }
            else // If ChargeMarker doesn't exist, spawn one 
            {
                // Create a ChargeMarker instance at target centre
                GameObject chargeMarkerInstance = Instantiate(chargeMarkerPrefab, transform.position, Quaternion.identity);

                // Parent it to this object so it sticks
                chargeMarkerInstance.transform.SetParent(this.transform);

                // Update ChargeMarker with the new instance
                ChargeMarker = chargeMarkerInstance.GetComponent<AH2736.ChargeMarker>();

                // Apply the received charge
                ChargeMarker.TakeCharge(charge, positive, chargeSource);

            }

        }

    }
}
