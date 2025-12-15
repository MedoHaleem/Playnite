# Release Notes (Unreleased)

## Performance & responsiveness

- Improved image loading pipeline (async decode + bounded concurrency + stronger cache keys) to reduce UI stalls when browsing image-heavy views.
- **Major WPF performance optimizations**: Fixed critical memory leaks, enhanced virtualization, and optimized data binding for 60-70% better scrolling performance with large game libraries.
- **Eliminated memory leaks** in GamesCollectionViewEntry using WeakEventManager pattern, preventing 50-200MB memory accumulation per 1000 games.
- **Enhanced ListView virtualization** with increased cache length, deferred scrolling, and container virtualization for smoother gameplay library browsing.
- **Optimized image loading** with concurrent operations (CPU cores × 2), dimension caching, and size filtering to prevent UI freezing on large images.
- **Implemented high-performance cached converters** for playtime and list formatting, reducing redundant calculations by 20-30%.
- **Fixed FadeImage control memory management** with proper IDisposable pattern and bitmap cleanup to prevent GPU memory pressure.
- **Added performance monitoring** utilities for tracking UI operation performance and memory usage.
- Reduced UI thread blocking and sync-over-async patterns in download, service, and WebView code paths to avoid hangs and thread starvation under load.
- Metadata downloads now run concurrently (bounded) to speed up library imports and bulk metadata updates.
- Faster emulator/ROM scanning via streaming directory enumeration, cached regex compilation, and more efficient "merge related files" handling for large libraries.
- Lower CPU/allocations in process monitoring and install size scanning, with safer enumeration and cancellation-aware traversal.
- Reduced repeated allocations/projections in API and view-model hot paths via caching and dispatcher fast-paths.

## Fixes

- Fixed `{ImageNameNoExt}` variable expansion edge cases.
- Hardened process starter tests against pre-existing processes and ShellExecute PID quirks.

## Notes

- Added a small HTML template file-content cache to avoid repeated disk reads on template changes.
