# OpenOCW

An open-source, institution-agnostic open courseware platform.  Institutions can
publish courses, syllabi, and lecture materials; students can browse, search,
enrol, and submit assignments.

---

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [MongoDB 7+](https://www.mongodb.com/try/download/community)

### Run (development)

```bash
# 1. Start MongoDB
mongod --dbpath ./data

# 2. Start the backend
cd Oocw.Backend
dotnet run

# 3. Start the frontend (separate terminal)
cd Oocw.Frontend
npm install
npm run dev
```

The API is available at `https://localhost:5051` and the frontend dev server at
`http://localhost:5040` (proxies `/api` to the backend).

### Build (Linux)

```bash
./build-linux.sh
```

---

## Project Structure

```
OpenOCW/
├── Oocw.Backend/       ASP.NET Core 8 REST API
├── Oocw.Database/      MongoDB models and data access
├── Oocw.Base/          Shared utilities
├── Oocw.Frontend/      Vue 3 SPA (Vite, TypeScript)
├── Deprecated/         Institution-specific code (not built)
│   ├── Oocw.Crawler/   Selenium scraper (Tokyo Tech OCW)
│   └── Oocw.Cli/       Import CLI (Tokyo Tech)
├── docs/               Documentation
│   ├── architecture.md System architecture
│   └── roadmap.md      Planned features
└── meta/               Static metadata files
```

See [`docs/architecture.md`](docs/architecture.md) for a full description of each
layer, data models, and deployment topology.

---

## Configuration

Backend configuration lives in `Oocw.Backend/appsettings.json`:

```json
{
  "Database": {
    "ConnectionHost": "mongodb://localhost:27017/"
  },
  "JWT": {
    "Secret": "<change-me>"
  }
}
```

Override values in `appsettings.Development.json` for local development.

---

## Roadmap

See [`docs/roadmap.md`](docs/roadmap.md) for the planned feature phases.

---

## Contributing

1. Fork the repository and create a feature branch.
2. Make your changes and run `dotnet build OpenOcw.sln` to verify.
3. Open a pull request with a clear description of what changed and why.

---

## License

[MIT](LICENSE)

---

## Acknowledgements

- [stopwords-iso/stopwords-ja](https://github.com/stopwords-iso/stopwords-ja)
- [stopwords-iso/stopwords-zh](https://github.com/stopwords-iso/stopwords-zh)
