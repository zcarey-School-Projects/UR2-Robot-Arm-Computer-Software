using System;
using System.Diagnostics;

public abstract class InputHandler{

	private Stopwatch timer;

	public InputHandler(){
	}

	~InputHandler() {
		Dispose();
	}

	protected abstract void Dispose();

	///<summary>
	///<para>Reads the raw bytes of the last loaded frame.</para>
	///</summary>
	///<returns>Bytes in [y,x, channel] format.</returns>
	public abstract byte[,,] readRawData();

	///<summary>
	///<para>Reads the next frame of the current input, if available.</para>
	///<para>Note, that if a camera, this method blocks until the camera returns a new frame, which may differ between cameras.</para>
	///<para>If a video for image file is loaded, the method will block to match 30fps.</para>
	///</summary>
	///<returns>The next frame, or null if no frame was available.</returns>
	public abstract Image<Bgr, byte> readFrame();

	///<summary>
	/// <returns>If there is a frame available to be read. 
	/// <para>For images always returns true.</para>
	/// <para>Videos return false at the end of the stream.</para>
	/// <para>Cameras return true if they are open.</para>
	/// </returns>
	/// </summary>
	public abstract bool isFrameAvailable();
	public abstract int getWidth();
	public abstract int getHeight();
}
