using NUnit.Framework;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK.Models;
using Playnite.Settings;
using Playnite.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using FilterIdItem = Playnite.IdItemFilterItemProperties;

namespace Playnite.DesktopApp.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class DesktopCollectionViewFilteredTagsTests
    {
        private static List<Guid> GetGroupedTagIds(DesktopCollectionView view)
        {
            return view.Items.
                Select(a => a.Tag?.Id ?? Guid.Empty).
                Where(a => a != Guid.Empty).
                Distinct().
                ToList();
        }

        [Test]
        public void FilteredTagsGrouping_UsesOnlySelectedTagIds()
        {
            using (var db = new GameDbTestWrapper())
            using (var controllers = new GameControllerFactory())
            using (var extensions = new ExtensionFactory(db.DB, controllers, _ => null))
            {
                var tagA = db.DB.Tags.Add("A");
                var tagB = db.DB.Tags.Add("B");
                var tagC = db.DB.Tags.Add("C");

                db.DB.Games.Add(new[]
                {
                    new Game("Game1") { TagIds = new List<Guid> { tagA.Id, tagC.Id } },
                    new Game("Game2") { TagIds = new List<Guid> { tagA.Id, tagB.Id } }
                });

                var settings = new PlayniteSettings();
                settings.FilterSettings.Tag = new FilterIdItem(new List<Guid> { tagA.Id });
                settings.ViewSettings.GroupingOrder = GroupableField.FilteredTag;

                using (var view = new DesktopCollectionView(db.DB, settings, extensions))
                {
                    var groupedTagIds = GetGroupedTagIds(view);
                    CollectionAssert.AreEquivalent(new[] { tagA.Id }, groupedTagIds);
                    CollectionAssert.DoesNotContain(groupedTagIds, tagB.Id);
                    CollectionAssert.DoesNotContain(groupedTagIds, tagC.Id);
                }
            }
        }

        [Test]
        public void FilteredTagsGrouping_ResolvesTextFilterToTagIds()
        {
            using (var db = new GameDbTestWrapper())
            using (var controllers = new GameControllerFactory())
            using (var extensions = new ExtensionFactory(db.DB, controllers, _ => null))
            {
                var hltb = db.DB.Tags.Add("HowLongToPlay 05-10");
                var pvp = db.DB.Tags.Add("PvP");

                db.DB.Games.Add(new[]
                {
                    new Game("Game1") { TagIds = new List<Guid> { hltb.Id, pvp.Id } }
                });

                var settings = new PlayniteSettings();
                settings.FilterSettings.Tag = new FilterIdItem("05-10");
                settings.ViewSettings.GroupingOrder = GroupableField.FilteredTag;

                using (var view = new DesktopCollectionView(db.DB, settings, extensions))
                {
                    var groupedTagIds = GetGroupedTagIds(view);
                    CollectionAssert.AreEquivalent(new[] { hltb.Id }, groupedTagIds);
                    CollectionAssert.DoesNotContain(groupedTagIds, pvp.Id);
                }
            }
        }

        [Test]
        public void FilteredTagsGrouping_UpdatesWhenTagFilterChanges()
        {
            using (var db = new GameDbTestWrapper())
            using (var controllers = new GameControllerFactory())
            using (var extensions = new ExtensionFactory(db.DB, controllers, _ => null))
            {
                var tagA = db.DB.Tags.Add("A");
                var tagB = db.DB.Tags.Add("B");

                db.DB.Games.Add(new[]
                {
                    new Game("Game1") { TagIds = new List<Guid> { tagA.Id } },
                    new Game("Game2") { TagIds = new List<Guid> { tagB.Id } }
                });

                var settings = new PlayniteSettings();
                settings.FilterSettings.Tag = new FilterIdItem(new List<Guid> { tagA.Id });
                settings.ViewSettings.GroupingOrder = GroupableField.FilteredTag;

                using (var view = new DesktopCollectionView(db.DB, settings, extensions))
                {
                    CollectionAssert.AreEquivalent(new[] { tagA.Id }, GetGroupedTagIds(view));

                    settings.FilterSettings.Tag = new FilterIdItem(new List<Guid> { tagB.Id });

                    CollectionAssert.AreEquivalent(new[] { tagB.Id }, GetGroupedTagIds(view));
                    CollectionAssert.DoesNotContain(GetGroupedTagIds(view), tagA.Id);
                }
            }
        }
    }
}
