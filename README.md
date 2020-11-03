<img src=https://github.com/kyle2277/bf2_Frosty_Res_Chunk_Importer/blob/master/FrostyResChunkImportIcon.png width="100" height="100"></img>
___
# TigerVenom22's Frosty Mesh Importer for SWBF2
*Formerly the Frosty Res/Chunk Importer*
## Overview
Running this app creates an instance of Frosty Editor and injects the following buttons to the toolbar: Import Mesh, Revert Mesh, Export Res, Link Source, Source Import, and History. This application streamlines the modding workflow by making importing and reverting one or multiple meshes in the Frosty Editor a one-click operation. It automates the handling of chunk and res files output by the mesh converter application, FrostMeshy.

64-bit Windows 10 build download (v1.2.0.3): http://www.mediafire.com/file/a4edtq6474bz0by/Frosty_Mesh_Importer_v1.2.0.3.zip/file \
64-bit Windows 10 build download for Frosty Editor Alpha (v1.2.0.3): http://www.mediafire.com/file/snulut7c1vsgzv3/Frosty_Alpha_Mesh_Importer_v1.2.0.3.zip/file

## Requirements
* Frosty Mod Editor v1.0.5.9 - [download link](https://frostytoolsuite.com/downloads.html)
* FrostMeshy v0.6.1.8 - [download link](https://www.mediafire.com/file/bmhr27uv2to2gmf/fmy-v0618-pre.zip/file)

## Documentation
### Installation
Install by placing `Frosty Mesh Importer.exe` in the same folder as your Frosty Editor installation.
### How to import a mesh using "Import Mesh" button
1. **Open the mesh you want to replace**. This prompts six new buttons to show up on the toolbar at the top of the window: Import Mesh, Revert Mesh, Export Res, Link Source, Souce Import, and History. These buttons will only be visible when an asset tab is open in Frosty Editor.
2. **Open the Res/Chunk Explorer** from the "Tools" drop-down menu. The Res/Chunk Explorer must be open to use any of this application's functions. Clicking on one of the buttons without having the Res/Chunk Explorer open will return an error in the log notifying you to open the Res/Chunk Explorer.
3. **Export the .res files using the "Export Res" button**. For each .res file associated with your mesh (blocks and \_mesh for a mesh. blocks, clothwrapping, \_mesh, and eacloth for  cloth) select the file in the res explorer and click the "Export Res" button in the toolbar at the top of the window. DO NOT use Frosty's default right-click > export function or the res file cannot be re-imported automatically.
4. **Run FrostMeshy**.
5. **Click the "Import Mesh" button on the toolbar and navigate to the FrostMeshy output folder for your mesh**. Inside the folder, click the "Open" button in the open-file pop-up without selecting any of the files inside the folder. All the .chunk files in the folder will be automatically imported into Frosty. The .res files will be automatically imported if two conditions are satisfied: the "Export Res" button was used to export the .res files and there's more than one mesh set in the FrostMeshy output folder. Check the Frosty Editor log to determine which files were successfully imported and whether you need to import any files manually.
6. **Refresh your mesh tab** to see changes.

### How to revert an imported mesh using "Revert Mesh" button
The default "revert" function in the Frosty Editor is not designed to revert meshes that have been imported using FrostMeshy. The "Revert Mesh" button restores a mesh by reverting all chunk and res files associated with said mesh. Reverting a mesh requires the output files from FrostMeshy because the names of the output files are used to locate the files to revert in Frosty Editor.
1. **With an asset tab and the Res/Chunk Explorer open, click the "Revert Mesh" button**. Navigate to the FrostMeshy output folder associated with the mesh you want to revert. Inside the folder, click the "Open" button in the open file pop-up without selecting any of the files inside the folder.
2. **Check the Frosty Editor log** to determine which files were successfully reverted and whether you need to revert any manually.
3. **Refresh your mesh tab** to see changes.

### Linking Frosty Mesh Importer to a FrostMeshy output folder
**Click the "Link Source" button and navigate to your FrostMeshy project output folder**. Select the output folder, not any of the mesh set folders inside it. This is the path that will be referenced upon clicking the "Source Import" button. Closing the application will clear the linked source path.

### Importing from a FrostMeshy output source folder
The "Source Import" function directly references and pulls mesh sets from the path specified by "Link Source." Any updates to the linked folder will be reflected in the "Source Import" dialog.
1. **Link your FrostMeshy output folder to Frosty Editor** using the "Link Source" button.
2. **With an asset tab and the Res/Chunk Explorer open, click the "Source Import" button**. Select one or more mesh set and click "Import."

A green check mark in the status column indicates that a meshset's .res files can be imported automatically. A red cross indicates that the mesh set's .res files need to be exported in the current session before they can be automatically imported. A mesh set with a red status can be imported, but its .res files will need to be imported manually.

### Re-importing and reverting via History
The "History" window allows you to manage all the mesh sets that have been imported during the current session, regardless of how they were imported ("Import Mesh" button, "Source Import" button, or "History" re-import) or whether they have been reverted. The "History" window can be used to re-import or revert multiple meshes. Selecting the "Remove reverted meshes from list" check box will remove the meshes selected for reversion from the import history.

A green check mark in the status column indicates that a meshset's .res files can be imported automatically. A red cross indicates that the mesh set's .res files need to be exported in the current session before they can be automatically imported. A mesh set with a red status can be imported, but its .res files will need to be imported manually. Closing the application clears the import history.

## Troubleshooting Tips
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
