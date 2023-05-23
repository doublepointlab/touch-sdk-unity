using UnityEngine;
using System.Collections.Generic;
using Psix.Interaction;

abstract public class VisualizerElement : MonoBehaviour
{
    // Subscribe oneself to required watch events
    abstract public void RegisterWatch(Watch watch);

    // If visualization depends on the gestures, this is needed
    virtual public void RegisterGestures(HashSet<Gesture> gestures) {}
}
