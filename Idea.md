Sofia Transport Intelligence Platform
A real-time + analytics platform that tracks, analyzes, and predicts public transport performance in entity["city","Sofia","capital of Bulgaria"].
🧠 Core Idea
Not just “show buses on a map” — that’s basic.
Instead, build:
A system that answers:
“How reliable is public transport in Sofia, and what will happen next?”
🔥 Key Features (what makes it stand out)
1. Real-Time Tracking
Live map with:
Buses, trams, metro
Current positions (GTFS realtime)
Filter by line / type
2. Delay Intelligence Dashboard
Average delay per line
Heatmap of problem areas
Peak hours analysis
👉 Example insight:
“Bus 204 is consistently late between 17:00–19:00”
3. Predictive Model (THIS is your wow factor)
Predict:
Arrival delays
Travel time
Use:
Time of day
Route
Historical delays
Model ideas:
Linear Regression (start)
XGBoost (upgrade)
4. Reliability Score System
Create your own metric:

Reliability Score = On-time % - Avg Delay Penalty
Rank:
Best lines
Worst lines
5. Stop Analytics
Most used stops
Congestion by hour
Transfer hubs
6. API Layer (important for backend credibility)
Build your own API:

GET /lines/{id}/reliability
GET /stops/{id}/predicted-arrivals
GET /heatmap/delays
🧱 Suggested Architecture (perfect for your stack)
Backend
ASP.NET Core (clean architecture)
Background jobs:
ingest GTFS realtime every X seconds
Data Processing
Python (pandas, scikit-learn)
Scheduled training jobs
Database
PostgreSQL + PostGIS
Tables:
vehicles
stops
trips
delay_logs
Frontend
React
Map:
Leaflet or Mapbox
Optional
Power BI dashboard (quick insights)
📊 Dataset Sources
Use:
Sofia GTFS (static + realtime)
Transport API (arrival times)