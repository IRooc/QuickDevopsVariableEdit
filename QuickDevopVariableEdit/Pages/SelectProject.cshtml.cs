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
            return (await VariableClient.GetProjectList()).value.OrderBy(p => p.name);
        }

        public IActionResult OnPostGotoEdit()
        {
            VariableClient.SetProject(Request.Form["projectName"]);
            return RedirectToPage("VariableEdit");
        }
    }
}
