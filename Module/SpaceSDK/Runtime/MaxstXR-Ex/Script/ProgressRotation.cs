using UnityEngine;

namespace MaxstXR.Extension
{
    public class ProgressRotation : MonoBehaviour
    {
        [SerializeField] private float speed = -50;

        private void Update()
        {
            transform.Rotate(new Vector3(0, 0, speed * Time.deltaTime));
        }
    }
}
