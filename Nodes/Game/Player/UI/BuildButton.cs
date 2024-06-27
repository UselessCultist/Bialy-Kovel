using Godot;
using System;

public partial class BuildButton : TextureButton
{
    private bool isButtonSelected = false;
    private PackedScene buildingScene;
    private Node2D game_objects;
    private Player player;

    public override void _Ready()
    {
        buildingScene = (PackedScene)ResourceLoader.Load("res://GameObjects/Storage/Storage.tscn");
        game_objects = GetNodeOrNull<Node2D>("/root/Game/GameObjects");

        player = (Player)GetTree().GetFirstNodeInGroup("player");
    }

    private void OnButtonPressed()
    {
        // Запоминаем, что кнопка была выбрана
        isButtonSelected = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && isButtonSelected)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                // Создаем экземпляр здания и добавляем его в сцену
                var buildingInstance = (Character)buildingScene.Instantiate();
                buildingInstance.Position = player.GetGlobalMousePosition();

                if (game_objects != null)
                {
                    if (player.GetResource(ResourceType.Wood) >= 200)
                    {
                        player.AddResource(ResourceType.Wood, - 200);
                        game_objects.AddChild(buildingInstance);
                    }
                }


                // Сбрасываем состояние кнопки
                isButtonSelected = false;
            }
            else 
            {
                isButtonSelected = false;
            }
        }
    }



    public override void _Pressed()
    {
        isButtonSelected = true;
    }
}
