//#define DEBUGOUT
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace ca.whittaker.Maui.Controls
{
    public sealed record ImageAsset(ImageSource Source, int DipSize);

    public sealed class ResourceHelper : IDisposable
    {
        private static readonly ConcurrentDictionary<string, ImageSource?> _imageCache =
            new(StringComparer.Ordinal);

        // Cache manifest names per assembly to avoid repeated reflection.
        private static readonly ConcurrentDictionary<Assembly, HashSet<string>> _manifestNameCache =
            new();

        private static HashSet<string> GetManifestNames(Assembly asm)
            => _manifestNameCache.GetOrAdd(asm, a =>
                new HashSet<string>(a.GetManifestResourceNames(), StringComparer.Ordinal));

        private static bool ResourceExists(string resourceName, Assembly assembly)
            => GetManifestNames(assembly).Contains(resourceName);

        private static string AssemblyNameOf(object o)
            => o.GetType().Assembly?.GetName()?.Name ?? string.Empty;

        // ----------------- High-level API -----------------

        /// <summary>
        /// Returns an ImageAsset containing the image and the DIP size the UI should use for layout.
        /// </summary>
        public ImageAsset? GetImageAsset(
            ButtonStateEnum state,
            ButtonIconEnum icon,
            SizeEnum size,
            CancellationToken cancellationToken = default,
            bool enforceMinTouchTarget = true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var asm = GetType().Assembly;
            var asmName = AssemblyNameOf(this);

            // Decide both the layout DIP and the pixel bucket here
            var (dip, bucketPx) = DeviceHelper.GetLayoutAndBucket(size, enforceMinTouchTarget);
            var suffixTheme = GetResourceThemeSuffix();
            var suffixState = GetResourceStateSuffix(state);
            var iconName = icon.ToString().ToLowerInvariant();

            // Build fully qualified resource name (must match your folder & naming scheme)
            // Example: My.Assembly.Resources.Images.size120/add_120_dark_disabled.png
            var resourceName =
                $"{asmName}.Resources.Images.size{bucketPx}." +
                $"{iconName}_{bucketPx}{suffixTheme}{suffixState}.png";

#if DEBUG
            var wasCached = _imageCache.ContainsKey(resourceName);
            Debug.WriteLine($"[ResourceHelper] Request state={state} icon={icon} size={size} dip={dip} bucket={bucketPx} theme={suffixTheme} stateSuffix={suffixState} resource={resourceName} cached={wasCached} enforceMin={enforceMinTouchTarget}");
#endif

            // Hot path: serve from cache
            if (_imageCache.TryGetValue(resourceName, out var cached) && cached is not null)
            {
#if DEBUG
                Debug.WriteLine($"[ResourceHelper] Cache hit for {resourceName} dip={dip}");
#endif
                return new ImageAsset(cached, dip);
            }

            // Validate existence once (avoids exception-driven flow)
            if (!ResourceExists(resourceName, asm))
            {
#if DEBUG
                Debug.WriteLine($"[ResourceHelper] Missing resource {resourceName}");
#endif
                return null;
            }

            try
            {
                var src = ImageSource.FromResource(resourceName, asm);
                _imageCache[resourceName] = src;
#if DEBUG
                Debug.WriteLine($"[ResourceHelper] Loaded resource {resourceName} dip={dip}");
#endif
                return new ImageAsset(src, dip);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"[ResourceHelper] Load error for {resourceName}: {ex}");
#endif
                return null;
            }
        }


        /// <summary>
        /// Convenience factory: returns an Image with Width/Height set to the correct DIP size.
        /// </summary>
        public Image? CreateImage(ButtonStateEnum state, ButtonIconEnum icon, SizeEnum size, CancellationToken ct = default, bool enforceMinTouchTarget = true)
        {
            var asset = GetImageAsset(state, icon, size, ct, enforceMinTouchTarget);
            if (asset is null) return null;

            return new Image
            {
                Source = asset.Source,
                WidthRequest = asset.DipSize,
                HeightRequest = asset.DipSize,
                MinimumWidthRequest = asset.DipSize,     // <— ensure WinUI respects it
                MinimumHeightRequest = asset.DipSize,
                Aspect = Aspect.AspectFit
            };
        }

        /// <summary>
        /// Convenience factory: returns an ImageButton sized and padded correctly (fixes WinUI inflation).
        /// </summary>
        public ImageButton? CreateImageButton(ButtonStateEnum state, ButtonIconEnum icon, SizeEnum size, CancellationToken ct = default, bool enforceMinTouchTarget = true)
        {
            var asset = GetImageAsset(state, icon, size, ct, enforceMinTouchTarget);
            if (asset is null) return null;

            return new ImageButton
            {
                Source = asset.Source,
                WidthRequest = asset.DipSize,
                HeightRequest = asset.DipSize,
                MinimumWidthRequest = asset.DipSize,     // <— clamp WinUI default MinHeight/MinWidth
                MinimumHeightRequest = asset.DipSize,
                Padding = 0,
                BorderWidth = 0,
                CornerRadius = 0,
                BackgroundColor = Colors.Transparent
            };
        }

        public Image? CreateImageForText(
       ButtonStateEnum state, ButtonIconEnum icon, SizeEnum size,
       double fontSize, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var asm = GetType().Assembly;
            var asmName = AssemblyNameOf(this);

            // Clamp icon DIP to text height and choose raster bucket
            var (dip, bucketPx) = DeviceHelper.GetIconForText(size, fontSize);

            var suffixTheme = GetResourceThemeSuffix();
            var suffixState = GetResourceStateSuffix(state);
            var iconName = icon.ToString().ToLowerInvariant();

            var resourceName =
                $"{asmName}.Resources.Images.size{bucketPx}." +
                $"{iconName}_{bucketPx}{suffixTheme}{suffixState}.png";

#if DEBUGOUT
    Debug.WriteLine($"[ResourceHelper] CreateImageForText dip={dip}, bucket={bucketPx}, name={resourceName}");
#endif

            if (_imageCache.TryGetValue(resourceName, out var cached) && cached is not null)
                return new Image
                {
                    Source = cached,
                    WidthRequest = dip,
                    HeightRequest = dip,
                    MinimumWidthRequest = dip,
                    MinimumHeightRequest = dip,
                    Aspect = Aspect.AspectFit
                };

            if (!ResourceExists(resourceName, asm))
                return null;

            try
            {
                var src = ImageSource.FromResource(resourceName, asm);
                _imageCache[resourceName] = src;

                return new Image
                {
                    Source = src,
                    WidthRequest = dip,
                    HeightRequest = dip,
                    MinimumWidthRequest = dip,
                    MinimumHeightRequest = dip,
                    Aspect = Aspect.AspectFit
                };
            }
            catch
            {
                return null;
            }
        }

        public ImageButton? CreateImageButtonForText(
            ButtonStateEnum state, ButtonIconEnum icon, SizeEnum size,
            double fontSize, CancellationToken ct = default)
        {
            var img = CreateImageForText(state, icon, size, fontSize, ct);
            if (img is null) return null;

            return new ImageButton
            {
                Source = img.Source,
                WidthRequest = img.WidthRequest,
                HeightRequest = img.HeightRequest,
                MinimumWidthRequest = img.MinimumWidthRequest,
                MinimumHeightRequest = img.MinimumHeightRequest,
                Padding = 0,
                BorderWidth = 0,
                CornerRadius = 0,
                BackgroundColor = Colors.Transparent
            };
        }

        // ----------------- Legacy-style API (kept for compatibility) -----------------

        /// <summary>
        /// Legacy: returns ImageSource only. Prefer GetImageAsset/CreateImage/CreateImageButton to ensure correct layout size.
        /// </summary>
        public ImageSource? GetImageSource(
            ButtonStateEnum buttonState,
            ButtonIconEnum baseButtonType,
            SizeEnum sizeEnum,
            CancellationToken cancellationToken = default,
            bool enforceMinTouchTarget = true)
        {
            // Delegate to new API and discard DIP (callers must set Width/Height if they use this).
            return GetImageAsset(buttonState, baseButtonType, sizeEnum, cancellationToken, enforceMinTouchTarget)?.Source;
        }

        // ----------------- Theme/State helpers -----------------

        private static string GetResourceThemeSuffix()
        {
            var theme = GetCurrentTheme();
            return theme == AppTheme.Dark ? "_dark" : "_light";
        }

        private static string GetResourceStateSuffix(ButtonStateEnum state)
            => state == ButtonStateEnum.Disabled ? "_disabled" : string.Empty;

        private static AppTheme GetCurrentTheme()
        {
            try { return Application.Current?.RequestedTheme ?? AppTheme.Unspecified; }
            catch { return AppTheme.Unspecified; }
        }

        // ----------------- IDisposable -----------------

        public void Dispose() => GC.SuppressFinalize(this);
    }
}




