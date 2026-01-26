using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace AA0000
{
    public class DiceThrower : MonoBehaviour
    {
        public KeyCode throwKey = KeyCode.R;

        public List<Rigidbody> diceRigidbodies; // Option 1 - Needs several GameObjects with Rigidbody
        [SerializeField] GameObject dicePrefab; // Option 2 - Needs an Prebaf that has GameObject with Rigidbody

        public bool useOption1 = true;
        public Vector3 forceDirection = Vector3.forward; // Will be read from forward direction of the GameObject this script is attached to
        public float forceAmount = 1000.0f; // 10 was enough


        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(throwKey))
            {
				forceDirection = transform.forward;
				switch (useOption1)
                { 
                    case true :
                        foreach (Rigidbody body in diceRigidbodies)
                            body.AddForce(forceDirection*forceAmount,ForceMode.Impulse);
                        break;
                    case false :
                        GameObject go = Instantiate(dicePrefab, transform.position, transform.rotation);
                        Rigidbody rb = go.GetComponent<Rigidbody>();
                        rb.AddForce(forceDirection * forceAmount, ForceMode.Impulse);
						break;
                }                  
            }
        }
    } 
}
