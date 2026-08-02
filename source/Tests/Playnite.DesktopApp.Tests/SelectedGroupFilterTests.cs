using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.Settings;

namespace Playnite.DesktopApp.Tests
{
    [TestFixture]
    public class SelectedGroupFilterTests
    {
        private InMemoryGameDatabase database;
        private PlayniteSettings settings;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            database = new InMemoryGameDatabase();
            Game.DatabaseReference = database;
            GameDatabase.GenerateSampleData(database);
        }

        [SetUp]
        public void SetUp()
        {
            settings = new PlayniteSettings();
        }

        private GamesCollectionViewEntry ProjectedEntry(GroupableField grouping, Guid id)
        {
            var game = new Game("Test " + Guid.NewGuid());
            return GamesCollectionViewEntry.GetAdvancedGroupedEntry(game, null, GroupType(grouping), id, database, settings);
        }

        private GamesCollectionViewEntry PlainEntry()
        {
            return new GamesCollectionViewEntry(new Game("Plain " + Guid.NewGuid()), null, settings);
        }

        private static Type GroupType(GroupableField grouping)
        {
            switch (grouping)
            {
                case GroupableField.Genre: return typeof(Genre);
                case GroupableField.Developer: return typeof(Developer);
                case GroupableField.Publisher: return typeof(Publisher);
                case GroupableField.Tag: return typeof(Tag);
                case GroupableField.Feature: return typeof(GameFeature);
                case GroupableField.Category: return typeof(Category);
                case GroupableField.Platform: return typeof(Platform);
                case GroupableField.AgeRating: return typeof(AgeRating);
                case GroupableField.Series: return typeof(Series);
                case GroupableField.Region: return typeof(Region);
                default: throw new ArgumentOutOfRangeException(nameof(grouping));
            }
        }

        private Guid FirstId(GroupableField grouping)
        {
            switch (grouping)
            {
                case GroupableField.Genre: return database.Genres.First().Id;
                case GroupableField.Developer: return database.Companies.First().Id;
                case GroupableField.Publisher: return database.Companies.Last().Id;
                case GroupableField.Tag: return database.Tags.First().Id;
                case GroupableField.Feature: return database.Features.First().Id;
                case GroupableField.Category: return database.Categories.First().Id;
                case GroupableField.Platform: return database.Platforms.First().Id;
                case GroupableField.AgeRating: return database.AgeRatings.First().Id;
                case GroupableField.Series: return database.Series.First().Id;
                case GroupableField.Region: return database.Regions.First().Id;
                default: throw new ArgumentOutOfRangeException(nameof(grouping));
            }
        }

