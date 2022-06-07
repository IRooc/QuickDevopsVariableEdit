using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json.Nodes;

namespace QuickDevopsVariableEdit.Pages
{
   public class PullRequestsModel : PageModel
   {

      public readonly VariableClient client;

      public List<PullRequestReference> prs = new List<PullRequestReference>();

      public PullRequestsModel(VariableClient client)
      {
         this.client = client;
      }

      public async Task<IActionResult> OnGet()
      {
         if (string.IsNullOrEmpty(client.organisationUrl))
         {
            return Redirect("/");
         }

         var projects = await this.client.GetProjectList();
         foreach (var project in projects?.value ?? new DevOpsReference[]{})
         {
            if (string.IsNullOrEmpty(project?.name)) continue;
            var repositories = await this.client.GetRepositoryList(project.name);
            if (repositories?.value == null) continue;
            foreach (var repo in repositories.value)
            {
               var prlist = await this.client.GetPullRequestList(repo.id!);
               if (prlist.count > 0)
                  prs.AddRange(prlist.value!);
            }
         }
         return Page();
      }
   }
}
