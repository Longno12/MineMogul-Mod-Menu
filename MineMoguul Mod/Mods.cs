using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MineMoguul_Mod
{
    internal static class Mods
    {
        public static PlayerController Player;
        private static EconomyManager economyManager;

        public static bool fastMove;
        public static bool customGravity;
        public static bool bunnyHop;
        public static bool airControl;
        public static bool momentum;
        public static bool flyMode;
        public static bool noClip;
        public static bool infiniteJump;
        public static bool superJump;
        public static bool speedLines;
        public static bool timeScale;
        public static bool esp;
        public static bool autoCollect;
        public static bool xrayVision;
        public static bool autoMinerESP;
        public static bool autoMinerTracers;

        public static bool infiniteMoney;
        public static bool unlockAllShopItems;
        public static bool freeShopping;
        public static bool sellMultiplierEnabled;
        public static float sellMultiplier = 2f;
        public static float moneyAmount = 1000000f;
        public static bool autoSellEnabled;

        public static float walkSpeed = 8f;
        public static float sprintSpeed = 12f;
        public static float gravity = -20f;
        public static float jumpMultiplier = 1.4f;
        public static float airControlStrength = 2.5f;
        public static float momentumStrength = 1.2f;
        public static float flySpeed = 10f;
        public static float superJumpHeight = 10f;
        public static float timeScaleMultiplier = 1f;
        public static float espRange = 100f;

        private static Vector3 storedVelocity;
        private static CharacterController originalController;
        private static bool wasFlying = false;
        private static float originalSlopeLimit;
        private static float originalStepOffset;
        private static LayerMask xrayOriginalMask;
        private static LayerMask interactLayerMaskBackup;
        private static GameObject speedLinesEffect;

        public static List<AutoMiner> autoMiners = new List<AutoMiner>();
        private static Dictionary<AutoMiner, GameObject> espObjects = new Dictionary<AutoMiner, GameObject>();
        private static Dictionary<AutoMiner, LineRenderer> tracerLines = new Dictionary<AutoMiner, LineRenderer>();
        private static Dictionary<AutoMiner, GameObject> tracerGlows = new Dictionary<AutoMiner, GameObject>();
        private static Material espMaterial;
        private static Material tracerMaterial;
        private static Color activeColor = new Color(0f, 1f, 0f, 0.5f);
        private static Color inactiveColor = new Color(1f, 0f, 0f, 0.5f);
        private static Color tracerColor = new Color(1f, 0.5f, 0f, 0.8f);

        public static void Init()
        {
            if (Player == null)
            {
                Player = UnityEngine.Object.FindObjectOfType<PlayerController>();
                if (Player != null)
                {
                    originalController = Player.CharacterController;
                    originalSlopeLimit = Player.StandingSlopeLimit;
                    originalStepOffset = Player.CharacterController.stepOffset;
                    interactLayerMaskBackup = Player.InteractLayerMask;
                }
            }

            if (economyManager == null)
            {
                economyManager = UnityEngine.Object.FindObjectOfType<EconomyManager>();
            }
            CreateSpeedLinesEffect();
            CreateESPMaterials();
            RefreshAutoMiners();
        }

        public static void Tick()
        {
            if (Player == null) return;

            ApplySpeed();
            ApplyGravity();
            HandleBunnyHop();
            HandleAirControl();
            HandleMomentum();
            HandleFlyMode();
            HandleNoClip();
            HandleInfiniteJump();
            HandleSuperJump();
            HandleSpeedLines();
            HandleTimeScale();
            HandleAutoCollect();
            HandleXrayVision();
            HandleAutoMinerESP();
            HandleMoneyMods();
            HandleShopMods();
        }

        private static void CreateESPMaterials()
        {
            try
            {
                espMaterial = new Material(Shader.Find("Unlit/Color"))
                {
                    color = new Color(1f, 0f, 0f, 0.5f)
                };

                tracerMaterial = new Material(Shader.Find("Unlit/Color"))
                {
                    color = tracerColor
                };

                espMaterial.SetInt("_ZWrite", 0);
                espMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
                tracerMaterial.SetInt("_ZWrite", 0);
                tracerMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
            }
            catch
            {
                Debug.LogError("Failed to create ESP materials. Shader might not be available.");
            }
        }

        public static void RefreshAutoMiners()
        {
            autoMiners.Clear();
            var foundMiners = UnityEngine.Object.FindObjectsOfType<AutoMiner>();
            if (foundMiners != null)
            {
                autoMiners.AddRange(foundMiners);
            }
            Debug.Log($"Found {autoMiners.Count} AutoMiners");
        }

        private static void HandleAutoMinerESP()
        {
            if (!autoMinerESP && !autoMinerTracers)
            {
                CleanupESP();
                return;
            }

            if (Time.frameCount % 120 == 0)
            {
                RefreshAutoMiners();
            }

            foreach (var miner in autoMiners)
            {
                if (miner == null) continue;

                float distance = Vector3.Distance(Player.transform.position, miner.transform.position);

                if (distance > espRange)
                {
                    RemoveESPObject(miner);
                    RemoveTracerLine(miner);
                    continue;
                }

                if (autoMinerESP)
                {
                    UpdateESPObject(miner);
                }
                else
                {
                    RemoveESPObject(miner);
                }

                if (autoMinerTracers)
                {
                    UpdateTracerLine(miner);
                }
                else
                {
                    RemoveTracerLine(miner);
                }
            }

            CleanupNullReferences();
        }

        private static void UpdateESPObject(AutoMiner miner)
        {
            if (!espObjects.ContainsKey(miner))
            {
                CreateESPObject(miner);
            }

            if (espObjects.TryGetValue(miner, out GameObject espObj) && espObj != null)
            {
                espObj.transform.position = miner.transform.position;
                espObj.transform.rotation = miner.transform.rotation;
                Renderer minerRenderer = miner.GetComponent<Renderer>();
                if (minerRenderer != null)
                {
                    Bounds bounds = minerRenderer.bounds;
                    espObj.transform.localScale = bounds.size * 1.2f;
                }
                else
                {
                    Collider collider = miner.GetComponent<Collider>();
                    if (collider != null)
                    {
                        espObj.transform.localScale = collider.bounds.size * 1.2f;
                    }
                    else
                    {
                        espObj.transform.localScale = Vector3.one * 2.5f;
                    }
                }
                Renderer espRenderer = espObj.GetComponent<Renderer>();
                if (espRenderer != null && espMaterial != null)
                {
                    Color color = miner.Enabled ? activeColor : inactiveColor;
                    float pulse = Mathf.Sin(Time.time * 3f) * 0.3f + 0.7f;
                    color.a = (miner.Enabled ? activeColor.a : inactiveColor.a) * pulse;
                    espRenderer.material.color = color;
                    espRenderer.material.renderQueue = 3000;
                }
            }
        }

        private static void CreateESPObject(AutoMiner miner)
        {
            GameObject espObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            espObj.name = $"ESP_{miner.name}";
            Collider collider = espObj.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.Destroy(collider);
            Renderer renderer = espObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(espMaterial);
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                renderer.material.SetInt("_ZWrite", 0);
                renderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            }
            espObjects[miner] = espObj;
        }

        private static void UpdateTracerLine(AutoMiner miner)
        {
            if (!tracerLines.ContainsKey(miner))
            {
                CreateTracerLine(miner);
            }

            if (tracerLines.TryGetValue(miner, out LineRenderer line) && line != null)
            {
                Vector3 startPos = Player.transform.position + Vector3.up * 1.5f;
                Vector3 endPos = miner.transform.position + Vector3.up * 1f;
                line.SetPosition(0, startPos);
                line.SetPosition(1, endPos);
                float distance = Vector3.Distance(startPos, endPos);
                float width = Mathf.Lerp(0.08f, 0.02f, distance / espRange);
                line.startWidth = width;
                line.endWidth = width * 0.3f;
                float pulse = Mathf.Sin(Time.time * 2f) * 0.4f + 0.6f;
                Color lineColor = miner.Enabled ? new Color(0f, 1f, 0f, 0.7f * pulse) : new Color(1f, 0.5f, 0f, 0.7f * pulse);
                line.material.color = lineColor;
                line.enabled = true;
                if (tracerGlows.TryGetValue(miner, out GameObject glowObj) && glowObj != null)
                {
                    LineRenderer glowLine = glowObj.GetComponent<LineRenderer>();
                    if (glowLine != null)
                    {
                        glowLine.SetPosition(0, startPos);
                        glowLine.SetPosition(1, endPos);
                        glowLine.startWidth = width * 2f;
                        glowLine.endWidth = width * 0.6f;
                        glowLine.material.color = new Color(lineColor.r, lineColor.g, lineColor.b, lineColor.a * 0.3f);
                        glowLine.enabled = true;
                    }
                }
            }
        }

        private static void CreateTracerLine(AutoMiner miner)
        {
            try
            {
                GameObject lineObj = new GameObject($"Tracer_{miner.name}");
                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                line.material = new Material(tracerMaterial);
                line.startWidth = 0.05f;
                line.endWidth = 0.02f;
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                line.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                line.material.SetInt("_ZWrite", 0);
                line.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                GameObject glowObj = new GameObject($"TracerGlow_{miner.name}");
                LineRenderer glowLine = glowObj.AddComponent<LineRenderer>();
                glowLine.material = new Material(Shader.Find("Unlit/Color"));
                glowLine.material.color = new Color(1f, 1f, 1f, 0.2f);
                glowLine.startWidth = 0.1f;
                glowLine.endWidth = 0.05f;
                glowLine.positionCount = 2;
                glowLine.useWorldSpace = true;
                glowLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                glowLine.receiveShadows = false;
                glowObj.transform.SetParent(lineObj.transform);
                tracerLines[miner] = line;
                tracerGlows[miner] = glowObj;
                Debug.Log($"Created tracer for miner: {miner.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create tracer line: {e.Message}");
            }
        }

        private static void RemoveESPObject(AutoMiner miner)
        {
            if (espObjects.TryGetValue(miner, out GameObject espObj) && espObj != null)
            {
                UnityEngine.Object.Destroy(espObj);
            }
            espObjects.Remove(miner);
        }

        private static void RemoveTracerLine(AutoMiner miner)
        {
            if (tracerLines.TryGetValue(miner, out LineRenderer line) && line != null)
            {
                UnityEngine.Object.Destroy(line.gameObject);
            }
            tracerLines.Remove(miner);

            if (tracerGlows.TryGetValue(miner, out GameObject glowObj) && glowObj != null)
            {
                UnityEngine.Object.Destroy(glowObj);
            }
            tracerGlows.Remove(miner);
        }

        private static void CleanupESP()
        {
            foreach (var kvp in espObjects)
            {
                if (kvp.Value != null)
                {
                    UnityEngine.Object.Destroy(kvp.Value);
                }
            }
            espObjects.Clear();
            foreach (var kvp in tracerLines)
            {
                if (kvp.Value != null)
                {
                    UnityEngine.Object.Destroy(kvp.Value.gameObject);
                }
            }
            tracerLines.Clear();

            foreach (var kvp in tracerGlows)
            {
                if (kvp.Value != null)
                {
                    UnityEngine.Object.Destroy(kvp.Value);
                }
            }
            tracerGlows.Clear();
        }

        private static void CleanupNullReferences()
        {
            var espToRemove = new List<AutoMiner>();
            foreach (var kvp in espObjects)
            {
                if (kvp.Key == null || kvp.Value == null)
                {
                    espToRemove.Add(kvp.Key);
                }
            }
            foreach (var miner in espToRemove)
            {
                espObjects.Remove(miner);
            }

            var tracerToRemove = new List<AutoMiner>();
            foreach (var kvp in tracerLines)
            {
                if (kvp.Key == null || kvp.Value == null)
                {
                    tracerToRemove.Add(kvp.Key);
                }
            }
            foreach (var miner in tracerToRemove)
            {
                tracerLines.Remove(miner);
                tracerGlows.Remove(miner);
            }
        }

        private static void ApplySpeed()
        {
            if (!fastMove) return;
            Player.WalkSpeed = walkSpeed;
            Player.SprintSpeed = sprintSpeed;
        }

        private static void ApplyGravity()
        {
            if (!customGravity) return;
            Player.Gravity = gravity;
        }

        private static void HandleBunnyHop()
        {
            if (!bunnyHop) return;
            var input = Player.GetInputActions();
            if (input.Player.Jump.triggered && Player.SelectedWalkSpeed > 0f)
            {
                Player.JumpHeight *= jumpMultiplier;
            }
        }

        private static void HandleAirControl()
        {
            if (!airControl || flyMode || noClip) return;
            Vector2 move = Player.MoveInput;
            if (move.sqrMagnitude < 0.01f) return;
            Vector3 wishDir = Player.transform.right * move.x + Player.transform.forward * move.y;
            storedVelocity += wishDir * airControlStrength * Time.deltaTime;
        }

        private static void HandleMomentum()
        {
            if (!momentum || flyMode || noClip) return;
            Vector2 move = Player.MoveInput;
            if (move.sqrMagnitude < 0.01f) return;
            Vector3 wishDir = Player.transform.forward * move.y + Player.transform.right * move.x;
            storedVelocity = Vector3.Lerp(storedVelocity, wishDir * momentumStrength, Time.deltaTime * 2f);
        }

        private static void HandleFlyMode()
        {
            if (!flyMode)
            {
                if (wasFlying)
                {
                    wasFlying = false;
                    Player.CharacterController.enabled = true;
                    Player.Gravity = customGravity ? gravity : -9.81f;
                }
                return;
            }
            wasFlying = true;
            Player.CharacterController.enabled = false;
            Player.Gravity = 0f;
            var input = Player.GetInputActions();
            Vector2 move = input.Player.Move.ReadValue<Vector2>();
            Vector3 moveDir = Player.transform.right * move.x + Player.transform.forward * move.y;
            if (input.Player.Jump.IsPressed()) moveDir.y = 1f;
            if (input.Player.Duck.IsPressed()) moveDir.y = -1f;
            Player.transform.position += moveDir.normalized * flySpeed * Time.deltaTime;
        }

        private static void HandleNoClip()
        {
            if (!noClip)
            {
                if (Player.CharacterController != null)
                {
                    Player.CharacterController.enabled = true;
                    Player.StandingSlopeLimit = originalSlopeLimit;
                    Player.CharacterController.stepOffset = originalStepOffset;
                }
                return;
            }

            Player.CharacterController.enabled = false;
            Player.StandingSlopeLimit = 90f;
            Player.CharacterController.stepOffset = 100f;
            var input = Player.GetInputActions();
            Vector2 move = input.Player.Move.ReadValue<Vector2>();
            Vector3 moveDir = Player.transform.right * move.x * walkSpeed * 0.5f + Player.transform.forward * move.y * walkSpeed * 0.5f;
            Player.transform.position += moveDir * Time.deltaTime;
        }

        private static float GetJumpVelocity()
        {
            return Mathf.Sqrt(Player.JumpHeight * -2f * Player.Gravity);
        }

        private static void HandleInfiniteJump()
        {
            if (!infiniteJump) return;
            var input = Player.GetInputActions();
            if (input.Player.Jump.triggered)
            {
                FieldInfo velocityField = typeof(PlayerController).GetField("_velocity", BindingFlags.NonPublic | BindingFlags.Instance);
                if (velocityField != null)
                {
                    Vector3 currentVelocity = (Vector3)velocityField.GetValue(Player);
                    currentVelocity.y = Mathf.Sqrt(Player.JumpHeight * -2f * Player.Gravity);
                    velocityField.SetValue(Player, currentVelocity);
                }
                else
                {
                    Player.JumpHeight *= 1.1f;
                }
            }
        }

        private static void HandleSuperJump()
        {
            if (!superJump) return;

            float originalJumpHeight = 2f;
            Player.JumpHeight = superJump ? superJumpHeight : originalJumpHeight;
        }

        private static void CreateSpeedLinesEffect()
        {
            if (speedLinesEffect != null) return;
            speedLinesEffect = new GameObject("SpeedLines");
            var ps = speedLinesEffect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startSpeed = -20f;
            main.startLifetime = 0.5f;
            main.startSize = 0.1f;
            main.maxParticles = 100;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f;
            shape.radius = 0.1f;
            speedLinesEffect.SetActive(false);
        }

        private static void HandleSpeedLines()
        {
            if (speedLinesEffect == null) return;

            bool shouldShow = speedLines && Player.SelectedWalkSpeed > walkSpeed * 0.8f;

            if (shouldShow && !speedLinesEffect.activeSelf)
            {
                speedLinesEffect.transform.position = Player.transform.position + Player.transform.forward * 0.5f;
                speedLinesEffect.transform.rotation = Player.transform.rotation;
                speedLinesEffect.transform.SetParent(Player.PlayerCamera.transform);
                speedLinesEffect.SetActive(true);
                var ps = speedLinesEffect.GetComponent<ParticleSystem>();
                var emission = ps.emission;
                emission.rateOverTime = Mathf.Lerp(0, 50, (Player.SelectedWalkSpeed - walkSpeed * 0.8f) / (walkSpeed * 0.2f));
                ps.Play();
            }
            else if (!shouldShow && speedLinesEffect.activeSelf)
            {
                speedLinesEffect.SetActive(false);
            }
        }

        private static void HandleTimeScale()
        {
            if (!timeScale) return;

            Time.timeScale = timeScaleMultiplier;
            Time.fixedDeltaTime = 0.02f * timeScaleMultiplier;
        }

        private static void HandleESP()
        {
            if (!esp) return;

            float range = espRange;
            Collider[] colliders = Physics.OverlapSphere(Player.transform.position, range, Player.InteractLayerMask);

            foreach (var collider in colliders)
            {
                if (collider.GetComponent<Renderer>() != null)
                {
                }
            }
        }
        private static void HandleAutoCollect()
        {
            if (!autoCollect) return;

            float range = 5f;
            Collider[] colliders = Physics.OverlapSphere(Player.transform.position, range);

            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Grabbable") || collider.GetComponent<OrePiece>() != null)
                {
                    if (Vector3.Distance(Player.transform.position, collider.transform.position) < 2f)
                    {
                        collider.transform.position = Vector3.MoveTowards( collider.transform.position, Player.transform.position, Time.deltaTime * 10f
                        );
                    }
                }
            }
        }

        private static void HandleXrayVision()
        {
            if (!xrayVision)
            {
                if (Player != null && Player.PlayerCamera != null)
                {
                    Player.PlayerCamera.cullingMask = interactLayerMaskBackup;
                }
                return;
            }

            if (Player != null && Player.PlayerCamera != null)
            {
                Player.PlayerCamera.cullingMask = ~0;
            }
        }

        public static void ToggleAutoMinerESP()
        {
            autoMinerESP = !autoMinerESP;
            if (!autoMinerESP)
            {
                CleanupESP();
            }
            else
            {
                RefreshAutoMiners();
            }
        }

        public static void ToggleAutoMinerTracers()
        {
            autoMinerTracers = !autoMinerTracers;
            if (!autoMinerTracers)
            {
                foreach (var kvp in tracerLines)
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.enabled = false;
                    }
                }
            }
        }

        public static void PrintAutoMinerInfo()
        {
            RefreshAutoMiners();
            int activeCount = 0;
            int inactiveCount = 0;

            foreach (var miner in autoMiners)
            {
                if (miner == null) continue;

                if (miner.Enabled)
                    activeCount++;
                else
                    inactiveCount++;
            }

            Debug.Log($"AutoMiners: {autoMiners.Count} total");
            Debug.Log($"  Active: {activeCount}");
            Debug.Log($"  Inactive: {inactiveCount}");
            Debug.Log($"  ESP Objects: {espObjects.Count}");
            Debug.Log($"  Tracer Lines: {tracerLines.Count}");
        }

        public static void TeleportForward(float distance = 6f)
        {
            if (Player == null) return;

            Player.TeleportPlayer(Player.transform.position + Player.transform.forward * distance);
        }

        public static void TeleportToCursor()
        {
            if (Player == null || Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Player.TeleportPlayer(hit.point + Vector3.up * 2f);
            }
        }

        public static void TeleportUp(float height = 50f)
        {
            if (Player == null) return;

            Player.TeleportPlayer(Player.transform.position + Vector3.up * height);
        }

        private static void HandleMoneyMods()
        {
            if (economyManager == null) return;

            if (infiniteMoney)
            {
                economyManager.SetMoney(moneyAmount);
            }
        }

        private static void HandleShopMods()
        {
            if (economyManager == null) return;

            if (unlockAllShopItems)
            {
                economyManager.UnlockAllShopItems();
            }

            if (freeShopping)
            {
                SetMoneyToMax();
            }
        }

        private static void SetMoneyToMax()
        {
            try
            {
                var moneyField = typeof(EconomyManager).GetField("_money", BindingFlags.NonPublic | BindingFlags.Instance);
                if (moneyField != null)
                {
                    moneyField.SetValue(economyManager, float.MaxValue * 0.1f);
                }
                else
                {
                    var setMoneyMethod = typeof(EconomyManager).GetMethod("SetMoney", BindingFlags.Public | BindingFlags.Instance);
                    if (setMoneyMethod != null)
                    {
                        setMoneyMethod.Invoke(economyManager, new object[] { 999999999f });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting money to max: {e.Message}");
            }
        }

        public static bool CanAffordPatch(float price)
        {
            if (freeShopping)
            {
                return true;
            }

            if (economyManager != null)
            {
                var canAffordMethod = typeof(EconomyManager).GetMethod("CanAfford", BindingFlags.Public | BindingFlags.Instance);
                if (canAffordMethod != null)
                {
                    return (bool)canAffordMethod.Invoke(economyManager, new object[] { price });
                }
            }
            return false;
        }

        public static void AddMoney(float amount)
        {
            if (economyManager != null)
            {
                economyManager.AddMoney(amount);
            }
        }

        public static void SetMoney(float amount)
        {
            if (economyManager != null)
            {
                economyManager.SetMoney(amount);
            }
        }

        public static float GetMoney()
        {
            if (economyManager != null)
            {
                FieldInfo moneyField = typeof(EconomyManager).GetField("_money", BindingFlags.NonPublic | BindingFlags.Instance);
                if (moneyField != null)
                {
                    return (float)moneyField.GetValue(economyManager);
                }
            }
            return 0f;
        }

        public static void AddMoneyGUI()
        {
            AddMoney(moneyAmount);
        }

        public static void SetMoneyGUI()
        {
            SetMoney(moneyAmount);
        }

        public static void ResetMoney()
        {
            if (economyManager != null)
            {
                economyManager.SetMoney(0f);
            }
        }

        public static void ResetOverrides()
        {
            if (Player == null) return;

            Player.WalkSpeed = 4f;
            Player.SprintSpeed = 6f;
            Player.Gravity = -9.81f;
            Player.JumpHeight = 2f;
            storedVelocity = Vector3.zero;

            if (wasFlying)
            {
                Player.CharacterController.enabled = true;
                wasFlying = false;
            }

            if (Player.CharacterController != null)
            {
                Player.CharacterController.enabled = true;
                Player.StandingSlopeLimit = originalSlopeLimit;
                Player.CharacterController.stepOffset = originalStepOffset;
            }

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            if (Player.PlayerCamera != null)
            {
                Player.PlayerCamera.cullingMask = interactLayerMaskBackup;
            }

            if (speedLinesEffect != null)
            {
                speedLinesEffect.SetActive(false);
            }

            CleanupESP();
            autoMinerESP = false;
            autoMinerTracers = false;
            infiniteMoney = false;
            unlockAllShopItems = false;
            freeShopping = false;
            sellMultiplierEnabled = false;
            autoSellEnabled = false;
        }
    }
}