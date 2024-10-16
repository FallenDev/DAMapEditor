using System.Drawing.Imaging;

using Capricorn.IO;

namespace Capricorn.Drawing;

/// <summary>
/// Image Type Enumeration
/// </summary>
public enum ImageType
{
    EPF,
    MPF,
    HPF,
    SPF,
    EFA,
    ZP,
    Tile
}

/// <summary>
/// Dark Ages Graphics Class
/// </summary>
public class DAGraphics
{
    public static int vscroll = 0;
    public static int hscroll = 0;
    /// <summary>
    /// Renders a HPF image to a standard bitmap image.
    /// </summary>
    /// <param name="hpf">HPF image to render.</param>
    /// <param name="palette">Palette of colors to use.</param>
    /// <returns>Bitmap of rendered image.</returns>
    public unsafe static Bitmap RenderImage(HPFImage hpf, Palette256 palette)
    {
        return SimpleRender(hpf.Width, hpf.Height, hpf.RawData, palette, ImageType.HPF);
    }

    /// <summary>
    /// Renders an EPF image to a standard bitmap image.
    /// </summary>
    /// <param name="epf">EPF frame image to render.</param>
    /// <param name="palette">Palette of colors to use.</param>
    /// <returns>Bitmap of rendered image.</returns>
    public unsafe static Bitmap RenderImage(EPFFrame epf, Palette256 palette)
    {
        return SimpleRender(epf.Width, epf.Height, epf.RawData, palette, ImageType.EPF);
    }

    /// <summary>
    /// Renders a MPF image to a standard bitmap image.
    /// </summary>
    /// <param name="mpf">MPF frame image to render.</param>
    /// <param name="palette">Palette of colors to use.</param>
    /// <returns>Bitmap of rendered image.</returns>
    public unsafe static Bitmap RenderImage(MPFFrame mpf, Palette256 palette)
    {
        return SimpleRender(mpf.Width, mpf.Height, mpf.RawData, palette, ImageType.MPF);
    }

    /// <summary>
    /// Renders a single tile to a standard bitmap image.
    /// </summary>
    /// <param name="tileData">Tile data to render.</param>
    /// <param name="palette">Palette of colors to use.</param>
    /// <returns>Bitmap of rendered tile image.</returns>
    public unsafe static Bitmap RenderTile(byte[] tileData, Palette256 palette)
    {
        return SimpleRender(Tileset.TileWidth, Tileset.TileHeight, tileData, palette, ImageType.Tile);
    }

