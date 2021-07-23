using HealthGamez.WebBTCycleSpeedCadencePlugin.Demo.Assets.Speedometer;
using UnityEngine;
using UnityEngine.UI;

namespace HealthGamez.WebBTCycleSpeedCadencePlugin.Demo {
    public class GameManager : MonoBehaviour
    {
        private Text sensorTypeText;
        private Text lastTimeStampText;
        private Text cumRoundsText;
        private Text speedText;
        private SpeedMeter speedMeter;
        private CscSensor cScSensor;
        private Image bluetoothConnectedIcon;

        // Start is called before the first frame update
        private void Start()
        {
            sensorTypeText = GameObject.Find("sensorTypeText").GetComponent<Text>();
            lastTimeStampText = GameObject.Find("lastTimeStampText").GetComponent<Text>();
            cumRoundsText = GameObject.Find("cumRoundsText").GetComponent<Text>();
            speedText = GameObject.Find("speedText").GetComponent<Text>();
            speedMeter = GameObject.Find("speedMeter").GetComponent<SpeedMeter>();
            cScSensor = GameObject.Find("WebBTCycleSpeedCadencePlugin").GetComponent<CscSensor>();
            bluetoothConnectedIcon = GameObject.Find("BluetoothConnectedIcon").GetComponent<Image>();
        }

        
        
        // Update is called once per frame
        private void Update()
        {
           bool  isConnected = cScSensor.IsConnected();
           bluetoothConnectedIcon.enabled = isConnected;

            if (isConnected)
            {
                switch (cScSensor.SensorType)
                {
                    case 1:
                        sensorTypeText.text = "Wheel Speed";
                        lastTimeStampText.text = cScSensor.LastWheelTimeStamp.ToString();
                        cumRoundsText.text = cScSensor.CumulativeWheelRevolutions.ToString();
                        speedText.text = cScSensor.WheelRotationSpeed.ToString("0.0") + " rpm";
                        speedMeter.speedValue = (float)cScSensor.WheelRotationSpeed;
                        break;

                    case 2:
                        sensorTypeText.text = "Cadence";
                        lastTimeStampText.text = cScSensor.LastCrankTimeStamp.ToString();
                        cumRoundsText.text = cScSensor.CumulativeCrankRevolutions.ToString();
                        speedText.text = cScSensor.Cadence.ToString("0.0") + " rpm";
                        speedMeter.speedValue = (float)cScSensor.Cadence;
                        break;

                    case 3:
                        Debug.LogError("Wheel and Cadence sensor not implemented in this demo scene");

                        break;
                }
            }
            else
            {
                sensorTypeText.text = "none";
                lastTimeStampText.text = "-";
                cumRoundsText.text = "-";
                speedText.text = "-";
                speedMeter.speedValue = 0; 
            }
        }
    }
}