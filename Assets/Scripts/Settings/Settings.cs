/*******************************************************
 * Copyright (C) 2017 Doron Weiss  - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of unity license.
 * 
 * See https://abnormalcreativity.wixsite.com/home for more info
 *******************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dweiss {
	[System.Serializable]
	public class Settings : ASettings {

        [Header("--Main settings--")]
        public string SparqlEndpoint = "https://dbpedia.org/sparql";

        public string StartingURI = "Biobank";

        private new void Awake() {
			base.Awake ();
            SetupSingelton();
        }


        #region  Singelton
        public static Settings _instance;
        public static Settings Instance { get { return _instance; } }
        private void SetupSingelton()
        {
            if (_instance != null)
            {
                Debug.LogError("Error in settings. Multiple singeltons exists: " + _instance.name + " and now " + this.name);
            }
            else
            {
                _instance = this;
            }
        }
        #endregion



    }
}