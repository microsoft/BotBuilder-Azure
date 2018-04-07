// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// The keys for application settings.
    /// </summary>
    public class AppSettingKeys
    {
        /// <summary>
        /// The bot state endpoint key.
        /// </summary>
        public const string StateEndpoint = "BotStateEndpoint";

        /// <summary>
        /// The open id url key.
        /// </summary>
        public const string OpenIdMetadata = "BotOpenIdMetadata";

        /// <summary>
        /// The Microsoft app Id key.
        /// </summary>
        public const string AppId = "MicrosoftAppId";

        /// <summary>
        /// The Microsoft app password key.
        /// </summary>
        public const string Password = "MicrosoftAppPassword";

        /// <summary>
        /// The key for azure table storage connection string.
        /// </summary>
        public const string TableStorageConnectionString = "AzureWebJobsStorage";

        /// <summary>
        /// Key for the flag indicating if table storage should be used as bot state store.
        /// </summary>
        public const string UseTableStorageForConversationState = "UseTableStorageForConversationState";

        /// <summary>
        /// Key for the flag indicating if table storage2 should be used as bot state store.
        /// </summary>
        public const string UseTableStorage2ForConversationState = "UseTableStorage2ForConversationState";

        /// <summary>
        /// Key for the flag indicating if cosmos db should be used as bot state store.
        /// </summary>
        public const string UseCosmosDbForConversationState = "UseCosmosDbForConversationState";

        /// <summary>
        /// The endpoint cosmos db storage if the UseCosmosDbForConversationState flag was set
        /// </summary>
        public const string CosmosDbEndpoint = "CosmosDbEndpoint";

        /// <summary>
        /// The key cosmos db storage if the UseCosmosDbForConversationState flag was set
        /// </summary>
        public const string CosmosDbKey = "CosmosDbKey";

        /// <summary>
        /// Key for the flag indicating if sql server should be used as bot state store.
        /// </summary>
        public const string UseSqlServerForConversationState = "UseSqlServerForConversationState";

        /// <summary>
        /// The key for sql table storage connection string.
        /// </summary>
        public const string SqlServerConnectionString = "SqlServerConnection";

    }


    /// <summary>
    /// A utility class for bots running on Azure.
    /// </summary>
    public sealed class Utils
    {
        /// <summary>
        /// Get value corresponding to the key from application settings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetAppSetting(string key)
        {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }

        /// <summary>
        /// Get the open Id configuration url from application settings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The open id configuration.</returns>
        public static string GetOpenIdConfigurationUrl(string key = AppSettingKeys.OpenIdMetadata)
        {
            var result = Utils.GetAppSetting(key);
            if (String.IsNullOrEmpty(result))
            {
                result = JwtConfig.ToBotFromChannelOpenIdMetadataUrl;
            }
            return result;
        }

        /// <summary>
        /// Get the state api endpoint. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The state api endpoint.</returns>
        public static string GetStateApiUrl(string key = AppSettingKeys.StateEndpoint)
        {
            var result = Utils.GetAppSetting(key);
            if (String.IsNullOrEmpty(result))
            {
                result = "https://state.botframework.com/";
            }
            return result;
        }

        private static readonly Regex AzureFunctionAssembly = new Regex(@"(\S+)#\d+-\d+", RegexOptions.Compiled);

        internal static string RemoveAzureFunctionsDynamicSuffix(string name)
        {
            var match = AzureFunctionAssembly.Match(name);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return name;
        }
    }

    /// <summary>
    /// This surrogate is responsible for serialization of delegates and map them to the matching delegates 
    /// in the assembly passed to the constructor during deserialization.
    /// </summary>
    public class BotServiceDelegateSurrogate : Serialization.ISurrogateProvider
    {

        /// <summary>
        /// The key for the type of delegate in the SerializationInfo.
        /// </summary>
        public const string NameType = "type";

        /// <summary>
        /// The key for the target of delegate in the SerializationInfo.
        /// </summary>
        public const string NameTarget = "target";

        /// <summary>
        /// The key for the <see cref="MethodInfo"/> of delegate in the SerializationInfo.
        /// </summary>
        public const string NameMethod = "method";

        /// <summary>
        /// The binding flags used for <see cref="Type.GetMethod(string,System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])"/>
        /// </summary>
        public static readonly BindingFlags BindingFlags =
            BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Assembly assembly;

        /// <summary>
        /// Constructs an instance of surrogate provider.
        /// </summary>
        /// <param name="assembly">The assembly that will use for delegate mapping.</param>
        public BotServiceDelegateSurrogate(Assembly assembly)
        {
            SetField.NotNull(out this.assembly, nameof(assembly), assembly);
        }

        bool Serialization.ISurrogateProvider.Handles(Type type, StreamingContext context, out int priority)
        {
            bool handles = typeof(Delegate).IsAssignableFrom(type);
            priority = handles ? int.MaxValue : 0;
            return handles;
        }


        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var lambda = (Delegate)obj;
            info.AddValue(NameType, lambda.GetType());
            info.AddValue(NameTarget, lambda.Target);
            info.AddValue(NameMethod, lambda.Method);
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var type = info.GetValue<Type>(NameType);
            var target = info.GetValue<object>(NameTarget);
            var method = info.GetValue<MethodInfo>(NameMethod);

            //this checks if azure function assembly is changed and maps the delegate to 
            //the new dynamic assembly generated.
            if (method.Module.Assembly.FullName != this.assembly.FullName &&
                Utils.RemoveAzureFunctionsDynamicSuffix(method.Module.Assembly.FullName) == Utils.RemoveAzureFunctionsDynamicSuffix(this.assembly.FullName))
            {
                return Delegate.CreateDelegate(type, target, this.GetMethodInfoFromCurrentAssembly(method));
            }

            return Delegate.CreateDelegate(type, target, method);
        }

        /// <summary>
        /// Maps the method info to the new assembly.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <returns>The method info for the corresponding method in the new assembly.</returns>
        protected virtual MethodInfo GetMethodInfoFromCurrentAssembly(MethodInfo method)
        {
            var targetType = this.assembly.GetType(method.ReflectedType.FullName, throwOnError: true);
            var methodParamTypes = method.GetParameters().Select(t => t.ParameterType).ToArray();

            return targetType.GetMethod(method.Name, BindingFlags, null, CallingConventions.Any, methodParamTypes, null);
        }
    }

    /// <summary>
    /// <see cref="SerializationBinder"/> responsible for mapping the matching types to the types in the assembly passed to the constructor during deserialization.
    /// </summary>
    public sealed class BotServiceSerializationBinder : SerializationBinder
    {
        private readonly Assembly assembly;

        /// <summary>
        /// Constructs an instance of bot service <see cref="SerializationBinder"/>.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public BotServiceSerializationBinder(Assembly assembly)
        {
            SetField.NotNull(out this.assembly, nameof(assembly), assembly);
        }

        /// <summary>
        /// <see cref="SerializationBinder.BindToType"/>
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="typeName"></param>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (Utils.RemoveAzureFunctionsDynamicSuffix(assemblyName) ==
                Utils.RemoveAzureFunctionsDynamicSuffix(this.assembly.FullName))
            {
                assemblyName = this.assembly.FullName;
            }

            // Get the type based on the type name and assembly name
            // https://msdn.microsoft.com/en-us/library/system.runtime.serialization.serializationbinder(v=vs.110).aspx
            return Type.GetType($"{typeName}, {assemblyName}");
        }
    }

    /// <summary>
    /// A helper class responsible for resolving the calling assembly
    /// </summary>
    public sealed class ResolveAssembly : IDisposable
    {
        private readonly Assembly assembly;

        /// <summary>
        /// Creates an instance of ResovelCallingAssembly
        /// </summary>
        /// <param name="assembly"> The assembly</param>
        public static ResolveAssembly Create(Assembly assembly)
        {
            return new ResolveAssembly(assembly);
        }

        /// <summary>
        /// Creates an instance of ResovelCallingAssembly
        /// </summary>
        /// <param name="assembly"> The assembly</param>
        private ResolveAssembly(Assembly assembly)
        {
            SetField.NotNull(out this.assembly, nameof(assembly), assembly);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        void IDisposable.Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs arguments)
        {
            Assembly resolvedAssembly = null;

            if (arguments.Name == this.assembly.FullName)
            {
                resolvedAssembly = assembly;
            }
            else
            {
                resolvedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                            .FirstOrDefault(a => a.GetName().FullName == arguments.Name);

                // Fix based on following comment: https://github.com/Microsoft/BotBuilder/issues/2407#issuecomment-325097648
                if (resolvedAssembly == null)
                {
                    resolvedAssembly = LoadFromFile(arguments.Name);
                }
            }

            return resolvedAssembly;
        }

        private Assembly LoadFromFile(string name)
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyName = new AssemblyName(name);
            var assemblyFileName = assemblyName.Name + ".dll";
            string assemblyPath;

            if (assemblyName.Name.EndsWith(".resources"))
            {
                var resourceDirectory = Path.Combine(assemblyDirectory, assemblyName.CultureName);
                assemblyPath = Path.Combine(resourceDirectory, assemblyFileName);
            }
            else
            {
                assemblyPath = Path.Combine(assemblyDirectory, assemblyFileName);
            }

            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            return null;
        }
    }
}
