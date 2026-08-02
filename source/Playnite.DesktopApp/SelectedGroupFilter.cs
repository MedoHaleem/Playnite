using System;
using System.Collections.Generic;
using Playnite.SDK.Models;
using Playnite.Settings;

namespace Playnite.DesktopApp
{
    /// <summary>
    /// Pure projection helper for the "show selected groups only" Desktop mode filter.
    /// It maps the currently grouped field to the matching exact-ID filter selection and
    /// to the singular projected object on a <see cref="GamesCollectionViewEntry"/>, so a
    /// grouped view can hide projected entries whose grouping value is not one of the
    /// exactly selected IDs. This never changes which games match the ordinary filter.
    /// </summary>
    internal static class SelectedGroupFilter
    {
        /// <summary>
        /// Returns the exact selected IDs for the given grouping field, or null when the
        /// selected-groups-only projection does not apply. Returns null for no filter set,
        /// empty ID list, text-only search, text-plus-ID input, and unsupported groupings,
        /// so callers can fail open to ordinary grouping. <see cref="Guid.Empty"/> is
        /// preserved as an ordinary selectable ID.
        /// </summary>
        public static List<Guid> GetExactSelectedIds(FilterSettings filterSettings, GroupableField grouping)
        {
            if (filterSettings == null)
            {
                return null;
            }

            IdItemFilterItemProperties field;
            switch (grouping)
            {
                case GroupableField.Category:
                    field = filterSettings.Category;
                    break;
                case GroupableField.Genre:
                    field = filterSettings.Genre;
                    break;
                case GroupableField.Developer:
                    field = filterSettings.Developer;
                    break;
                case GroupableField.Publisher:
                    field = filterSettings.Publisher;
                    break;
                case GroupableField.Tag:
                    field = filterSettings.Tag;
                    break;
                case GroupableField.Feature:
                    field = filterSettings.Feature;
                    break;
                case GroupableField.Platform:
                    field = filterSettings.Platform;
                    break;
                case GroupableField.Series:
                    field = filterSettings.Series;
                    break;
                case GroupableField.AgeRating:
                    field = filterSettings.AgeRating;
                    break;
                case GroupableField.Region:
                    field = filterSettings.Region;
                    break;
                default:
                    return null;
            }

            return field?.ExactIds;
        }

        /// <summary>
        /// Returns true when the entry's projected grouping value is one of the exactly
        /// selected IDs for the current grouping field. Returns true (fail open) when the
        /// projection does not apply: no filter, empty IDs, text search, text-plus-ID,
        /// unsupported grouping, a field mismatch, or a nullable projected id. A projected
        /// id of <see cref="Guid.Empty"/> is a valid, comparable id (matched against the
        /// corresponding <c>*.Empty</c> entry object).
        /// </summary>
        public static bool IsEntryVisible(FilterSettings filterSettings, GroupableField grouping, GamesCollectionViewEntry entry)
        {
            var selectedIds = GetExactSelectedIds(filterSettings, grouping);
            if (selectedIds == null)
            {
                return true;
            }

            var projectedId = GetProjectedId(grouping, entry);
            if (projectedId == null)
            {
                // Mismatched or unsupported grouping projected object: fail open.
                return true;
            }

            return selectedIds.Contains(projectedId.Value);
        }

        /// <summary>
        /// Returns the projected grouping id for the entry, or null when the entry has no
        /// singular projected object for the given field (mismatched/unsupported).
        /// </summary>
        private static Guid? GetProjectedId(GroupableField grouping, GamesCollectionViewEntry entry)
        {
            switch (grouping)
            {
                case GroupableField.Category:
                    return entry.Category?.Id;
                case GroupableField.Genre:
                    return entry.Genre?.Id;
                case GroupableField.Developer:
                    return entry.Developer?.Id;
                case GroupableField.Publisher:
                    return entry.Publisher?.Id;
                case GroupableField.Tag:
                    return entry.Tag?.Id;
                case GroupableField.Feature:
                    return entry.Feature?.Id;
                case GroupableField.Platform:
                    return entry.Platform?.Id;
                case GroupableField.Series:
                    return entry.Serie?.Id;
                case GroupableField.AgeRating:
                    return entry.AgeRating?.Id;
                case GroupableField.Region:
                    return entry.Region?.Id;
                default:
                    return null;
            }
        }
    }
}
