using Microsoft.AspNetCore.Mvc;
using Chapeau.Models;

namespace Chapeau.Components.EmployeeCreateForm;

public class EmployeeCreateFormViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        IEnumerable<EmployeeRole> roles,
        string draftName,
        int draftRoleId)
    {
        var model = new EmployeeCreateFormModel
        {
            Roles = roles ?? [],
            DraftName = draftName ?? "",
            DraftRoleId = draftRoleId
        };
        return View(model);
    }
}

public class EmployeeCreateFormModel
{
    public IEnumerable<EmployeeRole> Roles { get; set; } = [];
    public string DraftName { get; set; } = "";
    public int DraftRoleId { get; set; }
}
