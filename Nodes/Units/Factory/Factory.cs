using Godot;
using System.Linq;

public static class Factory
{
    // = Enemy
    // == Unit
    // === Soldier
    public static Character CreateEnemySoldier(Player player, Vector2I position)
    {
        Character character = new Character(Type.Unit);
        Image image = Image.LoadFromFile("res://Texture/Units/Enemy/Soldier/MoscowSoldier.png");

        int[][] sizeInCells = new int[][]
        {
                         /* 1 */
            /*1*/new int[]{-1 }
        };

        CircleShape2D circleShape2D = new CircleShape2D();
        circleShape2D.Radius = 8;

        // Add abilities
        character.AddAbility(new Sprite(image));
        character.AddAbility(new AnimationAbility());
        character.AddAbility(new MoveAbility());
        character.AddAbility(new HealthAbility(50, false));
        character.AddAbility(new ExtractResource());
        character.AddAbility(new AttackAbility());
        character.AddAbility(new SelectArea());
        character.AddAbility(new Collision(1, 1, circleShape2D));
        character.AddAbility(new AI());

        // Set options
        character.PlayerOwner = player;
        character.Position = new
            (
                position.X < 0 ? position.X * 16 + 8 : position.X * 16 - 8,
                position.Y < 0 ? position.Y * 16 + 8 : position.Y * 16 - 8
            );

        return character;
    }

    // == Unit
    // === Worker
    public static Character CreateWorker()
    {
        Character character = new Character(Type.Unit);

        int[][] sizeInCells = new int[][]
        {
                         /* 1 */
            /*1*/new int[]{-1 }
        };

        CircleShape2D circleShape2D = new CircleShape2D();
        circleShape2D.Radius = 8;

        var shape = new CapsuleShape2D();
        shape.Height = 32;
        shape.Radius = 7;

        // Add abilities
        character.AddAbility(new Sprite());
        character.AddAbility(new AnimationAbility());
        character.AddAbility(new MoveAbility());
        character.AddAbility(new SelectArea(shape));
        character.AddAbility(new HealthAbility(50, false));
        character.AddAbility(new ExtractResource());
        character.AddAbility(new AttackAbility());
        character.AddAbility(new Collision(1,1, circleShape2D));
        character.AddAbility(new AI());

        // Set options
        character.PlayerOwner = null;

        return character;
    }

    public static Character CreateWorker(Player player, Vector2I position)
    {
        Character character = new Character(Type.Unit);

        // Add abilities
        character.AddAbility(new Sprite());
        character.AddAbility(new AnimationAbility());
        character.AddAbility(new MoveAbility());
        character.AddAbility(new HealthAbility(50, false));
        character.AddAbility(new ExtractResource());
        character.AddAbility(new AttackAbility());
        character.AddAbility(new SelectArea());
        character.AddAbility(new AI());

        // Set options
        character.PlayerOwner = player;
        character.Position = new
            (
                position.X<0 ? position.X * 16+8 : position.X * 16-8,
                position.Y<0 ? position.Y * 16+8 : position.Y * 16-8
            );

        return character;
    }

    // == Building
    // === Storage
    public static Character CreateStorage(Player player, Vector2 position)
    {
        Character character = new Character(Type.Resource);

        Image image = Image.LoadFromFile("res://Texture/Game/Buildings/storage.png");
        int[][] sizeInCells = new int[][] 
        {
                         /* 1  2  3  4  5  6  7  8 */
            /*1*/new int[]{ 1, 1, 1, 1, 1, 1, 0, 0 },
            /*2*/new int[]{ 1, 1, 1, 1,-1, 1, 1, 1 },
            /*3*/new int[]{ 0, 0, 1, 1, 1, 1, 1, 1 },
        };

        // Add abilities
        character.AddAbility(new Sprite(image));
        character.AddAbility(new Collision(3,5, null));
        character.AddAbility(new HealthAbility(100, false));
        character.AddAbility(new Storage());

        // Set options
        character.PlayerOwner = player;
        character.Position = position * 16;

        return character;
    }

    // == Resource
    // === Stone
    public static Character CreateStone(Vector2 position)
    {
        Character character = new Character(Type.Resource);

        // Create elements for object
        int[][] sizeInCells = new int[][]
        {
                         /* 1 */
            /*1*/new int[]{-1 }
        };

        CircleShape2D circleShape2D = new CircleShape2D();
        circleShape2D.Radius = 8;

        Image image = Image.LoadFromFile("res://Texture/Game/Resource/Basic Grass Biom things 1.png");
        AtlasTexture atlas = new AtlasTexture();
        Texture2D texture = ImageTexture.CreateFromImage(image); ;
        atlas.Atlas = texture;
        atlas.Region = new(128, 16, 16, 16);

        // Add abilities
        character.AddAbility(new Sprite(atlas.GetImage()));
        character.AddAbility(new AnimationAbility());
        character.AddAbility(new SelectArea(circleShape2D));
        character.AddAbility(new HealthAbility(40, false));
        character.AddAbility(new Resource(ResourceType.Stone));
        character.AddAbility(new Collision(1, 1, circleShape2D));

        // Set options
        character.PlayerOwner = null;
        character.Position = new
            (
                position.X < 0 ? position.X * 16 + 8 : position.X * 16 - 8,
                position.Y < 0 ? position.Y * 16 + 8 : position.Y * 16 - 8
            );

        return character;
    }

    // === Tree
    public static Character CreateTree(Vector2 position)
    {
        Character character = new Character(Type.Resource);

        // Create elements for object
        var shape = new CapsuleShape2D();
        shape.Height = 8;
        shape.Radius = 8;

        Image image = Image.LoadFromFile("res://Texture/Game/Resource/Basic Grass Biom things 1.png");
        AtlasTexture atlas = new AtlasTexture();
        Texture2D texture = ImageTexture.CreateFromImage(image);
        atlas.Atlas = texture;
        atlas.Region = new(16, 0, 32, 32);

        // Add abilities
        character.AddAbility(new Sprite(atlas.GetImage()));
        character.AddAbility(new AnimationAbility());
        character.AddAbility(new SelectArea(shape));
        character.AddAbility(new HealthAbility(40, false));
        character.AddAbility(new Resource(ResourceType.Wood));

        // Set options
        character.PlayerOwner = null;
        character.Position = new
            (
                position.X < 0 ? position.X * 16 + 8 : position.X * 16 - 8,
                position.Y < 0 ? position.Y * 16 + 8 : position.Y * 16 - 8
            );
        character.GetAbility<Sprite>().Scale = new(1f, 1f);

        return character;
    }

}