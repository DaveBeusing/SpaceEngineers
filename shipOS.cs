
readonly StringBuilder statusStringBuilder = new StringBuilder();

readonly string[] animFrames = new[] {
    "shipOS |---",
    "shipOS -|--",
    "shipOS --|-",
    "shipOS ---|",
    "shipOS --|-",
    "shipOS -|--"
};

DateTime lastStatusUpdate;
int runningFrame;
bool isBridgeLCDOutput = true;

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
    //This makes the program automatically run every 10 ticks (about 6 times per second)
    SetPBLCDText("shipOS initializing...");
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
    // The main entry point of the script, invoked every time
    // one of the programmable block's Run actions are invoked,
    // or the script updates itself. The updateSource argument
    // describes where the update came from.
    //
    // The method itself is required, but the arguments above
    // can be removed if not needed.


    string HangarStatus = getHangarStatus();
    float CurrentShipMass = getCurrentShipMass();


    var now = DateTime.Now;
    if( (now - lastStatusUpdate).TotalSeconds >= 1 ){
      //shipOS animation
      statusStringBuilder.AppendLine(animFrames[runningFrame]);
      //isBridgeLCDOutput
      statusStringBuilder.Append("Bridge LCD: ").AppendLine( isBridgeLCDOutput.ToString() );

      //statusStringBuilder.Append("Tracking ").Append(lcdStatuses.Count).AppendLine(" display blocks");
      statusStringBuilder.Append("Ship Weight Status: ").Append( CurrentShipMass.ToString() ).AppendLine(" kg");

      statusStringBuilder.Append("Deck Status -> Bridge ").Append( getDeckStatus( "Nostromo Air Vent Bridge" ) ).AppendLine(" Oxygen");
      statusStringBuilder.Append("Deck Status -> Engineering ").Append( getDeckStatus( "Nostromo Air Vent Engineering Deck" ) ).AppendLine(" Oxygen");
      statusStringBuilder.Append("Deck Status -> Cargo ").Append( getDeckStatus( "Nostromo Air Vent Cargo Deck" ) ).AppendLine(" Oxygen");

      statusStringBuilder.Append("Hangar Status: ").AppendLine( HangarStatus );
      SetPBLCDText( statusStringBuilder.ToString() );
      runningFrame = (runningFrame + 1) % animFrames.Length;
      lastStatusUpdate = now;
    }
    statusStringBuilder.Clear();
}

public string getDeckStatus( string Deck ){
  var AirVent = GridTerminalSystem.GetBlockWithName( Deck ) as IMyAirVent;
  if( AirVent == null) {
      Echo("Error! I couldn't find block $Deck");
      return "Error! I couldn't find block $Deck";
  }
  return (100 * AirVent.GetOxygenLevel() ).ToString( "#0.00" ) +"%";
}



// Write Text to the LCD of the currently used programmable block
public void SetPBLCDText(string LCDText){

  //Output to Display of programmable block
  IMyTextSurface selfSurface = Me.GetSurface(0);
  selfSurface.ContentType = ContentType.TEXT_AND_IMAGE;
  selfSurface.Script = "";
  //selfSurface.FontSize = 2;
  //selfSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
  selfSurface.WriteText( LCDText );

  //Output Info to bridge LCD
  IMyTextSurface LCD = GridTerminalSystem.GetBlockWithName( "Nostromo Bridge LCD Panel shipOS" ) as IMyTextSurface;
  LCD.ContentType = ContentType.TEXT_AND_IMAGE;
  LCD.Script = "";
  LCD.WriteText( LCDText );
}


public float getCurrentShipMass(){
  var Controller = GridTerminalSystem.GetBlockWithName("Nostromo Control Seat") as IMyShipController;
  var Masses = Controller.CalculateShipMass();
  //float DryMass = Masses.BaseMass;
  float TotalMass = Masses.TotalMass;
  return TotalMass / 1000000;
}


public string getHangarStatus(){

  //Nostromo HangarTerminal -> am tor
  //Nostromo HangarTerminal Inside -> innen
  //Nostromo Bridge Panel Right Index@3

  var HangarStatus = "Error";

  var HangarDoor = GridTerminalSystem.GetBlockWithName( "Nostromo Hangar Door" ) as IMyAirtightHangarDoor;
  if( HangarDoor == null) {
      Echo("Error! I couldn't find block HangarDoor");
      return "Error! I couldn't find block HangarDoor";
  }

  var RotatingLight = GridTerminalSystem.GetBlockWithName( "Nostromo Hangar Rotating Light" ) as IMyReflectorLight;
  if( RotatingLight == null) {
      Echo("Error! I couldn't find block RotatingLight");
      return "Error! I couldn't find block RotatingLight";
  }

  var HangarAirVent = GridTerminalSystem.GetBlockWithName( "Nostromo Hangar Air Vent" ) as IMyAirVent;
  if( HangarAirVent == null) {
      Echo("Error! I couldn't find block HangarAirVent");
      return "Error! I couldn't find block HangarAirVent";
  }

  var HangarTerminal1 = GridTerminalSystem.GetBlockWithName( "Nostromo HangarTerminal" ) as IMyTextSurfaceProvider;
  IMyTextSurface HangarTerminal1Surface = HangarTerminal1.GetSurface(0);
  HangarTerminal1Surface.ContentType = ContentType.TEXT_AND_IMAGE;
  HangarTerminal1Surface.Script = "";

  var HangarTerminal2 = GridTerminalSystem.GetBlockWithName( "Nostromo HangarTerminal Inside" ) as IMyTextSurfaceProvider;
  IMyTextSurface HangarTerminal2Surface = HangarTerminal2.GetSurface(0);
  HangarTerminal2Surface.ContentType = ContentType.TEXT_AND_IMAGE;
  HangarTerminal2Surface.Script = "";

/*
  var BridgeTerminal = GridTerminalSystem.GetBlockWithName( "Nostromo Bridge Panel Right" ) as IMyTextSurfaceProvider;
  IMyTextSurface BridgeTerminalSurface = BridgeTerminal.GetSurface(3);
  BridgeTerminal.ContentType = ContentType.TEXT_AND_IMAGE;
  BridgeTerminal.Script = "";
*/

  //Opening	The door is in the process of being opened.
  //Open	The door is fully open.
  //Closing	The door is in the process of being closed.
  //Closed	The door is fully closed.
  if( HangarDoor.Status.ToString() == "Closed" ){
    RotatingLight.Enabled = false;
  }
  if( HangarDoor.Status.ToString() == "Closing" ){
    RotatingLight.Enabled = true;
  }
  if( HangarDoor.Status.ToString() == "Opening" ){
    RotatingLight.Enabled = true;
  }
  if( HangarDoor.Status.ToString() == "Open" ){
    RotatingLight.Enabled = true;
  }
  HangarStatus = HangarDoor.Status.ToString() + " / " + (100 * HangarAirVent.GetOxygenLevel() ).ToString( "#0.00" ) +"%";
  HangarTerminal1Surface.WriteText( "Hangar Status \n" + HangarStatus );
  HangarTerminal2Surface.WriteText( "Hangar Status \n" + HangarStatus );
  //BridgeTerminalSurface.WriteText( "Hangar Status \n" + HangarStatus );
  return HangarStatus;
}
