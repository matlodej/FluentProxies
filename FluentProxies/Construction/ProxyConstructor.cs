using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FluentProxies.Construction.Implementers;
using FluentProxies.Construction.Utils;
using FluentProxies.Models;

namespace FluentProxies.Construction
{
    internal class ProxyConstructor<T>
        where T : class, new()
    {
        #region Constants

        private const string WRAPPER_FIELD = "_proxyWrapper";

        #endregion

        #region Fields and properties

        private static readonly ConcurrentDictionary<ProxyConfiguration, Type> _proxyCache = new ConcurrentDictionary<ProxyConfiguration, Type>();

        private readonly ProxyBuilder<T> _builder;

        private readonly List<Implementer> _implementers;

        #endregion

        #region Initialization

        internal ProxyConstructor(ProxyBuilder<T> proxyBuilder)
        {
            _builder = proxyBuilder;
            _implementers = Implementer.Resolve(proxyBuilder.Configuration.Implementations);
        }

        #endregion

        #region Methods

        internal T Construct()
        {
            Type proxyType;

            ProxyConfiguration cachedConfiguration = _proxyCache.FirstOrDefault(x => Validator.AreEqual(x.Key, _builder.Configuration)).Key;
            
            if (cachedConfiguration == null || !_proxyCache.TryGetValue(cachedConfiguration, out proxyType))
            {
                TypeBuilder typeBuilder = CreateTypeBuilder();

                foreach (Implementer implementer in _implementers)
                {
                    implementer.Implement(typeBuilder);
                }

                AddProperties(typeBuilder);

                proxyType = typeBuilder.CreateType();

                _proxyCache.TryAdd(_builder.Configuration, proxyType);
            }

            T proxy = Instantiator.Clone(_builder.SourceReference, proxyType);

            if (_builder.Configuration.SyncsWithReference)
            {
                ProxyWrapper<T> proxyWrapper = new ProxyWrapper<T>(proxy, _builder.SourceReference);
                proxyType.GetField(WRAPPER_FIELD).SetValue(proxy, proxyWrapper);
            }

            return proxy;
        }

        private TypeBuilder CreateTypeBuilder()
        {
            AssemblyName assemblyName = new AssemblyName("NotifiableProxy.dll");

            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name, false);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeof(T).Name + "_Proxy",
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                typeof(T),
                _implementers.Select(x => x.Interface).ToArray());

            foreach (Type type in _implementers.Select(x => x.Interface))
            {
                typeBuilder.AddInterfaceImplementation(type);
            }

            return typeBuilder;
        }

        private void AddProperties(TypeBuilder typeBuilder)
        {
            FieldBuilder wrapperField = null;
            ProxyWrapper<T> tmp;

            if (_builder.Configuration.SyncsWithReference)
            {
                wrapperField = typeBuilder.DefineField(WRAPPER_FIELD, typeof(ProxyWrapper<T>), FieldAttributes.Public);
            }

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propertyInfo in properties)
            {
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);

                MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

                if (propertyInfo.GetGetMethod() != null && propertyInfo.GetGetMethod().IsVirtual)
                {
                    MethodBuilder getMethod = typeBuilder.DefineMethod("get_" + propertyInfo.Name, getSetAttr, propertyInfo.PropertyType, Type.EmptyTypes);

                    ILGenerator gen = getMethod.GetILGenerator();

                    _implementers.ForEach(x => x.BeforeGet(gen, propertyInfo));

                    if (_builder.Configuration.SyncsWithReference)
                    {
                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Ldfld, wrapperField);
                        gen.Emit(OpCodes.Call, typeof(ProxyWrapper<T>).GetProperty(nameof(tmp.SourceReference)).GetGetMethod());
                        gen.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
                    }
                    else
                    {
                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
                    }

                    gen.Emit(OpCodes.Ret);

                    propertyBuilder.SetGetMethod(getMethod);
                }

                if (propertyInfo.GetSetMethod() != null && propertyInfo.GetSetMethod().IsVirtual)
                {
                    MethodBuilder setMethod = typeBuilder.DefineMethod("set_" + propertyInfo.Name, getSetAttr, null, new Type[] { propertyInfo.PropertyType });

                    ILGenerator gen = setMethod.GetILGenerator();

                    _implementers.ForEach(x => x.BeforeSet(gen, propertyInfo));

                    if (_builder.Configuration.SyncsWithReference)
                    {
                        Label notInitialized = gen.DefineLabel();

                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Ldfld, wrapperField);
                        gen.Emit(OpCodes.Brfalse, notInitialized);
                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Ldfld, wrapperField);
                        gen.Emit(OpCodes.Call, typeof(ProxyWrapper<T>).GetProperty(nameof(tmp.SourceReference)).GetGetMethod());
                        gen.Emit(OpCodes.Ldarg_1);
                        gen.Emit(OpCodes.Call, propertyInfo.GetSetMethod());
                        gen.MarkLabel(notInitialized);
                    }
                    else
                    {
                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Ldarg_1);
                        gen.Emit(OpCodes.Call, propertyInfo.GetSetMethod());
                    }

                    _implementers.ForEach(x => x.AfterSet(gen, propertyInfo));

                    gen.Emit(OpCodes.Ret);

                    propertyBuilder.SetSetMethod(setMethod);
                }
            }
        }

        #endregion
    }
}
