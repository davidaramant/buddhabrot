# Given

* I am designing a system to find points very close to the boundary of the Mandelbrot Set.
* I have an existing dataset for the boundary regions close to the edge.
  * This is a flat list of "regions" (2D square areas) on the complex plane. The edge of the Mandelbrot Set passes through each region.
* I want to use the cloud offerings on Microsoft Azure
  * The goal is to minimize cost of hosting the cloud backend.
* I want the code written in C# using .NET

# Design

## Cloud Backend

### Boundary File

* There will be a file stored that contains the boundary regions. It is a single binary file that is 164MB uncompressed and 37MB zipped.
* This file should be able to be downloaded from the cloud by workers or other clients.
* The system does not need to support uploading a new boundary file. If this file needs to be updated, it will be done manually.

### Boundary Points

* I want to store points near the boundary of the Mandelbrot Set.
	* Each point consists of:
		* A complex point (two doubles)
		* An escape time (32-bit integer)
* Assume there will be thousands, perhaps up to millions, of points stored.
* The system needs to support uploading batches of new points
  * Each batch will include metadata (see the next section)
* The system needs to support downloading points.
  * There should be a way to download all the points.
  * There should also be a way to provide a minimum escape time. Only points with an escape time greater than the minimum should be downloaded.
  * In addition to the complex value, the escape time should also be downloaded.

### Batch Metadata

* When a batch of points is uploaded, there will also be some metadata about the batch.
* The metadata will include:
	* A string describing the machine that the batch was run on
      * This is only for human consumption; it will not need to be parsed in any way.
	* How long the batch took to run
	* The number of points that were checked
    * The number of points that were found
* There does not need to be a relationship stored between a batch of points and the metadata.
* The metadata should still be retained along with a timestamp of when it was uploaded.
* There should be an endpoint to retrieve all the uploaded batch metadata.

## Workers

### General Requirements

* The worker program will be a console application that downloads the boundary file to the machine, does computationally heavy work, and uploads the results to the cloud backend.
* Workers will be run on many different machines in parallel.
* A worker will do work in batches:
    * For each batch:
        * Pick N random regions
          * The exact size will have to be determined after the code is written and benchmarked. Assume it will be in the thousands (probably somewhere between 4000 and 16000)
        * Pick a random complex number inside of each selected region
        * Iterate the points to see if the point is inside of the Mandelbrot Set.
            * Use 10 million as the maximum iteration limit
            * Only points that are NOT inside of the Mandelbrot Set are interesting. However, they should take at least 100,000 iterations before they escape.
        * Collect the interesting points and report them to a central repository
            * For each point, report the complex value (two doubles) as well as how long it took to escape (a signed 32bit integer)
        * In addition to uploading the points, include metadata about the batch.
* The worker program needs to support an optional number of batches to run. It should end operations after working through all of the batches.
* The console program may also br run without a bound, in which case a user with access to the machine will manually cancel it.
* A worker needs to be able to be canceled at any time without losing any found points
  * When canceled, the worker should report a batch like normal. The only difference is that the number of points checked will not be the full batch size.
* There is no need to coordinate which workers work on which regions.
* Assume that the random number generator is good enough to spread out the work.
  * It is not a big deal if multiple workers pick the same region. The likelihood of this happening is low.
  * Assume it will be virtually impossible for two workers to find the exact same point.

