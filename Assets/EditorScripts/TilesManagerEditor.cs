using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(TilesManager))]
public class TilesManagerEditor : Editor
{
    
    //grid settings
    SerializedProperty pixelsPerUnit;
    SerializedProperty gridHStep;
    SerializedProperty gridVStep;

    SerializedProperty hSortingStep;
    SerializedProperty vSortingStep;

    SerializedProperty showGrid;

    //album and tiles settings
    /*
    SerializedProperty resourcePackNames;
    SerializedProperty tileTypeNames;
    SerializedProperty tileAlbum;
    //SerializedProperty currentUsedResourcePackId;
    //SerializedProperty currentEditingResourcePackId;
    //SerializedProperty currentEditingTileTypeId;

    //tile params
    /*
    public class TileSpriteSettings
    {
        public Sprite tileSpritePrefab;
        public Vector2 spriteCenter = new Vector2(128.0f, 64.0f);
        public int sortLayer = 0;
        public int sortOrder = 0;
    }
    */

    //SerializedProperty tileSpritePrefab;
    //SerializedProperty spriteCenter;
    //SerializedProperty sortLayer;
    //SerializedProperty sortOrder;

    string newPackName = "";
    string newTileName = "";

    private void OnEnable()
    {
        //assotiate local variables with controllable script
        pixelsPerUnit = serializedObject.FindProperty("pixelsPerUnit");
        gridHStep = serializedObject.FindProperty("gridHStep");
        gridVStep = serializedObject.FindProperty("gridVStep");

        hSortingStep = serializedObject.FindProperty("hSortingStep");
        vSortingStep = serializedObject.FindProperty("vSortingStep");

        showGrid = serializedObject.FindProperty("showGrid");

        /*
        resourcePackNames = serializedObject.FindProperty("resourcePackNames");
        tileTypeNames = serializedObject.FindProperty("tileTypeNames");
        tileAlbum = serializedObject.FindProperty("tileAlbum");
        /*
        currentUsedResourcePackId = serializedObject.FindProperty("currentUsedResourcePackId");
        currentEditingResourcePackId = serializedObject.FindProperty("currentEditingResourcePackId");
        currentEditingTileTypeId = serializedObject.FindProperty("currentEditingTileTypeId");
        */       

    }

