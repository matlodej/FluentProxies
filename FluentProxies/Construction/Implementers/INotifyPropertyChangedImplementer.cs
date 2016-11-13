using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Construction.Implementers
{
    internal class INotifyPropertyChangedImplementer : Implementer
    {
        #region Fields and properties

        private MethodInfo _raisePropertyChanged;

        internal override Type Interface
        {
            get
            {
                return typeof(INotifyPropertyChanged);
            }
        }

        #endregion

        #region Methods

        internal override void Implement(TypeBuilder typeBuilder)
        {
            MethodInfo delegateCombine = typeof(Delegate).GetMethod("Combine", new Type[] { typeof(Delegate), typeof(Delegate) });
            MethodInfo delegateRemove = typeof(Delegate).GetMethod("Remove", new Type[] { typeof(Delegate), typeof(Delegate) });

            MethodInfo invokeDelegate = typeof(PropertyChangedEventHandler).GetMethod("Invoke");

            FieldBuilder eventBack = typeBuilder.DefineField("PropertyChanged", typeof(PropertyChangedEventHandler), FieldAttributes.Private);

            ConstructorInfo createEventArgs = typeof(PropertyChangedEventArgs).GetConstructor(new Type[] { typeof(String) });

            MethodBuilder addPropertyChanged = typeBuilder.DefineMethod("add_PropertyChanged",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(void),
                new Type[] { typeof(PropertyChangedEventHandler) });

            ILGenerator gen = addPropertyChanged.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, eventBack);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, delegateCombine);
            gen.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
            gen.Emit(OpCodes.Stfld, eventBack);
            gen.Emit(OpCodes.Ret);

            MethodBuilder removePropertyChanged = typeBuilder.DefineMethod("remove_PropertyChanged",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(void),
                new Type[] { typeof(PropertyChangedEventHandler) });

            gen = removePropertyChanged.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, eventBack);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, delegateRemove);
            gen.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
            gen.Emit(OpCodes.Stfld, eventBack);
            gen.Emit(OpCodes.Ret);

            MethodBuilder raisePropertyChanged = typeBuilder.DefineMethod("OnPropertyChanged",
                MethodAttributes.Public,
                typeof(void),
                new Type[] { typeof(String) });

            _raisePropertyChanged = raisePropertyChanged;

            gen = raisePropertyChanged.GetILGenerator();

            Label lblDelegateOk = gen.DefineLabel();

            gen.DeclareLocal(typeof(PropertyChangedEventHandler));
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, eventBack);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brtrue, lblDelegateOk);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Newobj, createEventArgs);
            gen.Emit(OpCodes.Callvirt, invokeDelegate);
            gen.MarkLabel(lblDelegateOk);
            gen.Emit(OpCodes.Ret);

            EventBuilder eventBuilder = typeBuilder.DefineEvent("PropertyChanged", EventAttributes.None, typeof(PropertyChangedEventHandler));

            eventBuilder.SetRaiseMethod(raisePropertyChanged);
            eventBuilder.SetAddOnMethod(addPropertyChanged);
            eventBuilder.SetRemoveOnMethod(removePropertyChanged);
        }

        internal override void AfterSet(ILGenerator gen, PropertyInfo propertyInfo)
        {
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, propertyInfo.Name);
            gen.Emit(OpCodes.Call, _raisePropertyChanged);
        }

        #endregion
    }
}
