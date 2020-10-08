using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using static CuttingObject;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ObjectToCut : MonoBehaviour
{

    public CuttingObject cuttingGameObject;
    private Vector3 normal;
    private Vector3 position;
    private Renderer rend;

    //store the shader used before assigning the script in order to put it back when removing it
    private Shader oldShader = null;
    // Use this for initialization

    private CutType cutType = CutType.None;
    private float sphereRadius = 1;

    private bool stop = false;

    //number of planes
    private const int n = 6;
    private Vector3[] boxPlanesNormal = new Vector3[n];
    private Vector3[] boxPlanesPos = new Vector3[n];

    private Vector3[] boxBaseNormals = new Vector3[n];

    void Start()
    {
        initBox();
        rend = GetComponent<Renderer>();

        if (rend == null)
        {
            rend = gameObject.AddComponent<MeshRenderer>();
            Material mat = Resources.Load<Material>("default");
            Material[] temp = GetComponent<MeshRenderer>().sharedMaterials;
            temp[0] = mat;
            GetComponent<MeshRenderer>().sharedMaterials = temp;
        }

        oldShader = rend.sharedMaterial.shader;
        rend.sharedMaterial.shader = Shader.Find("GenericCrossSection/GenericShader");

        if (cuttingGameObject)
        {
            normal = cuttingGameObject.transform.TransformVector(new Vector3(0, 0, -1));
            position = cuttingGameObject.transform.position;
            UpdateShaderProperties();
        }
        else
        {
            rend.sharedMaterial.SetFloat("_Stop", 1);
        }
    }


    void Update()
    {
        if (cuttingGameObject == null)
        {
            return;
        }
        ReadCuttingObjectProperties();
        UpdateShaderProperties();
    }

    public void AssignScripts(CuttingObject co)
    {
        cuttingGameObject = co;
        foreach (Transform child in transform)
        {
            child.gameObject.AddComponent<ObjectToCut>();
            child.gameObject.GetComponent<ObjectToCut>().AssignScripts(co);
        }
    }

    public void RemoveScripts()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<ObjectToCut>().RemoveScripts();
        }

        if (oldShader != Shader.Find("GenericCrossSection/GenericShader"))
            rend.sharedMaterial.shader = oldShader;

        cuttingGameObject = null;

        if (Application.isEditor)
        {
            DestroyImmediate(GetComponent<ObjectToCut>());
        }
        else
        {
            Destroy(GetComponent<ObjectToCut>());
        }
    }

    public void Notify(CuttingObject cutObjCont)
    {
        if (cuttingGameObject == cutObjCont)
        {
            ReadCuttingObjectProperties();
        }
        else
        {
            cutObjCont.Unsubscribe(this);
        }
    }

    private void ReadCuttingObjectProperties()
    {
        if (cuttingGameObject == null)
        {
            stop = true;
            return;
        }

        stop = !cuttingGameObject.enabled
            || !(Application.isPlaying || cuttingGameObject.cutInEditMode)
            || cuttingGameObject.cutType == CutType.None;

        normal = cuttingGameObject.transform.TransformVector(new Vector3(0, 0, -1));
        position = cuttingGameObject.transform.position;

        cuttingGameObject = cuttingGameObject.GetComponent<CuttingObject>();
        cuttingGameObject.Subscribe(this);

        cutType = cuttingGameObject.cutType;

        if (cutType == CutType.Sphere)
            sphereRadius = cuttingGameObject.sphereRadius;
    }

    private void UpdateShaderProperties()
    {
        if (rend == null || rend.sharedMaterial == null)
            return;

        rend.sharedMaterial.SetFloat("_Stop", stop ? 1 : 0);
        rend.sharedMaterial.SetFloat("_Reversed", cuttingGameObject.reversed ? 1 : 0);
        rend.sharedMaterial.SetVector("_PlaneNormal", normal);
        rend.sharedMaterial.SetVector("_PlanePosition", position);
        rend.sharedMaterial.SetInt("_CutType", (int)cutType);
        rend.sharedMaterial.SetFloat("_SphereRadius", sphereRadius);

        UpdateBox();
    }

    private void initBox()
    {
        boxBaseNormals[0] = new Vector3(1, 0, 0);
        boxBaseNormals[1] = new Vector3(-1, 0, 0);
        boxBaseNormals[2] = new Vector3(0, 1, 0);
        boxBaseNormals[3] = new Vector3(0, -1, 0);
        boxBaseNormals[4] = new Vector3(0, 0, 1);
        boxBaseNormals[5] = new Vector3(0, 0, -1);
    }

    private void UpdateBox()
    {
        //base norm => rotation*base norm
        //base vector => new pos        
        for (int i = 0; i < n; i++)
        {

            boxPlanesNormal[i] = cuttingGameObject.transform.TransformVector(boxBaseNormals[i]).normalized;
            boxPlanesPos[i] = cuttingGameObject.transform.position + Vector3.Scale(boxPlanesNormal[i], cuttingGameObject.transform.localScale / 2);

            rend.sharedMaterial.SetVector("_BoxPlanesNormal" + i, boxPlanesNormal[i]);
            rend.sharedMaterial.SetVector("_BoxPlanesPos" + i, boxPlanesPos[i]);
        }
    }
}
