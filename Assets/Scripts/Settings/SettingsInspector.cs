using System.Collections;
using System.Collections.Generic;

/*******************************************************
 * Copyright (C) 2017 Doron Weiss  - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of unity license.
 * 
 * See https://abnormalcreativity.wixsite.com/home for more info
 *******************************************************/
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Dweiss
{
    [CustomEditor(typeof(Settings))]
    public class SettingsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var script = ((ASettings)target);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("_______Settings Buttons_______", EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            if (GUILayout.Button("Save to file"))
            {
                script.SaveToFile();
            }
            if (GUILayout.Button("Load from file"))
            {
                script.LoadToScript();
            }

            script.LoadSettingInEditorPlay = EditorGUILayout.Toggle("Load File On Play",
               script.LoadSettingInEditorPlay);

            script.AutoSave = EditorGUILayout.Toggle("Auto Save",
               script.AutoSave);
        }

    }
}