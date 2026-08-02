using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Settings;

namespace Playnite.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class GamesCollectionViewTests
    {
        /// <summary>
        /// Minimal <see cref="BaseCollectionView"/> subclass used to exercise the
        /// <see cref="BaseCollectionView"/> filter seam. It mirrors the Desktop override by
        /// hiding entries whose projected grouping id is not among the exactly selected ids
        /// when <see cref="FilterSettings.ShowSelectedGroupsOnly"/> is enabled, and otherwise
        /// delegating to the base (ordinary visibility).
        /// </summary>
        private class TestCollectionView : BaseCollectionView
        {
            private readonly FilterSettings filterSettings;
            private readonly GroupableField grouping;
            private readonly List<Guid> selectedIds;

            public TestCollectionView(
                IGameDatabaseMain database,
                FilterSettings filterSettings,
                PlayniteSettings settings,
                GroupableField grouping,
                List<Guid> selectedIds)
                : base(database, null, filterSettings, settings)
            {
                this.filterSettings = filterSettings;
                this.grouping = grouping;
                this.selectedIds = selectedIds;
            }

            public override void RefreshView()
            {
            }

            protected override bool IsEntryVisible(GamesCollectionViewEntry entry)
            {
                if (!filterSettings.ShowSelectedGroupsOnly)
                {
                    return true;
                }

                var projectedId = GetProjectedId(grouping, entry);
                if (projectedId == null)
                {
                    return true;
                }

                return selectedIds?.Contains(projectedId.Value) == true;
            }

            private static Guid? GetProjectedId(GroupableField grouping, GamesCollectionViewEntry entry)
            {
                switch (grouping)
                {
                    case GroupableField.Category: return entry.Category?.Id;
                    default: return null;
                }
            }
        }

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

        private static Mock<IGameDatabaseMain> MockDatabase(bool matchesFilter)
        {
            var mock = new Mock<IGameDatabaseMain>();
            mock.Setup(x => x.GetGameMatchesFilter(It.IsAny<Game>(), It.IsAny<FilterSettings>(), It.IsAny<bool>()))
                .Returns(matchesFilter);
            return mock;
        }

        [Test]
        public void Filter_IsGameMatchAndEntryVisibility()
        {
            // A game that does NOT match the ordinary filter, but whose projected ID is selected.
            // It must be hidden because the predicate is game-match AND entry-visibility.
            var categoryId = database.Categories.First().Id;
            var dbMock = MockDatabase(matchesFilter: false);
            var filter = new FilterSettings { ShowSelectedGroupsOnly = true };
            var view = new TestCollectionView(dbMock.Object, filter, settings, GroupableField.Category, new List<Guid> { categoryId });

            var game = new Game("NonMatchingGame") { CategoryIds = new List<Guid> { categoryId } };
            var entry = GamesCollectionViewEntry.GetAdvancedGroupedEntry(game, null, typeof(Category), categoryId, database, settings);
            Assert.IsNotNull(entry);
            view.Items.Add(entry);

            Assert.IsFalse(view.CollectionView.PassesFilter(entry));
        }

        [Test]
        public void Filter_GameMatchButHiddenProjectedEntryIsExcluded()
        {
            // Game matches the filter, but the projected entry's id is NOT selected.
            var categoryId = database.Categories.First().Id;
            var dbMock = MockDatabase(matchesFilter: true);
            var filter = new FilterSettings { ShowSelectedGroupsOnly = true };
            var view = new TestCollectionView(dbMock.Object, filter, settings, GroupableField.Category, new List<Guid> { Guid.NewGuid() });

            var game = new Game("MatchingGame") { CategoryIds = new List<Guid> { categoryId } };
            var entry = GamesCollectionViewEntry.GetAdvancedGroupedEntry(game, null, typeof(Category), categoryId, database, settings);
            Assert.IsNotNull(entry);
            view.Items.Add(entry);

            Assert.IsFalse(view.CollectionView.PassesFilter(entry));
        }

        [Test]
        public void Filter_GameMatchAndSelectedProjectedEntryIsIncluded()
        {
            var categoryId = database.Categories.First().Id;
            var dbMock = MockDatabase(matchesFilter: true);
            var filter = new FilterSettings { ShowSelectedGroupsOnly = true };
            var view = new TestCollectionView(dbMock.Object, filter, settings, GroupableField.Category, new List<Guid> { categoryId });

            var game = new Game("MatchingSelectedGame") { CategoryIds = new List<Guid> { categoryId } };
            var entry = GamesCollectionViewEntry.GetAdvancedGroupedEntry(game, null, typeof(Category), categoryId, database, settings);
            Assert.IsNotNull(entry);
            view.Items.Add(entry);

            Assert.IsTrue(view.CollectionView.PassesFilter(entry));
        }

        [Test]
        public void Filter_DisabledDelegatesToOrdinaryVisibility()
        {
            // ShowSelectedGroupsOnly false: a matching game whose projected id is NOT selected
            // must still be visible (entry-visibility hook returns true).
            var categoryId = database.Categories.First().Id;
            var dbMock = MockDatabase(matchesFilter: true);
            var filter = new FilterSettings { ShowSelectedGroupsOnly = false };
            var view = new TestCollectionView(dbMock.Object, filter, settings, GroupableField.Category, new List<Guid> { Guid.NewGuid() });

            var game = new Game("DisabledGame") { CategoryIds = new List<Guid> { categoryId } };
            var entry = GamesCollectionViewEntry.GetAdvancedGroupedEntry(game, null, typeof(Category), categoryId, database, settings);
            Assert.IsNotNull(entry);
            view.Items.Add(entry);

            Assert.IsTrue(view.CollectionView.PassesFilter(entry));
        }
    }
}
