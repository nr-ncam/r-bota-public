using System.Numerics;

namespace Rabota;

interface IAnimated
{
    public AnimHandler Anim { get; }
}

class AnimHandler
{
    public Vector2 SpriteOffset = new Vector2(0, 0);
    Dictionary<String, Animation> anims = new Dictionary<string, Animation>();
    Animation curAnim;

    public AnimHandler()
    {
        
    }

    public void Draw(Vector2 position)
    {
        curAnim.DrawFrame(position + SpriteOffset);
    }

    public void PlayAnim(
        String animName, 
        float fps = -1, 
        bool loop = true, 
        bool flipX = false,
        int startFrame = 0
    )
    {
        if(!anims.ContainsKey(animName))
        {
            Console.WriteLine($"Animation {animName} doesn't exist");
            return;
        }

        if(curAnim != null)
        {
            if(curAnim.Name == animName)
                if(curAnim.FlipX == flipX)
                    if(curAnim.Fps == fps)
                        return;
        }

        curAnim = anims[animName];

        if(fps == -1)
            curAnim.Fps = curAnim.DefaultFps;
        else
            curAnim.Fps = fps;

        curAnim.IsLooping = loop;
        curAnim.FlipX = flipX;

        if(curAnim.IsPlaying)
            return;
        curAnim.CurFrame = startFrame;
        curAnim.IsPlaying = true;
    }

    public void PlayAnim()
    {
        if(curAnim == null) return;

        curAnim.IsPlaying = true;
    }

    public void StopAnim()
    {
        curAnim.IsPlaying = false;
    }

    public void AddAnim(String animName, String path, Vector2 frameSize, List<int> frameIndicies, float baseFps)
    {
        if(!Path.Exists(path))
        {
            Console.WriteLine($"{path} doesn't exist");
            return;
        }

        Texture2D spritesheet = Jarvis.LoadTexture(path);
        Animation newAnim = new Animation(spritesheet, frameSize, frameIndicies, baseFps);
        newAnim.Name = animName;
        newAnim.DefaultFps = baseFps;

        anims.Add(animName, newAnim);
    }

    public float GetDefaultFps()
    {
        return curAnim.DefaultFps;
    }
    public float GetDefaultFps(String animName)
    {
        if(!anims.ContainsKey(animName))
        {
            Console.WriteLine($"Animation {animName} doesn't exist");
            return -1;
        }

        return anims[animName].DefaultFps;
    }
}

class Animation
{
    public String Name = "";
    public int CurFrame = 0;
    public bool IsLooping = true;
    public bool FlipX = false;
    public bool IsPlaying = false;
    public float Fps = 24;
    public float DefaultFps = 24;

    Texture2D spritesheet;
    Vector2 frameSize;
    List<int> frameIndicies;

    float sinceLastFrame = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spritesheet">Only *horizontal* spritesheets are supported so far!!!</param>
    /// <param name="frameSize"></param>
    /// <param name="frameIndicies"></param>
    public Animation(Texture2D spritesheet, Vector2 frameSize, List<int> frameIndicies, float baseFps)
    {
        this.spritesheet = spritesheet;
        this.frameSize = frameSize;
        this.frameIndicies = frameIndicies;
        this.DefaultFps = baseFps;
    }

    public void DrawFrame(Vector2 position)
    {
        Rectangle src = new Rectangle(new Vector2(frameSize.X * frameIndicies[CurFrame], 0), frameSize);
        Rectangle dest = new Rectangle(position, frameSize);

        if(FlipX)
        {
            src.X++;
            src.Width = -src.Width;
        }

        Jarvis.DrawTexturePro(spritesheet, src, dest, Vector2.Zero, 0, Color.White);

        sinceLastFrame += Jarvis.GetFrameTime();

        if(sinceLastFrame < 1/Fps)
            return;

        //why doesnt this work
        if(!IsPlaying) return;

        if(!MoveFrame(1))
            IsPlaying = false;
    }

    //BIGGGGG hacks there!1
    bool MoveFrame(int delta)
    {
        if(!IsPlaying)
            return true;
        
        CurFrame += delta;
        sinceLastFrame = 0;

        if(!IsLooping && (CurFrame < 0 || CurFrame >= frameIndicies.Count))
        {
            CurFrame -= delta;
            return false;
        }

        if(CurFrame < 0)
            CurFrame = frameIndicies.Count - 1;
        
        if(CurFrame >= frameIndicies.Count)
            CurFrame = 0;
        
        return true;
    }
}