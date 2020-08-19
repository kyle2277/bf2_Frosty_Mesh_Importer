<img src=https://github.com/kyle2277/bf2_Frosty_Res_Chunk_Importer/blob/Dev/FrostyResChunkImportIcon.png width="100" height="100"></img>
___
# SWBFII Frosty Editor Res/Chunk Importer
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
1. Open the mesh you want to replace. This prompts three new buttons to show up on the toolbar at the top of the window: Import Mesh, Revert Mesh, and Export Res. These buttons will only be visible when an asset tab is open in Frosty.
2. Open the Res/Chunk Explorer tab from the "Tools" dropdown menu. The Res/Chunk Explorer must be open to use any of this app's functions. Clicking on one of the buttons without having the Res/Chunk Explorer open will return an error in the log notifying you to open the Res/Chunk Explorer.
3. Export the .res files using the "Export Res" button. For each .res file associated with your mesh (blocks and \_mesh for a mesh. blocks, clothwrappiing, \_mesh, and eacloth for  cloth) select the file in the res explorer and click the "Export Res" button in the toolbar at the top of the window. DO NOT use Frosty's default right-click > export function or the res file will not be able to be re-imported automatically.
4. Run FrostMeshy.
5. Click the "Import Mesh" button on the toolbar and navigate to the FrostMeshy output folder for your mesh. Inside the folder, click the "Open" button in the file dialog without selecting any of the files inside the folder. All the .chunk files in the folder will be automatically imported into Frosty. The .res files will be automatically imported depending on two conditions, whether you used the "Export Res" button to export your files and whether you conoverted more than one mesh set in Frost Meshy. Both conditions must be true for the .res files to be imported automatically. Check the Frosty Editor log to determmine which files were succesfully imported and whether you need to import any files manually.
6. Close and re-open your mesh tab to see changes.

### How to revert an imported mesh
The default "revert" function in the Frosty Editor is not designed to revert meshes that have been imported using Frost Meshy. The "Revert Mesh" button restores a mesh by reverting all chunk and res files associated with said mesh.
1. With 
