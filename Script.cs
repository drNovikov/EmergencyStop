/* 
    v0.50
    
    Space Engineers ship emergency stop script by drNovikov.
    Saves your ships if you accidentally get out of your cockpit or if you get disconnected from a server. Does not rely on sensors.

    How to use: have a looping timer run a programmable block with this script and also restart itself every second. You can also run the script with the word "manual" as an argument if you want to stop your ship by a quick key, button panel, sensor, timer, etc.
    
    Special note: scripts need at least one working cockpit, control station or remote control on a ship in order to enable inertia dampeners. It's a good idea to have a remote control block or another control station hidden away from your main cockpit in case the main one gets destroyed mid-flight.

    What does it do: as soon as the script detects there is no one in control of the ship or any of its turrets, it enables inertia dampeners, turns on gyroscopes and thrusters, cancels their overrides. Also, turns off gravity generators and artificial masses to prevent them from counteracting the inertia dampener. 

    TODO: add a script to turn on a beacon or an antenna.
    TODO: add a script for power saving, so the lost ship stays working and broadcasting longer.
    TODO: add a script for auto-piloting a ship to a predefined GPS location.
    TODO: if possible, change grid name to show GPS, so a player could find it.
    TODO: detect if there are no working thrusters in a particular direction and rotate a ship accordingly to use its remaining working thrusters.

    Thanks to Mustardman24 for the idea of enabling inertia dampeners by sensors and his script that I initially used for my ships in conjunction with sensors, and later wrote my own smaller version with a reliable way of detecting loss of control.
*/

void Main(string argument)
{    
    if ((!ShipIsControlled() && !TurretsAreControlled()) || argument == "manual")
    {
        EnableInertiaDampeners();
        DisableThrusterOverrides();
        DisableGyroscopeOverrides();
        DisableGravityDrives(); // Comment this out if you don't have any gravity drives.
    }
}  
 
// drNovikov's famous pilot detector: checks is a ship is piloted. For example, if a pilot accidentally exits a ship, this could be used to engage inertia dampeners, turn on beacons, etc.

bool ShipIsControlled()
{
    List<IMyTerminalBlock> shipControllers = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyShipController>(shipControllers);
    for (int i = 0; i < shipControllers.Count; i++)
    {
        if ((shipControllers[i] as IMyShipController).IsUnderControl)
        {
            return true;
        }
    }
    return false;
}

bool TurretsAreControlled()
{
    List<IMyTerminalBlock> turrets = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(turrets);
    for (int i = 0; i < turrets.Count; i++)
    {
        if ((turrets[i] as IMyLargeTurretBase).IsUnderControl)
        {
            return true;
        }
    }
    return false;
}
 
// drNovikov's gravity drive disabler for large ships with gravity drives
 
void DisableGravityDrives()
{
    DisableGravityGenerators();
    DisableArtificialMasses();
}
 
void DisableGravityGenerators()
{
    List<IMyTerminalBlock> gravgens = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyGravityGenerator>(gravgens);
    for (int i = 0; i < gravgens.Count; i++)
    {
        gravgens[i].GetActionWithName("OnOff_Off").Apply(gravgens[i]);
    }
}
 
void DisableArtificialMasses()
{
    List<IMyTerminalBlock> artmasses = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyVirtualMass>(artmasses);
    for (int i = 0; i < artmasses.Count; i++)
    {  
        artmasses[i].GetActionWithName("OnOff_Off").Apply(artmasses[i]);
    }  
}
 
// Inertia dampeners enabler, inspired by Mustardman24's script

void EnableInertiaDampeners()
{
    List<IMyTerminalBlock> shipControllers = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyShipController>(shipControllers);
    for (int i = 0; i < shipControllers.Count; i++)
    {
        if ((shipControllers[i] as IMyShipController).DampenersOverride == false)
        {
            shipControllers[i].GetActionWithName("DampenersOverride").Apply(shipControllers[i]);
        }
    }
}
 
void DisableThrusterOverrides()
{
    List<IMyTerminalBlock> thrusters = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
    for (int i = 0; i < thrusters.Count; i++)
    {   
        thrusters[i].GetActionWithName("OnOff_On").Apply(thrusters[i]);
        thrusters[i].SetValueFloat("Override", 0);
    }
}
 
void DisableGyroscopeOverrides()
{
    List<IMyTerminalBlock> gyroscopes = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyroscopes);
    for (int i = 0; i < gyroscopes.Count; i++)
    {
        gyroscopes[i].GetActionWithName("OnOff_On").Apply(gyroscopes[i]);
        gyroscopes[i].SetValueFloat("Power", 1);
        if ((gyroscopes[i] as IMyGyro).GyroOverride)
        {
            gyroscopes[i].GetActionWithName("Override").Apply(gyroscopes[i]);
        }
    }
}