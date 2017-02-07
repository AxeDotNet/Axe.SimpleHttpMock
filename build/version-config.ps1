function getVersionConfiguration() {
    return @{
        "apiVersion" = 1
        "updateNumber" = 0
        "bugFix" = 0
        "buildNumber" = 4
        "preRelease" = $true
    }
}

function getVersionString($type) {
    $config = getVersionConfiguration;
    $basicVer = $config.apiVersion.ToString() + "." + $config.updateNumber.ToString() + "." + $config.bugFix.ToString();
    if ($type -eq 'ASSEMBLY_FILE_VERSION') {
        return "$basicVer." + $config.buildNumber.ToString();
    } elseif ($type -eq 'PACKAGE_VERSION') {
        return "$basicVer-*";
    } else {
        return $basicVer;
    }
}