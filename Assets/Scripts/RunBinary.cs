using System;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using SimpleFileBrowser;
using System.Collections;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using System.IO;
using UnityEngine.DedicatedServer;

public class RunBinary : MonoBehaviour
{

    #region Variables

    // UI Text Variables
    [SerializeField] TMP_Text saveFileText; // Text to display the selected save file name
    [SerializeField] TMP_InputField consoleText; // Text to display the console output
    // UI Dropdown Variables
    [SerializeField] TMP_Dropdown languageDropdown; // Dropdown to select the language
    [SerializeField] TMP_Dropdown depthDropdown; // Dropdown to select the depth

    // UI Toggle Variables
    [SerializeField] Toggle dumpToggle; // Toggle to enable or disable the --dump flag
    [SerializeField] Toggle visToggle; // Toggle to enable or disable the --no-vis flag
    [SerializeField] Toggle devModeToggle; // Toggle to enable or disable the --no-interaction flag
    [SerializeField] Toggle templateToggle; // Toggle to enable or disable the --use-internal-templates flag

    // UI Button Variables
    [SerializeField] Button gamePathButton; // Button to open the save file dialog
    [SerializeField] Button dumpButton; // Button to run the dump command
    [SerializeField] GameObject logButton; // Button to open the log file

    // UI Panel Variables
    [SerializeField] GameObject devPanel; // Panel to show the dev options

    //Filepath Variables
    string filename; // The path to the save file
    string output; // The path to the output folder
    string gamePath; // The path to the game folder
    string include; // The path to the mods folder
    string gitPath; // The path to the git folder
    string executablePath; // Path to the executable
    string jsonDump; // The name of the dump file
    string logPath; // The path to the log file

    //Output Variables 
    int depth; // The depth of the output
    string language; // The language of the output
    bool noVis; // The bool function to disable images

    //Debug Variables
    bool dump; // The bool function to enable the --dump flag {NOT WORKING}
    string dumpData; // The path to the dump data folder {NOT WORKING}
    
    bool noInteraction; // The bool function to enable the --no-interaction flag {Always enabled}
    bool useInternal; // The bool function to enable the --use-internal-templates flag

    #endregion

    #region Common Functions
    void Awake()        // This function is called when the script is loaded, it will define the variables and set the default values
    {
        DefineArgs();
        DefineSerialized();
        InitPaths();
        DisableLanguage();
    }

    void checkLinuxMac()
    {
#if !UNITY_EDITOR_Win && !UNITY_STANDALONE_WIN
    var chmod = Process.Start("chmod", $"+x \"{executablePath}\"");
    chmod.WaitForExit();
#endif
    }

    void DefineSerialized()
    {
        // This function is used to define the serialized variables
        saveFileText ??= GameObject.Find("SaveFileText").GetComponent<TMP_Text>();
        consoleText ??= GameObject.Find("ConsoleText").GetComponent<TMP_InputField>();

        languageDropdown ??= GameObject.Find("LanguageDropdown").GetComponent<TMP_Dropdown>();
        depthDropdown ??= GameObject.Find("DepthDropdown").GetComponent<TMP_Dropdown>();

        dumpToggle ??= GameObject.Find("Dump").GetComponent<Toggle>();
        visToggle ??= GameObject.Find("NoVis").GetComponent<Toggle>();
        devModeToggle ??= GameObject.Find("DevMode").GetComponent<Toggle>();
        templateToggle ??= GameObject.Find("TemplateToggle").GetComponent<Toggle>();

        dumpButton ??= GameObject.Find("DumpFinder").GetComponent<Button>();
        gamePathButton ??= GameObject.Find("GameFinder").GetComponent<Button>();
        logButton ??= GameObject.Find("LogButton");

        devPanel ??= GameObject.Find("DevPanel");

        dumpButton.interactable = false; // Disable the dump button until the dump toggle is enabled
    }

    void DefineArgs()
    {
        // Default values of arguments
        filename = null;
        output = null;
        gamePath = null;
        include = null;
        gitPath = null;
        executablePath = null;
        jsonDump = @"\dump.json";
        logPath = null;

        depth = 3;
        language = "english";
        noVis = false;

        dump = false;
        dumpData = null;

        noInteraction = true;

        useInternal = false;
    }
    void DisableLanguage() 
    {
        languageDropdown.interactable = false;
        languageDropdown.itemText.text = "Language selection is not available on this platform.";  
    }
    #endregion

