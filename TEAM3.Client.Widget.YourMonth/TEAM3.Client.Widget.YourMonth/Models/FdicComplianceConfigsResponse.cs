using  Alkami.Client.Framework.Mvc;
namespace TEAM3.Client.Widget.YourMonth.Models
{
    public class FdicComplianceConfigsResponse : BaseModel
    {
        public Desktop Desktop { get; set; }

        public Mobile Mobile { get; set; }

        public Pages Pages { get; set; }
    }

    public class Desktop
    {
        public Header Header { get; set; }

        public Footer Footer { get; set; }

        public Login Login { get; set; }
    }

    public class Mobile
    {
        public Footer Footer { get; set; }

        public Login Login { get; set; }
    }

    public class Pages
    {
        public bool IsEnabled { get; set; }
    }

    public class Header
    {
        public bool IsEnabled { get; set; }
    }

    public class Footer
    {
        public bool IsEnabled { get; set; }
    }

    public class Login
    {
        public bool IsEnabled { get; set; }

        public string SignColor { get; set; }
    }
}
