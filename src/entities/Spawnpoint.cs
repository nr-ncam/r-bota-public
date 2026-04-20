using System.Numerics;

namespace Rabota;

class Spawnpoint : Entity, IInteractable
{
    public Rectangle Hitbox => new Rectangle(position * Tile.TileSize, new Vector2(Tile.TileSize));

    public Spawnpoint(Vector2 pos)
    {
        position = pos;
    }

    public override void Draw()
    {
        Jarvis.DrawRectangleV(position * Tile.TileSize, new Vector2(Tile.TileSize), Color.DarkBrown);;
    }

    public void OnPlayerTouch(Player player)
    {
        if(Game.pick_collected != Game.pick_total)
            return;
        
        Program.ChangeState(new Menu());
    }
}