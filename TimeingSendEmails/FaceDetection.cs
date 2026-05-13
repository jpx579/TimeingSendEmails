using OpenCvSharp;

namespace TimeingSendEmails
{
    public class FaceDetection
    {
        private readonly string _faceDataPath;
        private readonly string _saveDirPath;

        public FaceDetection()
        {
            _faceDataPath = Path.Combine(Application.StartupPath, "Config", "haarcascade_frontalface_default.xml");
            _saveDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceImages");

            if (!Directory.Exists(_saveDirPath))
            {
                Directory.CreateDirectory(_saveDirPath);
            }
        }

        public (bool faceDetected, string filePath) DetectFace()
        {
            if (!File.Exists(_faceDataPath))
            {
                Logger.Error($"级联分类器文件丢失: {_faceDataPath}");
                return (false, "人脸识别文件丢失");
            }

            try
            {
                using (var capture = new VideoCapture(0))
                {
                    if (!capture.IsOpened())
                    {
                        Logger.Error("摄像头启动失败：可能被其他程序占用或未连接。");
                        return (false, "电脑摄像头启动失败");
                    }

                    System.Threading.Thread.Sleep(200);

                    using (var frame = new Mat())
                    using (var faceCascade = new CascadeClassifier(_faceDataPath))
                    {
                        capture.Read(frame);
                        if (frame.Empty())
                        {
                            Logger.Error("未能从摄像头捕获到有效画面。");
                            return (false, "未能从摄像头捕获到有效画面");
                        }

                        var faces = faceCascade.DetectMultiScale(frame);
                        bool isDetected = faces.Length > 0;

                        string fileName = $"Face_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                        string filePath = Path.Combine(_saveDirPath, fileName);

                        frame.SaveImage(filePath);

                        Logger.Info($"人脸检测完成。结果: {(isDetected ? "发现人脸" : "未发现人脸")}, 图片已保存至: {filePath}");

                        return (isDetected, filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("执行人脸检测时发生异常", ex);
                return (false, $"执行人脸检测时发生异常: {ex.Message}");
            }
        }
    }
}