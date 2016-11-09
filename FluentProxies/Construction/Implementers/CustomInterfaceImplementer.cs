using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FluentProxies.Enums;

namespace FluentProxies.Construction.Implementers
{
    public class CustomInterfaceImplementer : Implementer
    {
        #region Fields and properties

        private List<Action<TypeBuilder>> _implementHandlers = new List<Action<TypeBuilder>>();

        private List<Action<ILGenerator, PropertyInfo>> _beforeGetHandlers = new List<Action<ILGenerator, PropertyInfo>>();

        private List<Action<ILGenerator, PropertyInfo>> _beforeSetHandlers = new List<Action<ILGenerator, PropertyInfo>>();

        private List<Action<ILGenerator, PropertyInfo>> _afterSetHandlers = new List<Action<ILGenerator, PropertyInfo>>();

        #endregion

        #region Initialization

        public static CustomInterfaceImplementer CreateImplementer(Type interfaceType)
        {
            return new CustomInterfaceImplementer(interfaceType);
        }

        internal CustomInterfaceImplementer(Type interfaceType)
            : base(interfaceType)
        {
        }

        #endregion

        #region Methods

        public CustomInterfaceImplementer AddImplementation(Action<TypeBuilder> implementation)
        {
            _implementHandlers.Add(implementation);
            return this;
        }

        public CustomInterfaceImplementer AddPropertyAction(PropertyAction propertyAction, Action<ILGenerator, PropertyInfo> action)
        {
            switch (propertyAction)
            {
                case PropertyAction.BeforeGet:
                    _beforeGetHandlers.Add(action);
                    break;

                case PropertyAction.BeforeSet:
                    _beforeSetHandlers.Add(action);
                    break;

                case PropertyAction.AfterSet:
                    _afterSetHandlers.Add(action);
                    break;
            }

            return this;
        }

        internal override void Implement(TypeBuilder typeBuilder)
        {
            foreach (Action<TypeBuilder> implement in _implementHandlers)
            {
                implement(typeBuilder);
            }
        }

        internal override void BeforeGet(ILGenerator gen, PropertyInfo propertyInfo)
        {
            foreach (Action<ILGenerator, PropertyInfo> action in _beforeGetHandlers)
            {
                action(gen, propertyInfo);
            }
        }

        internal override void BeforeSet(ILGenerator gen, PropertyInfo propertyInfo)
        {
            foreach (Action<ILGenerator, PropertyInfo> action in _beforeSetHandlers)
            {
                action(gen, propertyInfo);
            }
        }

        internal override void AfterSet(ILGenerator gen, PropertyInfo propertyInfo)
        {
            foreach (Action<ILGenerator, PropertyInfo> action in _afterSetHandlers)
            {
                action(gen, propertyInfo);
            }
        }

        #endregion
    }
}
