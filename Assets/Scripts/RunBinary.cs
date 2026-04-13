using System;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using SimpleFileBrowser;
using System.Collections;

public class RunBinary : MonoBehaviour
{

    //UI Variables
    [SerializeField]
    TMP_Text saveFileText;

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
    }

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
            filename = FileBrowser.Result[0];
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

    public void runCLI() 
    {
        //Run the executable with the given parameters
    }
}
