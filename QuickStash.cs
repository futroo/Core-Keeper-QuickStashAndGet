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
using static CreditsData;
using System;

public class QuickStash : IMod
{
    public const string VERSION = "1.0.3";
    public const string NAME = "QuickStashAndGet";
    public const string AUTHOR = "Futroo";

    private LoadedMod modInfo;

    private Player player;

    private int lastUsed = 0;
    private int cooldown = 60;

    private float nearbyDistance = 5f;

    public void EarlyInit()
    {
        UnityEngine.Debug.Log($"[{NAME}]: Version: {VERSION}");
        modInfo = API.ModLoader.LoadedMods.FirstOrDefault(obj => obj.Handlers.Contains(this));
        if (modInfo == null)
        {
            UnityEngine.Debug.Log($"[{NAME}]: Failed to load {NAME}!");
            return;
        }
        UnityEngine.Debug.Log($"[{NAME}]: Mod loaded successfully!");

        CoreLibMod.LoadModules(typeof(RewiredExtensionModule));
        RewiredExtensionModule.AddKeybind("FutrooModQuickStashToNearby", "Quick stash to nearby (Futroo's Mod)", KeyboardKeyCode.A, ModifierKey.Control);
        RewiredExtensionModule.AddKeybind("FutrooModQuickGetFromNearby", "Quick get from nearby (Futroo's Mod)", KeyboardKeyCode.D, ModifierKey.Control);

        RewiredExtensionModule.rewiredStart += OnRewiredStart;
    }

    public void Init()
    {
        UnityEngine.Debug.Log("Successfully initiated QuickStashAndGet V" + VERSION);
    }

    public void Shutdown()
    {

    }

    public void ModObjectLoaded(UnityEngine.Object obj)
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
		
       	Transform poolChest = GameObject.Find("Pool Chest").transform;
	Transform poolBossChest = GameObject.Find("Pool BossChest").transform;
	Transform poolNonPaintableChest = GameObject.Find("Pool NonPaintableChest").transform;
	
	List<Transform> allChests = poolChest.GetAllChildren().Where(obj =>obj.gameObject.activeSelf).ToList();
	allChests.AddRange(poolBossChest.GetAllChildren().Where(obj => obj.gameObject.activeSelf).ToList());
	allChests.AddRange(poolNonPaintableChest.GetAllChildren().Where(obj => obj.gameObject.activeSelf).ToList());
	
	foreach (Transform t in allChests)
	{
		Chest _chestComponent = t.GetComponent<Chest>();
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
		
        if (chestCount > 0)
        {
            Vector3 _textPos = GameManagers.GetMainManager().player.RenderPosition + new Vector3(0, 1.5f, 0);
            CreateCoolText("Stashing to " + chestCount + " chest(s).", new Color(1f, 0f, 0.5f), _textPos);
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

        Transform poolChest = GameObject.Find("Pool Chest").transform;
	Transform poolBossChest = GameObject.Find("Pool BossChest").transform;
	Transform poolNonPaintableChest = GameObject.Find("Pool NonPaintableChest").transform;

	List<Transform> allChests = poolChest.GetAllChildren().Where(obj => obj.gameObject.activeSelf).ToList();
	allChests.AddRange(poolBossChest.GetAllChildren().Where(obj => obj.gameObject.activeSelf).ToList());
	allChests.AddRange(poolNonPaintableChest.GetAllChildren().Where(obj => obj.gameObject.activeSelf).ToList());

	foreach (Transform t in allChests)
	{
		Chest _chestComponent = t.GetComponent<Chest>();
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
