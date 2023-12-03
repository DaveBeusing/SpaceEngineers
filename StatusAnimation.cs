// https://github.com/malware-dev/MDK-SE/wiki/Api-Index

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

    StringBuilder _Status = new StringBuilder();
    _Status.AppendLine( "First line" );
    StatusAnimation( _Status, "StatusAnimation", StatusAnimationFrame++ );
    _Status.AppendLine( "last line");
    //Output to Display of programmable block
    IMyTextSurface selfSurface = Me.GetSurface(0);
    selfSurface.ContentType = ContentType.TEXT_AND_IMAGE;
    selfSurface.Script = "";
    //selfSurface.FontSize = 2;
    //selfSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
    selfSurface.WriteText( _Status );
    _Status.Clear();
}

int StatusAnimationFrame = 0;
StringBuilder StatusAnimation( StringBuilder sb, string msg, int csaf ){
  string[] StatusAnimation = new[] {"|---", "-|--", "--|-", "---|", "--|-", "-|--"};
  //string[] StatusAnimation = new[] { "", " ===||===", " ==|==|==", " =|====|=", " |======|", "  ======" };//
  csaf = ( csaf + 1 ) % StatusAnimation.Length;
  sb.Append( msg + " " ).AppendLine( StatusAnimation[csaf] );
  return sb;
}

StringBuilder DrawProgressBar(StringBuilder sb, float fontSize, float percent) {
 int total = (int)((1f / fontSize) * 72) - 4;
 int filled = (int) Math.Round(percent / 100 * total);
 sb.Append('[').Append('I', filled).Append('`', total - filled).Append(']');
 return sb;
}
