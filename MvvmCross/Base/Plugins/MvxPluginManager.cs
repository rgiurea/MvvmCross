﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Reflection;
using MvvmCross.Platform.Exceptions;
using MvvmCross.Platform.Logging;

namespace MvvmCross.Platform.Plugins
{
    public class MvxPluginManager
        : IMvxPluginManager
    {
        private readonly Dictionary<Type, IMvxPlugin> _loadedPlugins = new Dictionary<Type, IMvxPlugin>();

        public Func<Type, IMvxPluginConfiguration> ConfigurationSource { get; set; }

        public MvxLoaderPluginRegistry Registry { get; private set; } = new MvxLoaderPluginRegistry ();

        public bool IsPluginLoaded<T>() where T : IMvxPluginLoader
        {
            lock (this)
            {
                return _loadedPlugins.ContainsKey(typeof(T));
            }
        }

        public void EnsurePluginLoaded<TType>()
        {
            EnsurePluginLoaded(typeof(TType));
        }

        public virtual void EnsurePluginLoaded(Type type)
        {
            var field = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
            if (field == null)
            {
                MvxLog.Instance.Trace("Plugin Instance not found - will not autoload {0}", type.FullName);
                return;
            }

            var instance = field.GetValue(null);
            if (instance == null)
            {
                MvxLog.Instance.Trace("Plugin Instance was empty - will not autoload {0}", type.FullName);
                return;
            }

            var pluginLoader = instance as IMvxPluginLoader;
            if (pluginLoader == null)
            {
                MvxLog.Instance.Trace("Plugin Instance was not a loader - will not autoload {0}", type.FullName);
                return;
            }

            EnsurePluginLoaded(pluginLoader);
        }

        protected virtual void EnsurePluginLoaded(IMvxPluginLoader pluginLoader)
        {
            var configurable = pluginLoader as IMvxConfigurablePluginLoader;
            if (configurable != null)
            {
                MvxLog.Instance.Trace("Configuring Plugin Loader for {0}", pluginLoader.GetType().FullName);
                var configuration = ConfigurationFor(pluginLoader.GetType());
                configurable.Configure(configuration);
            }

            MvxLog.Instance.Trace("Ensuring Plugin is loaded for {0}", pluginLoader.GetType().FullName);
            pluginLoader.EnsureLoaded();
        }

        public void EnsurePlatformAdaptionLoaded<T>()
            where T : IMvxPluginLoader
        {
            lock (this)
            {
                if (IsPluginLoaded<T>())
                {
                    return;
                }

                var toLoad = typeof(T);
                _loadedPlugins[toLoad] = ExceptionWrappedLoadPlugin(toLoad);
            }
        }

        public bool TryEnsurePlatformAdaptionLoaded<T>()
            where T : IMvxPluginLoader
        {
            lock (this)
            {
                if (IsPluginLoaded<T>())
                {
                    return true;
                }

                try
                {
                    var toLoad = typeof(T);
                    _loadedPlugins[toLoad] = ExceptionWrappedLoadPlugin(toLoad);
                    return true;
                }
                // pokemon 'catch them all' exception handling allowed here in this Try method
                catch (Exception exception)
                {
                    MvxLog.Instance.Warn("Failed to load plugin adaption {0} with exception {1}", typeof(T).FullName, exception.ToLongString());
                    return false;
                }
            }
        }

        private IMvxPlugin ExceptionWrappedLoadPlugin(Type toLoad)
        {
            try
            {
                var plugin = LoadPlugin(toLoad);
                var configurablePlugin = plugin as IMvxConfigurablePlugin;
                if (configurablePlugin != null)
                {
                    var configuration = ConfigurationSource(configurablePlugin.GetType());
                    configurablePlugin.Configure(configuration);
                }
                plugin.Load();
                return plugin;
            }
            catch (MvxException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw exception.MvxWrap();
            }
        }

        private IMvxPlugin LoadFromRegistry (Type toLoad)
        {
            var loader = Registry.FindLoader (toLoad);
            return loader?.Invoke ();
        }

        private IMvxPlugin LoadPlugin (Type toLoad)
        {
            return LoadFromRegistry (toLoad) ?? FindPlugin(toLoad);
        }

        protected IMvxPluginConfiguration ConfigurationFor(Type toLoad) => ConfigurationSource?.Invoke(toLoad);

        protected virtual IMvxPlugin FindPlugin(Type toLoad)
        {
            throw new MvxException ("Could not find plugin loader for type {0}", toLoad.FullName);
        }
    }
}
