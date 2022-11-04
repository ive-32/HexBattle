using UnityEngine;
using System.Collections.Generic;

namespace IcwField
{
    class IcwHexTile
    {
        static public int Distance(Vector3Int cube1, Vector3Int cube2)
        {
            Vector3Int tileHex = cube1 - cube2;
            return Mathf.Max(Mathf.Abs(tileHex.x), Mathf.Abs(tileHex.y), Mathf.Abs(tileHex.z));
        }

        static public Vector2Int CubeToOrtho(Vector3Int cube)
        {
            int col = cube.x + (cube.y - (cube.y & 1)) / 2;
            int row = cube.y;
            return new Vector2Int(col, row);
        }

        static public Vector3Int OrthoToCube(Vector2Int tile)
        {
            int q = tile.x - (tile.y - (tile.y & 1)) / 2;
            int r = tile.y;
            int s = -q - r;
            return new Vector3Int(q, r, s);
        }

        static public Vector2Int RoundCubeToOrhto(Vector3 cube)
        {
            Vector3Int intV3 = Vector3Int.RoundToInt(cube);

            float q_diff = Mathf.Abs(intV3.x - cube.x);
            float r_diff = Mathf.Abs(intV3.y - cube.y);
            float s_diff = Mathf.Abs(intV3.z - cube.z);
            if (q_diff > r_diff && q_diff > s_diff)
            {
                intV3.x = -intV3.y - intV3.z;
            }
            else
            {
                if (r_diff > s_diff)
                    intV3.y = -intV3.x - intV3.z;
                else
                    intV3.z = -intV3.x - intV3.y;
            }
            return CubeToOrtho(intV3);
        }
        static public List<Vector2Int> GetRing(Vector2Int pos, int radius)
        {
            // возвращает кольцо 
            List<Vector2Int> ring = new List<Vector2Int>();
            Vector3Int tile = IcwHexTile.OrthoToCube(pos);
            for (int i = 0; i < radius; i++)
            {                   
                ring.Add(IcwHexTile.CubeToOrtho(new Vector3Int(-i, radius, -radius + i) + tile));
                ring.Add(IcwHexTile.CubeToOrtho(new Vector3Int(i, -radius, radius - i) + tile));
                ring.Add(IcwHexTile.CubeToOrtho(new Vector3Int(-radius + i, -i, radius) + tile));
                ring.Add(IcwHexTile.CubeToOrtho(new Vector3Int(radius - i, i, -radius) + tile));
                ring.Add(IcwHexTile.CubeToOrtho(new Vector3Int(radius, -radius + i, -i) + tile));
                ring.Add(IcwHexTile.CubeToOrtho(new Vector3Int(-radius, radius - i, i) + tile));
            }
            return ring;
        }
    }
}
