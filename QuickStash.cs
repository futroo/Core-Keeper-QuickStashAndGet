using PugMod;
using UnityEngine;
using Rewired;
using CoreLib;
using CoreLib.RewiredExtension;
using CoreLib.Util;
using CoreLib.Util.Extensions;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Unity.NetCode;

public class QuickStash : IMod
{
    private const string Version = "1.0.0";

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
        if (GameManagers.GetMainManager() == null || !GameManagers.GetMainManager().currentSceneHandler.isInGame || player == null)
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

        if (pl == null || pl.playerInventoryHandler == null)
        {
            return;
        }

        int chestCount = 0;
		
        Transform cameraManager = GameObject.Find("Camera Manager").transform;
        List<Transform> origoTransforms = cameraManager.GetAllChildren().Where(obj => obj.name == "OrigoTransform").ToList();

        foreach(Transform t in origoTransforms)
        {
            foreach (var _chest in t.GetAllChildren().Where(obj => obj.name.Contains("Chest") && obj.gameObject.activeInHierarchy))
            {
                Chest _chestComponent = _chest.GetComponent<Chest>();
                if (_chestComponent != null && IsInRange(pl.WorldPosition, _chestComponent.WorldPosition, nearbyDistance))
                {
                    InventoryHandler chestInventoryHandler = _chestComponent.inventoryHandler;
                    if (chestInventoryHandler != null)
                    {
                        pl.playerInventoryHandler.QuickStack(chestInventoryHandler);
                        chestCount++;
                    }
                }
            }
        }
        if (chestCount > 0)
        {
            Vector3 _textPos = GameManagers.GetMainManager().player.RenderPosition + new Vector3(0, 1.5f, 0);
            CreateCoolText("Stashing to " + chestCount + " chest(s).", new Color(0.769f, 0f, 1f), _textPos);
        }
        else
        {
            Vector3 _textPos = GameManagers.GetMainManager().player.RenderPosition + new Vector3(0, 1.5f, 0);
            CreateCoolText("Chests not found.", new Color(1f, 0f, 0f), _textPos);
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

        if (pl == null || pl.playerInventoryHandler == null)
        {
            return;
        }

        int chestCount = 0;

        Transform cameraManager = GameObject.Find("Camera Manager").transform;
        List<Transform> origoTransforms = cameraManager.GetAllChildren().Where(obj => obj.name == "OrigoTransform").ToList();

        foreach (Transform t in origoTransforms)
        {
            foreach (var _chest in t.GetAllChildren().Where(obj => obj.name.Contains("Chest") && obj.gameObject.activeInHierarchy))
            {
                Chest _chestComponent = _chest.GetComponent<Chest>();
                if (_chestComponent != null && IsInRange(pl.WorldPosition, _chestComponent.WorldPosition, nearbyDistance))
                {
                    InventoryHandler chestInventoryHandler = _chestComponent.inventoryHandler;
                    if (chestInventoryHandler != null)
                    {
                        chestInventoryHandler.QuickStack(pl.playerInventoryHandler);
                        chestCount++;
                    }
                }
            }
        }

        if (chestCount > 0)
        {
            Vector3 _textPos = GameManagers.GetMainManager().player.RenderPosition + new Vector3(0, 1.5f, 0);
            CreateCoolText("Getting from " + chestCount + " chest(s).", new Color(0.451f, 0.902f, 0f), _textPos);
        }
        else
        {
            Vector3 _textPos = GameManagers.GetMainManager().player.RenderPosition + new Vector3(0, 1.5f, 0);
            CreateCoolText("Chests not found.", new Color(1f, 0f, 0f), _textPos);
        }
    }
    private bool IsInRange(Vector3 position1, Vector3 position2, float distanceThreshold)
    {
        float distance = Vector3.Distance(position1, position2);
        return distance < distanceThreshold;
    }

    private static void CreateCoolText(string text, Color color, Vector3 pos)
    {
        GameManagers.GetManager<TextManager>().SpawnCoolText(text, pos, color, TextManager.FontFace.thinSmall, 0.2f, 1, 2, 0.8f, 0.8f);
    }
}
