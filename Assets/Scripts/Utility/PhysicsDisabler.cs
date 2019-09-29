using UnityEngine;

public class PhysicsDisabler : MonoBehaviour {
    void Awake() {
        Physics.autoSimulation = false;
    }
}
