/*
 * <NAME>
 * (c) 2021 
 * Version 1.3.0
 *
 * General SE Mod-SDK Documentation -> https://github.com/malware-dev/MDK-SE/wiki/Api-Index
 *
 */


List<IMyRemoteControl> _RemoteControl = new List<IMyRemoteControl>();
List<IMyGyro> _Gyros = new List<IMyGyro>();
List<IMyLandingGear> _LandingGear = new List<IMyLandingGear>();
List<IMyBatteryBlock> _Batteries = new List<IMyBatteryBlock>();
List<IMyThrust> _Thrusters = new List<IMyThrust>();
List<IMyThrust> forwardThrust = new List<IMyThrust>();
List<IMyThrust> otherThrust = new List<IMyThrust>();


/* Script related vars */
StringBuilder _Status = new StringBuilder();
string _ScriptName = "Stratos Booster";
string _ScriptVersion = "1.0";
string _ScriptTag = "[USC_Stratos]";


/* */
IMyRemoteControl rc = null;
bool isSetup = false;
bool isLaunched = false;
double _LaunchAltitude = 0;



public Program(){
    // The constructor, called only once every session and
    // always before any other method is called. Use it to
    // initialize your script.
    //
    // The constructor is optional and can be removed if not
    // needed.
    //
    // It's recommended to set RuntimeInfo.UpdateFrequency
    // here, which will allow your script to run itself without a
    // timer block.
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    // This makes the program automatically run every 10 ticks (about 6 times per second)
    // https://github.com/malware-dev/MDK-SE/wiki/Continuous-Running-No-Timers-Needed
}

public void Save(){
    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field
    // or some other means.
    //
    // This method is optional and can be removed if not
    // needed.
}

public void Main( string argument, UpdateType updateSource ){
    // The main entry point of the script, invoked every time
    // one of the programmable block's Run actions are invoked,
    // or the script updates itself. The updateSource argument
    // describes where the update came from.
    //
    // The method itself is required, but the arguments above
    // can be removed if not needed.
    StatusAnimation( _Status, _ScriptName + " " + _ScriptVersion, StatusAnimationFrame++ );
    _Status.AppendLine( "altitude: " +  _LaunchAltitude );


    if(!isSetup){
      Setup();
    }


    if( argument.Length != 0 ){
      RunCommand( argument );
    }


    /* Display _Status on Local programmable block */
    IMyTextSurface _me = Me.GetSurface(0);
    _me.ContentType = ContentType.TEXT_AND_IMAGE;
    _me.Script = "";
    _me.WriteText( _Status );
    _Status.Clear();
}//Main


public void RunCommand( string argument ){

  string command = argument.ToLower();

  if( command == "launch" ){

    Launch();

  }



}//RunCommand


public void Setup(){

  isSetup = true;

  GridTerminalSystem.GetBlocksOfType( _RemoteControl, block => block.CustomName.Contains( _ScriptTag ) );
  GridTerminalSystem.GetBlocksOfType( _Gyros, block => block.CustomName.Contains( _ScriptTag ) );
  GridTerminalSystem.GetBlocksOfType( _LandingGear, block => block.CustomName.Contains( _ScriptTag ) );
  GridTerminalSystem.GetBlocksOfType( _Batteries, block => block.CustomName.Contains( _ScriptTag ) );
  GridTerminalSystem.GetBlocksOfType( _Thrusters, block => block.IsSameConstructAs(Me) );

  if( _RemoteControl.Count == 0 ){
    Echo("Error: No Remote Controllers found");
    isSetup = false;
  }
  else {
    rc = _RemoteControl[0];
  }

  if( _Gyros.Count == 0 ){
    Echo("Error: No Gyroscopes found");
    isSetup = false;
  }

  if( _LandingGear.Count == 0 ){
    Echo("Optional: No LandingGear found");
  }

  if( _Batteries.Count == 0 ){
    Echo("Optional: No batteries found");
  }

  if( rc != null ){
    GridTerminalSystem.GetBlocksOfType(forwardThrust, block => block.WorldMatrix.Forward == rc.WorldMatrix.Backward);
    GridTerminalSystem.GetBlocksOfType(otherThrust, block => block.WorldMatrix.Forward != rc.WorldMatrix.Backward);
    if( forwardThrust.Count == 0 ){
      Echo("Error: No forward thrust was found");
      isSetup = false;
    }
  }

}//Setup


public void Launch(){

  foreach( var Battery in _Batteries ){
    Battery.Enabled = true;
    Battery.ChargeMode = ChargeMode.Discharge;
  }

  foreach( var Gyro in _Gyros ){
    Gyro.Enabled = true;
  }

  rc.TryGetPlanetElevation( MyPlanetElevation.Sealevel, out _LaunchAltitude );
  Echo($"LaunchAltitude '{_LaunchAltitude}'");

  foreach( var _Thrust in forwardThrust ){
    _Thrust.Enabled = true;
    _Thrust.ThrustOverridePercentage = 1f;
  }

  foreach( var _Thrust in otherThrust ){
    _Thrust.Enabled = true;
  }

  foreach( var LandingGear in _LandingGear ){
    //LandingGear.Enabled = false;
    LandingGear.Unlock();
  }

}//Launch



int StatusAnimationFrame = 0;
StringBuilder StatusAnimation( StringBuilder sb, string msg, int csaf ){
  string[] StatusAnimation = new[] {"|---", "-|--", "--|-", "---|", "--|-", "-|--"};
  //string[] StatusAnimation = new[] { "", " ===||===", " ==|==|==", " =|====|=", " |======|", "  ======" };//
  csaf = ( csaf + 1 ) % StatusAnimation.Length;
  sb.Append( msg + " " ).AppendLine( StatusAnimation[csaf] );
  return sb;
}
