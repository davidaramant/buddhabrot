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
- [ ] Real-Time Visualizations
  - [x] Working Canvas
  - [x] Switch to power-of-two divisions
  - [x] QuadTree for regions
  - [x] Render regions
  - [x] Zooming & panning
    - [ ] Zoom where the cursor was clicked
  - [x] Aspect ratio is WRONG
    - [x] Just show 4x4 area instead of populated area
  - [x] Region drawing optimizations
    - [x] Halt quad tree search based on pixel size? No need to grab sub-pixel stuff
    - [x] Building quad tree is too slow
      - [ ] Optimize this for real - saving it off is a hack
  - [x] Show entire set boundary
  - [ ] There is an unstable 1-pixel gap between regions
  - [ ] Render region interiors
    - [ ] Multi-threaded rendering
      - [ ] `WriteableBitmap`
        - [ ] `Screens` - get screen the window is on, get DPI from that 
  - [ ] Nicer aesthetic
- [x] NUnit test project for manual "tests"
- [ ] CI builds on GitHub
