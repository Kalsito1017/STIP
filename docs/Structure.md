sofia-transport/
│
├── backend/
│   ├── SofiaTransport.Api/            # ASP.NET Core Web API (entry point)
│   ├── SofiaTransport.Application/    # Use-cases, CQRS handlers, DTOs
│   ├── SofiaTransport.Domain/         # Entities, value objects, domain events
│   ├── SofiaTransport.Infrastructure/ # EF Core, PostGIS, GTFS client, SignalR hub
│   └── SofiaTransport.Worker/         # Background service – GTFS polling
│
├── ml/
│   ├── app/                           # Python package
│   │   ├── main.py                    # Inference server (FastAPI)
│   │   ├── train.py                   # Training pipeline
│   │   └── db.py                      # Database utilities
│   ├── scripts/                       # Utility scripts (GTFS seed generator)
│   ├── data/                          # Raw & processed GTFS data (gitignored)
│   ├── notebooks/                     # Exploratory analysis
│   ├── models/                        # Saved model artefacts (.pkl / .joblib)
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
│   ├── migrations/                    # SQL migration scripts
│   └── seed/                         # Static GTFS seed scripts
│
├── tests/
│   ├── SofiaTransport.Api.Tests/
│   ├── SofiaTransport.Application.Tests/
│   ├── SofiaTransport.Domain.Tests/
│   ├── SofiaTransport.Infrastructure.Tests/
│   ├── frontend/                     # Vitest unit tests
│   └── e2e/                          # Playwright E2E tests
│
└── docs/
    ├── HighLevelVision.md
    ├── Idea.md
    ├── RUNNING.md
    ├── SOFIA_TRANSPORT_MASTER_PLAN.md
    └── Structure.md
