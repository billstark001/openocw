
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Oocw.Database.Models;

namespace Oocw.Database.Utils;

/// <summary>
/// Helpers for resolving organisation names to their canonical codes and
/// keeping the <see cref="Course.Departments"/> list consistent with the
/// <see cref="Organization"/> collection.
/// </summary>
public static class OrganizationUtils
{
    public const string KEY_UNCAT = "uncat";

    /// <summary>
    /// Iterates over <see cref="Course"/> documents whose
    /// <see cref="Course.Departments"/> list is empty or contains the
    /// placeholder value <c>"uncat"</c>, and attempts to resolve each
    /// department name (in the specified <paramref name="lang"/>) to a
    /// canonical <see cref="Organization.Code"/> using the
    /// <see cref="Organization"/> collection.
    ///
    /// Courses whose department names cannot be matched are left with the
    /// <c>"uncat"</c> placeholder so that they can be fixed manually later.
    /// </summary>
    /// <param name="db">Database wrapper instance.</param>
    /// <param name="lang">Language code used to look up names in
    ///   <see cref="Organization.Aliases"/> and
    ///   <see cref="Organization.Name"/>.</param>
    /// <param name="handleUncategorized">
    ///   When <c>true</c>, also re-processes courses that were previously
    ///   marked as <c>"uncat"</c>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of courses updated.</returns>
    public static async Task<long> RefreshCourseOrganizationsAsync(
        this OocwDatabase db,
        string lang = "en",
        bool handleUncategorized = false,
        CancellationToken cancellationToken = default)
    {
        // Build alias → code map from the Organization collection
        var allOrgs = await db.Organizations
            .Find(FilterDefinition<Organization>.Empty)
            .ToListAsync(cancellationToken: cancellationToken);

        var nameToCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var org in allOrgs)
        {
            if (!string.IsNullOrWhiteSpace(org.Code))
            {
                // Primary name
                var primary = org.Name.GetTranslation(lang);
                if (!string.IsNullOrWhiteSpace(primary))
                    nameToCode.TryAdd(primary, org.Code);

                // All aliases
                foreach (var alias in org.Aliases)
                    if (!string.IsNullOrWhiteSpace(alias))
                        nameToCode.TryAdd(alias, org.Code);
            }
        }

        // Find courses that need department resolution
        FilterDefinition<Course> filter = handleUncategorized
            ? Builders<Course>.Filter.Or(
                Builders<Course>.Filter.Size(x => x.Departments, 0),
                Builders<Course>.Filter.AnyEq(x => x.Departments, KEY_UNCAT))
            : Builders<Course>.Filter.Size(x => x.Departments, 0);

        var cursor = await db.Courses.FindAsync(filter, cancellationToken: cancellationToken);
        long updatedCount = 0;

        await cursor.ForEachAsync(async course =>
        {
            List<string> resolved = [];
            bool changed = false;

            // Try to resolve each existing entry (or seed from name if empty)
            var candidates = course.Departments.Count > 0
                ? course.Departments
                : [];

            if (candidates.Count == 0 && !string.IsNullOrWhiteSpace(course.Name.GetTranslation(lang)))
            {
                // No department entries at all – mark as uncat
                var update = Builders<Course>.Update.AddToSet(x => x.Departments, KEY_UNCAT);
                await db.Courses.UpdateOneAsync(x => x.Id == course.Id, update, cancellationToken: cancellationToken);
                return;
            }

            foreach (var dep in candidates)
            {
                if (dep == KEY_UNCAT || string.IsNullOrWhiteSpace(dep))
                    continue;

                if (nameToCode.TryGetValue(dep, out var code))
                {
                    resolved.Add(code);
                    changed = true;
                }
                else
                {
                    resolved.Add(dep); // keep original
                }
            }

            if (!changed) return;

            var upd = Builders<Course>.Update.Set(x => x.Departments, resolved);
            await db.Courses.UpdateOneAsync(x => x.Id == course.Id, upd, cancellationToken: cancellationToken);
            updatedCount++;
        }, cancellationToken: cancellationToken);

        return updatedCount;
    }
}
