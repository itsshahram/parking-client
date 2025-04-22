

public static class CameraConfigManager
{
    private static List<CameraConfigModel> _cameras = new List<CameraConfigModel>();
    private static string _filePath = "cameras.json";

    static CameraConfigManager()
    {
        LoadFromFile();
    }

    private static void LoadFromFile()
    {
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _cameras = JsonConvert.DeserializeObject<List<CameraConfigModel>>(json) ?? new List<CameraConfigModel>();
        }
    }

    private static void SaveToFile()
    {
        var json = JsonConvert.SerializeObject(_cameras, Formatting.Indented); // استفاده از `Formatting.Indented`
        File.WriteAllText(_filePath, json);
    }

    public static List<CameraConfigModel> GetAllCameras() => _cameras;
    public static List<CameraConfigModel> GetActiveCameras() => _cameras.Where(c=>c.IsActive ==true).ToList();
    public static void AddCamera(CameraConfigModel camera)
    {
        _cameras.Add(camera);
        SaveToFile();
    }

    public static bool EditCamera(string name, CameraConfigModel updatedCamera)
    {
        var camera = _cameras.FirstOrDefault(c => c.Name == name);
        if (camera == null) return false;

        // بروزرسانی اطلاعات دوربین
        camera.Name = updatedCamera.Name;
        camera.Description = updatedCamera.Description;
        camera.RTSPUrl = updatedCamera.RTSPUrl;
        camera.SnapshotUrl = updatedCamera.SnapshotUrl;
        camera.Username = updatedCamera.Username;
        camera.Password = updatedCamera.Password;
        camera.IsActive = updatedCamera.IsActive;

        SaveToFile();
        return true;
    }

    public static bool RemoveCamera(string name)
    {
        var camera = _cameras.FirstOrDefault(c => c.Name == name);
        if (camera == null) return false;

        _cameras.Remove(camera);
        SaveToFile();
        return true;
    }
}