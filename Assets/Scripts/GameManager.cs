using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    Texture2D heightmap = null;

    // heightmap contains values between 0 and 1, we want greater height differences, so we scale the height values
    public int heightmapScaleFactor = 100;
    public float smallestRowPos = 0f;
    public float largestRowPos = 0f;
    public float smallestColPos = 0f;
    public float largestColPos = 0f;

    // how much we have to move the tiles, for the next position
    public float row_offset_x = -8.65f;
    public int col_offset_z = -10;

    // Start is called before the first frame update
    void Start()
    {
        // read heightmap image
        // https://answers.unity.com/questions/432655/loading-texture-file-from-pngjpg-file-on-disk.html
        byte[] fileData;
        if (File.Exists("Assets/Textures/Heightmap_16.png"))
        {
            fileData = File.ReadAllBytes("Assets/Textures/Heightmap_16.png");
            heightmap = new Texture2D(2, 2);
            heightmap.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            Debug.Log(heightmap);

            // iter over image width and height and instanciate tile accordinly
            for (int row_ind = 0; row_ind < heightmap.height; row_ind++)
            {
                if (row_ind > largestRowPos)
                {   
                    // keep these up to date so that they can be used as boundaries for camera script
                    largestRowPos = row_ind;
                }

                for (int col_ind = 0; col_ind < heightmap.width; col_ind++)
                {
                    if (col_ind > largestColPos)
                    {
                        largestColPos = col_ind;
                    }

                    // rgba color at each index, we use the maximun color value as height
                    UnityEngine.Color pixel_val = heightmap.GetPixel(row_ind, col_ind);
                    float intensity = pixel_val.maxColorComponent;
                    
                    string msg1 = "x: {0}, y: {1}, pixel: {2}";
                    Debug.LogFormat(string.Format(msg1, row_ind, col_ind, intensity));

                    UnityEngine.Vector3 pos_vec = offsetToPos(row_ind, col_ind, intensity * heightmapScaleFactor);
                    string msg2 = "row: {0}, col: {1}, height: {2}, pos_vec: {3}";
                    Debug.LogFormat(string.Format(msg2, row_ind, col_ind, intensity * heightmapScaleFactor, pos_vec));

                    Instantiate(getTileFromPixelVal(intensity), pos_vec,  Quaternion.identity);

                }
            }

            // scale boundaries from indices to correct game coordinates
            largestColPos = largestColPos * col_offset_z;
            largestRowPos = largestRowPos * row_offset_x;
            string msg3 = "Row range: [{0}, {1}], Col range: [{2}, {3}]";
            Debug.Log(string.Format(msg3, smallestRowPos, largestRowPos, smallestColPos, largestColPos));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector3 offsetToPos(int row_ind, int col_ind, float height)
    {
        // we need to convert the x,y indices from the heightmap to game coordinates



        float x_pos = row_offset_x * row_ind;
        float z_pos = col_offset_z * col_ind;

        if (row_ind % 2 == 1)
        {
            // if this is an odd row number, we need to shift the tile by half a tile width
            z_pos -= col_offset_z * 0.5f;
        }

        Vector3 pos_vec = new Vector3(x_pos, height, z_pos);

        return pos_vec;
    }

    GameObject getTileFromPixelVal(float pixelVal)
    {
        // return the gameobject to clone with instantiate, based on values given in moodle
        string tileName = "";
        if (pixelVal == 0)
        {
            tileName = "WaterTile";
        } else if (pixelVal > 0.0 && pixelVal <= 0.2)
        {
            tileName = "SandTile";
        } else if (pixelVal > 0.2 && pixelVal <= 0.4)
        {
            tileName = "GrassTile";
        } else if (pixelVal > 0.4 && pixelVal <= 0.6)
        {
            tileName = "ForestTile";
        } else if (pixelVal > 0.6 && pixelVal <= 0.8)
        {
            tileName = "StoneTile";
        } else
        {
            tileName = "MountainTile";
        }

        return GameObject.Find(tileName);
    }
}
