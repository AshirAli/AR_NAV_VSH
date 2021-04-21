//-----------------------------------------------------------------------
// <copyright file="ARViewManager.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// A manager component that helps with hosting and resolving Cloud Anchors.
    /// </summary>
    public class ViewManager : MonoBehaviour
    {
        /// <summary>
        /// The active ARSessionOrigin.
        /// </summary>
        public ARSessionOrigin SessionOrigin;

        /// <summary>
        /// The main controller.
        /// </summary>
        public UIController Controller;

        /// <summary>
        /// The mapping controller.
        /// </summary>
        public LocalizeMarkers MappingController;


        /// <summary>
        /// The 3D object that represents a Cloud Anchor.
        /// </summary>
        public GameObject CloudAnchorPrefab;

        int anchorNumber = 0;

        /// <summary>
        /// The UI element that displays the instructions to guide hosting experience.
        /// </summary>
        public GameObject InstructionBar;

        /// <summary>
        /// The UI panel that allows the user to name the Cloud Anchor.
        /// </summary>
        public GameObject NamePanel;

        /// <summary>
        /// The UI panel that prompts the user about reaching destination.
        /// </summary>
        public GameObject DestinationPanel;

        /// <summary>
        /// The UI element that displays warning message for invalid input name.
        /// </summary>
        public GameObject InputFieldWarning;

        /// <summary>
        /// The input field for naming Cloud Anchor.
        /// </summary>
        public InputField NameField;

        /// <summary>
        /// The instruction text in the top instruction bar.
        /// </summary>
        public Text InstructionText;

        /// <summary>
        /// The debug text in bottom snack bar.
        /// </summary>
        public Text DebugText;

        /// <summary>
        /// The button to save the typed name.
        /// </summary>
        public Button NameButton;

        /// <summary>
        /// The button to clear the placed markers.
        /// </summary>
        public Button ClearButton;

        /// <summary>
        /// The button to save current cloud anchor id into clipboard.
        /// </summary>
        public Button SaveButton;

        /// <summary>
        /// The time between enters AR View and ARCore session starts to host or resolve.
        /// </summary>
        private const float _startPrepareTime = 3.0f;

        /// <summary>
        /// The timer to indicate whether the AR View has passed the start prepare time.
        /// </summary>
        private float _timeSinceStart;

        /// <summary>
        /// True if the app is in the process of returning to home page due to an invalid state,
        /// otherwise false.
        /// </summary>
        private bool _isReturning;

        /// <summary>
        /// The history data that represents the current hosted Cloud Anchor.
        /// </summary>
        private string _hostedCloudAnchor;


        /// <summary>
        /// Get the camera pose for the current frame.
        /// </summary>
        /// <returns>The camera pose of the current frame.</returns>
        public Pose GetCameraPose()
        {
            return new Pose(Controller.MainCamera.transform.position,
                Controller.MainCamera.transform.rotation);
        }

        /// <summary>
        /// Callback handling the validaton of the input field.
        /// </summary>
        /// <param name="inputString">The current value of the input field.</param>
        public void OnInputFieldValueChanged(string inputString)
        {
            // Cloud Anchor name should only contains: letters, numbers, hyphen(-), underscore(_).
            var regex = new Regex("^[a-zA-Z0-9-_]*$");
            InputFieldWarning.SetActive(!regex.IsMatch(inputString));
            NameButton.enabled = (!InputFieldWarning.activeSelf && inputString.Length > 0);
        }

        /// <summary>
        /// Callback handling "Ok" button click event for input field.
        /// </summary>
        public void OnNameButtonClicked()
        {
            _hostedCloudAnchor = NameField.text;

            var regex = new Regex("^[a-zA-Z0-9-_]*$");

            if (!regex.IsMatch(_hostedCloudAnchor))
            {
                InputFieldWarning.SetActive(true);
                return;
            }

            if (!MappingController.SaveRoute(_hostedCloudAnchor))
            {
                InputFieldWarning.SetActive(true);
                Text warning = InputFieldWarning.GetComponent<Text>();
                warning.text = "Route name already present.";
                return;
            }
            DebugText.text = string.Format("Saved Route:\n{0}.", _hostedCloudAnchor);
            NamePanel.SetActive(false);
            InputFieldWarning.SetActive(false);
            Controller.SwitchToHomePage();
        }

        /// <summary>
        /// Callback handling "save" button click event.
        /// </summary>
        public void OnSaveButtonClicked()
        {
            anchorNumber = 0;
            NamePanel.SetActive(true);
            SaveButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Callback handling "clear" button click event.
        /// </summary>
        public void OnClearButtonClicked()
        {
            anchorNumber = 0;
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            
        }

        /// <summary>
        /// The Unity OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            _timeSinceStart = 0.0f;
            _isReturning = false;

            InstructionBar.SetActive(true);
            NamePanel.SetActive(false);
            InputFieldWarning.SetActive(false);
            SaveButton.gameObject.SetActive(false);
            DestinationPanel.SetActive(false);
            UpdatePlaneVisibility(true);

            switch (Controller.Mode)
            {
                case UIController.ApplicationMode.Ready:
                    ReturnToHomePage("Invalid application mode, returning to home page...");
                    break;
                case UIController.ApplicationMode.Hosting:
                case UIController.ApplicationMode.Resolving:
                    InstructionText.text = "Detecting flat surface...";
                    DebugText.text = "ARCore is preparing for " + Controller.Mode;
                    break;
            }
        }

        /// <summary>
        /// The Unity OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            UpdatePlaneVisibility(false);
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            // Give ARCore some time to prepare for hosting or resolving.
            if (_timeSinceStart < _startPrepareTime)
            {
                _timeSinceStart += Time.deltaTime;
                if (_timeSinceStart >= _startPrepareTime)
                {
                    UpdateInitialInstruction();
                }

                return;
            }

            //ARCoreLifecycleUpdate();
            if (_isReturning)
            {
                return;
            }

            if (Controller.Mode == UIController.ApplicationMode.Resolving)
            {
                //MappingController.FindPath();
            }
            else if (Controller.Mode == UIController.ApplicationMode.Hosting)
            {
                // Perform hit test and place an anchor on the hit test result.

                // If the player has not touched the screen then the update is complete.
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
            else if (Controller.Mode == UIController.ApplicationMode.Ending)
            {
                DestinationPanel.SetActive(true);
            }

        }

        private void PerformHitTest(Vector2 touchPos)
        {
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            Controller.RaycastManager.Raycast(
                touchPos, hitResults, TrackableType.PlaneWithinPolygon);

            // If a hit is obtained for raycast, then instantiate the corresponding object.
 
            if (hitResults.Count > 0)
            {
                ARPlane plane = Controller.PlaneManager.GetPlane(hitResults[0].trackableId);
                if (plane == null)
                {
                    Debug.LogWarningFormat("Failed to find the ARPlane with TrackableId {0}",
                        hitResults[0].trackableId);
                    return;
                }

                var hitPose = hitResults[0].pose;

                HostingCloudAnchors(hitPose);
                Debug.Log("AnchorManager AddAnchor");
            }
        }


        private void HostingCloudAnchors(Pose hit)
        {
            //var routeAnchor = Instantiate(CloudAnchorPrefab, _anchor.transform);
            var routeAnchor = Instantiate(CloudAnchorPrefab, hit.position, Quaternion.Euler(-90,0,0));
            routeAnchor.gameObject.name = anchorNumber.ToString();

            MappingController.AddMarkers(routeAnchor);

            anchorNumber++;

            DebugText.text = "Instantiated marker " + routeAnchor.name;
            Debug.Log("Instantiated anchor " + routeAnchor.name);

            // Hide plane generator so users can focus on the object they placed.
            //UpdatePlaneVisibility(false);

            SaveButton.gameObject.SetActive(true);

        }

        private void UpdateInitialInstruction()
        {
            switch (Controller.Mode)
            {
                case UIController.ApplicationMode.Hosting:
                    // Initial instruction for hosting flow:
                    InstructionText.text = "Tap to place marker.";
                    DebugText.text = "Tap a horizontal plane...";
                    ClearButton.gameObject.SetActive(true);
                    return;
                case UIController.ApplicationMode.Resolving:
                    // Initial instruction for resolving flow:
                    InstructionText.text =
                        "Follow the direction of AR Pointer.";
                    ClearButton.gameObject.SetActive(false);
                    MappingController.FindPath();
                    /*DebugText.text = string.Format("Attempting to resolve {0} anchors...",
                        Controller.ResolvingSet.Count);*/
                    return;
                default:
                    return;
            }
        }

        private void UpdatePlaneVisibility(bool visible)
        {
            foreach (var plane in Controller.PlaneManager.trackables)
            {
                plane.gameObject.SetActive(visible);
            }
        }

        private void ARCoreLifecycleUpdate()
        {
            // Only allow the screen to sleep when not tracking.
            var sleepTimeout = SleepTimeout.NeverSleep;
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                sleepTimeout = SleepTimeout.SystemSetting;
            }

            Screen.sleepTimeout = sleepTimeout;

            if (_isReturning)
            {
                return;
            }

            // Return to home page if ARSession is in error status.
            if (ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                ReturnToHomePage("ARCore encountered an error state " + ARSession.state +
                    ". Please start the app again.");
            }
        }

        private void ReturnToHomePage(string reason)
        {
            Debug.Log("Returning home for reason: " + reason);
            if (_isReturning)
            {
                return;
            }

            _isReturning = true;
            DebugText.text = reason;
            Invoke("DoReturnToHomePage", 3.0f);
        }

        private void DoReturnToHomePage()
        {
            Controller.SwitchToHomePage();
        }

        private void DoHideInstructionBar()
        {
            InstructionBar.SetActive(false);
        }
    }
}
