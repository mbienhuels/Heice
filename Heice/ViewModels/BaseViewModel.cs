using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Markup;
using Heice.Annotations;

namespace Heice.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object> _propertyStore = new();

        private readonly Dictionary<string, List<string>> _dependencyProperties = new();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            
            if (string.IsNullOrEmpty(propertyName))
                return;
            
            if (!_dependencyProperties.ContainsKey(propertyName))
                return;

            foreach (var depProp in _dependencyProperties[propertyName])
                OnPropertyChanged(depProp);
        }

        protected void SetProp<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException($"{nameof(propertyName)} required");

            if (value != null)
            {
                if (!_propertyStore.ContainsKey(propertyName))
                    _propertyStore.Add(propertyName, value);
                else
                    _propertyStore[propertyName] = value;
            }
            else if (_propertyStore.ContainsKey(propertyName))
                _propertyStore.Remove(propertyName);

            OnPropertyChanged(propertyName);
        }
        
        protected T GetProp<T>([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException($"{nameof(propertyName)} required");
                    
            if (!_propertyStore.ContainsKey(propertyName))
                return default;

            var value = _propertyStore[propertyName];

            if (!(value is T tValue))
                throw new ArgumentException("Invalid type provided");

            return tValue;
        }

        protected void MapDependencies<T>()
        {
            var type = typeof(T);
            var props= type.GetProperties();

            foreach (var prop in props)
                LoadPropertyDependencies(prop);
            
            // idk why but this seems to be required
            LoadPropertyDependencies(props[^1]);
        }

        private void LoadPropertyDependencies(PropertyInfo prop)
        {
            var depProps = prop
                .GetCustomAttributes(true)
                .ToList();
                
            if (!depProps.Any())
                return;

            foreach (var depPropObj in depProps)
            {
                if (!(depPropObj is DependsOnAttribute depProp))
                    continue;
                
                if (!_dependencyProperties.ContainsKey(depProp.Name))
                {
                    _dependencyProperties.Add(depProp.Name, new List<string> { prop.Name });
                    continue;
                }
                
                if (_dependencyProperties[depProp.Name].Contains(prop.Name))
                    continue;
                
                _dependencyProperties[depProp.Name].Add(prop.Name);
            }
        }
    }
}