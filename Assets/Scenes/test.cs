using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public Dictionary<string, List<GameObject>> Routes;

    public GameObject pointer;

    private List<GameObject> placedMarkers;

    private List<GameObject> savedMarkers;

    int i;    

    string keyC;
    // Start is called before the first frame update
    void Start()
    {
        placedMarkers = new List<GameObject>();
        savedMarkers = new List<GameObject>();
        Routes = new Dictionary<string, List<GameObject>>();
        savedMarkers = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var routeAnchor = Instantiate(pointer, gameObject.transform);
            placedMarkers.Add(routeAnchor);
            Debug.Log("placedMarker: " + placedMarkers.Count);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            keyC = placedMarkers.Count.ToString();

            Routes.Add(keyC, placedMarkers);
            Debug.Log("Saving... Routes: " + keyC);
            foreach (GameObject m in placedMarkers)
            {
                m.SetActive(false);
            }
            //placedMarkers.Clear();
            placedMarkers = new List<GameObject>();
        }


        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Accessing...");

            savedMarkers = new List<GameObject>(Routes[3.ToString()]);

            Debug.Log("savedMarkers: " + savedMarkers.Count);
            foreach (GameObject temp in savedMarkers)
            {
                temp.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Accessing...");

            savedMarkers = new List<GameObject>(Routes[4.ToString()]);

            Debug.Log("savedMarkers: " + savedMarkers.Count);
            for (int currentMarker = 0; currentMarker < savedMarkers.Count; currentMarker++)
            {
                savedMarkers[currentMarker].SetActive(true);
                Debug.Log("savedMarkers " + currentMarker + " Transform: " + savedMarkers[currentMarker].transform.position);
            }
        }

    }
}
