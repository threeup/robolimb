using UnityEngine;
using System.Collections.Generic;


namespace BasicCommon
{
    [System.Serializable]
    public class BasicTimer
    {
        [SerializeField]
        private float timeVal = -1f;
        public float TimeVal { get { return timeVal; } set { timeVal = value; } }
        [SerializeField]
        private float duration = -1f;
        public float Duration { get { return duration; } set { duration = value; } }

        private bool isLooping = false;
        private bool isPaused = false;
        public bool IsPaused { get { return isPaused; } }

        public float Percent { get { return timeVal / duration; } }

        public BasicTimer(float duration, bool isLooping = true)
        {
            this.timeVal = duration;
            this.duration = duration;
            this.isLooping = isLooping;
            this.isPaused = duration < 0.01f;
        }

        public bool Tick(float deltaTime)
        {
            bool result = false;
            if( timeVal < 0f || isPaused )
            {
                return result;
            }
            
            timeVal -= deltaTime;
            if( timeVal < 0f )
            {
                result = true;
                if( isLooping )
                {
                    Reset();
                }
            }
            return result;
        }

        public void Pause(bool shouldPause)
        {
            this.isPaused = shouldPause;
        }

        public void Reset()
        {
            timeVal = duration;
        }

        public void SetMin(float val)
        {
            timeVal = Mathf.Min(val, timeVal);
        }

        public override string ToString()
        {
            return timeVal.ToString() + " " + isPaused;
        }

    }
}