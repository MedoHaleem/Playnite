using Playnite.DesktopApp.ViewModels;
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

namespace Playnite.DesktopApp.API
{
    public class MainViewAPI : IMainViewAPI
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private DesktopAppViewModel mainModel;
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
            get => mainModel.AppSettings.ViewSettings.GamesViewType;
            set => mainModel.AppSettings.ViewSettings.GamesViewType = value;
        }

        public FullscreenView ActiveFullscreenView { get; } = FullscreenView.List;

        public SortOrder SortOrder
        {
            get => mainModel.AppSettings.ViewSettings.SortingOrder;
            set => mainModel.AppSettings.ViewSettings.SortingOrder = value;
        }

        public SortOrderDirection SortOrderDirection
        {
            get => mainModel.AppSettings.ViewSettings.SortingOrderDirection;
            set => mainModel.AppSettings.ViewSettings.SortingOrderDirection = value;
        }

        public GroupableField Grouping
        {
            get => mainModel.AppSettings.ViewSettings.GroupingOrder;
            set => mainModel.AppSettings.ViewSettings.GroupingOrder = value;
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

        public MainViewAPI(DesktopAppViewModel mainModel)
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
                return mainModel.SelectedGames?.Select(a => a.Game).ToList();
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
            return mainModel.OpenPluginSettings(pluginId);
        }

        public void SwitchToLibraryView()
        {
            mainModel.SwitchToLibraryView();
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
            mainModel.SelectGames(gameIds);
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
            return mainModel.AppSettings.SelectedFilterPreset;
        }

        public FilterPresetSettings GetCurrentFilterSettings()
        {
            return mainModel.AppSettings.FilterSettings.AsPresetSettings();
        }

        public void OpenSearch(string searchTerm)
        {
            mainModel.OpenSearch(searchTerm);
        }

        public void OpenSearch(SearchContext context, string searchTerm)
        {
            mainModel.OpenSearch(context, searchTerm);
        }

        public bool? OpenEditDialog(Guid gameId)
        {
            var game = mainModel.Database.Games.Get(gameId);
            if (game is null)
                return null;

            return mainModel.GamesEditor.EditGame(game);
        }

        public bool? OpenEditDialog(List<Guid> gameIds)
        {
            var games = mainModel.Database.Games.Get(gameIds);
            if (!games.HasItems())
                return null;

            return mainModel.GamesEditor.EditGames(games);
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
            throw new NotSupportedInDesktopException();
        }
    }
}
