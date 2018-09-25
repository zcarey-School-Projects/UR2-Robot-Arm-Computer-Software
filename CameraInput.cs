
public class CameraInput : InputHandler
{

	private VideoCapture captureDevice;
	private Image<Bgr, byte> buffer;
	private int width = 0;
	private int height = 0;
	private bool frameAvailable = false;

	public CameraInput()
	{
	}
}
