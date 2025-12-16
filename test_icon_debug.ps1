# Test script to debug icon rendering issues in Playnite extensions
# This helps identify why icons might be invisible but clickable

function Test-IconRendering {
    param(
        [string]$IconPath,
        [string]$IconType
    )

    Write-Host "Testing icon: $IconPath" -ForegroundColor Yellow
    Write-Host "Type: $IconType" -ForegroundColor Yellow

    # Test 1: Check if file exists
    if (Test-Path $IconPath) {
        Write-Host "✓ File exists" -ForegroundColor Green
        $fileInfo = Get-Item $IconPath
        Write-Host "  Size: $($fileInfo.Length) bytes"
        Write-Host "  Extension: $($fileInfo.Extension)"
    } else {
        Write-Host "✗ File does not exist" -ForegroundColor Red
        return
    }

    # Test 2: Try to load as image
    try {
        Add-Type -AssemblyName System.Windows.Forms
        $img = [System.Drawing.Image]::FromFile($IconPath)
        Write-Host "✓ Can load as image: $($img.Width)x$($img.Height)" -ForegroundColor Green
        $img.Dispose()
    } catch {
        Write-Host "✗ Cannot load as image: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 3: Check for common issues
    $ext = [System.IO.Path]::GetExtension($IconPath).ToLower()
    switch ($ext) {
        ".png" {
            # PNG files can have transparency issues
            Write-Host "⚠ PNG detected - check for transparency/metadata issues" -ForegroundColor Yellow
        }
        ".ico" {
            # ICO files can have multiple sizes
            Write-Host "⚠ ICO detected - ensure it contains proper sizes" -ForegroundColor Yellow
        }
        ".svg" {
            # SVG is not natively supported by WPF
            Write-Host "✗ SVG not directly supported by WPF" -ForegroundColor Red
        }
    }
}

# Test common icon scenarios
Write-Host "=== Icon Rendering Debug Tests ===" -ForegroundColor Cyan

# Test 1: Relative path (common issue)
Write-Host "`n--- Test 1: Relative Path ---" -ForegroundColor Cyan
Test-IconRendering -IconPath "icon.png" -IconType "Relative"

# Test 2: Absolute path
Write-Host "`n--- Test 2: Absolute Path ---" -ForegroundColor Cyan
$absPath = Join-Path $PSScriptRoot "icon.png"
Test-IconRendering -IconPath $absPath -IconType "Absolute"

# Test 3: Resource name
Write-Host "`n--- Test 3: Resource Name ---" -ForegroundColor Cyan
Test-IconRendering -IconPath "SpecialKIcon" -IconType "Resource"

# Test 4: Common problematic paths
Write-Host "`n--- Test 4: Common Issues ---" -ForegroundColor Cyan
Write-Host "Missing resource names will return null from ResourceProvider.GetResource()" -ForegroundColor Yellow
Write-Host "Invalid files will return null from File.Exists()" -ForegroundColor Yellow
Write-Host "Failed image loading will log errors but return null" -ForegroundColor Yellow

Write-Host "`n=== Debugging Tips ===" -ForegroundColor Cyan
Write-Host "1. Check Playnite log file for icon loading errors" -ForegroundColor White
Write-Host "2. Use absolute paths for file icons" -ForegroundColor White
Write-Host "3. Ensure resource names exist in ResourceProvider" -ForegroundColor White
Write-Host "4. Test with simple icons first" -ForegroundColor White
Write-Host "5. OrangeRed background indicates null Content in SidebarItem" -ForegroundColor White