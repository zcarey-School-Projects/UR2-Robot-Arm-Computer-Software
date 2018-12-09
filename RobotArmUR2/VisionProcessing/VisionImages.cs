using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.VisionProcessing {

	/// <summary>Collection of images that are produced when the Vision class is detecting shapes. Used to pass these images along so user can select any image they want to view.</summary>
	public class VisionImages {

		/// <summary>Full pixel size, non-edited</summary>
		public Image<Bgr, byte> Raw { get; private set; } = null;

		/// <summary>Resized raw image for better memory usage</summary>
		public Image<Bgr, byte> Input { get; private set; } = null;

		/// <summary>Grayscaled InputImage</summary>
		public Image<Gray, byte> Grayscale { get; private set; } = null;

		/// <summary>Thresholded Greyscale Image to either a black/white, no gray</summary>
		public Image<Gray, byte> Threshold { get; private set; } = null;

		/// <summary>Warped image so the corners of the paper are in the corner of the image.</summary>
		public Image<Gray, byte> Warped { get; private set; } = null;

		/// <summary>Canny edges in image form detected from ThresholdImage</summary>
		public Image<Gray, byte> Canny { get; private set; } = null;

		/// <summary>Same as Warped, but with detected shapes drawn on top.</summary>
		public Image<Bgr, byte> WarpedWithShapes { get; private set; } = null;

		/// <summary>The shapes nd their positions that were detected.</summary>
		public DetectedShapes Shapes { get; private set; } = null;
		//TODO this is dumb, dont pass everything through the constructor
		/// <summary>Creates a new object with all the parameters stored.</summary>
		public VisionImages(Image<Bgr, byte> raw, Image<Bgr, byte> input, Image<Gray, byte> grayscale, Image<Gray, byte> threshold, Image<Gray, byte> warped, Image<Gray, byte> canny, Image<Bgr, byte> warpedWithShapes, DetectedShapes shapes) {
			this.Raw = raw;
			this.Input = input;
			this.Grayscale = grayscale;
			this.Threshold = threshold;
			this.Warped = warped;
			this.Canny = canny;
			this.WarpedWithShapes = warpedWithShapes;
			this.Shapes = shapes;
		}

		/// <summary>Given an image type, returns the corresponding image.</summary>
		/// <param name="type">The type of image to be returned.</param>
		/// <returns></returns>
		public Image<Bgr, byte> GetImage(VisionImage type) {
			switch (type) {
				case VisionImage.Raw: return Raw;
				case VisionImage.Input: return Input;
				case VisionImage.Grayscale: return (Grayscale == null) ? null : Grayscale.Convert<Bgr, byte>();
				case VisionImage.Threshold: return (Threshold == null) ? null : Threshold.Convert<Bgr, byte>();
				case VisionImage.Warped: return (Warped == null) ? null : Warped.Convert<Bgr, byte>();
				case VisionImage.Canny: return (Canny == null) ? null : Canny.Convert<Bgr, byte>();
				case VisionImage.Shapes: return WarpedWithShapes;
			}

			return null;
		}

	}

	/// <summary>Enum list of all the images stored in the object. Used to select an image progmatically.</summary>
	public enum VisionImage {
		None,
		Raw, 
		Input, 
		Grayscale,
		Threshold,
		Warped,
		Canny,
		Shapes
	}
}
