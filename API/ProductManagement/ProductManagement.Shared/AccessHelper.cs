using System.Numerics;

using ProductManagement.Shared.Enums;
namespace ProductManagement.Shared;

public static class AccessHelper
{
    public static bool HasAccess(Roles role, params MenuItems[] menuItems)
    {
        return menuItems.Any(menuItem => HasAccess(role, menuItem));
    }

    private static bool HasAccess(Roles role, MenuItems menuItem)
    {
        return menuItem switch
        {
            MenuItems.Menu => new Roles[] { Roles.Admin}.Contains(role),
            

            MenuItems.Users => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.Financials => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.Dashboard => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.ReferenceData => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.Financial => new Roles[] { Roles.Admin }.Contains(role),
           
            MenuItems.LoginLogs => new Roles[] { Roles.Admin }.Contains(role),
           
            MenuItems.Audits => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.CompanyInfo => new Roles[] { Roles.Admin }.Contains(role),

            MenuItems.Gallery => new Roles[] { Roles.Admin }.Contains(role),

            MenuItems.FrequentlyQuestions => new Roles[] { Roles.Admin }.Contains(role),
            
            MenuItems.Payments => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.Notifications => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.CompetitiveAdvantages => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.ContactUsMessages => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.OurTeams => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.Services => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.Testimonials => new Roles[] { Roles.Admin }.Contains(role),
            MenuItems.SMSHistory => new Roles[] { Roles.Admin }.Contains(role),
            _ => false,
        };
    }
}
