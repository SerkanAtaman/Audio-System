using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace SeroJob.AudioSystem.Editor
{
    public class BuildCallbackReceiver : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            AudioContainerLibraryEditorUtils.FindAllContainers();
        }
    }
}