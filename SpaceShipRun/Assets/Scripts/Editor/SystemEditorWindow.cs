using System;
using System.Collections.Generic;
using System.Linq;
using Mechanics;
using UnityEditor;
using UnityEngine;


public class SystemEditorWindow : EditorWindow
{
    private List<PlanetData> _planets = new List<PlanetData>();
    private GameObject _planetPrefab;
    private GameObject _solar;
    
    [MenuItem("Window/System Window")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SystemEditorWindow));
    }

    private void OnEnable()
    {
        _solar = GameObject.Find("Solar");
        _planetPrefab = Resources.Load("Prefabs/Planet", typeof(GameObject)) as GameObject;
        UpdateCollection();
    }

    private void UpdateCollection()
    {
        _planets.Clear();
        var planetOrbits = FindObjectsOfType<PlanetOrbit>();
        foreach (var planetOrbit in planetOrbits)
        {
            if (!planetOrbit.gameObject.activeSelf)
            {
                continue;
            }
            _planets.Add(planetOrbit.PrepareData());
        }

        _planets.Sort((data1, data2) => data1.position.sqrMagnitude.CompareTo(data2.position.sqrMagnitude));
    }

    private void UpdatePlanet(PlanetData planetData)
    {
        planetData.planetOrbit.UpdateFromData(planetData);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh", GUILayout.Width(60f)))
        {
            UpdateCollection();
        }
        
        GUILayout.Label("Planets", EditorStyles.boldLabel);
        
        PlanetData planetToRemove = null;
        foreach (var planet in _planets)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            planet.name = EditorGUILayout.TextField(planet.name, GUILayout.Width(95f));
            EditorGUIUtility.labelWidth = 14f;
            planet.position.x = EditorGUILayout.FloatField("X", planet.position.x);
            planet.position.z = EditorGUILayout.FloatField("Z", planet.position.z);
            EditorGUIUtility.labelWidth = 25f;
            planet.offsetCos = EditorGUILayout.FloatField("Cos", planet.offsetCos);
            planet.offsetSin = EditorGUILayout.FloatField("Sin", planet.offsetSin);
            EditorGUIUtility.labelWidth = 35f;
            planet.scale = EditorGUILayout.FloatField("Size", planet.scale);
            planet.rotationSpeed = EditorGUILayout.FloatField("Day", planet.rotationSpeed);
            planet.circleInSecond = EditorGUILayout.FloatField("Year", planet.circleInSecond);
            EditorGUIUtility.labelWidth = 60f;
            planet.viewDistance = EditorGUILayout.FloatField("View Dist", planet.viewDistance);

            if (EditorGUI.EndChangeCheck())
            {
                UpdatePlanet(planet);
            }
            
            if (GUILayout.Button("-", GUILayout.Width(18f)))
            {
                planetToRemove = planet;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (planetToRemove != null)
        {
            _planets.Remove(planetToRemove);
            DestroyImmediate(planetToRemove.planetOrbit.gameObject);
        }

        if (GUILayout.Button("+", GUILayout.Width(30f)))
        {
            _planets.Add(AddPlanet());
        }
    }

    private PlanetData AddPlanet()
    {
        var planet = Instantiate(_planetPrefab);
        planet.name = "Planet";
        var planetOrbit = planet.GetComponent<PlanetOrbit>();
        planetOrbit.SetAroundPoint(_solar.transform);
        int planetsCount = _planets.Count;
        if (planetsCount > 0)
        {
            planetOrbit.CopyFrom(_planets[planetsCount-1].planetOrbit);
        }

        return planetOrbit.PrepareData();
    }
}
