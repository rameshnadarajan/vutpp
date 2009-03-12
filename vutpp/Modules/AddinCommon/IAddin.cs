using EnvDTE;
using Extensibility;
using System;

namespace VUTPP
{
	/// <summary>
	///   Add-in interface.
	/// </summary>
	public interface IAddin 
	{

		void OnConnection(object application, Extensibility.ext_ConnectMode connectMode, object addInInst, ref System.Array custom);

		void OnDisconnection(Extensibility.ext_DisconnectMode disconnectMode, ref System.Array custom);

		void OnAddInsUpdate(ref System.Array custom);

		void OnStartupComplete(ref System.Array custom);

		void OnBeginShutdown(ref System.Array custom);

		void QueryStatus(string commandName, EnvDTE.vsCommandStatusTextWanted neededText, ref EnvDTE.vsCommandStatus status, ref object commandText);

		void Exec(string commandName, EnvDTE.vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled);

	}
}
