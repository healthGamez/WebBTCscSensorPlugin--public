using UnityEngine;

namespace HealthGamez.WebBTCycleSpeedCadencePlugin.Demo.Assets.Speedometer
{
    public class SpeedMeter : MonoBehaviour
    {
        public GameObject needle;

        private float startPosition = 220f, endPosition = -50f;
        private float desiredPosition;
        private const float MAXSpeed = 180f;

        public float speedValue;

        // Update is called once per frame
        private void FixedUpdate()
        {
            UpdateNeedle();
        }

        private void UpdateNeedle()
        {
            desiredPosition = startPosition - endPosition;
            float speedPercentage = speedValue / MAXSpeed;
            needle.transform.eulerAngles = Vector3.MoveTowards(needle.transform.eulerAngles, new Vector3(0, 0, (startPosition - speedPercentage * desiredPosition)), 100 * Time.deltaTime);
        }

    }
}


