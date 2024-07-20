$solutionName = "CostTracker"
$srcFolder = "./src"
mkdir $srcFolder

$libsFolder = "$srcFolder/libs"
mkdir $libsFolder

$srvFolder = "$srcFolder/srvs"
mkdir $srvFolder


$webAppsFolder = "$srcFolder/web"
mkdir $webAppsFolder

$toolingFolder = "$srcFolder/tooling"
mkdir $toolingFolder

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
$corePrjFilePath = "$libsFolder/$solutionName.Domain.Core/$solutionName.Domain.Core.csproj"

New-ClassLib -ProjectName "$solutionName.ApiClient"       -ProjectFolder "$libsFolder" -NullableDisable $true
$apiClientPrjFilePath = "$libsFolder/$solutionName.ApiClient/$solutionName.ApiClient.csproj"
dotnet add $apiClientPrjFilePath reference $corePrjFilePath
dotnet add $apiClientPrjFilePath reference "ext_libs/Tangsem.WebApiClient.Extensions"

# write readme.md in the project folder
"
# $solutionName.ApiClient
- ApiClient is Generated
" > "$libsFolder/$solutionName.ApiClient/README.md"


# entities shared
New-ClassLib -ProjectName "$solutionName.Domain.Entities" -ProjectFolder "$libsFolder" -NullableDisable $true

"
# $solutionName.ApiClient
- Entities can be manually created or generated.
" > "$libsFolder/$solutionName.Domain.Entities/README.md"


# in "$solutionName.Domain.Entities", add project ref to "$solutionName.Domain.Core"
$entitiesPrjFilePath = "$libsFolder/$solutionName.Domain.Entities/$solutionName.Domain.Entities.csproj"
dotnet add $entitiesPrjFilePath reference $corePrjFilePath
dotnet add $entitiesPrjFilePath package Microsoft.EntityFrameworkCore --version 8.0.4

# entities repo
New-ClassLib -ProjectName "$solutionName.Domain.Repositories" -ProjectFolder "$libsFolder" -NullableDisable $true

$repoPrjFilePath = "$libsFolder/$solutionName.Domain.Repositories/$solutionName.Domain.Repositories.csproj"
dotnet add $repoPrjFilePath package Npgsql --version 8.0.3
dotnet add $repoPrjFilePath reference $entitiesPrjFilePath

# entities postgres
New-ClassLib -ProjectName "$solutionName.Domain.Entities.Postgres" -ProjectFolder "$libsFolder" -NullableDisable $true
$postgresEntitiesPrjFilePath = "$libsFolder/$solutionName.Domain.Entities.Postgres/$solutionName.Domain.Entities.Postgres.csproj"
dotnet add $postgresEntitiesPrjFilePath package Npgsql --version 8.0.3
dotnet add $postgresEntitiesPrjFilePath reference $repoPrjFilePath

# entities sqlite
New-ClassLib -ProjectName "$solutionName.Domain.Entities.Sqlite" -ProjectFolder "$libsFolder" -NullableDisable $true
$sqliteEntitiesPrjFilePath = "$libsFolder/$solutionName.Domain.Entities.Sqlite/$solutionName.Domain.Entities.Sqlite.csproj"
dotnet add $sqliteEntitiesPrjFilePath package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.6
dotnet add $sqliteEntitiesPrjFilePath reference $repoPrjFilePath

New-ClassLib -ProjectName "$solutionName.Domain.Srv.Services" -ProjectFolder "$srvFolder" -NullableDisable $true
$svcPrjFilePath = "$srvFolder/$solutionName.Domain.Srv.Services/$solutionName.Domain.Srv.Services.csproj"
dotnet add $svcPrjFilePath reference $postgresEntitiesPrjFilePath
dotnet add $svcPrjFilePath reference $sqliteEntitiesPrjFilePath

New-ClassLib -ProjectName "$solutionName.Domain.Srv.Services.Integrations" -ProjectFolder "$srvFolder" -NullableDisable $true
$svcIntegrationPrjFilePath = "$srvFolder/$solutionName.Domain.Srv.Services.Integrations/$solutionName.Domain.Srv.Services.Integrations.csproj"
dotnet add $svcIntegrationPrjFilePath reference $svcPrjFilePath
dotnet add $svcIntegrationPrjFilePath reference ext_libs\Tangsem.Tooling\Tangsem.Tooling.csproj


dotnet new webapi -o "$webAppsFolder/$solutionName.WebApi"
$webApiPrjFilePath = "$webAppsFolder/$solutionName.WebApi/$solutionName.WebApi.csproj"
Disable-Nullable($webApiPrjFilePath)

dotnet sln "$srcFolder/$SolutionName.sln" add $webApiPrjFilePath
dotnet add $webApiPrjFilePath reference $svcIntegrationPrjFilePath


# tooling
dotnet new mstest -o "$toolingFolder/$SolutionName.CodeGen.Toolbox"
$codegenToolboxPrjFilePath = "$toolingFolder/$SolutionName.CodeGen.Toolbox/$SolutionName.CodeGen.Toolbox.csproj"
dotnet sln "$srcFolder/$SolutionName.sln" add $codegenToolboxPrjFilePath
Disable-Nullable($codegenToolboxPrjFilePath)
dotnet add $codegenToolboxPrjFilePath reference $corePrjFilePath
dotnet add $codegenToolboxPrjFilePath reference $entitiesPrjFilePath
"
ApiClient code gen: REF to D:\git-temp\image-search\web-api\ImageSearch.Tools.App\CodeGeneratorsRunner.cs
" > "$toolingFolder/$SolutionName.CodeGen.Toolbox/README.md"

dotnet build "$srcFolder/$SolutionName.sln"
# dotnet watch run --project "src\web\CostTracker.WebApi\CostTracker.WebApi.csproj"