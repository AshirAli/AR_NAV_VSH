using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class LocalizeMarkers : MonoBehaviour
{

    public PersistentCloudAnchorsController Controller;

    //private CloudAnchorHistoryCollection _history = new CloudAnchorHistoryCollection();

    private List<GameObject> placedMarkers;

    public GameObject pointer;

    public GameObject routeAnchorPrefab;

    [HideInInspector]
    public bool routeSetting;

    Vector3 targetDirection, newDirection;

    float distanceToMarker;



    public void Start()
    {
        pointer.SetActive(false);
        placedMarkers = new List<GameObject>();
        routeSetting = false;
    }

    void Update()
    {
        if (routeSetting)
        {
            PlaceRouteObjects();
        }
    }

    public void FindPath()
    {
        if(placedMarkers.Count > 0)
        {
            pointer.SetActive(true);
            Debug.Log("Finding path...");
            foreach(GameObject marker in placedMarkers)
            {
                targetDirection = marker.transform.position - pointer.transform.position;
                distanceToMarker = Vector3.Distance(marker.transform.position, pointer.transform.position);
                
                if(distanceToMarker < 0.3f) {
                    newDirection = Vector3.RotateTowards(pointer.transform.forward, targetDirection, Time.deltaTime, 0.0f);
                    pointer.transform.rotation = Quaternion.LookRotation(newDirection);
                    marker.SetActive(false);
                }
            }
        }
    }

    public void AddMarkers(GameObject marker)
    {
        placedMarkers.Add(marker);
    }

    public void PlaceRouteObjects()
    {
        Debug.Log("Placing route objects");
        Touch touch;
        if (Input.touchCount < 1 ||
    (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }
        // Ignore the touch if it's pointing on UI objects.
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        // Perform hit test and place a pawn object.
        PerformHitTest(touch.position);
    }

    private void PerformHitTest(Vector2 touchPos)
    {
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
        Controller.RaycastManager.Raycast(
            touchPos, hitResults, TrackableType.PlaneWithinPolygon);

        //var planeType = PlaneAlignment.HorizontalUp;
        if (hitResults.Count > 0)
        {
            ARPlane plane = Controller.PlaneManager.GetPlane(hitResults[0].trackableId);
            if (plane == null)
            {
                Debug.LogWarningFormat("Failed to find the ARPlane with TrackableId {0}",
                    hitResults[0].trackableId);
                return;
            }

            //planeType = plane.alignment;
            var hitPose = hitResults[0].pose;
            var routeAnchor = Instantiate(routeAnchorPrefab, hitPose.position, hitPose.rotation);
            routeSetting = false;
            Debug.Log("Placed local anchor : " + routeAnchor.transform.position);
        }

        
    }
}
