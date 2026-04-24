# GameProgramming2026

**NOTE: Game is programmed in Unity 6000.3.4f1**

## Scripts:

### Enemy AI: Cohesion Behaviour

#### Main Learning Objectives:
- Finding and Using Things
- Encapsulation
- Unity API

#### /FPS/Scripts/AI/EnemyCohesion.cs
Defines enemy motion response to surrounding charges according to Coulomb’s Law.
Will not interfere with their navigation targets. They can still try following patrol paths, etc.

**Essentially: **
- Scan surroundings for charged objects
- Calculate total force vector exerted by these charges
- Move according to force vector


### Charge

#### Main Learning Objectives:
- Abstraction (types and interfaces)
- Encapsulation
- Inheritance
- Polymorphism
- OOP principles and organisation

#### /FPS/Scripts/Gameplay/ChargeBase.cs
Abstract class with virtual method, defining the charge-carrying object.
Primarily stores charge variables:
- Magnitude
- Positive or Negative

Charge is quantized and conserved.
Polymorphism restricted to behaviours that trigger with a change in charge, not how changes are handled themselves.

#### /FPS/Scripts/Gameplay/ChargeMarker.cs
Basic ChargeMarker class.
Inherits default behaviour from ChargeBase (storage place for variables).
Adds light-based effects to visualize charge value.

#### /FPS/Scripts/Gameplay/ChargeMarkerSparker.cs
Demonstration for polymorphism.
Doesn't really do anything special, just gives a different effect for being hit by a charge.
(effect is just a console log - haven't actually implemented visuals)

#### /FPS/Scripts/Gameplay/Chargeable.cs
(aka Ichargeable, but keeping convention of FPS Microgame)
Interface defining the contract for (potential) charge carriers.
Analogous to Damageable interface ('borrowed' a bit of that pre-existing script).
- If an object is charged, it has some kind of ChargeMarker
- If charge is imposed on a chargeable object, tell its ChargeMarker to modify its charge variables.

#### /FPS/Scripts/Gameplay/ProjectileStaticBolt.cs
Pretty janky proof-of-concept implementation for charge-based weaponry.
Inherits from pre-existing ProjectileBase.
Basically a copy of the laser projectile, but adds charge-based effects.
If a chargeable enemy is hit, then InflictCharge() as well as InflictDamage().

### Hold Area Objective and Alarm Events

#### Main Learning Objectives:
- Events
- Delegates
- Inheritance
- Polymorphism

#### FPS/Scripts/Gameplay/Objectives/ObjectiveHoldArea.cs
Defines the Hold Area Objective.
Inherits from pre-existing Objective class.
Code ‘borrowed’ heavily from similar pre-existing ReachObjective class.
Code also ‘borrowed’ from the pre-existing Health Bar functionality to produce a visual progress marker.
Result is a spatially defined objective area with a time elapsed progress bar.
The objective is completed if the player spends enough time inside the area.

Options can set:
- Length of time required to hold
- If the timer is retained or resets when the player exits the area
- If entering the area triggers an alarm

#### /FPS/Scripts/AI/AlarmManager.cs
Caller for Alarm Events.
Object is attached to the scene’s GameManager, alongside other managers.
When something in the scene (e.g. Hold Area Objective) raises an alarm, instruct all enemies within range of the source to respond.

#### /FPS/Scripts/Game/Events.cs
Pre-existing FPS Microgame script.
Modified to include AreaAlarmEvent.

Defines unique structure of alarm events, which otherwise inherit from pre-existing GameEvent
- Origin (Vector3)
- Range (float)

#### /FPS/Scripts/AI/EnemyMobile.cs
Pre-existing FPS Microgame script
Modified to include a response to Alarm triggers: OnAlarmTriggered()
Tells the enemy the location of the player (via DetectionModule) and sets their AI state to Follow.

#### /FPS/Scripts/AI/DetectionModule.cs
Pre-existing FPS Microgame script.
Modified with a method that injects the player’s location into the detection system.
(default is that enemy detection relies on line-of-sight, but we want alarms to have the same effect as ‘seeing’ the player)
Simply sets the player as a KnownDetectedTarget, and the pre-existing enemy AI takes it from there.

