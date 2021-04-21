using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class LocalizeMarkers : MonoBehaviour
{
    /// <summary>
    /// The main controller.
    /// </summary>
    public UIController Controller;

    /// <summary>
    /// Current view manager.
    /// </summary>
    public ViewManager ViewManager;

    /// <summary>
    /// The 3D object that represents a pointer for navigation.
    /// </summary>
    public GameObject pointer;

    /// <summary>
    /// The main camera.
    /// </summary>
    public GameObject ARCamera;

    /// <summary>
    /// The active ARSessionOrigin used in the example.
    /// </summary>
    public ARSessionOrigin SessionOrigin;

    public Dictionary<string, List<GameObject>> Routes;
    
    [HideInInspector]
    public string routeName;

    private List<GameObject> placedMarkers;

    private List<GameObject> savedMarkers;

    private Vector3 targetDirection, newDirection;

    private float distanceToMarker;

    private int currentMarker;

    /// <summary>
    /// Unity start method.
    /// </summary>

    public void Start()
    {
        pointer.SetActive(false);
        placedMarkers = new List<GameObject>();

        Routes = new Dictionary<string, List<GameObject>>();
    }

    /// <summary>
    /// Unity update method.
    /// </summary>

    void Update()
    {

    }

    /// <summary>
    /// Add passed marker into list of markers.
    /// </summary>

    public void AddMarkers(GameObject marker)
    {
        placedMarkers.Add(marker);
    }

    /// <summary>
    /// Clear all markers from list.
    /// </summary>
    public void ClearMarkers()
    {
        pointer.SetActive(false);

        if (placedMarkers.Count > 0)
        {
            foreach (GameObject marker in placedMarkers)
            {
                marker.SetActive(false);
            }
        }
            
        if(savedMarkers.Count > 0)
        {
            foreach(GameObject marker in savedMarkers)
            {
                marker.SetActive(false);
            }
        }

        placedMarkers = new List<GameObject>();
        savedMarkers = new List<GameObject>();
    }

    /// <summary>
    /// Save given route into route dictionary.
    /// </summary>
    public bool SaveRoute(string routeName)
    {
        if (Routes.ContainsKey(routeName))
            return false;

        Routes.Add(routeName, placedMarkers);

        foreach(GameObject marker in placedMarkers)
        {
            marker.SetActive(false);
        }
        placedMarkers = new List<GameObject>();

        return true;
    }

    /// <summary>
    /// Find path using current selected route.
    /// </summary>
    public void FindPath()
    {

        Debug.Log("Finding Path to " + routeName);

        savedMarkers = new List<GameObject>(Routes[routeName]);

        Debug.Log("savedMarkers : " + savedMarkers.Count);

        if (savedMarkers.Count > 0)
        {
            StartCoroutine("PointerUpdate");
        }
    }

    /// <summary>
    /// Coroutine method.
    /// </summary>
    IEnumerator PointerUpdate()
    {
        pointer.SetActive(true);

        currentMarker = 0;
        
        while (currentMarker < savedMarkers.Count)
        {
            savedMarkers[currentMarker].SetActive(true);
           
            distanceToMarker = Vector3.Distance(SessionOrigin.camera.transform.position, savedMarkers[currentMarker].transform.position);
            Debug.Log("Distance to marker (" + currentMarker + ") : " + distanceToMarker);

            targetDirection = SessionOrigin.camera.transform.position - savedMarkers[currentMarker].transform.position;
            pointer.transform.rotation = Quaternion.LookRotation(-targetDirection);

            if (distanceToMarker < 2f)
            {
                savedMarkers[currentMarker].SetActive(false);
                currentMarker++;
            }

            yield return new WaitForSeconds(0.1f);
        }

        //yield return 0;

        pointer.SetActive(false);
        Controller.OnReachingDestination();
    }
}