    /// <summary>
    /// Internal function used to render images.
    /// </summary>
    /// <param name="width">Width of the image.</param>
    /// <param name="height">Height of the image.</param>
    /// <param name="data">Raw data bits of the image.</param>
    /// <param name="palette">Palette to use to render the image.</param>
    /// <param name="type">Image type to render.</param>
    /// <returns>Bitmap of the rendered image.</returns>
    private unsafe static Bitmap SimpleRender(int width, int height, byte[] data, Palette256 palette, ImageType type)
    {
        // Create Bitmap
        Bitmap image = new Bitmap(width, height);

        // Lock Bits
        BitmapData bmd = image.LockBits(
            new Rectangle(0, 0, image.Width, image.Height),
            ImageLockMode.WriteOnly,
            image.PixelFormat);

        // Render Image
        for (int y = 0; y < bmd.Height; y++)
        {
            byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);

            for (int x = 0; x < bmd.Width; x++)
            {
                #region Get Value from Raw Data
                int colorIndex = 0;
                if (type == ImageType.EPF)
                {
                    colorIndex = data[x * height + y];
                }
                else
                {
                    colorIndex = data[y * width + x];
                }
                #endregion

                if (colorIndex > 0)
                {
                    #region 32 Bit Render
                    if (bmd.PixelFormat == PixelFormat.Format32bppArgb)
                    {
                        row[x * 4] = palette[colorIndex].B;
                        row[x * 4 + 1] = palette[colorIndex].G;
                        row[x * 4 + 2] = palette[colorIndex].R;
                        row[x * 4 + 3] = palette[colorIndex].A;
                    }
                    #endregion

                    #region 24 Bit Render
                    else if (bmd.PixelFormat == PixelFormat.Format24bppRgb)
                    {
                        row[x * 3] = palette[colorIndex].B;
                        row[x * 3 + 1] = palette[colorIndex].G;
                        row[x * 3 + 2] = palette[colorIndex].R;
                    }
                    #endregion

                    #region 15 Bit Render
                    else if (bmd.PixelFormat == PixelFormat.Format16bppRgb555)
                    {
                        // Get 15-Bit Color
                        ushort colorWORD = (ushort)(((palette[colorIndex].R & 248) << 7) +
                                                    ((palette[colorIndex].G & 248) << 2) +
                                                    (palette[colorIndex].B >> 3));

                        row[x * 2] = (byte)(colorWORD % 256);
                        row[x * 2 + 1] = (byte)(colorWORD / 256);
                    }
                    #endregion

                    #region 16 Bit Render
                    else if (bmd.PixelFormat == PixelFormat.Format16bppRgb565)
                    {
                        // Get 16-Bit Color
                        ushort colorWORD = (ushort)(((palette[colorIndex].R & 248) << 8)
                                                    + ((palette[colorIndex].G & 252) << 3) +
                                                    (palette[colorIndex].B >> 3));

                        row[x * 2] = (byte)(colorWORD % 256);
                        row[x * 2 + 1] = (byte)(colorWORD / 256);
                    }
                    #endregion
                }
            }
        }

        // Unlock Bits
        image.UnlockBits(bmd);

        // Flip Image
        if (type == ImageType.EPF)
        {
            image.RotateFlip(RotateFlipType.Rotate90FlipX);
        }            

