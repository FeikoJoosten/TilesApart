using UnityEngine;

public partial class Tile : MonoBehaviour {

    public void UpdateObjectName() {
        gameObject.name = "Row: " + tileIndex.x + " Column: " + tileIndex.y;
    }

    private Vector2Int GetTileGroupStart(Vector2Int startTile, Vector2Int movementDir) {
        // Inverse movement direction to check backwards
        Vector2Int inverseDir = movementDir * -1;

        // Start from first tile
        Vector2Int groupStart = startTile;

        // Loop backwards
        while (true) {
            Vector2Int next = groupStart + inverseDir;

            Tile nextTile = TileOwner.GetTileAtIndex(next);

            // Stop when you reach an empty tile or border
            if (nextTile == null || next == startTile) {
                return groupStart;
            }
            if (nextTile.IsMovableTile() == false) {
                return groupStart;
            }
            // Otherwise continue looping
            else {
                groupStart = next;
            }

        }
    }

    private bool IsValidMove(Vector2Int startIndex, Vector2Int movementDirection) {
        if (TileOwner == null) {
            return false;
        }

        if (attributeTile != null) {
            return attributeTile.MoveValidate();
        }

        Vector2Int currentIndex = Vector2Extensions.Minus1Int;
        int stepCount = 0;

        while (currentIndex != startIndex) {
            currentIndex = currentIndex == Vector2Extensions.Minus1Int ? GetNextTileIndex(startIndex, movementDirection, true) : GetNextTileIndex(currentIndex, movementDirection, true);

            Tile tileAtIndex = TileOwner.GetTileAtIndex(currentIndex);

            if (tileAtIndex == null) {
                stepCount++;
                continue;
            }

            if (tileAtIndex.IsMovableTile()) {
                stepCount++;
            }
        }

        return stepCount > 1;
    }

    public Vector2Int GetNextTileIndex(Vector2Int currentIndex, Vector2Int movementDirection, bool overwriteMovable = false) {
        Vector2Int newIndex = currentIndex;

        if (currentIndex.x <= TileOwner.GridSize.x - 1 && currentIndex.x + movementDirection.x > TileOwner.GridSize.x - 1) {
            newIndex.x = 0;
            newIndex.y = currentIndex.y;
        } else if (currentIndex.x >= 0 && currentIndex.x + movementDirection.x < 0) {
            newIndex.x = TileOwner.GridSize.x - 1;
            newIndex.y = currentIndex.y;
        } else if (currentIndex.y <= TileOwner.GridSize.y - 1 && currentIndex.y + movementDirection.y > TileOwner.GridSize.y - 1) {
            newIndex.x = currentIndex.x;
            newIndex.y = 0;
        } else if (currentIndex.y >= 0 && currentIndex.y + movementDirection.y < 0) {
            newIndex.x = currentIndex.x;
            newIndex.y = TileOwner.GridSize.y - 1;
        } else {
            newIndex += movementDirection;
        }

        if (overwriteMovable == true) {
            return newIndex;
        }

        if (TileOwner.GetTileAtIndex(newIndex) != null) {
            if (TileOwner.GetTileAtIndex(newIndex).IsMovableTile() == false) {
                return GetNextTileIndex(newIndex, movementDirection);
            }
        }

        return newIndex;
    }

    public bool WillTeleport(Vector2Int currentIndex, Vector2Int movementDirection) {
        Vector2Int nextIndex = GetNextTileIndex(currentIndex, movementDirection);

        if (movementDirection.x > 0 && nextIndex.x < currentIndex.x) {
            return true;
        } else if (movementDirection.x < 0 && nextIndex.x > currentIndex.x) {
            return true;
        } else if (movementDirection.y > 0 && nextIndex.y < currentIndex.y) {
            return true;
        } else if (movementDirection.y < 0 && nextIndex.y > currentIndex.y) {
            return true;
        }

        return false;
    }

    public bool IsMovableTile() {
        switch (tileType) {
            case TileType.Start:
            case TileType.End:
            case TileType.Border:
                return false;
            default:
                return true;
        }
    }

    public bool[] GetDirections() {
        return directions;
    }

    private void FixTileColor() {
        if (PropertyBlock == null) return;

        Vector3 localRot = transform.localRotation.eulerAngles;

        MeshRenderer.GetPropertyBlock(PropertyBlock);

        Color color1 = PropertyBlock.GetColor(tileColor1ShaderName);
        Color color3 = PropertyBlock.GetColor(tileColor3ShaderName);

        switch (tileType) {
            case TileType.Curve:
            case TileType.Pathless:
                if (localRot.y != 0 && localRot.y != 180) return;

                PropertyBlock.SetColor(tileColor3ShaderName, color1);
                PropertyBlock.SetColor(tileColor1ShaderName, color3);
                break;
            case TileType.Straight:
            case TileType.TCrossing:
                if (localRot.y != 90 && localRot.y != 270) return;

                PropertyBlock.SetColor(tileColor3ShaderName, color1);
                PropertyBlock.SetColor(tileColor1ShaderName, color3);
                break;
        }

        MeshRenderer.SetPropertyBlock(PropertyBlock);
    }
}