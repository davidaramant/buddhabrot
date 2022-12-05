# Benchmarks

Unorganized dumping ground for benchmark results.

## ThinkPad
### Old
Number of visited regions: 50,778,193
Found boundary for Vertical Divisions: 65,536 Max Iterations: 5,000,000.
	- Took 00:27:46.0629665
	- Found 35,410,146 border regions
Cache size: 1,358,995, num times cached value used: 8,683,678, Num nodes: 5,435,981
Constructed quad tree (size: 5,435,981). Took 00:08:10.0421168

### New

Visited boundary for Vertical Divisions: 65,536 Max Iterations: 5,000,000.
- Took 00:25:27.8801404
Found 1,130,138 boundary regions.
- Took 00:00:00.3412489
Normalized quad tree to Region Lookup
- Went from 22,151,249 to 5,826,673 nodes (26.30%)
- Took 00:00:01.1627687

### With CacheTable

Visited boundary for Vertical Divisions: 65,536 Max Iterations: 5,000,000.
- Took 00:23:03.7145559
Found 1,130,138 boundary regions.
- Took 00:00:00.3697175
Normalized quad tree to Region Lookup
- Went from 22,151,249 to 5,826,673 nodes (26.30%)
- Took 00:00:01.2813702

Visited boundary for Vertical Divisions: 131,072 Max Iterations: 5,000,000.
- Took 01:17:49.3831915
Found 2,739,961 boundary regions.
- Took 00:00:00.6050862
Normalized quad tree to Region Lookup
- Went from 73,633,001 to 17,758,873 nodes (24.12%)
- Took 00:00:03.8213886

## Mac

Visited boundary for Vertical Divisions: 131,072 Max Iterations: 5,000,000.
- Took 04:14:15.5064025
Found 2,739,961 boundary regions.
- Took 00:00:01.5352555
Normalized quad tree to Region Lookup
- Went from 73,633,001 to 17,301,061 nodes (23.496%)
- Took 00:00:06.1649922

Visited boundary for Vertical Divisions: 131,072 Max Iterations: 5,000,000.
- Took 04:22:57.5526510
Found 2,739,961 boundary regions.
- Took 00:00:01.7131763
Normalized quad tree to Region Lookup
- Went from 73,633,001 to 17,301,061 nodes (23.496%)
- Took 00:00:08.8693450


2022-12-03T19:06:21
macOS (13.0.1) |  10 Cores | 32 GB
Visited boundary for (16) Vertical Divisions: 65,536, Max Iterations: 5,000,000 (1 hour, 24 minutes)
Found 1,130,138 boundary regions (1 second, 66 milliseconds)
Normalized quad tree to Region Lookup (6 seconds, 192 milliseconds)
- Went from 22,151,249 to 5,630,141 nodes (25.417%)
