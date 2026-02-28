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

## Phase 1 – Core Course Workflows

### Course Management
- [ ] Create course (request → review → approval workflow)
- [ ] Edit course metadata (request → review → approval)
- [ ] Publish syllabus / lecture notes (request → approval)
- [ ] Per-course discussion threads (public and private)
- [ ] Course tagging and department assignment

### Class & Semester Management
- [ ] Create and manage class sections
- [ ] Create and manage semester instances of a class
- [ ] Upload lecture materials (PDF, video links, text)
- [ ] Copy materials from a previous semester

### Student Enrolment
- [ ] Browse enrolable courses for the current semester
- [ ] Apply for enrolment (`CourseSelection` → `Application`)
- [ ] Approve / reject enrolment (faculty / admin)
- [ ] Withdraw enrolment

---

## Phase 2 – Academic Interaction

### Assignments
- [ ] Create assignment (faculty: linked to a `ClassInstance` content item)
- [ ] Submit assignment (student: text and/or file upload)
- [ ] Re-submit / revise submission (revision history preserved)
- [ ] Grade submission (faculty; multiple re-grades allowed)
- [ ] Leave grading comments via `CourseDiscussion`

### Feedback
- [ ] Students submit per-semester feedback
- [ ] Faculty and admins view aggregated feedback

### Notifications
- [ ] Trigger in-system notifications on key events (enrolment status, grades)
- [ ] Mark individual or all notifications as read
- [ ] REST endpoint: list recent / unread notifications

---

## Phase 3 – Administration & Moderation

### Review Workflow
- [ ] Reviewer queue: approve / reject pending course and content changes
- [ ] Reviewer can directly assist opening a class on behalf of a faculty member
- [ ] `UpdateRequest` audit trail for all patch-based changes

### Admin Tools
- [ ] Set current academic year and term (`GlobalSettings`)
- [ ] Manage user roles (assign Student / Faculty / Reviewer / Admin)
- [ ] Manage `Organization` hierarchy (add, edit, remove entries)
- [ ] View and export course feedback

---

## Phase 4 – UI Completeness

### Pages
- [ ] Home page with working search (keyword / faculty / code / name filter buttons)
- [ ] Department browser (navigate organisation tree → list courses)
- [ ] Search results page with pagination and relevance sorting
- [ ] Course detail page (description, schedule, class list, related courses)
- [ ] Course detail edit mode (for faculty / reviewer)
- [ ] Enrolment mode with timetable visualisation
- [ ] User profile page (own view and public view)
- [ ] User profile edit mode
- [ ] Login / registration pages
- [ ] Admin management panel
- [ ] Reviewer moderation panel
- [ ] In-system notification centre
- [ ] Class instance page (materials, assignments)
- [ ] Assignment submission page

---

## Phase 5 – Quality & Performance

- [ ] Server-side rendering (SSR) for SEO and first-paint performance
- [ ] Async everywhere: replace all remaining blocking DB calls
- [ ] Persistent `SearchRecordService` rebuild queue (replace in-memory)
- [ ] Rate limiting and abuse prevention on auth endpoints
- [ ] Integration and end-to-end tests
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Container image (`Dockerfile` + `docker-compose.yml`)

---

## Phase 6 – Extended Features

- [ ] Multi-institution support (each institution seeds its own `Organization` tree)
- [ ] OAuth / external identity provider login
- [ ] Public API for third-party clients
- [ ] Mobile-friendly PWA manifest
- [ ] Accessibility (WCAG 2.1 AA)
- [ ] Data export (course catalogue as CSV / JSON-LD)
