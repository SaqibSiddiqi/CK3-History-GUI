using System;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using SimpleFileBrowser;
using System.Collections;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;

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



    //File Variables
    string filename;
    string output;
    string gamePath;
    string include;

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

    public void SetLanguage() 
    {
        language = languageDropdown.options[languageDropdown.value].text;
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
        //Run the executable with the given parameters
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
            saveFileText.text = filename;
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

}
