using System.Data;

namespace AkumsPrintingApplication.PL
{
    public class UserMasterModel
    {
        public int Id { get; set; }

        public string ProductVersion { get; set; }
        public string SiteCode { get; set; }
        public string GroupID { get; set; }
        public string UserName { get; set; }
        public string UserID { get; set; }
        public string UserType { get; set; }
        public string Password { get; set; }
        public string Active { get; set; }
        public string CreatedBy { get; set; }
        public string Result { get; set; }
    }
    public class GroupRigthsModel
    {
        public string GroupID { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public DataTable dtSaveRightsList { get; set; }
    }
    public class GroupMasterModel
    {
        public string GroupID { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string GroupType { get; set; } = string.Empty;
    }
}