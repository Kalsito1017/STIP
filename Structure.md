sofia-transport/
│
├── src/
│   ├── SofiaTransport.Api/            # ASP.NET Core Web API (entry point)
│   ├── SofiaTransport.Application/    # Use-cases, CQRS handlers, DTOs
│   ├── SofiaTransport.Domain/         # Entities, value objects, domain events
│   ├── SofiaTransport.Infrastructure/ # EF Core, PostGIS, GTFS client, SignalR hub
│   └── SofiaTransport.Worker/         # Background service – GTFS polling
│
├── ml/
│   ├── data/                          # Raw & processed GTFS data
│   ├── notebooks/                     # Exploratory analysis
│   ├── models/                        # Saved model artefacts (.pkl / .joblib)
│   ├── train.py                       # Training pipeline
│   ├── predict.py                     # Inference server (FastAPI)
│   └── requirements.txt
│
├── frontend/
│   ├── src/
│   │   ├── components/                # Reusable UI
│   │   ├── pages/                     # Dashboard, Map, Lines, Stops, Predictions
│   │   ├── hooks/                     # useRealtime, useDelays, usePrediction
│   │   ├── services/                  # Axios API clients
│   │   └── store/                     # Zustand global state
│   └── package.json
│
├── docker/
│   ├── docker-compose.yml
│   ├── Dockerfile.api
│   ├── Dockerfile.worker
│   └── Dockerfile.ml
│
├── database/
│   ├── migrations/                    # EF Core migrations
│   └── seed/                         # Static GTFS seed scripts
│
└── docs/
    ├── architecture.md
    └── api-spec.yaml                  # OpenAPI 3.0