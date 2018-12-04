using System;
using System.Configuration;
using System.Reflection;

namespace RobotArmUR2.Util {
	public class Setting<T> where T : struct {

		private PropertyInfo property;

		public Setting(string SettingName) {
			property = parsePropertyFromName(SettingName);
		}

		public T? Load() {
			try {
				if (property == null) throw new ArgumentNullException("Property was null, can't read.");
				if (!property.CanRead) throw new ArgumentOutOfRangeException("Could not read property.");
				object value = property.GetValue(Properties.Settings.Default, null);
				if (value == null) throw new NullReferenceException("Property does not have a value.");
				if (!(value is T)) throw new TypeAccessException("Value is of wrong type.");
				return (T)value;
			} catch (Exception e) {
				Console.WriteLine("ERROR reading property: " + e.Message);
				return null;
			}
		}

		public bool Save(T value) {
			try {
				if (property == null) throw new ArgumentNullException("Property was null.");
				if (!property.CanWrite) throw new ArgumentOutOfRangeException("Unable to write to property.");
				property.SetValue(Properties.Settings.Default, value);
				return true;
			}catch(Exception e) {
				Console.WriteLine("ERROR: Couldn't write to property: " + e.Message);
				return false;
			}
		}

		public string GetDefaultValue() {
			try {
				if (property == null) throw new ArgumentNullException("Property was null.");
				SettingsPropertyValue prop = Properties.Settings.Default.PropertyValues[property.Name];
				if (prop == null) throw new NullReferenceException("Could not find property: '" + property.Name + "'.");
				object data = prop.Property.DefaultValue;
				if (data == null) throw new NullReferenceException("Property '" + property.Name + "' did not have a value.");
				if (!(data is string)) throw new InvalidCastException("Property '" + property.Name + "' was not of type 'string'.");
				
				return (string)data;
			}catch(Exception e) {
				Console.Error.WriteLine("Could not parse default property value: " + e.Message);
				return null;
			}
		}

		private static PropertyInfo parsePropertyFromName(string name) {
			Type type = Properties.Settings.Default.GetType();
			if(type == null) {
				Console.WriteLine("ERROR: Could not find property type with name '" + name + "'.");
				return null;
			}
			PropertyInfo property = type.GetProperty(name);
			if (property == null) Console.WriteLine("ERROR: Could not find property with name '" + name + "'.");

			return property;
		}
	}
}
