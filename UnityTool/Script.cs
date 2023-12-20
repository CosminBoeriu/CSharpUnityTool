using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace UnityTool;

public class Script
{
    public string Id;
    public string PathToScript;
    public string PathToMeta;
    public string RelativePath;
    
    public Script(string fullPathToString, string pathToRootDirectory)
    {
        PathToScript = fullPathToString;
        PathToMeta = PathToScript + ".meta";
        RelativePath = PathToScript.Substring(pathToRootDirectory.Length);
        var x = File.ReadAllText(PathToMeta);  
        Id = (x.Split(new char[] {' ', '\n'}))[3];
    }

    public bool VerifySerialization()
    {
        string code = File.ReadAllText(PathToScript);
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        var treeRoot = syntaxTree.GetRoot();
        foreach (var node in treeRoot.DescendantNodes())
        {
            if (node.IsKind(SyntaxKind.Attribute))
            {
                if (node.ToString() == "SerializeField")
                {
                    return true;
                }
            }
        }
        return false;
    }
}