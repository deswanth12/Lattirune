namespace Lattirune.Save
{
    public enum SaveStatus
    {
        Success,
        NoSave,
        Corrupt,
        Invalid,
        UnsupportedVersion,
        RecoveredFromBackup,
        Failed
    }

    public class SaveResult
    {
        public SaveStatus Status { get; private set; }
        public string Message { get; private set; }
        public bool IsSuccess => Status == SaveStatus.Success;

        public SaveResult(SaveStatus status, string message = "")
        {
            Status = status;
            Message = message;
        }

        public static SaveResult Success() => new SaveResult(SaveStatus.Success, "Save written successfully.");
        public static SaveResult Failed(string message) => new SaveResult(SaveStatus.Failed, message);
    }

    public class LoadResult
    {
        public SaveStatus Status { get; private set; }
        public string Message { get; private set; }
        public SaveData Data { get; private set; }
        public bool IsSuccess => Status == SaveStatus.Success || Status == SaveStatus.RecoveredFromBackup;

        public LoadResult(SaveStatus status, SaveData data, string message = "")
        {
            Status = status;
            Data = data;
            Message = message;
        }

        public static LoadResult Success(SaveData data) => new LoadResult(SaveStatus.Success, data, "Save loaded successfully.");
        public static LoadResult RecoveredFromBackup(SaveData data) => new LoadResult(SaveStatus.RecoveredFromBackup, data, "Main save corrupt; successfully recovered from backup.");
        public static LoadResult NoSave(SaveData defaultData) => new LoadResult(SaveStatus.NoSave, defaultData, "No save file found; created default profile.");
        public static LoadResult Corrupt(SaveData defaultData, string message) => new LoadResult(SaveStatus.Corrupt, defaultData, message);
        public static LoadResult Failed(string message) => new LoadResult(SaveStatus.Failed, null, message);

        public static implicit operator SaveData(LoadResult result) => result?.Data;
    }
}
