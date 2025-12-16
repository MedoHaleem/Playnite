using Playnite.Common;
using Playnite.Database;
using Playnite.SDK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Playnite
{
    public class ImageSourceManager
    {
        private static ILogger logger = LogManager.GetLogger();
        private static GameDatabase database;
        public static MemoryCache Cache = new MemoryCache(Units.MegaBytesToBytes(100));
        private const string btmpPropsFld = "bitmappros";

        // Increase concurrent operations based on CPU cores for better performance
        private static readonly SemaphoreSlim decodeSemaphore = new SemaphoreSlim(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

        // Cache image dimensions to avoid repeated file access
        private static readonly ConcurrentDictionary<string, System.Windows.Size> imageDimensionsCache = new ConcurrentDictionary<string, System.Windows.Size>();

        // Limit dimension cache size to prevent memory bloat
        private const int MaxDimensionCacheSize = 1000;

        private static string GetCacheKey(string source, BitmapLoadProperties loadProperties)
        {
            if (loadProperties == null)
            {
                return source;
            }

            var dpiPart = loadProperties.DpiScale == null ? string.Empty : $"{loadProperties.DpiScale.Value.DpiScaleX}x{loadProperties.DpiScale.Value.DpiScaleY}";
            return $"{source}|{loadProperties.MaxDecodePixelWidth}x{loadProperties.MaxDecodePixelHeight}|{dpiPart}|{loadProperties.Scaling}";
        }

        public static void SetDatabase(GameDatabase db)
        {
            if (database != null)
            {
                database.DatabaseFileChanged -= Database_DatabaseFileChanged;
            }

            database = db;
            database.DatabaseFileChanged += Database_DatabaseFileChanged;
        }

        private static void Database_DatabaseFileChanged(object sender, DatabaseFileEventArgs args)
        {
            if (args.EventType == FileEvent.Removed)
            {
                Cache.TryRemove(args.FileId, out var file);
            }
        }

        public static string GetImagePath(string source)
        {
            if (source.IsNullOrEmpty())
            {
                return null;
            }

            if (source.StartsWith("resources:") || source.StartsWith("pack://"))
            {
                try
                {
                    var imagePath = source;
                    if (source.StartsWith("resources:"))
                    {
                        imagePath = source.Replace("resources:", "pack://application:,,,");
                    }

                    return imagePath;
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to create bitmap from resources " + source);
                    return null;
                }
            }

            if (StringExtensions.IsHttpUrl(source))
            {
                try
                {
                    var cachedFile = HttpFileCache.GetWebFile(source);
                    if (string.IsNullOrEmpty(cachedFile))
                    {
                        logger.Warn("Web file not found: " + source);
                        return null;
                    }

                    return cachedFile;
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"Failed to create bitmap from {source} file.");
                    return null;
                }
            }

            if (File.Exists(source))
            {
                return source;
            }

            if (database == null)
            {
                logger.Error("Cannot load database image, database not found.");
                return null;
            }

            try
            {
                return database.GetFullFilePath(source);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, $"Failed to get bitmap from {source} database file.");
                return null;
            }
        }

        public static BitmapSource GetResourceImage(string resourceKey, bool cached, BitmapLoadProperties loadProperties = null)
        {
            var cacheKey = GetCacheKey(resourceKey, loadProperties);
            if (cached && Cache.TryGet(cacheKey, out var image))
            {
                BitmapLoadProperties existingMetadata = null;
                if (image.Metadata.TryGetValue(btmpPropsFld, out object metaValue))
                {
                    existingMetadata = (BitmapLoadProperties)metaValue;
                }

                if (existingMetadata == loadProperties)
                {
                    return image.CacheObject as BitmapSource;
                }
                else
                {
                    Cache.TryRemove(cacheKey);
                }
            }

            var resource = ResourceProvider.GetResource(resourceKey) as BitmapSource;
            if (loadProperties?.MaxDecodePixelWidth > 0 && resource?.PixelWidth > loadProperties?.MaxDecodePixelWidth)
            {
                resource = resource.GetClone(loadProperties);
            }

            if (cached && resource != null)
            {
                long imageSize = 0;
                try
                {
                    imageSize = resource.GetSizeInMemory();
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to get image memory size: {resourceKey}");
                }

                if (imageSize > 0)
                {
                    Cache.TryAdd(cacheKey, resource, imageSize, new Dictionary<string, object>
                    {
                        { btmpPropsFld, loadProperties }
                    });
                }
            }

            return resource;
        }

        public static BitmapSource GetImage(string source, bool cached, BitmapLoadProperties loadProperties = null)
        {
            if (DesignerTools.IsInDesignMode)
            {
                cached = false;
            }

            if (source.IsNullOrEmpty())
            {
                return null;
            }

            var cacheKey = GetCacheKey(source, loadProperties);
            if (cached && Cache.TryGet(cacheKey, out var image))
            {
                BitmapLoadProperties existingMetadata = null;
                if (image.Metadata.TryGetValue(btmpPropsFld, out object metaValue))
                {
                    existingMetadata = (BitmapLoadProperties)metaValue;
                }

                if (existingMetadata == loadProperties)
                {
                    return image.CacheObject as BitmapSource;
                }
                else
                {
                    Cache.TryRemove(cacheKey);
                }
            }

            var imageData = LoadImageCore(source, loadProperties, cached, cacheKey);
            return imageData;
        }

        public static async Task<BitmapSource> GetImageAsync(string source, bool cached, BitmapLoadProperties loadProperties = null, CancellationToken cancelToken = default)
        {
            if (DesignerTools.IsInDesignMode)
            {
                cached = false;
            }

            if (source.IsNullOrEmpty())
            {
                return null;
            }

            var cacheKey = GetCacheKey(source, loadProperties);
            if (cached && Cache.TryGet(cacheKey, out var image))
            {
                BitmapLoadProperties existingMetadata = null;
                if (image.Metadata.TryGetValue(btmpPropsFld, out object metaValue))
                {
                    existingMetadata = (BitmapLoadProperties)metaValue;
                }

                if (existingMetadata == loadProperties)
                {
                    return image.CacheObject as BitmapSource;
                }
                else
                {
                    Cache.TryRemove(cacheKey);
                }
            }

            await decodeSemaphore.WaitAsync(cancelToken).ConfigureAwait(false);
            try
            {
                return await Task.Run(() => LoadImageCore(source, loadProperties, cached, cacheKey), cancelToken).ConfigureAwait(false);
            }
            finally
            {
                decodeSemaphore.Release();
            }
        }

        private static BitmapSource LoadImageCore(string source, BitmapLoadProperties loadProperties, bool cached, string cacheKey)
        {
            if (source.StartsWith("resources:") || source.StartsWith("pack://"))
            {
                try
                {
                    var imagePath = source;
                    if (source.StartsWith("resources:"))
                    {
                        imagePath = source.Replace("resources:", "pack://application:,,,");
                    }

                    var streamInfo = Application.GetResourceStream(new Uri(imagePath));
                    using (var stream = streamInfo.Stream)
                    {
                        var imageData = BitmapExtensions.BitmapFromStream(stream, loadProperties);
                        if (imageData != null)
                        {
                            TryAddToCache(cacheKey, imageData, loadProperties, cached);
                            return imageData;
                        }
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to create bitmap from resources " + source);
                    return null;
                }
            }

            if (StringExtensions.IsHttpUrl(source))
            {
                try
                {
                    var cachedFile = HttpFileCache.GetWebFile(source);
                    if (string.IsNullOrEmpty(cachedFile))
                    {
                        logger.Warn("Web file not found: " + source);
                        return null;
                    }

                    var imageData = BitmapExtensions.BitmapFromFile(cachedFile, loadProperties);
                    if (imageData != null)
                    {
                        TryAddToCache(cacheKey, imageData, loadProperties, cached);
                    }

                    return imageData;
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"Failed to create bitmap from {source} file.");
                    return null;
                }
            }

            if (File.Exists(source))
            {
                try
                {
                    var imageData = BitmapExtensions.BitmapFromFile(source, loadProperties);
                    if (imageData != null)
                    {
                        TryAddToCache(cacheKey, imageData, loadProperties, cached);
                        return imageData;
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to create bitmap from " + source);
                    return null;
                }
            }

            try
            {
                if (database == null)
                {
                    logger.Error("Cannot load database image, database not found.");
                    return null;
                }

                try
                {
                    var imageData = database.GetFileAsImage(source, loadProperties);
                    if (imageData == null)
                    {
                        logger.Warn("Image not found in database: " + source);
                        return null;
                    }
                    else
                    {
                        TryAddToCache(cacheKey, imageData, loadProperties, cached);
                        return imageData;
                    }
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"Failed to get bitmap from {source} database file.");
                    return null;
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to load image from database.");
                return null;
            }
        }

        private static System.Windows.Size? GetImageDimensionsFast(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            return imageDimensionsCache.GetOrAdd(imagePath, path =>
            {
                try
                {
                    // Clean up cache if it gets too large
                    if (imageDimensionsCache.Count > MaxDimensionCacheSize)
                    {
                        var keysToRemove = imageDimensionsCache.Keys.Take(MaxDimensionCacheSize / 4).ToList();
                        foreach (var key in keysToRemove)
                        {
                            imageDimensionsCache.TryRemove(key, out _);
                        }
                    }

                    // Fast dimension read without full decoding
                    using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var decoder = BitmapDecoder.Create(fileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                        if (decoder.Frames.Count > 0)
                        {
                            var frame = decoder.Frames[0];
                            return new System.Windows.Size(frame.PixelWidth, frame.PixelHeight);
                        }
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Debug(e, $"Failed to get image dimensions for: {imagePath}");
                }

                return new System.Windows.Size(0, 0);
            });
        }

        private static bool ShouldSkipImageDecoding(string imagePath, BitmapLoadProperties loadProperties)
        {
            if (loadProperties?.MaxDecodePixelWidth <= 0)
            {
                return false;
            }

            var dimensions = GetImageDimensionsFast(imagePath);
            if (dimensions.HasValue && dimensions.Value.Width > 0)
            {
                // If image is much larger than needed, skip for now and load thumbnail first
                if (dimensions.Value.Width > loadProperties.MaxDecodePixelWidth * 2)
                {
                    return true;
                }
            }

            return false;
        }

        private static void TryAddToCache(string cacheKey, BitmapSource imageData, BitmapLoadProperties loadProperties, bool cached)
        {
            if (!cached || imageData == null)
            {
                return;
            }

            Cache.TryAdd(cacheKey, imageData, imageData.GetSizeInMemory(),
                new Dictionary<string, object>
                {
                    { btmpPropsFld, loadProperties }
                });
        }
    }
}
