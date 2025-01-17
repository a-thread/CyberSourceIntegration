﻿using CyberSourceIntegration.Models;

namespace CyberSourceIntegration.Services
{
    public interface ICyberSourceService
    {
        Task<string> CreatePaymentSessionAsync();
        Task<string> ProcessPaymentAsync(string transientToken);
    }
}
