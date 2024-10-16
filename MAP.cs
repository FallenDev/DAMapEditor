namespace Capricorn.Drawing;

/// <summary>
/// Map File Class
/// </summary>
public class MAPFile
{
    private int width;
    private int height;
    private MapTile[] tiles;
    private int id;
    private string name;

    /// <summary>
    /// Gets or sets the map tile at the specified coordinate.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>Map tile at the location specified.</returns>
    public MapTile this[int x, int y]
    {
        get { return tiles[y * width + x]; }
        set { tiles[y * width + x] = value; }
    }

    /// <summary>
    /// Gets or sets the map name.
    /// </summary>
    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    /// <summary>
    /// Gets or sets the map ID number.
    /// </summary>
    public int ID
    {
        get { return id; }
        set { id = value; }
    }

    /// <summary>
    /// Gets the map tiles within the map itself.
    /// </summary>
    public MapTile[] Tiles
    {
        get { return tiles; }
    }

    /// <summary>
    /// Gets or sets the height of the map, in tiles.
    /// </summary>
    public int Height
    {
        get { return height; }
        set { height = value; }
    }

    /// <summary>
    /// Gets or sets the width of the map, in tiles.
    /// </summary>
    public int Width
    {
        get { return width; }
        set { width = value; }
    }

    /// <summary>
    /// Loads a map file from disk.
    /// </summary>
    /// <param name="file">Map file to load.</param>
    /// <returns>Map file loaded.</returns>
    public static MAPFile FromFile(string file)
    {
        // Create File Stream
        FileStream stream = new FileStream(file,
            FileMode.Open, FileAccess.Read, FileShare.Read);

        // Load File
        MAPFile map = LoadMap(stream);

        // Get Map Id
        //map.id = Convert.ToInt32(Path.GetFileNameWithoutExtension(file).Remove(0, 3));

        stream.Close();

        // Return Map
        return map;
    }

    /// <summary>
    /// Loads a map file from disk.
    /// </summary>
    /// <param name="file">Map file to load.</param>
    /// <param name="width">Map width, in tiles.</param>
    /// <param name="height">Map height, in tiles.</param>
    /// <returns>Map file loaded.</returns>
    public static MAPFile FromFile(string file, int width, int height)
    {
        // Create File Stream
        FileStream stream = new FileStream(file,
            FileMode.Open, FileAccess.Read, FileShare.Read);

        // Load File
        MAPFile map = LoadMap(stream);
        map.width = width;
        map.height = height;

        // Get Map Id
        //map.id = Convert.ToInt32(Path.GetFileNameWithoutExtension(file).Remove(0, 3));

        // Return Map
        return map;
    }

    /// <summary>
    /// Loads a map file from raw data bytes.
    /// </summary>
    /// <param name="data">Data bytes to use.</param>
    /// <returns>Map file loaded.</returns>
    public static MAPFile FromRawData(byte[] data)
    {
        // Create Memory
        MemoryStream stream = new MemoryStream(data);

        // Load File
        MAPFile map = LoadMap(stream);

        // Return Map
        return map;
    }

    /// <summary>
    /// Loads a map file from raw data bytes.
    /// </summary>
    /// <param name="data">Data bytes to use.</param>
    /// <param name="width">Map width, in tiles.</param>
    /// <param name="height">Map height, in tiles.</param>
    /// <returns>Map file loaded.</returns>
    public static MAPFile FromRawData(byte[] data, int width, int height)
    {
        // Create Memory
        MemoryStream stream = new MemoryStream(data);

        // Load File
        MAPFile map = LoadMap(stream);
        map.width = width;
        map.height = height;

        // Return Map
        return map;
    }

    /// <summary>
    /// Internal function that loads a map file from a data stream.
    /// </summary>
    /// <param name="stream">Data stream.</param>
    /// <returns>Map file loaded.</returns>
    private static MAPFile LoadMap(Stream stream)
    {
        // Create Binary Reader
        stream.Seek(0, SeekOrigin.Begin);
        BinaryReader reader = new BinaryReader(stream);

        // Get Tile Count
        int tileCount = (int)(reader.BaseStream.Length / 6);

        // Create New Map File
        MAPFile map = new MAPFile();
        map.tiles = new MapTile[tileCount];

        // Get Tiles
        for (int i = 0; i < tileCount; i++)
        {
            ushort floor = reader.ReadUInt16();
            ushort leftWall = reader.ReadUInt16();
            ushort rightWall = reader.ReadUInt16();

            // Create Tile
            map.tiles[i] = new MapTile(floor, leftWall, rightWall);

        } reader.Close();
            
        // Return Map
        return map;
    }

    /// <summary>
    /// Gets the string representation of the object.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "{Name = " + name + ", ID = " + id.ToString() + ", Width = " +
               width.ToString() + ", Height = " + height.ToString() + "}";
    }
}

/// <summary>
/// Map Tile Class
/// </summary>
public class MapTile
{
    private ushort floor;
    private ushort leftWall;
    private ushort rightWall;

    /// <summary>
    /// Gets or sets the right wall tile.
    /// </summary>
    public ushort RightWall
    {
        get { return rightWall; }
        set { rightWall = value; }
    }
	
    /// <summary>
    /// Gets or sets the left wall tile.
    /// </summary>
    public ushort LeftWall
    {
        get { return leftWall; }
        set { leftWall = value; }
    }
	
    /// <summary>
    /// Gets or sets the floor tile.
    /// </summary>
    public ushort FloorTile
    {
        get { return floor; }
        set { floor = value; }
    }

    /// <summary>
    /// Creates a map tile with the specified tile values.
    /// </summary>
    /// <param name="floor">Floor tile.</param>
    /// <param name="leftWall">Left wall tile.</param>
    /// <param name="rightWall">Right wall tile.</param>
    public MapTile(ushort floor, ushort leftWall, ushort rightWall)
    {
        this.floor = floor;
        this.leftWall = leftWall;
        this.rightWall = rightWall;
    }

    /// <summary>
    /// Gets the string representation of the object.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "{Floor = " + floor.ToString() + ", Left Wall = " +
               leftWall.ToString() + ", Right Wall = " +
               rightWall.ToString() + "}";
    }
}