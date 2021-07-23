using UnityEngine;
using System.Runtime.InteropServices;
using JetBrains.Annotations; //add support for JavaScript plugin call
//#pragma warning disable 649
#pragma warning disable 649

namespace HealthGamez.WebBTCycleSpeedCadencePlugin
{
    public class CscSensor : MonoBehaviour
    {

        public byte SensorType { get; private set; } //Sensor type, reading content [1=Wheel Rotation Sensor, 2=Cadence Sensor, 3=Wheel and Cadence]
        public uint CumulativeWheelRevolutions { get; private set; } //Cumulative Wheel Revolution, read only, do not roll over max 4,294,967,296 revolutions
        public ushort LastWheelTimeStamp { get; private set; } //Last Wheel Revolution Event Time-stamp [seconds*1024], read only, roll over every 64 seconds
        public double WheelRotationSpeed { get; private set; } //Wheel rotation speed [rpm], read only
        public ushort CumulativeCrankRevolutions { get; private set; } //Cumulative Crank Revolution,  read only, roll over every 65,535 revolutions.
        public ushort LastCrankTimeStamp { get; private set; } //Last Crank Revolution Event Time-stamp [seconds*1024], read only, roll over every 64 seconds
        public double Cadence { get; private set; } //Cadence (Crank rotation speed) [rpm]

        public double wheelReadingTimeout = 3.0; //Maximum time between cumulative readings before setting speed value to zero [seconds]
        public double crankReadingTimeout = 3.0; //Maximum time between cumulative readings before setting speed value to zero [seconds]


        [DllImport("__Internal")]
        private static extern void BeConnect();
         [DllImport("__Internal")]
        private static extern bool BeIsConnected();

        
        public  static CscSensor Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            } else {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }
         
        public void Connect() //Will initialize scan for compatible Bluetooth sensors. A popup window will be displayed in web-browser showing all compatible  Cycling and Cadence sensors in range
        {
            BeConnect();
        }

        public bool IsConnected()
        {
            return BeIsConnected();
        }

        [UsedImplicitly]
        private class CscMeasurement
        {
            public byte FlagField;
            public uint CumulativeWheelRevolutions;
            public ushort WheelTimeStamp;
            public ushort CumulativeCrankRevolutions;
            public ushort CrankTimeStamp;
        }

        private static uint _previousCumulativeWheelRevolutions;
        private static ushort _previousWheelTimeStamp;
        private static ushort _previousCumulativeCrankRevolutions;
        private static ushort _previousCrankTimeStamp;
        private static float _lastWheelUpdate;
        private static float _lastCrankUpdate;

        public void UpdateCscMeasurement(string cscMeasurementJson)
        {
            CscMeasurement cscMeasurement = JsonUtility.FromJson<CscMeasurement>(cscMeasurementJson);
            SensorType = cscMeasurement.FlagField;
            switch (SensorType)
            {
                case 1: //If wheel rotation sensor
                    UpdateWheelRevolutionAndSpeed(cscMeasurement);
                    break;
                case 2: //If Cadence sensor
                    UpdateCrankRevolutionAndSpeed(cscMeasurement);
                    break;
                case 3: //If speed and cadence sensor
                    UpdateWheelRevolutionAndSpeed(cscMeasurement);
                    UpdateCrankRevolutionAndSpeed(cscMeasurement);
                    break;
                default:
                    Debug.LogError("Error parsing sensor reading, SensorType not defined: " + SensorType);
                    break;
            }
        }

        private void UpdateWheelRevolutionAndSpeed(CscMeasurement cscMeasurement)
        {
            CumulativeWheelRevolutions = cscMeasurement.CumulativeWheelRevolutions;
            LastWheelTimeStamp = cscMeasurement.WheelTimeStamp;
            if (_previousCumulativeWheelRevolutions != cscMeasurement.CumulativeWheelRevolutions)
            {
                if (_previousCumulativeWheelRevolutions != 0)
                {
                    double wheelRoundsSinceLast = cscMeasurement.CumulativeWheelRevolutions - _previousCumulativeWheelRevolutions;
                    double timeSinceLastRound = (cscMeasurement.WheelTimeStamp - _previousWheelTimeStamp) / 1024.0;

                    if (timeSinceLastRound > 0.0 && timeSinceLastRound <= wheelReadingTimeout)
                    {
                        WheelRotationSpeed = (wheelRoundsSinceLast / timeSinceLastRound * 60.0);
                        _lastWheelUpdate = Time.fixedTime;
                    }
                }
                _previousCumulativeWheelRevolutions = cscMeasurement.CumulativeWheelRevolutions;
                _previousWheelTimeStamp = cscMeasurement.WheelTimeStamp;
            }
            else if ((Time.fixedTime - _lastWheelUpdate) > wheelReadingTimeout)
            {
                WheelRotationSpeed = 0.0;
            }
        }

        private void UpdateCrankRevolutionAndSpeed(CscMeasurement cscMeasurement)
        {
            CumulativeCrankRevolutions = cscMeasurement.CumulativeCrankRevolutions;
            LastCrankTimeStamp = cscMeasurement.CrankTimeStamp;
            if (_previousCumulativeCrankRevolutions != cscMeasurement.CumulativeCrankRevolutions)
            {
                if (_previousCumulativeCrankRevolutions != 0)
                {
                    double crankRoundsSinceLast = cscMeasurement.CumulativeCrankRevolutions - _previousCumulativeCrankRevolutions;
                    double timeSinceLastRound = (cscMeasurement.CrankTimeStamp - _previousCrankTimeStamp) / 1024.0;

                    if (timeSinceLastRound > 0.0 && timeSinceLastRound <= crankReadingTimeout)
                    {
                        Cadence = (crankRoundsSinceLast / timeSinceLastRound * 60.0);
                        _lastCrankUpdate = Time.fixedTime;
                    }
                }
                _previousCumulativeCrankRevolutions = cscMeasurement.CumulativeCrankRevolutions;
                _previousCrankTimeStamp = cscMeasurement.CrankTimeStamp;
            }
            else if ((Time.fixedTime - _lastCrankUpdate) > crankReadingTimeout)
            {
                Cadence = 0.0;
            }
        }
    }
}