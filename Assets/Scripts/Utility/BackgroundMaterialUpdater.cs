using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundMaterialUpdater : MonoBehaviour {
	[SerializeField]
	private Image[] imagesToUpdate = null;
	[SerializeField]
	private RawImage[] rawImagesToUpdate = null;
	[SerializeField]
	private MeshRenderer[] meshesToUpdate = null;
	[SerializeField]
	private string variableToUpdate = "Animation Time";

	private static float currentTime;
	private static Coroutine updateCoroutine;
	private static event System.Action OnUpdateCalled = delegate { };
	private Dictionary<string, List<Image>> imageRenderers = new Dictionary<string, List<Image>>();
	private Dictionary<string, List<RawImage>> rawImageRenderers = new Dictionary<string, List<RawImage>>();
	private Dictionary<string, List<MeshRenderer>> materialRenderers = new Dictionary<string, List<MeshRenderer>>();

	private void Start() {
		foreach (Image image in imagesToUpdate) {
			if (image == null) continue;
			if (image.material == null) continue;

			if (imageRenderers.ContainsKey(image.material.name) == false) {
				imageRenderers.Add(image.material.name, new List<Image>());
			}

			if (image.material.IsKeywordEnabled(variableToUpdate)) {
				imageRenderers[image.material.name].Add(image);
				continue;
			}

			image.material.EnableKeyword(variableToUpdate);
			imageRenderers[image.material.name].Add(image);
		}

		foreach (RawImage image in rawImagesToUpdate) {
			if (image == null) continue;
			if (image.material == null) continue;

			if (rawImageRenderers.ContainsKey(image.material.name) == false) {
				rawImageRenderers.Add(image.material.name, new List<RawImage>());
			}

			if (image.material.IsKeywordEnabled(variableToUpdate)) {
				rawImageRenderers[image.material.name].Add(image);
				continue;
			}

			image.material.EnableKeyword(variableToUpdate);
			rawImageRenderers[image.material.name].Add(image);
		}

		foreach (MeshRenderer mesh in meshesToUpdate) {
			if (mesh == null) continue;
			if (mesh.material == null) continue;

			if (materialRenderers.ContainsKey(mesh.material.name) == false) {
				materialRenderers.Add(mesh.material.name, new List<MeshRenderer>());
			}

			if (mesh.material.IsKeywordEnabled(variableToUpdate)) {
				materialRenderers[mesh.material.name].Add(mesh);
				continue;
			}

			mesh.sharedMaterial.EnableKeyword(variableToUpdate);
			materialRenderers[mesh.material.name].Add(mesh);
		}

		if (updateCoroutine == null) {
			currentTime = 0;
			updateCoroutine = StartCoroutine(WaitForEndOfFrame());
		}

		UpdateMaterials();
		OnUpdateCalled += UpdateMaterials;
	}

	private void OnDestroy() {
		OnUpdateCalled -= UpdateMaterials;
        updateCoroutine = null;
	}

	private void OnApplicationQuit() {
		currentTime = 0;
		UpdateMaterials();
	}

	private IEnumerator WaitForEndOfFrame() {
		while (Application.isPlaying) {
			yield return new WaitForEndOfFrame();

			currentTime += Time.deltaTime;
			OnUpdateCalled();
		}
	}

	private void UpdateMaterials() {
		foreach (KeyValuePair<string, List<Image>> imageToUpdate in imageRenderers) {
			if (imageToUpdate.Value == null) continue;
			if (imageToUpdate.Value.Count == 0) continue;

			Material imageMaterial = imageToUpdate.Value[0].material;
			imageMaterial.SetFloat(variableToUpdate, currentTime);

			foreach (Image image in imageToUpdate.Value) {
				image.material = imageMaterial;
			}
		}

		foreach (KeyValuePair<string, List<RawImage>> imageToUpdate in rawImageRenderers) {
			if (imageToUpdate.Value == null) continue;
			if (imageToUpdate.Value.Count == 0) continue;

			Material imageMaterial = imageToUpdate.Value[0].material;
			imageMaterial.SetFloat(variableToUpdate, currentTime);

			foreach (RawImage image in imageToUpdate.Value) {
				image.material = imageMaterial;
			}
		}

		foreach (KeyValuePair<string, List<MeshRenderer>> renderersToUpdate in materialRenderers) {
			if (renderersToUpdate.Value == null) continue;
			if (renderersToUpdate.Value.Count == 0) continue;

			Material[] sharedMaterials = renderersToUpdate.Value[0].materials;
			foreach (Material sharedMaterial in sharedMaterials) {
				sharedMaterial.SetFloat(variableToUpdate, currentTime);
			}

			foreach (MeshRenderer meshRenderer in renderersToUpdate.Value) {
				meshRenderer.materials = sharedMaterials;
			}
		}
	}
}
