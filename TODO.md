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
    - [x] ~~Visualization (video) of batches being cached & evicted~~ - This seems pointless
  - [ ] Benchmark boundary calculation process
- [x] Investigating putting an index into the `VisitedRegions` - EDIT: complete dud
- [x] Support diffing arbitrary quad trees
  - [x] ~~Do I need a more generic quad tree internal data structure?~~ - Split into two types of trees
  - [x] Expand RegionType to 3 bits
  - [x] Separate RegionType for VisitedRegions and RegionLookup
  - [x] Generate quad tree diffs
    - [x] Determine Region Type based on Left and Right
- [x] Diagnose problems with algorithm:
  - [x] Super sample interior of region
  - [x] Heuristic for regions
    - [x] "Holes" in boundary/filaments
    - [x] Useless border regions around i = 0 - EDIT: "Good enough" fix found
- [x] Allow metadata in boundary file names (IE specify variants of same dataset)
- [x] Allow specifying heuristic to use for boundary detection
  - [x] Interface for region classifier
  - [x] Break out new heuristic to different class
  - [x] 2x2 heuristic
  - [x] Command line argument for BoundaryFinder for heuristic
  - [x] GUI for heuristic for completeness when generating boundaries
  - [x] ~~Benchmarks for heuristics~~ - No. Just benchmark the entire operation. I'm going with Internal4

## BoundaryExplorer

- [x] Better busy indicator
- [x] Allow log area to be hidden
- [ ] Real-Time Visualizations
  - [x] Working Canvas
  - [x] Switch to power-of-two divisions
  - [x] Quadtree for regions
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
      - [x] BUG: Panning & zooming causes things to go haywire with region interiors
      - [ ] Deal with splotchy in-range points (distance estimator somehow?)
        - [ ] Multiple options of rendering interior
        - [ ] Implement DE interior rendering
  - [x] Nicer aesthetic
  - [x] ~~Fill the Mandelbrot interior (scanline rendering)~~ - No good method of doing this
  - [x] Pick palette to render with
    - [x] UI for palette choices
    - [x] Backend for picking palettes
    - [x] Faster palette lookup for RegionType (dictionary)
    - [x] Show a palette legend in the UI
  - [x] More palettes to choose
  - [x] Black & White palette for max contrast
  - [x] Deep zooms on enormous trees are slow (optimize RegionLookup) - Minor optimizations
- [x] Update colors for the new region types
  - [x] Associate a type with every pixel rendered
  - [x] Compute the colors for InSet/Outside/InRange
- [x] UI to create diff between two `RegionLookup`s
  - [x] New tab
    - [x] UI framework
    - [x] Queries for left & right side
    - [x] Compute
  - [x] Show diffs in drop down correctly on Visualize tab
- [x] Update stats on Calculate page to show something useful (size as gigapixels)
- [x] Better way of showing location of cursor
  - [x] Remove that old junk region display
  - [x] Show complex point of cursor position - even better, showing the region
- [x] Move the global log to a tab
- [x] Put logs on the calculator / diff tabs
- [x] Display heuristics about a region
  - [x] UI to pick a region
  - [x] Calculate heuristics
  - [x] Display heuristics
  - [x] Choose which type of heuristic to display
- [ ] Refactor how rendering is done
  - [ ] Rendering to an `SKCanvas` or whatever should be a distinct operation
  - [ ] UI should just be concerned with drawing the `SKImage` on the `RenderingTargetBitmap`
  - [ ] PoC for handling image buffers
  - [ ] Research into how to parallelize rendering of pixels

### Avalonia 11 Upgrade

- [x] `NumericUpDown` has changed from `double` to `decimal`
- [x] What is going on with UI control properties?
- [x] `VisualizeTab`: `ItemsRepeater` is gone?
- [x] Fix `MandelbrotRenderer`
