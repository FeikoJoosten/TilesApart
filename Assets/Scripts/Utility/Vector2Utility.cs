using UnityEngine;

public static class Vector2Extensions {

    private static Vector2? minus1;

    public static Vector2 Minus1 {
        get {
            if(!minus1.HasValue)
                minus1 = new Vector2(-1, -1);

            return minus1.Value;
        }
    }

    private static Vector2Int? minus1Int;

    public static Vector2Int Minus1Int {
        get {
            if(!minus1Int.HasValue)
                minus1Int = new Vector2Int(-1, -1);

            return minus1Int.Value;
        }
    }

    private static Vector2Int? zero;

    public static Vector2Int Zero {
        get {
            if(!zero.HasValue)
                zero = new Vector2Int(0, 0);

            return zero.Value;
        }
    }

    private static Vector2Int? left;
    public static Vector2Int Left {
        get {
            if (!left.HasValue)
                left = new Vector2Int(-1, 0);

            return left.Value;
        }
    }

    private static Vector2Int? right;
    public static Vector2Int Right {
        get {
            if(!right.HasValue)
                right = new Vector2Int(1, 0);

            return right.Value;
        }
    }

    private static Vector2Int? up;
    public static Vector2Int Up {
        get {
            if(!up.HasValue)
                up = new Vector2Int(0, 1);

            return up.Value;
        }
    }

    private static Vector2Int? down;
    public static Vector2Int Down {
        get {
            if(!down.HasValue)
                down = new Vector2Int(0, -1);

            return down.Value;
        }
    }
}
