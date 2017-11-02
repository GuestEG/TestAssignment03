using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TileSprite : MonoBehaviour {

    //sprite graphic to use on tile
    [SerializeField]
    public Sprite tileSpritePrefab = new Sprite();

    //sprite center to provide correct offset
    //defaults are: 128 from left and 64 from bottom
    [SerializeField]
    Vector2 spriteCenter = new Vector2(128.0f, 64.0f);

    //Tile type
    //For human-readable output use names from TilesManager
    [SerializeField]
    int typeId = 0;

    public int TypeId
    {
        get
        {
            return typeId;
        }
        set
        {
            typeId = value;
            RefreshSettings();
        }
    }

    //tile (not transform) position in world (units)
    //Vector2 tilePosition = new Vector2(0,0);

    //logical tile position on grid (row,column)
    [SerializeField]
    int row = 0;
    [SerializeField]
    int column = 0;
    [SerializeField]
    int baseSortingOrder = 0;
    
    int finalSortingOrder = 0;
    [SerializeField]
    int sortLayer = 0;

    //bool locked = false;

    private void Awake()
    {
        //subscribe to update event
        //TilesManager.OnUpdateSettingsEvent += UpdateSettingsEventHandler;
        //TilesManager.Instance.OnUpdateSettings += OnUpdateSettingsHandler;

        //set sprite to renderer
        GetComponentInChildren<SpriteRenderer>().sprite = tileSpritePrefab;
    }

    private void OnUpdateSettingsHandler(object sender, System.EventArgs e)
    {
        //throw new System.NotImplementedException();
        RefreshSettings();
    }

    private void UpdateSettingsEventHandler(int tileTypeId)
    {
        //throw new System.NotImplementedException();
        //Debug.Log("Got event with id = " + tileTypeId + ", my id = " + typeId);

        //if it is not our or generic type update - drop out
        if ((tileTypeId != typeId) || (tileTypeId != -1))
        {

        }
        //else get and apply settings
        else RefreshSettings();
    }

    public void RefreshSettings()
    {
        //get and apply settings
        TileSpriteSettings tileSpriteSettings = new TileSpriteSettings();// = TilesManager.Instance.TryGetTileSettings(typeId);
        if (!TilesManager.Instance.TryGetTileSettings(typeId, ref tileSpriteSettings))
        {
            Debug.LogError(name + ": No tile settings found! Tile type was removed? Dropping to type 0. Apply settings again.", this);
            typeId = 0;
            //tileSpriteSettings = TilesManager.Instance.GetTileSettings(typeId);
            

            //unsubscribe from event
            //TilesManager.OnUpdateSettingsEvent -= UpdateSettingsEventHandler;
            
            //think twice before destroying anything in edit mode!
            //DestroyImmediate(gameObject);
            return;
        }

        //if there are any sprites at all
        if (tileSpriteSettings.tileSpritePrefab && tileSpritePrefab)
        {
            //is there any difference between texture sizes?
            float hOffset = ((tileSpriteSettings.tileSpritePrefab.texture.width / tileSpriteSettings.tileSpritePrefab.pixelsPerUnit) - (tileSpritePrefab.texture.width / tileSpritePrefab.pixelsPerUnit)) / 2;
            float vOffset = ((tileSpriteSettings.tileSpritePrefab.texture.height / tileSpriteSettings.tileSpritePrefab.pixelsPerUnit) - (tileSpritePrefab.texture.height / tileSpritePrefab.pixelsPerUnit)) / 2;
            //move to compensate the difference
            transform.Translate(new Vector3(hOffset, vOffset));
        }

        tileSpritePrefab = tileSpriteSettings.tileSpritePrefab;
        spriteCenter = tileSpriteSettings.spriteCenter;
        baseSortingOrder = tileSpriteSettings.sortOrder;
        sortLayer = tileSpriteSettings.sortLayer;
        //finalSortingOrder = baseSortingOrder + TilesManager.Instance.vSortingStep * row + TilesManager.Instance.hSortingStep * column;

        //set sprite to renderer
        GetComponentInChildren<SpriteRenderer>().sprite = tileSpritePrefab;

        //GetComponentInChildren<SpriteRenderer>().sortingOrder = finalSortingOrder;

        //unlock
        //locked = false;
        //update position
        //Update();
    }


    // Update in editor envoked on every object if something is changed
    void Update()
    {
        //don't move?
        //if (locked) return;

        float spriteScale = 100;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        if (renderer && renderer.sprite) spriteScale = renderer.sprite.pixelsPerUnit;

        float hTileOffset = 0;
        float vTileOffset = 0;
        //there might be no sprite! we should not move then probably...
        if (tileSpritePrefab)
        {
            //transform center is in the center of the texture
            //but actual "tile" might be of different size, e.g. high wall texture is higher than tile
            //so we need to offset transform to difference between texture center and tile center
            //in case of high wall transform will be higher than center of the tile
            hTileOffset = (tileSpritePrefab.texture.width / 2 - spriteCenter.x) / spriteScale;
            vTileOffset = (tileSpritePrefab.texture.height / 2 - spriteCenter.y) / spriteScale;
            //Debug.Log("Object "+ name +" hTileOffset = " + hTileOffset + ", vTileOffset = " + vTileOffset);
        }
        else return;


        //we are looking into tile position, not transform position
        float gridVSpriteStep = TilesManager.Instance.gridVStep / spriteScale;
        int nearestGridVStep = Mathf.RoundToInt((transform.position.y - vTileOffset) / gridVSpriteStep); //row
        row = nearestGridVStep;
        float nearestGridVCoord = nearestGridVStep * gridVSpriteStep + vTileOffset;
        //Debug.Log("Current y = " + transform.position.y + ", gridVSpriteStep " + gridVSpriteStep + ", nearestGridVStep = " + nearestGridVStep + ", nearestGridVCoord = " + nearestGridVCoord);

        //check if our row is even or odd
        bool isOdd = (nearestGridVStep % 2) == 0;

        float gridHSpriteStep = TilesManager.Instance.gridHStep / spriteScale;
        //Debug.Log("is this row odd? " + isOdd + ", offset = " + (isOdd ? gridHSpriteStep / 2 : 0));
        int nearestGridHStep = Mathf.RoundToInt((transform.position.x - (isOdd ? gridHSpriteStep / 2 : 0) - hTileOffset) / gridHSpriteStep); //column        
        column = nearestGridHStep;
        float nearestGridHCoord = nearestGridHStep * gridHSpriteStep + (isOdd ? gridHSpriteStep / 2 : 0) + hTileOffset; //coord + odd row offset if needed
        //Debug.Log("Current x = " + transform.position.x + ", gridHSpriteStep "+ gridHSpriteStep + ", nearestGridHStep = " + nearestGridHStep + ", nearestGridHCoord = " + nearestGridHCoord);

        finalSortingOrder = baseSortingOrder + TilesManager.Instance.vSortingStep * (-row) + TilesManager.Instance.hSortingStep * column;
        //sorting order
        GetComponentInChildren<SpriteRenderer>().sortingOrder = finalSortingOrder;
        GetComponentInChildren<SpriteRenderer>().sortingLayerID = sortLayer;

        transform.position = new Vector3(nearestGridHCoord, nearestGridVCoord, transform.position.z);
    
    }

    private void OnDestroy()
    {
        //unsubscribe from event
        //TilesManager.OnUpdateSettingsEvent -= UpdateSettingsEventHandler;
        //TilesManager.Instance.OnUpdateSettings -= OnUpdateSettingsHandler;
    }
}
