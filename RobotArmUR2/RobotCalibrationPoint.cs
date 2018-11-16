using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotArmUR2 {
	public class RobotCalibrationPoint : RobotPoint {

		private PropertyInfo rotationProperty;
		private PropertyInfo extensionProperty;

		public RobotCalibrationPoint(string RotationName, string ExtensionName){
			rotationProperty = parsePropertyFromName(RotationName);
			extensionProperty = parsePropertyFromName(ExtensionName);
			float? rot = parsePropertyValue(rotationProperty);
			float? ext = parsePropertyValue(extensionProperty);
			if (rot == null || ext == null) {
				MessageBox.Show("Could not retrieve saved data: " + RotationName + " & " + ExtensionName);
			} else {
				Rotation = (float)rot;
				Extension = (float)ext;
			}
		}

		public bool ResetToDefault() {
			float? rot = parseDefaultValue(rotationProperty);
			float? ext = parseDefaultValue(extensionProperty);
			if (rot == null || ext == null) {
				MessageBox.Show("Unable to retieve default value.");
				return false;
			} else {
				Rotation = (float)rot;
				Extension = (float)ext;
				return true;
			}
		}

		public bool Save() {
			bool savedRot = setPropertyValue(rotationProperty, Rotation);
			bool savedExt = setPropertyValue(extensionProperty, Extension);
			if (!savedRot || !savedExt) {
				MessageBox.Show("Error saving property.");
				return false;
			} else {
				return true;
			}
		}

		private static PropertyInfo parsePropertyFromName(string name) {
			Type type = Properties.Settings.Default.GetType();
			if (type == null) {
				Console.WriteLine("ERROR: Could not determine property type: '" + name + "'.");
				return null;
			}
			PropertyInfo property = type.GetProperty(name);
			if (property == null) Console.WriteLine("ERROR: Could not find property: '" + name + "'.");

			return property;
		}

		private static float? parsePropertyValue(PropertyInfo property) {
			try {
				if (property == null) throw new ArgumentNullException("Property was null.");
				if(!property.CanRead) throw new ArgumentOutOfRangeException("Could not read property.");
				object value = property.GetValue(Properties.Settings.Default, null);
				if (value == null) throw new NullReferenceException("Property does not have a value.");

				return (float)value;
			}catch(Exception ex) {
				Console.WriteLine("ERROR reading property: " + ex.Message);

				return null;
			}
		}

		private static bool setPropertyValue(PropertyInfo property, object value) {
			try {
				if (property == null) throw new ArgumentNullException("Property was null.");
				if (!property.CanWrite) throw new ArgumentOutOfRangeException("Could not write property.");
				property.SetValue(Properties.Settings.Default, value);

				return true;
			} catch (Exception ex) {
				Console.WriteLine("ERROR writing property: " + ex.Message);

				return false;
			}
		}

		private static float? parseDefaultValue(PropertyInfo prop) {
			try {
				if (prop == null) throw new ArgumentNullException("Property was null.");
				SettingsPropertyValue property = Properties.Settings.Default.PropertyValues[prop.Name];
				if (property == null) throw new NullReferenceException("Could not find property '" + prop.Name + "'.");
				object data = property.Property.DefaultValue;
				if (data == null) throw new NullReferenceException("Property '" + prop.Name + "' did not have a value.");
				if (!(data is string)) throw new InvalidCastException("Property '" + prop.Name + "' was not of type 'string'.");
				return float.Parse((string)data); 
			} catch (Exception ex) {
				Console.WriteLine("ERROR parsing property value: " + ex.Message);

				return null;
			}
		}

	}

}
