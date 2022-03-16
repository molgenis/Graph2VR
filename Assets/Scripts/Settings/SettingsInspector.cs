
/*******************************************************
 * Copyright (C) 2017 Doron Weiss  - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of unity license.
 * 
 * See https://abnormalcreativity.wixsite.com/home for more info
 *******************************************************/
using UnityEditor;
using UnityEngine;

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

      // Disable loading from json on play for now. This can make you loose your changes when forgetting to save.
      /*script.LoadSettingInEditorPlay = EditorGUILayout.Toggle("Load File On Play",
         script.LoadSettingInEditorPlay);*/

    }
  }
}