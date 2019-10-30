using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class TileMover : MonoBehaviour {
    // Used to keep track of the currently faded tiles
    private bool onlyDisplayLine = false;
    private Vector2Int lastFadeDirection = Vector2Extensions.Zero;
    private readonly List<Tile> crossNeighbours = new List<Tile>();

    /// <summary> Call startfade function for fading in </summary>
    private void ColorFadeIn(List<Tile> tiles) {
        StartFade(true, tiles);
    }

    /// <summary> Call startfade function for fading out </summary>
    private void ColorFadeOut(List<Tile> tiles) {
        StartFade(false, tiles);
    }

    /// <summary> Start fading in or out for all tiles in this list </summary>
    private void StartFade(bool fadeIn, List<Tile> tiles) {
        // Change colour of all tiles
        if (tiles == null) return;
        for (int i = 0, length = tiles.Count; i < length; i++) {
            // Only continue if there's actually a tile and mesh
            if (tiles[i] == null) continue;

            // Only continue if the tile index has colors set
            if (tiles[i].StartColorsLength == 0) continue;

            // Set the colors
            switch (tiles[i].tileType) {
                case TileType.TCrossing:
                    ChangeAllColors(GridManager.TileData.crossingTileLowlightColors, tiles[i], fadeIn, GridManager.TileData.CrossingTileLowlightColorsLength);
                    break;
                case TileType.Curve:
                    ChangeAllColors(GridManager.TileData.curveTileLowlightColors, tiles[i], fadeIn, GridManager.TileData.CurveTileLowlightColorsLength);
                    break;
                case TileType.Pathless:
                    ChangeAllColors(GridManager.TileData.pathlessTileLowlightColors, tiles[i], fadeIn, GridManager.TileData.PathlessTileLowlightColorsLength);
                    break;
                case TileType.Start:
                    if (GridManager.GridData.startEndTilesFading == false) break;
                    ChangeAllColors(GridManager.TileData.startTileLowlightColors, tiles[i], fadeIn, GridManager.TileData.StartTileLowlightColorsLength);
                    break;
                case TileType.End:
                    if (GridManager.GridData.startEndTilesFading == false) break;
                    ChangeAllColors(GridManager.TileData.endTileLowlightColors, tiles[i], fadeIn, GridManager.TileData.EndTileLowlightColorsLength);
                    break;
                case TileType.Straight:
                    ChangeAllColors(GridManager.TileData.straightTileLowlightColors, tiles[i], fadeIn, GridManager.TileData.StraightTileLowlightColorsLength);
                    break;
            }
        }
    }

    private void ChangeAllColors(Color[] darkColors, Tile tile, bool fadeIn, int darkColorsLength) {
        for (int i = 0, length = GridManager.TileData.MaterialColorNamesLength; i < length; i++) {
            if (i >= darkColorsLength) break;
            if (i >= tile.StartColorsLength) break;

            StartCoroutine(ChangeColor(tile, GridManager.TileData.materialColorNames[i], darkColors[i], tile.startColors[i], fadeIn));
        }
    }

    /// <summary> Change a color of a material from a to b </summary>
    private IEnumerator ChangeColor(Tile tile, string materialColorName, Color color1, Color color2, bool inverse = false) {
        // Abort if the color is non-existent
        if (tile.MeshRenderer.sharedMaterial.HasProperty(materialColorName) == false) yield break;
        tile.MeshRenderer.GetPropertyBlock(tile.PropertyBlock);

        // Prepare variables
        Color a = tile.PropertyBlock.GetColor(materialColorName);
        Color b = color2;

        // Inverse colors if needed
        if (inverse) {
            a = tile.PropertyBlock.GetColor(materialColorName);
            b = color1;
        }

        // Abort of the color is already correct
        if (tile.PropertyBlock.GetColor(materialColorName) == b) yield break;

        float dragLength = (Input.mousePosition - startScreenPosition).magnitude;

        dragLength = Mathf.Clamp(dragLength, GridManager.GridData.gradientMinLength, GridManager.GridData.gradientMaxLength);

        // When fading back to normal color
        if (inverse == false) {
            float inversionTime = dragLength / GridManager.GridData.gradientMaxLength;

            tile.PropertyBlock.SetColor(materialColorName, b);
            tile.MeshRenderer.SetPropertyBlock(tile.PropertyBlock);

            while (inversionTime > 0) {
                inversionTime -= Time.unscaledDeltaTime;

                tile.PropertyBlock.SetColor(materialColorName, Color.Lerp(b, a, inversionTime));
                tile.MeshRenderer.SetPropertyBlock(tile.PropertyBlock);

                yield return null;
            }

            tile.PropertyBlock.SetColor(materialColorName, b);
        }
        // When fading with gradient to dark
        else {
            // Contains check is to make sure it still needs to be faded
            while (isDragging && crossNeighbours.Contains(tile)) {
                dragLength = (Input.mousePosition - startScreenPosition).magnitude;

                dragLength = Mathf.Clamp(dragLength, GridManager.GridData.gradientMinLength, GridManager.GridData.gradientMaxLength);

                // Manual lerp to make sure it doesn't blink to the next gradient step too quickly
                tile.PropertyBlock.SetColor(materialColorName, Color.Lerp(a, b, dragLength / GridManager.GridData.gradientMaxLength));

                tile.MeshRenderer.SetPropertyBlock(tile.PropertyBlock);

                yield return null;
            }

            tile.PropertyBlock.SetColor(materialColorName, b);
        }

        tile.MeshRenderer.SetPropertyBlock(tile.PropertyBlock);
    }

    /// <summary> Check which tiles need to be faded out and fade them </summary>
    private void UpdateNeighbours(Vector3 inputDirection) {
        // Lock to axis
        Vector2Int direction = Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.z) ? Vector2Extensions.Up : Vector2Extensions.Right;

        // Only when direction changed
        if (lastFadeDirection == direction) return;

        // If the direction changed while not going through center
        if (lastFadeDirection != Vector2Extensions.Zero) {
            // UpdateNeighboursBack(inputDirection);
        }

        List<Tile> cross = GetCrossNeighbours(selectedTile, true);

        if (cross == null) return;

        // Find tiles that need to be lowlighted
        //for (int i = 0; i < cross.Count; i++) {
        //    Vector2 offset = (cross[i].tileIndex - selectedTile.tileIndex);
        //    offset = (offset / offset.magnitude);
        //    offset = new Vector2(Mathf.Abs(offset.x), Mathf.Abs(offset.y));
        //    if (offset == direction) {

        //        neighbours.Add(cross[i]);
        //    }
        //}

        lastFadeDirection = direction;

        ColorFadeIn(crossNeighbours);
        onlyDisplayLine = true;
    }

    /// <summary> Check which tiles don't need to be faded out and unfade them </summary>
    private void UpdateNeighboursBack(Vector3 inputDirection) {
        List<Tile> cross = GetCrossNeighbours(selectedTile);
        for (int i = 0, length = cross.Count; i < length; i++) {
            crossNeighbours.Remove(cross[i]);
        }

        ColorFadeOut(cross);
        onlyDisplayLine = false;

        // Reset so you can change it again
        lastFadeDirection = Vector2Extensions.Zero;
    }

    /// <summary> Get all tiles in the same row or column of the player </summary>
    public List<Tile> GetCrossNeighbours(Tile center, bool inverse = false) {
        crossNeighbours.Clear();

        if (center.IsMovableTile() == false) {
            return crossNeighbours;
        }

        Vector2Int centerIndex = center.tileIndex;
        crossNeighbours.Add(center);
        // Add row
        Vector2Int indexToCheck = center.GetNextTileIndex(centerIndex, Vector2Extensions.Right);
        while (indexToCheck != centerIndex) {
            // Only register if it actually has a material
            if (GridManager.GetTileAtIndex(indexToCheck) != null) {
                if (GridManager.GetTileAtIndex(indexToCheck).tileType != TileType.Border)
                    crossNeighbours.Add(GridManager.GetTileAtIndex(indexToCheck));
            }

            indexToCheck = center.GetNextTileIndex(indexToCheck, Vector2Extensions.Right);
        }

        indexToCheck = center.GetNextTileIndex(centerIndex, Vector2Extensions.Up);

        // Add column
        while (indexToCheck != centerIndex) {
            // Only register if it actually has a material
            if (GridManager.GetTileAtIndex(indexToCheck) != null) {
                if (GridManager.GetTileAtIndex(indexToCheck).tileType != TileType.Border)
                    crossNeighbours.Add(GridManager.GetTileAtIndex(indexToCheck));
            }
            indexToCheck = center.GetNextTileIndex(indexToCheck, Vector2Extensions.Up);
        }

        return crossNeighbours;
    }
}