    #region UI Button Functions
    public void OpenSaveDialog()
    {
        StartCoroutine(SaveDialog());
    }

    public void OpenOutputDialog()
    {
        StartCoroutine(OutputDialog());
    }

    public void OpenGamePathDialog()
    {
        StartCoroutine(GamePathDialog());
    }

    public void OpenDumpPathDialog()
    {
        StartCoroutine(DumpData());
    }

    public void OpenGitPathDialog()
    {
        StartCoroutine(GitFolder());
    }
    public void SetLanguage()
    {
        language = languageDropdown.options[languageDropdown.value].text;
        language = language.ToLower();
        if (language == "simplified chinese")
        {
            language = "simp_chinese"; //As the CLI uses "simp_chinese" instead of "simplified chinese" for the language argument, we need to change it here
        }
        Debug.Log("Language set to: " + language);
    }

    public void SetDepth()
    {
        depth = int.Parse(depthDropdown.options[depthDropdown.value].text);
    }

    public void SetDumpButton()
    {
        if (dumpToggle.isOn)
        {
            dump = true;
            dumpButton.interactable = true;
        }
        else
        {
            dump = false;
            dumpButton.interactable = false;
        }
    }
    public void OpenLog()
    {
        if (!string.IsNullOrEmpty(logPath) && File.Exists(logPath))
        {
            Process.Start("explorer.exe", $"/select,\"{logPath}\"");
        }
        else
        {
            consoleText.text += "Log file not found: " + logPath;
        }
    }
    #endregion

    #region UI Toggle Functions

    public void checkVisToggle()
    {
        if (visToggle.isOn)
        {
            noVis = true;
        }
        else
        {
            noVis = false;
        }
    }

    public void checkDevModeToggle()
    {
        if (devModeToggle.isOn)
        {
            noInteraction = true;
            devPanel.SetActive(true);
        }
        else
        {
            noInteraction = false;
            devPanel.SetActive(false);
        }
    }

    public void checkTemplateToggle()
    {
        if (templateToggle.isOn)
        {
            useInternal = true;
        }
        else
        {
            useInternal = false;
        }
    }
    #endregion

    #region Main
    public void runCLI()
    {
        executablePath = Path.Combine(Application.streamingAssetsPath, "CK3 Extractor", "Binary", "ck3_history_extractor.exe");
        consoleText.text = executablePath;

        checkLinuxMac();

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = false,
        };

        // Add required args
        startInfo.ArgumentList.Add(filename);
        startInfo.ArgumentList.Add("--output"); startInfo.ArgumentList.Add(output);
        startInfo.ArgumentList.Add("--game-path"); startInfo.ArgumentList.Add(gamePath);
        startInfo.ArgumentList.Add("--language"); startInfo.ArgumentList.Add(language.ToLower());
        startInfo.ArgumentList.Add("--depth"); startInfo.ArgumentList.Add(depth.ToString());

        // Add optional flags only when true
        if (noVis) startInfo.ArgumentList.Add("--no-vis");
        //if (dump) startInfo.ArgumentList.Add("-- --dump " + dumpData + jsonDump);
        if (noInteraction) startInfo.ArgumentList.Add("--no-interaction");
        if (useInternal) startInfo.ArgumentList.Add("--use-internal-templates");

        // Add --dump-data only if dump is enabled and dumpData is set
        if (dump && !string.IsNullOrEmpty(dumpData))
        {
            startInfo.ArgumentList.Add("--dump-data");
            startInfo.ArgumentList.Add(dumpData);
        }

        consoleText.text = "Running CLI with arguments: " + string.Join(" ", startInfo.ArgumentList);

        var process = new Process { StartInfo = startInfo };

        process.Start();

        consoleText.text += "\nProcess started with PID: " + process.Id;
        string CLIout = process.StandardOutput.ReadToEnd();
        string CLIerr = process.StandardError.ReadToEnd();
        consoleText.text += "\nStandard Output: " + CLIout;

        process.WaitForExit();
        consoleText.text += "\nProcess exited with code: " + process.ExitCode;       


        if (!string.IsNullOrEmpty(CLIerr))
        {
            consoleText.text += "\nError: " + CLIerr;
            logPath = ExtractReportPath(CLIerr);
            logButton.SetActive(true);
        }
        else
        {
            consoleText.text += "\nOutput: " + CLIout;
            logButton.SetActive(false);
            Process.Start("explorer.exe", output);
        }
    }
