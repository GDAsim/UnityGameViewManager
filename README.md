# UnityGameViewManager
Programmatically changes Unity Game Window's Resolution On Play. Useful for testing under fix resolution
## How to use:
- Place UnityGameViewManager.cs anywhere in assets folder
- Call UnityResolutionManager.UseCustomResolution(width,height,label) to set the resolution of the GameView within Unity Editor on Play 
- Automatically removes when Play ends
- Note that you should not remove or add new resolution while in Play Mode

## Dev Note:
- Took the code from unity forums and add Contains() function from syy9

## Source:
- https://answers.unity.com/questions/956123/add-and-select-game-view-resolution.html
- https://forum.unity.com/threads/add-game-view-resolution-programatically-old-solution-doesnt-work.860563/
- https://github.com/Syy9/GameViewSizeChanger
