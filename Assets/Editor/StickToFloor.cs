using UnityEngine;
using UnityEditor;

public class SnapTreesToTerrain : MonoBehaviour
{
    [MenuItem("Tools/Snap Trees To Terrain")]
    static void SnapTrees()
    {
        // Find the "Trees" parent GameObject
        GameObject treesParent = GameObject.Find("Trees");
        if (treesParent == null)
        {
            Debug.LogError("Could not find a GameObject named 'Trees' in the scene.");
            return;
        }

        int snappedCount = 0;
        int skippedCount = 0;

        // Register the full undo operation
        Undo.RegisterFullObjectHierarchyUndo(treesParent, "Snap Trees To Terrain");

        foreach (Transform tree in treesParent.GetComponentsInChildren<Transform>(true))
        {
            // Skip the parent itself
            if (tree == treesParent.transform)
                continue;

            // Only process direct or indirect children that are actual tree roots
            // (skip transforms that are children of other children, i.e. sub-meshes)
            if (tree.parent != treesParent.transform)
                continue;

            Vector3 rayOrigin = tree.position + Vector3.up * 500f;
            Ray ray = new Ray(rayOrigin, Vector3.down);

            // Raycast against everything (terrain, meshes, colliders)
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Undo.RecordObject(tree, "Snap Tree To Terrain");
                tree.position = hit.point;
                snappedCount++;
            }
            else
            {
                // Fallback: try SampleHeight if a Terrain exists
                Terrain terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    float height = terrain.SampleHeight(tree.position);
                    Vector3 newPos = tree.position;
                    newPos.y = height + terrain.transform.position.y;
                    Undo.RecordObject(tree, "Snap Tree To Terrain");
                    tree.position = newPos;
                    snappedCount++;
                }
                else
                {
                    Debug.LogWarning($"Could not snap '{tree.name}' — no raycast hit and no active Terrain found.");
                    skippedCount++;
                }
            }
        }

        Debug.Log($"Snap Trees To Terrain complete: {snappedCount} snapped, {skippedCount} skipped.");
    }
}
