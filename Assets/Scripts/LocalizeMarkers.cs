using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors;

public class LocalizeMarkers : MonoBehaviour
{

    public PersistentCloudAnchorsController Controller;


    private CloudAnchorHistoryCollection _history = new CloudAnchorHistoryCollection();

    private List<GameObject> placedMarkers;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void FindPath()
    {
        if(placedMarkers.Count > 0)
        {
            Debug.Log("Finding path...");

        }
    }

    public void AddMarkers(GameObject marker)
    {
        placedMarkers.Add(marker);
    }
}
