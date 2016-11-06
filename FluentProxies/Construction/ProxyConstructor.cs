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

namespace FluentProxies.Construction
{
    internal class ProxyConstructor<T>
        where T : class, new()
    {
        private const string WRAPPER_FIELD = "_proxyWrapper";

        private readonly ProxyBuilder<T> _builder;

        private readonly List<Implementer> _implementers = new List<Implementer>();

        private static readonly ConcurrentDictionary<Type, Type> _proxiedTypes = new ConcurrentDictionary<Type, Type>();

        internal ProxyConstructor(ProxyBuilder<T> proxyBuilder)
        {
            _builder = proxyBuilder;
        }

        internal T Construct()
        {
            Type proxyType;
            
            if (_builder.OverridesCache || !_proxiedTypes.TryGetValue(typeof(T), out proxyType))
            {
                TypeBuilder typeBuilder = CreateTypeBuilder();

                foreach (Type type in _builder.TypesToImplement)
                {
                    Implementer implementer = Implementer.Resolve(type).Implement(typeBuilder);
                    _implementers.Add(implementer);
                }

                AddProperties(typeBuilder);

                proxyType = typeBuilder.CreateType();

                _proxiedTypes.TryAdd(typeof(T), proxyType);
            }

            T proxy = Instantiator.Clone(_builder.SourceObject, proxyType);

            if (_builder.IncludesWrapper)
            {
                ProxyWrapper<T> proxyWrapper = new ProxyWrapper<T>(_builder, proxy);
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
                _builder.TypesToImplement.ToArray());

            foreach (Type type in _builder.TypesToImplement)
            {
                typeBuilder.AddInterfaceImplementation(type);
            }

            return typeBuilder;
        }

        private void AddProperties(TypeBuilder typeBuilder)
        {
            FieldBuilder wrapperField = null;

            if (_builder.IncludesWrapper)
            {
                wrapperField = typeBuilder.DefineField(WRAPPER_FIELD, typeof(ProxyWrapper<T>), FieldAttributes.Public);
            }

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            bool hasComplexProperties = properties.Any(x => !x.PropertyType.IsValueType && x.PropertyType != typeof(String));

            foreach (PropertyInfo propertyInfo in properties)
            {
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);

                MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

                if (propertyInfo.GetGetMethod() != null && propertyInfo.GetGetMethod().IsVirtual)
                {
                    MethodBuilder getMethod = typeBuilder.DefineMethod("get_" + propertyInfo.Name, getSetAttr, propertyInfo.PropertyType, Type.EmptyTypes);

                    ILGenerator gen = getMethod.GetILGenerator();

                    _implementers.ForEach(x => x.BeforeGet(gen, propertyInfo));

                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
                    gen.Emit(OpCodes.Ret);

                    propertyBuilder.SetGetMethod(getMethod);
                }

                if (propertyInfo.GetSetMethod() != null && propertyInfo.GetSetMethod().IsVirtual)
                {
                    MethodBuilder setMethod = typeBuilder.DefineMethod("set_" + propertyInfo.Name, getSetAttr, null, new Type[] { propertyInfo.PropertyType });

                    ILGenerator gen = setMethod.GetILGenerator();

                    _implementers.ForEach(x => x.BeforeSet(gen, propertyInfo));

                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldarg_1);
                    gen.Emit(OpCodes.Call, propertyInfo.GetSetMethod());

                    if (_builder.SyncsWithSourceObject
                        && !hasComplexProperties)
                    {
                        ProxyWrapper<T> tmp;
                        Label notInitialized = gen.DefineLabel();

                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Ldfld, wrapperField);
                        gen.Emit(OpCodes.Brfalse, notInitialized);
                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Ldfld, wrapperField);
                        gen.Emit(OpCodes.Call, typeof(ProxyWrapper<T>).GetProperty(nameof(tmp.SourceObject)).GetGetMethod());
                        gen.Emit(OpCodes.Ldarg_1);
                        gen.Emit(OpCodes.Call, propertyInfo.GetSetMethod());
                        gen.MarkLabel(notInitialized);
                    }

                    _implementers.ForEach(x => x.AfterSet(gen, propertyInfo));

                    gen.Emit(OpCodes.Ret);
                    
                    propertyBuilder.SetSetMethod(setMethod);
                }
            }
        }
    }
}
