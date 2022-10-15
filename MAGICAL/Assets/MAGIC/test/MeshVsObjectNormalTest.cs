using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshVsObjectNormalTest : MonoBehaviour
{
	//public GameObject normalObject;
	Mesh[] rawMesh;
	Matrix4x4 objectMatrix;
	Camera cam;
	Material mat;

    // Start is called before the first frame update
    void Start()
	{
        rawMesh = new Mesh[1];
		rawMesh[0] = this.GetComponent<MeshFilter>().mesh;
		
        // objectMatrix = Matrix4x4.identity;
        objectMatrix.SetTRS(
                    new Vector3(32, 0, 32),     //position
                                                // new Vector3(32,0,32),		//position
                    Quaternion.Euler(
                        -90,
                        0,
                        0
                    ),                      //rotation
                                            // new Vector3(64,1,64)		//scale
                    new Vector3(100, 100, 100)     //scale
                );


		cam = Camera.main;
		mat = this.GetComponent<Renderer>().material;
		
    }

    // Update is called once per frame
    //void Update()
	void LateUpdate()
    //void OnPostRender()
    {
	    //Graphics.DrawMesh(rawMesh[0], objectMatrix, mat,0, cam);
	    Graphics.DrawMesh(rawMesh[0], objectMatrix, mat,0);
	    //Graphics.DrawMesh(rawMesh[0], Vector3.zero, Quaternion.identity, mat,0, cam);
	    // Graphics.DrawMesh(rawMesh[0], Vector3.zero, Quaternion.identity, mat,0);
	    
	    //mat.SetPass(0);
	    //Graphics.DrawMeshNow(rawMesh[0], objectMatrix);
	    
	    Debug.Log(cam);
    }
    
}
