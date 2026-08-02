using NUnit.Framework;
using Playnite;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SdkModels = Playnite.SDK.Models;
using SdkSerialization = Playnite.SDK.Data.Serialization;

namespace Playnite.Tests.Settings
{
    [TestFixture]
    public class FilterSettingsTests
    {
        [Test]
        public void ShouldSerializeTest()
        {
            var settings = new FilterSettings();

            var json = Serialization.ToJson(settings);
            StringAssert.DoesNotContain(nameof(FilterSettings.Name), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Series), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Source), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.AgeRating), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Region), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Genre), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Publisher), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Developer), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Category), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Tag), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Platform), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Library), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Feature), json);

            settings.Name = "test";
            settings.Series = new IdItemFilterItemProperties() { Text = "test" };
            settings.Source = new IdItemFilterItemProperties() { Text = "test" };
            settings.AgeRating = new IdItemFilterItemProperties() { Text = "test" };
            settings.Region = new IdItemFilterItemProperties() { Text = "test" };
            settings.Genre = new IdItemFilterItemProperties() { Text = "test" };
            settings.Publisher = new IdItemFilterItemProperties() { Text = "test" };
            settings.Developer = new IdItemFilterItemProperties() { Text = "test" };
            settings.Category = new IdItemFilterItemProperties() { Text = "test" };
            settings.Tag = new IdItemFilterItemProperties() { Text = "test" };
            settings.Platform = new IdItemFilterItemProperties() { Text = "test" };
            settings.Library = new IdItemFilterItemProperties() { Text = "test" };
            settings.Feature = new IdItemFilterItemProperties() { Text = "test" };
            json = Serialization.ToJson(settings);

            StringAssert.Contains(nameof(FilterSettings.Name), json);
            StringAssert.Contains(nameof(FilterSettings.Series), json);
            StringAssert.Contains(nameof(FilterSettings.Source), json);
            StringAssert.Contains(nameof(FilterSettings.AgeRating), json);
            StringAssert.Contains(nameof(FilterSettings.Region), json);
            StringAssert.Contains(nameof(FilterSettings.Genre), json);
            StringAssert.Contains(nameof(FilterSettings.Publisher), json);
            StringAssert.Contains(nameof(FilterSettings.Developer), json);
            StringAssert.Contains(nameof(FilterSettings.Category), json);
            StringAssert.Contains(nameof(FilterSettings.Tag), json);
            StringAssert.Contains(nameof(FilterSettings.Platform), json);
            StringAssert.Contains(nameof(FilterSettings.Library), json);
            StringAssert.Contains(nameof(FilterSettings.Feature), json);
        }

        [Test]
        public void StringFilterItemPropertiesEqualsTest()
        {
            Assert.IsTrue(new StringFilterItemProperties("test").Equals(new SdkModels.StringFilterItemProperties("test")));
            Assert.IsTrue(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new SdkModels.StringFilterItemProperties(new List<string> { "test", "test2" })));
            Assert.IsTrue(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new SdkModels.StringFilterItemProperties(new List<string> { "test2", "test" })));
            Assert.IsTrue(new StringFilterItemProperties().Equals(new SdkModels.StringFilterItemProperties()));

            Assert.IsTrue(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new StringFilterItemProperties(new List<string> { "test", "test2" })));
            Assert.IsTrue(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new StringFilterItemProperties(new List<string> { "test2", "test" })));
            Assert.IsTrue(new StringFilterItemProperties("test").Equals(new StringFilterItemProperties("test")));
            Assert.IsTrue(new StringFilterItemProperties().Equals(new StringFilterItemProperties()));

            Assert.IsFalse(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new SdkModels.StringFilterItemProperties(new List<string> { "test" })));
            Assert.IsFalse(new StringFilterItemProperties("test").Equals(new SdkModels.StringFilterItemProperties("test2")));
            Assert.IsFalse(new StringFilterItemProperties().Equals((SdkModels.StringFilterItemProperties)null));

            Assert.IsFalse(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new StringFilterItemProperties(new List<string> { "test2" })));
            Assert.IsFalse(new StringFilterItemProperties().Equals((StringFilterItemProperties)null));
            Assert.IsFalse(new StringFilterItemProperties("test").Equals(new StringFilterItemProperties("test2")));
        }

        [Test]
        public void StringFilterItemPropertiesSdkModelTest()
        {
            CollectionAssert.AreEqual(new StringFilterItemProperties(new List<string> { "test", "test2" }).ToSdkModel().Values, new List<string> { "test", "test2" });
            CollectionAssert.AreEqual(new StringFilterItemProperties("test").ToSdkModel().Values, new List<string> { "test" });
            Assert.AreEqual(new StringFilterItemProperties().ToSdkModel(), null);

            CollectionAssert.AreEqual(StringFilterItemProperties.FromSdkModel(new SdkModels.StringFilterItemProperties(new List<string> { "test", "test2" })).Values, new List<string> { "test", "test2" });
            CollectionAssert.AreEqual(StringFilterItemProperties.FromSdkModel(new SdkModels.StringFilterItemProperties("test")).Values, new List<string> { "test" });
            Assert.AreEqual(StringFilterItemProperties.FromSdkModel(new SdkModels.StringFilterItemProperties()), null);
        }

        [Test]
        public void EnumFilterItemPropertiesEqualsTest()
        {
            Assert.IsTrue(new EnumFilterItemProperties(1).Equals(new SdkModels.EnumFilterItemProperties(1)));
            Assert.IsTrue(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new SdkModels.EnumFilterItemProperties(new List<int> { 1, 2 })));
            Assert.IsTrue(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new SdkModels.EnumFilterItemProperties(new List<int> { 2, 1 })));
            Assert.IsTrue(new EnumFilterItemProperties().Equals(new SdkModels.EnumFilterItemProperties()));

            Assert.IsTrue(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new EnumFilterItemProperties(new List<int> { 1, 2 })));
            Assert.IsTrue(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new EnumFilterItemProperties(new List<int> { 2, 1 })));
            Assert.IsTrue(new EnumFilterItemProperties(1).Equals(new EnumFilterItemProperties(1)));
            Assert.IsTrue(new EnumFilterItemProperties().Equals(new EnumFilterItemProperties()));

            Assert.IsFalse(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new SdkModels.EnumFilterItemProperties(new List<int> { 1 })));
            Assert.IsFalse(new EnumFilterItemProperties(1).Equals(new SdkModels.EnumFilterItemProperties(2)));
            Assert.IsFalse(new EnumFilterItemProperties().Equals((SdkModels.EnumFilterItemProperties)null));

            Assert.IsFalse(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new EnumFilterItemProperties(new List<int> { 2 })));
            Assert.IsFalse(new EnumFilterItemProperties().Equals((EnumFilterItemProperties)null));
            Assert.IsFalse(new EnumFilterItemProperties(1).Equals(new EnumFilterItemProperties(2)));
        }

        [Test]
        public void EnumFilterItemPropertiesSdkModelTest()
        {
            CollectionAssert.AreEqual(new EnumFilterItemProperties(new List<int> { 1, 2 }).ToSdkModel().Values, new List<int> { 1, 2 });
            CollectionAssert.AreEqual(new EnumFilterItemProperties(1).ToSdkModel().Values, new List<int> { 1 });
            Assert.IsNull(new EnumFilterItemProperties().ToSdkModel());

            CollectionAssert.AreEqual(EnumFilterItemProperties.FromSdkModel(new SdkModels.EnumFilterItemProperties(new List<int> { 1, 2 })).Values, new List<int> { 1, 2 });
            CollectionAssert.AreEqual(EnumFilterItemProperties.FromSdkModel(new SdkModels.EnumFilterItemProperties(1)).Values, new List<int> { 1 });
            Assert.IsNull(EnumFilterItemProperties.FromSdkModel(new SdkModels.EnumFilterItemProperties()));
        }

        [Test]
        public void FilterItemPropertiesEqualsTest()
        {
            var id = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            Assert.IsTrue(new IdItemFilterItemProperties("test").Equals(new IdItemFilterItemProperties("test")));
            Assert.IsTrue(new IdItemFilterItemProperties(id).Equals(new IdItemFilterItemProperties(id)));
            Assert.IsTrue(new IdItemFilterItemProperties(new List<Guid> { id, id2 }).Equals(new IdItemFilterItemProperties(new List<Guid> { id2, id })));
            Assert.IsTrue(new IdItemFilterItemProperties().Equals(new IdItemFilterItemProperties()));

            Assert.IsTrue(new IdItemFilterItemProperties("test").Equals(new SdkModels.IdItemFilterItemProperties("test")));
            Assert.IsTrue(new IdItemFilterItemProperties(id).Equals(new SdkModels.IdItemFilterItemProperties(id)));
            Assert.IsTrue(new IdItemFilterItemProperties(new List<Guid> { id, id2 }).Equals(new SdkModels.IdItemFilterItemProperties(new List<Guid> { id2, id })));
            Assert.IsTrue(new IdItemFilterItemProperties().Equals(new SdkModels.IdItemFilterItemProperties()));

            Assert.IsFalse(new IdItemFilterItemProperties("test").Equals(new IdItemFilterItemProperties("test2")));
            Assert.IsFalse(new IdItemFilterItemProperties(id).Equals(new IdItemFilterItemProperties(id2)));
            Assert.IsFalse(new IdItemFilterItemProperties(new List<Guid> { id }).Equals(new IdItemFilterItemProperties(new List<Guid> { id, id2 })));
            Assert.IsFalse(new IdItemFilterItemProperties(id).Equals(new IdItemFilterItemProperties()));
            Assert.IsFalse(new IdItemFilterItemProperties().Equals(new IdItemFilterItemProperties(id)));

            Assert.IsFalse(new IdItemFilterItemProperties("test").Equals(new SdkModels.IdItemFilterItemProperties("test2")));
            Assert.IsFalse(new IdItemFilterItemProperties(id).Equals(new SdkModels.IdItemFilterItemProperties(id2)));
            Assert.IsFalse(new IdItemFilterItemProperties(new List<Guid> { id }).Equals(new SdkModels.IdItemFilterItemProperties(new List<Guid> { id, id2 })));
            Assert.IsFalse(new IdItemFilterItemProperties(id).Equals(new SdkModels.IdItemFilterItemProperties()));
            Assert.IsFalse(new IdItemFilterItemProperties().Equals(new SdkModels.IdItemFilterItemProperties(id)));
        }

        [Test]
        public void FilterItemPropertiesSdkModelTest()
        {
            var id = Guid.NewGuid();
            Assert.AreEqual(new IdItemFilterItemProperties("test").ToSdkModel().Text, "test");
            CollectionAssert.AreEqual(new IdItemFilterItemProperties(id).ToSdkModel().Ids, new List<Guid> { id });
            CollectionAssert.AreEqual(new IdItemFilterItemProperties(new List<Guid> { id }).ToSdkModel().Ids, new List<Guid> { id });
            Assert.IsNull(new IdItemFilterItemProperties().ToSdkModel());

            Assert.AreEqual(IdItemFilterItemProperties.FromSdkModel(new SdkModels.IdItemFilterItemProperties("test")).Text, "test");
            Assert.AreEqual(IdItemFilterItemProperties.FromSdkModel(new SdkModels.IdItemFilterItemProperties(id)).Ids, new List<Guid> { id });
            Assert.AreEqual(IdItemFilterItemProperties.FromSdkModel(new SdkModels.IdItemFilterItemProperties(new List<Guid> { id })).Ids, new List<Guid> { id });
            Assert.IsNull(IdItemFilterItemProperties.FromSdkModel(new SdkModels.IdItemFilterItemProperties()));
        }

        [Test]
        public void ShowSelectedGroupsOnlyDefaultsToFalse()
        {
            Assert.IsFalse(new FilterSettings().ShowSelectedGroupsOnly);
        }

        [Test]
        public void ShowSelectedGroupsOnlyIsExcludedFromIsActive()
        {
            var settings = new FilterSettings { ShowSelectedGroupsOnly = true };
            Assert.IsFalse(settings.IsActive, "ShowSelectedGroupsOnly must not activate the filter.");
        }

        [Test]
        public void ShowSelectedGroupsOnlyRaisesPropertyAndFilterChange()
        {
            var settings = new FilterSettings();
            var propertyChanges = new List<string>();
            var filterFields = new List<string>();
            settings.PropertyChanged += (_, e) => propertyChanges.Add(e.PropertyName);
            settings.FilterChanged += (_, e) => filterFields.AddRange(e.Fields);

            settings.ShowSelectedGroupsOnly = true;

            CollectionAssert.Contains(propertyChanges, nameof(FilterSettings.ShowSelectedGroupsOnly));
            CollectionAssert.Contains(filterFields, nameof(FilterSettings.ShowSelectedGroupsOnly));
            // Setting IsActive is also notified by OnFilterChanged.
            CollectionAssert.Contains(propertyChanges, nameof(FilterSettings.IsActive));
        }

        [Test]
        public void ShowSelectedGroupsOnlyDoesNotRaiseOnSameValue()
        {
            var settings = new FilterSettings { ShowSelectedGroupsOnly = true };
            var propertyChanges = 0;
            var filterChanges = 0;
            settings.PropertyChanged += (_, __) => propertyChanges++;
            settings.FilterChanged += (_, __) => filterChanges++;

            settings.ShowSelectedGroupsOnly = true;
            Assert.AreEqual(0, propertyChanges);
            Assert.AreEqual(0, filterChanges);
        }

        [Test]
        public void ClearFiltersResetsShowSelectedGroupsOnlyAndBatchesNotification()
        {
            var settings = new FilterSettings { ShowSelectedGroupsOnly = true };
            var filterFields = new List<string>();
            settings.FilterChanged += (_, e) => filterFields.AddRange(e.Fields);

            settings.ClearFilters();

            Assert.IsFalse(settings.ShowSelectedGroupsOnly);
            // ClearFilters batches a single FilterChanged event whose payload includes the field.
            Assert.AreEqual(1, filterFields.Count(f => f == nameof(FilterSettings.ShowSelectedGroupsOnly)));
        }

        [Test]
        public void ClearFiltersNoNotificationWhenFlagAlreadyFalse()
        {
            var settings = new FilterSettings { ShowSelectedGroupsOnly = false };
            var filterFields = new List<string>();
            settings.FilterChanged += (_, e) => filterFields.AddRange(e.Fields);

            settings.ClearFilters();
            CollectionAssert.DoesNotContain(filterFields, nameof(FilterSettings.ShowSelectedGroupsOnly));
        }

        [Test]
        public void ExactIdsNullWhenEmpty()
        {
            Assert.IsNull(new IdItemFilterItemProperties().ExactIds);
            Assert.IsNull(new IdItemFilterItemProperties(new List<Guid>()).ExactIds);
            Assert.IsNull(new IdItemFilterItemProperties("text").ExactIds);
        }

        [Test]
        public void ExactIdsReturnsIdsListByReferenceWhenTextEmpty()
        {
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var field = new IdItemFilterItemProperties(ids);
            Assert.AreSame(ids, field.ExactIds);
        }

        [Test]
        public void ExactIdsPreservesGuidEmpty()
        {
            var ids = new List<Guid> { Guid.Empty };
            var field = new IdItemFilterItemProperties(ids);
            Assert.AreSame(ids, field.ExactIds);
            CollectionAssert.Contains(field.ExactIds, Guid.Empty);
        }

        [Test]
        public void ExactIdsNullWhenTextPlusId()
        {
            var field = new IdItemFilterItemProperties(Guid.NewGuid()) { Text = "search" };
            Assert.IsNull(field.ExactIds);
        }

        [Test]
        public void ExactIdsRaisesNotificationFromIdsAndTextSetters()
        {
            var field = new IdItemFilterItemProperties();
            var changes = new List<string>();
            ((INotifyPropertyChanged)field).PropertyChanged += (_, e) => changes.Add(e.PropertyName);

            field.Ids = new List<Guid> { Guid.NewGuid() };
            Assert.IsTrue(changes.Contains(nameof(IdItemFilterItemProperties.ExactIds)));

            changes.Clear();
            field.Text = "search";
            Assert.IsTrue(changes.Contains(nameof(IdItemFilterItemProperties.ExactIds)));

            changes.Clear();
            field.Text = null;
            Assert.IsTrue(changes.Contains(nameof(IdItemFilterItemProperties.ExactIds)));
        }

        [Test]
        public void ShowSelectedGroupsOnlySdkConversionRoundTrip()
        {
            var settings = new FilterSettings { ShowSelectedGroupsOnly = true };
            var sdk = settings.AsPresetSettings();
            Assert.IsTrue(sdk.ShowSelectedGroupsOnly);

            var restored = FilterSettings.FromSdkFilterSettings(sdk);
            Assert.IsTrue(restored.ShowSelectedGroupsOnly);
        }

        [Test]
        public void ShowSelectedGroupsOnlyApplyFilterDetectsDifference()
        {
            var settings = new FilterSettings { ShowSelectedGroupsOnly = false };
            var fields = new List<string>();
            settings.FilterChanged += (_, e) => fields.AddRange(e.Fields);

            settings.ApplyFilter(new SdkModels.FilterPresetSettings { ShowSelectedGroupsOnly = true });

            Assert.IsTrue(settings.ShowSelectedGroupsOnly);
            CollectionAssert.Contains(fields, nameof(FilterSettings.ShowSelectedGroupsOnly));
        }

        [Test]
        public void ShowSelectedGroupsOnlyApplyFilterNoOpOnSameValue()
        {
            var settings = new FilterSettings { ShowSelectedGroupsOnly = true };
            var fields = new List<string>();
            settings.FilterChanged += (_, e) => fields.AddRange(e.Fields);

            settings.ApplyFilter(new SdkModels.FilterPresetSettings { ShowSelectedGroupsOnly = true });
            CollectionAssert.DoesNotContain(fields, nameof(FilterSettings.ShowSelectedGroupsOnly));
        }

        [Test]
        public void ShowSelectedGroupsOnlyLiveJsonRoundTrip()
        {
            var settings = new FilterSettings { ShowSelectedGroupsOnly = true };
            var json = Serialization.ToJson(settings);
            StringAssert.Contains(nameof(FilterSettings.ShowSelectedGroupsOnly), json);

            var restored = Serialization.FromJson<FilterSettings>(json);
            Assert.IsTrue(restored.ShowSelectedGroupsOnly);
        }

        [Test]
        public void ShowSelectedGroupsOnlyOmittedPropertyCompatibility()
        {
            // Old config/preset JSON that omits the new boolean must deserialize to the
            // false default without error (no migration needed).
            var json = Serialization.ToJson(new FilterSettings()).Replace(nameof(FilterSettings.ShowSelectedGroupsOnly), "__removed__");
            Assert.IsFalse(json.Contains(nameof(FilterSettings.ShowSelectedGroupsOnly)));
            var restored = Serialization.FromJson<FilterSettings>(json);
            Assert.IsFalse(restored.ShowSelectedGroupsOnly);
        }

        [Test]
        public void ShowSelectedGroupsOnlySdkPresetJsonRoundTrip()
        {
            var sdk = new SdkModels.FilterPresetSettings { ShowSelectedGroupsOnly = true };
            var json = SdkSerialization.ToJson(sdk);
            StringAssert.Contains(nameof(SdkModels.FilterPresetSettings.ShowSelectedGroupsOnly), json);

            var restored = SdkSerialization.FromJson<SdkModels.FilterPresetSettings>(json);
            Assert.IsTrue(restored.ShowSelectedGroupsOnly);
        }

        [Test]
        public void ShowSelectedGroupsOnlyCarriedByNestedFilterPresetCopyDiffTo()
        {
            // The new value lives on nested Settings, so replacing the Settings object carries it.
            var source = new SdkModels.FilterPreset
            {
                Settings = new SdkModels.FilterPresetSettings { ShowSelectedGroupsOnly = true }
            };
            var target = new SdkModels.FilterPreset
            {
                Settings = new SdkModels.FilterPresetSettings { ShowSelectedGroupsOnly = false }
            };

            source.CopyDiffTo(target);
            Assert.IsTrue(target.Settings.ShowSelectedGroupsOnly);
        }
    }
}
