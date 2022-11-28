# TODO

## BoundaryFinder

- [x] Refactor data storage stuff
  - [ ] Allow changing local files location 
- [x] Better busy indicator
- [x] Integrate DE in boundary scanning
  - [x] Update algorithm
  - [x] Persist filament regions
  - [x] Render filament regions
  - [x] Debug what on earth went wrong
- [x] Visualizations of raw quad trees
  - [x] Integrate this into the app somewhere
- [x] Flip Y again - regions going down isn't helpful anymore
- [x] Allow log area to be hidden
- [x] Split regions and quad tree into different files
- [ ] Optimize Boundary Calculations
  - [x] Always pass in VisitedRegions into BorderScan
  - [x] Rework region types to Unknown/Border/Filament/Rejected
    - [ ] Update colors for the new region types
  - [x] Build up a quad tree during boundary scan
    - [x] New quad tree node
    - [x] New quad tree class
      - [x] It's real slow - benchmark QuadNode & QuadDimensions
      - [x] Check if a RingBuffer cache for HasVisited or Visit helps
    - [x] Return border regions from quad tree
    - [x] Compress quad tree into RegionLookup
  - [ ] Calculate batches of corners (in parallel) and cache them
- [ ] Real-Time Visualizations
  - [x] Working Canvas
  - [x] Switch to power-of-two divisions
  - [x] QuadTree for regions
  - [x] Render regions
  - [x] Zooming & panning
    - [x] Zoom where the cursor was clicked
  - [x] Aspect ratio is WRONG
    - [x] Just show 4x4 area instead of populated area
  - [x] Region drawing optimizations
    - [x] Halt quad tree search based on pixel size? No need to grab sub-pixel stuff
    - [x] Building quad tree is too slow
      - [x] Optimize this for real - saving it off is a hack (taken care of by the dual-quadtree approach)
  - [x] Show entire set boundary
  - [x] There is an unstable 1-pixel gap between regions
  - [ ] Rework how the logical area is calculated (EDIT: what did this mean?)
  - [ ] Render region interiors
    - [x] `RenderTargetBitmap` - https://github.com/AvaloniaUI/Avalonia/issues/2492
    - [x] `RenderInstructions`
    - [x] Handle resizing
    - [x] BUG: Resizing too fast goes haywire
    - [x] Panning
    - [ ] New mode for rendering border region interiors
      - [x] UI toggle for rendering interior
      - [ ] New iteration kernel method for running through a list of points
      - [ ] Do the actual rendering
  - [x] Nicer aesthetic
  - ~~[ ] Fill the Mandelbrot interior (scanline rendering)~~ - No good method of doing this
  - [ ] Pick palette to render with
    - [x] UI for palette choices
    - [ ] Backend for picking palettes
    - [ ] More palettes to choose from
- [x] NUnit test project for manual "tests"
- [x] CI builds on GitHub


## Benchmarks
### ThinkPad
#### 16 / 5M
##### Old
Number of visited regions: 50,778,193
Found boundary for Vertical Divisions: 65,536 Max Iterations: 5,000,000.
	- Took 00:27:46.0629665
	- Found 35,410,146 border regions
Cache size: 1,358,995, num times cached value used: 8,683,678, Num nodes: 5,435,981
Constructed quad tree (size: 5,435,981). Took 00:08:10.0421168

##### New

Visited boundary for Vertical Divisions: 65,536 Max Iterations: 5,000,000.
- Took 00:25:27.8801404
Found 1,130,138 boundary regions.
- Took 00:00:00.3412489
Normalized quad tree to Region Lookup
- Went from 22,151,249 to 5,826,673 nodes (26.30%)
- Took 00:00:01.1627687

##### With CacheTable

Visited boundary for Vertical Divisions: 65,536 Max Iterations: 5,000,000.
- Took 00:23:03.7145559
Found 1,130,138 boundary regions.
- Took 00:00:00.3697175
Normalized quad tree to Region Lookup
- Went from 22,151,249 to 5,826,673 nodes (26.30%)
- Took 00:00:01.2813702
