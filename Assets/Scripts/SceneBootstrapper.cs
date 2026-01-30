using UnityEngine;

public static class SceneBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        Debug.Log("[SceneBootstrapper] Checking essential components...");

        // 1. Ensure Camera exists
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        
        // Remove DebugSpikeInfo if attached
        var debugInfo = cam.GetComponent("DebugSpikeInfo");
        if (debugInfo != null)
        {
             Object.Destroy(debugInfo);
        }

        // 2. Ensure GameManager
        if (Object.FindObjectOfType<GameManager>() == null)
        {
            GameObject gm = new GameObject("GameManager_Auto");
            gm.AddComponent<GameManager>();
        }

        // 4. Ensure SpikeSpawner
        if (Object.FindObjectOfType<SpikeSpawner>() == null)
        {
            GameObject ss = new GameObject("SpikeSpawner_Auto");
            ss.AddComponent<SpikeSpawner>();
        }
    }
}