////#define DEBUGOUT
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui.Graphics;

//namespace ca.whittaker.Maui.Controls
//{
//    public sealed record ImageAsset(ImageSource Source, int DipSize);

//    public sealed class ResourceHelper : IDisposable
//    {
//        private static readonly ConcurrentDictionary<string, Lazy<ImageSource?>> _imageCache =
//            new(StringComparer.Ordinal);

//        private static string AssemblyNameOf(object o)
//            => o.GetType().Assembly?.GetName()?.Name ?? string.Empty;

//        private static IEnumerable<string> BuildResourceCandidates(
//            string asmName,
//            string iconName,
//            int bucketPx,
//            string themeSuffix,
//            string stateSuffix)
//        {
//            var baseName = $"{asmName}.Resources.Images.size{bucketPx}.{iconName}_{bucketPx}";
//            var candidates = new[]
//            {
//                themeSuffix + stateSuffix,
//                themeSuffix,
//                stateSuffix,
//                string.Empty
//            };

//            var seen = new HashSet<string>(StringComparer.Ordinal);

//            foreach (var suffix in candidates)
//            {
//                var resourceName = suffix.Length > 0
//                    ? $"{baseName}{suffix}.png"
//                    : $"{baseName}.png";

//                if (seen.Add(resourceName))
//                    yield return resourceName;
//            }
//        }

