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
  - [ ] Region drawing optimizations
    - [ ] Halt quad tree search based on pixel size? No need to grab sub-pixel stuff
    - [ ] Building quad tree is too slow
  - [ ] Render region interiors
    - [ ] Multi-threaded rendering
