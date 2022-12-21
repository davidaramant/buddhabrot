# TODO

## BoundaryFinder

### Misc

- [x] Refactor data storage stuff
  - [ ] Allow changing local files location
- [x] NUnit test project for manual "tests"
- [x] CI builds on GitHub
  - [x] ~~Pretty reporting of test results~~ - Tried "Publish Test Results". It's super slow and gives wonky output.
- [x] Improve logging of calculating boundary so it can copied easier
- [ ] Readme for boundary scanning

### Calculating Boundary

- [x] Integrate DE in boundary scanning
  - [x] Update algorithm
  - [x] Persist filament regions
  - [x] Render filament regions
  - [x] Debug what on earth went wrong
- [x] Visualizations of raw quad trees
  - [x] Integrate this into the app somewhere
- [x] Flip Y again - regions going down isn't helpful anymore
- [x] Split regions and quad tree into different files
- [ ] Optimize Boundary Calculations
  - [x] Always pass in VisitedRegions into BorderScan
  - [x] Rework region types to Unknown/Border/Filament/Rejected
  - [x] Build up a quad tree during boundary scan
    - [x] New quad tree node
    - [x] New quad tree class
      - [x] It's real slow - benchmark QuadNode & QuadDimensions
      - [x] Check if a RingBuffer cache for HasVisited or Visit helps
    - [x] Return border regions from quad tree
    - [x] Compress quad tree into RegionLookup
  - [x] Filter out Rejected regions from RegionLookup. They aren't visually interesting.
  - [x] Calculate batches of corners (in parallel) and cache them
    - [x] Batch data can be packed into a `ushort` (don't combine corners and centers, I think)
    - [x] Fixed-size cache for corners
    - [ ] Determine best amount of data to batch & cache
    - [ ] Visualization (video) of batches being cached & evicted
- [x] Investigating putting an index into the `VisitedRegions` - EDIT: complete dud
- [ ] Support diffing arbitrary quad trees
  - [x] ~~Do I need a more generic quad tree internal data structure?~~ - Split into two types of trees 
  - [x] Expand RegionType to 3 bits
  - [ ] Generate quad tree diffs
- [ ] Diagnose problems with algorithm:
  - [ ] "Holes" in boundary/filaments
  - [ ] Useless border regions around i = 0

### GUI

- [x] Better busy indicator
- [x] Allow log area to be hidden
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
  - [x] ~~Rework how the logical area is calculated~~ (EDIT: what did this mean?)
  - [ ] Render region interiors
    - [x] `RenderTargetBitmap` - https://github.com/AvaloniaUI/Avalonia/issues/2492
    - [x] `RenderInstructions`
    - [x] Handle resizing
    - [x] BUG: Resizing too fast goes haywire
    - [x] Panning
    - [ ] New mode for rendering border region interiors
      - [x] UI toggle for rendering interior
      - [ ] New iteration kernel method for running through a list of points
      - [x] Do the actual rendering
      - [ ] BUG: Panning & zooming causes things to go haywire with region interiors
  - [x] Nicer aesthetic
  - [x] ~~Fill the Mandelbrot interior (scanline rendering)~~ - No good method of doing this
  - [x] Pick palette to render with
    - [x] UI for palette choices
    - [x] Backend for picking palettes
  - [ ] More palettes to choose from
  - [x] Deep zooms on enormous trees are slow (optimize RegionLookup) - Minor optimizations
- [ ] Update colors for the new region types
- [ ] UI to create diff between two `RegionLookup`s
