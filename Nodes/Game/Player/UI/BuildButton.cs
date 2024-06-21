using Godot;
using System;

public partial class BuildButton : TextureButton
{
    private bool isButtonSelected = false;
    private PackedScene buildingScene;
    private Node2D game_objects; 

    public override void _Ready()
    {
        buildingScene = (PackedScene)ResourceLoader.Load("res://GameObjects/Storage/Storage.tscn");
        game_objects = GetNodeOrNull<Node2D>("/root/Game/GameObjects");
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
                // Получаем позицию курсора мыши
                Vector2 mousePosition = mouseEvent.Position;

                // Создаем экземпляр здания и добавляем его в сцену
                var buildingInstance = (Character)buildingScene.Instantiate();
                buildingInstance.GlobalPosition = mousePosition;

                if (game_objects != null)
                {
                    game_objects.AddChild(buildingInstance);
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
