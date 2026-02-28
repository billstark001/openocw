# Architecture

## Overview

OpenOCW is an open-source, institution-agnostic courseware platform.  It allows
institutions to publish courses, syllabi, and lecture materials, and lets students
browse, search, enrol, and submit assignments.

The system is divided into four main layers:

```
┌─────────────────────────────────────────────────┐
│                  Oocw.Frontend                  │  Vue 3 SPA / SSR
└────────────────────────┬────────────────────────┘
                         │ REST / JSON
┌────────────────────────▼────────────────────────┐
│                  Oocw.Backend                   │  ASP.NET Core 8
│  ┌──────────┐  ┌──────────┐  ┌──────────────┐  │
│  │   Auth   │  │  Course  │  │    Search    │  │
│  │ Controller│  │Controller│  │   Service    │  │
│  └──────────┘  └──────────┘  └──────────────┘  │
└────────────────────────┬────────────────────────┘
                         │ MongoDB Driver
┌────────────────────────▼────────────────────────┐
│                  Oocw.Database                  │  MongoDB 7+
│  Collections: Course · Class · ClassInstance   │
│               User · Notification              │
│               CourseDiscussion                  │
│               CourseSelection                   │
│               AssignmentSubmission              │
│               Organization · GlobalSettings    │
│               CourseRecord (search index)       │
└─────────────────────────────────────────────────┘
```

---

## Projects

### Oocw.Base
Shared utilities used across all C# projects:
- `FileUtils` – JSON serialisation helpers and file I/O.
- `NestedDictionary` – generic tree structure.
- Extension methods (`StringExtensions`, `WebStringUtils`, …).

### Oocw.Database
Contains all MongoDB model classes and the `OocwDatabase` context class.

**Key models**

| Class | Collection | Purpose |
|-------|-----------|---------|
| `Course` | `Course` | A course offering (title, code, departments, lecturers). |
| `Class` | `Class` | A named section of a course (e.g. "2024 Spring Section A"). |
| `ClassInstance` | `ClassInstance` | One semester run of a class, including lecture content. |
| `User` | `User` | Accounts with role-based access (Guest / Student / Faculty / Admin). |
| `Notification` | `Notification` | In-system notifications per user. |
| `CourseDiscussion` | `CourseDiscussion` | Threaded discussion attached to a course or assignment. |
| `CourseSelection` | `CourseSelection` | Enrolment record linking a student to a `ClassInstance`. |
| `AssignmentSubmission` | `AssignmentSubmission` | A student's submission for an assignment within a `ClassInstance`. |
| `Organization` | `Organization` | Hierarchical institutional unit (school → department → programme). |
| `GlobalSettings` | `GlobalSettings` | Singleton: current academic year and term flags. |
| `CourseRecord` | `CourseRecord` | Denormalised, language-specific search index document. |

**Technical base types** (`Oocw.Database.Models.Technical`)
- `DataModel` – base class with `Id`, `CreateTime`, `UpdateTime`, `Deleted`.
- `MultiLingualField` – `Dictionary<string, string>` with language-fallback helpers.
- `UpdateRequest` – patch-based audit record for change-request workflow.

### Oocw.Backend
ASP.NET Core 8 Web API.

**Controllers**

| Controller | Route prefix | Responsibilities |
|-----------|-------------|-----------------|
| `CourseController` | `/api/course` | List, search, retrieve, and edit courses. |
| `AuthController` | `/api/user` | Register, login, JWT refresh, logout. |
| `WebController` | `/` | Serve the SPA `index.html` (or redirect). |

**Services**

| Service | Notes |
|---------|-------|
| `DatabaseService` | Singleton; holds the `OocwDatabase` instance. |
| `SearchService` | Full-text search over `CourseRecord`; marks dirty records for re-indexing. |
| `SearchRecordService` | Background `IHostedService`; rebuilds dirty `CourseRecord` documents. |

**Authentication** – JWT bearer tokens via `Microsoft.AspNetCore.Authentication.JwtBearer`.
Refresh tokens are stored in an HTTP-only cookie.  The `JwtAuthMiddleware` exchanges
a valid refresh token for a short-lived access token automatically.

### Oocw.Frontend
Vue 3 single-page application (Vite build, TypeScript).

```
src/
  api/        REST client helpers (auth, query, user)
  components/ Reusable UI components
  pages/      Route-level page components (Main, DB, Info, User, Register)
  stores/     Pinia stores
  i18n.ts     Vue-i18n setup
  router.ts   Vue-Router configuration
```

SSR entry points (`entry-server.ts` / `entry-client.ts`) enable server-side
rendering via Vite's SSR build mode when the backend serves the SPA.

---

## Data Flow

### Course Browse
```
Client → GET /api/course/list?page=1
       ← ListResult<CourseBrief>  (name, tags, brief description)

Client → GET /api/course/info/{code}
       ← CourseSchema  (full details + class list)
```

### Full-text Search
```
Client → GET /api/course/search?contentVague=quantum+mechanics
       ← ListResult<CourseBrief>  (scored, paginated)
```
Text scoring is provided by MongoDB Atlas Search / text index on `CourseRecord.ContentRecord`.

### Authentication
```
Client → POST /api/user/login  { uname, pwd }
       ← { token }  (refresh JWT, also set as HTTP-only cookie)

Client → GET  /api/user/status  (Authorization: Bearer <access_token>)
       ← 200 OK
```

### Enrolment
```
Student → POST /api/selection/apply  { classInstanceId }
Admin   → POST /api/selection/approve { selectionId }
```

---

## Deprecated Code

The `Deprecated/` directory contains two projects that were originally written
for Tokyo Institute of Technology's OCW system and are **not built** by the main
solution:

- **`Deprecated/Oocw.Crawler`** – Selenium-based scraper targeting
  `http://www.ocw.titech.ac.jp/`.
- **`Deprecated/Oocw.Cli`** – CLI pipeline that imported crawled data into
  MongoDB using Titech-specific organisation codes and department trees.

Their functionality has been superseded by the generic `Organization` model and
`OrganizationUtils.RefreshCourseOrganizationsAsync`.

---

## Deployment

The backend serves both the API and the pre-built frontend static files from
`wwwroot/`.  A typical production setup:

```
Nginx (TLS termination / reverse proxy)
   └── Kestrel  :5051  (Oocw.Backend)
         ├── /api/**   → ASP.NET Core controllers
         └── /**       → wwwroot/index.html  (SPA fallback)
   MongoDB  :27017
```

See `build-linux.sh` for the Linux build script.
