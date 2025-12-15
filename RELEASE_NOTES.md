# Release Notes (Unreleased)

## Performance & responsiveness

- Improved image loading pipeline (async decode + bounded concurrency + stronger cache keys) to reduce UI stalls when browsing image-heavy views.
- Reduced UI thread blocking and sync-over-async patterns in download, service, and WebView code paths to avoid hangs and thread starvation under load.
- Metadata downloads now run concurrently (bounded) to speed up library imports and bulk metadata updates.
- Faster emulator/ROM scanning via streaming directory enumeration, cached regex compilation, and more efficient “merge related files” handling for large libraries.
- Lower CPU/allocations in process monitoring and install size scanning, with safer enumeration and cancellation-aware traversal.
- Reduced repeated allocations/projections in API and view-model hot paths via caching and dispatcher fast-paths.

## Fixes

- Fixed `{ImageNameNoExt}` variable expansion edge cases.
- Hardened process starter tests against pre-existing processes and ShellExecute PID quirks.

## Notes

- Added a small HTML template file-content cache to avoid repeated disk reads on template changes.