#endregion

    #region Sub Functions
    static string CleanSaveFileName(string fullPath)
    {
        // Get just the filename without directory and extension
        string fileNameNoExt = Path.GetFileNameWithoutExtension(fullPath);

        // Replace underscores with spaces
        string cleaned = fileNameNoExt.Replace('_', ' ');

        return cleaned;
    }

    public string ExtractReportPath(string errorText)
    {
        int start = errorText.IndexOf('"') + 1;
        int end = errorText.IndexOf('"', start);

        if (start > 0 && end > start)
            return errorText.Substring(start, end - start);

        return null;
    }

    public void UpdateButton()
    {
        Process.Start(Path.Combine(Application.streamingAssetsPath, "CK3 Extractor", "Binary"));
    }

    public void HelpButton()
    {
        Process.Start("https://github.com/SaqibSiddiqi/CK3-History-GUI/blob/main/README.md");
    }

    public void OriginalRepo()
    {
        Process.Start("https://github.com/TCA166/CK3-history-extractor");
    }
    #endregion

        #region Dialog Functions
    // Coroutines to open the file dialogs, these will be called by the button functions
    IEnumerator SaveDialog()
    {
        string initialPath = string.IsNullOrEmpty(filename) ? null : Path.GetDirectoryName(filename);
        string initialFile = string.IsNullOrEmpty(filename) ? null : Path.GetFileName(filename);

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, initialPath, initialFile, "Select Game Save File", "Load");

        if (FileBrowser.Success)
        {
            filename = FileBrowser.Result[0];
            saveFileText.text = CleanSaveFileName(filename);
            saveFileText.enableAutoSizing = true;
            SavePaths();
        }
    }

    IEnumerator OutputDialog()
    {
        string initialPath = string.IsNullOrEmpty(output) ? null : output;

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, initialPath, null, "Select Output Folder", "Select");

        if (FileBrowser.Success)
        {
            output = FileBrowser.Result[0];
            SavePaths();
        }
    }

    IEnumerator GamePathDialog()
    {
        string initialPath = string.IsNullOrEmpty(gamePath) ? null : gamePath;

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, initialPath, null, "Select Game Folder", "Select");

        if (FileBrowser.Success)
        {
            gamePath = FileBrowser.Result[0];
            gamePathButton.image.color = gamePath.Contains(@"steamapps\common\Crusader Kings III") ? Color.green : Color.red;
            SavePaths();
        }
    }

    IEnumerator DumpData()
    {
        string initialPath = string.IsNullOrEmpty(dumpData) ? null : dumpData;

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, initialPath, null, "Select Dump Folder", "Select");

        if (FileBrowser.Success)
        {
            dumpData = FileBrowser.Result[0];
            SavePaths();
        }
    }

    IEnumerator GitFolder()
    {
        string initialPath = string.IsNullOrEmpty(gitPath) ? null : gitPath;

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, initialPath, null, "Select Git Folder", "Select");

        if (FileBrowser.Success)
        {
            gitPath = FileBrowser.Result[0];
            SavePaths();
        }
    }
    #endregion


    #region Saved Paths
    private static string SavedPathsFile;

    [System.Serializable]
    private class SavedPaths
    {
        public string filename;
        public string output;
        public string gamePath;
        public string dumpData;
        public string gitPath;
    }

    void InitPaths()
    {
        SavedPathsFile = Path.Combine(Application.persistentDataPath, "savedPaths.json");
        SavedPaths saved = LoadPaths();
        filename = saved.filename ?? "";
        output = saved.output ?? "";
        gamePath = saved.gamePath ?? "";
        dumpData = saved.dumpData ?? "";
        gitPath = saved.gitPath ?? "";
    }

    void SavePaths()
    {
        SavedPaths paths = new SavedPaths
        {
            filename = filename,
            output = output,
            gamePath = gamePath,
            dumpData = dumpData,
            gitPath = gitPath
        };
        File.WriteAllText(SavedPathsFile, JsonUtility.ToJson(paths, true));
    }

    private SavedPaths LoadPaths()
    {
        if (File.Exists(SavedPathsFile))
            return JsonUtility.FromJson<SavedPaths>(File.ReadAllText(SavedPathsFile));
        return new SavedPaths();
    }
    #endregion
}


