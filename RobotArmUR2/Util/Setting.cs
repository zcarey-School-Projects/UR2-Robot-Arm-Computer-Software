using System;
using System.Configuration;
using System.Reflection;

namespace RobotArmUR2.Util {

	/// <summary>Assists in using application settings to save/load data in persistent storage.</summary>
	/// <typeparam name="T">The type of data being stored.</typeparam>
	public class Setting<T> where T : struct {

		/// <summary>The stored property info.</summary>
		private PropertyInfo property;

		/// <summary>Initializes the class by finding a property with the given name.</summary>
		/// <param name="SettingName"></param>
		public Setting(string SettingName) {
			property = parsePropertyFromName(SettingName);
		}

		/// <summary>Attempts to read the data from storage and parse it into the correct data type.</summary>
		/// <returns>Null if any errors occure.</returns>
		public T? Read() {
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

		/// <summary>Attempts to write the given data to the setting in persistent storage.
		/// THIS DOES NOT MEAN IT IS SAVED. You MUST call Properties.Setting.Default.Save() or similar function to dump data into storage.</summary>
		/// <param name="value">Value to be written.</param>
		/// <returns></returns>
		public bool Set(T value) {
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

		/// <summary>Attempts to read the default value of the setting, however this value is a string so it can not be parsed automatically.</summary>
		/// <returns></returns>
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

		/// <summary>Tries to find a property with the given name in the application settings.</summary>
		/// <param name="name"></param>
		/// <returns></returns>
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
