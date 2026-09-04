$source = Join-Path $PSScriptRoot "solucion_lab9.md"
$output = Join-Path $PSScriptRoot "solucion_lab9.pdf"
$lines = Get-Content $source -Encoding UTF8 | ForEach-Object {
    ($_ -replace '#|`|\*\*|\|', '').Normalize([Text.NormalizationForm]::FormD) -replace '\p{Mn}', ''
}
$pages = @()
for ($index = 0; $index -lt $lines.Count; $index += 48) {
    $pages += ,($lines[$index..([Math]::Min($index + 47, $lines.Count - 1))])
}
$objects = @('<< /Type /Catalog /Pages 2 0 R >>')
$kids = (0..($pages.Count - 1) | ForEach-Object { "$(4 + $_ * 2) 0 R" }) -join ' '
$objects += "<< /Type /Pages /Kids [$kids] /Count $($pages.Count) >>"
$objects += '<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>'
foreach ($page in $pages) {
    $content = "BT`n/F1 9 Tf`n45 755 Td`n12 TL`n"
    foreach ($line in $page) {
        $safe = $line -replace '\\', '\\' -replace '\(', '\(' -replace '\)', '\)'
        $content += "($($safe.Substring(0, [Math]::Min(105, $safe.Length)))) Tj`n0 -12 Td`n"
    }
    $content += 'ET'
    $objects += '<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents ' + (5 + ($objects.Count - 3) * 2) + ' 0 R >>'
    $objects += "<< /Length $($content.Length) >>`nstream`n$content`nendstream"
}
$pdf = "%PDF-1.4`n"
$offsets = @(0)
for ($number = 0; $number -lt $objects.Count; $number++) {
    $offsets += $pdf.Length
    $pdf += "$($number + 1) 0 obj`n$($objects[$number])`nendobj`n"
}
$xref = $pdf.Length
$pdf += "xref`n0 $($objects.Count + 1)`n0000000000 65535 f `n"
for ($number = 1; $number -lt $offsets.Count; $number++) { $pdf += "{0:D10} 00000 n `n" -f $offsets[$number] }
$pdf += "trailer`n<< /Size $($objects.Count + 1) /Root 1 0 R >>`nstartxref`n$xref`n%%EOF`n"
[IO.File]::WriteAllBytes($output, [Text.Encoding]::ASCII.GetBytes($pdf))
Write-Output "Created $output"