using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

public class CopySteamLibraries
{
    [PostProcessBuild(1)]
    public static void Copy(BuildTarget target, string pathToBuiltProject)
    {
        //
        // Only steam
        //
        if (!target.ToString().StartsWith("Standalone"))
            return;

        //
        // You only need a steam_appid.txt if you're launching outside of Steam, you don't need to ship with it
        // but most games do anyway.
        //

        FileUtil.ReplaceFile("steam_appid.txt", Path.GetDirectoryName(pathToBuiltProject) + "/steam_appid.txt");

        //
        // Put these dlls next to the exe
        //
        if (target == BuildTarget.StandaloneWindows64)
            FileUtil.ReplaceFile("steam_api64.dll", Path.GetDirectoryName(pathToBuiltProject) + "/steam_api64.dll");

        if (target == BuildTarget.StandaloneLinux64)
            FileUtil.ReplaceFile("libsteam_api64.so", Path.GetDirectoryName(pathToBuiltProject) + "/libsteam_api64.so");
    }
}
