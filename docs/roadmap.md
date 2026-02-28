# Roadmap

This document describes the planned feature set for OpenOCW.  Items are grouped
by phase.  Within each phase, features are roughly ordered by priority.

---

## Phase 0 – Foundation ✅ (current)

- [x] Core data models: Course, Class, ClassInstance, User
- [x] JWT authentication (register / login / logout)
- [x] Course list and detail APIs
- [x] Full-text search via MongoDB text index
- [x] Vue 3 SPA with i18n and routing
- [x] Generic `Organization` model (replaces hardcoded Titech mapping)
- [x] `GlobalSettings` model (current year / term)
- [x] Deprecated institution-specific crawler / CLI isolated in `Deprecated/`

---

## Phase 1 – Core Course Workflows ✅

### Course Management
- [x] Create course (request → review → approval workflow)
- [x] Edit course metadata (request → review → approval)
- [x] Publish syllabus / lecture notes (request → approval)
- [x] Per-course discussion threads (public and private)
- [x] Course tagging and department assignment

### Class & Semester Management
- [x] Create and manage class sections
- [x] Create and manage semester instances of a class
- [x] Upload lecture materials (PDF, video links, text)
- [x] Copy materials from a previous semester

### Student Enrolment
- [x] Browse enrolable courses for the current semester
- [x] Apply for enrolment (`CourseSelection` → `Application`)
- [x] Approve / reject enrolment (faculty / admin)
- [x] Withdraw enrolment

---

## Phase 2 – Academic Interaction ✅

### Assignments
- [x] Create assignment (faculty: linked to a `ClassInstance` content item)
- [x] Submit assignment (student: text and/or file upload)
- [x] Re-submit / revise submission (revision history preserved)
- [x] Grade submission (faculty; multiple re-grades allowed)
- [x] Leave grading comments via `CourseDiscussion`

### Feedback
- [x] Students submit per-semester feedback
- [x] Faculty and admins view aggregated feedback

### Notifications
- [x] Trigger in-system notifications on key events (enrolment status, grades)
- [x] Mark individual or all notifications as read
- [x] REST endpoint: list recent / unread notifications

---

## Phase 3 – Administration & Moderation ✅

### Review Workflow
- [x] Reviewer queue: approve / reject pending course and content changes
- [x] Reviewer can directly assist opening a class on behalf of a faculty member
- [x] `UpdateRequest` audit trail for all patch-based changes

### Admin Tools
- [x] Set current academic year and term (`GlobalSettings`)
- [x] Manage user roles (assign Student / Faculty / Reviewer / Admin)
- [x] Manage `Organization` hierarchy (add, edit, remove entries)
- [x] View and export course feedback

---

## Phase 4 – UI Completeness ✅

### Pages
- [x] Home page with working search (keyword / faculty / code / name filter buttons)
- [x] Department browser (navigate organisation tree → list courses)
- [x] Search results page with pagination and relevance sorting
- [x] Course detail page (description, schedule, class list, related courses)
- [x] Course detail edit mode (for faculty / reviewer)
- [x] Enrolment mode with timetable visualisation
- [x] User profile page (own view and public view)
- [x] User profile edit mode
- [x] Login / registration pages
- [x] Admin management panel
- [x] Reviewer moderation panel
- [x] In-system notification centre
- [x] Class instance page (materials, assignments)
- [x] Assignment submission page

---

## Phase 5 – Quality & Performance ✅

- [x] Server-side rendering (SSR) for SEO and first-paint performance
- [x] Async everywhere: replace all remaining blocking DB calls
- [x] Persistent `SearchRecordService` rebuild queue (replace in-memory)
- [x] Rate limiting and abuse prevention on auth endpoints
- [ ] Integration and end-to-end tests
- [x] CI/CD pipeline (GitHub Actions)
- [x] Container image (`Dockerfile` + `docker-compose.yml`)

---

## Phase 6 – Extended Features

- [ ] Multi-institution support (each institution seeds its own `Organization` tree)
- [ ] OAuth / external identity provider login
- [ ] Public API for third-party clients
- [ ] Mobile-friendly PWA manifest
- [ ] Accessibility (WCAG 2.1 AA)
- [ ] Data export (course catalogue as CSV / JSON-LD)
