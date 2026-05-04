# Graph Report - STIP  (2026-05-04)

## Corpus Check
- 229 files · ~55,379 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 905 nodes · 774 edges · 105 communities detected
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 6 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Community 0|Community 0]]
- [[_COMMUNITY_Community 1|Community 1]]
- [[_COMMUNITY_Community 2|Community 2]]
- [[_COMMUNITY_Community 3|Community 3]]
- [[_COMMUNITY_Community 4|Community 4]]
- [[_COMMUNITY_Community 5|Community 5]]
- [[_COMMUNITY_Community 6|Community 6]]
- [[_COMMUNITY_Community 7|Community 7]]
- [[_COMMUNITY_Community 8|Community 8]]
- [[_COMMUNITY_Community 9|Community 9]]
- [[_COMMUNITY_Community 10|Community 10]]
- [[_COMMUNITY_Community 11|Community 11]]
- [[_COMMUNITY_Community 12|Community 12]]
- [[_COMMUNITY_Community 13|Community 13]]
- [[_COMMUNITY_Community 14|Community 14]]
- [[_COMMUNITY_Community 15|Community 15]]
- [[_COMMUNITY_Community 16|Community 16]]
- [[_COMMUNITY_Community 17|Community 17]]
- [[_COMMUNITY_Community 18|Community 18]]
- [[_COMMUNITY_Community 19|Community 19]]
- [[_COMMUNITY_Community 20|Community 20]]
- [[_COMMUNITY_Community 21|Community 21]]
- [[_COMMUNITY_Community 22|Community 22]]
- [[_COMMUNITY_Community 23|Community 23]]
- [[_COMMUNITY_Community 24|Community 24]]
- [[_COMMUNITY_Community 25|Community 25]]
- [[_COMMUNITY_Community 26|Community 26]]
- [[_COMMUNITY_Community 27|Community 27]]
- [[_COMMUNITY_Community 28|Community 28]]
- [[_COMMUNITY_Community 29|Community 29]]
- [[_COMMUNITY_Community 30|Community 30]]
- [[_COMMUNITY_Community 31|Community 31]]
- [[_COMMUNITY_Community 32|Community 32]]
- [[_COMMUNITY_Community 33|Community 33]]
- [[_COMMUNITY_Community 34|Community 34]]
- [[_COMMUNITY_Community 35|Community 35]]
- [[_COMMUNITY_Community 36|Community 36]]
- [[_COMMUNITY_Community 37|Community 37]]
- [[_COMMUNITY_Community 38|Community 38]]
- [[_COMMUNITY_Community 39|Community 39]]
- [[_COMMUNITY_Community 40|Community 40]]
- [[_COMMUNITY_Community 41|Community 41]]
- [[_COMMUNITY_Community 42|Community 42]]
- [[_COMMUNITY_Community 43|Community 43]]
- [[_COMMUNITY_Community 44|Community 44]]
- [[_COMMUNITY_Community 45|Community 45]]
- [[_COMMUNITY_Community 46|Community 46]]
- [[_COMMUNITY_Community 47|Community 47]]
- [[_COMMUNITY_Community 48|Community 48]]
- [[_COMMUNITY_Community 49|Community 49]]
- [[_COMMUNITY_Community 50|Community 50]]
- [[_COMMUNITY_Community 51|Community 51]]
- [[_COMMUNITY_Community 52|Community 52]]
- [[_COMMUNITY_Community 53|Community 53]]
- [[_COMMUNITY_Community 54|Community 54]]
- [[_COMMUNITY_Community 55|Community 55]]
- [[_COMMUNITY_Community 56|Community 56]]
- [[_COMMUNITY_Community 57|Community 57]]
- [[_COMMUNITY_Community 58|Community 58]]
- [[_COMMUNITY_Community 60|Community 60]]
- [[_COMMUNITY_Community 61|Community 61]]
- [[_COMMUNITY_Community 62|Community 62]]
- [[_COMMUNITY_Community 63|Community 63]]
- [[_COMMUNITY_Community 64|Community 64]]
- [[_COMMUNITY_Community 65|Community 65]]
- [[_COMMUNITY_Community 66|Community 66]]
- [[_COMMUNITY_Community 67|Community 67]]
- [[_COMMUNITY_Community 68|Community 68]]
- [[_COMMUNITY_Community 69|Community 69]]
- [[_COMMUNITY_Community 70|Community 70]]
- [[_COMMUNITY_Community 71|Community 71]]
- [[_COMMUNITY_Community 72|Community 72]]
- [[_COMMUNITY_Community 73|Community 73]]
- [[_COMMUNITY_Community 77|Community 77]]
- [[_COMMUNITY_Community 79|Community 79]]
- [[_COMMUNITY_Community 80|Community 80]]
- [[_COMMUNITY_Community 81|Community 81]]
- [[_COMMUNITY_Community 82|Community 82]]
- [[_COMMUNITY_Community 83|Community 83]]
- [[_COMMUNITY_Community 84|Community 84]]
- [[_COMMUNITY_Community 85|Community 85]]
- [[_COMMUNITY_Community 86|Community 86]]
- [[_COMMUNITY_Community 87|Community 87]]
- [[_COMMUNITY_Community 88|Community 88]]
- [[_COMMUNITY_Community 89|Community 89]]
- [[_COMMUNITY_Community 90|Community 90]]
- [[_COMMUNITY_Community 91|Community 91]]
- [[_COMMUNITY_Community 92|Community 92]]
- [[_COMMUNITY_Community 93|Community 93]]
- [[_COMMUNITY_Community 98|Community 98]]
- [[_COMMUNITY_Community 99|Community 99]]
- [[_COMMUNITY_Community 100|Community 100]]
- [[_COMMUNITY_Community 101|Community 101]]
- [[_COMMUNITY_Community 102|Community 102]]
- [[_COMMUNITY_Community 103|Community 103]]
- [[_COMMUNITY_Community 104|Community 104]]
- [[_COMMUNITY_Community 105|Community 105]]
- [[_COMMUNITY_Community 106|Community 106]]
- [[_COMMUNITY_Community 107|Community 107]]
- [[_COMMUNITY_Community 172|Community 172]]
- [[_COMMUNITY_Community 173|Community 173]]
- [[_COMMUNITY_Community 174|Community 174]]
- [[_COMMUNITY_Community 175|Community 175]]
- [[_COMMUNITY_Community 176|Community 176]]
- [[_COMMUNITY_Community 177|Community 177]]

