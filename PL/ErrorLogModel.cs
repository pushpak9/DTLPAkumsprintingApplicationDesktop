namespace AkumsPrintingApplication.PL
{
    public class ErrorLogModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public string LineCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string DeviceIP { get; set; } = string.Empty;

        public string BatchNo { get; set; } = string.Empty;
        public string LogType { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;

    }
}