        // Return Bitmap
        return image;
    }

    public static Dictionary<int, Bitmap> cachedFloor = new Dictionary<int, Bitmap>();
    public static Dictionary<int, Bitmap> cachedWalls = new Dictionary<int, Bitmap>();
    public static Dictionary<int, Bitmap> cachedFloorLight = new Dictionary<int, Bitmap>();
    public static Dictionary<int, Bitmap> cachedWallsLight = new Dictionary<int, Bitmap>();

    // Change to EditParameters, RenderParameters
    public static int displaywidth = 560;
    public static int displayheight = 480;

    public static bool bgShow = true;
    public static bool fg1Show = true;
    public static bool fg2Show = true;

    public static int selectionXStart = -1;
    public static int selectionYStart = -1;
    public static int selectionXEnd = -1;
    public static int selectionYEnd = -1;

    // 012 bg, fg1, fg2; 3=all
    public static int editMode = 0;

    public static int clickmode = 0;

    public static int MouseHoverX = -1;
    public static int MouseHoverY = -1;

    // Stored selection block
    public static int selectionWidth;
    public static Capricorn.Drawing.MapTile[] selection;

    public static bool drawEmptyWalls = false;
    public static byte[] sotp;
    //public static bool[] transparent;
    public static bool tabMap = false;

    public static bool transparency = false;

    static int debug_renders = 0;
    static long debug_bgTime = 0;
    static long debug_fgTime = 0;
    static long debug_initTime = 0;
    static long debug_endTime = 0;
    static long debug_totalTime = 0;
    //private static Bitmap prevBitmap = null;
    /// <summary>
    /// Renders a map file, given the parameters.
    /// </summary>
    /// <param name="map">Map file to render.</param>
    /// <param name="tiles">Tileset to use.</param>
    /// <param name="tileTable">Tile palette table.</param>
    /// <param name="wallTable">Wall palette table.</param>
    /// <param name="wallSource">Wall source data archive.</param>
    /// <returns>Render map image.</returns>
    public static Bitmap RenderMap(MAPFile map, Tileset tiles,
        PaletteTable tileTable, PaletteTable wallTable,
        DATArchive wallSource)
    {
        long debug_initStart = DateTime.Now.Ticks;
        int additionalTop = 0;//256, 
        int additionalBottom = 0;// 96; 

        debug_renders++;

        int sxs = Capricorn.Drawing.DAGraphics.selectionXStart;
        int sys = Capricorn.Drawing.DAGraphics.selectionYStart;
        int sxe = Capricorn.Drawing.DAGraphics.selectionXEnd;
        int sye = Capricorn.Drawing.DAGraphics.selectionYEnd;
        // set selection
        int xMin = sxs < sxe ? sxs : sxe;
        int yMin = sys < sye ? sys : sye;
        int xMax = sxs > sxe ? sxs : sxe;
        int yMax = sys > sye ? sys : sye;

        Bitmap mapImage = new Bitmap(displaywidth, displayheight);
        //if (prevBitmap == null || prevBitmap.Width != displaywidth || prevBitmap.Height != displayheight)
        //mapImage = new Bitmap(displaywidth, displayheight);
        //else
        //{
        //mapImage = prevBitmap;
        //Graphics _g = Graphics.FromImage(mapImage);
        //_g.FillRectangle(new SolidBrush(Color.Black), 0, 0, mapImage.Width, mapImage.Height);

        //}

        //prevBitmap = mapImage;
        if (map == null)
            return mapImage;
        //Tileset.TileWidth * map.Width,
        //Tileset.TileHeight * (map.Height + 1) + additionalTop + additionalBottom);

        Graphics g = Graphics.FromImage(mapImage);

        #region Render Floor

        // Set Origins
        int xOrigin = ((mapImage.Width / 2) - 1) - Tileset.TileWidth / 2 + 1 - hscroll;
        int yOrigin = -vscroll;

        bool[,] collision = null;
        if (tabMap)
        {
            collision = new bool[map.Width, map.Height];
        }
        // Cached Tiles
        int xMul = Tileset.TileWidth / 2;
        int yxMultiplier = (Tileset.TileHeight + 1) / 2;

        long debug_initEnd = DateTime.Now.Ticks;
        debug_initTime += (debug_initEnd - debug_initStart);

        long debug_bgStart = DateTime.Now.Ticks;
        if (bgShow)
        {
            for (int y = 0; y < map.Height; y++)
            {
                //if (yOrigin <= displayheight && xOrigin + (map.Width - 1) * yxMultiplier >= 0)
                //{
                for (int x = 0; x < map.Width; x++)
                {
                    int xd = xOrigin + x * xMul;
                    int yd = yOrigin + x * yxMultiplier;
                    if (xd >= -Tileset.TileWidth && yd >= -Tileset.TileHeight && xd < displaywidth && yd < displayheight)
                    {
                        //if (xd - hscroll <= displaywidth && yd - vscroll <= displayheight && xd + Tileset.TileWidth >= 0 && yd + Tileset.TileHeight >= 0)
                        //{
                        // Get Floor Value
                        int floor = map[x, y].FloorTile;

                        bool showSelection = false;

                        if (editMode == 0 || editMode == 3)
                        {
                            if (clickmode == 0 && selection != null && MouseHoverX != -1)
                            {
                                int selectionHeight = selection.Length / selectionWidth;
                                if (x >= MouseHoverX && x < MouseHoverX + selectionWidth && y >= MouseHoverY && y < MouseHoverY + selectionHeight)
                                {
                                    int _x = x - MouseHoverX;
                                    int _y = y - MouseHoverY;
                                    floor = selection[_x + _y * selectionWidth].FloorTile;
                                    showSelection = true;
                                }
                            }
                        }
                        if (floor > 0)
                            floor -= 1;

                        // Cache the Tile if Not Cached Already
                        if (!cachedFloor.ContainsKey(floor))
                        {
                            if (floor >= 0 && floor < tiles.TileCount)
                            {
                                Bitmap floorTile = DAGraphics.RenderTile(tiles[floor], tileTable[floor + 2]);
                                cachedFloor.Add(floor, floorTile);
                            }
                        }

                        // Render Image
                        Bitmap thisTile = cachedFloor[floor];
                        if (editMode == 0 || editMode == 3)
                        {
                            if (showSelection || (selectionXStart != -1 && x >= xMin && x <= xMax && y >= yMin && y <= yMax))
                            {
                                if (!cachedFloorLight.ContainsKey(floor))
                                {
                                    if (floor >= 0 && floor < tiles.TileCount)
                                    {
                                        thisTile = DAGraphics.RenderTile(tiles[floor], tileTable[floor + 2]);
                                        for (int yy = 0; yy < thisTile.Height; yy++)
                                        {
                                            for (int xx = 0; xx < thisTile.Width; xx++)
                                            {
                                                Color c = thisTile.GetPixel(xx, yy);
                                                int colorR = c.R + 25;
                                                if (colorR > 255)
                                                    colorR = 255;
                                                int colorG = c.G + 25;
                                                if (colorG > 255)
                                                    colorG = 255;
                                                int colorB = c.B + 50;
                                                if (colorB > 255)
                                                    colorB = 255;
                                                if (c.R != 0 || c.G != 0 || c.B != 0)
                                                    thisTile.SetPixel(xx, yy, Color.FromArgb(colorR, colorG, colorB));
                                            }

                                        }
                                        cachedFloorLight.Add(floor, thisTile);
                                    }
                                }
                                else
                                {
                                    thisTile = cachedFloorLight[floor];
                                }
                            }
                        }
                        g.DrawImageUnscaled(thisTile,
                            xd,
                            yd);
                    }
                    //}
                }

                //}
                // Offset Origin
                xOrigin -= xMul;
                yOrigin += yxMultiplier;
            }
        }
        #endregion
        long debug_bgEnd = DateTime.Now.Ticks;
        debug_bgTime += (debug_bgEnd - debug_bgStart);

        long debug_fgStart = DateTime.Now.Ticks;
        #region Render Walls

        // Set Origins
        xOrigin = ((mapImage.Width / 2) - 1) - Tileset.TileWidth / 2 + 1 - hscroll;
        yOrigin = -vscroll;

        // Cached Tiles

        int yAdd = -
            150 + (Tileset.TileHeight + 1) / 2;
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                #region Render Left Wall
                int xd = xOrigin + x * xMul;
                int yd = yOrigin + (x + 1) * yxMultiplier + yAdd;

                // 200 to account for very high walls
                if (xd >= -Tileset.TileWidth && yd >= -200 && xd < displaywidth && yd < displayheight + 200)
                {
                    //if (xd - hscroll <= displaywidth && yd - vscroll <= displayheight && xd + Tileset.TileWidth >= 0 && yd + Tileset.TileHeight >= 0)
                    //{
                    // Get Left Wall Value
                    int leftWall = map[x, y].LeftWall;

                    if (fg1Show)
                    {
                        bool showSelection = false;

                        if (editMode == 1 || editMode == 3 || editMode == 4)
                        {
                            if (clickmode == 0 && selection != null && MouseHoverX != -1)
                            {
                                int selectionHeight = selection.Length / selectionWidth;
                                if (x >= MouseHoverX && x < MouseHoverX + selectionWidth && y >= MouseHoverY && y < MouseHoverY + selectionHeight)
                                {
                                    int _x = x - MouseHoverX;
                                    int _y = y - MouseHoverY;
                                    int _leftWall = selection[_x + _y * selectionWidth].LeftWall;
                                    if (_leftWall > 0 || drawEmptyWalls)
                                    {
                                        leftWall = _leftWall;
                                        showSelection = true;
                                    }
                                }
                            }
                        }
                        // filter out old ruc tiles
                        if (leftWall >= 13)
                        {
                            // Cache the HPF if Not Cached Already
                            if (!cachedWalls.ContainsKey(leftWall))
                            {
                                HPFImage hpf = HPFImage.FromArchive("stc" + leftWall.ToString().PadLeft(5, '0') + ".hpf", true, wallSource);
                                Bitmap wall = DAGraphics.RenderImage(hpf, wallTable[leftWall + 1]);
                                cachedWalls.Add(leftWall, wall);
                            }

                            Bitmap thisTile = cachedWalls[leftWall];
                            if (editMode == 1 || editMode == 3 || editMode == 4)
                            {
                                if (showSelection || (selectionXStart != -1 && x >= xMin && x <= xMax && y >= yMin && y <= yMax))
                                {
                                    if (!cachedWallsLight.ContainsKey(leftWall))
                                    {
                                        HPFImage hpf = HPFImage.FromArchive("stc" + leftWall.ToString().PadLeft(5, '0') + ".hpf", true, wallSource);
                                        thisTile = DAGraphics.RenderImage(hpf, wallTable[leftWall + 1]);
                                        for (int yy = 0; yy < thisTile.Height; yy++)
                                        {
                                            for (int xx = 0; xx < thisTile.Width; xx++)
                                            {
                                                Color c = thisTile.GetPixel(xx, yy);
                                                int colorR = c.R + 25;
                                                if (colorR > 255)
                                                    colorR = 255;
                                                int colorG = c.G + 25;
                                                if (colorG > 255)
                                                    colorG = 255;
                                                int colorB = c.B + 50;
                                                if (colorB > 255)
                                                    colorB = 255;
                                                if (c.R != 0 || c.G != 0 || c.B != 0)
                                                    thisTile.SetPixel(xx, yy, Color.FromArgb(colorR, colorG, colorB));
                                            }

                                        }
                                        cachedWallsLight.Add(leftWall, thisTile);
                                    }
                                    else
                                    {
                                        thisTile = cachedWallsLight[leftWall];
                                    }
                                }
                            }
                            // Render Image
                            if ((leftWall % 10000) > 1)
                            {
                                int xleft = xOrigin + x * xMul;
                                int yleft = yOrigin + (x + 1) * yxMultiplier -
                                            thisTile.Height +
                                            (Tileset.TileHeight + 1) / 2;

                                if (transparency && leftWall > 0 && leftWall - 1 < sotp.Length && (sotp[leftWall - 1] & 0x80) == 0x80)
                                {
                                    // gotta be a better way to do this - temporary
                                    for (int yy = 0; yy < thisTile.Height; yy++)
                                    {
                                        for (int xx = 0; xx < thisTile.Width; xx++)
                                        {
                                            int origX = xleft + xx;
                                            int origY = yleft + yy;
                                            if (origX >= 0 && origX < mapImage.Width && origY >= 0 && origY < mapImage.Height)
                                            {
                                                Color newC = thisTile.GetPixel(xx, yy);
                                                if (newC.R != 0 || newC.G != 0 || newC.B != 0)
                                                {
                                                    Color origC = mapImage.GetPixel(origX, origY);
                                                    int _r = origC.R + newC.R;
                                                    if (_r > 255)
                                                        _r = 255;
                                                    int _g = origC.G + newC.G;
                                                    if (_g > 255)
                                                        _g = 255;
                                                    int _b = origC.B + newC.B;
                                                    if (_b > 255)
                                                        _b = 255;
                                                    Color combined = Color.FromArgb(_r, _g, _b);
                                                    mapImage.SetPixel(xleft + xx, yleft + yy, combined);
                                                }
                                            }
                                        }
                                    }
                                    //                                ImageAttributes ia = new ImageAttributes();
                                    //                                float pos = 1.0f;
                                    //                                float neg = 0.5f;
                                    //                                ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix(new float[][]

                                    //{

                                    //    new float[] {pos, neg, neg, 1.0f, 0.0f}, // ?, ?, ?, ?, ?

                                    //    new float[] {neg, pos, neg, 1.0f, 0.0f}, // ?, ?, ?, ?, ?

                                    //    new float[] {neg, neg, pos, 1.0f, 0.0f}, // ?, ?, ?, ?, ?

                                    //    new float[] {0.0f, 0.0f, 0.0f, -0.0f, 110.0f}, // ?, ?, ?, brightness, ?

                                    //    new float[] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f} // ?, ?, ?, contrast?, ?

                                    //});
                                    //                                    //new ColorMatrix();
                                    //                                //cm.Matrix11 = 1.0f;
                                    //                                //cm.Matrix44 = 1.0f;
                                    //                                //cm.m
                                    //                                //cm.Matrix22 = 1.0f;
                                    //                                ia.SetColorMatrix(cm);
                                    //                                g.DrawImage(thisTile, new Rectangle(xleft, yleft, thisTile.Width, thisTile.Height),
                                    //                                    0, 0, thisTile.Width, thisTile.Height, GraphicsUnit.Pixel, ia);

                                    //                                //ImageAttributes ia2 = new ImageAttributes();
                                    //                                //ColorMatrix cm2 = new ColorMatrix();
                                    //                                //cm2.Matrix33 = 0.5f;
                                    //                                //cm.Matrix22 = 1.0f;
                                    //                                //ia2.SetColorMatrix(cm2);
                                    //                                //g.DrawImage(thisTile, new Rectangle(xleft, yleft, thisTile.Width, thisTile.Height),
                                    //                                    //0, 0, thisTile.Width, thisTile.Height, GraphicsUnit.Pixel, ia2);

                                    //                                //g.DrawImage(thisTile, destPara1, srcRect, GraphicsUnit.Pixel, ia);
                                }
                                else
                                {
                                    g.DrawImageUnscaled(thisTile,
                                        xleft,
                                        yleft);
                                }
                            }

                        }
                    }

                    if (tabMap && leftWall > 0 && leftWall - 1 < sotp.Length && sotp[leftWall - 1] == 0x0F)
                    {
                        collision[x, y] = true;
                    }
                    #endregion

                    #region Render Right Wall
                    // Get Right Wall Value
                    int rightWall = map[x, y].RightWall;

                    if (fg2Show)
                    {

                        bool showSelection = false;
                        if (editMode == 2 || editMode == 3 || editMode == 4)
                        {
                            if (clickmode == 0 && selection != null && MouseHoverX != -1)
                            {
                                int selectionHeight = selection.Length / selectionWidth;
                                if (x >= MouseHoverX && x < MouseHoverX + selectionWidth && y >= MouseHoverY && y < MouseHoverY + selectionHeight)
                                {
                                    int _x = x - MouseHoverX;
                                    int _y = y - MouseHoverY;
                                    int _rightWall = selection[_x + _y * selectionWidth].RightWall;
                                    if (_rightWall > 0 || drawEmptyWalls)
                                    {
                                        rightWall = _rightWall;
                                        showSelection = true;
                                    }
                                }
                            }
                        }
                        // filter out old ruc tiles
                        if (rightWall >= 13)
                        {

                            // Cache the HPF if Not Cached Already
                            if (!cachedWalls.ContainsKey(rightWall))
                            {
                                HPFImage hpf = HPFImage.FromArchive("stc" + rightWall.ToString().PadLeft(5, '0') + ".hpf", true, wallSource);
                                Bitmap wall = DAGraphics.RenderImage(hpf, wallTable[rightWall + 1]);
                                cachedWalls.Add(rightWall, wall);
                            }

                            Bitmap thisTile = cachedWalls[rightWall];
                            if (editMode == 2 || editMode == 3 || editMode == 4)
                            {
                                if (showSelection || (selectionXStart != -1 && x >= xMin && x <= xMax && y >= yMin && y <= yMax))
                                {
                                    if (!cachedWallsLight.ContainsKey(rightWall))
                                    {
                                        HPFImage hpf = HPFImage.FromArchive("stc" + rightWall.ToString().PadLeft(5, '0') + ".hpf", true, wallSource);
                                        thisTile = DAGraphics.RenderImage(hpf, wallTable[rightWall + 1]);
                                        for (int yy = 0; yy < thisTile.Height; yy++)
                                        {
                                            for (int xx = 0; xx < thisTile.Width; xx++)
                                            {
                                                Color c = thisTile.GetPixel(xx, yy);
                                                int colorR = c.R + 25;
                                                if (colorR > 255)
                                                    colorR = 255;
                                                int colorG = c.G + 25;
                                                if (colorG > 255)
                                                    colorG = 255;
                                                int colorB = c.B + 50;
                                                if (colorB > 255)
                                                    colorB = 255;
                                                if (c.R != 0 || c.G != 0 || c.B != 0)
                                                    thisTile.SetPixel(xx, yy, Color.FromArgb(colorR, colorG, colorB));
                                            }
                                        }
                                        cachedWallsLight.Add(rightWall, thisTile);
                                    }
                                    else
                                    {
                                        thisTile = cachedWallsLight[rightWall];
                                    }
                                }
                            }
                            // Render Image
                            if ((rightWall % 10000) > 1)
                            {
                                int xright = xOrigin + (x + 1) * xMul;
                                int yright = yOrigin + (x + 1) * yxMultiplier -
                                             thisTile.Height +
                                             (Tileset.TileHeight + 1) / 2;
                                //if (xd - hscroll <= 640 && yd - vscroll <= 480 && xd + Tileset.TileWidth >= 0 && yd + Tileset.TileHeight >= 0)

                                if (transparency && rightWall > 0 && rightWall - 1 < sotp.Length && (sotp[rightWall - 1] & 0x80) == 0x80)
                                {
                                    // gotta be a better way to do this - temporary
                                    for (int yy = 0; yy < thisTile.Height; yy++)
                                    {
                                        for (int xx = 0; xx < thisTile.Width; xx++)
                                        {
                                            int origX = xright + xx;
                                            int origY = yright + yy;
                                            if (origX >= 0 && origX < mapImage.Width && origY >= 0 && origY < mapImage.Height)
                                            {
                                                Color origC = mapImage.GetPixel(origX, origY);
                                                Color newC = thisTile.GetPixel(xx, yy);
                                                int _r = origC.R + newC.R;
                                                if (_r > 255)
                                                    _r = 255;
                                                int _g = origC.G + newC.G;
                                                if (_g > 255)
                                                    _g = 255;
                                                int _b = origC.B + newC.B;
                                                if (_b > 255)
                                                    _b = 255;
                                                Color combined = Color.FromArgb(_r, _g, _b);
                                                mapImage.SetPixel(xright + xx, yright + yy, combined);
                                            }
                                        }
                                    }
                                    //g.DrawImage(thisTile, destPara1, srcRect, GraphicsUnit.Pixel, ia);
                                }
                                else
                                {
                                    g.DrawImageUnscaled(thisTile,
                                        xright,
                                        yright);
                                }
                                
                            }
                        }
                    }
                    if (tabMap && rightWall > 0 && rightWall - 1 < sotp.Length && sotp[rightWall - 1] == 0x0F)
                    {
                        collision[x, y] = true;
                    }

                    // }
                    #endregion
                }

            }

            // Offset Origin
            xOrigin -= xMul;
            yOrigin += yxMultiplier;
        }
        #endregion
        long debug_fgEnd = DateTime.Now.Ticks;
        debug_fgTime += (debug_fgEnd - debug_fgStart);

        #region Draw Text
        /*
        SolidBrush brush = new SolidBrush(Color.White);
        g.DrawString(map.Name.ToUpper(),
            new Font("04b03b", 6.0f, FontStyle.Regular),
            brush,
            16, 16);

        g.DrawString("LOD" + map.ID.ToString() + ".MAP",
            new Font("04b03b", 6.0f, FontStyle.Regular),
            brush,
            16, 26);

        g.DrawString(map.Width.ToString() + "x" + map.Height.ToString() + " TILES",
            new Font("04b03b", 6.0f, FontStyle.Regular),
            brush,
            16, 36);
        brush.Dispose();
         * */
        #endregion

        if (tabMap)
        {
            xOrigin = ((mapImage.Width / 2) - 1) - Tileset.TileWidth / 2 + 1 - hscroll;
            yOrigin = -vscroll;

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (tabMap && collision[x, y])
                    {
                        int xDraw = xOrigin + x * Tileset.TileWidth / 2 + Tileset.TileWidth / 2;
                        int yDraw = yOrigin + x * (Tileset.TileHeight + 1) / 2;

                        PointF[] ps = new PointF[4];
                        ps[0] = new PointF(xDraw, yDraw);
                        ps[1] = new PointF(xDraw + Tileset.TileWidth / 2, yDraw + Tileset.TileHeight/2);
                        ps[2] = new PointF(xDraw, yDraw + Tileset.TileHeight);
                        ps[3] = new PointF(xDraw - Tileset.TileWidth / 2, yDraw + Tileset.TileHeight / 2);
                        g.FillPolygon((new SolidBrush(Color.FromArgb(128, 255, 255, 255))), ps);
                    }
                }

                xOrigin -= Tileset.TileWidth / 2;
                yOrigin += (Tileset.TileHeight + 1) / 2;

            }
        }

        //if (drawGridLines)
        //{
        //xOrigin = ((mapImage.Width / 2) - 1) - hscroll;
        //yOrigin = -vscroll;
        //for (int y = 0; y < map.Height; y++)
        //{
        //int xPos = xOrigin - y * (Tileset.TileWidth) / 2;
        //int yPos = (int)(yOrigin + y * ((Tileset.TileHeight + 1) / 2)) - 1;
        //int xEnd = xPos + (map.Width - 1) * (Tileset.TileWidth + 1) / 2;
        //int yEnd = (int)(yPos + (map.Height - 1) * (Tileset.TileHeight + 1.5) / 2);
        //g.DrawLine(new Pen(new SolidBrush(Color.Green)), xPos, yPos, xEnd, yEnd);
        //}
