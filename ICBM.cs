/*
 * Inter Continental Balistic Missile Guidance Script
 * (c) 2021 
 * Version 1.2.7
 *
 * General SE Mod-SDK Documentation -> https://github.com/malware-dev/MDK-SE/wiki/Api-Index
 *
 *
 */
 //GPS:Missile Test Site Air:-127.51:48938.82:44980.92:#FF75C9F1:
 //GPS:Missile Test Site Ground:-265.16:48475.76:43718.48:#FF75C9F1:


string ICBM_TAG = "[USC_ICBM]";
List<IMyRemoteControl> _RemoteControl = new List<IMyRemoteControl>();
List<IMyGyro> _Gyros = new List<IMyGyro>();
List<IMyLandingGear> _LandingGear = new List<IMyLandingGear>();
List<IMyShipMergeBlock> _Merges = new List<IMyShipMergeBlock>();
List<IMyBatteryBlock> _Batteries = new List<IMyBatteryBlock>();
List<IMyReactor> _Reactors = new List<IMyReactor>();
List<IMyThrust> _Thrusters = new List<IMyThrust>();
List<IMyThrust> forwardThrust = new List<IMyThrust>();
List<IMyThrust> otherThrust = new List<IMyThrust>();
IMyRemoteControl rc = null;
bool isLaunched = false;
double _LaunchAltitude = 0;
bool isAuto = false;

bool isDebug = true;

int Counter = 0;

//Vector3D planetCenter = new Vector3D(0, 0, 0);
Vector3D TargetPosition = new Vector3D(0, 0, 0);


List<string> _History = new List<string>();

readonly StringBuilder _Status = new StringBuilder();
readonly string[] StatusAnimation = new[] {"|---", "-|--", "--|-", "---|", "--|-", "-|--"};
int CurrentStatusAnimationFrame;
DateTime lastStatusUpdate;


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

public void Main(string argument, UpdateType updateSource){
  //Status Animation
  _Status.Append( "USC ICBM " ).AppendLine( StatusAnimation[CurrentStatusAnimationFrame] );
  //Control Sequence
  if( isLaunched ){

    if( !isAuto ){
        MissileControl();
    }


    if( TargetPosition.Equals( rc.GetPosition() ) ){
      foreach( var Battery in _Batteries ){
        Battery.Enabled = false;
      }
      foreach( var _Thrust in forwardThrust ){
        _Thrust.Enabled = false;
      }

      foreach( var _Thrust in otherThrust ){
        _Thrust.Enabled = false;
      }

    }


  }
  if( !isLaunched ){
    if( argument != "" ){
      //Vector3D TargetPosition = new Vector3D(0, 0, 0);
      bool isGPS = TryParseGPS( argument, out TargetPosition );
      if(isDebug) Echo($"Target vector isGPS:{isGPS} '{TargetPosition}'");
      if( isGPS ){
        //Pre-launch Setup
        bool isSetup = Setup();
        if( isSetup ){
          //Increase UpdateFrequency
          Runtime.UpdateFrequency = UpdateFrequency.Update1;
          //Start Launch sequence
          Launch( TargetPosition );
          isLaunched = true;
        }//isSetup
      }//isGPS
    }//argument != ""
    else {
      _Status.AppendLine( "Waiting for Target" );
    }
  }//!isLaunched

  var now = DateTime.Now;
  if( ( now - lastStatusUpdate ).TotalSeconds >= 1 ){
    Display( _Status.ToString() );
    CurrentStatusAnimationFrame = ( CurrentStatusAnimationFrame + 1 ) % StatusAnimation.Length;
    lastStatusUpdate = now;
  }
  _Status.Clear();
}//Main


public void Display(string Text){
  IMyTextSurface _Display = Me.GetSurface(0);
  _Display.ContentType = ContentType.TEXT_AND_IMAGE;
  _Display.Script = "";
  _Display.WriteText( Text );
}//Display

