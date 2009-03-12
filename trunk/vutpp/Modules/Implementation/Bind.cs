using Microsoft.VisualStudio.VCProject;
using Microsoft.VisualStudio.VCProjectEngine;
using EnvDTE;
using System;

namespace VUTPP
{
	/// <summary>
	/// Bind에 대한 요약 설명입니다.
	/// </summary>
	public class VCBind
	{
		static public string GetPrimaryOutput(Project project, string ActiveConfigurationName, BIND_TYPE bindType)
		{
			VCProject vcProject = (VCProject)project.Object;
			if (vcProject != null)
			{
				IVCCollection configs = (IVCCollection)vcProject.Configurations;
				VCConfiguration config = (VCConfiguration)configs.Item(ActiveConfigurationName);
				switch( bindType )
				{
					case BIND_TYPE.Application:
						if (config.ConfigurationType != Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeApplication)
							return null;
						break;

					case BIND_TYPE.DynamicLibrary:
						if (config.ConfigurationType != Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeDynamicLibrary)
							return null;
						break;
				}
				return config.PrimaryOutput;
			}
			return null;
		}

		static public string GetPreprocessorDefinitions(Project project)
		{
			VCProject vcProject = (VCProject)project.Object;
			if (vcProject != null)
			{
				IVCCollection configs = (IVCCollection)vcProject.Configurations;
				VCConfiguration config = (VCConfiguration)configs.Item(project.DTE.Solution.SolutionBuild.ActiveConfiguration.Name);
				if (config != null)
				{
					IVCCollection tools = (IVCCollection)config.Tools;
					VCCLCompilerTool compiler = (VCCLCompilerTool)tools.Item("VCCLCompilerTool");
					if (compiler != null)
					{
						if (compiler.PreprocessorDefinitions != null)
						{
							return compiler.PreprocessorDefinitions;
						}
					}
				}
			}
			return null;
		}

		static public ProjectItem GetProjectItemFromVCFile(object item)
		{
			VCFile file = item as VCFile;
			return file.Object as ProjectItem;
		}
	}
}
