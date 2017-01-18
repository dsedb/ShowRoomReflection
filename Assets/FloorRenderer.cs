using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FloorRenderer : MonoBehaviour {
	public LayerMask reflectLayers = -1;
	public Material surface_material_;
	private Vector4 reflectionPlane = new Vector4(0f, 1f, 0f, 0f);
	private Dictionary<Camera, Camera> reflection_camera_hash_ = new Dictionary<Camera, Camera>();
	private RenderTexture reflection_texture_;
	private MeshFilter mf_;

	public Texture getReflectionTexture() { return reflection_texture_; }


	private void calc_reflection_matrix(ref Matrix4x4 mat, Vector4 plane)
	{
		mat.m00 = (1f - 2f * plane.x * plane.x);
		mat.m01 = (- 2f * plane.x * plane.y);
		mat.m02 = (- 2f * plane.x * plane.z);
		mat.m03 = (- 2f * plane.x * plane.w);
		mat.m10 = (- 2f * plane.y * plane.x);
		mat.m11 = (1f - 2f * plane.y * plane.y);
		mat.m12 = (- 2f * plane.y * plane.z);
		mat.m13 = (- 2f * plane.y * plane.w);
		mat.m20 = (- 2f * plane.z * plane.x);
		mat.m21 = (- 2f * plane.z * plane.y);
		mat.m22 = (1f - 2f * plane.z * plane.z);
		mat.m23 = (- 2f * plane.z * plane.w);
		mat.m30 = 0f;
		mat.m31 = 0f;
		mat.m32 = 0f;
		mat.m33 = 1f;
	}

	private void create_objects(Camera camera, out Camera reflection_camera)
	{
		if (reflection_texture_ == null) {
			const int textureSize = 1024;
			reflection_texture_ = new RenderTexture(textureSize, textureSize, 16);
			reflection_texture_.name = "__WaterReflection" + GetInstanceID();
			reflection_texture_.isPowerOfTwo = true;
			reflection_texture_.hideFlags = HideFlags.DontSave;
		}

		reflection_camera_hash_.TryGetValue(camera, out reflection_camera);
		if (reflection_camera == null) {
			var go = new GameObject("ReflectionCamera" + GetInstanceID() + " for " + camera.GetInstanceID(), typeof(Camera), typeof(Skybox));
			reflection_camera = go.GetComponent<Camera>();
			reflection_camera.enabled = false;
			reflection_camera.useOcclusionCulling = false;
			reflection_camera.transform.position = transform.position;
			reflection_camera.transform.rotation = transform.rotation;
			go.hideFlags = HideFlags.HideAndDontSave;
			reflection_camera_hash_[camera] = reflection_camera;
		}
	}

	void Start()
	{
		var mr = GetComponent<MeshRenderer>();
		mr.sharedMaterial = surface_material_;
	}

	void OnDisable()
	{
		DestroyImmediate(reflection_texture_);
		reflection_texture_ = null;
		foreach (var kvp in reflection_camera_hash_) {
			DestroyImmediate((kvp.Value).gameObject);
		}
		reflection_camera_hash_.Clear();
	}

	void Update()
	{
	}

	void OnWillRenderObject()
	{
		Camera camera = Camera.current;
		Camera reflection_camera;
		create_objects(camera, out reflection_camera);

		var reflection_matrix = new Matrix4x4();
		calc_reflection_matrix(ref reflection_matrix, reflectionPlane);

		var local_reflection_matrix = camera.worldToCameraMatrix * reflection_matrix;
	    {
			var normal = new Vector3(reflectionPlane.x, reflectionPlane.y, reflectionPlane.z);
			Vector3 cnormal = local_reflection_matrix.MultiplyVector(normal);
			Vector3 cpos = local_reflection_matrix.MultiplyPoint(Vector3.zero);
			Vector4 clip_plane = new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
			reflection_camera.worldToCameraMatrix = local_reflection_matrix;
			reflection_camera.projectionMatrix = camera.CalculateObliqueMatrix(clip_plane);
			reflection_camera.cullingMask = ~(1 << 4) & reflectLayers.value; // never render water layer
			reflection_camera.targetTexture = reflection_texture_;
		}
	    {
			Vector3 campos = camera.transform.position;
			Vector3 refcampos = reflection_matrix.MultiplyPoint(campos);
			reflection_camera.farClipPlane = camera.farClipPlane;
			reflection_camera.nearClipPlane = camera.nearClipPlane;
			reflection_camera.transform.position = refcampos;
			var euler = camera.transform.eulerAngles;
			reflection_camera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
		}
		{
			bool prev = GL.invertCulling;
			GL.invertCulling = !prev;
			reflection_camera.Render();
			GL.invertCulling = prev;
			surface_material_.SetTexture("_ReflectionTex", reflection_texture_);
		}
	}
}

} // namespace UTJ {
