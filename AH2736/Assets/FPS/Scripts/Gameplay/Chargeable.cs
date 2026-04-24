using AH2736;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AH2736
{

    // +++ Charge Interface
    // + Contract for (potential) charge carriers
    public class Chargeable : MonoBehaviour
    {
        // ++ Chargability Variable
        [Tooltip("Multiplier to apply to the received charge")]
        public int ChargeMultiplier = 1; 
        // Defines how chargeable the object is compared to default. Allows variation in materials. E.g. Conductors vs Insulators.

        // ++ ChargeMarker object to use
        // + Charge is held by independent ChargeMarkers.
        // The concrete object to use as a 'charge-carrier' must be defined for each chargable object.
        [Tooltip("The charge prefab to spawn if not native")]
        [SerializeField] private GameObject chargeMarkerPrefab; 

        // ++ Public Read-Only Property for the ChargeMarker
        // Allows other objects to read the chargeable object's charge, but not to change it.
        public ChargeBase ChargeMarker { get; private set; }

        // + Attach the ChargeMarker at Initialization (if one exists)
        void Awake()
        {
            // Find the charge component
            ChargeMarker = GetComponent<ChargeBase>();
            if (!ChargeMarker)
            {
                ChargeMarker = GetComponentInParent<ChargeBase>();
            }
            if (!ChargeMarker)
            {
                ChargeMarker = GetComponentInChildren<ChargeBase>();
            }
        }

        // ++ Key Method: How to handle the imposition of charge between objects
        public void InflictCharge(int charge, bool positive, GameObject chargeSource)
        {
            //+ If ChargeMarker already exists, modify it
            //todo Method is a bit clunky because of how charge is represented. A holdover from initial implementation.
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
                ChargeMarker = chargeMarkerInstance.GetComponent<AH2736.ChargeBase>();

                // Apply the received charge
                ChargeMarker.TakeCharge(charge, positive, chargeSource);

            }

        }

    }
}
