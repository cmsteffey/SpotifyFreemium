using System;
using CMSlib.Extensions;
using CMSlib.ConsoleModule;
using System.Linq;
namespace FreemiumV2
{
    class Program
    {
	
	private static string blacklistDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpotifyFreemium";
	private static string blacklistPath = blacklistDir + @"\ads.txt";
        static async System.Threading.Tasks.Task Main(string[] args)
        {	
	    if(!System.Environment.OSVersion.ToString().ToLower().Contains("win")){Console.WriteLine("Program only supports Windows OS."); return;}
	    ITerminal terminal = new WinTerminal();
	    string[] blacklist;
	    
	    if(System.IO.File.Exists(blacklistPath)){
		blacklist = System.IO.File.ReadAllLines(blacklistPath);
	    }else{
		System.IO.Directory.CreateDirectory(blacklistDir);
		using var writer = System.IO.File.CreateText(blacklistPath);
		writer.WriteLine("Advertisement");
		blacklist = new string[]{"Advertisement"};
	    }
	    var sim = new StandardInputModule("Spotify Freemium", 0, 0, Console.WindowWidth, Console.WindowHeight);
	    ModuleManager manager = new(terminal){
		new ModulePage(){
		    sim
		}
	    };
	    manager.SetWindowTitle("Freemium by cmsteffey");
            sim.AddText("Blacklisted titles:");
	    sim.AddText(string.Join('\n', blacklist));
	    sim.AddText(AnsiEscape.SgrGreenForeGround + AnsiEscape.SgrBrightBold + "Type \"blacklist add\" to add the currently playing content to the blacklist.");
	    sim.WriteOutput();
	    int currProcId = System.Diagnostics.Process.GetCurrentProcess().Id;
	    System.Diagnostics.Process spotify = System.Diagnostics.Process.GetProcesses().FirstOrDefault(x=>x.ProcessName.ToLower().Contains("spotify") && x.Id != currProcId);
	    if(spotify is null){
		sim.AddText(AnsiEscape.SgrRedForeGround + AnsiEscape.SgrBrightBold + "Couldn't find spotify process :/");
		sim.AddText("Press Ctrl + C to quit.");
		sim.WriteOutput();
	    }
	    if(string.IsNullOrEmpty(spotify.MainWindowTitle)){
		sim.AddText("This program doesn't work when the spotify window is closed.");
		sim.WriteOutput();
	    }
	    string cachedTitle = null;
	    bool cachedMuteState = false;
	    sim.LineEntered += async(s, e) =>{
		BaseModule senderSim = s as BaseModule;
		senderSim?.AddText("> " + e.Line);
		senderSim?.WriteOutput();
		switch(e.Line){
		    case "blacklist add":
			try{
			    string title = String.Copy(cachedTitle);
			    WriteNewBlacklistItem(title);
			    blacklist.Append(title);
			    cachedTitle = null;
			    senderSim?.AddText($"Successfully registered \"{title}\" as an ad");
			    senderSim?.WriteOutput();
			}catch(Exception ex){
			    if(senderSim is null) terminal.QuitApp(ex);
			    senderSim.AddText(ex);
			    senderSim.WriteOutput();
			}
			break;
		}
	    };
	    while(true){
		spotify = System.Diagnostics.Process.GetProcessById(spotify.Id);
		if(string.IsNullOrEmpty(spotify.MainWindowTitle)){
		    if(cachedTitle is not null){
			sim.AddText("This program doesn't work when the spotify window is closed.");
			sim.WriteOutput();
		    }
		    await 100;
		    continue;
		}
		if(spotify.MainWindowTitle == cachedTitle){
		    await 100;
			
		    continue;
		}else{
		    cachedTitle = spotify.MainWindowTitle;
		}
		if(blacklist.Contains(cachedTitle) && !cachedMuteState){
		    sim.AddText("Ad found! Muting now.");
		    sim.WriteOutput();
		    Mute();
		    cachedMuteState = true;
		}else if(!cachedMuteState){
		    sim.AddText("Now playing: " + cachedTitle);
		    sim.WriteOutput();
		}else if(!blacklist.Contains(cachedTitle)){
		    cachedMuteState = false;
		    Unmute();
		    sim.AddText("Now playing: " + cachedTitle);
		    sim.WriteOutput();
		}
	    }
        }
        private static void Mute(){
	    ((byte)0xAD).KeyPress();
	}
	private static void Unmute(){
	    ((byte)0xAD).KeyPress();
	}
	private static void WriteNewBlacklistItem(string newItem){
	    using var w = System.IO.File.AppendText(blacklistPath);
	    w.WriteLine(newItem);
	}
    }
}
