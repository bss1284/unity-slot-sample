## Slot Machine Demo 
<img src="Docs/Capture1.JPG?raw=true" alt="Example" width="512px" height="284px"/>

## Classes
- __Reel  (Assets/SlotGame/Scripts/Reel.cs)__   
It have sprite renderers as childs and update renderers based on internal cursor values(List<int>).
When 'ReelStatus'  is set to moving, 'moveValue' is increased and cursor values is updated automatically. Then renderers is updated using 'OnRenderUpdated' event callback function.

- __Machine  (Assets/SlotGame/Scripts/Machine.cs)__   
It provides a common interface for slot machines and have reels as childs.

### CREATE BY
- Sanghun Lee
- Email: tkdgns1284@gmail.com
- Github: https://github.com/bss1284