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

    //UI Variables
    [SerializeField]
    TMP_Text saveFileText;
    [SerializeField]
    TMP_Dropdown languageDropdown;
    [SerializeField]
    TMP_Dropdown depthDropdown;

    [SerializeField]
    Toggle dumpToggle;
    [SerializeField]
    Button dumpButton;

    [SerializeField]
    Toggle visToggle;

    [SerializeField]
    Toggle devModeToggle;
    [SerializeField]
    GameObject devPanel;

    [SerializeField]
    Toggle templateToggle;

    [SerializeField]
    TMP_Text consoleText;



    //File Variables
    string filename;
    string output;
    string gamePath;
    string include;
    string gitPath;
    string executablePath = "REDACTED_PATH_TO_EXECUTABLE"; // Path to the executable, this will be set by the user through the UI
    string jsonDump = @"\dump.json"; // The name of the dump file, this will be appended to the dumpData path when the --dump flag is used

    //Output Variables 
    int depth = 3;
    string language = "english";
    bool noVis;

    //Debug Variables
    bool dump;
    string dumpData;
    bool noInteraction;
    bool useInternal;

    // Nullify the variables to prevent errors, they will be set by the user through the UI
    void Awake()
    {
        // Default values
        filename = null;
        depth = 3;
        language = "english";
        gamePath = null;
        include = null;
        output = ".";
        dump = false;
        dumpData = null;
        noVis = false;
        noInteraction = false;
        useInternal = false;

        dumpButton.interactable = false;
    }

    // Button functions to set the variables, these will be called by the UI buttons
    public void GetInfo()
    {
        filename = saveFileText.text;
    }

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
        language.ToLower();
    }

    public void SetDepth()
    {
        depth = int.Parse(depthDropdown.options[depthDropdown.value].text);
    }

    public void SetDumpButton()
    {
        if(dumpToggle.isOn)
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

    public void runCLI()
    {
        executablePath = Path.Combine(Application.streamingAssetsPath, "CK3 Extractor", "Binary", "ck3_history_extractor.exe");
        Debug.Log(executablePath);
        consoleText.text = executablePath;

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

        Debug.Log("Running CLI with arguments: " + string.Join(" ", startInfo.ArgumentList));
        consoleText.text = "Running CLI with arguments: " + string.Join(" ", startInfo.ArgumentList);

        var process = new Process { StartInfo = startInfo };
        
        process.Start();

        consoleText.text += "\nProcess started with PID: " + process.Id + "A terminal should be running";
        string CLIout = process.StandardOutput.ReadToEnd();
        string CLIerr = process.StandardError.ReadToEnd();

        consoleText.text += "\nProcess exited with code: " + process.ExitCode;

        if (!string.IsNullOrEmpty(CLIerr))
        {
            Debug.LogError("Error: " + CLIerr); 
            consoleText.text += "\nError: " + CLIerr;
        }
        else
        {
            Debug.Log("Output: " + CLIout);
            consoleText.text += "\nOutput: " + CLIout;
        }
    }
    static string CleanSaveFileName(string fullPath)
    {
        // Get just the filename without directory and extension
        string fileNameNoExt = Path.GetFileNameWithoutExtension(fullPath);

        // Replace underscores with spaces
        string cleaned = fileNameNoExt.Replace('_', ' ');

        return cleaned;
    }

    // Coroutines to open the file dialogs, these will be called by the button functions
    IEnumerator SaveDialog()
    {
        /* 
         public static IEnumerator WaitForLoadDialog(   
                                                        PickMode pickMode, 
                                                        bool allowMultiSelection = false, 
                                                        string initialPath = null, 
                                                        string initialFilename = null, 
                                                        string title = "Load", 
                                                        string loadButtonText = "Select" 
                                                    );
         */
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Select Game Save File", "Load");

        if (FileBrowser.Success)
        {
            filename = FileBrowser.Result[0];
            saveFileText.text = CleanSaveFileName(filename);
            saveFileText.enableAutoSizing = true;
        }

    }

    IEnumerator OutputDialog() 
    {
        
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, null, null, "Select Output Folder", "Select");

        if (FileBrowser.Success)
        {
            output = FileBrowser.Result[0];
        }
    }

    IEnumerator GamePathDialog()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, null, null, "Select Game Folder", "Select");
        if (FileBrowser.Success)
        {
            gamePath = FileBrowser.Result[0];
        }
    }

    IEnumerator DumpData()
    {
        //Run the executable with the given parameters to dump the data, then read the output and store it in the dumpData variable
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, null, null, "Select Dump Folder", "Select");
        if (FileBrowser.Success)
        { 
            dumpData = FileBrowser.Result[0];
        }

    }

    IEnumerator GitFolder()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, null, null, "Select Git Folder", "Select");
        if (FileBrowser.Success)
        {
            string gitPath = FileBrowser.Result[0];
        }
    }

}
