IMPORTANT NOTICE
================

If you are the developer, publisher, or an authorized party and would like this mod removed
or any features changed/removed, please contact me directly on Discord:

Discord: 2025JOE


MineMoguul Mod Menu
==================

An injectable Unity mod menu for MineMoguul, written in C# and injected at runtime using SharpMonoInjector.

This mod adds an in-game GUI that provides movement, visual, ESP, economy, and teleport modifications.


Injection Method
----------------

This mod does NOT use BepInEx.

It is injected directly into MineMoguul’s Mono runtime using SharpMonoInjector.

Injection entry point:
Loading.Loader.Load()

The loader creates a persistent GameObject and attaches the mod menu, which survives scene changes.


Controls
--------

INSERT  - Open / Close the mod menu  
Mouse   - Navigate the menu  
Drag    - Move the menu window  


Features
--------

Movement Mods:
- Fast Move (custom walk & sprint speed)
- Bunny Hop
- Air Control
- Momentum movement
- Fly Mode
- No Clip
- Infinite Jump
- Super Jump (adjustable height)
- Custom Gravity
- Speed Lines effect

Visual Mods:
- Time Scale control
- X-Ray Vision
- General ESP toggle
- Adjustable ESP range

ESP / Automation:
- AutoMiner ESP boxes
  - Green = Active
  - Red = Inactive
- AutoMiner tracer lines
- Pulsing & distance-based visuals
- Refresh AutoMiners
- Debug info output

Economy Mods:
- Infinite Money
- Free Shopping
- Unlock All Shop Items
- Add Money
- Set Money
- Reset Money
- Sell Multiplier
- Auto Sell support

Teleport Mods:
- Teleport Forward
- Teleport Up
- Teleport To Cursor (raycast-based)


Technical Details
-----------------

- Written in C#
- Uses Unity IMGUI
- Injected via SharpMonoInjector
- Runtime MonoBehaviour injection
- Uses reflection to access game systems
- Handles cleanup and unloading
- ESP objects are safely destroyed when disabled


Project Structure
-----------------

Loader.cs   - Injection entry point  
ModMenu.cs - GUI & menu logic  
Mods.cs    - All gameplay modifications  


Unloading
---------

The mod supports clean unloading using:
Loading.Loader.UnloadCheat()

This removes injected objects and runs garbage collection.


Notes
-----

Game updates may break class names or reflection calls.
Use at your own risk.
For educational and personal use only.


Credits
-------

Created by Longno12  