    public override void OnInspectorGUI()
    {
        //OL' INSPECTOR
        //base.OnInspectorGUI();

        //what we are editing
        TilesManager managerScript = (TilesManager)target;


        //EditorGUILayout.Separator();
        //EditorGUILayout.LabelField("NEW INSPECTOR!");
        
        //get params from object we are editing
        serializedObject.Update();

        //actual GUI

        //drop-downs
        //int index = 0;
        // Debug.Log("resourcePackNames type = " + resourcePackNames.type + ", items type = "+ resourcePackNames.arrayElementType);
        //List<string> resPack = resourcePackNames.objectReferenceValue as List<string>;

        /*
         * OLD SCHOOL EDITOR INTERFACE MODE
         * NEVER DO LIKE THAT. LIKE NEVER. EVER.
         * Serialization should be your friend. Because undo etc.
         */

        //choose resource pack
        managerScript.currentEditingResourcePackId = EditorGUILayout.Popup("Resource Pack: ", managerScript.currentEditingResourcePackId, managerScript.resourcePackNames.ToArray());
        
        //buttons to add, rename, delete and apply.
        newPackName = EditorGUILayout.TextField("New pack name:", newPackName);
        if (GUILayout.Button("Add new resource pack"))
        {
            //Debug.Log("adding new?");
            //newName = EditorGUILayout.TextField("Name:", newName);
            /*
            EnterNameDialog window = ScriptableObject.CreateInstance<EnterNameDialog>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.ShowPopup();
            */
            if (newPackName.Trim().Length > 0)
            {
                //check for duplicates
                if (!managerScript.resourcePackNames.Contains(newPackName))
                {
                    managerScript.AddNewResourcePack(newPackName);
                }
                else
                {
                    Debug.LogWarning("Can't add '" + newPackName + "': pack name already exists!");
                }

                //reload serialized object to prevent shenanigans
                serializedObject.Update();
            }
        }

        if (managerScript.resourcePackNames.Count > 0)
        {

            if (GUILayout.Button("Rename selected pack"))
            {
                if (newPackName.Trim().Length > 0)
                {
                    //check for duplicates
                    if (!managerScript.resourcePackNames.Contains(newPackName))
                    {
                        managerScript.RenameResourcePack(managerScript.currentEditingResourcePackId, newPackName);
                    }
                    else
                    {
                        Debug.LogWarning("Can't rename to '" + newPackName + "': pack name already exists!");
                    }

                    //reload serialized object to prevent shenanigans
                    serializedObject.Update();
                }
            }

            if (GUILayout.Button("Delete selected pack"))
            {
                //show warning!
                if (EditorUtility.DisplayDialog("Delete resource pack",
                    "Are you sure you want to delete pack '" + managerScript.resourcePackNames[managerScript.currentEditingResourcePackId] + "' ?",
                    "Delete '" + managerScript.resourcePackNames[managerScript.currentEditingResourcePackId] + "'!", "Cancel"))
                {
                    managerScript.RemoveResourcePack(managerScript.currentEditingResourcePackId);

                    //reload serialized object to prevent shenanigans
                    serializedObject.Update();
                }

            }


            if (GUILayout.Button("Apply pack to scene"))
            {
                managerScript.ApplyResourcePackId();

                //reload serialized object to prevent shenanigans
                serializedObject.Update();
            }
        }

        EditorGUILayout.Separator();

        if ((managerScript.resourcePackNames.Count > 0))
        {
            //choose tile type
            managerScript.currentEditingTileTypeId = EditorGUILayout.Popup("Tile type: ", managerScript.currentEditingTileTypeId, managerScript.tileTypeNames.ToArray());

            //buttons to add, rename, delete and apply.
            newTileName = EditorGUILayout.TextField("New tile type name:", newTileName);
            if (GUILayout.Button("Add new tile type"))
            {
                if (newTileName.Trim().Length > 0)
                {
                    //check for duplicates
                    if (!managerScript.tileTypeNames.Contains(newTileName))
                    {
                        //int newIndex = managerScript.AddNewTileType(newTileName);
                        managerScript.AddNewTileType(newTileName);
                        //Debug.Log("New index = " + newIndex);
                    }
                    else
                    {
                        Debug.LogWarning("Can't add '" + newTileName + "': type already exists!");
                    }

                    //managerScript.currentEditingTileTypeId = newIndex;

                    //reload serialized object to prevent shenanigans
                    serializedObject.Update();
                }
            }

            if (managerScript.tileTypeNames.Count > 0)
            {
                if (GUILayout.Button("Rename selected type"))
                {
                    if (newTileName.Trim().Length > 0)
                    {
                        //check for duplicates
                        if (!managerScript.tileTypeNames.Contains(newTileName))
                        {
                            managerScript.RenameTileType(managerScript.currentEditingTileTypeId, newTileName);
                        }
                        else
                        {
                            Debug.LogWarning("Can't rename to '" + newTileName + "': type already exists!");
                        }

                        //reload serialized object to prevent shenanigans
                        serializedObject.Update();
                    }
                }

                if (GUILayout.Button("Delete selected type"))
                {
                    //show warning!
                    if (EditorUtility.DisplayDialog("Delete tile type",
                        "Are you sure you want to delete tile type '" + managerScript.tileTypeNames[managerScript.currentEditingTileTypeId] + "' ? " //+
                        //"\nAll tiles with this type WILL BE DELETED FROM THE SCENE!!!",

                        ,"Delete '" + managerScript.tileTypeNames[managerScript.currentEditingTileTypeId] + "'!", "Cancel"))
                    {
                        managerScript.RemoveTileType(managerScript.currentEditingTileTypeId);

                        //reload serialized object to prevent shenanigans
                        serializedObject.Update();
                    }

                }
            }
        }

        //tile params editor

        //pre-check for possible emptyness
        if ((managerScript.resourcePackNames.Count > 0) && (managerScript.tileTypeNames.Count > 0))
        {
            //selected tile
            TileSpriteSettings tileSettings = managerScript.tileAlbum[managerScript.currentEditingResourcePackId].tileTypes[managerScript.currentEditingTileTypeId];


            //sprite prefab
            //EditorGUI.ObjectField(new Rect(3, 3, 200, 20), tileSpritePrefab, typeof(Sprite));
            tileSettings.tileSpritePrefab = EditorGUILayout.ObjectField("Tile Sprite:", tileSettings.tileSpritePrefab, typeof(Sprite), false) as Sprite;
            //tile center
            tileSettings.spriteCenter = EditorGUILayout.Vector2Field("Tile Center:", tileSettings.spriteCenter);
            //sorting layer
            //this is complicated
            //EditorGUILayout.LayerField("Sorting Layer:", tileSettings.sortLayer);
            List<string> layerNames = new List<string>();
            foreach (SortingLayer layer in SortingLayer.layers)
            {
                layerNames.Add(layer.name);
            }

            tileSettings.sortLayer = EditorGUILayout.Popup("Sorting Layer:", tileSettings.sortLayer, layerNames.ToArray());

            //Sorting order
            tileSettings.sortOrder = EditorGUILayout.DelayedIntField("Sorting Order:", tileSettings.sortOrder);

            //save back
            managerScript.tileAlbum[managerScript.currentEditingResourcePackId].tileTypes[managerScript.currentEditingTileTypeId] = tileSettings;            

            //apply edited?
            if (GUILayout.Button("Apply tile settings to scene"))
            {
                managerScript.ApplyTileSettings();

                //reload serialized object to prevent shenanigans
                serializedObject.Update();
            }

        }
        
        //grid params - directly serialized
        EditorGUILayout.Separator();
        //there will be [HEADER] from script
        EditorGUILayout.PropertyField(pixelsPerUnit);
        EditorGUILayout.PropertyField(gridHStep);
        EditorGUILayout.PropertyField(gridVStep);
        EditorGUILayout.PropertyField(hSortingStep);
        EditorGUILayout.PropertyField(vSortingStep);
        EditorGUILayout.PropertyField(showGrid);

        //send params to script
        serializedObject.ApplyModifiedProperties();
    }
    
}
