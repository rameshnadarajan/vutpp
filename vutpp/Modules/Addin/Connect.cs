namespace VUTPP
{
	using EnvDTE;
	using Extensibility;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;

	#region Read me for Add-in installation and setup information.
	// When run, the Add-in wizard prepared the registry for the Add-in.
	// At a later time, if the Add-in becomes unavailable for reasons such as:
	//   1) You moved this project to a computer other than which is was originally created on.
	//   2) You chose 'Yes' when presented with a message asking if you wish to remove the Add-in.
	//   3) Registry corruption.
	// you will need to re-register the Add-in by building the MyAddin21Setup project 
	// by right clicking the project in the Solution Explorer, then choosing install.
	#endregion
	
	/// <summary>
	///   The object for implementing an Add-in.
	/// </summary>
	/// <seealso class='IDTExtensibility2' />
	[GuidAttribute("E4A48A23-4919-4379-8C38-26DAD17B2F59"), ProgId(VUTPP.Constants.ProgId)]
	public class Connect : Object, Extensibility.IDTExtensibility2, IDTCommandTarget
	{
		/// <summary>
		///		Implements the constructor for the Add-in object.
		///		Place your initialization code within this method.
		/// </summary>
		public Connect()
		{
		}

		#region Public methods
		/// <summary>
		///      Implements the OnConnection method of the IDTExtensibility2 interface.
		///      Receives notification that the Add-in is being loaded.
		/// </summary>
		/// <param term='application'>
		///      Root object of the host application.
		/// </param>
		/// <param term='connectMode'>
		///      Describes how the Add-in is being loaded.
		/// </param>
		/// <param term='addInInst'>
		///      Object representing this Add-in.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, Extensibility.ext_ConnectMode connectMode, object addInInst, ref System.Array custom)
		{
//			Trace.Write( string.Format( "Addin.OnConnection {0}", connectMode.ToString() ) );

			Debug.Assert(application != null);

			LoadAddin(application);
			if (m_addin != null) 
			{
				m_addin.OnConnection(application, connectMode, addInInst, ref custom);
			}
//			Trace.Write( "Addin.OnConnection finish" );
		}

		/// <summary>
		///     Implements the OnDisconnection method of the IDTExtensibility2 interface.
		///     Receives notification that the Add-in is being unloaded.
		/// </summary>
		/// <param term='disconnectMode'>
		///      Describes how the Add-in is being unloaded.
		/// </param>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(Extensibility.ext_DisconnectMode disconnectMode, ref System.Array custom)
		{
//			Trace.Write( "Addin.OnDisconnection start" );
			if (m_addin != null) 
			{
				m_addin.OnDisconnection(disconnectMode, ref custom);
			}
//			Trace.Write( "Addin.OnDisconnection finish" );
		}

		/// <summary>
		///      Implements the OnAddInsUpdate method of the IDTExtensibility2 interface.
		///      Receives notification that the collection of Add-ins has changed.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnAddInsUpdate(ref System.Array custom)
		{
//			Trace.Write( "Addin.OnAddInsUpdate start" );
			if (m_addin != null) 
			{
				m_addin.OnAddInsUpdate(ref custom);
			}
//			Trace.Write( "Addin.OnAddInsUpdate finish" );
		}

		/// <summary>
		///      Implements the OnStartupComplete method of the IDTExtensibility2 interface.
		///      Receives notification that the host application has completed loading.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref System.Array custom)
		{
//			Trace.Write( "Addin.OnStartupComplete start" );
			if (m_addin != null) 
			{
				m_addin.OnStartupComplete(ref custom);
			}
//			Trace.Write( "Addin.OnStartupComplete finish" );
		}

		/// <summary>
		///      Implements the OnBeginShutdown method of the IDTExtensibility2 interface.
		///      Receives notification that the host application is being unloaded.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref System.Array custom)
		{
//			Trace.Write( "Addin.OnBeginShutdown start" );
			if (m_addin != null) 
			{
				m_addin.OnBeginShutdown(ref custom);
			}
//			Trace.Write( "Addin.OnBeginShutdown finish" );
		}
		
		/// <summary>
		///   Implements the QueryStatus method of the IDTCommandTarget interface.
		///   This is called when the command's availability is updated
		/// </summary>
		/// <param term='commandName'>
		///	  The name of the command to determine state for.
		/// </param>
		/// <param term='neededText'>
		///	  Text that is needed for the command.
		/// </param>
		/// <param term='status'>
		///	  The state of the command in the user interface.
		/// </param>
		/// <param term='commandText'>
		///	  Text requested by the neededText parameter.
		/// </param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText) 
		{
//			Trace.Write( "Addin.QueryStatus start" );
			if (m_addin != null) 
			{
				m_addin.QueryStatus(commandName, neededText, ref status, ref commandText);
			}
//			Trace.Write( "Addin.QueryStatus finish" );
		}

		/// <summary>
		///   Implements the Exec method of the IDTCommandTarget interface.
		///   This is called when the command is invoked.
		/// </summary>
		/// <param term='commandName'>
		///	  The name of the command to execute.
		/// </param>
		/// <param term='executeOption'>
		///	  Describes how the command should be run.
		/// </param>
		/// <param term='varIn'>
		///	  Parameters passed from the caller to the command handler.
		/// </param>
		/// <param term='varOut'>
		///	  Parameters passed from the command handler to the caller.
		/// </param>
		/// <param term='handled'>
		///	  Informs the caller if the command was handled or not.
		/// </param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled) 
		{
//			Trace.Write( "Addin.QueryStatus start" );
			if (m_addin != null) 
			{
				m_addin.Exec(commandName, executeOption, ref varIn, ref varOut, ref handled);
			}
//			Trace.Write( "Addin.QueryStatus finish" );
		}
		#endregion // Public methods

		#region Private methods

		private const string FrameworkNotSupported                  = "VS version {0} not supported.";

		/// <summary>
		///   Loads the add-in main module, depending on the running version of 
		///   the .NET Framework.
		/// </summary>
		private void LoadAddin( object application ) 
		{
			if( m_addin != null )
				return;

			try 
			{
				string dteVersion = ((DTE)application).Version;
				Assembly assembly = ImplementationAssemblyLoader.LoadMainAssembly(dteVersion);
				// find the main type that implements IAddin interface and create it
				Type type = assembly.GetType("VUTPP.VUTPPMain", true, true);
				Debug.Assert(type.GetInterface("IAddin", true) != null);
				ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
				m_addin = (IAddin)ci.Invoke(new object[0]);
				return;
			}
			catch (FileNotFoundException e) 
			{
				MessageBox.Show(string.Format(FileNotFoundMessage, e.FileName), Constants.ProgramTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception e) 
			{
				MessageBox.Show(e.ToString(), Constants.ProgramTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion // Private methods

		#region Private fields

		private IAddin m_addin = null;

		#endregion // Private fields

		#region String constants

		private const string FileNotFoundMessage                    = "File '{0}' not found.\nPlease reinstall the application.";

		#endregion // String constants
		
	}
}