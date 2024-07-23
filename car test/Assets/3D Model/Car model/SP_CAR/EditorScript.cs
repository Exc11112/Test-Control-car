using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class EditorScript : EditorWindow
{
    [MenuItem("tools/vehicle")]

    public static void Open()
    {
        GetWindow<EditorScript>();
    }

    public GameObject car;

    public GameObject[] wheels;

    void OnGUI()
    {
        runChecks();

        SerializedObject R1 = new SerializedObject(this);
        EditorGUILayout.PropertyField(R1.FindProperty("car"));
        R1.ApplyModifiedProperties();

        SerializedObject h2 = new SerializedObject(this);
        EditorGUILayout.PropertyField(h2.FindProperty("wheels"));
        h2.ApplyModifiedProperties();

        buttos();
    }

    void runChecks()
    {
        if (wheels.Length == 2 || wheels.Length == 1) return; 
        

        if(wheels == null)
        {
            wheels = new GameObject[0];
        }

        if(wheels != null)
        {
            if(wheels.Length >= 2) 
            {
                wheels = new GameObject[2];
            }
            if(wheels.Length == 0) 
            {
                wheels = new GameObject[1];
            }
        }
    }

    void buttos()
    {
        if(car != null)

        if (GUILayout.Button("create", GUILayout.Height(40)))
        {
            createComponents();
        }
        if (GUILayout.Button("spawn wheels", GUILayout.Height(40)))
        {
            spawnWheels();
        }
    }

    void spawnWheels()
    {
        if(wheels.Length == 1)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject a = Instantiate(wheels[0]);
                a.transform.parent = car.gameObject.transform;
                a.transform.position = Vector3.zero;
                a.transform.localPosition = Vector3.zero;
                a.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    void createComponents()
    {
        Rigidbody A = null;

        try
        {
            A = car.GetComponent<Rigidbody>();
        }
        catch { }

        if (A != null)
        {
            A = car.AddComponent<Rigidbody>();
            A.mass = 1200;
        }
    }
}
