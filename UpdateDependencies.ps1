#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

function EnsureEnvironmentVariable([string]$envVarName)
{
    If ([Environment]::GetEnvironmentVariable($envVarName) -eq $null)
    {
        throw "Can't find environment variable '$envVarName'"
    }
}

function SetEnvIfDefault([string]$envVarName, [string]$value)
{
    If ([Environment]::GetEnvironmentVariable($envVarName) -eq $null)
    {
        [Environment]::SetEnvironmentVariable($envVarName, $value)
    }
}

EnsureEnvironmentVariable "GITHUB_USER"
EnsureEnvironmentVariable "GITHUB_EMAIL"
EnsureEnvironmentVariable "GITHUB_PASSWORD"

SetEnvIfDefault 'COREFX_VERSION_URL' 'https://raw.githubusercontent.com/dotnet/versions/master/dotnet/corefx/master/Latest.txt'
SetEnvIfDefault 'GITHUB_ORIGIN_OWNER' $env:GITHUB_USER
SetEnvIfDefault 'GITHUB_UPSTREAM_OWNER' 'dotnet'
SetEnvIfDefault 'GITHUB_PROJECT' 'corefx'
SetEnvIfDefault 'GITHUB_UPSTREAM_BRANCH' 'master'


$CoreFxLatestVersion = Invoke-WebRequest $env:COREFX_VERSION_URL -UseBasicParsing
$CoreFxLatestVersion = $CoreFxLatestVersion.ToString().Trim()

# Updates the dir.props file with the latest CoreFX build number
function UpdateValidDependencyVersionsFile
{
    if (!$CoreFxLatestVersion)
    {
        Write-Error "Unable to find latest CoreFX version at $env:COREFX_VERSION_URL"
        return $false
    }

    $DirPropsPath = "$PSScriptRoot\dir.props"
    $DirPropsText = [IO.File]::ReadAllText($DirPropsPath)

    $CoreFxRegex = new-object Text.RegularExpressions.Regex '<CoreFxExpectedPrerelease>.*</CoreFxExpectedPrerelease>'

    $DirPropsText = $CoreFxRegex.Replace($DirPropsText, "<CoreFxExpectedPrerelease>$CoreFxLatestVersion</CoreFxExpectedPrerelease>")

    [IO.File]::WriteAllText($DirPropsPath, $DirPropsText)
    return $true
}

# Updates all the project.json files with out of date version numbers
function RunUpdatePackageDependencyVersions
{
    cmd /c $PSScriptRoot\build.cmd /t:UpdateInvalidPackageVersions | Out-Host

    return $LASTEXITCODE -eq 0
}

# Creates a Pull Request for the updated version numbers
function CreatePullRequest
{
    git add . | Out-Host

    $UserName = $env:GITHUB_USER
    $Email = $env:GITHUB_EMAIL
    $CommitMessage = "Updating CoreFX dependencies to $CoreFxLatestVersion"

    $env:GIT_COMMITTER_NAME = $UserName
    $env:GIT_COMMITTER_EMAIL = $Email
    git commit -m "$CommitMessage" --author "$UserName <$Email>" | Out-Host

    $RemoteUrl = "github.com/$env:GITHUB_ORIGIN_OWNER/$env:GITHUB_PROJECT.git"
    $RemoteBranchName = "UpdateDependencies$([DateTime]::UtcNow.ToString('yyyyMMddhhmmss'))"
    $RefSpec = "HEAD:refs/heads/$RemoteBranchName"

    Write-Host "git push https://$RemoteUrl $RefSpec"
    # pipe this to null so the password secret isn't in the logs
    git push "https://$($UserName):$env:GITHUB_PASSWORD@$RemoteUrl" $RefSpec 2>&1 | Out-Null

    $CreatePRBody = @"
    {
        "title": "$CommitMessage", 
        "head": "$($env:GITHUB_ORIGIN_OWNER):$RemoteBranchName", 
        "base": "$env:GITHUB_UPSTREAM_BRANCH" 
    }
"@

    $CreatePRHeaders = @{'Accept'='application/vnd.github.v3+json'; 'Authorization'="token $env:GITHUB_PASSWORD"}

    try
    {
        Invoke-WebRequest https://api.github.com/repos/$env:GITHUB_UPSTREAM_OWNER/$env:GITHUB_PROJECT/pulls -UseBasicParsing -Method Post -Body $CreatePRBody -Headers $CreatePRHeaders
    }
    catch
    {
        Write-Error $_.ToString()
        return $false
    }

    return $true
}

if (!(UpdateValidDependencyVersionsFile))
{
    Exit -1
}

if (!(RunUpdatePackageDependencyVersions))
{
    Exit -1
}

if (!(CreatePullRequest))
{
    Exit -1
}

Write-Host -ForegroundColor Green "Successfully updated dependencies from the latest build numbers"

exit $LastExitCode
