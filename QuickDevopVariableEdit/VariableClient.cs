


using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace QuickDevopsVariableEdit;
public class VariableClient
{
    private HttpClient _client = new HttpClient();
    public string? organisationUrl;
    public string? projectName;
    public string? token;
    public VariableClient()
    {

    }

    public void SetAccessToken(string organisationUrl, string token)
    {
        this.organisationUrl = organisationUrl;
        this.token = token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
           Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", token))));
    }
    public void SetProject(string project)
    {
        this.projectName = project;
    }

    public async Task<DevOpsReferenceList> GetProjectList()
    {
        var response = await _client.GetAsync($"{organisationUrl}/_apis/projects?api-version=7.1-preview.4");
        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(body))
            {
                return JsonConvert.DeserializeObject<DevOpsReferenceList>(body) ?? throw new ApplicationException($"Invalid body returned {body}");
            }
        }
        throw new ApplicationException($"Invalid status code returned {response.StatusCode}");
    }

    public async Task<DevOpsReferenceList> GetRepositoryList(string projectName)
    {
        //GET https://dev.azure.com/{organization}/{project}/_apis/sourceProviders/{providerName}/repositories?api-version=6.0-preview.1
        var response = await _client.GetAsync($"{organisationUrl}/{projectName}/_apis/git/repositories?api-version=6.0");
        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(body))
            {
                return JsonConvert.DeserializeObject<DevOpsReferenceList>(body) ?? throw new ApplicationException($"Invalid body returned {body}");
            }
        }
        throw new ApplicationException($"Invalid status code returned {response.StatusCode}");
    }
    public async Task<PullRequestReferenceList> GetPullRequestList(string repositoryId)
    {
        //GET https://dev.azure.com/{organization}/{project}/_apis/git/repositories/{repositoryId}/pullrequests?api-version=7.1-preview.1
        var response = await _client.GetAsync($"{organisationUrl}/_apis/git/repositories/{repositoryId}/pullrequests?api-version=6.0");
        var debugbody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(body))
            {
                return JsonConvert.DeserializeObject<PullRequestReferenceList>(body) ?? throw new ApplicationException($"Invalid body returned {body}");
            }
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new PullRequestReferenceList
            {
                value = new PullRequestReference[] { }
            };
        }
        throw new ApplicationException($"Invalid status code returned {response.StatusCode}");
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

    public async Task<bool> SaveAllVariables(VariableGroup groupInit)
    {
        var groupResponse = await _client.GetAsync($"{organisationUrl}/{projectName}/_apis/distributedtask/variablegroups/{groupInit.id}?api-version=7.1-preview.2");
        var groupBody = await groupResponse.Content.ReadAsStringAsync();
        var group = JsonConvert.DeserializeObject<VariableGroup>(groupBody);
        group.variables = groupInit.variables;
        var response = await _client.PutAsJsonAsync($"{organisationUrl}/_apis/distributedtask/variablegroups/{group.id}?api-version=7.1-preview.2", group);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"BODY:{body}");
            return false;
        }
        return true;
    }

    public async Task<bool> CreateVariableGroup(StringValues groupName)
    {
        var projects = await GetProjectList();
        var project = projects.value.FirstOrDefault(p => p.name == this.projectName);

        var group = new VariableGroup();
        group.name = groupName;
        group.type = "Vsts";
        group.variables = new Dictionary<string, VariableValue> {
            {"remove-me-please", new VariableValue {
                value ="dummy value"
            }
            }
        };
        group.variableGroupProjectReferences = new VariableGroupProjectReference[]
        {
            new VariableGroupProjectReference
            {
                name = groupName,
                description = groupName,
                projectReference = new Projectreference
                {
                    id = project.id,
                    name = project.name
                }
            }
        };

        var response = await _client.PostAsJsonAsync($"{organisationUrl}/_apis/distributedtask/variablegroups?api-version=7.1-preview.2", group);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"BODY:{body}");
            return false;
        }
        return true;
    }
}

public class DevOpsReferenceList
{
    public int count { get; set; }
    public DevOpsReference[]? value { get; set; }

}

public class DevOpsReference
{
    public string? id { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
    public string? url { get; set; }
    public string? defaultTeamImageUrl { get; set; }
    public DevOpsReference? project { get; set; }
}

public class PullRequestReferenceList
{
    public int count { get; set; }
    public PullRequestReference[]? value { get; set; }

}

public class PullRequestReference
{
    public string? title { get; set; }
    public int pullRequestid { get; set; }
    public string? url { get; set; }
    public string? status { get; set; }
    public DevOpsUser? createdBy { get; set; }
    public string? mergeStatus { get; set; }
    public DevOpsReference? repository { get; set; }
}
public class DevOpsUser
{
    public string? id { get; set; }
    public string? displayName { get; set; }
}

public class VariableGroupList
{
    public int count { get; set; }
    public VariableGroup[]? value { get; set; }
}

public class VariableGroup
{
    public Dictionary<string, VariableValue>? variables { get; set; }
    public int id { get; set; }
    public string? type { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
    public VariableGroupProjectReference[]? variableGroupProjectReferences { get; set; }
}
public class VariableGroupProjectReference
{
    public string? description { get; set; }
    public string? name { get; set; }
    public Projectreference? projectReference { get; set; }
}

public class Projectreference
{
    public string? id { get; set; }
    public string? name { get; set; }
}
public class VariableValue
{
    public string? value { get; set; }
}

public class Modifiedby
{
    public string? displayName { get; set; }
    public string? id { get; set; }
    public string? uniqueName { get; set; }
}
