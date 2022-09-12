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
    public VariableGroupList? Variables { get; set; }

    public VariableEditModel(VariableClient client)
    {
        this.client = client;
    }

    public async Task<IActionResult> OnGet()
    {
        if (string.IsNullOrWhiteSpace(this.client.organisationUrl))
        {
            return Redirect("/");
        }
        Variables = await client.GetVariableGroups();
        return Page();
    }

    public async Task<JsonResult> OnGetDownload()
    {
        if (string.IsNullOrWhiteSpace(this.client.organisationUrl))
        {
            return new JsonResult(null);
        }
        Variables = await client.GetVariableGroups();
        var settings = Variables.value.FirstOrDefault(v => v.name == this.GroupName);
        return new JsonResult(settings.variables.Where((k) => string.IsNullOrEmpty(Prefix) || k.Key.StartsWith(Prefix)).ToDictionary(k => k.Key, k => k.Value));
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

    public async Task<IActionResult> OnPostSaveAll([FromBody] SaveAllModel model)
    {
        var groups = await client.GetVariableGroups();
        var group = groups?.value?.FirstOrDefault(d => d.id == model.GroupId);

        if (group == null || group.variables == null) return new JsonResult(new { success = false });

        foreach(var keyValue in model.Variables)
        {
            if (group.variables.ContainsKey(keyValue.Key))
            {
                group.variables[keyValue.Key] = new VariableValue {  value = keyValue.Value };
            }
            else
            {
                group.variables.Add(keyValue.Key, new VariableValue { value = keyValue.Value });
            }
        }
        var result = await client.SaveAllVariables(group);

        return new JsonResult(new { success = result });
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

            result = await client.SaveAllVariables(group);

            if (result)
            {
                // ViewData["Saved"] = key;
            }

        }
        return RedirectToPage(new { groupname = GroupName, prefix = Prefix, Result = result });
    }

    public async Task<IActionResult> OnPostCreate(string groupName)
    {
        var result = true;

        result = await client.CreateVariableGroup(groupName);

        return RedirectToPage(new { groupname = groupName, Result = result });
    }
    public async Task<IActionResult> OnPostUpload()
    {
        var file = Request.Form.Files["Settings"];
        if (file != null)
        {
            var overWrite = Request.Form["Overwrite"] == "true";
            Variables = await client.GetVariableGroups();
            var groupname = Request.Form["groupname"];
            var group = Variables?.value?.FirstOrDefault(d => d.name == groupname);

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var data = reader.ReadToEnd();
                var dict = JsonSerializer.Deserialize<Dictionary<string, VariableValue>>(data);
                foreach (var kvp in dict)
                {
                    if (group.variables.ContainsKey(kvp.Key) && !overWrite) continue;
                    group.variables[kvp.Key] = kvp.Value;
                }
                await client.SaveAllVariables(group);
            }
            return RedirectToPage(new { groupname = groupname });
        }
        return RedirectToPage();
    }

    public class SaveAllModel
    {
        public int GroupId { get; set; }

        public Dictionary<string, string> Variables { get; set; }
    }

}


