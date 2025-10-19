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

#### Access Control and Networking
- All endpoints and data surfaces shall be private; no anonymous or publicly readable access shall be permitted.
- The boundary file download shall not be publicly accessible; clients shall authenticate to download it.
- Azure-hosted components shall authenticate to Azure resources using Azure Managed Identity.
- Non-Azure workers and tools shall authenticate to Azure Blob Storage using Shared Access Signatures (SAS).
- The system shall prefer User Delegation SAS issued via the backend using its Managed Identity, rather than Account SAS, where supported.
- SAS tokens for non-Azure workers shall be time-limited and scope-restricted to the minimum necessary containers/blobs and permissions.
- The system shall not require publicly routable endpoints for normal operation; any control plane used to issue SAS tokens shall also be private and require operator authentication.

##### SAS Permissions
- Boundary file download SAS: `Read` on the specific blob (and optional `List` on the parent path if directory listing is required).
- Points upload SAS: only the minimal write permissions (`Write`/`Create`, or `Add` for Append Blobs) scoped to the target container/prefix; optional `Read` if verification is required.
- Points download SAS: `Read` scoped to the required containers/prefixes (all points or a single escape-time bucket).
- Batch metadata retrieval: `Read` on metadata objects if accessed via Blob Storage; if accessed via a backend endpoint, access shall be private and require authentication.

#### Operational Policies
- The system shall not enforce per-identity or per-endpoint rate limits or request quotas.

##### Boundary File
- The system shall store a single `boundary file` (approximately `164 MB` uncompressed, `37 MB` zipped).
- The system shall make the boundary file downloadable by workers and other clients.
- The system shall not support uploading new boundary files via the system; boundary file updates shall be performed manually out-of-band.
- The boundary file download shall support gzip compression via standard `Accept-Encoding/Content-Encoding` negotiation.
- SAS tokens granted for boundary file download shall include only `Read` permission on the specific blob (and optional `List` on its parent path if directory listing is required).

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
  - To minimize write contention and enable many simultaneous uploads, the system shall store uploaded points as immutable segment files (blobs) under each escape-time bucket rather than appending to a single shared object.
  - Each upload operation may create one or more new segment files in the target bucket; uploads shall not require read-modify-write of existing point files.
  - Segment files shall be written as `Block Blob` objects (or functionally equivalent) using staged blocks and a single commit to avoid server-side append contention.
  - Segment files shall target a maximum uncompressed size (e.g., 64–256 MiB; the exact initial target shall be 128 MiB unless changed during implementation) to balance efficient downloads with parallelism and list performance.
  - Segment file names shall encode the bucket, UTC date/time partitioning, and a unique identifier to ensure lexicographic time ordering and easy filtering. A non-normative example path pattern is:
      - `points/bucket-<id>/yyyy/mm/dd/hh/mm/<yyyyMMddTHHmmssfffZ>__<batchId>__<segmentSeq>.bin`.
  - The system shall rely on the server-side creation or last-modified timestamp for each segment file (`createdAtUtc`), and the timestamp embedded in the name, to support time-based retrieval.
  - Batch uploads shall be idempotent with server-side de-duplication on retry. Clients shall provide a unique `batchId` per batch and stable per-segment identifiers (e.g., blob names embedding `batchId` and `segmentSeq`); replays of the same `(batchId, segmentId/segmentSeq)` shall not create additional segment files and shall be treated as success/no-op by the backend.
  - The backend shall enforce at-most-once commit per unique segment identifier; if an identically named/identified segment already exists and is fully committed, the upload retry shall not alter stored data nor re-increment any counters.
  - The idempotency guarantee shall apply irrespective of client-side network retries or timeouts and shall not require clients to perform existence checks prior to retrying.
- The system shall support downloading points with the following capabilities:
	- The system shall provide a way to download all points.
	- The system shall provide a way to download only the points in a single escape-time bucket.
	- The system shall include both the complex value and the `escape time` in download results.
