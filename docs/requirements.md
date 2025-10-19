### Scope

This document defines the requirements for a distributed system that finds and stores points near the boundary of the Mandelbrot Set. All requirements are expressed in a consistent format using “shall” statements.

### Goals and Constraints

- The system shall target Microsoft Azure services with an emphasis on minimizing cloud hosting cost.
- The system shall use Azure Managed Identity (Entra ID) as the authentication mechanism for any Azure/cloud resources in deployed environments.
- The system shall be implemented in `C#` on `.NET`.
- The system shall ingest and serve data related to Mandelbrot boundary exploration at scale (thousands to millions of points).

### Definitions

- `Boundary file`: A single binary file describing square regions in the complex plane that the Mandelbrot set boundary passes through.
- `Region`: A 2D square area on the complex plane contained in the boundary file.
- `Point`: A complex coordinate near the boundary with an associated `escape time` produced by iteration.
- `Escape time`: A 32-bit signed integer representing the number of iterations until escape.
- `Batch`: A unit of work performed by a worker containing many candidate points evaluated and some interesting points uploaded, alongside batch metadata.

### Functional Requirements

#### Cloud Backend

##### Boundary File
- The system shall store a single `boundary file` (approximately `164 MB` uncompressed, `37 MB` zipped).
- The system shall make the boundary file downloadable by workers and other clients.
- The system shall not support uploading new boundary files via the system; boundary file updates shall be performed manually out-of-band.

##### Boundary Points Storage and Access
- The system shall store points near the Mandelbrot boundary, where each stored point shall consist of:
	- A complex value represented by two `double` values `(real, imaginary)`.
	- An `escape time` represented by a 32-bit signed integer.
- The system shall accept uploads of points in batches.
- The system shall transmit points in a binary format for uploads and downloads to prevent precision loss.
- The system shall store points server-side in Azure Blob Storage.
	- Points shall be partitioned into 10 escape-time buckets and stored in separate containers or prefixes per bucket.
	- Buckets shall cover escape times from `100,000` to `10,000,000`, using manually chosen, aesthetically pleasing boundaries with finer granularity at lower escape times and coarser granularity at higher escape times.
	- The 10 buckets shall use the following boundaries (lower bounds divisible by 1,000):
		- Bucket 0: [100,000, 150,000)
		- Bucket 1: [150,000, 200,000)
		- Bucket 2: [200,000, 300,000)
		- Bucket 3: [300,000, 400,000)
		- Bucket 4: [400,000, 600,000)
		- Bucket 5: [600,000, 1,000,000)
		- Bucket 6: [1,000,000, 1,500,000)
		- Bucket 7: [1,500,000, 2,500,000)
		- Bucket 8: [2,500,000, 5,000,000)
		- Bucket 9: [5,000,000, 10,000,000]
- The system shall support downloading points with the following capabilities:
	- The system shall provide a way to download all points.
	- The system shall provide a way to download only the points in a single escape-time bucket.
	- The system shall include both the complex value and the `escape time` in download results.
- The system shall provide a way to retrieve the total count of stored points.
- The system shall provide a way to retrieve the count of stored points in each escape-time bucket.
- There shall be no requirement on the order in which points are stored or retrieved.
- The system shall support storage at scales from thousands up to millions of points.

##### Batch Metadata
- The system shall accept batch metadata uploads whenever a batch of points is uploaded.
- The system shall store the following metadata for each batch:
	- A free-form string describing the machine that ran the batch (for human consumption only; no parsing required).
	- The total wall-clock duration of the batch run.
	- The number of points checked.
	- The number of points found (i.e., interesting points uploaded).
	- A server-side timestamp indicating when the metadata was uploaded.
- The system shall not store an explicit relationship between any specific uploaded points and their associated batch metadata.
- The system shall provide an endpoint to retrieve all uploaded batch metadata records.

#### Workers

##### General Behavior
- The system shall provide a console-based worker application that:
	- Downloads the boundary file to local storage.
	- Performs computationally intensive exploration work.
	- Uploads interesting points and batch metadata to the cloud backend.
- The worker shall be runnable either on a local developer machine or on a cloud machine managed by Azure Batch.
- The system shall support running many worker instances in parallel on different machines without coordination.

##### Batch Processing
- The worker shall perform work in batches. For each batch, the worker shall:
	- Select `N` random `regions` from the boundary file.
		- The worker shall allow `N` to be configured and tuned post-implementation, with an expected range on the order of thousands (approximately `4,000`–`16,000`).
	- For each selected region, select a random complex point uniformly from within the region.
	- Iterate each point to determine membership in the Mandelbrot Set using a maximum iteration limit of `10,000,000`.
	- Identify as “interesting” only those points that are not in the set and whose `escape time` is at least `100,000` iterations.
	- Collect each interesting point’s complex value `(two doubles)` and `escape time` `(signed 32-bit integer)`.
	- Upload the collected interesting points to the cloud backend.
	- Upload batch metadata as specified above.

##### Execution Control and Cancellation
- The worker shall support an optional configuration to run a specified number of batches and then terminate.
- The worker shall support running indefinitely until manually canceled by a user with access to the machine.
- The worker shall be safely cancelable at any time without losing any already-found interesting points.
	- Upon cancellation, the worker shall complete the current batch upload as usual; however, the `points checked` value may be less than the configured batch size.

##### Concurrency and Coordination
- The system shall not require coordination among workers regarding which regions they select.
- The system shall rely on a sufficiently random selection process to distribute work across regions.
- The system shall accept that multiple workers may occasionally select the same region.
- The system shall assume the probability of two workers discovering the exact same point is negligible.

### Interfaces (High-Level)

- The system shall expose endpoints or equivalents for:
	- Downloading the `boundary file`.
	- Uploading a batch of points (bulk insert) and its associated batch metadata.
	- Downloading all points or only the points in a single escape-time bucket.
	- Retrieving the total count of stored points.
	- Retrieving counts of stored points per escape-time bucket.
	- Retrieving all batch metadata records.

### Data Model (Logical)

- `Point`
	- `real: double`
	- `imag: double`
	- `escapeTime: int32`

- `BatchMetadata`
	- `machineDescription: string` (free-form)
	- `duration: timespan` (or total milliseconds as `int64`)
	- `pointsChecked: int32`
	- `pointsFound: int32`
	- `uploadedAtUtc: datetime`

### Non-Functional Requirements

- The system shall emphasize low operational cost on Azure (e.g., by favoring object storage and simple compute where possible).
- The system shall be operable at scales up to millions of stored points.
- The system shall ensure durability of uploaded points and metadata, resilient to worker cancellations or failures.

### Assumptions

- The provided boundary file correctly enumerates regions that include the Mandelbrot boundary.
- Random number generation is sufficient to distribute sampling across regions.
- Manual boundary file updates, if required, will be handled outside the system’s interfaces.

### Out of Scope

- Interactive or real-time coordination among workers for region selection.
- Deduplication of points across workers beyond implicit statistical rarity of duplicates.
- Parsing or validation of the free-form `machineDescription` beyond storage and retrieval.
