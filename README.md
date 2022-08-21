# PathFindingA (2D and 3D) using threads

<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#How-to-use">How to use</a></li>
    <li><a href="#How-it-works">How it works</a></li>
    <li><a href="#How-to-add-new-subsystem">How add new subsystem</a></li> 
  </ol>
</details>

## How to use
- Import sample prefabs.
- Drop Sample prefab to the scene
- For 2D or 3D switch `bool` in inspector at `GridManager.cs`
- Call `PathFinding.Request_FindPath()` for pathFinding or `ClosestCellsFinding.Request_Cells` for find closest cells

## How it works
- You call any system like `PathFinding.Request_FindPath()` with a callback (endpoint)
- `PathFinding.Request_FindPath()` make request with method for threads and put it to the `RequestManager.cs`
- `RequestManager.cs` invoke request at threads
- Threads do work
- `RequestManager.cs` get a callback from threads when work is done
- `RequestManager.cs` call callback (endpoint)

## How to add new subsystem
- Inherit from `AFinding_Base`, it takes GridManager.cs and makes Singleton 
- If you need threading
   - Create classes `Request` and `RequestResult`
   - Make static (enterPoint) method which will make request for `RequestManager.cs`
- Make the main static method

The best approach just to look to the `PathFinding.cs`