- The system shall provide a way to retrieve the total count of stored points.
- The system shall provide a way to retrieve the count of stored points in each escape-time bucket.
- There shall be no requirement on the order in which points are stored or retrieved.
- The system shall support storage at scales from thousands up to millions of points.
- The system shall support gzip compression for all uploads and downloads of binary payloads (points and boundary file).
- For uploads, clients may send compressed payloads with `Content-Encoding: gzip`; the backend shall accept both compressed and uncompressed uploads.
- For downloads, clients may advertise support using `Accept-Encoding: gzip`; when appropriate, the backend shall return compressed responses with `Content-Encoding: gzip` and include `Vary: Accept-Encoding`.
- Compression negotiation shall use standard HTTP/Blob Storage content-encoding semantics; no custom headers are required.
- The system shall preserve the binary format of point data irrespective of compression; compression is applied only as a transport encoding.
- SAS tokens granted for point uploads shall include only the minimal write permissions required for the designated container/prefix (e.g., `Write`/`Create`, or `Add` for Append Blobs) and optional `Read` if the client must verify writes.
- SAS tokens granted for point downloads shall include only `Read` permission scoped to the containers/prefixes required by the request (e.g., all points or a single escape-time bucket).
- The system shall allow exact-duplicate points in storage; the backend shall not attempt deduplication nor verify uniqueness during upload.
- All point counts (total and per escape-time bucket) shall reflect raw stored rows, including exact duplicates.
- The system shall provide a way to retrieve only newly uploaded points since a specified server-side timestamp, at a coarse granularity of whole segment files (i.e., entire segments are included or excluded; no partial-segment filtering).
- The time filter shall use the server-side `createdAtUtc` (or `last-modified`) of segment files and/or their lexicographically ordered timestamped names; client clocks shall not determine inclusion.
- The retrieval operation shall support optional filtering by a single escape-time bucket.
- The retrieval operation shall return:
	- Zero or more segment files containing points with `createdAtUtc >= startTimestampUtc`.
	- A `watermark` value indicating the highest included `createdAtUtc` (and tie-breaker name) that clients can persist and use as the next `startTimestampUtc` to achieve incremental consumption.
- The system shall define deterministic tie-breaking when multiple segments share the same `createdAtUtc` (e.g., by lexicographic blob name); the `watermark` shall include both timestamp and last-included name to prevent duplication or gaps on retry.
- The system shall document that results may include duplicates across successive incremental calls if clients use an older watermark (idempotent re-consumption).
- The system may store points in per-upload segment files; this does not constitute an explicit, queryable mapping between individual points and batch metadata.
- The system shall maintain strongly consistent counters for the total number of stored points and for the number of stored points in each escape-time bucket.
- Reads of these counters shall be read-after-write consistent for any successfully completed upload operation (i.e., once an upload call returns success, a subsequent counter read reflects that upload’s points).
- Counter updates shall be atomic with the commit of new segment files and shall increment by the exact number of points contained in each committed segment.
- Counter updates shall be idempotent with respect to retries: replays of the same logical segment (e.g., same `segmentId` derived from the blob name) shall not double-increment counters.
- In the event an upload fails and its segment is not committed, no counter increments shall be applied.
- All counters shall reflect raw stored rows, including exact duplicates, consistent with existing duplicate-allowance requirements.

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
- SAS tokens used to retrieve batch metadata via storage shall include only `Read` permission on the metadata objects, if accessed directly via Blob Storage; if retrieved via a backend endpoint, access shall still be private and require authentication.
- The system shall maintain a strongly consistent counter for the total number of batches uploaded.
- A “batch uploaded” shall be counted exactly once upon successful acceptance of the batch’s metadata (and, if applicable, its associated point segments) by the backend.
- The batch counter shall be idempotent with respect to client retries (e.g., identified by a unique `batchId`) to prevent double-counting.
- The upload of batch metadata shall be idempotent using `batchId`; retries for the same `batchId` shall not create additional metadata records and shall not increment the batch counter.

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
        - This value will be referred to as the batch size.
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
- All endpoints shall be versioned and include an explicit major version identifier (e.g., `/v1/...`) in the endpoint path or equivalent API surface.
- The initial released API version shall be `v1`.
- Backward-incompatible (breaking) changes shall only be introduced in a new major version (e.g., `v2`), coexisting with prior versions for a deprecation period.
- Documentation for each endpoint shall identify its version and any compatibility guarantees.
- Retrieving new points uploaded since a specified server-side timestamp (optionally scoped to a single escape-time bucket), at segment granularity, returning a `watermark` for incremental continuation.
- Retrieving the total count of stored points (strongly consistent).
- Retrieving counts of stored points per escape-time bucket (strongly consistent).
- Retrieving the total number of batches uploaded (strongly consistent).

### Data Model (Logical)

- `Point`
	- `real: double`
	- `imag: double`
	- `escapeTime: int32`

- `BatchMetadata`
	- `batchId: uuid` (client-supplied unique identifier for idempotency)
	- `machineDescription: string` (free-form)
	- `duration: timespan` (or total milliseconds as `int64`)
	- `pointsChecked: int32`
	- `pointsFound: int32`
	- `uploadedAtUtc: datetime`

### Non-Functional Requirements

- The system shall emphasize low operational cost on Azure (e.g., by favoring object storage and simple compute where possible).
- The system shall be operable at scales up to millions of stored points.
- The system shall ensure durability of uploaded points and metadata, resilient to worker cancellations or failures.
- The system shall not enforce per-identity or per-endpoint rate limits or request quotas.

### Assumptions

- The provided boundary file correctly enumerates regions that include the Mandelbrot boundary.
- Random number generation is sufficient to distribute sampling across regions.
- Manual boundary file updates, if required, will be handled outside the system’s interfaces.

### Out of Scope

- Interactive or real-time coordination among workers for region selection.
- Deduplication of points across workers beyond implicit statistical rarity of duplicates.
- Parsing or validation of the free-form `machineDescription` beyond storage and retrieval.
