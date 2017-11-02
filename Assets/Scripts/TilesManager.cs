using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSpriteSettings
{
    public Sprite tileSpritePrefab;
    public Vector2 spriteCenter = new Vector2(128.0f, 64.0f);
    public int sortLayer = 0;
    public int sortOrder = 0;
}

[System.Serializable]
public struct resourcePack
{
    public int resourcePackId;
    public List<TileSpriteSettings> tileTypes;    
}


[ExecuteInEditMode]
public class TilesManager : MonoBehaviour {

    //these things should be "dynamic enums"

    //all possible ResourcePack names
    //or "visual settings"
    //or "environment styles"
    //or whatever
    //like "stone", "wooden" etc.
    //[SerializeField]
    public List<string> resourcePackNames = new List<string>( new string[]{"base", "wood", "stone"} );
    //public Dictionary<int, string> resourcePackNames = new Dictionary<int, string>();

    //all possible Tile type names
    //like "floor", "NWall" etc.
    //[SerializeField]
    public List<string> tileTypeNames = new List<string>(new string[] { "floor", "wallWFull", "wallNW" });
    //public Dictionary<int, string> tileTypeNames = new Dictionary<int, string>();

    //actual "album" of tiles = 2-dimensional array (since every visual style have all tile types)
    //consists of "TileSprite" script params
    //under resource pack and tile type

    //TileSpriteSettings[,] tileAlbum = new TileSpriteSettings[1,1];
    /* Dictionary can't be serialized. Nested containers also can not be serialized. Sad but true.
     * Dictionary<int, Dictionary<int, TileSpriteSettings>> tileAlbum = new Dictionary<int, Dictionary<int, TileSpriteSettings>>();
     */
    //[SerializeField]
    public List<resourcePack> tileAlbum = new List<resourcePack>();
    

    //resource pack, used in scene
    [SerializeField]
    int currentUsedResourcePackId = 0;

    ///[SerializeField]
    public int currentEditingResourcePackId = 0;
    //[SerializeField]
    public int currentEditingTileTypeId = 0;

    //inspector for this manager should include ability to:
    //1. add and remove resourcePack names
    //2. add and remove tileType names
    //3. for each tileType under resourcePack - to add sprite and edit params of TileSprite (basically TileSprite inspector)

    //[SerializeField]
    TileSprite currentTile;

    [Header("Grid settings")]
    //grid settings
    public float pixelsPerUnit = 100.0f;
    public float gridHStep = 256.0f;
    public float gridVStep = 64.0f;

    public int hSortingStep = 5;
    public int vSortingStep = 10;

    [SerializeField]
    bool showGrid = false;

    
    //an event for all TileSprites to subscribe for
    //public delegate void UpdateSettingsDelegate(int tileTypeId);

    //public event UpdateSettingsDelegate OnUpdateSettingsEvent;

    //public event System.EventHandler OnUpdateSettings;


