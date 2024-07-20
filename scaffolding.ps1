$solutionName = "CostTracker"
$srcFolder = "./src"
mkdir $srcFolder

$libsFolder = "$srcFolder/libs"
mkdir $libsFolder

$srvFolder = "$srcFolder/srvs"
mkdir $srvFolder

function New-ClassLib {
    param (
      [string]$ProjectName,
      [string]$ProjectFolder,
      [bool] $NullableDisable
    )

    if (-not $ProjectName) {
        Write-Output "Usage: New-ClassLibWithNullableDisable -ProjectName <ProjectName>"
        return
    }

    # Create the class library project
    dotnet new classlib -o "$ProjectFolder/$ProjectName"

    # print to the project directory
    Write-Output "Project $ProjectName created in $ProjectFolder"

    # Add Nullable disable to the .csproj file
    $prjFilePath = "$ProjectFolder/$ProjectName/$ProjectName.csproj"
    Disable-Nullable($prjFilePath)

    dotnet sln "$srcFolder/$SolutionName.sln" add $prjFilePath
}


function Disable-Nullable {
    param (
      [string]$ProjectFilePath
    )

    [xml]$csproj = Get-Content -Path $ProjectFilePath
    $propertyGroup = $csproj.Project.PropertyGroup
    $nullableElement = $propertyGroup.Nullable

    write-host $nullableElement -ForegroundColor Green
    
    if ($null -eq $nullableElement) {
        $nullableElement = $csproj.CreateElement("Nullable")
        $nullableElement.InnerText = "disable"
        $propertyGroup.AppendChild($nullableElement)
    } else {
        $propertyGroup.Nullable = "disable"
    }
    
    $csproj.Save($ProjectFilePath)
}


dotnet new sln -n "$solutionName" -o "$srcFolder"

New-ClassLib -ProjectName "$solutionName.Domain.Core"     -ProjectFolder "$libsFolder" -NullableDisable $true

New-ClassLib -ProjectName "$solutionName.ApiClient"       -ProjectFolder "$libsFolder" -NullableDisable $true
dotnet add "$libsFolder/$solutionName.ApiClient/$solutionName.ApiClient.csproj" reference "$libsFolder/$solutionName.Domain.Core/$solutionName.Domain.Core.csproj"

# write readme.md in the project folder
"
# $solutionName.ApiClient
- ApiClient is Generated
" > "$libsFolder/$solutionName.ApiClient/README.md"


# entities shared
New-ClassLib -ProjectName "$solutionName.Domain.Entities" -ProjectFolder "$libsFolder" -NullableDisable $true

# in "$solutionName.Domain.Entities", add project ref to "$solutionName.Domain.Core"
dotnet add "$libsFolder/$solutionName.Domain.Entities/$solutionName.Domain.Entities.csproj" reference "$libsFolder/$solutionName.Domain.Core/$solutionName.Domain.Core.csproj"
dotnet add "$libsFolder/$solutionName.Domain.Entities/$solutionName.Domain.Entities.csproj" package Microsoft.EntityFrameworkCore --version 8.0.4

# entities repo
New-ClassLib -ProjectName "$solutionName.Domain.Repositories" -ProjectFolder "$libsFolder" -NullableDisable $true
dotnet add "$libsFolder/$solutionName.Domain.Repositories/$solutionName.Domain.Repositories.csproj" package Npgsql --version 8.0.3
dotnet add "$libsFolder/$solutionName.Domain.Repositories/$solutionName.Domain.Repositories.csproj" reference "$libsFolder/$solutionName.Domain.Entities/$solutionName.Domain.Entities.csproj"

# entities postgres
New-ClassLib -ProjectName "$solutionName.Domain.Entities.Postgres" -ProjectFolder "$libsFolder" -NullableDisable $true
dotnet add "$libsFolder/$solutionName.Domain.Entities.Postgres/$solutionName.Domain.Entities.Postgres.csproj" package Npgsql --version 8.0.3
dotnet add "$libsFolder/$solutionName.Domain.Entities.Postgres/$solutionName.Domain.Entities.Postgres.csproj" reference "$libsFolder/$solutionName.Domain.Repositories/$solutionName.Domain.Repositories.csproj"

# entities sqlite
New-ClassLib -ProjectName "$solutionName.Domain.Entities.Sqlite" -ProjectFolder "$libsFolder" -NullableDisable $true
dotnet add "$libsFolder/$solutionName.Domain.Entities.Sqlite/$solutionName.Domain.Entities.Sqlite.csproj" package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.6
dotnet add "$libsFolder/$solutionName.Domain.Entities.Sqlite/$solutionName.Domain.Entities.Sqlite.csproj" reference "$libsFolder/$solutionName.Domain.Repositories/$solutionName.Domain.Repositories.csproj"


New-ClassLib -ProjectName "$solutionName.Domain.Srv.Services" -ProjectFolder "$srvFolder" -NullableDisable $true
$svcPrjFilePath = "$srvFolder/$solutionName.Domain.Srv.Services/$solutionName.Domain.Srv.Services.csproj"
dotnet add $svcPrjFilePath reference "$libsFolder/$solutionName.Domain.Entities.Postgres/$solutionName.Domain.Entities.Postgres.csproj"
dotnet add $svcPrjFilePath reference "$libsFolder/$solutionName.Domain.Entities.Sqlite/$solutionName.Domain.Entities.Sqlite.csproj"


New-ClassLib -ProjectName "$solutionName.Domain.Srv.Services.Integrations" -ProjectFolder "$srvFolder" -NullableDisable $true
$svcIntegrationPrjFilePath = "$srvFolder/$solutionName.Domain.Srv.Services.Integrations/$solutionName.Domain.Srv.Services.Integrations.csproj"
dotnet add $svcIntegrationPrjFilePath reference $svcPrjFilePath

$webAppsFolder = "$srcFolder/web"
mkdir $webAppsFolder


dotnet new webapi -o "$webAppsFolder/$solutionName.WebApi"
$webApiPrjFilePath = "$webAppsFolder/$solutionName.WebApi/$solutionName.WebApi.csproj"
Disable-Nullable($webApiPrjFilePath)


dotnet sln "$srcFolder/$SolutionName.sln" add $webApiPrjFilePath
dotnet add $webApiPrjFilePath reference $svcIntegrationPrjFilePath

dotnet build "$srcFolder/$solutionName.sln"
# dotnet watch run --project "src\web\CostTracker.WebApi\CostTracker.WebApi.csproj"