using System.Collections.Specialized;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Transactions;

namespace Rabota;

class Player : Entity, IAnimated
{
    public AnimHandler Anim { get; }
    
    const float moveSpeed = 8.0f;
    const float hitboxShrink = (float)Tile.TileSize / 3f;
    const float velocityStopThreshold = 1f;

    Vector2 velocity = new Vector2();
    float frames_speedup = 5;
    float frames_slowdown = 12;
    float frames_animstart = 10;
    float frames_animstop = 12;
    float animTimer;
    bool animTimerStarted;
    bool isLeft = false;
    bool lastFrameIsLeft = false;

    public Player(Vector2 pos)
    {
        position = pos;

        Anim = new AnimHandler();
        Anim.SpriteOffset = new Vector2(0, -8);
        Anim.AddAnim(
            "idle", 
            "res/silky smooth.png", 
            new Vector2(Tile.TileSize), 
            [2,8],
            2
        );
        Anim.AddAnim(
            "run", 
            "res/silky smooth.png", 
            new Vector2(Tile.TileSize), 
            [0,1,2,3,4,5,6,7,8,9,10,11], 
            20
        );
        Anim.AddAnim(
            "halt", 
            "res/silky smooth.png", 
            new Vector2(Tile.TileSize), 
            [12],
            2
        );
        Anim.PlayAnim("idle");
    }

    public void Update(World curWorld)
    {
        Vector2 moveDir = GetMoveDir();
        if(moveDir != Vector2.Zero)
        {
            moveDir = Vector2.Normalize(moveDir);

            Vector2 targetVelocity = moveDir * moveSpeed * Tile.TileSize * Jarvis.GetFrameTime();
            velocity = Vector2.Lerp(velocity, targetVelocity, 1.0f/(float)frames_speedup);

            if(moveDir.X > 0)
                isLeft = false;
            else if(moveDir.X < 0)
                isLeft = true;
            
            if(animTimerStarted)
            {
                //Console.WriteLine($"Cur run speed: {Anim.GetDefaultFps() / (animTimer * 4 + 1)}");
                Anim.PlayAnim("run", flipX: isLeft, fps: Anim.GetDefaultFps() / (animTimer + 1));
            } 
            else
                Anim.PlayAnim("run", flipX: isLeft);

            if(!animTimerStarted && animTimer <= 0)
            {
                //Console.WriteLine("Starting");
                animTimer += frames_animstart;
                animTimerStarted = true;
            }
        }
        else
        {
            velocity = Vector2.Lerp(velocity, Vector2.Zero, 1.0f/(float)frames_slowdown);

            if(Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2)) <= velocityStopThreshold)
                velocity = Vector2.Zero;
            
            if(animTimerStarted && animTimer <= 0)
            {
                //Console.WriteLine("Stopping");
                animTimer += frames_animstop;
                animTimerStarted = false;
            }
        }

        if(velocity == Vector2.Zero)
            Anim.PlayAnim("idle", flipX: isLeft);

        if(!HandleCollision(curWorld, velocity))
        {
            position += velocity;

            if(velocity.X > 0)
                isLeft = false;
            else if(velocity.X < 0)
                isLeft = true;  

            if(animTimer > 0)
                animTimer--;
            lastFrameIsLeft = isLeft;
            return;
        }

        Vector2 velX = new Vector2(velocity.X, 0);
        if(!HandleCollision(curWorld, velX))
            position += velX;

        Vector2 velY = new Vector2(0, velocity.Y);
        if(!HandleCollision(curWorld, velY))
            position += velY;
        
        if(animTimer > 0)
            animTimer--;
        lastFrameIsLeft = isLeft;
    }

    Vector2 GetMoveDir()
    {
        Vector2 inputDir = Vector2.Zero;

        if(Jarvis.IsKeyDown(KeyboardKey.A))
            inputDir.X--;
        if(Jarvis.IsKeyDown(KeyboardKey.D))
            inputDir.X++;
        if(Jarvis.IsKeyDown(KeyboardKey.W))
            inputDir.Y--;
        if(Jarvis.IsKeyDown(KeyboardKey.S))
            inputDir.Y++;
        
        return inputDir;
    }

    bool HandleCollision(World curWorld, Vector2 velocity)
    {
        Vector2 targetPosition = position + velocity;
        Rectangle playerRect = new Rectangle(
            targetPosition.X + hitboxShrink, 
            targetPosition.Y + hitboxShrink,
            Tile.TileSize - 2 * hitboxShrink,
            Tile.TileSize - 2 * hitboxShrink
        );

        int minTileX = (int)MathF.Floor(targetPosition.X / Tile.TileSize);
        int minTileY = (int)MathF.Floor(targetPosition.Y / Tile.TileSize);
        int maxTileX = (int)MathF.Floor(targetPosition.X + Tile.TileSize / Tile.TileSize);
        int maxTileY = (int)MathF.Floor(targetPosition.Y + Tile.TileSize / Tile.TileSize);

        minTileX = Math.Max(0, minTileX);
        minTileY = Math.Max(0, minTileY);
        maxTileX = (int)Math.Min(curWorld.GetSize().X - 1, maxTileX);
        maxTileY = (int)Math.Min(curWorld.GetSize().Y - 1, maxTileY);

        for(int y = minTileY; y <= maxTileY; y++)
        {
            for(int x = minTileX; x <= maxTileX; x++)
            {
                Tile tile = curWorld.GetTile(new Vector2(x,y));

                if(tile.tileType != TileType.Wall)
                    continue;

                Rectangle tileRect = tile.GetHitbox(new Vector2(x, y));

                if(Jarvis.CheckCollisionRecs(tileRect, playerRect))
                    return true;
            }
        }

        return false;
    }

    public Rectangle GetHitbox()
    {
        return new Rectangle(position, new Vector2(Tile.TileSize));
    }

    public override void Draw()
    {
        //HitboxDraw();
        Anim.Draw(position);
    }
    void HitboxDraw()
    {
        Rectangle playerRect = new Rectangle(
            position.X + hitboxShrink, 
            position.Y + hitboxShrink,
            (Tile.TileSize - 2 * hitboxShrink),
            (Tile.TileSize - 2 * hitboxShrink)
        );
        Jarvis.DrawRectangleRec(playerRect, Color.Red);
    }
}