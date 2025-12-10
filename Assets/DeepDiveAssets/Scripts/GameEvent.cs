using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject
{
    private event Action _listeners;

    public void Raise()
    {
        _listeners?.Invoke();
    }

    public void Register(Action listener)
    {
        _listeners += listener;
    }

    public void Unregister(Action listener)
    {
        _listeners -= listener;
    }
}
