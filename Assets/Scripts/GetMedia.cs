using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMedia : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

	/// <summary>
	/// GetImage Callback handling gallery access.
	/// </summary>
	public void GetImage(GameObject poiObject)
    {
		// Don't attempt to pick media from Gallery/Photos if
		// another media pick operation is already in progress
		if (NativeGallery.IsMediaPickerBusy())
			return;

		Debug.Log("Pick Image.");
		// Pick a PNG image from Gallery/Photos
		// If the selected image's width and/or height is greater than 512px, down-scale the image
		PickImage(poiObject, 512);
	}

	private void PickImage(GameObject poiObject,int maxSize)
	{
		NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
		{
			Debug.Log("Image path: " + path);
			if (path != null)
			{
				// Create Texture from selected image
				Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize);
				if (texture == null)
				{
					Debug.Log("Couldn't load texture from " + path);
					return;
				}

				// Assign texture to a temporary quad and destroy it after 5 seconds
				/*				GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
								quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
								quad.transform.forward = Camera.main.transform.forward;
								quad.transform.localScale = new Vector3(1f, texture.height / (float)texture.width, 1f);
				*/
				//var placedPOI = Instantiate(POI);

				// Assign texture to a placed poi prefab
				var imageCube = poiObject.transform.GetChild(0);

				Material material = imageCube.GetComponent<Renderer>().material;
				if (!material.shader.isSupported) // happens when Standard shader is not included in the build
					material.shader = Shader.Find("Legacy Shaders/Diffuse");

				material.mainTexture = texture;

			}
		}, "Select an image", "image/*");

		Debug.Log("Permission result: " + permission);
	}
}
