using System.Numerics;
using System.Security.Cryptography;

namespace Rabota;

class Drunkard
{
    public float TurnChance;
    public int worldWidth, worldHeight = 0;
    public int Steps = 0;

    Vector2 position;
    Vector2 prevDir;

    public Drunkard(Vector2 spawn)
    {
        position = spawn;
    }

    public void Step(Random rng)
    {
        bool changeDir = rng.NextInt64(100) > TurnChance;
        int randomDir = rng.Next(0,4);
        Vector2 moveDir = Vector2.Zero;

        if(!changeDir)
        {
            switch(randomDir)
            {
                case 0:
                    moveDir.X++;
                    break;
                case 1:
                    moveDir.Y++;
                    break;
                case 2:
                    moveDir.X--;
                    break;
                case 3:
                    moveDir.Y--;
                    break;
            }
        }
        else
        {
            moveDir = prevDir;
        }

        position += moveDir;

        position.X = Math.Clamp(position.X, 1, worldWidth - 2);
        position.Y = Math.Clamp(position.Y, 1, worldHeight - 2);

        prevDir = moveDir;

        Steps--;
    }

    public Vector2 GetPosition()
    {
        return position;
    }
}