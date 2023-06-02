using System;
using System.Collections.Generic;

public class FloatTree
{
    public string Name { get; set; }
    public float Value { get; set; }
    public List<FloatTree> Children { get; set; }
    public FloatTree Parent { get; set; }

    public FloatTree()
    {
        Children = new List<FloatTree>();
    }

    public FloatTree(string name)
    {
        Name = name;
        Children = new List<FloatTree>();
    }

    public FloatTree(float value)
    {
        Value = value;
        Children = new List<FloatTree>();
    }

    public FloatTree(string name, float value)
    {
        Name = name;
        Value = value;
        Children = new List<FloatTree>();
    }

    public void AddChild(FloatTree child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public FloatTree FindNode(string nodeName)
    {
        if (Name == nodeName)
        {
            return this;
        }

        foreach (var child in Children)
        {
            var result = child.FindNode(nodeName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    public void Print(int indentLevel = 0)
    {
        Console.WriteLine(new string(' ', indentLevel * 4) + $"{Name}: {Value}");

        foreach (var child in Children)
        {
            child.Print(indentLevel + 1);
        }
    }
}