//        private ImageAsset? TryLoadAsset(
//            Assembly asm,
//            string asmName,
//            string iconName,
//            int bucketPx,
//            int dip,
//            string themeSuffix,
//            string stateSuffix,
//            CancellationToken cancellationToken)
//        {
//            var tried = new List<string>();

//            foreach (var resourceName in BuildResourceCandidates(asmName, iconName, bucketPx, themeSuffix, stateSuffix))
//            {
//                cancellationToken.ThrowIfCancellationRequested();

//                tried.Add(resourceName);

//                var source = GetOrLoadImage(asm, resourceName);
//                if (source != null)
//                    return new ImageAsset(source, dip);
//            }

//            Debug.WriteLine($"[ResourceHelper] Unable to locate resource for {iconName} ({bucketPx}px). Tried: {string.Join(", ", tried)}");
//            return null;
//        }

//        private static ImageSource? GetOrLoadImage(Assembly asm, string resourceName)
//        {
//            var loader = _imageCache.GetOrAdd(resourceName, name =>
//                new Lazy<ImageSource?>(() => LoadImageSource(asm, name), LazyThreadSafetyMode.ExecutionAndPublication));

//            return loader.Value;
//        }

//        private static ImageSource? LoadImageSource(Assembly asm, string resourceName)
//        {
//            try
//            {
//                using var resourceStream = asm.GetManifestResourceStream(resourceName);
//                if (resourceStream == null)
//                    return null;

//                using var memory = new MemoryStream();
//                resourceStream.CopyTo(memory);
//                var bytes = memory.ToArray();

//                if (bytes.Length == 0)
//                    return null;

//                return new StreamImageSource
//                {
//                    Stream = token =>
//                    {
//                        token.ThrowIfCancellationRequested();
//                        return Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
//                    }
//                };
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine($"[ResourceHelper] Failed to load {resourceName}: {ex}");
//                return null;
//            }
//        }

//        // ----------------- High-level API -----------------

//        /// <summary>
//        /// Returns an ImageAsset containing the image and the DIP size the UI should use for layout.
//        /// </summary>
//        public ImageAsset? GetImageAsset(
//            ButtonStateEnum state,
//            ButtonIconEnum icon,
//            SizeEnum size,
//            CancellationToken cancellationToken = default)
//        {
//            cancellationToken.ThrowIfCancellationRequested();

//            var asm = GetType().Assembly;
//            var asmName = AssemblyNameOf(this);

//            var (dip, bucketPx) = DeviceHelper.GetLayoutAndBucket(size);
//            var suffixTheme = GetResourceThemeSuffix();
//            var suffixState = GetResourceStateSuffix(state);
//            var iconName = icon.ToString().ToLowerInvariant();

//#if DEBUGOUT
//            Debug.WriteLine($"[ResourceHelper] Request: dip={dip}, bucket={bucketPx}, icon={iconName}");
//#endif

//            return TryLoadAsset(asm, asmName, iconName, bucketPx, dip, suffixTheme, suffixState, cancellationToken);
//        }

//        /// <summary>
//        /// Convenience factory: returns an Image with Width/Height set to the correct DIP size.
//        /// </summary>
//        public Image? CreateImage(ButtonStateEnum state, ButtonIconEnum icon, SizeEnum size, CancellationToken ct = default)
//        {
//            var asset = GetImageAsset(state, icon, size, ct);
//            if (asset is null) return null;

