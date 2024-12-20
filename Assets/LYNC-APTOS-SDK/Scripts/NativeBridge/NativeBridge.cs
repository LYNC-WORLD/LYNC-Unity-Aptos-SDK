using LYNC;
using LYNC.DeepLink;
using LYNC.Wallet;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class NativeBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    private class WebGL
    {
        [DllImport("__Internal")]
        public static extern void WebGLLogin(string url, string websocketUrl, string gameObjectName);

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            DeepLinkManager.browserProcessStarted += BrowserProcessStarted;
        }

        private static void BrowserProcessStarted(string url)
        {

            if (Application.isEditor)
            {
                Debug.LogWarning("Aborting WebGL process because Unity is running in the Editor.");
                return;
            }


            WebGLLogin(url, LyncManager.BaseServerURL.Replace("http", "ws"), DeepLinkManager.gameObjectName);
        }
    }
#endif

#if UNITY_EDITOR_WIN || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
    private class StandaloneAndMobile
    {
        // Windows configuration
        private readonly string launcherPath = (Application.streamingAssetsPath + "/Executables/Launcher.exe").Replace("/", "\\");
        private readonly string registerPath = (Application.streamingAssetsPath + "/Executables/register.reg").Replace("/", "\\");
        private readonly string sharedFilePath = @"C:\ProgramData\launcherdata.txt";


        private Coroutine runningCoroutine = null;

        private static StandaloneAndMobile Instance;


        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            Instance = new StandaloneAndMobile();

        }
        private StandaloneAndMobile()
        {

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                RegisterCustomProtocol();
                ClearSharedFile();
                OpenLauncher();
            }

            DeepLinkManager.browserProcessStarted += BrowserProcessStarted;
        }

        private void BrowserProcessStarted(string url)
        {

            // Open auth page for standalone and mobile
            Application.OpenURL(url);

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                ClearSharedFile();
                OpenLauncher();
                runningCoroutine = LyncManager.Instance.StartCoroutine(ListenForBrowserMessageWindows());
            }
        }

        #region Windows platform methods
        private IEnumerator ListenForBrowserMessageWindows()
        {
            string text = File.ReadAllText(sharedFilePath);
            if (text.IndexOf(DeepLinkRegistration.DeepLinkUrl.ToLower()) > -1)
            {
                string url = text.Replace(System.Diagnostics.Process.GetCurrentProcess().Id.ToString(), "").Trim();
                ClearSharedFile();
                LyncManager.Instance.StopCoroutine(runningCoroutine);

                // Handle the message
                DeepLinkManager.Instance.HandleMessage(url);
                yield break;
            }

            yield return new WaitForSeconds(0.1f);

            runningCoroutine = LyncManager.Instance.StartCoroutine(ListenForBrowserMessageWindows());

        }

        private void OpenLauncher()
        {
            string processId = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(launcherPath, "processid" + processId);
        }

        private void ClearSharedFile()
        {
            using (StreamWriter writer = File.CreateText(sharedFilePath))
            {
                writer.Write("");
            }
        }

        private void RegisterCustomProtocol()
        {
            string tempFilePath = registerPath.Replace("register.reg", "temp.reg");
            string temp = File.ReadAllText(registerPath);
            temp = temp.Replace("%APP_NAME%", DeepLinkRegistration.DeepLinkUrl);
            temp = temp.Replace("%LAUNCHER_PATH%", launcherPath.Replace(@"\", @"\\"));

            using (StreamWriter writer = new StreamWriter(tempFilePath))
            {
                writer.Write(temp);
            }

            System.Diagnostics.Process regeditProcess = new System.Diagnostics.Process();
            regeditProcess.StartInfo.FileName = "C:\\Windows\\System32\\reg.exe";
            regeditProcess.StartInfo.Arguments = "import \"" + tempFilePath + "\"";
            regeditProcess.StartInfo.UseShellExecute = false;
            regeditProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            regeditProcess.Start();
            regeditProcess.WaitForExit();

            File.Delete(tempFilePath);
        }
        #endregion
    }
#endif
}
