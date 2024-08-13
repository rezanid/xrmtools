﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XrmGen.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("XrmGen.Resources.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No entities found! Please check the URL and Application ID in your &quot;.csproj&quot; file..
        /// </summary>
        internal static string CompletionNoEntityFound {
            get {
                return ResourceManager.GetString("CompletionNoEntityFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Searching entities....
        /// </summary>
        internal static string CompletionSearch {
            get {
                return ResourceManager.GetString("CompletionSearch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provided entity type does not have any logical name..
        /// </summary>
        internal static string EntityGenerator_NoLogicalName {
            get {
                return ResourceManager.GetString("EntityGenerator.NoLogicalName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Environment not initialized. You will have to first initialize the environment by calling {0}..
        /// </summary>
        internal static string EnvironmentNotInitialized {
            get {
                return ResourceManager.GetString("EnvironmentNotInitialized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to acquire DTE from Visual Studio. Consequently {0} is not initialized successfully..
        /// </summary>
        internal static string Package_InitializationErroMissingDte {
            get {
                return ResourceManager.GetString("Package.InitializationErroMissingDte", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to serialize plugin definition from {0}..
        /// </summary>
        internal static string PluginGenerator_DeserializationError {
            get {
                return ResourceManager.GetString("PluginGenerator.DeserializationError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Code generation failed due to invalid input file..
        /// </summary>
        internal static string PluginGenerator_InvalidConfig {
            get {
                return ResourceManager.GetString("PluginGenerator.InvalidConfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Assembly configuration does not contain any plugin types..
        /// </summary>
        internal static string PluginGenerator_NoPluginTypes {
            get {
                return ResourceManager.GetString("PluginGenerator.NoPluginTypes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The template provided for plugin code generation is empty. Please check the file in the following path: {0}.
        /// </summary>
        internal static string PluginGenerator_NullTemplate {
            get {
                return ResourceManager.GetString("PluginGenerator.NullTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The template provided for plugin code generation has erros. FilePath: {0}
        ///Error details: {1}.
        /// </summary>
        internal static string PluginGenerator_TemplateError {
            get {
                return ResourceManager.GetString("PluginGenerator.TemplateError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The path provided for plugin code generator is invalid. Path: {0}.
        /// </summary>
        internal static string PluginGenerator_TemplateInvalidPath {
            get {
                return ResourceManager.GetString("PluginGenerator.TemplateInvalidPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The path provided for plugin code generator was not found. Path: {0}.
        /// </summary>
        internal static string PluginGenerator_TemplatePathNotFound {
            get {
                return ResourceManager.GetString("PluginGenerator.TemplatePathNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Configuration does contain the path to template file..
        /// </summary>
        internal static string PluginGenerator_TemplatePathNotSet {
            get {
                return ResourceManager.GetString("PluginGenerator.TemplatePathNotSet", resourceCulture);
            }
        }
    }
}