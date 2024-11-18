using OpenCvSharp;

namespace TimeingSendEmails
{
    public class FaceDetection
    {
        string logPath = "Log.txt";
        // 检测人脸方法
        public (bool, string) DetectFace()
        {
            try
            {
                using (var capture = new VideoCapture(0))
                {
                    if (!capture.IsOpened())
                    {
                        File.AppendAllText("相机被别的程序占用", logPath);
                        return (false, null); // 摄像头未打开，可能被其他程序占用
                    }
                    string appPath = Application.StartupPath;
                    string path = Path.Combine(appPath, "Config\\haarcascade_frontalface_default.xml");

                    var faceCascade = new CascadeClassifier(path); // 加载人脸检测的级联分类器
                    Mat frame = new Mat(); // 创建一个 Mat 对象，用于存储图像帧
                    capture.Read(frame); // 从摄像头读取一帧图像

                    var faces = faceCascade.DetectMultiScale(frame); // 检测人脸

                    string filePath = $"D:\\jiapengxu\\TimeingSendEmailsApp\\FaceImages\\Faceimage.jpg"; // 保存的图片文件路径
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    filePath = Path.Combine(baseDirectory, "Faceimage.jpg");
                    frame.SaveImage(filePath); // 保存图像
                    frame.Dispose();
                    faceCascade.Dispose();
                    return (faces.Length > 0, filePath); // 返回检测结果和保存的文件路径
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText($"获取人脸失败：{ex}", logPath);
                return (false, null);
            }
        }
    }
}
