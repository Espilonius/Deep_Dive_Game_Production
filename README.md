## This repository is for my deep dive of the Game production semester of Fontys ICT
In this repository is almost a plug and play day night system + time event system. Both these systems can be configured to how you want them to work as they are made to be dynamic.

## User Manual – Time & Event System (Unity)

1. Introduction
This manual explains how to use the Time & Event System inside Unity. The system allows you to create a fully configurable day-night cycle with dynamic, time-based events such as meteor showers and eclipses.

2. System Requirements
- Unity 2021 or newer
- C#
- ScriptableObject workflow enabled

3. Core Components Overview
- TimeManager: Controls in-game time.
- TimeSettings: Stores global time configuration.
- DayPartInfo: Defines sections of the day.
- TimeEventManager: Schedules and triggers time-based events.
- TimeEvent: Defines a single event.
- GameEvent: Event channel for triggering gameplay responses.

4. Setting Up the Time System
Step 1: Create a TimeSettings asset.
Right click in the Project window → Create → Day Night Cycle → TimeSettings.

Step 2: Configure time values:
- Minutes Per Hour
- Hours Per Day
- Time Multiplier

Step 3: Create DayParts.
Right click → Create → Day Night Cycle → DayPartInfo.
Set:
- Name
- Start Hour
- Skybox
- Light Gradient
- Transition Time

Step 4: Assign your DayParts inside TimeSettings.

Step 5: Add TimeManager to a scene GameObject.
Assign:
- TimeSettings asset
- Directional Light

5. Using the DayPart Debug Tools
- Enter Play Mode.
- Select the GameObject with TimeManager.
- Use the "Jump to DayPart" buttons to instantly change time.

6. Creating Time Events
Step 1: Create a TimeEvent asset.
Right click → Create → Day Night Cycle → Time Event.

Step 2: Configure:
- Event ID
- Allowed DayParts
- Trigger Mode
- Window Start Hour
- Window End Hour
- Occurrence Mode
- Frequency
- Start Day Number

Step 3: Assign a GameEvent channel.

7. Setting Up Event Responses
- Create a GameEvent asset.
- Add a GameEventListener to a GameObject.
- Assign the GameEvent.
- Bind UnityActions such as:
  - Spawn meteors
  - Change lighting
  - Play sound effects

8. Using the TimeEventManager
- Add TimeEventManager to a GameObject.
- Assign:
  - TimeManager
  - TimeEvent assets

During Play Mode you can:
- View active events.
- See next trigger times.
- Monitor remaining occurrences.
- Force trigger an event from the inspector.

9. Best Practices
- Always define clear DayParts.
- Avoid overlapping time windows.
- Use Start Day Number for story events.
- Use ChancePerDay for random encounters.
- Use OnceEver for unique story moments.

10. Common Issues
Event does not trigger:
- Check if DayPart is allowed.
- Check Frequency and Start Day Number.
- Check GameEvent and listeners.

Time does not update:
- Ensure TimeMultiplier > 0.
- Ensure TimeManager is in the scene.

11. Advanced Usage
- Combine multiple TimeEvents for complex world behavior.
- Use different TimeSettings profiles for different levels.
- Connect TimeEvents to AI behavior systems.

12. Conclusion
This system provides a flexible and powerful framework for managing time and world events in Unity projects. It is suitable for RPGs, survival games, and simulations.

