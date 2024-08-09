using System;
using UnityEngine;

namespace AliN.Microcontroller.Classes
{
    
    public class TangibleObject: MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The value can be 5k to 500k")]
        [Header("*(5 Hz to 500kHz)*")]
        [Range(5,500000)]
        private int _initialFrequencyInHz = 50000;
        public int initialFrequencyInHz
        {
            get { return _initialFrequencyInHz; }
            set { _initialFrequencyInHz = Mathf.Clamp(value, 5, 500000); }
        }

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
        public int scanningDistanceFreq
        {
            get { return _scanningDistanceFreq; }
            set { _scanningDistanceFreq = Mathf.Clamp(value, 5, 500000); }
        }

        [SerializeField]
        [Tooltip("The frequency related to the proximity of the object.")]
        [Range(5, 500000)]
        private int _proximityFreq = 100000;
        public int proximityFreq
        {
            get { return _proximityFreq; }
            set { _proximityFreq = Mathf.Clamp(value, 5, 500000); }
        }
    }
}