## God Nodes (most connected - your core abstractions)
1. `CoordinatesEdgeCasesTests` - 17 edges
2. `DelayLogRepository` - 12 edges
3. `ReliabilityScoreRepository` - 12 edges
4. `RoutesControllerTests` - 11 edges
5. `RouteRepository` - 10 edges
6. `VehicleRepository` - 10 edges
7. `GtfsPollingService` - 10 edges
8. `RoutesController` - 9 edges
9. `StopRepository` - 9 edges
10. `AnalyticsControllerTests` - 9 edges

## Surprising Connections (you probably didn't know these)
- `get_connection_params()` --calls--> `_get_db_pool()`  [INFERRED]
  ml/db.py → ml/predict.py
- `AnimatedStat()` --calls--> `useCountUp()`  [INFERRED]
  frontend/src/components/AnimatedStat.tsx → frontend/src/hooks/useCountUp.ts
- `PredictPanel()` --calls--> `useDelayPrediction()`  [INFERRED]
  frontend/src/components/PredictPanel.tsx → frontend/src/hooks/usePrediction.ts
- `PredictPanel()` --calls--> `useStops()`  [INFERRED]
  frontend/src/components/PredictPanel.tsx → frontend/src/hooks/useStops.ts
- `useAllRouteShapes()` --calls--> `FitBoundsOnShapes()`  [INFERRED]
  frontend/src/hooks/useRouteShapes.ts → frontend/src/pages/LiveMapPage.tsx

## Communities

### Community 0 - "Community 0"
Cohesion: 0.03
Nodes (24): GetActiveAlertsHandler, GetDelayHeatmapHandler, GetPeakHoursHandler, GetReliabilityRankingHandler, GetSystemOverviewHandler, IRequestHandler, PredictDelayHandler, PredictTravelTimeHandler (+16 more)

### Community 1 - "Community 1"
Cohesion: 0.05
Nodes (9): ControllerBase, AlertsController, AnalyticsController, AuthController, PredictionsController, RoutesController, StopsController, TripUpdatesController (+1 more)

### Community 2 - "Community 2"
Cohesion: 0.06
Nodes (16): AbstractValidator, GetDelayHeatmapValidator, GetReliabilityRankingValidator, PredictDelayValidator, PredictTravelTimeValidator, GetRouteDelayPatternValidator, GetRouteDetailValidator, GetRouteReliabilityHistoryValidator (+8 more)

### Community 3 - "Community 3"
Cohesion: 0.09
Nodes (13): Alert, EntitySelector, FeedEntity, FeedMessage, Position, StopTimeEvent, StopTimeEventUpdate, TimeRange (+5 more)

### Community 4 - "Community 4"
Cohesion: 0.08
Nodes (6): IDelayLogRepository, IReliabilityScoreRepository, IRouteRepository, IStopRepository, IVehicleRepository, IRepository

### Community 5 - "Community 5"
Cohesion: 0.11
Nodes (22): create_connection(), get_connection_params(), parse_db_connection_string(), Shared database utilities for the STIP ML service., Convert .NET-style connection string to psycopg2 kwargs.      Input:  'Host=post, Parse DB_CONNECTION_STRING env var into psycopg2 kwargs., Create a psycopg2 connection from DB_CONNECTION_STRING env var., build_route_encoding() (+14 more)

### Community 6 - "Community 6"
Cohesion: 0.1
Nodes (9): Migration, InitialCreate, SofiaTransport.Infrastructure.Persistence.Migrations, AddPerformanceIndicesAndSeedData, SofiaTransport.Infrastructure.Persistence.Migrations, AddUsers, SofiaTransport.Infrastructure.Persistence.Migrations, AddShapes (+1 more)

### Community 7 - "Community 7"
Cohesion: 0.15
Nodes (18): BaseModel, _find_latest_model_path(), _get_db_pool(), lifespan(), load_model(), _lookup_historical_avg_delay(), predict(), PredictionRequest (+10 more)

### Community 8 - "Community 8"
Cohesion: 0.11
Nodes (1): CoordinatesEdgeCasesTests

### Community 9 - "Community 9"
Cohesion: 0.15
Nodes (2): IDelayLogRepository, DelayLogRepository

### Community 10 - "Community 10"
Cohesion: 0.15
Nodes (2): IReliabilityScoreRepository, ReliabilityScoreRepository

### Community 11 - "Community 11"
Cohesion: 0.17
Nodes (1): RoutesControllerTests

### Community 12 - "Community 12"
Cohesion: 0.18
Nodes (2): IRouteRepository, RouteRepository

### Community 13 - "Community 13"
Cohesion: 0.18
Nodes (2): IVehicleRepository, VehicleRepository

### Community 14 - "Community 14"
Cohesion: 0.31
Nodes (2): BackgroundService, GtfsPollingService

### Community 15 - "Community 15"
Cohesion: 0.2
Nodes (2): IStopRepository, StopRepository

### Community 16 - "Community 16"
Cohesion: 0.2
Nodes (1): AnalyticsControllerTests

### Community 17 - "Community 17"
Cohesion: 0.2
Nodes (1): StopsControllerTests

