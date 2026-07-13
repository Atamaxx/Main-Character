using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My
{
    public class TimeChanges : MonoBehaviour
    {
        public static float FloatByTime(float fromValue, float toValue, float fromTime, float toTime, float currTime)
        {
            return Mathf.Lerp(fromValue, toValue, CalculatePercent(fromTime, toTime, currTime));
        }

        public static Vector2 Vector2ByTime(Vector2 fromValue, Vector2 toValue, float fromTime, float toTime, float currTime)
        {
            return Vector2.Lerp(fromValue, toValue, CalculatePercent(fromTime, toTime, currTime));
        }

        public static Vector4 Vector4ByTime(Vector4 fromValue, Vector4 toValue, float fromTime, float toTime, float currTime)
        {
            return Vector4.Lerp(fromValue, toValue, CalculatePercent(fromTime, toTime, currTime));
        }


        public static Color ColorByTime(Color startColor, Color endColor, float fromTime, float toTime, float currTime)
        {
            return Color.Lerp(startColor, endColor, CalculatePercent(fromTime, toTime, currTime));
        }
        public static float CalculatePercent(float fromTime, float toTime, float currTime)
        {
            if (currTime <= fromTime)
            {
                return 0;
            }
            else if (currTime > toTime)
            {
                return 1;
            }
            return Mathf.Clamp01((currTime - fromTime) / (toTime - fromTime));
        }

        public static bool BeyondTime(float fromTime, float toTime, float currTime)
        {
            if (currTime < fromTime || currTime > toTime)
            {
                return true;
            }

            return false;
        }

        public static bool BeyondTime(List<Vector2> timeBounds, float currTime)
        {
            foreach (Vector2 timeBound in timeBounds)
            {
                if (currTime >= timeBound.x && currTime <= timeBound.y)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool BeyondTime(Vector2 timeBound, float currTime)
        {
            if (currTime < timeBound.x || currTime > timeBound.y)
            {
                return true;
            }

            return false;
        }

        public static float BeyondTimeValue(float fromTime, float toTime, float currTime)
        {
            if (currTime < fromTime)
            {
                return fromTime;
            }
            else if (currTime > toTime)
            {
                return toTime;
            }
            return currTime;
        }


    }


}