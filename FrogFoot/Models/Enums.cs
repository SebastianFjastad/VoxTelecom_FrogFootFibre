using System.ComponentModel.DataAnnotations;

namespace FrogFoot.Models
{
    public enum UserType
    {
        Admin = 1,
        ISPUser = 2,
        Client = 3,
        FFUser = 4,
        FFManager = 5,
        Comms = 6
    }

    public enum OrderStatus
    {
        New = 1,
        Pending = 2,
        Ordered = 3,
        Accepted = 4,
        Canceled = 5
    }

    public enum ProductType
    {
        LineSpeed = 1,
        Option = 2,
        Quantity = 3
    }

    public enum LineSpeed
    {
        [Display(Name = "10 Mbps")]
        TenMbps = 1,
        [Display(Name = "20 Mbps")]
        TwentyMbps = 2,
        [Display(Name = "50 Mbps")]
        FiftyMbps = 3,
        [Display(Name = "100 Mbps")]
        HundredMbps = 4,
        [Display(Name = "1 Gbps")]
        OneGps = 5
    }

    public enum Discount
    {
        Zero = 0,
        Fifty = 50,
        Hundred = 100
    }

    public enum PostType
    {
        Update = 1,
        Article = 2,
        Post = 3
    }

    public enum PostGridding
    {
        Zone = 1,
        Precinct = 2,
        Clients = 3,
        Public = 4
    }

    public enum TrenchingStatus
    {
        [Display(Name = "Unscheduled")]
        Undefined = 1,
        [Display(Name = "Scheduled")]
        HasDates = 2,
        [Display(Name = "Construction")]
        WorkInProgress = 3,
        [Display(Name = "Completed")]
        Completed = 4
    }

    public enum UserAction
    {
        Create = 1, 
        Edit = 2, 
        Cancel = 4,
        Message = 5
    }

    public enum ContractTerm
    {
        MonthToMonth = 1,
        Month24 = 2
    }

    public enum SpecialType
    {
        [Display(Name = "Early Bird")]
        EarlyBird = 1,
        [Display(Name = "Just In Time")]
        JustInTime = 2
    }

    public enum UserFilterType
    {
        UserTable = 1,
        UserMap = 2,
        UserLeads
    }
}