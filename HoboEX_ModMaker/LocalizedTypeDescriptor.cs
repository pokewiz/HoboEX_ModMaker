using System;
using System.ComponentModel;

namespace HoboEX_ModMaker
{
    public class LocalizedTypeDescriptorProvider : TypeDescriptionProvider
    {
        public LocalizedTypeDescriptorProvider(TypeDescriptionProvider parent) : base(parent) { }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new LocalizedTypeDescriptor(base.GetTypeDescriptor(objectType, instance), instance);
        }
        
        public static void Register(Type type)
        {
            TypeDescriptor.AddProvider(new LocalizedTypeDescriptorProvider(TypeDescriptor.GetProvider(type)), type);
        }
    }

    public class LocalizedTypeDescriptor : CustomTypeDescriptor
    {
        private object _instance;

        public LocalizedTypeDescriptor(ICustomTypeDescriptor parent, object instance) : base(parent) 
        {
            _instance = instance;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var original = base.GetProperties();
            var newProps = new PropertyDescriptor[original.Count];
            for (int i = 0; i < original.Count; i++)
            {
                var lpd = new LocalizedPropertyDescriptor(original[i]);
                lpd.SetInstanceContext(_instance);
                newProps[i] = lpd;
            }
            return new PropertyDescriptorCollection(newProps);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var original = base.GetProperties(attributes);
            var newProps = new PropertyDescriptor[original.Count];
            for (int i = 0; i < original.Count; i++)
            {
                var lpd = new LocalizedPropertyDescriptor(original[i]);
                lpd.SetInstanceContext(_instance); // Pass instance for context
                newProps[i] = lpd;
            }
            return new PropertyDescriptorCollection(newProps);
        }
    }

    public class LocalizedPropertyDescriptor : PropertyDescriptor
    {
        private readonly PropertyDescriptor _basePropertyDescriptor;
        private string _localizedName;
        private string _localizedDescription;
        private string _localizedCategory;

        public LocalizedPropertyDescriptor(PropertyDescriptor basePropertyDescriptor) : base(basePropertyDescriptor)
        {
            _basePropertyDescriptor = basePropertyDescriptor;
        }

        private void LoadLocalization()
        {
            string className = _basePropertyDescriptor.ComponentType.Name;
            
            // Map class names to short keys used in JSON (e.g. DialogueOptionJson -> Option)
            if (className == "L10nItem") className = "L10n";
            else if (className == "NpcDialogueJson") className = "NPC";
            else 
            {
                if (className.EndsWith("Json")) className = className.Substring(0, className.Length - 4);
                if (className.StartsWith("Dialogue")) className = className.Substring(8);
            }

            string propName = _basePropertyDescriptor.Name;
            string typeContext = null;

            // Specifically for Action/Condition Key & Value, we want to know the "Type" to give better help
            // E.g. Desc_Action_Pay_Value
            if ((className == "Action" || className == "Condition") && (propName == "key" || propName == "value"))
            {
                try
                {
                    // Access the "type" property of the instance being edited
                    // Note: We need the instance, but Descriptor is stateless per instance usually. 
                    // However, PropertyGrid calls GetValue(component).
                    // Wait, PropertyDescriptor methods like GetValue(component) work on component.
                    // But DisplayName/Description are properties of the Descriptor itself, not the component instance.
                    // THIS IS TRICKY. Standard PropertyGrid queries DisplayName/Description from the Descriptor ONCE or per select.
                    // We cannot return different Descriptions for different instances using the SAME PropertyDescriptor instance 
                    // if the PropertyDescriptor is shared (which it usually is via TypeDescriptor).
                    // BUT our LocalizedTypeDescriptorProvider creates a NEW LocalizedTypeDescriptor wrapper every GetTypeDescriptor call.
                    // AND LocalizedTypeDescriptor wraps the properties.
                    // The issue: The *same* LocalizedPropertyDescriptor instance might be used? 
                    // No, GetTypeDescriptor(objectType, instance) returns a new CustomTypeDescriptor.
                    // But usually PropertyDescriptorCollection is cached.
                    
                    // Actually, for dynamic description based on INSTANCE value, we likely need to check the instance
                    // inside the Description property getter? No, Description property takes no arguments.
                    // Just returns string.
                    
                    // HOWEVER, if we used ICustomTypeDescriptor.GetProperties(Attribute[]) we are returning a collection.
                    // If the provider returns a FRESH collection for that specific instance, we can bind the instance to the descriptor.
                    // But TypeDescriptionProvider.GetTypeDescriptor has `instance` argument!
                    // Let's modify LocalizedPropertyDescriptor to hold the reference to the instance if available?
                }
                catch {}
            }
            
            // Wait, we can't easily access the instance inside LoadLocalization unless we passed it down.
            // But we can update LocalizedTypeDescriptor to pass the instance to the properties if possible?
            // Standard PropertyDescriptors are often static/shared for the Type.
            // If we want Instance-specific description, we need Instance-specific PropertyDescriptors.
            
            // Fortunately, GetTypeDescriptor(Type, object instance) is called with the instance.
            // We can pass this instance into our LocalizedTypeDescriptor, and then into LocalizedPropertyDescriptor.
            
            var info = LocalizationManager.GetPropertyInfo(className, propName, _specificTypeContext);
            
            _localizedName = info.displayName;
            _localizedDescription = info.description;
            
            // Translate Category if possible
            string catKey = "Category" + _basePropertyDescriptor.Category;
            _localizedCategory = LocalizationManager.Get(catKey);
            if (_localizedCategory == catKey) _localizedCategory = _basePropertyDescriptor.Category; // fallback
        }
        
        // Add field to store context
        private string _specificTypeContext = null;
        
        public void SetInstanceContext(object instance)
        {
            if (instance == null) return;
            try 
            {
                // Reflection to get "type" property value string
                var typeProp = instance.GetType().GetProperty("type");
                if (typeProp != null)
                {
                    var val = typeProp.GetValue(instance)?.ToString();
                    if (!string.IsNullOrEmpty(val)) _specificTypeContext = val;
                }
            }
            catch {}
        }

        public override string DisplayName
        {
            get
            {
                LoadLocalization(); // Reload every time in case language changed
                return _localizedName;
            }
        }

        public override string Description
        {
            get
            {
                LoadLocalization();
                return _localizedDescription;
            }
        }

        public override string Category
        {
            get
            {
                LoadLocalization();
                return _localizedCategory;
            }
        }

        public override bool CanResetValue(object component) => _basePropertyDescriptor.CanResetValue(component);
        public override Type ComponentType => _basePropertyDescriptor.ComponentType;
        public override object GetValue(object component) => _basePropertyDescriptor.GetValue(component);
        public override bool IsReadOnly => _basePropertyDescriptor.IsReadOnly;
        public override Type PropertyType => _basePropertyDescriptor.PropertyType;
        public override void ResetValue(object component) => _basePropertyDescriptor.ResetValue(component);
        public override void SetValue(object component, object value) => _basePropertyDescriptor.SetValue(component, value);
        public override bool ShouldSerializeValue(object component) => _basePropertyDescriptor.ShouldSerializeValue(component);
        
        // Forward attributes, but maybe we could filter DescriptionAttribute?
        // Actually base(basePropertyDescriptor) copies attributes. 
        // We override Name/Description properties so attributes are less relevant for these fields, 
        // but PropertyGrid might inspect attributes directly if we are not careful?
        // PropertyGrid uses descriptor properties first usually.
    }
}
