using PugMod;
using UnityEngine;
using Rewired;
using CoreLib;
using CoreLib.RewiredExtension;
using CoreLib.Util;

public class QuickStash : IMod
{
    private const string Version = "0.0.1";

    private Player player;

    private int lastUsed = 0;
    private int cooldown = 60;

    private float nearbyDistance = 4f;

    public void EarlyInit()
    {
        CoreLibMod.LoadModules(typeof(RewiredExtensionModule));
        RewiredExtensionModule.AddKeybind("FutrooModQuickStashToNearby", "Quick stash to nearby (Futroo Mod)", KeyboardKeyCode.A, ModifierKey.Control);
        RewiredExtensionModule.AddKeybind("FutrooModQuickGetFromNearby", "Quick get from nearby (Futroo Mod)", KeyboardKeyCode.D, ModifierKey.Control);

        RewiredExtensionModule.rewiredStart += OnRewiredStart;
    }
    
    public void Init()
    {
        UnityEngine.Debug.Log("Successfully initiated QuickStashAndGet V" + Version);
        
    }

    public void Shutdown()
    {

    }

    public void ModObjectLoaded(Object obj)
    {

    }

    public bool CanBeUnloaded()
    {
        return true;
    }

    public void Update()
    {
        if (GameManagers.GetMainManager() == null || !GameManagers.GetMainManager().currentSceneHandler.isInGame)
        {
            return;
        }
        if(player == null)
        {
            return;
        }
        if (player.GetButtonDown("FutrooModQuickStashToNearby"))
        {
            QuickStashToNearby();
        }
        if (player.GetButtonDown("FutrooModQuickGetFromNearby"))
        {
            QuickGetFromNearby();
        }
    }
    private void OnRewiredStart()
    {
        player = ReInput.players.GetPlayer(0);
    }

    private void QuickStashToNearby()
    {
        if (Time.frameCount - lastUsed < cooldown)
        {
            return;
        }
        lastUsed = Time.frameCount;

        PlayerController pl = GameManagers.GetMainManager().player;

        if (pl == null)
        {
            return;
        }
        if (pl.playerInventoryHandler == null)
        {
            return;
        }


        int chestCount = 0;

        Transform cameraManager = GameObject.Find("Camera Manager").transform;

        for (int i = 0; i < cameraManager.childCount; i++)
        {
            if (cameraManager.GetChild(i).name == "OrigoTransform")
            {

                for (int z = 0; z < cameraManager.GetChild(i).childCount; z++)
                {
                    Transform _chest = cameraManager.GetChild(i).GetChild(z);

                    if (_chest.name.Contains("Chest"))
                    {
                        if (_chest.gameObject.activeInHierarchy)
                        {
                            if (IsWithinDistance(pl.WorldPosition, _chest.localPosition, nearbyDistance))
                            {
                                Chest chestComponent = _chest.GetComponent<Chest>();
                                if (chestComponent == null)
                                {
                                    continue;
                                }
                                InventoryHandler chestInventoryHandler = chestComponent.inventoryHandler;
                                if (chestInventoryHandler == null)
                                {
                                    continue;
                                }
                                pl.playerInventoryHandler.QuickStack(chestInventoryHandler);
                                chestCount++;
                            }
                        }
                    }
                }
            }
        }

        if (chestCount > 0)
        {
            CreateCoolText("Stashing to " + chestCount + " chest(s).", new Color(0.769f, 0f, 1f));
        }
        else
        {
            CreateCoolText("Chests not found.", new Color(1f, 0f, 0f));
        }
    }
    private void QuickGetFromNearby()
    {
        if (Time.frameCount - lastUsed < cooldown)
        {
            return;
        }
        lastUsed = Time.frameCount;

        PlayerController pl = GameManagers.GetMainManager().player;
        if (pl == null)
        {
            return;
        }
        if (pl.playerInventoryHandler == null)
        {
            return;
        }


        int chestCount = 0;

        Transform cameraManager = GameObject.Find("Camera Manager").transform;

        for (int i = 0; i < cameraManager.childCount; i++)
        {
            if (cameraManager.GetChild(i).name == "OrigoTransform")
            {
                for (int z = 0; z < cameraManager.GetChild(i).childCount; z++)
                {
                    Transform _chest = cameraManager.GetChild(i).GetChild(z);

                    if (_chest.name.Contains("Chest"))
                    {
                        if (_chest.gameObject.activeInHierarchy)
                        {
                            if (IsWithinDistance(pl.WorldPosition, _chest.localPosition, nearbyDistance))
                            {
                                Chest chestComponent = _chest.GetComponent<Chest>();
                                if (chestComponent == null)
                                {
                                    continue;
                                }
                                InventoryHandler chestInventoryHandler = chestComponent.inventoryHandler;
                                if (chestInventoryHandler == null)
                                {
                                    continue;
                                }
                                chestInventoryHandler.QuickStack(pl.playerInventoryHandler);
                                chestCount ++;
                            }
                        }
                    }
                }
            }
        }

        if (chestCount > 0)
        {
            CreateCoolText("Getting from " + chestCount + " chest(s).", new Color(0.451f, 0.902f, 0f));
        }
        else
        {
            CreateCoolText("Chests not found.", new Color(1f, 0f, 0f));
        }
    }
    private bool IsWithinDistance(Vector3 position1, Vector3 position2, float distanceThreshold)
    {
        float distance = Vector3.Distance(position1, position2);
        return distance < distanceThreshold;
    }

    private static void CreateCoolText(string text, Color color)
    {
        Vector3 textPos = GameManagers.GetMainManager().player.RenderPosition + new Vector3(0, 1.5f, 0);
        GameManagers.GetManager<TextManager>().SpawnCoolText(text, textPos, color, TextManager.FontFace.thinSmall, 0.2f, 1, 2, 0.8f, 0.8f);
        
    }
}
