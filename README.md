# Building Restrictions

A plugin that allows server admins to set limits on the number of buildings players can place in Unturned.

## Features

- Limit total buildings per player
- Limit barricades and structures separately
- Set specific limits for different building types
- Height restrictions for buildings
- Location-based building limits
- Multipliers for players with special permissions

## Commands

- `/buildingstats` - Shows your current building counts and limits
- `/buildingstats <player>` - Shows another player's building counts and limits (requires permission)

## Permissions

- `buildingstats` - Allows using the `/buildingstats` command
- `buildingstats.other` - Allows checking other players' building stats
- `buildings.vip` - Example permission for multiplier (customizable)

## Configuration

### General Settings

```xml
<MessageColor>yellow</MessageColor>
<MessageIconUrl>https://i.imgur.com/LlEcfBg.png</MessageIconUrl>
<EnableMaxBuildings>false</EnableMaxBuildings>
<MaxBuildings>200</MaxBuildings>
<EnableMaxBarricades>false</EnableMaxBarricades>
<MaxBarricades>100</MaxBarricades>
<EnableMaxStructures>false</EnableMaxStructures>
<MaxStructures>150</MaxStructures>
<BypassAdmin>false</BypassAdmin>
```

- `MessageColor` - Color of plugin messages
- `MessageIconUrl` - Icon shown with messages
- `EnableMaxBuildings` - Set to `true` to limit total buildings
- `MaxBuildings` - Maximum total buildings allowed per player
- `EnableMaxBarricades` - Set to `true` to limit barricades separately
- `MaxBarricades` - Maximum barricades allowed per player
- `EnableMaxStructures` - Set to `true` to limit structures separately
- `MaxStructures` - Maximum structures allowed per player
- `BypassAdmin` - Set to `true` to let admins ignore all restrictions

### Height Restrictions

```xml
<EnableMaxBarricadeHeight>false</EnableMaxBarricadeHeight>
<MaxBarricadeHeight>100</MaxBarricadeHeight>
<EnableMaxStructureHeight>false</EnableMaxStructureHeight>
<MaxStructureHeight>100</MaxStructureHeight>
```

- `EnableMaxBarricadeHeight` - Set to `true` to limit barricade height
- `MaxBarricadeHeight` - Maximum height (in meters) for barricades
- `EnableMaxStructureHeight` - Set to `true` to limit structure height
- `MaxStructureHeight` - Maximum height (in meters) for structures

### Location Restrictions

```xml
<EnableMaxBuildingsPerLocation>false</EnableMaxBuildingsPerLocation>
<MaxBuildingsPerLocationHeight>100</MaxBuildingsPerLocationHeight>
<MaxBuildingsPerLocation>10</MaxBuildingsPerLocation>
```

- `EnableMaxBuildingsPerLocation` - Set to `true` to limit buildings in specific areas
- `MaxBuildingsPerLocationHeight` - Maximum height for location detection
- `MaxBuildingsPerLocation` - Maximum buildings allowed in a location
  - Set to `0` to completely disable building in locations
  - The following aren't counted: Safezone Radiators, Horde Beacons, Charges, Vehicles

### Specific Building Limits

```xml
<Barricades>
  <Barricade Name="sentries" Build="SENTRY" Max="5" />
  <Barricade Name="stereos" Build="STEREO" Max="1" />
  <Barricade Name="campfires" Build="CAMPFIRE" Max="2" />
  <Barricade Name="Sandbag" ItemId="365" Max="10" />
</Barricades>
<Structures>
  <Structure Name="roofs" Construct="ROOF" Max="30" />
  <Structure Name="floors" Construct="FLOOR" Max="30" />
  <Structure Name="Metal Wall" ItemId="371" Max="20" />
</Structures>
```

- `Barricades` - List of specific barricade type restrictions
- `Structures` - List of specific structure type restrictions
- For each entry:
  - `Name` - Display name for the building type (informational only, doesn't affect functionality)
  - `Build` - Barricade type (use values from Barricade Build types list)
  - `Construct` - Structure type (use values from Structure Construct types list)
  - `ItemId` - Specific item ID (overrides Build/Construct)
  - `Max` - Maximum number allowed

#### Restriction Methods:
You can restrict buildings in three ways:

1. **By Building Category**: Use the `Build` attribute for barricades or `Construct` for structures to restrict entire categories (e.g., all SENTRY types).

2. **By Specific Item ID**: Use the `ItemId` attribute to restrict a specific item. For example:
   - `ItemId="365"` restricts Sandbag barricades specifically
   - `ItemId="371"` restricts Metal Walls specifically
   
3. **Combined Restrictions**: You can have both category restrictions and specific item restrictions active at the same time.

> **Note**: When using ItemId, you need to know the exact ID number of the item in Unturned. Using ItemId will override category-based restrictions for that specific item. The `Name` attribute is just for your reference and doesn't need to match the exact item name in Unturned.

### Permission Multipliers

```xml
<Multipliers>
  <Multiplier Permission="buildings.vip" Multiplier="1.5" />
</Multipliers>
```

- Use multipliers to give certain players higher limits
- `Permission` - The permission to check for
- `Multiplier` - How much to multiply the limits (1.5 = +50%)

## Building Types Reference

### Barricade Types
```
FORTIFICATION, BARRICADE, DOOR, GATE, BED, LADDER, STORAGE, FARM, 
TORCH, CAMPFIRE, SPIKE, WIRE, GENERATOR, SPOT, SAFEZONE, FREEFORM, 
SIGN, VEHICLE, CLAIM, BEACON, STORAGE_WALL, BARREL_RAIN, OIL, CAGE, 
SHUTTER, TANK, CHARGE, SENTRY, SENTRY_FREEFORM, OVEN, LIBRARY, 
OXYGENATOR, GLASS, NOTE, HATCH, MANNEQUIN, STEREO, SIGN_WALL, 
CLOCK, BARRICADE_WALL
```

### Structure Types
```
FLOOR, WALL, RAMPART, ROOF, PILLAR, POST, FLOOR_POLY, ROOF_POLY
```