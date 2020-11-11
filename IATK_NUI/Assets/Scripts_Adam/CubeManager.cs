using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

public class CubeManager : MonoBehaviour, IMixedRealityGestureHandler
{
    public void OnGestureCanceled(InputEventData eventData)
    {
        Debug.Log("OnGestureCanceled");
    }

    public void OnGestureCompleted(InputEventData eventData)
    {
        Debug.Log("OnGestureCompleted");
    }

    public void OnGestureStarted(InputEventData eventData)
    {
        Debug.Log("OnGestureStarted");
    }

    public void OnGestureUpdated(InputEventData eventData)
    {
        Debug.Log("OnGestureUpdated");
    }

}