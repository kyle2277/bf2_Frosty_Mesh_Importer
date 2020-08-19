<img src=https://github.com/kyle2277/bf2_Frosty_Res_Chunk_Importer/blob/Dev/FrostyResChunkImportIcon.png width="100" height="100"></img>
___
# TigerVenom22's Frosty Res/Chunk Importer for SWBFII
## Overview
Running this app creates an instance of Frosty Editor and injects the following functions: Import Mesh, Revert Mesh, and Export Res. This application streamlines the modding workflow by making importing and reverting meshes in the Frosty Editor a one-click operation. It automates the handling of chunk and res files outputted by the mesh converter application, FrostMeshy.

64-bit Windows 10 build download: 

## Requirements
* Frosty Mod Editor v1.0.5.9 - [download link](https://frostytoolsuite.com/downloads.html)
* FrostMeshy v0.6.1.8 - [download link](https://www.mediafire.com/file/bmhr27uv2to2gmf/fmy-v0618-pre.zip/file)

## Documentation
### Installation
Install by placing Frosty_Res_Chunk_Importer.exe in the same folder as your Frosty Editor installation.
### How to import a mesh
1. Open the mesh you want to replace. This prompts three new buttons to show up on the toolbar at the top of the window: Import Mesh, Revert Mesh, and Export Res. These buttons will only be visible when an asset tab is open in Frosty Editor.
2. Open the Res/Chunk Explorer from the "Tools" dropdown menu. The Res/Chunk Explorer must be open to use any of this app's functions. Clicking on one of the buttons without having the Res/Chunk Explorer open will return an error in the log notifying you to open the Res/Chunk Explorer.
3. Export the .res files using the "Export Res" button. For each .res file associated with your mesh (blocks and \_mesh for a mesh. blocks, clothwrappiing, \_mesh, and eacloth for  cloth) select the file in the res explorer and click the "Export Res" button in the toolbar at the top of the window. DO NOT use Frosty's default right-click > export function or the res file will not be able to be re-imported automatically.
4. Run Frost Meshy.
5. Click the "Import Mesh" button on the toolbar and navigate to the FrostMeshy output folder for your mesh. Inside the folder, click the "Open" button in the file dialog without selecting any of the files inside the folder. All the .chunk files in the folder will be automatically imported into Frosty. The .res files will be automatically imported if two conditions are satisfied: the "Export Res" button was used to export the .res files and there's more than one mesh set in the Frost Meshy output folder. Check the Frosty Editor log to determmine which files were succesfully imported and whether you need to import any files manually.
6. Refresh your mesh tab to see changes.

### How to revert an imported mesh
The default "revert" function in the Frosty Editor is not designed to revert meshes that have been imported using Frost Meshy. The "Revert Mesh" button restores a mesh by reverting all chunk and res files associated with said mesh. Reverting a mesh requires the output files from Frost Meshy because the names of the output files are used to locate the files to revert in Frosty Editor.
1. With an asset tab and the Res/Chunk Explorer open, click the "Revert Mesh" button. Navigate to the Frost Meshy output folder associated with the mesh you want to revert. Inside the folder, click the "Open" button in the file dialog without selecting any of the files inside the folder.
2. Check the Frosty Editor log to determmine which files were successfully reverted and whether you need to revert any manyally.
3. Refresh your mesh tab to see changes.

## Tips
* This functions of this app rely on the file structure and naming conventions of the files outputted by Frost Meshy. Name your mesh set folders using unique identifiers that make it clear which asset each folder is associated with. Avoid moving or renaming mesh set input and output folders after exporting .res files from Frosty Editor and after running Frost Meshy.
* Chunk files should never fail to automatically import because their file names are unique identnifiers which can always be matched to a chunk file in the Frosty Editor. Res files are not named using unique identifiers, so imported .res files are identified using an ID value noted during export. These ID values are lost when the application is exited, meaning you can only automatically import .res files that you've exported in the same session.
* Read the Frosty Editor log. All operations by this app, successful or unsuccessful, are written to the log. The only indication that an operation has failed will be in the log. Below is a table explaining each possible log message.

Log Message | Explanation
----------- | -------------
ERROR: SelectedFileIsNotFolder | User has selected a file in the open file dialog, not a folder. Select a folder that contains the files you want to import/revert, not any of the files inside
ERROR: NonChunkResFileFound | User has selected a folder that contains files other than .chunk and .res files. Output folders from Frost Meshy will only contain .chunk and .res files
ERROR: NoResFileSelected | User has clicked "Export Res" button without selecting a file in the Res Explorer
ERROR: CannotOverwriteExistingFile | User has attempted to export a .res to a folder which already contains a .res file of the same name. Delete old .res files before re-exporting them.
ERROR: ResFileNotFound | The import could not be completed because no .res file in the Frosty Res Explorer matches the identifier of the .res file to be imported/reverted
WARNING: NonCriticalResImportError | Denotes application state which is non-nominal but does not interrupt any core operations
WARNING: NonCriticalResImportError: MissingResID | Exported .res file identifier missing. Either the .res file was not exported using the "Export Res" button, or the .res file was exported in a different session, or only one mesh is present in the Frost Meshy output folder. The file must be imported manually
WARNING: NonCriticalResImportError: UnableToRefreshExplorer | Internal error denoting communication failure with the Res Explorer UI element
