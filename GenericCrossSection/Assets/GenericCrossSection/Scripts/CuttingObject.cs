using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CuttingObject : MonoBehaviour
{

    private List<ObjectToCut> listeners = new List<ObjectToCut>();

    public enum CutType
    {
        None = 0,
        Plane = 1,
        Sphere = 2,
        Box = 3
    };

    public CutType cutType = CutType.Plane;

    public float sphereRadius = 1;

    public bool reversed = false;
    public bool cutInEditMode = true;

    [SerializeField]
    internal List<GameObject> objectsToCut = new List<GameObject>(0);
    internal List<GameObject> oldObjectsToCut = new List<GameObject>(0);


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateProperties();
    }

    public void UpdateProperties()
    {

        if(this == null)
            return;

        if (GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
         
        if (GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
            Material mat = Resources.Load<Material>("DoubleFaceTransparent");
            Material[] temp = GetComponent<MeshRenderer>().sharedMaterials;
            temp[0] = mat;
            GetComponent<MeshRenderer>().sharedMaterials = temp;
        }

        if (cutType == CutType.None)
        {
            GetComponent<MeshFilter>().mesh = null;
        }
        else if (cutType == CutType.Sphere)
        {
            GetComponent<MeshFilter>().mesh = PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Sphere);
            float scale = sphereRadius * 2;
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else if (cutType == CutType.Plane)
        {
            GetComponent<MeshFilter>().mesh = PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Quad);
        }
        else if (cutType == CutType.Box)
        {
            GetComponent<MeshFilter>().mesh = PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube);
        }

    }

    public void AssignCuttingScript()
    {
        IEnumerable<GameObject> newGameObjects = objectsToCut.Except(oldObjectsToCut);
        IEnumerable<GameObject> oldGameObjects = oldObjectsToCut.Except(objectsToCut);

        foreach(GameObject go in newGameObjects)
        {
            go.AddComponent<ObjectToCut>().AssignScripts(this);
        }

        foreach(GameObject go in oldGameObjects)
        {
            if(go.GetComponent<ObjectToCut>() != null)
            {
                go.GetComponent<ObjectToCut>().RemoveScripts();
            }
        }

        oldObjectsToCut = new List<GameObject>(objectsToCut);
    }

    public void Subscribe(ObjectToCut genericCuttingController)
    {
        if( !listeners.Contains(genericCuttingController) )
            listeners.Add(genericCuttingController);
    }

    public void Unsubscribe(ObjectToCut genericCuttingController)
    {
        listeners.Remove(genericCuttingController);
    }
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CuttingObject))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var myScript = target as CuttingObject;


        myScript.cutType = (CuttingObject.CutType) EditorGUILayout.EnumPopup("object type", myScript.cutType);

        if (myScript.cutType == CuttingObject.CutType.Sphere)
            myScript.sphereRadius = EditorGUILayout.FloatField("Sphere Radius:", myScript.sphereRadius);

        myScript.reversed = (bool)EditorGUILayout.Toggle("Reversed", myScript.reversed);
        myScript.cutInEditMode = (bool)EditorGUILayout.Toggle("Cut in edit mode", myScript.cutInEditMode);

        //myScript.objectsToCut = (List<GameObject>) EditorGUILayout.L
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectsToCut"),true);
        serializedObject.ApplyModifiedProperties();
        if (GUILayout.Button("Assign cutting controler"))
        {
            myScript.AssignCuttingScript();
        }
    }

    void OnEnable() {
        var myScript = target as CuttingObject;
        EditorApplication.update += myScript.UpdateProperties;
    }
    void OnDisable() {
        var myScript = target as CuttingObject;
        EditorApplication.update -= myScript.UpdateProperties;
    }
    void OnDestroy()
    {
        var myScript = target as CuttingObject;
        EditorApplication.update -= myScript.UpdateProperties;
    }
}

