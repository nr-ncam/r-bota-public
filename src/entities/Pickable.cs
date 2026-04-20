using System.Numerics;
using Raylib_cs;

namespace Rabota;

class Pickable : Entity, IInteractable
{
    public Rectangle Hitbox => new Rectangle(position * Tile.TileSize, new Vector2(Tile.TileSize));

    public Pickable(Vector2 pos)
    {
        position = pos;
    }

    public override void Draw()
    {
        Jarvis.DrawRectangleV(position * Tile.TileSize, new Vector2(Tile.TileSize, Tile.TileSize), Color.Blue);
    }

    public void OnPlayerTouch(Player player)
    {
        Console.WriteLine("yamate kudasai player san");
        Game.pick_collected++;

        if(Game.pick_collected == Game.pick_total)
            Console.WriteLine("Alr quit NOW");

        IsActive = false;
    }
}
