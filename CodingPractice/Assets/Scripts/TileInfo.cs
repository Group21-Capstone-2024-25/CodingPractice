using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public int x, z;

    public void SetCoordinates(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
}
