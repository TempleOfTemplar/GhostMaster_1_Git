// EventSystem.cs - Simple event system for game communication
using UnityEngine;
using System;
using System.Collections.Generic;

public static class EventSystem
{
    private static Dictionary<string, Action<object>> events = new Dictionary<string, Action<object>>();
    
    public static void Subscribe(string eventName, Action<object> callback)
    {
        if (!events.ContainsKey(eventName))
            events[eventName] = null;
            
        events[eventName] += callback;
    }
    
    public static void Unsubscribe(string eventName, Action<object> callback)
    {
        if (events.ContainsKey(eventName))
            events[eventName] -= callback;
    }
    
    public static void Trigger(string eventName, object data = null)
    {
        if (events.ContainsKey(eventName) && events[eventName] != null)
            events[eventName].Invoke(data);
    }
    
    public static void Clear()
    {
        events.Clear();
    }
}
