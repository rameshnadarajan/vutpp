using Microsoft.VisualStudio.VCProject;
using Microsoft.VisualStudio.VCProjectEngine;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Text;

public class VCBind
{
    static public string GetPrimaryOutput(Project project, string ActiveConfigurationName)
    {
        VCProject vcProject = (VCProject)project.Object;
        if (vcProject != null)
        {
            IVCCollection configs = (IVCCollection)vcProject.Configurations;
            VCConfiguration config = (VCConfiguration)configs.Item(ActiveConfigurationName);
            if (config.ConfigurationType == Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeDynamicLibrary)
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
}