### Community 18 - "Community 18"
Cohesion: 0.2
Nodes (1): DelayLogEdgeCasesTests

### Community 19 - "Community 19"
Cohesion: 0.31
Nodes (2): RedisVehicleCache, IVehicleCache

### Community 20 - "Community 20"
Cohesion: 0.22
Nodes (1): VehicleTests

### Community 21 - "Community 21"
Cohesion: 0.25
Nodes (1): IRepository

### Community 22 - "Community 22"
Cohesion: 0.32
Nodes (2): RedisAlertCache, IAlertCache

### Community 23 - "Community 23"
Cohesion: 0.32
Nodes (2): RedisTripUpdateCache, ITripUpdateCache

### Community 24 - "Community 24"
Cohesion: 0.25
Nodes (1): DelayLogTests

### Community 25 - "Community 25"
Cohesion: 0.25
Nodes (1): RouteTests

### Community 26 - "Community 26"
Cohesion: 0.25
Nodes (1): StopTimeTests

### Community 27 - "Community 27"
Cohesion: 0.25
Nodes (1): CoordinatesExtendedTests

### Community 28 - "Community 28"
Cohesion: 0.25
Nodes (1): CoordinatesTests

### Community 29 - "Community 29"
Cohesion: 0.33
Nodes (3): RateLimitEntry, RateLimitingMiddleware, RateLimitingMiddlewareExtensions

### Community 30 - "Community 30"
Cohesion: 0.29
Nodes (1): IVehicleCache

### Community 31 - "Community 31"
Cohesion: 0.33
Nodes (2): IRealtimeBroadcaster, RealtimeBroadcaster

### Community 32 - "Community 32"
Cohesion: 0.29
Nodes (2): Hub, VehicleHub

### Community 33 - "Community 33"
Cohesion: 0.29
Nodes (3): IJob, DelayAggregationJob, MlRetrainTriggerJob

### Community 34 - "Community 34"
Cohesion: 0.29
Nodes (3): PredictPanel(), useDelayPrediction(), useStops()

### Community 35 - "Community 35"
Cohesion: 0.29
Nodes (2): useAllRouteShapes(), FitBoundsOnShapes()

### Community 36 - "Community 36"
Cohesion: 0.29
Nodes (1): StopTests

### Community 37 - "Community 37"
Cohesion: 0.29
Nodes (1): TripTests

### Community 38 - "Community 38"
Cohesion: 0.33
Nodes (1): IAlertCache

### Community 39 - "Community 39"
Cohesion: 0.33
Nodes (1): ITripUpdateCache

### Community 40 - "Community 40"
Cohesion: 0.33
Nodes (2): IStopTimeRepository, StopTimeRepository

### Community 41 - "Community 41"
Cohesion: 0.33
Nodes (2): IUserRepository, UserRepository

### Community 42 - "Community 42"
Cohesion: 0.33
Nodes (1): PredictionsControllerTests

### Community 43 - "Community 43"
Cohesion: 0.33
Nodes (1): GetDelayHeatmapHandlerTests

### Community 44 - "Community 44"
Cohesion: 0.33
Nodes (1): GetReliabilityRankingHandlerTests

### Community 45 - "Community 45"
Cohesion: 0.33
Nodes (1): GetRouteDelayPatternHandlerTests

### Community 46 - "Community 46"
Cohesion: 0.33
Nodes (1): GetRouteDetailHandlerTests

### Community 47 - "Community 47"
Cohesion: 0.33
Nodes (2): DelayBucketTests, TransitTypeTests

### Community 48 - "Community 48"
Cohesion: 0.4
Nodes (2): ExceptionHandlingMiddleware, ExceptionHandlingMiddlewareExtensions

### Community 49 - "Community 49"
Cohesion: 0.4
Nodes (2): SecurityHeadersMiddleware, SecurityHeadersMiddlewareExtensions

### Community 50 - "Community 50"
Cohesion: 0.4
Nodes (1): IStopTimeRepository

### Community 51 - "Community 51"
Cohesion: 0.4
Nodes (1): IUserRepository

### Community 52 - "Community 52"
Cohesion: 0.5
Nodes (2): AlertFeedClient, IAlertFeedClient

### Community 53 - "Community 53"
Cohesion: 0.5
Nodes (2): GtfsFeedClient, IGtfsFeedClient

### Community 54 - "Community 54"
Cohesion: 0.5
Nodes (2): TripUpdateFeedClient, ITripUpdateFeedClient

### Community 55 - "Community 55"
Cohesion: 0.4
Nodes (2): IMLService, MLService

### Community 56 - "Community 56"
Cohesion: 0.4
Nodes (3): SofiaTransport.Infrastructure.Persistence.Migrations, TransportDbContextModelSnapshot, ModelSnapshot

### Community 57 - "Community 57"
Cohesion: 0.4
Nodes (2): IShapeRepository, ShapeRepository

### Community 58 - "Community 58"
Cohesion: 0.5
Nodes (2): IVehicleBroadcaster, VehicleBroadcaster

### Community 60 - "Community 60"
Cohesion: 0.4
Nodes (1): ExceptionHandlingMiddlewareTests

### Community 61 - "Community 61"
Cohesion: 0.4
Nodes (1): GetPeakHoursHandlerTests

### Community 62 - "Community 62"
Cohesion: 0.4
Nodes (1): GetRouteReliabilityHistoryHandlerTests

### Community 63 - "Community 63"
Cohesion: 0.4
Nodes (1): GetStopCongestionHandlerTests

### Community 64 - "Community 64"
Cohesion: 0.4
Nodes (1): GetLiveVehiclesHandlerTests

### Community 65 - "Community 65"
Cohesion: 0.4
Nodes (1): ReliabilityScoreTests

### Community 66 - "Community 66"
Cohesion: 0.5
Nodes (2): ValidationBehavior, IPipelineBehavior

