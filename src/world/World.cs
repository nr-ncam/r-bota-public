using System.Numerics;
using System.Runtime.CompilerServices;

namespace Rabota;

enum WorldType
{
    Default,
    Straight,
    Chaos,
}

class World
{
    public List<Tile> Tiles = new List<Tile>();
    public List<Entity> Entities = new List<Entity>();
    public Player player;

    public int Seed;
    public string WorldType;

    float miniTileSize = Tile.TileSize;
    float miniDisplayTileSize = 1;

    public bool Ready = false;

    public int Width
    {
        get => _width;
        set
        {
            _width = value;
            miniTileSize = Jarvis.GetScreenHeight() / Height;
        }
    }
    int _width = 1;
    
    public int Height
    {
        get => _height;
        set
        {
            _height = value;
            miniTileSize = Math.Min(Jarvis.GetScreenWidth() / Width, Jarvis.GetScreenHeight() / Height);
        }
    }
    int _height = 1;

    Texture2D tileset;

    public World()
    {
        tileset = Jarvis.LoadTexture("res/tileset.png");
    }

    public Tile GetTile(Vector2 position)
    {
        return Tiles[((int)position.Y) * ((int)Width) + ((int)position.X)];
    }

    public Vector2 GetSize()
    {
        return new Vector2(Width, Height);
    }

    public void Update()
    {
        List<Entity> toRemove = new List<Entity>();

        foreach(Entity e in Entities)
        {
            if(e is IInteractable interactable)
            {
                if(Jarvis.CheckCollisionRecs(player.GetHitbox(), interactable.Hitbox))
                {
                    interactable.OnPlayerTouch(player);
                }
            }

            if(!e.IsActive)
                toRemove.Add(e);
        }

        foreach(Entity e in toRemove)
            Entities.Remove(e);
    }

    public void Draw()
    {
        DrawTiles();
        
        foreach(Entity e in Entities)
            e.Draw();
    }

    public void DrawMini()
    {
        if(miniDisplayTileSize < miniTileSize)
            miniDisplayTileSize = float.Lerp(miniDisplayTileSize, miniTileSize, 0.05f);

        for(int y = 0; y < Height; y++)
        {
            for(int x = 0; x < Width; x++)
            {
                Tile curTile = Tiles[((int)(y * Width + x))];

                if(curTile.tileType == TileType.Wall)
                {
                    Jarvis.DrawRectangleV(
                        new Vector2(x * miniTileSize, y * miniTileSize),
                        new Vector2(miniDisplayTileSize),
                        Color.White
                    );
                }
            }
        }

        foreach(Entity e in Entities)
        {
            if(e is Spawnpoint)
            {
                Vector2 textSize = Jarvis.MeasureTextEx(GameAssets.win95, "You", 18, 1);
                textSize.X /= 2;

                Jarvis.DrawTextEx(
                    GameAssets.win95, 
                    "You", 
                    e.position * miniTileSize - textSize,
                    18, 
                    1, 
                    Color.Red
                );
                Jarvis.DrawRectangleV(
                    new Vector2(e.position.X * miniTileSize, e.position.Y * miniTileSize),
                    new Vector2(miniDisplayTileSize),
                    Color.Red
                );
            }
            if(e is Pickable)
            {
                Jarvis.DrawRectangleV(
                    new Vector2(e.position.X * miniTileSize, e.position.Y * miniTileSize),
                    new Vector2(miniDisplayTileSize),
                    Color.Blue
                );
            }
        }
    }

    void DrawTiles()
    {
        for(int y = 0; y < Height; y++)
            for(int x = 0; x < Width; x++)
            {
                Tile curTile = Tiles[((int)(y * Width + x))];

                Jarvis.DrawRectangleV(new Vector2(x,y) * Tile.TileSize, new Vector2(Tile.TileSize), Color.DarkGray);

                if(curTile.tileType == TileType.Wall)
                {
                    Vector2 tilePos = new Vector2(x, y) * Tile.TileSize;
                    Rectangle srcRect = new Rectangle(curTile.DirToTile() * Tile.TileSize, new Vector2(Tile.TileSize));
                    Rectangle destRect = new Rectangle(tilePos, new Vector2(Tile.TileSize));

                    Jarvis.DrawTexturePro(tileset, srcRect, destRect, Vector2.Zero, 0.0f, Color.White);

                    foreach(Vector4 line in curTile.Lines)
                    {
                        Vector2 startPos = new Vector2(line.X, line.Y);
                        Vector2 endPos = new Vector2(line.Z, line.W);

                        Jarvis.DrawLineEx(tilePos + startPos, tilePos + endPos, 1f, Color.Black);
                    }
                }
            }
    }

    public void Unload()
    {
        Jarvis.UnloadTexture(tileset);
    }
}