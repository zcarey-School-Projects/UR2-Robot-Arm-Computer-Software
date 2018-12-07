using System;
using System.Windows.Forms;
using RobotArmUR2.Util;
using System.Collections.Generic;

namespace RobotArmUR2.VisionProcessing {
	public class SelectablePictureBox : EmguPictureBox {

		private ContextMenuStrip menu;
		private Dictionary<VisionImage, ToolStripMenuItem> listItems = new Dictionary<VisionImage, ToolStripMenuItem>();

		private VisionImage selectedImage;
		public VisionImage SelectedImage {
			get => selectedImage;
			set {
				selectedImage = value;
				if (images != null) base.Image = images.GetImage(value);
				checkType(value);
			}
		}

		private VisionImages images = null;
		public VisionImages Images { get => images;
			set {
				images = value;
				if (value == null) base.Image = null;
				else base.Image = value.GetImage(SelectedImage);
			}
		}

		public SelectablePictureBox(Form form, PictureBox picture, VisionImage DefaultSelection) : base(form, picture) {
			menu = new ContextMenuStrip();
			foreach(VisionImage type in Enum.GetValues(typeof(VisionImage))) {
				ToolStripMenuItem item = new ToolStripMenuItem(type.ToString());
				listItems.Add(type, item);

				item.Click += (object sender, EventArgs e) => {
					selectedImage = type;
					clearChecks();
					item.Checked = true;
				};

				menu.Items.Add(item);
			}

			picture.ContextMenuStrip = menu;
			SelectedImage = DefaultSelection;
		}

		private void clearChecks() {
			foreach(ToolStripMenuItem entry in listItems.Values) {
				entry.Checked = false;
			}
		}

		private void checkType(VisionImage type) {
			clearChecks();

			ToolStripMenuItem value;
			if(listItems.TryGetValue(type, out value)) {
				value.Checked = true;
			}
		}

	}
}