### Community 67 - "Community 67"
Cohesion: 0.5
Nodes (1): IMLService

### Community 68 - "Community 68"
Cohesion: 0.5
Nodes (1): IShapeRepository

### Community 69 - "Community 69"
Cohesion: 0.5
Nodes (2): DbContext, TransportDbContext

### Community 70 - "Community 70"
Cohesion: 0.5
Nodes (2): InitialCreate, SofiaTransport.Infrastructure.Persistence.Migrations

### Community 71 - "Community 71"
Cohesion: 0.5
Nodes (2): AddPerformanceIndicesAndSeedData, SofiaTransport.Infrastructure.Persistence.Migrations

### Community 72 - "Community 72"
Cohesion: 0.5
Nodes (2): AddUsers, SofiaTransport.Infrastructure.Persistence.Migrations

### Community 73 - "Community 73"
Cohesion: 0.5
Nodes (2): AddShapes, SofiaTransport.Infrastructure.Persistence.Migrations

### Community 77 - "Community 77"
Cohesion: 0.5
Nodes (2): AnimatedStat(), useCountUp()

### Community 79 - "Community 79"
Cohesion: 0.5
Nodes (1): VehiclesControllerTests

### Community 80 - "Community 80"
Cohesion: 0.5
Nodes (1): SecurityHeadersMiddlewareTests

### Community 81 - "Community 81"
Cohesion: 0.5
Nodes (1): GetSystemOverviewHandlerTests

### Community 82 - "Community 82"
Cohesion: 0.5
Nodes (1): PredictDelayHandlerTests

### Community 83 - "Community 83"
Cohesion: 0.5
Nodes (1): PredictTravelTimeHandlerTests

### Community 84 - "Community 84"
Cohesion: 0.5
Nodes (1): GetRoutesHandlerTests

### Community 85 - "Community 85"
Cohesion: 0.5
Nodes (1): GetNearbyStopsHandlerTests

### Community 86 - "Community 86"
Cohesion: 0.5
Nodes (1): GetPredictedArrivalsHandlerTests

### Community 87 - "Community 87"
Cohesion: 0.67
Nodes (1): ApiServiceRegistration

### Community 88 - "Community 88"
Cohesion: 0.67
Nodes (1): IAlertFeedClient

### Community 89 - "Community 89"
Cohesion: 0.67
Nodes (1): IGtfsFeedClient

### Community 90 - "Community 90"
Cohesion: 0.67
Nodes (1): ITripUpdateFeedClient

### Community 91 - "Community 91"
Cohesion: 0.67
Nodes (1): ReliabilityScore

### Community 92 - "Community 92"
Cohesion: 0.67
Nodes (2): StopTimeUpdate, TripUpdate

### Community 93 - "Community 93"
Cohesion: 0.67
Nodes (1): InfrastructureServiceRegistration

### Community 98 - "Community 98"
Cohesion: 0.67
Nodes (1): GetStopsHandlerTests

### Community 99 - "Community 99"
Cohesion: 1.0
Nodes (1): DelayLog

### Community 100 - "Community 100"
Cohesion: 1.0
Nodes (1): Route

### Community 101 - "Community 101"
Cohesion: 1.0
Nodes (1): ServiceAlert

### Community 102 - "Community 102"
Cohesion: 1.0
Nodes (1): Shape

### Community 103 - "Community 103"
Cohesion: 1.0
Nodes (1): Stop

### Community 104 - "Community 104"
Cohesion: 1.0
Nodes (1): StopTime

### Community 105 - "Community 105"
Cohesion: 1.0
Nodes (1): Trip

### Community 106 - "Community 106"
Cohesion: 1.0
Nodes (1): User

### Community 107 - "Community 107"
Cohesion: 1.0
Nodes (1): Vehicle

### Community 172 - "Community 172"
Cohesion: 1.0
Nodes (1): Validate that a model file path is within MODEL_DIR (prevent path traversal).

### Community 173 - "Community 173"
Cohesion: 1.0
Nodes (1): Find the latest versioned model file in MODEL_DIR.      Strategy:     1. Read mo

### Community 174 - "Community 174"
Cohesion: 1.0
Nodes (1): Load the latest model and metadata into module-level globals.

### Community 175 - "Community 175"
Cohesion: 1.0
Nodes (1): Get or create a threaded connection pool.

### Community 176 - "Community 176"
Cohesion: 1.0
Nodes (1): Query PostgreSQL for the 7-day rolling average delay for a route+hour.      Retu

### Community 177 - "Community 177"
Cohesion: 1.0
Nodes (1): Remove old .joblib model files, keeping the *keep* most recent.

