using UnityEngine;
using System;
using System.Collections.Generic;

namespace MineMoguul_Mod
{
    internal class ModMenu : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 420, 650);
        private bool menuOpen = false;

        private bool fastMove;
        private bool bunnyHop;
        private bool airControl;
        private bool customGravity;
        private bool momentum;
        private bool flyMode;
        private bool noClip;
        private bool infiniteJump;
        private bool superJump;
        private bool speedLines;
        private bool timeScale;
        private bool esp;
        private bool autoCollect;
        private bool xrayVision;
        private bool autoMinerESP;
        private bool autoMinerTracers;
        private bool infiniteMoney;
        private bool unlockAllShopItems;
        private bool freeShopping;
        private bool sellMultiplierEnabled;
        private bool autoSellEnabled;

        private string[] tabs = { "Movement", "Visual", "ESP", "Economy", "Teleport" };
        private int selectedTab = 0;

        private string statusText = "Press INSERT to open menu";
        private float currentMoney = 0f;
        private Vector2 scrollPosition = Vector2.zero;
        private float tabButtonWidth = 80f;
        private float tabStartY = 25f;
        private float contentStartY = 60f;

        void Awake()
        {
            Debug.Log("MineMoguul ModMenu Awake called!");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                menuOpen = !menuOpen;
                Debug.Log($"Menu toggled: {menuOpen}");
                statusText = $"Menu {(menuOpen ? "opened" : "closed")}";
            }

            Mods.Init();
            SyncTogglesToMods();
            Mods.Tick();
            currentMoney = Mods.GetMoney();
        }

        void SyncTogglesToMods()
        {
            Mods.fastMove = fastMove;
            Mods.bunnyHop = bunnyHop;
            Mods.airControl = airControl;
            Mods.customGravity = customGravity;
            Mods.momentum = momentum;
            Mods.flyMode = flyMode;
            Mods.noClip = noClip;
            Mods.infiniteJump = infiniteJump;
            Mods.superJump = superJump;
            Mods.speedLines = speedLines;
            Mods.timeScale = timeScale;
            Mods.esp = esp;
            Mods.autoCollect = autoCollect;
            Mods.xrayVision = xrayVision;
            Mods.autoMinerESP = autoMinerESP;
            Mods.autoMinerTracers = autoMinerTracers;
            Mods.infiniteMoney = infiniteMoney;
            Mods.unlockAllShopItems = unlockAllShopItems;
            Mods.freeShopping = freeShopping;
            Mods.sellMultiplierEnabled = sellMultiplierEnabled;
            Mods.autoSellEnabled = autoSellEnabled;
        }

        void OnGUI()
        {
            GUI.depth = -9999;
            GUI.Label(new Rect(10, 10, 400, 20), $"MineMoguul Mod - Menu: {(menuOpen ? "OPEN" : "CLOSED")}");
            GUI.Label(new Rect(10, 30, 400, 20), $"Money: ${currentMoney:#,##0.00} | {statusText}");
            if (!menuOpen) return;
            windowRect = GUI.Window(1337, windowRect, DrawWindow, "MineMoguul Mod Menu v1.0");
        }

        void DrawWindow(int id)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                float xPos = 10 + (i * (tabButtonWidth + 5));
                if (GUI.Button(new Rect(xPos, tabStartY, tabButtonWidth, 25), tabs[i]))
                {
                    selectedTab = i;
                    scrollPosition = Vector2.zero;
                }

                if (selectedTab == i)
                {
                    GUI.Box(new Rect(xPos, tabStartY, tabButtonWidth, 25), "");
                }
            }

            GUI.BeginGroup(new Rect(10, contentStartY, 400, 520));

            switch (selectedTab)
            {
                case 0: DrawMovementContent(); break;
                case 1: DrawVisualContent(); break;
                case 2: DrawESPContent(); break;
                case 3: DrawEconomyContent(); break;
                case 4: DrawTeleportContent(); break;
            }

            GUI.EndGroup();

            if (GUI.Button(new Rect(10, 590, 195, 30), "Reset All"))
            {
                Mods.ResetOverrides();
                ResetAllToggles();
                statusText = "All settings reset";
            }

            if (GUI.Button(new Rect(215, 590, 195, 30), "Close Menu"))
            {
                menuOpen = false;
                statusText = "Menu closed";
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        void DrawMovementContent()
        {
            float yPos = 10f;

            GUI.Label(new Rect(10, yPos, 200, 20), "Basic Movement:");
            yPos += 25;
            fastMove = GUI.Toggle(new Rect(10, yPos, 150, 20), fastMove, "Fast Move");
            bunnyHop = GUI.Toggle(new Rect(170, yPos, 150, 20), bunnyHop, "Bunny Hop");
            yPos += 25;
            airControl = GUI.Toggle(new Rect(10, yPos, 150, 20), airControl, "Air Control");
            momentum = GUI.Toggle(new Rect(170, yPos, 150, 20), momentum, "Momentum");
            yPos += 35;

            GUI.Label(new Rect(10, yPos, 200, 20), "Special Movement:");
            yPos += 25;
            flyMode = GUI.Toggle(new Rect(10, yPos, 150, 20), flyMode, "Fly Mode");
            noClip = GUI.Toggle(new Rect(170, yPos, 150, 20), noClip, "No Clip");
            yPos += 25;
            infiniteJump = GUI.Toggle(new Rect(10, yPos, 150, 20), infiniteJump, "Infinite Jump");
            superJump = GUI.Toggle(new Rect(170, yPos, 150, 20), superJump, "Super Jump");
            yPos += 35;

            GUI.Label(new Rect(10, yPos, 200, 20), "Speed Settings:");
            yPos += 25;
            GUI.Label(new Rect(10, yPos, 150, 20), $"Walk: {Mods.walkSpeed:F1}");
            Mods.walkSpeed = GUI.HorizontalSlider(new Rect(170, yPos, 200, 20), Mods.walkSpeed, 2f, 20f);
            yPos += 25;
            GUI.Label(new Rect(10, yPos, 150, 20), $"Sprint: {Mods.sprintSpeed:F1}");
            Mods.sprintSpeed = GUI.HorizontalSlider(new Rect(170, yPos, 200, 20), Mods.sprintSpeed, 6f, 30f);
            yPos += 25;

            if (superJump)
            {
                GUI.Label(new Rect(10, yPos, 150, 20), $"Jump: {Mods.superJumpHeight:F1}");
                Mods.superJumpHeight = GUI.HorizontalSlider(new Rect(170, yPos, 200, 20), Mods.superJumpHeight, 5f, 30f);
                yPos += 25;
            }

            if (flyMode)
            {
                GUI.Label(new Rect(10, yPos, 150, 20), $"Fly: {Mods.flySpeed:F1}");
                Mods.flySpeed = GUI.HorizontalSlider(new Rect(170, yPos, 200, 20), Mods.flySpeed, 5f, 30f);
                yPos += 25;
            }

            GUI.Label(new Rect(10, yPos, 200, 20), "Physics:");
            yPos += 25;
            customGravity = GUI.Toggle(new Rect(10, yPos, 150, 20), customGravity, "Custom Gravity");
            yPos += 25;

            if (customGravity)
            {
                GUI.Label(new Rect(10, yPos, 150, 20), $"Gravity: {Mods.gravity:F1}");
                Mods.gravity = GUI.HorizontalSlider(new Rect(170, yPos, 200, 20), Mods.gravity, -50f, -1f);
            }
        }

        void DrawVisualContent()
        {
            float yPos = 10f;

            GUI.Label(new Rect(10, yPos, 200, 20), "Visual Effects:");
            yPos += 25;
            speedLines = GUI.Toggle(new Rect(10, yPos, 150, 20), speedLines, "Speed Lines");
            timeScale = GUI.Toggle(new Rect(170, yPos, 150, 20), timeScale, "Time Scale");
            yPos += 25;
            xrayVision = GUI.Toggle(new Rect(10, yPos, 150, 20), xrayVision, "X-Ray Vision");
            autoCollect = GUI.Toggle(new Rect(170, yPos, 150, 20), autoCollect, "Auto Collect");
            yPos += 25;
            esp = GUI.Toggle(new Rect(10, yPos, 150, 20), esp, "General ESP");
            yPos += 35;

            if (timeScale)
            {
                GUI.Label(new Rect(10, yPos, 200, 20), "Time Scale:");
                yPos += 25;
                GUI.Label(new Rect(10, yPos, 150, 20), $"{Mods.timeScaleMultiplier:F2}x");
                Mods.timeScaleMultiplier = GUI.HorizontalSlider(new Rect(170, yPos, 200, 20), Mods.timeScaleMultiplier, 0.1f, 5f);
                yPos += 35;
            }

            GUI.Label(new Rect(10, yPos, 200, 20), "ESP Range:");
            yPos += 25;
            GUI.Label(new Rect(10, yPos, 150, 20), $"{Mods.espRange:F0}m");
            Mods.espRange = GUI.HorizontalSlider(new Rect(170, yPos, 200, 20), Mods.espRange, 10f, 500f);
        }

        void DrawESPContent()
        {
            float yPos = 10f;

            GUI.Label(new Rect(10, yPos, 200, 20), "AutoMiner ESP:");
            yPos += 25;
            autoMinerESP = GUI.Toggle(new Rect(10, yPos, 180, 20), autoMinerESP, "ESP Boxes");
            autoMinerTracers = GUI.Toggle(new Rect(200, yPos, 180, 20), autoMinerTracers, "Tracer Lines");
            yPos += 35;

            if (GUI.Button(new Rect(10, yPos, 185, 25), "Refresh Miners"))
            {
                Mods.RefreshAutoMiners();
                statusText = $"Found {Mods.autoMiners?.Count ?? 0} miners";
            }

            if (GUI.Button(new Rect(205, yPos, 185, 25), "Print Info"))
            {
                Mods.PrintAutoMinerInfo();
                statusText = "Printed miner info";
            }
            yPos += 35;

            GUI.Label(new Rect(10, yPos, 200, 20), "Miner Information:");
            yPos += 25;

            if (Mods.autoMiners != null && Mods.autoMiners.Count > 0)
            {
                int activeCount = 0;
                int inactiveCount = 0;

                foreach (var miner in Mods.autoMiners)
                {
                    if (miner != null)
                    {
                        if (miner.Enabled) activeCount++;
                        else inactiveCount++;
                    }
                }

                GUI.Label(new Rect(10, yPos, 200, 20), $"Total: {Mods.autoMiners.Count}");
                GUI.Label(new Rect(210, yPos, 200, 20), $"Active: {activeCount}");
                yPos += 20;
                GUI.Label(new Rect(10, yPos, 200, 20), $"Inactive: {inactiveCount}");
            }
            else
            {
                GUI.Label(new Rect(10, yPos, 380, 40), "No miners found. Click Refresh Miners.");
            }
        }

        void DrawEconomyContent()
        {
            float yPos = 10f;

            GUI.Label(new Rect(10, yPos, 200, 20), "Economy Mods:");
            yPos += 25;
            infiniteMoney = GUI.Toggle(new Rect(10, yPos, 180, 20), infiniteMoney, "Infinite Money");
            freeShopping = GUI.Toggle(new Rect(200, yPos, 180, 20), freeShopping, "Free Shopping");
            yPos += 25;
            sellMultiplierEnabled = GUI.Toggle(new Rect(10, yPos, 180, 20), sellMultiplierEnabled, "Sell Multiplier");
            unlockAllShopItems = GUI.Toggle(new Rect(200, yPos, 180, 20), unlockAllShopItems, "Unlock All");
            yPos += 25;
            autoSellEnabled = GUI.Toggle(new Rect(10, yPos, 180, 20), autoSellEnabled, "Auto Sell");
            yPos += 35;

            GUI.Label(new Rect(10, yPos, 200, 20), "Money Controls:");
            yPos += 25;
            GUI.Label(new Rect(10, yPos, 150, 20), $"Amount: ${Mods.moneyAmount:#,##0}");
            Mods.moneyAmount = GUI.HorizontalSlider(new Rect(170, yPos, 200, 20), Mods.moneyAmount, 1000f, 10000000f);
            yPos += 35;

            if (GUI.Button(new Rect(10, yPos, 185, 30), "Add Money"))
            {
                Mods.AddMoneyGUI();
                statusText = $"Added ${Mods.moneyAmount:#,##0}";
            }

            if (GUI.Button(new Rect(205, yPos, 185, 30), "Set Money"))
            {
                Mods.SetMoneyGUI();
                statusText = $"Set to ${Mods.moneyAmount:#,##0}";
            }
            yPos += 40;

            if (GUI.Button(new Rect(10, yPos, 380, 25), "Reset Money to 0"))
            {
                Mods.ResetMoney();
                statusText = "Money reset to 0";
            }
            yPos += 35;

            if (sellMultiplierEnabled)
            {
                GUI.Label(new Rect(10, yPos, 200, 20), "Sell Multiplier:");
                yPos += 25;
                GUI.Label(new Rect(10, yPos, 150, 20), $"{Mods.sellMultiplier:F1}x");
                Mods.sellMultiplier = GUI.HorizontalSlider(new Rect(170, yPos, 200, 20), Mods.sellMultiplier, 1f, 100f);
                yPos += 35;
            }

            GUI.Label(new Rect(10, yPos, 200, 20), "Current Status:");
            yPos += 25;
            GUI.Label(new Rect(10, yPos, 380, 20), $"Money: ${currentMoney:#,##0.00}");
            yPos += 20;
            GUI.Label(new Rect(10, yPos, 380, 20), $"Infinite Money: {(infiniteMoney ? "ON" : "OFF")}");
            yPos += 20;
            GUI.Label(new Rect(10, yPos, 380, 20), $"Free Shopping: {(freeShopping ? "ON" : "OFF")}");
        }

        void DrawTeleportContent()
        {
            float yPos = 10f;

            GUI.Label(new Rect(10, yPos, 380, 30), "Teleport Options:", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
            yPos += 40;

            if (GUI.Button(new Rect(10, yPos, 380, 40), "TELEPORT FORWARD (6m)"))
            {
                Mods.TeleportForward();
                statusText = "Teleported forward";
            }
            yPos += 50;

            if (GUI.Button(new Rect(10, yPos, 380, 40), "TELEPORT UP (50m)"))
            {
                Mods.TeleportUp(50f);
                statusText = "Teleported up";
            }
            yPos += 50;

            if (GUI.Button(new Rect(10, yPos, 380, 40), "TELEPORT TO CURSOR"))
            {
                Mods.TeleportToCursor();
                statusText = "Teleported to cursor";
            }
            yPos += 60;

            GUI.Label(new Rect(10, yPos, 380, 20), "Teleport Distances:");
            yPos += 25;
            GUI.Label(new Rect(10, yPos, 380, 20), "Forward: 6 meters");
            yPos += 20;
            GUI.Label(new Rect(10, yPos, 380, 20), "Up: 50 meters");
            yPos += 20;
            GUI.Label(new Rect(10, yPos, 380, 20), "Cursor: Raycast up to 1000m");
        }

        void ResetAllToggles()
        {
            fastMove = false; bunnyHop = false; airControl = false; customGravity = false; momentum = false;
            flyMode = false; noClip = false; infiniteJump = false; superJump = false;
            speedLines = false; timeScale = false; esp = false; autoCollect = false; xrayVision = false;
            autoMinerESP = false; autoMinerTracers = false;
            infiniteMoney = false; unlockAllShopItems = false; freeShopping = false;
            sellMultiplierEnabled = false; autoSellEnabled = false;
        }
    }
}