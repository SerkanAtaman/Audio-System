namespace SeroJob.AudioSystem.Editor
{
    public class AudioSystemAssetPostprocessor : UnityEditor.AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (deletedAssets != null && deletedAssets.Length > 0)
                AudioContainerLibraryEditorUtils.RemoveDeletedContainers(deletedAssets.Length);
        }
    }
}