param
( 
    [string]$fileFullPath,
    [string]$version
)

[xml]$myXML = Get-Content $fileFullPath

$myXML.package.metadata.version = $version

$myXML.Save($fileFullPath)