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

## List of Barricade Build types
FORTIFICATION, BARRICADE, DOOR, GATE, BED, LADDER, STORAGE, FARM, TORCH, CAMPFIRE, SPIKE, WIRE, GENERATOR, SPOT, SAFEZONE, FREEFORM, SIGN, VEHICLE, CLAIM, BEACON, STORAGE_WALL, BARREL_RAIN, OIL, CAGE, SHUTTER, TANK, CHARGE, SENTRY, SENTRY_FREEFORM, OVEN, LIBRARY, OXYGENATOR, GLASS, NOTE, HATCH, MANNEQUIN, STEREO, SIGN_WALL, CLOCK, BARRICADE_WALL

## List of Structure Construct types
- FLOOR
- WALL
- RAMPART
- ROOF
- PILLAR
- POST
- FLOOR_POLY
- ROOF_POLY

## Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<BuildingRestrictionsConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <MessageColor>yellow</MessageColor>
  <MessageIconUrl>https://i.imgur.com/LlEcfBg.png</MessageIconUrl>
  <EnableMaxBuildings>false</EnableMaxBuildings>
  <MaxBuildings>200</MaxBuildings>
  <EnableMaxBarricades>false</EnableMaxBarricades>
  <MaxBarricades>100</MaxBarricades>
  <EnableMaxStructures>false</EnableMaxStructures>
  <MaxStructures>150</MaxStructures>
  <BypassAdmin>false</BypassAdmin>
  <EnableMaxBarricadeHeight>false</EnableMaxBarricadeHeight>
  <MaxBarricadeHeight>100</MaxBarricadeHeight>
  <EnableMaxStructureHeight>false</EnableMaxStructureHeight>
  <MaxStructureHeight>100</MaxStructureHeight>
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
  <Translation Id="BuildingsRestriction" Value="You can't build [[b]]{0}[[/b]] because you have reached the limit of max [[b]]{1}[[/b]] buildings." />
  <Translation Id="BarricadesRestriction" Value="You can't build [[b]]{0}[[/b]] because you have reached the limit of max [[b]]{1}[[/b]] barricades." />
  <Translation Id="StructuresRestriction" Value="You can't build [[b]]{0}[[/b]] because you have reached the limit of max [[b]]{1}[[/b]] structures." />
  <Translation Id="SpecificRestriction" Value="You can't build [[b]]{0}[[/b]] because you have reached the limit of max [[b]]{1} {2}[[/b]]." />
  <Translation Id="SpecificRestrictionInfo" Value="You have placed [[b]]{0}/{1} {2}[[/b]]." />
  <Translation Id="PlayerBuildingStatsYou" Value="You have placed [[b]]{0}{1}[[/b]] barricades and [[b]]{2}{3}[[/b]] structures, so in total [[b]]{4}{5}[[/b]] buildings." />
  <Translation Id="PlayerBuildingStats" Value="[[b]]{0}[[/b]] have placed [[b]]{1}{2}[[/b]] barricades and [[b]]{3}{4}[[/b]] structures, so in total [[b]]{5}{6}[[/b]] buildings." />
  <Translation Id="BuildingStats" Value="[[b]]{0}[[/b]] players have built [[b]]{1}[[/b]] barricades and [[b]]{2}[[/b]] structures, so in total [[b]]{3}[[/b]] buildings." />
  <Translation Id="PlayerNotFound" Value="Player [[b]]{0}[[/b]] not found." />
  <Translation Id="BuildingStatsOtherNoPermission" Value="You don't have permission to check other player building stats." />
  <Translation Id="MaxBarricadeHeightRestriction" Value="You can't build [[b]]{0}[[/b]] because it's higher than max [[b]]{1}m[[/b]] height above the ground." />
  <Translation Id="MaxStructureHeightRestriction" Value="You can't build [[b]]{0}[[/b]] because it's higher than max [[b]]{1}m[[/b]] height above the ground." />
</Translations>
```