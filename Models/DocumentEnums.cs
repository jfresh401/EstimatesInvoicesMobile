namespace FreshEstimate.Mobile.Models;

public enum DocumentType
{
    Estimate = 0,
    Invoice = 1
}

public enum DocumentStatus
{
    Draft = 0,
    Sent = 1,
    Viewed = 2,
    Approved = 3,
    Paid = 4,
    Overdue = 5,
    Cancelled = 6
}
