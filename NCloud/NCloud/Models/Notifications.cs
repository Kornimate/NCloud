﻿namespace NCloud.Models
{
    public class Error : ACloudNotification
    {
        public Error(string? text) : base("bg-danger", @"/utilities/error.svg", "Error", 1, text)
        {
        }
    }

    public class Warning : ACloudNotification
    {
        public Warning(string? text) : base("bg-warning", @"/utilities/warning.svg", "Warning", 2, text)
        {
        }
    }

    public class Success : ACloudNotification
    {
        public Success(string? text) : base("bg-success", @"/utilities/success.svg", "Success", 3, text)
        {
        }
    }

    public class Information : ACloudNotification
    {
        public Information(string? text) : base("bg-info", @"/utilities/info.svg", "Information", 4, text)
        {
        }
    }
}