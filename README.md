<img src=https://github.com/kyle2277/bf2_Frosty_Mesh_Importer/blob/master/FrostyMeshImporter/FrostyMeshImportIcon.png width="100" height="100"></img>
___
# TigerVenom22's Frosty Mesh Importer for SWBF2
*Formerly the Frosty Res/Chunk Importer*
## Overview
Running this app creates an instance of Frosty Editor and injects mesh importing functionalities. The added tools streamline the modding workflow by making importing and reverting one or multiple mesh sets in Frosty Editor a one-click operation. It automates the handling of chunk and res files output by the mesh converter application, FrostMeshy. This program also integrates FrosTxt, a utility for merging localization (UI text) files, into Frosty Editor.

64-bit Windows 10 build download (v1.2.0.4): https://www.mediafire.com/file/pb3f9js8iq1xrjy/Frosty_Mesh_Importer_v1.2.0.4.zip/file  
64-bit Windows 10 build download for Frosty Editor Alpha (v1.2.0.4): https://www.mediafire.com/file/13owebn4i7i1532/Frosty_Alpha_Mesh_Importer_v1.2.0.4.zip/file

## Documentation Table of Contents
> [Requirements](#requirements)  
> [Installation](#installation)  
> [Selecting a Toolkit](#selecting-a-toolkit)  
> [How to Mesh Import From File](#mesh-import-from-file)  
> > [Mesh Import Overview](#mesh-import-overview)  
> > [How to Export Mesh Res Files](#how-to-export-mesh-res-files)  
> > [Basic Mesh Importing](#basic-mesh-importing)  
> > [Basic Mesh Reversion](#basic-mesh-reversion)  
>
> [How to Mesh Import From a Linked FrostMeshy Output](#mesh-import-from-linked-frostmeshy-output)    
> > [Linking Frosty Mesh Importer and FrostMeshy](#linking-frosty-mesh-importer-to-a-frostmeshy-output-folder)  
> > [Importing From a FrostMeshy Output Folder](#importing-from-a-frostmeshy-output-folder)  
>
> [Re-Importing and Reverting From History](#re-importing-and-reverting-from-history)  
> [FrosTxt: Merging Localization Files](#frostxt-merging-localization-files)  
> > [FrosTxt Overview](#frostxt-overview)  
> > [Opening FrosTxt](#opening-frostxt)  
> > [Merging Localization Files](#merging-localization-files)  
> > [Reverting Localization Files](#reverting-localization-files)  
>
> [Troubleshooting Tips](#troubleshooting-tips)

## Requirements
* Frosty Mod Editor v1.0.5.9 - [download link](https://frostytoolsuite.com/downloads.html)
* FrostMeshy v0.6.1.8 - [download link](https://www.mediafire.com/file/bmhr27uv2to2gmf/fmy-v0618-pre.zip/file)

## Installation
Install by placing `Frosty Mesh Importer.exe` and `FrosTxtCore.dll` in the same folder as your Frosty Editor installation.

## Selecting a Toolkit
The `Toolkit` button on the toolbar allows the user to choose what buttons are dislayed on the toolbar. Frosty Mesh Importer has three toolbar settings: 
1. `Mesh Import (Default)` - The default toolkit for modifying mesh files. This setting displays all buttons related to mesh importing on the toolbar.
2. `FrostMeshy Import` - A more concise toolkit for modifying mesh files. This setting only displays FrostMeshy related mesh importing buttons.
3. `FrosTxt` - A utility for merging localization (UI text) files. This setting displays buttons for opening FrosTxt and reverting localization files. 

## Mesh Import From File

### Mesh Import Overview
Mesh set data in the Frostbite game engine is stored in two types of files: Compiles Resource (res) and Binary Chunk (chunk) files. Each mesh set has corresponding chunk and res files. Importing meshes into Frosty Editor involves exporing a mesh's respective res files, converting the mesh to-be-imported using FrostMeshy ([FrostMeshy tutorial](https://youtu.be/y5aK0nRnzKE)), and importing the chunk and res files generated by FrostMeshy. Frosty Mesh Importer automates the exporting and importing steps of this process.

### How to Export Mesh Res Files
Before a mesh can be imported, its Compiled Resource (res) files must be exported for FrostMeshy. A mesh's res files can be exported either of the two following ways:  
- Export from the main asset explorer. Right click on the mesh in the asset explorer and click `Export Mesh Files`. Use the file select pop-up to navigate into the                desired output folder and click `Open`. All the res files belonging to the selected mesh will be exported to this location.  
- Export from the Res/Chunk explorer window. For the res file associated with your mesh (blocks and \_mesh for a mesh. blocks, clothwrapping, \_mesh, and eacloth for              cloth), select the file in the res explorer and click the `Export Res` button in the toolbar at the top of the window  

**DO NOT use Frosty Editor's default right-click > export function** or the res file cannot be re-imported automatically.

### Basic Mesh Importing
1. **Locate the mesh you want to replace in the main asset explorer**. Frosty Mesh Importer can modify meshes of type SkinnedMeshAsset, RigidMeshAsset, and ComponentMeshAsset.
2. **Export the mesh's res files** ([How To Export Res Files](#how-to-export-mesh-res-files)).
3. **Run FrostMeshy.**
4. **Click the `Import Mesh` button on the toolbar and navigate to the FrostMeshy output folder for your mesh**. Inside the folder, click the `Open` button in the open-file pop-up without selecting any of the files inside the folder. All the chunk files in the folder will be automatically imported into Frosty. The res files will be automatically imported if two conditions are satisfied: the res files were exporting using one of the two methods outlined in [How to Export Mesh Res Files](#how-to-export-mesh-res-files) and there's more than one mesh set in the FrostMeshy output folder. A message box will pop up if any res files need to be imported manually.
5. **Refresh your mesh tab** to see changes.

### Basic Mesh Reversion
The default Frosty Editor revert function is not designed to revert meshes that have been imported using FrostMeshy. The Frosty Mesh Importer `Revert Mesh` button in the toolbar restores a mesh to its default state by reverting all associated chunk and res files. Reverting a mesh using this method requires the output files from FrostMeshy because the names of the output files are used to locate the mesh's respective files in Frosty Editor.
1. **Click the `Revert Mesh` button on the toolbar**. Navigate to the FrostMeshy output folder associated with the mesh you want to revert. Inside the folder, click the `Open button in the open file pop-up without selecting any of the files inside the folder.
2. **Check the Frosty Editor log** to determine which files were successfully reverted and whether any need to be reverted manually.
3. **Refresh your mesh tab** to see changes.

## Mesh Import From Linked FrostMeshy Output

### Linking Frosty Mesh Importer to a FrostMeshy Output Folder
**Click the `Link Source` button and navigate to your FrostMeshy project output folder**. Select the output folder, not any of the mesh set folders inside it. This is the path that will be referenced upon clicking the `Source Import` button. Closing the application will clear the linked source path.

### Importing From a FrostMeshy Output Folder
The `Source Import` function directly references and pulls mesh sets from the path specified by `Link Source`. Any updates to the linked folder will be reflected in the `Source Import` dialog.
1. **Locate the mesh you want to replace in the main asset explorer**. Frosty Mesh Importer can modify meshes of type SkinnedMeshAsset, RigidMeshAsset, and ComponentMeshAsset.
2. **Export the mesh's res files** ([How To Export Res Files](#how-to-export-mesh-res-files)).
3. **Run FrostMeshy**.
4. **Link your FrostMeshy output folder to Frosty Editor** using the `Link Source` button, if not done so already.
5. **Click the `Source Import` button**. Select one or more mesh set and click `Import`.

A green check mark in the status column indicates that a mesh set's res files can be imported automatically. A red cross indicates that the mesh set's res files need to be exported in the current session before they can be automatically imported. A mesh set with a red status can be imported, but its res files will need to be imported manually.

## Re-Importing and Reverting From History
The `History` window allows you to manage all the mesh sets that have been imported during the current session, regardless of how they were imported (`Import Mesh` button, `Source Import` button, or `History` re-import) or whether they have been reverted. The `History` window can be used to re-import or revert multiple meshes. Checking the `Remove reverted meshes from list` check box will remove the meshes selected for reversion from the import history.

A green check mark in the status column indicates that a mesh set's res files can be imported automatically. A red cross indicates that the mesh set's res files need to be exported in the current session before they can be automatically imported. A mesh set with a red status can be imported, but its res files will need to be imported manually. Closing the application clears the import history.

## FrosTxt: Merging Localization Files

### FrosTxt Overview
A localization file is a chunk file which contains the game's UI text. There exists a localization file for every language supported by the game. Previously, it was only possible to view the UI text-edits of one mod because Frosty Mod Manager can only apply one edited localization file. FrosTxt is a tool packaged with Frosty Mesh Importer that merges discrete localization files into one file that can be applied by Frosty Mod Manager. It operates on localization chunk files which can be obtained from the author of a mod.  

A FrosTxt merge operation involves a base localization file (the default localization file corresponding to a specific language that will be replaced) and a list of modified localization files (the localization files whose differences from the base file will be merged together).

### Opening FrosTxt
Open the FrosTxt window by left-click selecting a localization asset in the main asset explorer (Found at `Localization > WSLocalization_<language>`) and then clicking the `Open FrosTxt` button on the FrosTxt toolbar ([Selecting a Toolkit](#selecting-a-toolkit)). Alternatively, right-click on a localization asset in the main asset explorer and select `Open FrosTxt`. Both of these methods open a FrosTxt window with the selected localization file as the base file. The base language file can be switched via the `Base language` drop-down selector at the top of the window.

### Merging Localization Files
1. **Select a base localization asset and open FrosTxt.**
2. **Import modified localization files to merge using the `Add`/`Remove` buttons.** The files are merged upwards in the order that they appear in the list, meaning files lower in the list take priority if there are conflicts. Use the `Move up` and `Move down` buttons to change a file's place in the merge order.
3. **Click the `Merge` button.** FrosTxt will merge the staged files and automatically modify the game files corresponding to the base language.
4. **Export the mod** and apply it as the last mod in Frosty Mod Manager. In the Frosty Mod Manager `Conflicts` tab, verify that the correct localization file is being applied.  

Closing the FrosTxt window saves all imported localization files with the current base file, meaning different sets of files can be imported for different languages and switching between them preserves their respective imported files regardless of whether they have been merged yet. At any point, the FrosTxt window `Save` button can be used to save a specific language's merged chunk file to disk.

### Reverting Localization Files
The default Frosty Editor right-click > revert function does not revert all files corresponding to a specific localization asset. Use of of the two following methods for reverting localization files merged by FrosTxt:
- Right-click on a localization asset in the main asset explorer and select `Revert FrosTxt`.
- Click `Revert FrosTxt` in the FrosTxt toolbar, prompting a window listing all localization assets that have been modified by FrosTxt. Select the localization file(s) to revert, and click `Revert`. 

## Troubleshooting Tips
* The majority of functions of this program operate using the Res/Chunk explorer. Executing an import/export operation without having a Res/Chunk explorer tab open will automatically open a new explorer tab which freezes the UI for a few seconds.
* This functions of this app rely on the file structure and naming conventions of the files output by FrostMeshy. Name your mesh set folders using unique identifiers that make it clear which asset each folder is associated with. Avoid moving or renaming mesh set input and output folders after exporting .res files from Frosty Editor and after running FrostMeshy.
* Chunk files should never fail to automatically import because their file names are unique identnifiers which can always be matched to a chunk file in the Frosty Editor. Res files are not named using unique identifiers, so imported .res files are identified using an ID value noted during export. These ID values are lost when the application is closed, meaning you can only automatically import .res files that you've exported in the same session.
* Read the Frosty Editor log. Some messages do not appear in pop-up message boxes while all operations by this app, successful or unsuccessful, are written to the log. Below is a table for troubleshooting different log messages.

Log Message | Explanation/Solution
----------- | -------------
ERROR: SelectedFileIsNotFolder | User has selected a file in the open file dialog, not a folder. Select a folder that contains the files you want to import/revert, not any of the files inside
ERROR: NonChunkResFileFound | User has selected a folder that contains files other than .chunk and .res files. Output folders from FrostMeshy will only contain .chunk and .res files
ERROR: NoResFileSelected | User has clicked "Export Res" button without selecting a file in the Res Explorer
ERROR: CannotOverwriteExistingFile | User has attempted to export a .res file to a folder which already contains a .res file of the same name. Delete old .res files before re-exporting them
ERROR: CriticalResFileError | The import could not be completed because no .res file in the Frosty Res Explorer matches the identifier of the .res file to be imported/reverted. Internal error, not related to any user actions
WARNING: NonCriticalResImportError | Denotes application state which is non-nominal but does not interrupt any core operations
WARNING: NonCriticalResImportError: MissingResID | Exported .res file identifier missing. Either the .res file was not exported using the "Export Res" button, or the .res file was exported in a different session, or only one mesh set is present in the FrostMeshy output folder. The file must be imported manually
WARNING: NonCriticalResImportError: UnableToRefreshExplorer | Internal error denoting communication failure with the Res Explorer UI element

___
Licensed under GPL v3.0
