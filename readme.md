![Preview](https://github.com/tetreum/war-effort/raw/master/src/preview/preview.gif "Preview")

# WarEffort

I made this project to understand and build a chain management game (like Factorio).

The game is built over a 2D grid (yet it's 3d, so i'm using Vector3) where you can place "Machines".
Machines can take 1 or more grid slots and there are several types:
- Belt: Moves items from one slot to another.
- Generator: Creates items out of nowhere and places them on it's connected belts.
- Converter: Takes X items to create and Y item.
- Seller: Makes items disappear in exchange of cash.

To avoid having tons of Monobehaviours, nor Machines or items have them, instead they're managed by the Grid or the machine where they (the items) are.

I hardly remember how i coded it, but i believe it was multithread. With belts running as one job and the rest of the machines in another.
The demo is WebGL, and webGL isn't multithread in unity (yet?), so it won't make any difference in that platform.
