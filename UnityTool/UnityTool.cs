namespace UnityTool;

static class UnityTool
{
    public static HashSet<string> UsedScripts = new HashSet<string>();
    static string[] ParseString(string str)
    {
        string[] retVal;
        var aux = str.Split("&", 2);
        retVal = aux[1].Split("\n", 2);
        return retVal;
    }

    static List<string> GetSceneRoots(Object obj)
    {
        var roots = (List<object>)(obj.Fields["SceneRoots"])["m_Roots"];
        var result = new List<string>();
        foreach (var myobj in roots)
        {
            result.Add(((Dictionary<object, object>)myobj).Values.First().ToString());
        }

        return result;
    }

    static void MarkUsedScript(string scriptId)
    {
        UsedScripts.Add(scriptId);
    }
    
    static void ComputeUnityFileHierarchy(string pathToUnityFile, string outputDirectory)
    {
        var unityFileName = Path.GetFileName(pathToUnityFile);
        Dictionary<string, Dictionary<string, Object>> objects = ParseUnityFile(pathToUnityFile);
        var sceneRoots = GetSceneRoots(objects["SceneRoots"].Values.First());
        foreach (var key in objects["Transform"].Keys)
        {
            foreach (GameObject gameObject in objects["GameObject"].Values)
            {
                if(gameObject.Components.Contains(objects["Transform"][key].Id))
                {
                    ((Transform)objects["Transform"][key]).AttachedGameObject = gameObject.Id;
                }
            }
        }

        Stack<(Transform, int)> resultStack = new Stack<(Transform, int)>();
        sceneRoots.Reverse();
        sceneRoots.ForEach(item => resultStack.Push(((Transform)objects["Transform"][item], 0)));
        
        string filePath = Path.Combine(outputDirectory, unityFileName) + ".dump";
        StreamWriter sw = File.CreateText(filePath);
        while (resultStack.Count != 0)
        {
            (Transform t, int level) = resultStack.Peek();
            resultStack.Pop();
            string name = ((GameObject)objects["GameObject"][t.AttachedGameObject]).Name;
            t.Children.Reverse();
            sw.WriteLine(new string('-', 2 * level) + name);
            t.Children.ForEach(item => resultStack.Push(( (Transform)objects["Transform"][item], level + 1)));
        }
        sw.Close();

        foreach (Behaviour behaviour in objects["MonoBehaviour"].Values)
        {
            MarkUsedScript(behaviour.Script);
        }
        
    }
    static Dictionary<string, Dictionary<string, Object>> ParseUnityFile(string pathToUnityFile)
    {
        Dictionary<string, Dictionary<string, Object>> result = new Dictionary<string, Dictionary<string, Object>>();
        result.Add("GameObject", new Dictionary<string, Object>());
        result.Add("Transform", new Dictionary<string, Object>());
        result.Add("MonoBehaviour", new Dictionary<string, Object>());
        string unityFileString = (File.ReadAllText(pathToUnityFile));
        string input = unityFileString.Split('\n', 3)[2];
        string[] gameObjs = input.Split("--- !u!");
        foreach (var str in gameObjs.Skip(1))
        {
            var argString = ParseString(str);
            Object myObj = new Object(argString);
            switch (myObj.Type)
            {
                case "GameObject":
                    var myGameObj = new GameObject(argString);
                    result[myObj.Type].Add(myGameObj.Id, myGameObj);
                    break;
                case "Transform":
                    var myTransform = new Transform(argString);
                    result[myObj.Type].Add(myTransform.Id, myTransform);
                    break;
                case "MonoBehaviour":
                    var myBehaviour = new Behaviour(argString);
                    result[myObj.Type].Add(myBehaviour.Id, myBehaviour);
                    break;
                default:
                    if(!result.ContainsKey(myObj.Type))
                        result.Add(myObj.Type, new Dictionary<string, Object>());
                    result[myObj.Type].Add(myObj.Id, myObj);
                    break;
            }
        }
        return result;
    }

    static string ComputeUnusedScripts(Script script)
    {
        if (!script.VerifySerialization() && !UsedScripts.Contains(script.Id)) 
        {
            return $"{script.RelativePath},{script.Id}\n";
        }
        return "";
    }

    static void Main(string[] args)
    {
        var csFiles = Directory.EnumerateFiles(args[0], "*.cs", SearchOption.AllDirectories);
        var unityFiles = Directory.EnumerateFiles(args[0], "*.unity", SearchOption.AllDirectories);
        var threadList = new List<Thread>(); 
        foreach (var unityFile in unityFiles)
        {
            threadList.Add(new Thread(() => ComputeUnityFileHierarchy(unityFile, args[1])));
            threadList.Last().Start();
        }

        foreach (var thread in threadList)
        {
            thread.Join();
        }
        
        string scriptOutputFile = Path.Combine(args[1], "UnusedScripts.csv");
        StreamWriter sw = File.CreateText(scriptOutputFile);
        sw.Write("Relative Path,GUId\n");
        foreach (var scriptPath in csFiles)
        {
            Script s = new Script(scriptPath, args[0]);
            sw.Write(ComputeUnusedScripts(s));
        }
        sw.Close();
    }
}