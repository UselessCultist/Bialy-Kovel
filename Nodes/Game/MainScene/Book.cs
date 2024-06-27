using Godot;
using System;
using System.Collections.Generic;

public partial class Book : Control
{
    private Label _leftPageLabel;
    private Label _rightPageLabel;
    private TextureButton _prevButton;
    private TextureButton _nextButton;

    private int _currentPage = 0;
    private List<string> _pages = new List<string>
    {
        "В старину кирпичи делали на месте и первая же собранная печь, для их изготовления, состояла не из выветренных кирпичей, закаляясь в процессе.",
        "Смолянский замок возвели на правом берегу реки Дерновка. Размером он был 100 х 200 метров, позднее Белый Ковель был разрушен во время войны.",
        "Славяне носили вышиванки, которые служили не только одеждой, но и оберегами, защищавшими от злых духов.",
        "Славянские лесопилки располагались у рек, что облегчало транспортировку древесины и использование водяных мельниц для работы пил.",
        "Славянские воины украшали свои щиты и оружие символами и узорами, веря, что это придаст им силы и защитит в бою.",
        "Осадой белого ковеля занималось русское войско из около 10 000  человек, защитников замка же было около 2 000."
    };

    public override void _Ready()
    {
        _leftPageLabel = GetNode<Label>("LeftPageLabel");
        _rightPageLabel = GetNode<Label>("RightPageLabel");
        _prevButton = GetNode<TextureButton>("PrevButton");
        _nextButton = GetNode<TextureButton>("NextButton");

        _prevButton.Pressed += OnPrevButtonPressed;
        _nextButton.Pressed += OnNextButtonPressed;

        UpdatePages();
    }

    private void OnPrevButtonPressed()
    {
        if (_currentPage > 0)
        {
            _currentPage -= 2; // Переход на предыдущую пару страниц
            UpdatePages();
        }
    }

    private void OnNextButtonPressed()
    {
        if (_currentPage < _pages.Count - 2)
        {
            _currentPage += 2; // Переход на следующую пару страниц
            UpdatePages();
        }
    }

    private void UpdatePages()
    {
        _leftPageLabel.Text = _pages[_currentPage];
        _rightPageLabel.Text = _currentPage + 1 < _pages.Count ? _pages[_currentPage + 1] : "";

        _prevButton.Disabled = _currentPage == 0;
        _nextButton.Disabled = _currentPage >= _pages.Count - 2;
    }
}
