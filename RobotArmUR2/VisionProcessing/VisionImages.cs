using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.VisionProcessing {
	public class VisionImages {

		/// <summary>
		/// Full pixel size, non-edited
		/// </summary>
		public Image<Bgr, byte> Raw { get; private set; } = null;

		/// <summary>
		/// Resized raw image for better memory usage
		/// </summary>
		public Image<Bgr, byte> Input { get; private set; } = null;

		/// <summary>
		/// Grayscaled InputImage
		/// </summary>
		public Image<Gray, byte> Grayscale { get; private set; } = null;

		/// <summary>
		/// Thresholded Greyscale Image to either a black/white, no gray
		/// </summary>
		public Image<Gray, byte> Threshold { get; private set; } = null;

		/// <summary>
		/// Warped image so the corners of the paper are in the corner of the image.
		/// </summary>
		public Image<Gray, byte> Warped { get; private set; } = null;

		/// <summary>
		/// Canny edges in image form detected from ThresholdImage
		/// </summary>
		public Image<Gray, byte> Canny { get; private set; } = null;

		/// <summary>
		/// Same as Warped, but with detected shapes drawn on top.
		/// </summary>
		public Image<Bgr, byte> WarpedWithShapes { get; private set; } = null;

		public DetectedShapes Shapes { get; private set; } = null;

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

	}
}
