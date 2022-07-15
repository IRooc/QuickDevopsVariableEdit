using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuickDevopsVariableEdit.Pages
{
    public class SelectxProjectModel : PageModel
    {
        public VariableClient VariableClient { get; }

        public SelectxProjectModel(VariableClient keyVaultService)
        {
            VariableClient = keyVaultService;
        }

        public async Task<IEnumerable<DevOpsReference>> GetProjects()
        {
            var devOpsReferenceList = await VariableClient.GetProjectList();
            if (devOpsReferenceList?.value == null) return Enumerable.Empty<DevOpsReference>();
            return devOpsReferenceList.value.OrderBy(p => p.name);
        }

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(VariableClient.organisationUrl))
            {
                return Redirect("/");
            }
            return Page();
        }

        public IActionResult OnPostGotoEdit()
        {
            VariableClient.SetProject(Request.Form["projectName"]);
            return RedirectToPage("VariableEdit");
        }
    }
}
