using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TreeTerrainTools : EditorWindow
{
    // ── Shared ────────────────────────────────────────────────────────────────
    private int selectedTab = 0;
    private readonly string[] tabNames = { "Snap to Terrain", "Mass Place Trees" };

    // ── Mass Placement ────────────────────────────────────────────────────────
    private List<PrefabEntry> prefabEntries = new List<PrefabEntry>();
    private int totalCount = 50;
    private float areaMinX = -50f;
    private float areaMaxX = 50f;
    private float areaMinZ = -50f;
    private float areaMaxZ = 50f;
    private bool useTerrainBounds = true;

    private float minScale = 0.8f;
    private float maxScale = 1.2f;
    private bool randomRotationY = true;
    private float slopeLimit = 45f;
    private bool avoidWater = true;
    private float waterHeight = 0f;

    private string parentName = "Trees";
    private int placementSeed = 0;
    private bool useRandomSeed = true;

    private Vector2 scrollPos;

    // ── Snap ──────────────────────────────────────────────────────────────────
    private string snapParentName = "Trees";

    // ── Serialised prefab entry ───────────────────────────────────────────────
    [System.Serializable]
    private class PrefabEntry
    {
        public GameObject prefab;
        public float weight = 1f;
    }

    // ── Open window ───────────────────────────────────────────────────────────
    [MenuItem("Tools/Tree Terrain Tools")]
    public static void ShowWindow()
    {
        var w = GetWindow<TreeTerrainTools>("Tree Terrain Tools");
        w.minSize = new Vector2(380, 520);
    }

    // Keep legacy menu item working
    [MenuItem("Tools/Snap Trees To Terrain")]
    static void SnapTreesLegacy()
    {
        ShowWindow();
    }

    // ── GUI ───────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        EditorGUILayout.Space(6);
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        EditorGUILayout.Space(4);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (selectedTab == 0)
            DrawSnapTab();
        else
            DrawPlaceTab();

        EditorGUILayout.EndScrollView();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TAB 0 – SNAP
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawSnapTab()
    {
        EditorGUILayout.HelpBox(
            "Finds every direct child of the named parent and snaps it down onto the terrain.",
            MessageType.Info);

        EditorGUILayout.Space(4);
        snapParentName = EditorGUILayout.TextField("Parent Object Name", snapParentName);
        EditorGUILayout.Space(8);

        if (GUILayout.Button("Snap Trees to Terrain", GUILayout.Height(36)))
            SnapTrees(snapParentName);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TAB 1 – MASS PLACE
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawPlaceTab()
    {
        // ── Prefab list ──
        Section("Tree Prefabs");
        if (prefabEntries.Count == 0)
            EditorGUILayout.HelpBox("Add at least one prefab.", MessageType.Warning);

        for (int i = 0; i < prefabEntries.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            prefabEntries[i].prefab = (GameObject)EditorGUILayout.ObjectField(
                prefabEntries[i].prefab, typeof(GameObject), false);
            EditorGUILayout.LabelField("Weight", GUILayout.Width(46));
            prefabEntries[i].weight = EditorGUILayout.FloatField(
                Mathf.Max(0.01f, prefabEntries[i].weight), GUILayout.Width(48));
            if (GUILayout.Button("✕", GUILayout.Width(24)))
            { prefabEntries.RemoveAt(i); break; }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("+ Add Prefab"))
            prefabEntries.Add(new PrefabEntry());

        // ── Count & area ──
        EditorGUILayout.Space(4);
        Section("Placement Area");

        totalCount = EditorGUILayout.IntSlider("Tree Count", totalCount, 1, 2000);

        useTerrainBounds = EditorGUILayout.Toggle("Use Full Terrain Bounds", useTerrainBounds);
        if (!useTerrainBounds)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("X Range");
            EditorGUI.indentLevel++;
            areaMinX = EditorGUILayout.FloatField("Min", areaMinX);
            areaMaxX = EditorGUILayout.FloatField("Max", areaMaxX);
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Z Range");
            EditorGUI.indentLevel++;
            areaMinZ = EditorGUILayout.FloatField("Min", areaMinZ);
            areaMaxZ = EditorGUILayout.FloatField("Max", areaMaxZ);
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        // ── Transform ──
        EditorGUILayout.Space(4);
        Section("Transform Randomisation");
        EditorGUILayout.MinMaxSlider("Scale Range",
            ref minScale, ref maxScale, 0.1f, 5f);
        EditorGUILayout.LabelField($"   {minScale:F2}  →  {maxScale:F2}",
            EditorStyles.miniLabel);
        randomRotationY = EditorGUILayout.Toggle("Random Y Rotation", randomRotationY);

        // ── Filters ──
        EditorGUILayout.Space(4);
        Section("Placement Filters");
        slopeLimit = EditorGUILayout.Slider("Max Slope (°)", slopeLimit, 0f, 90f);
        avoidWater = EditorGUILayout.Toggle("Avoid Water (Y threshold)", avoidWater);
        if (avoidWater)
        {
            EditorGUI.indentLevel++;
            waterHeight = EditorGUILayout.FloatField("Water Height", waterHeight);
            EditorGUI.indentLevel--;
        }

        // ── Output ──
        EditorGUILayout.Space(4);
        Section("Output");
        parentName = EditorGUILayout.TextField("Parent Object Name", parentName);
        useRandomSeed = EditorGUILayout.Toggle("Random Seed", useRandomSeed);
        if (!useRandomSeed)
        {
            EditorGUI.indentLevel++;
            placementSeed = EditorGUILayout.IntField("Seed", placementSeed);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        GUI.enabled = prefabEntries.Count > 0 &&
                      prefabEntries.Exists(e => e.prefab != null);

        if (GUILayout.Button("Place Trees", GUILayout.Height(36)))
            PlaceTrees();

        GUI.enabled = true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LOGIC – SNAP
    // ─────────────────────────────────────────────────────────────────────────
    static void SnapTrees(string parentName)
    {
        GameObject treesParent = GameObject.Find(parentName);
        if (treesParent == null)
        {
            Debug.LogError($"[TreeTerrainTools] No GameObject named '{parentName}' found.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(treesParent, "Snap Trees To Terrain");

        int snapped = 0, skipped = 0;

        foreach (Transform tree in treesParent.GetComponentsInChildren<Transform>(true))
        {
            if (tree == treesParent.transform || tree.parent != treesParent.transform)
                continue;

            if (TryGetTerrainY(tree.position, out float y))
            {
                Undo.RecordObject(tree, "Snap Tree");
                Vector3 p = tree.position; p.y = y;
                tree.position = p;
                snapped++;
            }
            else
            {
                Debug.LogWarning($"[TreeTerrainTools] Could not snap '{tree.name}'.");
                skipped++;
            }
        }

        Debug.Log($"[TreeTerrainTools] Snap complete — {snapped} snapped, {skipped} skipped.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LOGIC – PLACE
    // ─────────────────────────────────────────────────────────────────────────
    private void PlaceTrees()
    {
        Terrain terrain = Terrain.activeTerrain;

        // Determine placement bounds
        float xMin, xMax, zMin, zMax;
        if (useTerrainBounds && terrain != null)
        {
            Vector3 tp = terrain.transform.position;
            Vector3 ts = terrain.terrainData.size;
            xMin = tp.x; xMax = tp.x + ts.x;
            zMin = tp.z; zMax = tp.z + ts.z;
        }
        else
        {
            xMin = areaMinX; xMax = areaMaxX;
            zMin = areaMinZ; zMax = areaMaxZ;
        }

        // Build weighted prefab table
        var table = BuildWeightedTable();
        if (table.Count == 0)
        {
            Debug.LogError("[TreeTerrainTools] No valid prefabs assigned.");
            return;
        }

        // Seed
        int seed = useRandomSeed ? Random.Range(0, int.MaxValue) : placementSeed;
        Random.InitState(seed);
        Debug.Log($"[TreeTerrainTools] Using seed {seed}");

        // Get or create parent
        GameObject parent = GameObject.Find(parentName)
                         ?? new GameObject(parentName);
        Undo.RegisterCreatedObjectUndo(parent, "Place Trees");

        int placed = 0, rejected = 0;
        int maxAttempts = totalCount * 10;

        for (int attempt = 0; attempt < maxAttempts && placed < totalCount; attempt++)
        {
            float x = Random.Range(xMin, xMax);
            float z = Random.Range(zMin, zMax);

            // Get terrain height at this XZ
            Vector3 samplePos = new Vector3(x, 0f, z);
            if (!TryGetTerrainY(samplePos, out float y))
            { rejected++; continue; }

            Vector3 pos = new Vector3(x, y, z);

            // Slope filter
            if (terrain != null && slopeLimit < 90f)
            {
                float slope = GetSlope(terrain, pos);
                if (slope > slopeLimit) { rejected++; continue; }
            }

            // Water filter
            if (avoidWater && y <= waterHeight) { rejected++; continue; }

            // Pick prefab
            GameObject prefab = PickPrefab(table);

            // Instantiate
            float scale = Random.Range(minScale, maxScale);
            float rotY = randomRotationY ? Random.Range(0f, 360f) : 0f;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            Undo.RegisterCreatedObjectUndo(go, "Place Tree");

            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            go.transform.localScale = Vector3.one * scale;

            placed++;
        }

        Debug.Log($"[TreeTerrainTools] Placed {placed} trees, rejected {rejected} candidates (seed {seed}).");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────
    static bool TryGetTerrainY(Vector3 pos, out float y)
    {
        // ── Primary: SampleHeight directly on the Terrain ─────────────────────
        // This reads the heightmap and is immune to buildings, roads, or any
        // other colliders sitting above the ground.
        Terrain terrain = FindTerrainAt(pos);
        if (terrain != null)
        {
            y = terrain.SampleHeight(pos) + terrain.transform.position.y;
            return true;
        }

        // ── Fallback: raycast filtered to TerrainCollider only ────────────────
        // Builds a layer mask that includes every layer a TerrainCollider lives
        // on, then double-checks the hit component is actually a TerrainCollider
        // so we never land on a building or road mesh.
        Ray ray = new Ray(new Vector3(pos.x, pos.y + 500f, pos.z), Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        float bestY = float.MaxValue;
        bool found = false;

        foreach (var hit in hits)
        {
            if (hit.collider is TerrainCollider)
            {
                if (hit.point.y < bestY)   // pick the lowest terrain surface
                {
                    bestY = hit.point.y;
                    found = true;
                }
            }
        }

        if (found) { y = bestY; return true; }

        y = 0f;
        return false;
    }

    // Returns the Terrain whose XZ bounds contain the given world position.
    // Handles multi-terrain setups; falls back to activeTerrain when only one exists.
    static Terrain FindTerrainAt(Vector3 pos)
    {
        foreach (Terrain t in Terrain.activeTerrains)
        {
            Vector3 tp = t.transform.position;
            Vector3 ts = t.terrainData.size;
            if (pos.x >= tp.x && pos.x <= tp.x + ts.x &&
                pos.z >= tp.z && pos.z <= tp.z + ts.z)
                return t;
        }
        return Terrain.activeTerrain; // single-terrain fallback (may be null)
    }

    static float GetSlope(Terrain terrain, Vector3 worldPos)
    {
        TerrainData td = terrain.terrainData;
        Vector3 tp = terrain.transform.position;
        float nx = (worldPos.x - tp.x) / td.size.x;
        float nz = (worldPos.z - tp.z) / td.size.z;
        nx = Mathf.Clamp01(nx);
        nz = Mathf.Clamp01(nz);
        return td.GetSteepness(nx, nz);
    }

    private List<(GameObject prefab, float cumWeight)> BuildWeightedTable()
    {
        var table = new List<(GameObject, float)>();
        float total = 0f;
        foreach (var e in prefabEntries)
        {
            if (e.prefab == null) continue;
            total += e.weight;
            table.Add((e.prefab, total));
        }
        return table;
    }

    static GameObject PickPrefab(List<(GameObject prefab, float cumWeight)> table)
    {
        float maxW = table[table.Count - 1].cumWeight;
        float r = Random.Range(0f, maxW);
        foreach (var (prefab, cumW) in table)
            if (r <= cumW) return prefab;
        return table[table.Count - 1].prefab;
    }

    // ── UI helper ─────────────────────────────────────────────────────────────
    static void Section(string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        Rect r = GUILayoutUtility.GetLastRect();
        r.y += EditorGUIUtility.singleLineHeight - 2;
        r.height = 1;
        EditorGUI.DrawRect(r, new Color(0.4f, 0.4f, 0.4f, 0.6f));
        EditorGUILayout.Space(2);
    }
}