// https://github.com/malware-dev/MDK-SE/wiki/Api-Index

readonly StringBuilder PBLCDString = new StringBuilder();
readonly StringBuilder LCDString = new StringBuilder();
readonly string[] StatusAnimation = new[] {"|---", "-|--", "--|-", "---|", "--|-", "-|--"};
int CurrentStatusAnimationFrame;
DateTime lastStatusUpdate;




/*
https://spaceengineers.fandom.com/wiki/Laser_Antenna
A Laser Antenna requires a direct line of sight to the Laser Antenna that it's linked to. If a player, ship, station, or planet is in the way of the signal, the link is broken. The Laser Antenna will attempt to reconnect until the line of sight is restored.

The power consumption of a Laser Antenna depends on the distance to the Laser Antenna that it is linked to. At a distance of less than 200km, it will consume 10*distance_in_meters Watts. At a distance of greater than 200 km, it will consume 0.000025 * distance_in_meters ^ 2 + 1000000 Watts.

Power consumption is linear up to 200km, then it turns into a quadratic. You need 25MW for a 1000km connection, and 600MW for a 6000km distance.


*/

struct ComRelay {

  public string Name;
  public string Status;
  public string Target;
  public string PowerDraw;

}

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
    PBLCDString.Append("ComRelay ").AppendLine(StatusAnimation[CurrentStatusAnimationFrame]);

    var StationID = Me.CubeGrid.CustomName;
    PBLCDString.AppendLine( "Station ID: " + StationID.ToString() );



    var AntennaTag = "Laser";
    var Antennas = new List<IMyLaserAntenna>();
    GridTerminalSystem.GetBlocksOfType(Antennas, block => block.CustomName.Contains(AntennaTag));
    if( Antennas.Count == 0 ){
      Echo($"Error: No Antenna named '{AntennaTag}' was found");
      return;
    }
    PBLCDString.AppendLine( "Tracking " + Antennas.Count.ToString() + " ComRelays");

    foreach( IMyLaserAntenna _Antenna in Antennas ){
      //PBLCDString.Append(_Antenna.CustomName.ToString()).AppendLine(" ").AppendLine(_Antenna.Status.ToString()).AppendLine(" -> ").AppendLine(_Antenna.DetailedInfo.ToString());

      /*
      Type: Laser Antenna
      Current Input: 445.16 kW
      Connected to <Target Name>
      */
      var info = _Antenna.DetailedInfo.Split('\n');
      if (info.Length != 3) return;
      ComRelay relay;
      relay.Name = _Antenna.CustomName.ToString();
      relay.Status = "Unknown";
      relay.Target = "None";
      if(info[2].StartsWith("Connected to ")){
          relay.Status = info[2].Split(' ')[0];
          relay.Target = info[2].Substring(12);
      }
      relay.PowerDraw = info[1].Split(':')[1];

      //PBLCDString.Append(relay.Name).Append( " " ).Append(relay.Status).Append( " " ).Append(relay.Target).Append( " " ).Append(relay.PowerDraw).AppendLine(" ");
      //PBLCDString.Append( "ComRelay: " ).Append(relay.Name).AppendLine( "Status: " ).Append(relay.Status).AppendLine( "Target: " ).Append(relay.Target).AppendLine( "Power draw: " ).Append(relay.PowerDraw);
      LCDString.AppendLine( "ComRelay: " + relay.Name );
      LCDString.AppendLine( "Status: " + relay.Status );
      LCDString.AppendLine( "Target: " + relay.Target );
      LCDString.AppendLine( "Power draw: " + relay.PowerDraw );
      LCDString.AppendLine();

      /*
      ComRelay -|--
      Station ID Earth Orbit Relay Station
      Relays -> 3
      ComRelay: Laser Antenna LeftStatus:
      UnknownTarget:
      NonePower draw:
       100 WComRelay: Laser Antenna CenterStatus:
      ConnectedTarget:
       Earth Mountain Relay Base Laser AntennaPower draw:
       445.16 kWComRelay: Laser Antenna RightStatus:
      UnknownTarget:
      NonePower draw:
       100 W
      */

    }




    var now = DateTime.Now;
    if( (now - lastStatusUpdate).TotalSeconds >= 1 ){
      WriteToPB( PBLCDString.ToString() );
      WriteToLCD( "ComRelayLCD", LCDString.ToString());
      CurrentStatusAnimationFrame = (CurrentStatusAnimationFrame + 1) % StatusAnimation.Length;
      lastStatusUpdate = now;
    }
    PBLCDString.Clear();
    LCDString.Clear();
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

public void WriteToLCD(string LCDTag, string LCDMessage){
  //Output Info to bridge LCD
  //Output Info to bridge LCD
  IMyTextSurface LCD = GridTerminalSystem.GetBlockWithName( LCDTag ) as IMyTextSurface;
  LCD.ContentType = ContentType.TEXT_AND_IMAGE;
  LCD.Script = "";
  LCD.WriteText( LCDMessage );
}
