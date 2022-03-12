using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class RenderToTexel : EditorWindow {
	
	Texture2D normalMap;
	
	int numberOfBakeTargets = 1;
	int lastNumberOfBakeTargets = 1;
	BakeTarget[] bakeTargets;
	
	bool worldSpaceCoordsDirty = true;
	
	Transform myTransform;
	
	Texture2D worldSpaceNormal;
	Texture2D worldSpacePos;
	Vector3 worldSpaceMin;
	Vector3 worldSpaceMax;
	
	Vector3 oldPosition;
	Quaternion oldRotation;
	
	int texSizeX = 1024;
	int texSizeY = 1024;
	float texSizeXRecip = 1;
	float texSizeYRecip = 1;
	
	[System.Serializable]
	public class BakeTarget { 
		public BakeTarget () {
			numberOfLights = 1;
			lastNumberOfLights = 1;
			lights = new Light[1];
			numberOfVantagePoints = 1;
			lastNumberOfVantagePoints = 1;
			vantagePoints = new Transform[1];
		}
		public Material material;
		public int numberOfLights;
		public int lastNumberOfLights;
		public Light[] lights;
		public int numberOfVantagePoints;
		public int lastNumberOfVantagePoints;
		public Transform[] vantagePoints;
	}
	
	string path;
	
	[MenuItem ("Window/Render to Texel")]
	static void Init () {
		RenderToTexel window = (RenderToTexel)EditorWindow.GetWindow (typeof (RenderToTexel));
		window.Show ();
		window.Prepare();
	}
	
	public void Prepare () {
		bakeTargets = new BakeTarget[1];
		bakeTargets[0] = new BakeTarget();
	}
	
	void OnGUI () {

		GUILayout.Space (10);
		myTransform = EditorGUILayout.ObjectField ("To Bake: ", myTransform, typeof (Transform), true) as Transform;
		
		if(!myTransform) {
			GUILayout.Label ("Assign an object to bake", EditorStyles.boldLabel);
			return;
		}
		
		if(myTransform.GetComponent<Renderer>() && myTransform.GetComponent<Renderer>().sharedMaterial.HasProperty("_BumpMap")) {
			normalMap = myTransform.GetComponent<Renderer>().sharedMaterial.GetTexture("_BumpMap") as Texture2D;
		}
		
		
		normalMap = EditorGUILayout.ObjectField ("Normal Map: ", normalMap, typeof (Texture2D), true) as Texture2D;
		
		
		if(!normalMap) {
			GUILayout.Label ("Requires a tangent space normal map", EditorStyles.boldLabel);
			return;
		}
		if(normalMap.format != TextureFormat.ARGB32) {
			GUILayout.Label ("Please set normal map format to \"Automatic Truecolor\" or ARGB32. Must Be a Normal Map. Current Format: " + normalMap.format, EditorStyles.boldLabel);
			return;
		}
		
		numberOfBakeTargets = EditorGUILayout.IntField ("Number of Bake Targets: ", numberOfBakeTargets, GUILayout.Width(200));
		if(numberOfBakeTargets < 0) numberOfBakeTargets = 0; if(numberOfBakeTargets > 100) numberOfBakeTargets = 100;
		if(lastNumberOfBakeTargets != bakeTargets.Length) lastNumberOfBakeTargets = bakeTargets.Length;
		
		if(numberOfBakeTargets != lastNumberOfBakeTargets) {
			bakeTargets = ResizeArray<BakeTarget>(bakeTargets, numberOfBakeTargets);
			for(int i = lastNumberOfBakeTargets; i < numberOfBakeTargets; i++) {
				bakeTargets[i] = new BakeTarget();
			}
			lastNumberOfBakeTargets = numberOfBakeTargets;
		}
		
		for(int i = 0; i < bakeTargets.Length; i++) {
			GUILayout.Space (5);
			BakeTarget bti = bakeTargets[i];
			bti.material = EditorGUILayout.ObjectField ("    ["+i+"].material: ", bti.material, typeof (Material), true) as Material;
			
			bti.numberOfLights = EditorGUILayout.IntField ("    ["+i+"].numberOfLights: ", bti.numberOfLights, GUILayout.Width(220));
			if(bti.numberOfLights != bti.lastNumberOfLights) {
				bti.lights = ResizeArray<Light>(bti.lights, bti.numberOfLights);
				bti.lastNumberOfLights = bti.numberOfLights;
			}
			
			for(int ii = 0; ii < bti.lights.Length; ii++) {
				bti.lights[ii] = EditorGUILayout.ObjectField ("        ["+i+"].lights["+ii+"]: ", bti.lights[ii], typeof (Light), true) as Light;
			}
			
			bti.numberOfVantagePoints = EditorGUILayout.IntField ("    ["+i+"].numberOfVantagePoints: ", bti.numberOfVantagePoints, GUILayout.Width(220));
			if(bti.numberOfVantagePoints != bti.lastNumberOfVantagePoints) {
				bti.vantagePoints = ResizeArray<Transform>(bti.vantagePoints, bti.numberOfVantagePoints);
				bti.lastNumberOfVantagePoints = bti.numberOfVantagePoints;
			}
			
			for(int ii = 0; ii < bti.vantagePoints.Length; ii++) {
				bti.vantagePoints[ii] = EditorGUILayout.ObjectField ("        ["+i+"].vantagePoints["+ii+"]: ", bti.vantagePoints[ii], typeof (Transform), true) as Transform;
			}
		}
		
		string bakeError = "";
		if(bakeTargets.Length == 0) bakeError = "There are zero bake targets";
		for(int i = 0; i < bakeTargets.Length; i++) {
			if(bakeTargets[i] == null) bakeError = "Bake target "+i+" is null, aborting";

			if(!bakeTargets[i].material)  bakeError = "Bake target "+i+" has no material, aborting";
			
			if(!bakeTargets[i].material || !bakeTargets[i].material.HasProperty("_WorldNormal") || !bakeTargets[i].material.HasProperty("_WorldPos") ) {
				bakeError = "Bake target "+i+"'s material does not have both _WorldNormal and _WorldPos properties, aborting";
			}
			if(!bakeTargets[i].material || bakeTargets[i].material.passCount < 2) bakeError = "Bake target "+i+"'s material does not have at least two passes, aborting";

			if(bakeTargets[i].lights.Length == 0) bakeError = "Bake target "+i+" has no lights, aborting";
			
			if(bakeTargets[i].vantagePoints.Length == 0) bakeError = "Bake target "+i+" has no vantagePoints, aborting";
		}
		
		if(bakeError != "") {
			GUILayout.Label (bakeError, EditorStyles.boldLabel);
			return;
		}
		GUILayout.Space (10);
		if (GUILayout.Button ("Bake It!", GUILayout.MaxWidth (200))) {
			Bake ();
		}
	}
	
	T[] ResizeArray<T> (T[] arr, int newSize) {
		T[] newArray = new T[newSize];
		for(int i = 0; i < newSize; i++) {
			if(i < arr.Length) {
				newArray[i] = arr[i];
			}
		}
		return newArray;
	}
	
	void Bake () {
		texSizeX = normalMap.width;
		texSizeY = normalMap.height;
		texSizeXRecip = 1f/texSizeX;
		texSizeYRecip = 1f/texSizeY;
		
		if(oldPosition != myTransform.position || oldRotation != myTransform.rotation) {
			worldSpaceCoordsDirty = true;
		}
		
		if(!worldSpaceNormal || !worldSpacePos || worldSpaceCoordsDirty) {
			UpdateWorldSpaceCoords();
			oldPosition = myTransform.position;
			oldRotation = myTransform.rotation;
		}
		
		path = EditorUtility.SaveFilePanelInProject("Save texture as PNG", path, "png", "Please enter a file name to save the texture to");
		
		string path1 = path.Substring(0, path.LastIndexOf(".png"));
		
		for(int i = 0; i < bakeTargets.Length; i++) {
			RenderTexture rt = new RenderTexture(texSizeX, texSizeY, 0, RenderTextureFormat.ARGB32);

			// convention: Pass 0 is ambient, pass 1 is per-pixel light pass.
			Graphics.Blit(null, rt, bakeTargets[i].material, 0);
			
			float recip = 1f/bakeTargets[i].vantagePoints.Length;
			
			for(int li = 0; li < bakeTargets[i].lights.Length; li++) {
				for(int vi = 0; vi < bakeTargets[i].vantagePoints.Length; vi++) {
					if(bakeTargets[i].lights[li] && bakeTargets[i].vantagePoints[vi]) {
						SetupMaterial(bakeTargets[i].material, bakeTargets[i].lights[li], bakeTargets[i].vantagePoints[vi], recip);
						Graphics.Blit(null, rt, bakeTargets[i].material, 1);
					}
				}
			}
			
			
			Texture2D tex = new Texture2D(texSizeX, texSizeY, TextureFormat.RGB24, true);
			
			RenderTexture.active = rt;
			tex.ReadPixels(new Rect(0, 0, texSizeX, texSizeY), 0, 0, true);
			if(path.Length != 0 && path1.Length != 0) {
				byte[] bytes = tex.EncodeToPNG();
				if(bytes != null) {
					File.WriteAllBytes(path1+i+".png", bytes);
				}
			}
		}
		AssetDatabase.Refresh();
		
	}
	
	// r is used to average samples from different vantage points. it is the reciprocal of the number of vantage points.
	void SetupMaterial (Material mat, Light l, Transform v, float r) {
		mat.SetTexture("_WorldNormal", worldSpaceNormal);
		mat.SetTexture("_WorldPos", worldSpacePos);
		mat.SetVector("_WorldSpaceMin", new Vector4(worldSpaceMin.x, worldSpaceMin.y, worldSpaceMin.z, 0));
		mat.SetVector("_WorldSpaceSize", new Vector4(worldSpaceMax.x-worldSpaceMin.x, worldSpaceMax.y-worldSpaceMin.y, worldSpaceMax.z-worldSpaceMin.z, 0));
		
		mat.SetVector("_LightPos", new Vector4(l.transform.position.x, l.transform.position.y, l.transform.position.z, l.range));
		float i = l.intensity * r;
		mat.SetVector("_LightColor", new Vector4(l.color.r * i, l.color.g * i, l.color.b * i, 0));
		
		mat.SetVector("_ViewPos", new Vector4(v.transform.position.x, v.transform.position.y, v.transform.position.z, 0));
	}

	void UpdateWorldSpaceCoords () {
		MeshFilter mf = myTransform.GetComponent<MeshFilter>();
		Mesh mesh = mf.sharedMesh;
		Vector3[] verts = mesh.vertices;
		Vector2[] uvs = mesh.uv;
		Vector3[] normals = mesh.normals;
		Vector4[] tangents = mesh.tangents;
		int[] tris = mesh.triangles;
		
		worldSpaceNormal = new  Texture2D (texSizeX, texSizeY, TextureFormat.RGB24, false);
		worldSpacePos = new  Texture2D (texSizeX, texSizeY, TextureFormat.RGB24, false);
		
		worldSpaceMin = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
		worldSpaceMax = new Vector3(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity);
		
		for(int i =0; i < verts.Length; i++) {
			Vector3 w = myTransform.TransformPoint(verts[i]);
			if(worldSpaceMin.x > w.x) worldSpaceMin.x = w.x; if(worldSpaceMax.x < w.x) worldSpaceMax.x = w.x;
			if(worldSpaceMin.y > w.y) worldSpaceMin.y = w.y; if(worldSpaceMax.y < w.y) worldSpaceMax.y = w.y;
			if(worldSpaceMin.z > w.z) worldSpaceMin.z = w.z; if(worldSpaceMax.z < w.z) worldSpaceMax.z = w.z;
		}
		
		//Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(1, 0, 0), Color.white);
		//Debug.DrawLine(new Vector3(1, 0, 0), new Vector3(1, 1, 0), Color.white);
		//Debug.DrawLine(new Vector3(1, 1, 0), new Vector3(0, 1, 0), Color.white);
		//Debug.DrawLine(new Vector3(0, 1, 0), new Vector3(0, 0, 0), Color.white);
		
		for(int i = 0; i < tris.Length; i+=3) {
			float xMin = 1;	float yMin = 1;
			float xMax = 0;	float yMax = 0;
			// UV coordinates are based on the lower lefthand corner and pixel coordinates are based on the upper left, so we will invert the Y
			Vector2 uv0 = uvs[tris[i  ]]; //uv0.y = 1f-uv0.y;
			Vector2 uv1 = uvs[tris[i+1]]; //uv1.y = 1f-uv1.y;
			Vector2 uv2 = uvs[tris[i+2]]; //uv2.y = 1f-uv2.y;
			
			//Debug.DrawLine(new Vector3(uv0.x, uv0.y, 0), new Vector3(uv1.x, uv1.y, 0), Color.green);
			//Debug.DrawLine(new Vector3(uv1.x, uv1.y, 0), new Vector3(uv2.x, uv2.y, 0), Color.green);
			//Debug.DrawLine(new Vector3(uv2.x, uv2.y, 0), new Vector3(uv0.x, uv0.y, 0), Color.green);
			
			//Construct a Rect that encapsulates the triangle
			if(uv0.x < xMin) xMin = uv0.x; if(uv0.x > xMax) xMax = uv0.x; if(uv0.y < yMin) yMin = uv0.y; if(uv0.y > yMax) yMax = uv0.y;
			if(uv1.x < xMin) xMin = uv1.x; if(uv1.x > xMax) xMax = uv1.x; if(uv1.y < yMin) yMin = uv1.y; if(uv1.y > yMax) yMax = uv1.y;
			if(uv2.x < xMin) xMin = uv2.x; if(uv2.x > xMax) xMax = uv2.x; if(uv2.y < yMin) yMin = uv2.y; if(uv2.y > yMax) yMax = uv2.y;
			
			//Prepare variables for this triangle
			Vector3 v0 = verts[tris[i  ]];	Vector3 n0 = normals[tris[i  ]];  Vector4 tt0 = tangents[tris[i  ]];
			Vector3 v1 = verts[tris[i+1]];	Vector3 n1 = normals[tris[i+1]];  Vector4 tt1 = tangents[tris[i+1]];
			Vector3 v2 = verts[tris[i+2]];	Vector3 n2 = normals[tris[i+2]];  Vector4 tt2 = tangents[tris[i+2]];
			//HeronsForumula finds the area of a triangle based on its side lengths (The vertex positions are used as inputs here)
			float area = HeronsForumula(uv0, uv1, uv2);
			
			bool GLES = (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.iOS 
				|| EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android 
				//|| EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.GLESEmu
				|| EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android);
			
			// Crop rect to size of texture and iterate through every pixel in that rect
			int pixMinX = (int)Mathf.Max(Mathf.Floor(xMin*texSizeX)-1, 0); int pixMaxX = (int)Mathf.Min(Mathf.Ceil(xMax*texSizeX)+1, texSizeX); 
			int pixMinY = (int)Mathf.Max(Mathf.Floor(yMin*texSizeY)-1, 0); int pixMaxY = (int)Mathf.Min(Mathf.Ceil(yMax*texSizeY)+1, texSizeY);
			
			for(int x = pixMinX; x < pixMaxX; x++) {
				for(int y = pixMinY; y < pixMaxY; y++) {
					Vector2 pixel = new Vector2(((float)x)*texSizeXRecip, ((float)y)*texSizeYRecip);
					//Early rejection of pixel if outside of triangle or outside of texture.
					
					if(Vector2.Angle(uv0-pixel, uv1-pixel) + Vector2.Angle(uv1-pixel, uv2-pixel) + Vector2.Angle(uv2-pixel, uv0-pixel) > 359.9) {
						//For constructing the barycentric coordinate of this pixel within this triangle, we use the area of the main triangle, and the areas of the
						// triangles that result when we subdivide the triangle by drawing lines from each of its vertices to the pixel.
						
						//Find the areas of the triangles formed by this subdivision, indexed such that the triangle is across from, not next to, its vertex
						float a0 = HeronsForumula(pixel, uv1, uv2);
						float a1 = HeronsForumula(uv0, pixel, uv2);
						float a2 = HeronsForumula(uv0, uv1, pixel);
						
						// the position of the pixel is composed of the position of each vertex. the percentage or "wieght" of each vertex in this position
						// is obtained by taking the ratio of the opposing triangle's area to the total area
						Vector3 bary = new Vector3(a0/area, a1/area, a2/area);
						
						// encode world space relative to bounding volume to enhance precision
						Vector3 pixelPos = myTransform.TransformPoint(bary.x*v0+bary.y*v1+bary.z*v2);
						pixelPos = new Vector3( Mathf.InverseLerp(worldSpaceMin.x, worldSpaceMax.x, pixelPos.x),
												Mathf.InverseLerp(worldSpaceMin.y, worldSpaceMax.y, pixelPos.y),
												Mathf.InverseLerp(worldSpaceMin.z, worldSpaceMax.z, pixelPos.z));
						
						// prepare Tangent Space Transformation Matrix
						Vector3 pixelZ = (bary.x*n0+bary.y*n1+bary.z*n2).normalized; // normal
						Vector4 tangent = bary.x*tt0+bary.y*tt1+bary.z*tt2;
						Vector3 pixelX = new Vector3(tangent.x, tangent.y, tangent.z).normalized; // tangent
						Vector3 pixelY = Vector3.Cross(pixelZ, pixelX);	 // binormal
						
						Color c = normalMap.GetPixel(x,y);
						
						// Depending on build target the normals are compressed differently. See "UnpackNormal" in the cginc files
						Vector3 tangentSpaceNormal;
						if(GLES) {
							tangentSpaceNormal = new Vector3(c.r*2 - 1f, c.g*2 - 1f, c.b*2 - 1f);
						} else {
							tangentSpaceNormal = new Vector3(c.a*2 - 1f, c.g*2 - 1f, 0);
							tangentSpaceNormal.z = Mathf.Sqrt(1f - tangentSpaceNormal.x*tangentSpaceNormal.x - tangentSpaceNormal.y*tangentSpaceNormal.y);
						}
						
						// Simply multiply to get world space normal!
						
						Vector3 worldNormal = tangentSpaceNormal.x*pixelX + tangentSpaceNormal.y*pixelY + tangentSpaceNormal.z*pixelZ;
						
						worldSpaceNormal.SetPixel(x, y, new Color(worldNormal.x*0.5f + 0.5f, worldNormal.y*0.5f + 0.5f, worldNormal.z*0.5f + 0.5f));
						worldSpacePos.SetPixel(x, y, new Color(pixelPos.x, pixelPos.y, pixelPos.z));
					}
				}
			}
		}
		
		worldSpaceNormal.Apply ();
		worldSpacePos.Apply ();
	}
	
	float HeronsForumula (Vector2 a, Vector2 b, Vector2 c) {
		float ab = (a-b).magnitude;
		float bc = (b-c).magnitude;
		float ca = (c-a).magnitude;
		float s = 0.5f * (ab + bc + ca);
		return Mathf.Sqrt(s*(s-ab)*(s-bc)*(s-ca));				
	}
}
