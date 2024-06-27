using Godot;
using System;
using System.Collections.Generic;

public partial class CustomDataLayer<T> : Node
{
    // Хранение данных слоя
    private Dictionary<Vector2I, T> dataLayer = new ();

    // Метод для установки значения в слой данных
    public void SetData(Vector2I position, T value)
    {
        dataLayer[position] = value;
        // Здесь можно добавить логику для обновления отображения
    }

    // Метод для получения значения из слоя данных
    public T GetData(Vector2I position)
    {
        return dataLayer.ContainsKey(position) ? dataLayer[position] : default;
    }

    // Метод для удаления данных из слоя
    public void RemoveData(Vector2I position)
    {
        if (dataLayer.ContainsKey(position))
        {
            dataLayer.Remove(position);
            // Здесь можно добавить логику для обновления отображения
        }
    }

    public void SetDataAsync(Vector2I position, T value)
    {
        if (value is Variant variant) 
        {
            CallDeferred(nameof(UpdateData), position, variant);
        }
    }

    private void UpdateData(Vector2I position, T value)
    {
        dataLayer[position] = value;
        // Логика для обновления отображения
    }

    public void SetDataBatch(Dictionary<Vector2I, T> batchData)
    {
        foreach (var kvp in batchData)
        {
            dataLayer[kvp.Key] = kvp.Value;
        }
        // Логика для обновления отображения
    }
}