using FluentProxies.Helpers;
using FluentProxies.Implementers;
using FluentProxies.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies
{
    internal class ProxyConstructor<T>
        where T : class, new()
    {
        private const string WRAPPER_FIELD = "_proxyWrapper";

        private readonly ProxyBuilder<T> _builder;

        private readonly List<Implementer> _implementers;

        internal ProxyConstructor(ProxyBuilder<T> proxyBuilder)
        {
            _builder = proxyBuilder;
            _implementers = Implementer.Resolve(proxyBuilder.Blueprint.Implementations);
        }

        internal T Construct()
        {
            Type proxyType = Cache.Types.GetAll(_builder.Blueprint, (x, y) => Validator.AreEqual(x, y)).FirstOrDefault();

            if (proxyType is null)
            {
                TypeBuilder typeBuilder = CreateTypeBuilder();

                foreach (Implementer implementer in _implementers)
                {
                    implementer.Implement(typeBuilder);
                }

                OverrideBaseProperties(typeBuilder);
                AddCustomProperties(typeBuilder);

                proxyType = typeBuilder.CreateType();

                Cache.Types.TryAdd(_builder.Blueprint, proxyType);
            }

            T proxy = Instantiator.Clone(_builder.SourceReference, proxyType);

            if (_builder.Blueprint.SyncsWithReference)
            {
                ProxyWrapper<T> proxyWrapper = new ProxyWrapper<T>(_builder.SourceReference);
                proxyType.GetField(WRAPPER_FIELD).SetValue(proxy, proxyWrapper);

                Cache.Wrappers.TryAdd(proxy, proxyWrapper);
            }

            return proxy;
        }

        private TypeBuilder CreateTypeBuilder()
        {
            Type[] interfaces = _implementers.Select(x => x.Interface)
                .Concat(_builder.Blueprint.Interfaces.Select(x => x.Type).Distinct())
                .ToArray();

            AssemblyName assemblyName = new AssemblyName("NotifiableProxy.dll");

            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name, false);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeof(T).Name + "_Proxy",
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                typeof(T),
                interfaces);

            foreach (Type i in interfaces)
            {
                typeBuilder.AddInterfaceImplementation(i);
            }

            return typeBuilder;
        }

        private void OverrideBaseProperties(TypeBuilder typeBuilder)
        {
            FieldBuilder wrapperField = null;
            ProxyWrapper<T> tmp;

            if (_builder.Blueprint.SyncsWithReference)
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

                    _implementers.ForEach(x => x.BeforeGet(gen, propertyBuilder));

                    if (_builder.Blueprint.SyncsWithReference)
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

                    _implementers.ForEach(x => x.BeforeSet(gen, propertyBuilder));

                    if (_builder.Blueprint.SyncsWithReference)
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

                    _implementers.ForEach(x => x.AfterSet(gen, propertyBuilder));

                    gen.Emit(OpCodes.Ret);

                    propertyBuilder.SetSetMethod(setMethod);
                }
            }
        }

        private void AddCustomProperties(TypeBuilder typeBuilder)
        {
            foreach (PropertyModel propertyModel in _builder.Blueprint.Properties)
            {
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyModel.Name, PropertyAttributes.None, propertyModel.Type, Type.EmptyTypes);

                MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.Virtual;

                FieldBuilder fieldBuilder = typeBuilder.DefineField($"_{Char.ToLower(propertyModel.Name[0])}{propertyModel.Name.Substring(1)}", propertyModel.Type, FieldAttributes.Private);

                MethodBuilder getMethod = typeBuilder.DefineMethod("get_" + propertyModel.Name, getSetAttr, propertyModel.Type, Type.EmptyTypes);

                ILGenerator gen = getMethod.GetILGenerator();

                _implementers.ForEach(x => x.BeforeGet(gen, propertyBuilder));

                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, fieldBuilder);
                gen.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getMethod);

                MethodBuilder setMethod = typeBuilder.DefineMethod("set_" + propertyModel.Name, getSetAttr, null, new Type[] { propertyModel.Type });

                gen = setMethod.GetILGenerator();

                _implementers.ForEach(x => x.BeforeSet(gen, propertyBuilder));

                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, fieldBuilder);

                _implementers.ForEach(x => x.AfterSet(gen, propertyBuilder));

                gen.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setMethod);
            }
        }
    }
}
