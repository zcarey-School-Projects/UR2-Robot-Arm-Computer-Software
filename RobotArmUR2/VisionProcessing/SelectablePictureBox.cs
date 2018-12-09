using System;
using System.Windows.Forms;
using RobotArmUR2.Util;
using System.Collections.Generic;

namespace RobotArmUR2.VisionProcessing {

	/// <summary> Allows the user to use a right-click menu to select which image is being viewed. Extends EmguPictureBox </summary>
	public class SelectablePictureBox : EmguPictureBox {

		private ContextMenuStrip menu; //The menu stored for use later.
		private Dictionary<VisionImage, ToolStripMenuItem> listItems = new Dictionary<VisionImage, ToolStripMenuItem>(); //For each VisionImage, the corresponding generating list item

		/// <summary> The image that is selected for viewing. </summary>
		public VisionImage SelectedImage {
			get => selectedImage;
			set {
				selectedImage = value;
				if (images != null) base.Image = images.GetImage(value);
				checkType(value);
			}
		}
		private VisionImage selectedImage;

		/// <summary>The collection of images to choose from for input.</summary>
		public VisionImages Images { get => images;
			set {
				images = value;
				if (value == null) base.Image = null;
				else base.Image = value.GetImage(SelectedImage);
			}
		}
		private VisionImages images = null;

		/// <summary>Wraps the standard PictureBox and generates a right-click menu for the user to use.</summary>
		/// <param name="form">The form that hold the picturebox, for threading purposes.</param>
		/// <param name="picture">The picturebox to wrap.</param>
		/// <param name="DefaultSelection">The default image that is selected.</param>
		public SelectablePictureBox(Form form, PictureBox picture, VisionImage DefaultSelection) : base(form, picture) {
			menu = new ContextMenuStrip();

			//For every entry in the VisionImage enum, generate a menu entry
			foreach(VisionImage type in Enum.GetValues(typeof(VisionImage))) {
				ToolStripMenuItem item = new ToolStripMenuItem(type.ToString()); //Creates an entry with the name of the enum value
				listItems.Add(type, item); //Store it in the dictionary so we can find it easily later, corresponding with the type

				//When clicked, select the corresponding image type and check this item while unchecking the others.
				item.Click += (object sender, EventArgs e) => {
					selectedImage = type;
					clearChecks();
					item.Checked = true;
				};

				//Add item to the menu
				menu.Items.Add(item);
			}

			picture.ContextMenuStrip = menu; //Add menu to the picture box so it actually functions
			SelectedImage = DefaultSelection; //Set default selection, which should also check the box.
		}

		/// <summary>Clears all check marks from the list.</summary>
		private void clearChecks() {
			foreach(ToolStripMenuItem entry in listItems.Values) {
				entry.Checked = false;
			}
		}

		/// <summary>Checks the menu item that corresponds with the image type.</summary>
		/// <param name="type"></param>
		private void checkType(VisionImage type) {
			clearChecks();

			ToolStripMenuItem value;
			if(listItems.TryGetValue(type, out value)) {
				value.Checked = true;
			}
		}

	}
}
