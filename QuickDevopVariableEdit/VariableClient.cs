


using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace QuickDevopsVariableEdit;
public class VariableClient
{
   private HttpClient _client = new HttpClient();
   public string organisationUrl;
   public string projectName;
   public string token;
   public VariableClient()
   {

   }

   public void SetAccessToken(string organisationUrl, string projectName, string token)
   {
      this.organisationUrl = organisationUrl;
      this.projectName = projectName;
      this.token = token;
      _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
         Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", token))));
   }


   public async Task<VariableGroupList> GetVariableGroups()
   {
      var response = await _client.GetAsync($"{organisationUrl}/{projectName}/_apis/distributedtask/variablegroups?api-version=7.1-preview.2");

      if (response.IsSuccessStatusCode)
      {
         var body = await response.Content.ReadAsStringAsync();
         if (!string.IsNullOrWhiteSpace(body))
         {
            return JsonConvert.DeserializeObject<VariableGroupList>(body) ?? throw new ApplicationException($"Invalid body returned {body}");
         }
      }
      throw new ApplicationException($"Invalid status code returned {response.StatusCode}");
   }

   public async Task<bool> SaveVariable(StringValues groupId, string key, string val)
   {
      var groupResponse = await _client.GetAsync($"{organisationUrl}/{projectName}/_apis/distributedtask/variablegroups/{groupId}?api-version=7.1-preview.2");
      var groupBody = await groupResponse.Content.ReadAsStringAsync();
      var group = JsonConvert.DeserializeObject<VariableGroup>(groupBody);
      if (group.variables.ContainsKey(key))
      {
         group.variables[key] = new VariableValue { value = val };
      }
      else
      {
         group.variables.Add(key, new VariableValue { value = val });
      }
      var response = await _client.PutAsJsonAsync($"{organisationUrl}/_apis/distributedtask/variablegroups/{groupId}?api-version=7.1-preview.2", group);
      if (!response.IsSuccessStatusCode)
      {
         var body = await response.Content.ReadAsStringAsync();
         Console.WriteLine($"BODY:{body}");
         return false;
      }
      return true;
   }

   internal async Task<bool> DeleteVariable(StringValues groupId, StringValues key)
   {
      var groupResponse = await _client.GetAsync($"{organisationUrl}/{projectName}/_apis/distributedtask/variablegroups/{groupId}?api-version=7.1-preview.2");
      var groupBody = await groupResponse.Content.ReadAsStringAsync();
      var group = JsonConvert.DeserializeObject<VariableGroup>(groupBody);
      if (group.variables.ContainsKey(key))
      {
         group.variables.Remove(key);
         var response = await _client.PutAsJsonAsync($"{organisationUrl}/_apis/distributedtask/variablegroups/{groupId}?api-version=7.1-preview.2", group);
         if (!response.IsSuccessStatusCode)
         {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"BODY:{body}");
            return false;
         }
      }
      return true;
   }

   public async Task<bool> SaveAllVariables(StringValues groupId, Dictionary<string, VariableValue> variables)
   {
      var groupResponse = await _client.GetAsync($"{organisationUrl}/{projectName}/_apis/distributedtask/variablegroups/{groupId}?api-version=7.1-preview.2");
      var groupBody = await groupResponse.Content.ReadAsStringAsync();
      var group = JsonConvert.DeserializeObject<VariableGroup>(groupBody);
      group.variables = variables;
      var response = await _client.PutAsJsonAsync($"{organisationUrl}/_apis/distributedtask/variablegroups/{groupId}?api-version=7.1-preview.2", group);
         if (!response.IsSuccessStatusCode)
         {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"BODY:{body}");
            return false;
         }
         return true;
   }
}



public class VariableGroupList
{
   public int count { get; set; }
   public VariableGroup[] value { get; set; }
}

public class VariableGroup
{
   public Dictionary<string, VariableValue> variables { get; set; }
   public int id { get; set; }
   public string type { get; set; }
   public string name { get; set; }
   public string description { get; set; }
   public VariableGroupProjectReference[] variableGroupProjectReferences { get; set; }
}
public class VariableGroupProjectReference
{
   public string description { get; set; }
   public string name { get; set; }
   public Projectreference projectReference { get; set; }
}

public class Projectreference
{
   public string id { get; set; }
   public string name { get; set; }
}
public class VariableValue
{
   public string value { get; set; }
}

public class Modifiedby
{
   public string displayName { get; set; }
   public string id { get; set; }
   public string uniqueName { get; set; }
}
