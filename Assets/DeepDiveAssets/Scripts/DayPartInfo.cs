using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DayPartInfo", menuName = "Day Night cycle/DayPartInfo")]
public class DayPartInfo : ScriptableObject
{
    [Tooltip("Name shown in logs/debug (e.g. Morning, Night, Eclipse)")]
    public string DayPartName;

    [Tooltip("Skybox texture used during this day part.")]
    public Texture2D DayPartSkybox;

    [Tooltip("Directional light color over the course of this day part.")]
    public Gradient DayPartGradient;

    [Tooltip("At which in-game HOUR this day part starts.\n" +
             "0 = midnight, with a range based on HoursPerDay in TimeSettings.")]
    [Range(0, 143)]
    public int DayPartStart;

    [Tooltip("Time (in real seconds) to blend from the previous day part to this one.")]
    public float DaypartTransitionTime = 5f;
}
