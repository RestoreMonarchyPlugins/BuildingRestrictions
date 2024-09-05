# Building Restrictions
Set the limits for the number of buildings the player can build.

## Commands
- `/buildingstats` - Shows player the number of buildings they have built and the limit.
- `/buildingstats <player>` - Shows player the number of buildings the specified player has built and the limit. 

## Permissions
```xml
<Permission Cooldown="0">buildingstats</Permission>
<Permission Cooldown="0">buildingstats.other</Permission>
```

## Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<BuildingRestrictionsConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <MessageColor>yellow</MessageColor>
  <EnableMaxBuildings>false</EnableMaxBuildings>
  <MaxBuildings>200</MaxBuildings>
  <EnableMaxBarricades>false</EnableMaxBarricades>
  <MaxBarricades>100</MaxBarricades>
  <EnableMaxStructures>false</EnableMaxStructures>
  <MaxStructures>150</MaxStructures>
  <BypassAdmin>false</BypassAdmin>
  <Barricades>
    <Barricade Name="sentries" Build="SENTRY" Max="5" />
    <Barricade Name="stereos" Build="STEREO" Max="1" />
    <Barricade Name="campfires" Build="CAMPFIRE" Max="2" />
  </Barricades>
  <Structures>
    <Structure Name="roofs" Construct="ROOF" Max="30" />
    <Structure Name="floors" Construct="FLOOR" Max="30" />
  </Structures>
  <Multipliers>
    <Multiplier Permission="buildings.vip" Multiplier="1.5" />
  </Multipliers>
</BuildingRestrictionsConfiguration>
```

## Translations
```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="BuildingsRestriction" Value="You can't build {0} because you have reached the limit of max {1} buildings." />
  <Translation Id="BarricadesRestriction" Value="You can't build {0} because you have reached the limit of max {1} barricades." />
  <Translation Id="StructuresRestriction" Value="You can't build {0} because you have reached the limit of max {1} structures." />
  <Translation Id="SpecificRestriction" Value="You can't build {0} because you have reached the limit of max {1} {2}." />
  <Translation Id="SpecificRestrictionInfo" Value="You have placed {0}/{1} {2}." />
  <Translation Id="PlayerBuildingStatsYou" Value="You have placed {0}{1} barricades and {2}{3} structures, so in total {4}{5} buildings." />
  <Translation Id="PlayerBuildingStats" Value="{0} have placed {1}{2} barricades and {3}{4} structures, so in total {5}{6} buildings." />
  <Translation Id="BuildingStats" Value="{0} players have built {1} barricades and {2} structures, so in total {3} buildings." />
  <Translation Id="PlayerNotFound" Value="Player {0} not found." />
  <Translation Id="BuildingStatsOtherNoPermission" Value="You don't have permission to check other player building stats." />
</Translations>
```