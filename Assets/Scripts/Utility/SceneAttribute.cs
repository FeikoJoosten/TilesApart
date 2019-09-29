using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SceneAttribute : PropertyAttribute {
    public string[] invalidScenes;

    public SceneAttribute(params string[] invalidScenes) {
        this.invalidScenes = invalidScenes;
    }
}