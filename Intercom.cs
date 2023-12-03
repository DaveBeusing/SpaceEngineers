/*
 * Intercom All-in-one IGC handling
 * (c) 2021 
 * Version 1.1.4
 *
 * General SE Mod-SDK Documentation -> https://github.com/malware-dev/MDK-SE/wiki/Api-Index
 *
 *
 */

Intercom _Intercom;

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
    //Runtime.UpdateFrequency = UpdateFrequency.Once;
    // This makes the program automatically run every 10 ticks (about 6 times per second)
    // https://github.com/malware-dev/MDK-SE/wiki/Continuous-Running-No-Timers-Needed
    // Initialize Intercom
    _Intercom = new Intercom( this );
}

public void Save(){
    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field
    // or some other means.
    //
    // This method is optional and can be removed if not
    // needed.
}

/* Test */
StringBuilder _Status = new StringBuilder();
string _ScriptName = "Intercom";
string _ScriptVersion = "1.1.4";

public void Main( string argument, UpdateType updateSource ){
    // The main entry point of the script, invoked every time
    // one of the programmable block's Run actions are invoked,
    // or the script updates itself. The updateSource argument
    // describes where the update came from.
    //
    // The method itself is required, but the arguments above
    // can be removed if not needed.

    // Echo some information about 'me' and why we were run
    //Echo("Source=" + updateSource.ToString());
    //Echo("Me=" + Me.EntityId.ToString("X"));
    //Echo(Me.CubeGrid.CustomName);


    if( !_Intercom.Initialized ){
      _Intercom.AddBroadcastListener( _BroadCastTag, BroadcastCallback );
      _Intercom.Initialized = true;
    }

    _Intercom.Receive();

    if( _Intercom.Triggers > 0 && argument != "" ){
      _Intercom.Send( argument, _BroadCastTag );
    }

    StatusAnimation( _Status, _ScriptName + " " + _ScriptVersion, StatusAnimationFrame++ );
    _Status.AppendLine( _ScriptName + " Sent->" + _Intercom.HistorySent.Count + " Received->" + _Intercom.HistoryReceived.Count );

    IMyTextSurface _me = Me.GetSurface(0);
    _me.ContentType = ContentType.TEXT_AND_IMAGE;
    _me.Script = "";
    _me.WriteText( _Status );
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



/* Intercom */

string _BroadCastTag = "Intercom Broadcast Test";

void BroadcastCallback( MyIGCMessage msg ){
  if( msg.Data is string ){
    //Echo("Received Test Message");
    //Echo(" Source=" + msg.Source.ToString("X"));
    //Echo(" Data=\"" + msg.Data + "\"");
    //Echo(" Tag=" + msg.Tag);

    string _Message = "Received Test Message" + "\n Source=" + msg.Source.ToString("X") + "\n Data=\"" + msg.Data + "\"" + "\n Tag=" + msg.Tag;

  }
}//BroadcastCallback


public class Intercom {

  MyGridProgram _MyGridProgram;
  IMyUnicastListener _UnicastListener;
  List<Action<MyIGCMessage>> _UnicastCallbacks = new List<Action<MyIGCMessage>>();
  List<Action<MyIGCMessage>> _BroadcastCallbacks = new List<Action<MyIGCMessage>>();
  List<IMyBroadcastListener> _BroadcastChannels = new List<IMyBroadcastListener>();


  public struct IntercomMessage {
    public string Channel;
    public string Source;
    public string Text;
  }

  public List<IntercomMessage> HistorySent = new List<IntercomMessage>();
  public List<IntercomMessage> HistoryReceived = new List<IntercomMessage>();


  private bool isDebug = false;
  public bool Initialized = false;

  public UpdateType Triggers = UpdateType.Terminal | UpdateType.Trigger | UpdateType.Mod | UpdateType.Script;


  public Intercom( MyGridProgram program, bool debug = false ){
    _MyGridProgram = program;
    isDebug = debug;
  }

  public bool AddBroadcastListener( string Tag, Action<MyIGCMessage> Callback ){
    IMyBroadcastListener _BroadcastListener;
    _BroadcastListener = _MyGridProgram.IGC.RegisterBroadcastListener( Tag );
    _BroadcastListener.SetMessageCallback( Tag );
    _BroadcastCallbacks.Add( Callback );
    _BroadcastChannels.Add( _BroadcastListener );
    return true;
  }

  public bool AddUnicastListener( Action<MyIGCMessage> Callback ){
    _UnicastListener = _MyGridProgram.IGC.UnicastListener;
    _UnicastListener.SetMessageCallback( "UNICAST" );
    _UnicastCallbacks.Add( Callback );
    return true;
  }

  public void Send( string Message, string Tag ){
    _MyGridProgram.IGC.SendBroadcastMessage( Tag, Message );
    HistorySent.Add( new IntercomMessage(){ Channel=Tag, Source=_MyGridProgram.Me.EntityId.ToString("X"), Text=Message } );
    if( isDebug ) _MyGridProgram.Echo("Intercom.Send \n Message: " + Message);
  }

  public void Receive(){
    if( isDebug ){
      _MyGridProgram.Echo( _BroadcastChannels.Count.ToString()  + " broadcast channels");
      _MyGridProgram.Echo( _BroadcastCallbacks.Count.ToString() + " broadcast callbacks");
      _MyGridProgram.Echo( _UnicastCallbacks.Count.ToString()   + " unicast callbacks");
    }
    bool PendingMessages = false;
    //_BroadcastChannels
    do {
      PendingMessages = false;
      foreach( var Channel in _BroadcastChannels ){
        if( Channel.HasPendingMessage ){
          PendingMessages = true;
          MyIGCMessage Message = Channel.AcceptMessage();
          if( isDebug ) _MyGridProgram.Echo("Intercom.Receive \n Message: " +Message.Tag+" SRC:"+Message.Source.ToString("X")+"\n" );

          HistoryReceived.Add( new IntercomMessage(){ Channel=Message.Tag, Source=Message.Source.ToString("X"), Text=Message.Data.ToString() } );

          foreach( var Callback in _BroadcastCallbacks ){
            Callback( Message );
          }
        }
      }
    } while( PendingMessages );//Process pending messages

    //TODO implement Unicast handling

  }//Intercom.Receive

}//Class Intercom