        private void SetFilterProperty(FilterSettings filter, GroupableField grouping, IdItemFilterItemProperties value)
        {
            switch (grouping)
            {
                case GroupableField.Genre: filter.Genre = value; break;
                case GroupableField.Developer: filter.Developer = value; break;
                case GroupableField.Publisher: filter.Publisher = value; break;
                case GroupableField.Tag: filter.Tag = value; break;
                case GroupableField.Feature: filter.Feature = value; break;
                case GroupableField.Category: filter.Category = value; break;
                case GroupableField.Platform: filter.Platform = value; break;
                case GroupableField.AgeRating: filter.AgeRating = value; break;
                case GroupableField.Series: filter.Series = value; break;
                case GroupableField.Region: filter.Region = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(grouping));
            }
        }

        private static readonly GroupableField[] SupportedGroupings =
        {
            GroupableField.Category,
            GroupableField.Genre,
            GroupableField.Developer,
            GroupableField.Publisher,
            GroupableField.Tag,
            GroupableField.Feature,
            GroupableField.Platform,
            GroupableField.Series,
            GroupableField.AgeRating,
            GroupableField.Region
        };

        [Test, Description("Each supported grouping maps to its filter property and projected entry object.")]
        public void GetExactSelectedIds_AllMappings_Hit([ValueSource(nameof(SupportedGroupings))] GroupableField grouping)
        {
            var id = FirstId(grouping);
            var filter = new FilterSettings();
            SetFilterProperty(filter, grouping, new IdItemFilterItemProperties(id));

            var exact = SelectedGroupFilter.GetExactSelectedIds(filter, grouping);
            Assert.IsNotNull(exact);
            CollectionAssert.Contains(exact, id);

            var hit = ProjectedEntry(grouping, id);
            Assert.IsNotNull(hit, $"GetAdvancedGroupedEntry returned null for {grouping}");
            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(filter, grouping, hit));
        }

        [Test]
        public void IsEntryVisible_MissedIdIsHidden([ValueSource(nameof(SupportedGroupings))] GroupableField grouping)
        {
            var selectedId = FirstId(grouping);
            var otherId = Guid.NewGuid();
            var filter = new FilterSettings();
            SetFilterProperty(filter, grouping, new IdItemFilterItemProperties(selectedId));

            var miss = ProjectedEntry(grouping, otherId);
            // Database has no item for a random Guid, so the entry is null (not projected).
            // A non-projected (plain) entry with Guid.Empty should be hidden when Guid.Empty is not selected.
            Assert.IsNull(miss);

            var plain = PlainEntry();
            // Plain entry's projected id is Guid.Empty, which is not among the selected real ids.
            Assert.IsFalse(SelectedGroupFilter.IsEntryVisible(filter, grouping, plain));
        }

        [Test, Description("Multiple selected IDs: entry matching any is visible, matching none is hidden.")]
        public void MultipleSelectedIdsTest([ValueSource(nameof(SupportedGroupings))] GroupableField grouping)
        {
            var firstId = FirstId(grouping);
            var secondId = Guid.NewGuid();
            // Add a second real item for the field where possible, otherwise just use first.
            var filter = new FilterSettings();
            SetFilterProperty(filter, grouping, new IdItemFilterItemProperties(new List<Guid> { firstId, secondId }));

            var exact = SelectedGroupFilter.GetExactSelectedIds(filter, grouping);
            CollectionAssert.AreEquivalent(new[] { firstId, secondId }, exact);

            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(filter, grouping, ProjectedEntry(grouping, firstId)));
            // Plain entry (Guid.Empty) is not in the selected set.
            Assert.IsFalse(SelectedGroupFilter.IsEntryVisible(filter, grouping, PlainEntry()));
        }

        [Test, Description("Selected Guid.Empty matches the corresponding *.Empty projected entry object.")]
        public void SelectedEmptyMatchesEmptyEntry([ValueSource(nameof(SupportedGroupings))] GroupableField grouping)
        {
            var filter = new FilterSettings();
            SetFilterProperty(filter, grouping, new IdItemFilterItemProperties(Guid.Empty));

            var exact = SelectedGroupFilter.GetExactSelectedIds(filter, grouping);
            Assert.IsNotNull(exact);
            CollectionAssert.Contains(exact, Guid.Empty);

            var plain = PlainEntry();
            // Plain entry's projected property is *.Empty with Id == Guid.Empty.
            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(filter, grouping, plain));
        }

        [Test, Description("Unselected Guid.Empty: a real id is selected, so the *.Empty entry is hidden.")]
        public void UnselectedEmptyIsHidden([ValueSource(nameof(SupportedGroupings))] GroupableField grouping)
        {
            var realId = FirstId(grouping);
            var filter = new FilterSettings();
            SetFilterProperty(filter, grouping, new IdItemFilterItemProperties(realId));

            Assert.IsFalse(SelectedGroupFilter.IsEntryVisible(filter, grouping, PlainEntry()));
        }

        [Test]
        public void FailOpen_NoFilter([ValueSource(nameof(SupportedGroupings))] GroupableField grouping)
        {
            var filter = new FilterSettings();
            Assert.IsNull(SelectedGroupFilter.GetExactSelectedIds(filter, grouping));
            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(filter, grouping, PlainEntry()));
        }

        [Test]
        public void FailOpen_EmptyIds([ValueSource(nameof(SupportedGroupings))] GroupableField grouping)
        {
            var filter = new FilterSettings();
            SetFilterProperty(filter, grouping, new IdItemFilterItemProperties(new List<Guid>()));
            Assert.IsNull(SelectedGroupFilter.GetExactSelectedIds(filter, grouping));
            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(filter, grouping, PlainEntry()));
        }

        [Test]
        public void FailOpen_TextOnly([ValueSource(nameof(SupportedGroupings))] GroupableField grouping)
        {
            var filter = new FilterSettings();
            SetFilterProperty(filter, grouping, new IdItemFilterItemProperties("text"));
            Assert.IsNull(SelectedGroupFilter.GetExactSelectedIds(filter, grouping));
            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(filter, grouping, PlainEntry()));
        }

        [Test]
        public void FailOpen_TextPlusId([ValueSource(nameof(SupportedGroupings))] GroupableField grouping)
        {
            var id = FirstId(grouping);
            var filter = new FilterSettings();
            // Both text and id set => text-plus-id => fail open.
            var field = new IdItemFilterItemProperties(id) { Text = "text" };
            SetFilterProperty(filter, grouping, field);
            Assert.IsNull(SelectedGroupFilter.GetExactSelectedIds(filter, grouping));
            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(filter, grouping, PlainEntry()));
        }

        [Test]
        public void FailOpen_UnsupportedGrouping()
        {
            var filter = new FilterSettings();
            filter.Library = new IdItemFilterItemProperties(Guid.NewGuid());
            foreach (var unsupported in new[]
            {
                GroupableField.None,
                GroupableField.Library,
                GroupableField.Source,
                GroupableField.ReleaseYear,
                GroupableField.CompletionStatus,
                GroupableField.UserScore,
                GroupableField.CriticScore,
                GroupableField.CommunityScore,
                GroupableField.LastActivity,
                GroupableField.RecentActivity,
                GroupableField.Added,
                GroupableField.Modified,
                GroupableField.PlayTime,
                GroupableField.InstallationStatus,
                GroupableField.Name,
                GroupableField.InstallDrive,
                GroupableField.InstallSize
            })
            {
                Assert.IsNull(SelectedGroupFilter.GetExactSelectedIds(filter, unsupported), unsupported.ToString());
                Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(filter, unsupported, PlainEntry()), unsupported.ToString());
            }
        }

        [Test]
        public void FailOpen_NullFilterSettings()
        {
            Assert.IsNull(SelectedGroupFilter.GetExactSelectedIds(null, GroupableField.Category));
            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(null, GroupableField.Category, PlainEntry()));
        }

        [Test, Description("Selecting a field that is not the current grouping fails open for the current grouping.")]
        public void FieldMismatchFailsOpen()
        {
            var filter = new FilterSettings();
            // Select a Genre id, but group by Category.
            filter.Genre = new IdItemFilterItemProperties(database.Genres.First().Id);
            Assert.IsNull(SelectedGroupFilter.GetExactSelectedIds(filter, GroupableField.Category));
            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(filter, GroupableField.Category, PlainEntry()));
        }

        [Test, Description("ExactIds returns the Ids list by reference and preserves Guid.Empty.")]
        public void ExactIdsReturnsByReferenceAndPreservesEmpty()
        {
            var ids = new List<Guid> { Guid.Empty, Guid.NewGuid() };
            var field = new IdItemFilterItemProperties(ids);
            Assert.AreSame(ids, field.ExactIds);
            CollectionAssert.AreEqual(ids, field.ExactIds);

            // Adding text makes it fail open.
            field.Text = "search";
            Assert.IsNull(field.ExactIds);

            field.Text = null;
            Assert.AreSame(ids, field.ExactIds);

            // Empty list returns null.
            Assert.IsNull(new IdItemFilterItemProperties(new List<Guid>()).ExactIds);
        }
    }
}
