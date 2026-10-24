[CmdletBinding()]
param(
    [string]$RepositoryPath = "."
)

$prefix = "v" + (Get-Date).ToString("yyyyMM") + "."
$tags = & git -C $RepositoryPath tag --list "$prefix*"
if ($LASTEXITCODE -ne 0) {
    throw "git tag failed with exit code $LASTEXITCODE."
}

$maxSequence = -1

foreach ($tag in $tags) {
    if (-not $tag.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
        continue
    }

    $suffix = $tag.Substring($prefix.Length)
    $sequence = 0
    if ([int]::TryParse($suffix, [ref]$sequence) -and $sequence -gt $maxSequence) {
        $maxSequence = $sequence
    }
}

Write-Output ("{0}{1}" -f $prefix, ($maxSequence + 1))