## Knowledge Gaps
- **45 isolated node(s):** `RateLimitEntry`, `DelayLog`, `Route`, `ServiceAlert`, `Shape` (+40 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **Thin community `Community 8`** (18 nodes): `CoordinatesEdgeCasesTests.cs`, `CoordinatesEdgeCasesTests`, `.Constructor_ExactlyAtLatBoundary_Works()`, `.Constructor_ExactlyAtLonBoundary_Works()`, `.Constructor_ExactlyOnLowerLatBoundary_Works()`, `.Constructor_ExactlyOnLowerLonBoundary_Works()`, `.Constructor_ExactlyOnUpperLatBoundary_Works()`, `.Constructor_ExactlyOnUpperLonBoundary_Works()`, `.Constructor_JustBelowLatBoundary_Throws()`, `.Constructor_JustBelowLonBoundary_Throws()`, `.Equality_SameValues_DifferentInstances_AreEqual()`, `.InequalityOperator_DifferentCoordinates_ReturnsTrue()`, `.InequalityOperator_SameCoordinates_ReturnsFalse()`, `.Lat_VariousValidValues_Work()`, `.LatExceptionMessage_ContainsParamName()`, `.Lon_VariousValidValues_Work()`, `.LonExceptionMessage_ContainsParamName()`, `.ToString_Returns6DecimalPlaces()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 9`** (13 nodes): `DelayLogRepository.cs`, `IDelayLogRepository`, `DelayLogRepository`, `.AddAsync()`, `.DeleteAsync()`, `.GetAllAsync()`, `.GetByDateAsync()`, `.GetByIdAsync()`, `.GetByRouteAsync()`, `.GetByStopAsync()`, `.GetCountAsync()`, `.GetForHeatmapAsync()`, `.UpdateAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 10`** (13 nodes): `ReliabilityScoreRepository.cs`, `IReliabilityScoreRepository`, `ReliabilityScoreRepository`, `.AddAsync()`, `.DeleteAsync()`, `.GetAllAsync()`, `.GetByIdAsync()`, `.GetByRouteAndDateAsync()`, `.GetByRouteAsync()`, `.GetCountAsync()`, `.GetLatestByRouteAsync()`, `.GetRankingAsync()`, `.UpdateAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 11`** (12 nodes): `RoutesControllerTests`, `.GetAll_ReturnsOk()`, `.GetAll_WithTypeFilter_PassesType()`, `.GetById_ReturnsNotFound_WhenRouteIsNull()`, `.GetById_ReturnsOk_WhenRouteExists()`, `.GetDelayPattern_PassesDateParam()`, `.GetDelayPattern_ReturnsOk()`, `.GetReliability_ReturnsNotFound_WhenRouteIsNull()`, `.GetReliability_ReturnsOk_WhenRouteExists()`, `.GetReliabilityHistory_PassesDateParams()`, `.GetReliabilityHistory_ReturnsOk()`, `RoutesControllerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 12`** (11 nodes): `RouteRepository.cs`, `IRouteRepository`, `RouteRepository`, `.AddAsync()`, `.DeleteAsync()`, `.GetAllAsync()`, `.GetByIdAsync()`, `.GetByShortNameAsync()`, `.GetByTypeAsync()`, `.GetCountAsync()`, `.UpdateAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 13`** (11 nodes): `VehicleRepository.cs`, `IVehicleRepository`, `VehicleRepository`, `.AddAsync()`, `.DeleteAsync()`, `.GetAllAsync()`, `.GetByIdAsync()`, `.GetByRouteAsync()`, `.GetCountAsync()`, `.GetLiveAsync()`, `.UpdateAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 14`** (11 nodes): `GtfsPollingService.cs`, `BackgroundService`, `GtfsPollingService`, `.CleanupStaleVehiclesAsync()`, `.ExecuteAsync()`, `.FindNearestStopTimeAsync()`, `.PollAlertsAsync()`, `.PollTripUpdatesAsync()`, `.PollVehiclesAsync()`, `.ShouldLogDelayAsync()`, `.WriteDelayLogAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 15`** (10 nodes): `StopRepository.cs`, `IStopRepository`, `StopRepository`, `.AddAsync()`, `.DeleteAsync()`, `.GetAllAsync()`, `.GetByIdAsync()`, `.GetCountAsync()`, `.GetNearbyAsync()`, `.UpdateAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 16`** (10 nodes): `AnalyticsControllerTests`, `.GetDelayHeatmap_PassesFromAndToQueryParams()`, `.GetDelayHeatmap_ReturnsOkWithHeatmapData()`, `.GetOverview_ReturnsOkWithOverview()`, `.GetPeakHours_ReturnsOkWithData()`, `.GetPeakHours_WithoutDateParam_PassesNull()`, `.GetReliabilityRanking_CustomParams_Top5BestFalse()`, `.GetReliabilityRanking_DefaultParams_Top10BestTrue()`, `.GetReliabilityRanking_ReturnsOkWithRankings()`, `AnalyticsControllerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 17`** (10 nodes): `StopsControllerTests`, `.GetAll_ReturnsOk()`, `.GetById_ReturnsNotFound_WhenStopIsNull()`, `.GetById_ReturnsOk_WhenStopExists()`, `.GetCongestion_PassesDateParam()`, `.GetCongestion_ReturnsOk()`, `.GetNearby_PassesQueryParams()`, `.GetNearby_ReturnsOk()`, `.GetPredictedArrivals_ReturnsOk()`, `StopsControllerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 18`** (10 nodes): `DelayLogEdgeCasesTests`, `.DelaySeconds_CanBeExtreme()`, `.DelaySeconds_CanBeNegative_EarlyArrival()`, `.DelaySeconds_CanBeVeryLarge_HoursOfDelay()`, `.NullStopId_Allowed()`, `.NullTripId_Allowed()`, `.NullVehicleId_Allowed()`, `.RecordedAt_CanBeSetToUtcNow()`, `.RecordedAt_DefaultValue_IsDefaultDateTime()`, `DelayLogEdgeCasesTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 19`** (9 nodes): `RedisVehicleCache.cs`, `RedisVehicleCache`, `.Deserialize()`, `.GetAllAsync()`, `.GetAsync()`, `.GetByRouteAsync()`, `.RemoveAsync()`, `.SetAsync()`, `IVehicleCache`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 20`** (9 nodes): `VehicleTests`, `.Bearing_CanBeNegative()`, `.Bearing_CanExceed360()`, `.Constructor_DefaultValues_AreSetCorrectly()`, `.Properties_CanBeSetAndGet()`, `.RouteId_CanBeNull()`, `.Speed_CanBeZero()`, `.TripId_CanBeNull()`, `VehicleTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 21`** (8 nodes): `IRepository.cs`, `IRepository`, `.AddAsync()`, `.DeleteAsync()`, `.GetAllAsync()`, `.GetByIdAsync()`, `.GetCountAsync()`, `.UpdateAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 22`** (8 nodes): `RedisAlertCache.cs`, `RedisAlertCache`, `.Deserialize()`, `.GetAllAsync()`, `.GetByRouteAsync()`, `.RemoveAsync()`, `.SetAsync()`, `IAlertCache`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 23`** (8 nodes): `RedisTripUpdateCache.cs`, `RedisTripUpdateCache`, `.Deserialize()`, `.GetAllAsync()`, `.GetByRouteAsync()`, `.RemoveAsync()`, `.SetAsync()`, `ITripUpdateCache`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 24`** (8 nodes): `DelayLogTests`, `.Constructor_DefaultValues_AreSetCorrectly()`, `.DelaySeconds_CanBeNegative_EarlyArrival()`, `.DelaySeconds_ZeroMeansOnTime()`, `.Id_UsesLong_PrimaryKey()`, `.NullableFields_CanAllBeNull()`, `.Properties_CanBeSetAndGet()`, `DelayLogTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 25`** (8 nodes): `RouteTests`, `.Constructor_DefaultValues_AreSetCorrectly()`, `.LongName_CanBeEmptyString()`, `.LongName_CanBeNull()`, `.Properties_CanBeSetAndGet()`, `.Trips_Collection_IsMutable()`, `.Type_SupportsAllTransitTypes()`, `RouteTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 26`** (8 nodes): `StopTimeTests`, `.ArrivalTime_CanBeLateNight()`, `.ArrivalTime_CanExceed24Hours()`, `.Constructor_DefaultValues_AreSetCorrectly()`, `.NavigationProperties_AreIndependent()`, `.Properties_CanBeSetAndGet()`, `.StopSequence_CanBeLargeNumber()`, `StopTimeTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 27`** (8 nodes): `CoordinatesExtendedTests.cs`, `CoordinatesExtendedTests`, `.DifferentLat_SameLon_AreNotEqual()`, `.Equality_SameTo15DecimalPlaces_AreEqual()`, `.Equality_VeryCloseButNotEqual_AreNotEqual()`, `.Equality_WithExactlySofiaCityCenter_Equal()`, `.SameLat_DifferentLon_AreNotEqual()`, `.SofiaCityCenterCoordinates_Valid()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 28`** (8 nodes): `CoordinatesTests.cs`, `CoordinatesTests`, `.Constructor_AtLatBoundary_Works()`, `.Constructor_InvalidCoordinates_ThrowsArgumentOutOfRange()`, `.Constructor_ValidSofiaCoordinates_CreatesInstance()`, `.Equality_DifferentCoordinates_AreNotEqual()`, `.Equality_TwoIdenticalCoordinates_AreEqual()`, `.ToString_ReturnsFormattedCoordinates()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 30`** (7 nodes): `IVehicleCache.cs`, `IVehicleCache`, `.GetAllAsync()`, `.GetAsync()`, `.GetByRouteAsync()`, `.RemoveAsync()`, `.SetAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 31`** (7 nodes): `RealtimeBroadcaster.cs`, `IRealtimeBroadcaster`, `.BroadcastAlertAsync()`, `.BroadcastTripUpdateAsync()`, `RealtimeBroadcaster`, `.BroadcastAlertAsync()`, `.BroadcastTripUpdateAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 32`** (7 nodes): `VehicleHub.cs`, `Hub`, `VehicleHub`, `.SubscribeToAlerts()`, `.SubscribeToRoute()`, `.UnsubscribeFromAlerts()`, `.UnsubscribeFromRoute()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 35`** (7 nodes): `useRouteShapes.ts`, `LiveMapPage.tsx`, `useAllRouteShapes()`, `useRouteShape()`, `FitBoundsOnShapes()`, `formatTimeAgo()`, `stopsToGeoJSON()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 36`** (7 nodes): `StopTests`, `.Constructor_DefaultValues_AreSetCorrectly()`, `.Location_UsesCoordinatesValueObject()`, `.Properties_CanBeSetAndGet()`, `.StopId_CanContainHyphens()`, `.StopName_SupportsBulgarianCharacters()`, `StopTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 37`** (7 nodes): `TripTests`, `.Constructor_DefaultValues_AreSetCorrectly()`, `.DirectionId_CanBeOne()`, `.DirectionId_CanBeZero()`, `.Properties_CanBeSetAndGet()`, `.StopTimes_Collection_IsMutable()`, `TripTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 38`** (6 nodes): `IAlertCache.cs`, `IAlertCache`, `.GetAllAsync()`, `.GetByRouteAsync()`, `.RemoveAsync()`, `.SetAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 39`** (6 nodes): `ITripUpdateCache.cs`, `ITripUpdateCache`, `.GetAllAsync()`, `.GetByRouteAsync()`, `.RemoveAsync()`, `.SetAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 40`** (6 nodes): `StopTimeRepository.cs`, `IStopTimeRepository`, `StopTimeRepository`, `.GetByStopAndRouteAsync()`, `.GetByTripAsync()`, `.GetUpcomingByStopAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 41`** (6 nodes): `UserRepository.cs`, `IUserRepository`, `UserRepository`, `.AddAsync()`, `.GetByEmailAsync()`, `.GetByIdAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 42`** (6 nodes): `PredictionsControllerTests`, `.PredictDelay_CorrectCommandIsSent_ForwardsParameters()`, `.PredictDelay_ValidRequest_ReturnsOkWithResponse()`, `.PredictTravelTime_CorrectCommandIsSent_ForwardsParameters()`, `.PredictTravelTime_ValidRequest_ReturnsOkWithResponse()`, `PredictionsControllerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 43`** (6 nodes): `GetDelayHeatmapHandlerTests`, `.Handle_LogsWithNullStopId_FilteredOut()`, `.Handle_LogsWithUnknownStop_FilteredOut()`, `.Handle_NoLogs_ReturnsEmptyList()`, `.Handle_WithLogs_ReturnsHeatmapPoints()`, `GetDelayHeatmapHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 44`** (6 nodes): `GetReliabilityRankingHandlerTests`, `.Handle_BestRanking_ReturnsTopBestRoutes()`, `.Handle_EmptyRanking_ReturnsEmptyList()`, `.Handle_RouteNotInRouteDict_UsesRouteIdAsShortName()`, `.Handle_WorstRanking_ReturnsTopWorstRoutes()`, `GetReliabilityRankingHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 45`** (6 nodes): `GetRouteDelayPatternHandlerTests`, `.Handle_MultipleHours_OrderedByHour()`, `.Handle_NoLogsForRoute_ReturnsEmptyList()`, `.Handle_SingleHour_ReturnsOneEntry()`, `.Handle_WithLogs_ReturnsHourlyDelayPattern()`, `GetRouteDelayPatternHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 46`** (6 nodes): `GetRouteDetailHandlerTests`, `.Handle_MultipleScores_PicksLatestByScoreDate()`, `.Handle_RouteExistsWithoutScores_ReturnsDetailWithNullReliability()`, `.Handle_RouteExistsWithScores_ReturnsDetailWithReliability()`, `.Handle_RouteNotFound_ReturnsNull()`, `GetRouteDetailHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 47`** (6 nodes): `DelayBucketTests`, `.DelayBucket_HasFourValues()`, `.DelayBucket_ValuesOrderCorrect()`, `TransitTypeTests`, `.TransitType_HasCorrectValues()`, `TransitTypeTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 48`** (5 nodes): `ExceptionHandlingMiddleware.cs`, `ExceptionHandlingMiddleware`, `.InvokeAsync()`, `ExceptionHandlingMiddlewareExtensions`, `.UseExceptionHandling()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 49`** (5 nodes): `SecurityHeadersMiddleware.cs`, `SecurityHeadersMiddleware`, `.InvokeAsync()`, `SecurityHeadersMiddlewareExtensions`, `.UseSecurityHeaders()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 50`** (5 nodes): `IStopTimeRepository.cs`, `IStopTimeRepository`, `.GetByStopAndRouteAsync()`, `.GetByTripAsync()`, `.GetUpcomingByStopAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 51`** (5 nodes): `IUserRepository.cs`, `IUserRepository`, `.AddAsync()`, `.GetByEmailAsync()`, `.GetByIdAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 52`** (5 nodes): `AlertFeedClient.cs`, `AlertFeedClient`, `.FetchAlertsAsync()`, `.ParseAlert()`, `IAlertFeedClient`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 53`** (5 nodes): `GtfsFeedClient.cs`, `GtfsFeedClient`, `.FetchVehiclePositionsAsync()`, `.ParseVehicle()`, `IGtfsFeedClient`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 54`** (5 nodes): `TripUpdateFeedClient.cs`, `TripUpdateFeedClient`, `.FetchTripUpdatesAsync()`, `.ParseTripUpdate()`, `ITripUpdateFeedClient`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 55`** (5 nodes): `MLService.cs`, `IMLService`, `MLService`, `.PredictDelayAsync()`, `.PredictTravelTimeAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 57`** (5 nodes): `ShapeRepository.cs`, `IShapeRepository`, `ShapeRepository`, `.GetAllGroupedByRouteAsync()`, `.GetByRouteIdAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 58`** (5 nodes): `VehicleBroadcaster.cs`, `IVehicleBroadcaster`, `.BroadcastAsync()`, `VehicleBroadcaster`, `.BroadcastAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 60`** (5 nodes): `ExceptionHandlingMiddlewareTests`, `.InvokeAsync_ExceptionIsCaught_Returns500WithJsonErrorBody()`, `.InvokeAsync_ExceptionIsLogged()`, `.InvokeAsync_NormalRequest_PassesThroughWithoutError()`, `ExceptionHandlingMiddlewareTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 61`** (5 nodes): `GetPeakHoursHandlerTests`, `.Handle_MultipleHours_OrderedByHour()`, `.Handle_NoLogs_ReturnsEmptyList()`, `.Handle_WithLogs_ReturnsPeakHourData()`, `GetPeakHoursHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 62`** (5 nodes): `GetRouteReliabilityHistoryHandlerTests`, `.Handle_DefaultDateRange_ReturnsLast30Days()`, `.Handle_NoScores_ReturnsEmptyList()`, `.Handle_ReturnsHistoryFilteredByDate()`, `GetRouteReliabilityHistoryHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 63`** (5 nodes): `GetStopCongestionHandlerTests`, `.Handle_MultipleHours_OrderedByHour()`, `.Handle_NoLogsForStop_ReturnsEmptyList()`, `.Handle_WithLogs_ReturnsHourlyCongestion()`, `GetStopCongestionHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 64`** (5 nodes): `GetLiveVehiclesHandlerTests.cs`, `GetLiveVehiclesHandlerTests`, `.Handle_NoRouteFilter_ReturnsAllVehicles()`, `.Handle_WithRouteFilter_ReturnsFilteredVehicles()`, `.Handle_WithUnknownRouteFilter_ReturnsEmpty()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 65`** (5 nodes): `ReliabilityScoreTests`, `.Calculate_ReturnsCorrectScore()`, `.Entity_PropertiesAreSetCorrectly()`, `.PenaltyFactor_IsFive()`, `ReliabilityScoreTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 66`** (4 nodes): `ValidationBehavior.cs`, `ValidationBehavior`, `.Handle()`, `IPipelineBehavior`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 67`** (4 nodes): `IMLService.cs`, `IMLService`, `.PredictDelayAsync()`, `.PredictTravelTimeAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 68`** (4 nodes): `IShapeRepository.cs`, `IShapeRepository`, `.GetAllGroupedByRouteAsync()`, `.GetByRouteIdAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 69`** (4 nodes): `TransportDbContext.cs`, `DbContext`, `TransportDbContext`, `.OnModelCreating()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 70`** (4 nodes): `20260428101743_InitialCreate.Designer.cs`, `InitialCreate`, `.BuildTargetModel()`, `SofiaTransport.Infrastructure.Persistence.Migrations`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 71`** (4 nodes): `20260428120126_AddPerformanceIndicesAndSeedData.Designer.cs`, `AddPerformanceIndicesAndSeedData`, `.BuildTargetModel()`, `SofiaTransport.Infrastructure.Persistence.Migrations`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 72`** (4 nodes): `20260428150000_AddUsers.Designer.cs`, `AddUsers`, `.BuildTargetModel()`, `SofiaTransport.Infrastructure.Persistence.Migrations`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 73`** (4 nodes): `20260428194826_AddShapes.Designer.cs`, `AddShapes`, `.BuildTargetModel()`, `SofiaTransport.Infrastructure.Persistence.Migrations`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 77`** (4 nodes): `AnimatedStat()`, `AnimatedStat.tsx`, `useCountUp.ts`, `useCountUp()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 79`** (4 nodes): `VehiclesControllerTests`, `.GetLive_PassesRouteIdFilter()`, `.GetLive_ReturnsVehiclesWithoutFilter()`, `VehiclesControllerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 80`** (4 nodes): `SecurityHeadersMiddlewareTests`, `.InvokeAsync_CallsNextDelegate()`, `.InvokeAsync_SetsAllFiveSecurityHeaders()`, `SecurityHeadersMiddlewareTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 81`** (4 nodes): `GetSystemOverviewHandlerTests`, `.Handle_NoVehicles_ReturnsZeroCount()`, `.Handle_ReturnsOverview()`, `GetSystemOverviewHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 82`** (4 nodes): `PredictDelayHandlerTests`, `.Handle_DelegatesToMLService_ReturnsResponse()`, `.Handle_PassesAllParametersCorrectly()`, `PredictDelayHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 83`** (4 nodes): `PredictTravelTimeHandlerTests`, `.Handle_NoMatchingTrips_ReturnsZeroPrediction()`, `.Handle_WithMatchingTrips_ReturnsHeuristicPrediction()`, `PredictTravelTimeHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 84`** (4 nodes): `GetRoutesHandlerTests`, `.Handle_EmptyRepository_ReturnsEmptyList()`, `.Handle_ReturnsAllRoutes()`, `GetRoutesHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 85`** (4 nodes): `GetNearbyStopsHandlerTests`, `.Handle_NoNearbyStops_ReturnsEmptyList()`, `.Handle_ReturnsNearbyStops()`, `GetNearbyStopsHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 86`** (4 nodes): `GetPredictedArrivalsHandlerTests`, `.Handle_NoUpcomingStopTimes_ReturnsEmptyList()`, `.Handle_ReturnsPredictedArrivals()`, `GetPredictedArrivalsHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 87`** (3 nodes): `ApiServiceRegistration.cs`, `ApiServiceRegistration`, `.AddApiServices()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 88`** (3 nodes): `IAlertFeedClient.cs`, `IAlertFeedClient`, `.FetchAlertsAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 89`** (3 nodes): `IGtfsFeedClient.cs`, `IGtfsFeedClient`, `.FetchVehiclePositionsAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 90`** (3 nodes): `ITripUpdateFeedClient.cs`, `ITripUpdateFeedClient`, `.FetchTripUpdatesAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 91`** (3 nodes): `ReliabilityScore.cs`, `ReliabilityScore`, `.Calculate()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 92`** (3 nodes): `TripUpdate.cs`, `StopTimeUpdate`, `TripUpdate`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 93`** (3 nodes): `InfrastructureServiceRegistration.cs`, `InfrastructureServiceRegistration`, `.AddInfrastructure()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 98`** (3 nodes): `GetStopsHandlerTests`, `.Handle_ReturnsAllStops()`, `GetStopsHandlerTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 99`** (2 nodes): `DelayLog.cs`, `DelayLog`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 100`** (2 nodes): `Route.cs`, `Route`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 101`** (2 nodes): `ServiceAlert.cs`, `ServiceAlert`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 102`** (2 nodes): `Shape.cs`, `Shape`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 103`** (2 nodes): `Stop.cs`, `Stop`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 104`** (2 nodes): `StopTime.cs`, `StopTime`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 105`** (2 nodes): `Trip.cs`, `Trip`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 106`** (2 nodes): `User.cs`, `User`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 107`** (2 nodes): `Vehicle.cs`, `Vehicle`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 172`** (1 nodes): `Validate that a model file path is within MODEL_DIR (prevent path traversal).`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 173`** (1 nodes): `Find the latest versioned model file in MODEL_DIR.      Strategy:     1. Read mo`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 174`** (1 nodes): `Load the latest model and metadata into module-level globals.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 175`** (1 nodes): `Get or create a threaded connection pool.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 176`** (1 nodes): `Query PostgreSQL for the 7-day rolling average delay for a route+hour.      Retu`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 177`** (1 nodes): `Remove old .joblib model files, keeping the *keep* most recent.`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `get_connection_params()` connect `Community 5` to `Community 7`?**
  _High betweenness centrality (0.001) - this node is a cross-community bridge._
- **Why does `_get_db_pool()` connect `Community 7` to `Community 5`?**
  _High betweenness centrality (0.001) - this node is a cross-community bridge._
- **What connects `RateLimitEntry`, `DelayLog`, `Route` to the rest of the system?**
  _45 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.03 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.05 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.06 - nodes in this community are weakly interconnected._
- **Should `Community 3` be split into smaller, more focused modules?**
  _Cohesion score 0.09 - nodes in this community are weakly interconnected._