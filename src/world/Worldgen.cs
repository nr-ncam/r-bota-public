using System.Numerics;
using System.Text.Json;

namespace Rabota;

class Worldgen
{
    World curWorld;
    int seed;

    WorldConfig worldConfig = new WorldConfig();

    int worldWidth;
    int worldHeight;
    
    float playerSpawnChance = 30;

    const int pickableMaxAttempts = 5;
    const int pickableFallbackCandidates = 5;
    int pickableAmount;
    int pickableMinDistance = 9;

    List<Drunkard> drunkards = new List<Drunkard>();
    int drunkardAmount;
    int drunkardSteps;
    /// <summary>
    /// 0-100%. idgaffff
    /// </summary>
    float drunkardTurnChance;
    float drunkardRoomChance;

    bool isChaos = false;

    Random rng;

    public void Config(String _worldType, int _seed)
    {
        seed = _seed;

        worldConfig = WorldConfig.FetchConfig(_worldType);

        if(worldConfig.IsChaos)
            worldConfig.DrunkTurnChance = Random.Shared.Next(100);

        playerSpawnChance = 1f / worldConfig.DrunkAmount * 100f;

        Game.pick_collected = 0;
        Game.pick_total = worldConfig.PickableAmount;

        curWorld = new World();
        curWorld.Seed = _seed;
        curWorld.WorldType = _worldType;
    }

    /// <summary>
    /// Note: by default creates a Default world. Config() must be set beforehand
    /// </summary>
    public World Generate()
    {
        curWorld.Width = worldConfig.Width;
        curWorld.Height = worldConfig.Height;

        //Step 0: create rng
        rng = new Random(seed);

        //Step 1: all walls (step one say youll get it donee)
        for(int y = 0; y < worldConfig.Height; y++)
        {
            for(int x = 0; x < worldConfig.Width; x++)
            {
                Tile newTile = new Tile();
                newTile.tileType = TileType.Wall;
                curWorld.Tiles.Add(newTile);
            }
        }

        //Step 2: spawn drunkards (and have them roam)
        Vector2 drunkardSpawn = Vector2.One;
        drunkardSpawn.X = rng.Next(1, curWorld.Width - 1);
        drunkardSpawn.Y = rng.Next(1, curWorld.Height - 1);

        for(int i = 0; i < worldConfig.DrunkAmount; i++)
        {
            Drunkard newDrunkard = new Drunkard(drunkardSpawn);
            newDrunkard.worldWidth = worldConfig.Width;
            newDrunkard.worldHeight = worldConfig.Height;
            newDrunkard.TurnChance = worldConfig.DrunkTurnChance;
            newDrunkard.Steps = worldConfig.DrunkSteps;
            drunkards.Add(newDrunkard);
        }

        UpdateGeneration();

        SetTileGraphics();        

        return curWorld;
    }

    void SetFloor(Vector2 position, bool isRoom = false)
    {
        if(worldConfig.IsChaos && !isRoom)
        {
            if(curWorld.GetTile(position).tileType == TileType.Floor)
                curWorld.GetTile(position).tileType = TileType.Wall;
            else 
                curWorld.GetTile(position).tileType = TileType.Floor;
        }
        else
        {
            curWorld.GetTile(position).tileType = TileType.Floor;
        }
    }

    void CarveRoom(Vector2 position)
    {
        for(int y = -1; y <= 1; y++)
        {
            for(int x = -1; x <= 1; x++)
            {
                Vector2 carvePos = new Vector2(position.X + x, position.Y + y);

                carvePos.X = Math.Clamp(carvePos.X, 1, worldConfig.Width - 2);
                carvePos.Y = Math.Clamp(carvePos.Y, 1, worldConfig.Height - 2);

                SetFloor(carvePos, true);
            }
        }

        if(curWorld.player == null && rng.Next(100) < playerSpawnChance)
            SpawnPlayer(position);
        else
            playerSpawnChance += playerSpawnChance / 4; //jankyyy
    }

