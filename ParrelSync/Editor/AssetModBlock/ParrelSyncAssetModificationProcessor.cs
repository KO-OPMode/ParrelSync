using UnityEditor;
using UnityEngine;

namespace ParrelSync
{
    /// <summary>
    /// For preventing assets being modified from the clone instance.
    /// </summary>
    public class ParrelSyncAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        private const float MIN_TIME_BETWEEN_DIALOGS = 60f;
        private const float MIN_TIME_BETWEEN_LOGS = 1f;

        private static double _lastDialogTime = -1f;
        private static double _lastLogTime = -1f;
        
        private static string ModificationsBlockedTitle = ClonesManager.ProjectName + ": Asset modifications saving detected and blocked";

        private static string ModificationsBlockedDescription =
            "Asset modifications saving are blocked in the clone instance. \n\n" +
            "This is a clone of the original project. \n" +
            "Making changes to asset files via the clone editor is not recommended. \n" +
            "Please use the original editor window if you want to make changes to the project files.\n\n" +
            "You can enable asset modifications in this editor via the ParrelSync > Preferences menu if you wish."; 
        
        public static string[] OnWillSaveAssets(string[] paths)
        {
            if (ClonesManager.IsClone() && Preferences.AssetModPref.Value)
            {
                if (paths != null && paths.Length > 0 && !EditorQuit.IsQuiting)
                {
                    // Notify the player that we're blocking their changes
                    double time = EditorApplication.timeSinceStartup;
                    if (_lastDialogTime < 0f || time > _lastDialogTime + MIN_TIME_BETWEEN_DIALOGS)
                    {
                        // Only pop up a full blocking dialog occasionally
                        EditorUtility.DisplayDialog(
                            ModificationsBlockedTitle,
                            ModificationsBlockedDescription,
                            "Okay"
                        );

                        _lastDialogTime = time;
                    }

                    if (_lastLogTime < 0f || time > _lastLogTime + MIN_TIME_BETWEEN_LOGS)
                    {
                        // Log more frequently (but still not all the time in case we get a bunch of rapid sequential imports
                        Debug.LogWarning($"{ModificationsBlockedTitle}\n{ModificationsBlockedDescription}");
                        _lastLogTime = time;
                    }

                    foreach (var path in paths)
                    {
                        Debug.Log("Blocked attempt to save " + path + ".");
                    }
                }
                return new string[0] { };
            }
            return paths;
        }
    }
}