namespace SeroJob.AudioSystem.Editor
{
    public class AudioSystemAssetPostprocessor : UnityEditor.AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            AudioContainerLibraryEditorUtils.RemoveDeletedContainers(deletedAssets.Length);
        }
    }
}