using System.Collections;
using UnityEngine;


namespace MaxstXR.Place
{
    public class PoICollider : MonoBehaviour
    {
        public const float STAFF_MOVEMENT_DISTANCE = 1F;
        public const float POI_MOVEMENT_DISTANCE = 3F;

        private const float MOVEMENT_SCALAR = 0.01F;

        
        [SerializeField] private GameObject main;
        [SerializeField] private LayerMask collisionLayerMask;
        [HideInInspector] private float maxMoveMentDistance = POI_MOVEMENT_DISTANCE;

        private Vector3 data = Vector3.zero;
        private Vector3 originalLocation = Vector3.zero;

        private void Start()
        {
            StartCoroutine(ConfigOriginLocation());
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!collisionLayerMask.IsValid(collision.gameObject.layer))
            {
                return;
            }

            var otherCollider = collision.gameObject.GetComponent<PoICollider>();
            if (otherCollider)
            {
                return;
            }

            ContactPoint point = collision.GetContact(0);

            if (data == Vector3.zero)
            {
                data = point.normal * MOVEMENT_SCALAR;
                data = new Vector3(data.x, 0, data.z);
            }

            if (main != null)
            {
                var expectLocation = main.transform.position + data;
                if (Vector3.Distance(expectLocation, originalLocation) < maxMoveMentDistance)
                {
                    main.transform.position = expectLocation;
                    //Debug.Log($"OnCollisionStay expectLocation Distance {main.name}/{Vector3.Distance(expectLocation, originalLocation)}");
                }
            }
            else
            {
                var expectLocation = gameObject.transform.parent.transform.position + data;
                if (Vector3.Distance(expectLocation, originalLocation) < maxMoveMentDistance)
                {
                    gameObject.transform.parent.transform.position = expectLocation;
                    //Debug.Log($"OnCollisionStay expectLocation Distance {gameObject.transform.parent.name}/{Vector3.Distance(expectLocation, originalLocation)}");
                }
            }
        }

        private IEnumerator ConfigOriginLocation()
        {
            yield return new WaitForEndOfFrame();
            if (main)
            {
                originalLocation = main.transform.position;
            }
            else
            {
                originalLocation = gameObject.transform.parent.transform.position;
            }
        }
    }
}
