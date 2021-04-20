//-----------------------------------------------------------------------
// <copyright file="ResolveMenuManager.cs" company="Google LLC">
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
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A manager component that helps to populate and handle the options of resolving anchors.
    /// </summary>
    public class ResolveManager : MonoBehaviour
    {
        /// <summary>
        /// The main controller.
        /// </summary>
        public UIController Controller;

        /// <summary>
        /// The mapping controller.
        /// </summary>
        public LocalizeMarkers MappingController;

        /// <summary>
        /// A multiselection dropdown component that contains all available resolving options.
        /// </summary>
        public MultiselectionDropdown Multiselection;

        /// <summary>
        /// A dropdown component that contains all available resolving options.
        /// </summary>
        public Dropdown Dropdown;

        /// <summary>
        /// Dropdown label text.
        /// </summary>
        public Text DropdownText;

        /// <summary>
        /// An input field for manually typing Cloud Anchor Id(s).
        /// </summary>
        public InputField InputField;

        /// <summary>
        /// The warning text that appears when invalid characters are filled in.
        /// </summary>
        public GameObject InvalidInputWarning;

        /// <summary>
        /// The resolve button which leads to AR view screen.
        /// </summary>
        public Button ResolveButton;

        /// <summary>
        /// Cached Cloud Anchor history data used to fetch the Cloud Anchor Id using
        /// the index given by multi-selection dropdown.
        /// </summary>
        private List<string> _history;
        List<string> temp;

        /// <summary>
        /// Cached active color for interactable buttons.
        /// </summary>
        private Color _activeColor;

        /// <summary>
        /// Switch to AR view, and disable all other screens.
        /// </summary>
        public void OnResolveMenuButton()
        {
            if(_history.Count != 0)
            {
                ResolveButton.enabled = true;
                string selectedRoute = _history[Dropdown.value];
                Debug.Log("Selected Route : " + selectedRoute);
                //MappingController.FindPath(selectedRoute);
                MappingController.routeName = selectedRoute;
            }
            /*if(temp.Count != 0)
            {
                string selectedRoute = temp[Dropdown.value];
                Debug.Log("Selected Route : " + selectedRoute);
                MappingController.FindPath(selectedRoute);
            }*/
            else
            {
                ResolveButton.enabled = false;
                Debug.Log("Options not populated");
            }
        }



        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            _activeColor = ResolveButton.GetComponent<Image>().color;
        }

        /// <summary>
        /// The Unity OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            if (MappingController.Routes.Keys != null)
            {
                _history = new List<string>(MappingController.Routes.Keys);
                //temp = new List<string> { "option1", "option2" };
                Dropdown.options.Clear();
                //Dropdown.AddOptions(temp);
                Dropdown.AddOptions(_history);
            }
        }

        /// <summary>
        /// The Unity OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            Dropdown.options.Clear();
            Dropdown.value = 0;
            //_history.Clear();
        }

    }
}