//
        //for (int x = 0; x < map.Width; x++)
        //{
        //int xPos = xOrigin + x * (Tileset.TileWidth) / 2;
        //int yPos = (int)(yOrigin + x * ((Tileset.TileHeight + 1) / 2)) - 1;
        //int xEnd = xPos - (map.Width - 1) * (Tileset.TileWidth + 1) / 2;
        //int yEnd = (int)(yPos + (map.Height - 1) * (Tileset.TileHeight + 1.5) / 2);
        //g.DrawLine(new Pen(new SolidBrush(Color.Green)), xPos, yPos, xEnd, yEnd);
        //}

        //}
        long debug_endStart = DateTime.Now.Ticks;
        g.Dispose();
        // ~~~ don't do EVERY render?
        System.GC.Collect();

        long debug_endEnd = DateTime.Now.Ticks;
        debug_endTime += (debug_endEnd - debug_endStart);
        debug_totalTime += (debug_endEnd - debug_initStart);

        if ((debug_renders % 100) == 99)
        {
            //MessageBox.Show("BG time avg = " + ((debug_bgTime / 10000) / (double)debug_renders) + " ms." +
            //"FG time avg = " + ((debug_fgTime / 10000) / (double)debug_renders) + " ms." +
            //"init time avg = " + ((debug_initTime / 10000) / (double)debug_renders) + " ms." +
            //"end time avg = " + ((debug_endTime / 10000) / (double)debug_renders) + " ms." +
            //"total time avg = " + ((debug_totalTime / 10000) / (double)debug_renders) + " ms.");
        }

        return mapImage;
    }
}