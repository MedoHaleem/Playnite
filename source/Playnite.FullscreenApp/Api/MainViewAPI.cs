using Playnite.FullscreenApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Playnite.FullscreenApp.API
{
    public class MainViewAPI : IMainViewAPI
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private FullscreenAppViewModel mainModel;
        private List<Game> filteredGamesCache;
        private bool filteredGamesCacheHooked;

        public IEnumerable<Game> SelectedGames
        {
            get
            {
                if (UIDispatcher.CheckAccess())
                {
                    return GetSelectedGamesCore();
                }

                return UIDispatcher.Invoke(() => GetSelectedGamesCore());
            }
        }

        public DesktopView ActiveDesktopView
        {
            get => DesktopView.Details;
            set { }
        }

        public FullscreenView ActiveFullscreenView
        {
            get => mainModel.GameDetailsVisible ? FullscreenView.Details : FullscreenView.List;
        }

        public SortOrder SortOrder
        {
            get => mainModel.AppSettings.Fullscreen.ViewSettings.SortingOrder;
            set => mainModel.AppSettings.Fullscreen.ViewSettings.SortingOrder = value;
        }

        public SortOrderDirection SortOrderDirection
        {
            get => mainModel.AppSettings.Fullscreen.ViewSettings.SortingOrderDirection;
            set => mainModel.AppSettings.Fullscreen.ViewSettings.SortingOrderDirection = value;
        }

        public GroupableField Grouping
        {
            get => GroupableField.None;
            set { }
        }

        public List<Game> FilteredGames
        {
            get
            {
                if (UIDispatcher.CheckAccess())
                {
                    return GetFilteredGamesCore();
                }

                return UIDispatcher.Invoke(() => GetFilteredGamesCore());
            }
        }

        public Dispatcher UIDispatcher => PlayniteApplication.CurrentNative.Dispatcher;

        public MainViewAPI(FullscreenAppViewModel mainModel)
        {
            this.mainModel = mainModel;
        }

        private List<Game> GetSelectedGamesCore()
        {
            if (mainModel.SelectedGames == null && mainModel.SelectedGame != null)
            {
                return new List<Game>() { mainModel.SelectedGame.Game };
            }
            else
            {
                return mainModel.SelectedGames?.Where(a => a != null).Select(a => a.Game).ToList();
            }
        }

        private void EnsureFilteredGamesCacheHooked()
        {
            if (filteredGamesCacheHooked)
            {
                return;
            }

            if (mainModel.GamesView?.CollectionView is INotifyCollectionChanged notify)
            {
                notify.CollectionChanged += (_, __) => filteredGamesCache = null;
            }

            filteredGamesCacheHooked = true;
        }

        private List<Game> GetFilteredGamesCore()
        {
            EnsureFilteredGamesCacheHooked();
            if (filteredGamesCache != null)
            {
                return filteredGamesCache;
            }

            var results = new List<Game>();
            var seen = new HashSet<Guid>();
            foreach (GamesCollectionViewEntry entry in mainModel.GamesView.CollectionView)
            {
                var game = entry?.Game;
                if (game != null && seen.Add(game.Id))
                {
                    results.Add(game);
                }
            }

            filteredGamesCache = results;
            return results;
        }

        public bool OpenPluginSettings(Guid pluginId)
        {
            throw new NotSupportedInFullscreenException("Cannot open plugin settings in Fullscreen mode.");
        }

        public void SwitchToLibraryView()
        {
            throw new NotSupportedInFullscreenException();
        }

        public void SelectGame(Guid gameId)
        {
            var game = mainModel.Database.Games.Get(gameId);
            if (game == null)
            {
                logger.Error($"Can't select game, game ID {gameId} not found.");
            }
            else
            {
                mainModel.SelectGame(game.Id);
            }
        }

        public void SelectGames(IEnumerable<Guid> gameIds)
        {
            throw new NotSupportedInFullscreenException();
        }

        public void ApplyFilterPreset(Guid filterId)
        {
            mainModel.ApplyFilterPreset(filterId);
        }

        public void ApplyFilterPreset(FilterPreset preset)
        {
            mainModel.ActiveFilterPreset = preset;
        }

        public Guid GetActiveFilterPreset()
        {
            return mainModel.AppSettings.Fullscreen.SelectedFilterPreset;
        }

        public FilterPresetSettings GetCurrentFilterSettings()
        {
            return mainModel.AppSettings.Fullscreen.FilterSettings.AsPresetSettings();
        }

        public void OpenSearch(string searchTerm)
        {
            throw new NotSupportedInFullscreenException();
        }

        public void OpenSearch(SearchContext context, string searchTerm)
        {
            throw new NotSupportedInFullscreenException();
        }

        public bool? OpenEditDialog(Guid gameId)
        {
            throw new NotSupportedInFullscreenException();
        }

        public bool? OpenEditDialog(List<Guid> gameIds)
        {
            throw new NotSupportedInFullscreenException();
        }

        public List<FilterPreset> GetSortedFilterPresets()
        {
            return mainModel.SortedFilterPresets.ToList();
        }

        public List<FilterPreset> GetSortedFilterFullscreenPresets()
        {
            return mainModel.SortedFilterFullscreenPresets.ToList();
        }

        public void ToggleFullscreenView()
        {
            UIDispatcher.Invoke(() => mainModel.ToggleGameDetailsCommand?.Execute(null));
        }
    }
}
