# TODO

## BoundaryFinder

- [x] Refactor data storage stuff
  - [ ] Allow changing local files location 
- [x] Better busy indicator
- [ ] Integrate DE in boundary scanning?
- [x] Flip Y again - regions going down isn't helpful anymore
- [ ] Real-Time Visualizations
  - [x] Working Canvas
  - [x] Switch to power-of-two divisions
  - [x] QuadTree for regions
  - [x] Render regions
  - [x] Zooming & panning
    - [ ] Zoom where the cursor was clicked
  - [x] Aspect ratio is WRONG
    - [ ] Just show 4x4 area instead of populated area
  - [x] Region drawing optimizations
    - [x] Halt quad tree search based on pixel size? No need to grab sub-pixel stuff
    - [x] Building quad tree is too slow
      - [ ] Optimize this for real - saving it off is a hack
  - [x] Show entire set boundary
  - [ ] Render region interiors
    - [ ] Multi-threaded rendering
  - [ ] Nicer aesthetics