    void SpawnPlayer(Vector2 position)
    {
        Spawnpoint spawn = new Spawnpoint(position);
        curWorld.Entities.Add(spawn);
        curWorld.player = new Player(position * Tile.TileSize);
    }

    public void UpdateGeneration()
    {
        //I LOVE BS FIXESSS
        if(drunkards.Count <= 0)
            return;
        
        int stepsPerFrame = 1;

        Console.WriteLine("Get ts done dont just stand there");
        stepsPerFrame = worldConfig.DrunkSteps;

        for(int i = 0; i < stepsPerFrame; i++)
        {
            foreach (Drunkard newDrunkard in drunkards)
            {
                //Console.WriteLine("Stepping..");

                SetFloor(newDrunkard.GetPosition());

                if(rng.Next(100) < worldConfig.DrunkRoomChance)
                    CarveRoom(newDrunkard.GetPosition());

                newDrunkard.Step(rng);

                if(newDrunkard.Steps <= 0)
                    curWorld.Ready = true;

                //Console.WriteLine($"Stepped to {newDrunkard.GetPosition().X}, {newDrunkard.GetPosition().Y}");
            }
        }

        if(curWorld.Ready) {
            drunkards.Clear();
            PlacePickables();
        }
    }

    void PlacePickables()
    {
        Console.WriteLine("Let's place em");
        List<Vector2> floorTiles = new List<Vector2>();

        //Step 0: Adding the floor tiles
        for(int y = 0; y < worldConfig.Height; y++)
        {
            for(int x = 0; x < worldConfig.Width; x++)
            {
                if(curWorld.GetTile(new Vector2(x, y)).tileType == TileType.Floor)
                    floorTiles.Add(new Vector2(x,y));
            }
        }

        //Step 1: Spawn first pickable completely randomly
        Pickable firstPickable = new Pickable(floorTiles[rng.Next(0, floorTiles.Count)]);
        curWorld.Entities.Add(firstPickable);

        //Step 2: Spawn the rest. Scary algorithm
        for(int pickIndex = 1; pickIndex < worldConfig.PickableAmount; pickIndex++)
        {
            Vector2 randomPos;
            bool valid = false;
            int attempts = 0;

            while(!valid && attempts < pickableMaxAttempts)
            {
                randomPos = floorTiles[rng.Next(0, floorTiles.Count)];
                if(DistanceCheck(randomPos))
                {
                    valid = true;
                    curWorld.Entities.Add(new Pickable(randomPos));
                }
                attempts++;
            }

            if(!valid)
            {
                Console.WriteLine("Cant find a far enough point; using fallback");

                Vector2 bestPos = Vector2.Zero;
                int bestMinDist = -1;

                for(int i = 0; i < pickableFallbackCandidates; i++)
                {
                    Vector2 candidate = floorTiles[rng.Next(0, floorTiles.Count)];
                    int minDist = int.MaxValue;

                    foreach(Pickable p in curWorld.Entities.OfType<Pickable>())
                    {
                        if(VectorDistance(candidate, p.position) < minDist)
                            minDist = VectorDistance(candidate, p.position);
                    }

                    if(minDist > bestMinDist)
                    {
                        bestMinDist = minDist;
                        bestPos = candidate;
                    }
                }

                curWorld.Entities.Add(new Pickable(bestPos));
            }
        }
    }

    bool DistanceCheck(Vector2 pos)
    {
        foreach (Entity e in curWorld.Entities)
        {
            if(e is Pickable placedPickable)
            {
                int distance = VectorDistance(placedPickable.position, pos);

                if(distance < pickableMinDistance) 
                    return false;

                Console.WriteLine(distance);
            }
        }

        return true;
    }

