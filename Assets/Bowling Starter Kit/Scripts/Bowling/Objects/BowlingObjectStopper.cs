using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyApp.BowlingKit.Extera
{
    [RequireComponent(typeof(Collider))]
    public class BowlingObjectStopper : MonoBehaviour
    {
        #region variable
        [HideInInspector] public Collider _collider;
        #endregion
        #region Functions
        private void Start()
        {
            _collider = GetComponent<Collider>(); _collider.isTrigger = true;
        }
        private void OnTriggerStay(Collider other)
        {
            if (other == _collider) return;
            BowlingBall ball = other.gameObject.GetComponent<BowlingBall>();
            BowlingPin pin = other.gameObject.GetComponent<BowlingPin>();
            if (ball != null)
            {
                ball._rb.velocity = Vector3.zero;
            }
            if (pin != null)
            {
                pin._rb.velocity = Vector3.zero;
            }
        }
        #endregion
    }
}