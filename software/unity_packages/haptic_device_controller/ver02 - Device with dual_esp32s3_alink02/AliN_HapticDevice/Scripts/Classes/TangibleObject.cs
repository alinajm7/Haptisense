using System;
using UnityEngine;

namespace AliN.Microcontroller.Classes
{
    
    public class TangibleObject: MonoBehaviour
    {
        [Header("--------------Change intencity by distance----------------")]
        [SerializeField]
        [Tooltip("The value can be 5k to 500k")]
        [Header("*(5 Hz to 500kHz)*")]
        [Range(5,500000)]
        private int _initialFrequencyInHz = 1000;
        public int initialFrequencyInHz
        {
            get { return _initialFrequencyInHz; }
            set { _initialFrequencyInHz = Mathf.Clamp(value, 5, 500000); }
        }

        [Header("--------------Change frequency by distance----------------")]
        [SerializeField]
        private bool _useDynamicFrequency = false;
        public bool useDynamicFrequency
        {
            get { return _useDynamicFrequency; }
            set { _useDynamicFrequency = value; }
        }

        [SerializeField]
        [Tooltip("The frequency related to the scanning distance of the object.")]
        [Range(5, 500000)]
        private int _scanningDistanceFreq = 5;
        public int distanceScanningFrequency
        {
            get { return _scanningDistanceFreq; }
            set { _scanningDistanceFreq = Mathf.Clamp(value, 5, 500000); }
        }

        [SerializeField]
        [Tooltip("The frequency related to the proximity of the object.")]
        [Range(5, 500000)]
        private int _proximityFreq = 100000;
        public int proximityDetectionFrequency
        {
            get { return _proximityFreq; }
            set { _proximityFreq = Mathf.Clamp(value, 5, 500000); }
        }

        [Header("-------------- Fixed Values ----------------")]
        
        
        public bool useFixedLowTimeFrequency = false;
        [Tooltip("Min = 1   and Max= 99999.   Value 0 keeps actuator off")]
        public int fixedLowTimeFrequency = 1;
        [SerializeField]
        
        public bool useFixedHighTimeFrequency = false;
        [Tooltip("Min = 0   and Max= 99999")]
        public int fixedHighTimeFrequency = 0;

        [Header("--------------*** Specified thresholds ***----------------")]
        public bool useLocalMinThreshold = false;
        [Range(0, 1f)]
        public float tangibleMinValueThreshold = 0.14f;
        public bool useLocalMaxValue = false;
        [Range(0, 1f)]
        public float tangibleMaxValue = 1.0f;
    }
}
