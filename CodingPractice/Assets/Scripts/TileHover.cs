using UnityEngine;
using TMPro;

public class TileHover : MonoBehaviour
{
    public TMP_Text tileInfoText;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            TileInfo tileInfo = hit.collider.GetComponent<TileInfo>();
            if (tileInfo != null)
            {
                tileInfoText.text = $"Tile Position: {tileInfo.x}, {tileInfo.z}";
            }
        }
    }
}
