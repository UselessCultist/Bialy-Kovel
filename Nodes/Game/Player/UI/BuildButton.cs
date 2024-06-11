using Godot;
using System;

public partial class BuildButton : TextureButton
{
    private bool isButtonSelected = false;
    private PackedScene buildingScene;

    public override void _Ready()
    {
        buildingScene = (PackedScene)ResourceLoader.Load("res://GameObjects/Storage/Storage.tscn");
    }

    private void OnButtonPressed()
    {
        // Запоминаем, что кнопка была выбрана
        isButtonSelected = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (isButtonSelected)
            {
                // Получаем позицию курсора мыши
                Vector2 mousePosition = mouseEvent.Position;

                // Создаем экземпляр здания и добавляем его в сцену
                var buildingInstance = (Character)buildingScene.Instantiate();
                buildingInstance.Position = mousePosition;

                GetTree().CurrentScene.AddChild(buildingInstance);

                // Сбрасываем состояние кнопки
                isButtonSelected = false;
            }
        }
    }

    public override void _Pressed()
    {
        isButtonSelected = true;
    }
}
