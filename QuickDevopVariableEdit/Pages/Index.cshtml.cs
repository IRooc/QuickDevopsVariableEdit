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

      public IActionResult OnPostGotoEdit()
      {
         VariableClient.SetAccessToken(Request.Form["url"], Request.Form["projectName"], Request.Form["accessToken"]);
         return RedirectToPage("VariableEdit");
      }
   }
}
