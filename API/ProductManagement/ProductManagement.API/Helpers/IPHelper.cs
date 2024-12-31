using Microsoft.Extensions.Primitives;

using System.Net;

namespace ProductManagement.API.Helpers;

public static class IPHelper
{
    public static string? GetIPAddress(HttpContext context)
    {
        var ip = GetGlobalIPAddress(context);
        if (string.IsNullOrEmpty(ip) || ip == "::1")
            ip = GetLocalIPAddress();

        return ip;
    }

    private static string? GetGlobalIPAddress(HttpContext context)
    {
        var request = context.Request;
        if (request.Headers.TryGetValue("CF-CONNECTING-IP", out var cfConnectingIp) && !StringValues.IsNullOrEmpty(cfConnectingIp))
            return cfConnectingIp;

        var ipAddress = request.Headers["HTTP_X_FORWARDED_FOR"];
        if (!StringValues.IsNullOrEmpty(ipAddress))
        {
            var addresses = ipAddress.ToString().Split(',');
            if (addresses.Length != 0)
                return addresses[0];
        }

        return request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static string? GetLocalIPAddress()
    {
        var ipAddresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        foreach (var ipAddress in ipAddresses)
            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                return ipAddress.ToString();

        return null;
    }
}