bool Setup(){

  bool setup = true;

  GridTerminalSystem.GetBlocksOfType( _RemoteControl, block => block.CustomName.Contains( ICBM_TAG ) );
  GridTerminalSystem.GetBlocksOfType( _Gyros, block => block.CustomName.Contains( ICBM_TAG ) );
  GridTerminalSystem.GetBlocksOfType( _LandingGear, block => block.CustomName.Contains( ICBM_TAG ) );
  GridTerminalSystem.GetBlocksOfType( _Merges, block => block.CustomName.Contains( ICBM_TAG ) );
  GridTerminalSystem.GetBlocksOfType( _Batteries, block => block.CustomName.Contains( ICBM_TAG ) );
  GridTerminalSystem.GetBlocksOfType( _Reactors, block => block.CustomName.Contains( ICBM_TAG ) );
  GridTerminalSystem.GetBlocksOfType( _Thrusters );

  if( _RemoteControl.Count == 0 ){
    Echo("Error: No Remote Controllers found");
    setup = false;
  }
  else {
    rc = _RemoteControl[0];
  }

  if( _Gyros.Count == 0 ){
    Echo("Error: No Gyroscopes found");
    setup = false;
  }

  if( _LandingGear.Count == 0 ){
    Echo("Optional: No LandingGear found");
  }

  if( _Merges.Count == 0 ){
    Echo("Optional: No merges found");
  }

  if( _Batteries.Count == 0 ){
    Echo("Optional: No batteries found");
  }

  if( _Reactors.Count == 0 ){
    Echo("Optional: No reactors found");
  }


  if( rc != null ){


    //GridTerminalSystem.GetBlocksOfType(forwardThrust, block => block.WorldMatrix.Forward == rc.WorldMatrix.Backward);
    //GridTerminalSystem.GetBlocksOfType(otherThrust, block => block.WorldMatrix.Forward != rc.WorldMatrix.Backward);

    GridTerminalSystem.GetBlocksOfType(forwardThrust, block => block.WorldMatrix.Forward == rc.WorldMatrix.Backward);//rc.WorldMatrix.Backward //rc.WorldMatrix.Up
    GridTerminalSystem.GetBlocksOfType(otherThrust, block => block.WorldMatrix.Forward != rc.WorldMatrix.Backward);
    if( forwardThrust.Count == 0 ){
      Echo("Error: No forward thrust was found");
      setup = false;
    }
  }

  return setup;

}//Setup


bool Launch(Vector3D Target){

  foreach( var Battery in _Batteries ){
    Battery.Enabled = true;
    Battery.ChargeMode = ChargeMode.Discharge;
  }

  foreach( var Reactor in _Reactors ){
    Reactor.Enabled = true;
  }

  foreach( var Gyro in _Gyros ){
    Gyro.Enabled = true;
  }

  rc.FlightMode = FlightMode.OneWay;//OneWay
  rc.Direction = Base6Directions.Direction.Forward;//Up
  rc.ClearWaypoints();
  rc.AddWaypoint(Target, "Target");
  //rc.SetAutoPilotEnabled(true);
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

  foreach( var Merge in _Merges ){
    Merge.Enabled = false;
  }


  return true;

}//Launch


void MissileControl(){
  Counter++;
  Echo($"MissileControl ... {Counter}");
  GridTerminalSystem.GetBlocksOfType( _RemoteControl, block => block.CustomName.Contains( ICBM_TAG ) );
  GridTerminalSystem.GetBlocksOfType( _Gyros, block => block.CustomName.Contains( ICBM_TAG ) );
  rc = _RemoteControl[0];

  double _altitude = 0;
  rc.TryGetPlanetElevation( MyPlanetElevation.Sealevel, out _altitude );
  double rcThreshold = _altitude - _LaunchAltitude;
  Echo($"rcThreshold '{rcThreshold}'");

  if( rcThreshold > 60 ){
    Echo($"rcThreshold reached'");
    rc.SetAutoPilotEnabled(true);
    isAuto = true;
  }


}//MissileControl



//borrowed from https://github.com/Whiplash141/SpaceEngineersScripts/blob/master/Public/Cruise%20Missile%20(ICBM)%20Code%20v13.cs
bool TryParseGPS(string gpsString, out Vector3D vector){
    vector = new Vector3D(0, 0, 0);
    var gpsStringSplit = gpsString.Split(':');
    //Echo($"TryParseGPS: gpsStringSplit.Length > {gpsStringSplit.Length} ");
    double x, y, z;
    if (gpsStringSplit.Length != 7)
        return false;
    bool passX = double.TryParse(gpsStringSplit[2], out x);
    bool passY = double.TryParse(gpsStringSplit[3], out y);
    bool passZ = double.TryParse(gpsStringSplit[4], out z);
    //Echo($"{x},{y},{z}");
    if(passX && passY && passZ){
      vector = new Vector3D(x, y, z);
      //Echo($"Target Position vector '{vector}'");
      return true;
    }
    else
      return false;
}
