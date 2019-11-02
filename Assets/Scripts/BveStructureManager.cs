using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.Win32;

public class BveStructureManager : MonoBehaviour
{
    private UnityDragAndDropHook _hook;

    private BveImportStructureCsv Importer { get; set; }
    private List<string> CurrentImportedFiles { get; set; } = new List<string>();

    private void OnEnable()
    {
        // must be created on the main thread to get the right thread id.
        _hook = new UnityDragAndDropHook();
        _hook.InstallHook();
        _hook.OnDroppedFiles += OnFiles;
    }

    private void OnDisable()
    {
        _hook.UninstallHook();
    }

    private void OnFiles(List<string> files, POINT cursorPosition)
    {
        foreach (var file in files)
        {
            Importer.Parse(file);
        }


        CurrentImportedFiles = files;
    }

    // Start is called before the first frame update
    private void Start()
    {
        Importer = GetComponent<BveImportStructureCsv>();


#if UNITY_EDITOR
        var path = @"D:\BVE\Archives\BVE\Railway\Object\kintetsu_s\Pole4_0.csv";
        Importer.Parse(path);
        CurrentImportedFiles.Add(path);
#endif
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            ClearAllStructures();
            CurrentImportedFiles.Clear();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            ClearAllStructures();
            LoadStructuresFromList();
        }
    }

    public void ClearAllStructures()
    {
        for (var i = 0; i < transform.childCount; ++i)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void LoadStructuresFromList()
    {
        foreach (var file in CurrentImportedFiles)
        {
            Importer.Parse(file);
        }
    }
}
