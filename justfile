version := `xmllint --xpath "string(//Version)" BetterLoadouts/BetterLoadouts.csproj`

build:
    dotnet build BetterLoadouts.slnx

release:
    dotnet build BetterLoadouts.slnx -c Release
    rm -rf release
    mkdir -p release/BepInEx/plugins
    cp BetterLoadouts/bin/Release/BetterLoadouts.dll release/BepInEx/plugins/
    cd release && zip -r ../BetterLoadouts-{{version}}.zip BepInEx/
    rm -rf release
    @echo "Created BetterLoadouts-{{version}}.zip"

deploy target="~/Games/SPTarkov2":
    dotnet build BetterLoadouts.slnx -c Release
    cp BetterLoadouts/bin/Release/BetterLoadouts.dll {{target}}/BepInEx/plugins/
