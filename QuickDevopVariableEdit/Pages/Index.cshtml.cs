using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuickDevopsVariableEdit.Pages
{
    public class IndexModel : PageModel
    {
        public VariableClient VariableClient { get; }

        public IndexModel(VariableClient keyVaultService)
        {
            VariableClient = keyVaultService;
        }

        public IActionResult OnPostGotoProject()
        {
            VariableClient.SetAccessToken(Request.Form["url"], Request.Form["accessToken"]);
            return RedirectToPage("SelectProject");

        }

    }
}