//            return new Image
//            {
//                Source = asset.Source,
//                WidthRequest = asset.DipSize,
//                HeightRequest = asset.DipSize,
//                MinimumWidthRequest = asset.DipSize,     // <— ensure WinUI respects it
//                MinimumHeightRequest = asset.DipSize,
//                Aspect = Aspect.AspectFit
//            };
//        }

//        /// <summary>
//        /// Convenience factory: returns an ImageButton sized and padded correctly (fixes WinUI inflation).
//        /// </summary>
//        public ImageButton? CreateImageButton(ButtonStateEnum state, ButtonIconEnum icon, SizeEnum size, CancellationToken ct = default)
//        {
//            var asset = GetImageAsset(state, icon, size, ct);
//            if (asset is null) return null;

//            return new ImageButton
//            {
//                Source = asset.Source,
//                WidthRequest = asset.DipSize,
//                HeightRequest = asset.DipSize,
//                MinimumWidthRequest = asset.DipSize,     // <— clamp WinUI default MinHeight/MinWidth
//                MinimumHeightRequest = asset.DipSize,
//                Padding = 0,
//                BorderWidth = 0,
//                CornerRadius = 0,
//                BackgroundColor = Colors.Transparent
//            };
//        }

//        public Image? CreateImageForText(
//       ButtonStateEnum state, ButtonIconEnum icon, SizeEnum size,
//       double fontSize, CancellationToken ct = default)
//        {
//            ct.ThrowIfCancellationRequested();

//            var asm = GetType().Assembly;
//            var asmName = AssemblyNameOf(this);

//            var (dip, bucketPx) = DeviceHelper.GetIconForText(size, fontSize);

//            var suffixTheme = GetResourceThemeSuffix();
//            var suffixState = GetResourceStateSuffix(state);
//            var iconName = icon.ToString().ToLowerInvariant();

//#if DEBUGOUT
//    Debug.WriteLine($"[ResourceHelper] CreateImageForText dip={dip}, bucket={bucketPx}, icon={iconName}");
//#endif

//            var asset = TryLoadAsset(asm, asmName, iconName, bucketPx, dip, suffixTheme, suffixState, ct);
//            if (asset is null) return null;

//            return new Image
//            {
//                Source = asset.Source,
//                WidthRequest = asset.DipSize,
//                HeightRequest = asset.DipSize,
//                MinimumWidthRequest = asset.DipSize,
//                MinimumHeightRequest = asset.DipSize,
//                Aspect = Aspect.AspectFit
//            };
//        }

//        public ImageButton? CreateImageButtonForText(
//            ButtonStateEnum state, ButtonIconEnum icon, SizeEnum size,
//            double fontSize, CancellationToken ct = default)
//        {
//            var img = CreateImageForText(state, icon, size, fontSize, ct);
//            if (img is null) return null;

//            return new ImageButton
//            {
//                Source = img.Source,
//                WidthRequest = img.WidthRequest,
//                HeightRequest = img.HeightRequest,
//                MinimumWidthRequest = img.MinimumWidthRequest,
//                MinimumHeightRequest = img.MinimumHeightRequest,
//                Padding = 0,
//                BorderWidth = 0,
//                CornerRadius = 0,
//                BackgroundColor = Colors.Transparent
//            };
//        }

//        // ----------------- Legacy-style API (kept for compatibility) -----------------

//        /// <summary>
//        /// Legacy: returns ImageSource only. Prefer GetImageAsset/CreateImage/CreateImageButton to ensure correct layout size.
//        /// </summary>
//        public ImageSource? GetImageSource(
//            ButtonStateEnum buttonState,
//            ButtonIconEnum baseButtonType,
//            SizeEnum sizeEnum,
//            CancellationToken cancellationToken = default)
//        {
//            // Delegate to new API and discard DIP (callers must set Width/Height if they use this).
//            return GetImageAsset(buttonState, baseButtonType, sizeEnum, cancellationToken)?.Source;
//        }

//        // ----------------- Theme/State helpers -----------------

//        private static string GetResourceThemeSuffix()
//        {
//            var theme = GetCurrentTheme();
//            return theme == AppTheme.Dark ? "_dark" : "_light";
//        }

//        private static string GetResourceStateSuffix(ButtonStateEnum state)
//            => state == ButtonStateEnum.Disabled ? "_disabled" : string.Empty;

//        private static AppTheme GetCurrentTheme()
//        {
//            try { return Application.Current?.RequestedTheme ?? AppTheme.Unspecified; }
//            catch { return AppTheme.Unspecified; }
//        }

//        // ----------------- IDisposable -----------------

//        public void Dispose() => GC.SuppressFinalize(this);
//    }
//}
