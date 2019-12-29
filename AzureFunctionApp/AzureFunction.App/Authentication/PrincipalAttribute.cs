using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzureFunction.App.Authentication
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class PrincipalAttribute : Attribute
    {
    }
}