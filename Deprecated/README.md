# Deprecated

This directory contains code that was tightly coupled to a specific institution
(Tokyo Institute of Technology / 東京工業大学, referred to as Titech) and is no
longer part of the main build.

## Contents

### `Oocw.Crawler`
A Selenium-based web scraper targeting the Titech OCW site
(`http://www.ocw.titech.ac.jp/`). It extracts department trees, course lists, and
per-course syllabi using the site's specific HTML structure and query parameters.

### `Oocw.Cli`
A command-line interface that orchestrated the Titech crawl pipeline and imported
scraped data into MongoDB.  Key files:

- `Utils/TitechUtils.cs` – hardcoded Japanese-to-code organisation mapping for
  Titech's school/department hierarchy, plus a `RefreshOrganizations` helper that
  back-populated the `departments.key` field on `Course` documents.
- `Tasks/SingleUpdate.cs` – database import logic that converted the crawler
  models (`CourseRecord`, `SyllabusRecord`) into the generic `Course`/`Class`
  database models.

## Replacement

The hardcoded organisation mapping is superseded by the generic `Organization`
collection in the database (see `Oocw.Database/Models/Organization.cs`).  Any
institution can seed its own hierarchy into that collection and use the
`OrganizationUtils.RefreshCourseOrganizationsAsync` extension method to
back-populate course department keys.