    int VectorDistance(Vector2 a, Vector2 b)
    {
        float distX = (a.X - b.X);
        float distY = (a.Y - b.Y);
        double hypotenuse = Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));

        return (int)(hypotenuse);
    }

    void SetTileGraphics()
    {
        //First pass: The base
        for(int y = 0; y < worldConfig.Height; y++)
            for(int x = 0; x < worldConfig.Width; x++)
            {
                Tile curTile = curWorld.GetTile(new Vector2(x, y));

                if(curTile.tileType == TileType.Floor)
                    continue;
                
                Vector2 curPos = new Vector2(x,y);
                
                curTile.tileDir = GetNeighborTiles(curPos) switch
                {
                    0 => TileDir.Block,
                    1 => TileDir.VWallU,
                    10 => TileDir.VWallD,
                    11 => TileDir.VWallMID,
                    100 => TileDir.HWallL,
                    101 => TileDir.UL,
                    110 => TileDir.DL,
                    111 => TileDir.ML,
                    1000 => TileDir.HWallR,
                    1001 => TileDir.UR,
                    1010 => TileDir.DR,
                    1011 => TileDir.MR,
                    1100 => TileDir.HWallMID,
                    1101 => TileDir.UMID,
                    1110 => TileDir.DMID,
                    1111 => TileDir.MMID,
                };
            }
        
        //Second pass: Add missing lines
        for(int y = 0; y < worldConfig.Height; y++)
        {
            for(int x = 0; x < worldConfig.Width; x++)
            {
                Tile curTile = curWorld.GetTile(new Vector2(x,y));
                Tile upperNeigh = new Tile();

                List<TileDir> excludeTiles = new List<TileDir>();

                switch(curTile.tileDir)
                {
                    case TileDir.ML | TileDir.DL:
                        upperNeigh = curWorld.GetTile(new Vector2(x, y-1));

                        excludeTiles = [
                            TileDir.ML, TileDir.MR, TileDir.UL, TileDir.UR
                        ];
                        if (excludeTiles.Contains(upperNeigh.tileDir))
                            continue;

                        upperNeigh.Lines.Add(new Vector4(0.5f, 12, 0.5f, 24));
                        break;
                    case TileDir.MR | TileDir.DR:
                        upperNeigh = curWorld.GetTile(new Vector2(x, y-1));

                        excludeTiles = [
                            TileDir.ML, TileDir.MR, TileDir.UL, TileDir.UR
                        ];
                        if (excludeTiles.Contains(upperNeigh.tileDir))
                            continue;

                        upperNeigh.Lines.Add(new Vector4(23.5f, 12, 23.5f, 24));
                        break;
                    case TileDir.VWallD:
                        upperNeigh = curWorld.GetTile(new Vector2(x, y-1));
                        upperNeigh.Lines.Add(new Vector4(0.5f, 12, 0.5f, 24));
                        upperNeigh.Lines.Add(new Vector4(23.5f, 12, 23.5f, 24));
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Returns an array, formatted like [LEFT, RIGHT, UP, DOWN]
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    int GetNeighborTiles(Vector2 position)
    {
        List<int> retVal = new List<int>();

        for(int i = 0; i < 4; i++)
            retVal.Insert(i, 0);

        if(position.X < 1)
            retVal[0] = 1;
        if(position.X >= worldConfig.Width - 1)
            retVal[1] = 1;
        if(position.Y < 1)
            retVal[2] = 1;
        if(position.Y >= worldConfig.Height - 1)
            retVal[3] = 1;
        
        int multiplier = 1000;
        int code = 0;
        for(int i = 0; i < 4; i++)
        {
            if(retVal[i] != 0)
                continue;
            
            //ох гойда
            Tile checkTile = i switch
            {
                0 => curWorld.GetTile(new Vector2(position.X - 1, position.Y + 0)),
                1 => curWorld.GetTile(new Vector2(position.X + 1, position.Y + 0)),
                2 => curWorld.GetTile(new Vector2(position.X - 0, position.Y - 1)),
                3 => curWorld.GetTile(new Vector2(position.X - 0, position.Y + 1)),
            };

            if(checkTile.tileType == TileType.Wall) 
                retVal[i] = 1;
        }

        for(int i = 0; i < 4; i++)
        {
            if(retVal[i] == 1)
                code += multiplier;
            
            multiplier /= 10;
        }

        return code;
    }
}