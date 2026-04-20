using System.Numerics;

namespace Rabota;

enum TileBiome
{
    City
}

enum TileType
{
    Floor,
    Wall
}

enum TileDir
{
    UL,
    UMID,
    UR,
    Block,
    ML,
    MMID,
    MR,
    VWallU,
    DL,
    DMID,
    DR,
    VWallMID,
    HWallL,
    HWallMID,
    HWallR,
    VWallD,
}

class Tile
{
    public const int TileSize = 24;

    public TileType tileType;
    public TileBiome tileBiome;
    public TileDir tileDir = TileDir.Block; //technically only applied to walls

    public List<Vector4> Lines = new List<Vector4>();

    public Vector2 DirToTile()
    {
        Vector2 retVal = tileDir switch
        {
            TileDir.UL          => new Vector2(0,0),
            TileDir.UMID        => new Vector2(1,0),
            TileDir.UR          => new Vector2(2,0),
            TileDir.Block       => new Vector2(3,0),

            TileDir.ML          => new Vector2(0,1),
            TileDir.MMID        => new Vector2(1,1),
            TileDir.MR          => new Vector2(2,1),
            TileDir.VWallU      => new Vector2(3,1),

            TileDir.DL          => new Vector2(0,2),
            TileDir.DMID        => new Vector2(1,2),
            TileDir.DR          => new Vector2(2,2),
            TileDir.VWallMID    => new Vector2(3,2),

            TileDir.HWallL      => new Vector2(0,3),
            TileDir.HWallMID    => new Vector2(1,3),
            TileDir.HWallR      => new Vector2(2,3),
            TileDir.VWallD      => new Vector2(3,3),
        };
        return retVal;
    }

    public Rectangle GetHitbox(Vector2 position)
    {
        return new Rectangle(position * TileSize, new Vector2(TileSize));
    }
}