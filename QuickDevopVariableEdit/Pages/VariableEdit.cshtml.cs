using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuickDevopsVariableEdit.Pages;

public class VariableEditModel : PageModel
{
   [BindProperty(SupportsGet = true)]
   public string? GroupName { get; set; }


   [BindProperty(SupportsGet = true)]
   public string? Prefix { get; set; }



   public readonly VariableClient client;

   public VariableEditModel(VariableClient client)
   {
      this.client = client;
   }

   public VariableGroupList? Variables { get; set; }

   public async Task<IActionResult> OnGet()
   {
      if (string.IsNullOrWhiteSpace(this.client.organisationUrl))
      {
         return Redirect("/");
      }
      Variables = await client.GetVariableGroups();
      return Page();
   }


   public async Task<IActionResult> OnPostSave()
   {
      Variables = await client.GetVariableGroups();
      var groupId = Request.Form["groupid"];
      var key = Request.Form["Key"];
      var val = Request.Form["Value"];
      var gudub = int.Parse(groupId);
      var group = Variables?.value?.FirstOrDefault(d => d.id == gudub);
      var result = true;
      if (group != null)
      {
         result = await client.SaveVariable(groupId, key, val);
         if (result)
         {
            ViewData["Saved"] = key;
         }

      }
      return RedirectToPage(new { groupname = GroupName, prefix = Prefix, Result = result, Key = key });
   }

   public async Task<IActionResult> OnPostDelete()
   {
      Variables = await client.GetVariableGroups();
      var groupId = Request.Form["groupid"];
      var key = Request.Form["Key"];
      var gudub = int.Parse(groupId);
      var group = Variables?.value?.FirstOrDefault(d => d.id == gudub);
      var result = true;
      if (group != null)
      {
         result = await client.DeleteVariable(groupId, key);
         if (result)
         {
            ViewData["Saved"] = key;
         }

      }
      return RedirectToPage(new { groupname = GroupName, prefix = Prefix, Result = result });
   }


   public async Task<IActionResult> OnPostCopy(string prefixToCopy)
   {
      Variables = await client.GetVariableGroups();
      var groupId = Request.Form["groupid"];
      var gudub = int.Parse(groupId);
      var group = Variables?.value?.FirstOrDefault(d => d.id == gudub);
      var result = true;
      if (group != null)
      {
         var copyVars = group.variables?.Where(p => p.Key.StartsWith(prefixToCopy)).ToList() ?? new List<KeyValuePair<string, VariableValue>>();
         foreach (var cp in copyVars)
         {
            var key = cp.Key.Replace(prefixToCopy, Prefix);
            if (!group.variables!.ContainsKey(key))
            {
               group.variables.Add(key, group.variables[cp.Key]);
            }
         }

         result = await client.SaveAllVariables(groupId, group.variables!);

         if (result)
         {
            // ViewData["Saved"] = key;
         }

      }
      return RedirectToPage(new { groupname = GroupName, prefix = Prefix, Result = result });
   }



}


