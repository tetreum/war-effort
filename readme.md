![Preview](https://github.com/tetreum/war-effort/blob/master/docs/preview.gif?raw=true)

# WarEffort

I made this project to understand and build a chain management game (like Factorio).

## Demo

[Click here to check it](https://tetreum.github.io/war-effort/)

## Info

The game is built over a 2D grid (yet it's 3d, so i'm using Vector3) where you can place "Machines".
Machines can take 1 or more grid slots and there are several types:
- Belt: Moves items from one slot to another.
- Generator: Creates items out of nowhere and places them on it's connected belts.
- Converter: Takes X items to create an Y item.
- Seller: Makes items disappear in exchange of cash.

To avoid having tons of Monobehaviours, items are managed by the Grid or the machine where they are.

I hardly remember how i coded it, but i believe it was multithread. With belts running as one job and the rest of the machines in another.
The demo is WebGL, and webGL isn't multithread in unity (yet?), so it won't make any difference in that platform.
This was made before ECS. If i were you, i would do it in ECS 100%.
