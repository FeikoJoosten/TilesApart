# Tools created specifically for Tiles Apart

During the development process of Tiles Apart multiple tools were created to make it easier to create levels and to automate processes.
This extends from custom Unity editor property windows, to quality of life functionality. If something could be automated or improved a tool is most likely available for you.

* [Custom Editor windows](#custom-editor-windows)
  * [The Grid Editor](#grid-editor)
  * [The Tile Editor](#tile-editor)

## Custom editor windows <a name="custom-editor-windows"/>

Multiple editor windows were created, where the biggest two would be the `Grid Editor` and the `Tile Editor`.

### The Grid Editor <a name="grid-editor"/>

The Grid Editor has a custom rendering implementation for a quick overview of the tiles available in the scene.  
![Screenshot of the Grid Editor window](Images/GridEditorWindow.png)

You can easily resize the grid to your desired size and the Grid Editor will automatically populate or remove tiles as required.  
![Gif showcasing the creation of new tiles when adjusting the Grid Size](Images/GridEditorGridResizing.gif)

You can easily reskin every single tile in your scene to match the style assigned in the `Default Tile Data`  
![Gif showcasing the tile reskinning](Images/ThemeSwapping.gif)

The Grid Editor will automatically detect the start and end tiles. It'll also spawn in the player pawn whenever the start tile has been assigned.

### The Tile Editor <a name="tile-editor"/>

Clicking on a tile using the Scene View or the Hierarchy window will bring up the Tile Editor.  
![Screenshot of the Tile Editor window](Images/TileEditorWindow.png)

The Tile Editor has a dropdown which allows you to define the type of the tile. Changing the tile will result in the tile changing its model and settings.
![Video showcasing the different tile types](Images/TileTypeSwapping.gif)

The Tile Editor has buttons for moving and rotating the tile across the grid.
![Video showcasing moving and rotating of tiles](Images/TileMovingAndRotating.gif)
