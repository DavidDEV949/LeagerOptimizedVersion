﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkItemController : MonoBehaviour {

    [SerializeField] public LayerMask tilesMasks;
    [SerializeField] public LayerMask entitiesMasks;
    [SerializeField] GameObject CraftMenu;
    [SerializeField] public Sprite[] spritesRenders;
    [SerializeField] public bool[] canInteract;
    GameObject workIcon;
    public BoxCollider2D collider2d;

    void Start ()
    {
        workIcon = transform.GetChild(0).gameObject;
	}

    void Update()
    {
        if (GameManager.gameManagerReference.InGame)
        {
            Vector3Int mousePos = Vector3Int.FloorToInt((Vector2)GameManager.gameManagerReference.mouseCurrentPosition + Vector2.one * 0.5f);
            int idx = mousePos.x * GameManager.gameManagerReference.WorldHeight + mousePos.y;
            GameObject tile = GameManager.gameManagerReference.GetTileObjectAt(idx);
            int tileSelected = GameManager.gameManagerReference.GetTileAt(idx);

            workIcon.GetComponent<SpriteRenderer>().sprite = GameManager.gameManagerReference.tiles[0];

            if (tile != null)
                if (!GameManager.gameManagerReference.doingAnAction)
                {
                    if (canInteract[tileSelected] == true)
                    {
                        workIcon.GetComponent<SpriteRenderer>().sprite = spritesRenders[tileSelected];
                        if (GInput.GetMouseButtonDown(1) || (GInput.leagerInput.platform == "Mobile" && !GameManager.gameManagerReference.cancelPlacing && GInput.GetMouseButtonDown(0)))
                        {
                            DisplayForTile(tileSelected, tile);
                        }
                    }
                }
                else
                {
                    if (GInput.GetMouseButton(0))
                    {
                        Vector4 tileSize = GameManager.gameManagerReference.tileSize[tileSelected];
                        collider2d.size = tileSize;
                        collider2d.offset = new Vector2(-tileSize.z, -tileSize.w);

                        if (collider2d.Cast(Vector2.zero, new RaycastHit2D[99]) == 0)
                            tile.transform.parent.GetComponent<ChunkController>().ClickedTile(tile);
                    }
                }
        }
    }

    public void DisplayForTile(int tile, GameObject tileObj)
    {
        if (tile == 15)
        {
            tileObj.transform.GetComponentInChildren<Box>().ToggleItems();
        }


        if (tile == 16)
        {
            if (GameObject.Find("CraftMenu") == null /*&& TechManager.techTree.fullyUnlockedItems.Count > 0*/)
            {
                GameObject a = Instantiate(CraftMenu, GameObject.Find("UI Menus").transform);
                a.name = "CraftMenu";
                a.GetComponent<CraftMenuController>().InvokeMenu(tileObj.transform);
            }
        }

        if (tile == 89)
        {
            if (GameManager.gameManagerReference.InGame)
            {
                MenuController.menuController.PlanetMenuDeploy(tileObj.transform.GetComponentInChildren<ResourceLauncher>());
            }
        }

        if (tile == 102)
        {
            tileObj.transform.GetComponentInChildren<Box>().ToggleItems();
        }
    }
}
