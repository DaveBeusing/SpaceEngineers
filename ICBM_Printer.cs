/*
 * Inter Continental Balistic Missile Printer Script
 * (c) 2021 
 * Version 1.2.11
 *
 * General SE Mod-SDK Documentation -> https://github.com/malware-dev/MDK-SE/wiki/Api-Index
 *
 *
 */

readonly StringBuilder _Status = new StringBuilder();
readonly string[] StatusAnimation = new[] {"|---", "-|--", "--|-", "---|", "--|-", "-|--"};
int CurrentStatusAnimationFrame;
DateTime lastStatusUpdate;

string SCRIPT_NAME = "ICBM Printer";
string SCRIPT_VERSION = "1.1.14";

List<IMyShipWelder> _Welders = new List<IMyShipWelder>();

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
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
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
    _Status.Append( SCRIPT_NAME + " v" + SCRIPT_VERSION + " " ).AppendLine( StatusAnimation[CurrentStatusAnimationFrame] );
    IMyTextSurface _Display = Me.GetSurface(0);
    _Display.ContentType = ContentType.TEXT_AND_IMAGE;
    _Display.Script = "";

    GridTerminalSystem.GetBlocksOfType( _Welders, block => block.CustomName.Contains( "Printer Welder" ) );
    var Indicator = GridTerminalSystem.GetBlockWithName("Printer IndicatorLight") as IMyReflectorLight;
    var Projector = GridTerminalSystem.GetBlockWithName("Printer Projector") as IMyProjector;
    var Piston = GridTerminalSystem.GetBlockWithName("Printer Welder Piston") as IMyPistonBase;
    Piston.MinLimit = (float)0.5;
    Piston.MaxLimit = (float)5.2;

    if(Projector.IsProjecting){

      int TotalBlocks = Projector.TotalBlocks;
      int RemainingBlocks = Projector.RemainingBlocks;
      float coveragePercent = (float) (RemainingBlocks/100)*1;

      Echo($"RB:{TotalBlocks} TB:{RemainingBlocks} P:{coveragePercent}");

      //Start print
      if( argument == "print" ){
        if( RemainingBlocks == TotalBlocks ){
          Piston.Extend();
          foreach( var Welder in _Welders ){
            Welder.Enabled = true;
          }
          Indicator.Enabled = true;
        }
        else {
            _Status.AppendLine("! CAUTION Printtable not clear ! \n Please remove last print first!");
        }
      }

      if( TotalBlocks == RemainingBlocks ){
          _Status.AppendLine("Printer ready... \n waiting for printjob");
      }

      //Show Progessbar
      if( TotalBlocks > RemainingBlocks ){
        _Status.AppendLine( RemainingBlocks + "/" + TotalBlocks );
        _Status.Append("\nCoverage: ").Append(Math.Round(coveragePercent, 0)).Append("%\n");
        //DrawProgressBar( _Status, _Display.FontSize, coveragePercent ).Append('\n');
      }

      //Print finished
      if( RemainingBlocks == 0 ){
        Piston.Retract();
        foreach( var Welder in _Welders ){
          Welder.Enabled = false;
        }
        Indicator.Enabled = false;
        _Status.AppendLine("Printjob done!");
      }



    }//Projector.IsProjecting
    else {
      _Status.AppendLine("Printer needs to be configured");
    }

     var now = DateTime.Now;
     if( ( now - lastStatusUpdate ).TotalSeconds >= 1 ){
        _Display.WriteText( _Status.ToString() );
       CurrentStatusAnimationFrame = ( CurrentStatusAnimationFrame + 1 ) % StatusAnimation.Length;
       lastStatusUpdate = now;
     }
     _Status.Clear();
}//Main



StringBuilder DrawProgressBar(StringBuilder sb, float fontSize, float percent) {
 int total = (int)((1f / fontSize) * 72) - 4;
 int filled = (int) Math.Round(percent / 100 * total);
 sb.Append('[').Append('I', filled).Append('`', total - filled).Append(']');
 return sb;
}
