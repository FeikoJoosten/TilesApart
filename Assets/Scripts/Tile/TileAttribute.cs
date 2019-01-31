public enum Attributes {
    None,
    Immobile,
    Reverse
}

public abstract class TileAttribute {
    protected Tile ownerTile = null;

    public abstract bool MoveValidate();

    protected bool OwnerTileSet() {
        if (ownerTile)
            return true;

        return false;
    }
}

public class TileAttributeImmobile : TileAttribute {
    public TileAttributeImmobile(Tile owner) {
        ownerTile = owner;
    }

    public override bool MoveValidate() {
        return false;
    }
}