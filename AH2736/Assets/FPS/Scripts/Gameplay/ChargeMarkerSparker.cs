using UnityEngine;

namespace AH2736
{

    public class ChargeMarkerSparker : ChargeBase
    {
        protected override void OnChargeChanged()
        {
            DoSomething();
        }

        private void DoSomething()
        {
            Debug.Log("This object is making sparks...");
        }


    }


}