    //Singletone implementation for easy access
    private static TilesManager instance;
    //property for access and lazy instantiation
    public static TilesManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<TilesManager>();//new TilesManager();
            }
            return instance;
        }
    }

    public bool TryGetTileSettings(int tileTypeId, ref TileSpriteSettings tileSpriteSettings)
    {
        //TileSpriteSettings result = null;
        /*
        Dictionary<int, TileSpriteSettings> pack;
        tileAlbum.TryGetValue(currentResourcePackId, out pack);
        pack.TryGetValue(tileTypeId, out result);
        */

        //is there such id at all?
        if ((resourcePackNames.Count > 0) && (tileTypeNames.Count > 0))
        {
            if (tileTypeId < tileTypeNames.Count)
            {
                tileSpriteSettings = tileAlbum[currentUsedResourcePackId].tileTypes[tileTypeId];
                return true;
            }
            //out of bounds
            else return false;
        }
        else
        {
            Debug.LogError("No resource packs or tile types found! Add some in Tiles Manager!");
            return false;
        }        
    }

    private void OnDrawGizmos()
    {
        //grid is 100x100 units from origin. 100 is a majuuk number.
        //TODO: should be virtually infinite in camera frustum instead!

        if (!showGrid) return;

        //scaling based purely on grid settings, not actual sprites!
        float gridVSpriteStep = gridVStep / pixelsPerUnit;
        float gridHSpriteStep = gridHStep / pixelsPerUnit;

        //grid helpers - rays from axis x to nearest grid nodes at rows 1 and -1
        //one-unit directions
        Vector3 rayDirection = new Vector3(gridHSpriteStep / 2, gridVSpriteStep, 0);
        Vector3 rayCrossDirection = new Vector3(-gridHSpriteStep / 2, gridVSpriteStep, 0);

        Gizmos.color = Color.cyan;

        for (int i = -100; i < 100; i++) 
        {
            //starts
            Vector3 rayOrigin = new Vector3(i * gridHSpriteStep, 0, 0);
            //ends            
            Gizmos.DrawRay(rayOrigin, rayDirection * 100);
            Gizmos.DrawRay(rayOrigin, -rayDirection * 100);
            Gizmos.DrawRay(rayOrigin, rayCrossDirection * 100);
            Gizmos.DrawRay(rayOrigin, -rayCrossDirection * 100);
        }
        
    }

    public void ResourcePackSelect(int packId)
    {
        currentEditingResourcePackId = packId;
    }

    public void TileTypeSelect(int typeId)
    {
        currentEditingTileTypeId = typeId;
    }

    public void ApplyResourcePackId()
    {
        currentUsedResourcePackId = currentEditingResourcePackId;
        //UpdateSettingsEvent(-1);
        //if (OnUpdateSettingsEvent != null) OnUpdateSettingsEvent(-1); else Debug.Log("No subscribers");
        //if (OnUpdateSettings != null) OnUpdateSettings(this, System.EventArgs.Empty); else Debug.Log("No subscribers");
        
        //events not working - try dumb system
        foreach (TileSprite tile in FindObjectsOfType<TileSprite>())
        {
            tile.RefreshSettings();
        }
    }

    public void ApplyTileSettings()
    {
        //if (OnUpdateSettingsEvent!= null) OnUpdateSettingsEvent(currentEditingTileTypeId); else Debug.Log("No subscribers");
        //if (OnUpdateSettings != null) OnUpdateSettings(this, System.EventArgs.Empty); else Debug.Log("No subscribers");
        //events not working - try dumb system
        foreach (TileSprite tile in FindObjectsOfType<TileSprite>())
        {
            tile.RefreshSettings();
        }
    }

    public void AddNewResourcePack(string packName)
    {
        resourcePackNames.Add(packName);
        resourcePack tiles = new resourcePack();
        tiles.resourcePackId = resourcePackNames.BinarySearch(packName);
        tiles.tileTypes = new List<TileSpriteSettings>();
        for (int type = 0; type < tileTypeNames.Count; type++)
        {
            //for each tileType - add them to resourcepack
            tiles.tileTypes.Add(new TileSpriteSettings());
        }
        //add pack to album
        tileAlbum.Add(tiles);

        //refresh GUI
    }

    //public int AddNewTileType(string typeName)
    public void AddNewTileType(string typeName)
    {
        tileTypeNames.Add(typeName);
        //add tile in every pack
        foreach (resourcePack pack in tileAlbum)
        {
            pack.tileTypes.Add(new TileSpriteSettings());
        }
        //return tileTypeNames.BinarySearch(typeName);

        //refresh GUI
    }

    public void RemoveResourcePack(int packIndex)
    {
        resourcePackNames.RemoveAt(packIndex);
        tileAlbum.RemoveAt(packIndex);
        currentEditingResourcePackId = 0;
        ApplyResourcePackId();

        //refresh GUI
    }

    public void RemoveTileType(int typeIndex)
    {
        tileTypeNames.RemoveAt(typeIndex);
        //remove from each pack
        foreach (resourcePack pack in tileAlbum)
        {
            pack.tileTypes.RemoveAt(typeIndex);
        }
        currentEditingTileTypeId = 0;
        //probably delete all tiles of now-null setting
        ApplyResourcePackId();

        //refresh GUI
    }

    public void RenameResourcePack(int packIndex, string newName)
    {
        resourcePackNames[packIndex] = newName;
        //refresh GUI
    }

    public void RenameTileType(int typeIndex, string newName)
    {
        tileTypeNames[typeIndex] = newName;
        //refresh GUI
    }

    private void Awake()
    {
        //Debug.Log(name + " Awoken", this);
        //fill the album with empty tiles
        /*
        if (tileAlbum.Count == 0)
        {
            for (int pack = 0; pack < resourcePackNames.Count; pack++)
            {
                //Dictionary<int, TileSpriteSettings> tiles = new Dictionary<int, TileSpriteSettings>();
                resourcePack tiles = new resourcePack();
                tiles.resourcePackId = pack;
                tiles.tileTypes = new List<TileSpriteSettings>();
                for (int type = 0; type < tileTypeNames.Count; type++)
                {
                    //for each tileType - add them to resourcepack

                    tiles.tileTypes.Add(new TileSpriteSettings());

                }
                //add each pack to album
                tileAlbum.Add(tiles);
            }
        }
        */
    }

}
