using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace UnityTool;

class Object
{
    public string Id{get; set;} // this is the yaml anchor - the file Id
    public Dictionary<string, Dictionary<string, object>> Fields;
    public string Type;
    public Object(string[] argString)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        Id = argString[0];
        Fields = deserializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(argString[1]);
        Type = new List<string>(Fields.Keys)[0];
    }
}

class GameObject : Object
{
    public List<string> Components;
    public string? Name;
    public GameObject(string[] argString) : base(argString)
    {
        Components = SetComponents();
        if (Fields.ContainsKey(Type))
        {
            if (Fields[Type].ContainsKey("m_Name"))
            {
                Name = Fields[Type]["m_Name"].ToString();
            }
        }
        
    }

    public List<string> SetComponents()
    {
        var complist = (List<object>)Fields[Type]["m_Component"];
        List<string> result = new List<string>();
        foreach (var obj in complist)
        {
            var dict = (Dictionary<object, object>)obj;
            foreach (var obj2 in dict.Values)
            {
                var dict2 = (Dictionary<object, object>)obj2;
                foreach (var val in dict2.Values)
                {
                    string? temp = val.ToString();
                    result.Add(temp);
                }
            }
        }
        return result;
    }
}

class Transform : Object
{
    public List<string> Children;
    public string? AttachedGameObject = "0";
    public Transform(string[] argString) : base(argString)
    {
        Children = SetChildren();
    }

    public List<string> SetChildren()
    {
        var complist = (List<object>)Fields[Type]["m_Children"];
        List<string> result = new List<string>();
        foreach (var obj in complist)
        {
            var dict = (Dictionary<object, object>)obj;
            foreach (var obj2 in dict.Values)
            {
                    string? temp = obj2.ToString();
                    result.Add(temp);
            }
        }
        return result;
    }
}

class Behaviour : Object
{
    public string Script;
    public Behaviour(string[] argString) : base(argString)
    {
        Script = set_script();
    } 
    public string set_script()
    {
        var complist = (Dictionary<object, object>)Fields[Type]["m_Script"];
        return complist["guid"].ToString();
    }
}