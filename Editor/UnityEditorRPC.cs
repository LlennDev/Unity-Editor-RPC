#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Threading.Tasks;
using Discord;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public static class UnityEditorRPC
{
    private const string applicationId = "1300876381994877099";
    private static Discord.Discord discord;

    private static long startTimestamp;

    private static string largeimage;

    #region Initialization
    static UnityEditorRPC()
    {
        DelayStart();
    }

    public static async void DelayStart(int delay = 1000)
    {
        await Task.Delay(delay);
        Init();
    }

    public static void Init()
    {
        // Start Discord plugin
        try
        {
            discord = new Discord.Discord(long.Parse(applicationId), (long)CreateFlags.Default);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return;
        }
        
        string unityver = Application.unityVersion;
        unityver = unityver.Substring(0, 4);

        switch (unityver)
        {
            case "6000":
                largeimage = "unity6";
                break;
            case "2023":
            case "2022":
            case "2021":
            case "2020":
                largeimage = "unitymodern";
                break;
            default:
                largeimage = "unityold";
                break;
        }

        // Get start timestamp
        startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Update activity on scene change
        EditorSceneManager.sceneOpened += OnSceneOpened;

        // Update activity
        EditorApplication.update += Update;
        UpdateActivity();
    }
    #endregion

    #region SceneChanged
    // Callback for scene changes
    private static void OnSceneOpened(Scene previousScene, OpenSceneMode  newScene)
    {
        UpdateActivity();
    }
    #endregion

    #region Update
    private static void Update()
    {
        if (discord != null) discord.RunCallbacks();
    }

    public static void UpdateActivity()
    {
        if (discord == null)
        {
            Init();
            return;
        }

        Activity activity = new Activity
        {
            State = "Scene: " + EditorSceneManager.GetActiveScene().name,
            Details = Application.productName,
            Timestamps = { Start = startTimestamp },
            Assets =
            {
                LargeImage = largeimage,
                LargeText = "Unity " + Application.unityVersion
            },
        };

        discord.GetActivityManager().UpdateActivity(activity, result =>
        {
            if (result != Result.Ok) Debug.LogError(result.ToString());
        });
    }
    #endregion
}
#endif
