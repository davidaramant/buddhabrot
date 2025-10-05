# Given

* I am designing a system to find points very close to the boundary of the Mandelbrot Set.
* I have an existing dataset for the boundary regions close to the edge. It is a single file that is 164MB uncompressed and 37MB zipped.
    * This is a flat list of "regions" (2D square areas) on the complex plane. The edge of the Mandelbrot Set passes through each region.
* I want to use the cloud offerings on Microsoft Azure
* I want the code written in C# using .NET

# Design

## Cloud

## Workers

### General Requirements

* The worker program will be a console application that downloads the boundary file to the machine, does computationally heavy work, and uploads the results to a central repository. 
* Workers will be run on many different machines in parallel.
* A worker needs to be able to be cancelled at any time without losing progress.
* A worker will do work in batches:
    * For each batch:
        * Pick N regions
        * Pick a random complex number inside of each region
        * Iterate the points to see if the point is inside of the Mandelbrot Set.
            * Use 10 million as the maximum iteration limit
            * Only points that are NOT inside of the Mandelbrot Set are interesting. However, they should take at least 100,000 iterations before they escape.
        * Collect the interesting points and report them to a central repository
            * For each point, report the complex value (two doubles) as well as how long it took to escape (a signed 32bit integer)
        * In addition to uploading the points, include some metadata about the batch:
            * How long did the batch take to run
            * How many points were checked
            * A simple description of the machine (assume this will be a `string`; I will supply the code to do this)

