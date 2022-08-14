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
  - [x] Aspect ratio is WRONG
  - [x] Region drawing optimizations
    - [x] Halt quad tree search based on pixel size? No need to grab sub-pixel stuff
    - [x] Building quad tree is too slow
      - [ ] Optimize this for real - saving it off is a hack
  - [ ] Render region interiors
    - [ ] Multi-threaded rendering
