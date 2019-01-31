using UnityEngine;
using UnityEngine.UI;

public class ImageToShader : MonoBehaviour {
	[SerializeField]
	private RawImage imageToUpdate = null;
	[SerializeField]
	private string variableToUse = null;

	private void OnValidate() {
		Awake();
	}

	private void Awake() {
		if (imageToUpdate == null || string.IsNullOrEmpty(variableToUse)) {
			enabled = false;
			return;
		}

		if (imageToUpdate.material == null || imageToUpdate.mainTexture == null) {
			enabled = false;
			return;
		}

		imageToUpdate.material.EnableKeyword(variableToUse);
		imageToUpdate.material.SetTexture(variableToUse, imageToUpdate.texture);
	}
}
