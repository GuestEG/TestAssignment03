using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileSprite))]
[CanEditMultipleObjects]
public class TileSpriteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //old inspector
        //base.OnInspectorGUI();

        //what we are editing
        TileSprite tileScript = (TileSprite)target;

        if ((TilesManager.Instance.resourcePackNames.Count > 0) && (TilesManager.Instance.tileTypeNames.Count > 0))
        {
            if (tileScript.TypeId < TilesManager.Instance.tileTypeNames.Count )
            {
                //choose tile type
                tileScript.TypeId = EditorGUILayout.Popup("Tile type: ", tileScript.TypeId, TilesManager.Instance.tileTypeNames.ToArray());

            }
            else
            {
                //EditorGUILayout.HelpBox("Tile type of that tile is invalid!", MessageType.Warning);
            }

        }
        else
        {
            EditorGUILayout.HelpBox("No resource packs or tile types defined! Check Tiles Manager!", MessageType.Warning);
        }

        //EditorGUILayout.LabelField()
        if (tileScript.tileSpritePrefab)
        {
            GUILayout.Label(tileScript.tileSpritePrefab.texture);
        }
         
        
    }
}
