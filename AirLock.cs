// https://github.com/malware-dev/MDK-SE/wiki/Api-Index



readonly StringBuilder PBLCDString = new StringBuilder();
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
    // The main entry point of the script, invoked every time
    // one of the programmable block's Run actions are invoked,
    // or the script updates itself. The updateSource argument
    // describes where the update came from.
    //
    // The method itself is required, but the arguments above
    // can be removed if not needed.
    PBLCDString.Append("AirLock ").AppendLine(StatusAnimation[CurrentStatusAnimationFrame]);

    //Critial Ones because they might be exposed to depressurized area
    checkAirLock("Nostromo Airlock Bridge Inner", "Bridge Inner", "Nostromo Airlock Bridge Outer", "Bridge Outer");
    checkAirLock("Nostromo Airlock CargoExit Inner", "CargoExit Inner", "Nostromo Airlock CargoExit Outer", "CargoExit Outer");
    checkAirLock("Nostromo Airlock Hangar Inner", "Hangar Inner", "Nostromo Airlock Hangar Outer", "Hangar Outer");

    //Internal Ones - not so critical because they might never be exposed to depressurisation
    checkAirLock("Nostromo Airlock Tech Inner", "TechBay Inner", "Nostromo Airlock Tech Outer", "TechBay Outer");
    checkAirLock("Nostromo Airlock Cargo Inner", "CargoBay Inner", "Nostromo Airlock Cargo Outer", "CargoBay Outer");

    var now = DateTime.Now;
    if( (now - lastStatusUpdate).TotalSeconds >= 1 ){
      WriteToPB( PBLCDString.ToString() );
      CurrentStatusAnimationFrame = (CurrentStatusAnimationFrame + 1) % StatusAnimation.Length;
      lastStatusUpdate = now;
    }
    PBLCDString.Clear();
}

// Write Text to the LCD of the currently used programmable block
public void WriteToPB(string LCDText){
  //Output to Display of programmable block
  IMyTextSurface selfSurface = Me.GetSurface(0);
  selfSurface.ContentType = ContentType.TEXT_AND_IMAGE;
  selfSurface.Script = "";
  //selfSurface.FontSize = 2;
  //selfSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
  selfSurface.WriteText( LCDText );
}

public string ReadableStatus(bool status){
  return status ? "Unlocked" : "Locked";
}

public void checkAirLock(string Block1, string Name1, string Block2, string Name2){
  var Door1 = GridTerminalSystem.GetBlockWithName( Block1 ) as IMyDoor;
  var Door2 = GridTerminalSystem.GetBlockWithName( Block2 ) as IMyDoor;
  if( Door1.Status.ToString() == "Open" ){
    Door2.ApplyAction("OnOff_Off");
  }
  if( Door1.Status.ToString() == "Closed" ){
    Door2.ApplyAction("OnOff_On");
  }
  if( Door2.Status.ToString() == "Open" ){
    Door1.ApplyAction("OnOff_Off");
  }
  if( Door2.Status.ToString() == "Closed" ){
    Door1.ApplyAction("OnOff_On");
  }
  PBLCDString.Append( Name1 + " : ").Append( ReadableStatus(Door1.Enabled) + " / " ).AppendLine( Door1.Status.ToString() );
  PBLCDString.Append( Name2 + " : ").Append( ReadableStatus(Door2.Enabled) + " / " ).AppendLine( Door2.Status.ToString() );
}
