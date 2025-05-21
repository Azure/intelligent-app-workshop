using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        // Let's find all constructors
        Console.WriteLine("AzureAIAgent constructors:");
        foreach (var ctor in typeof(AzureAIAgent).GetConstructors())
        {
            Console.WriteLine($"  {ctor}");
        }
        
        Console.WriteLine("\nAzureAIAgentInvokeOptions properties:");
        foreach (var prop in typeof(AzureAIAgentInvokeOptions).GetProperties())
        {
            Console.WriteLine($"  {prop.Name}: {prop.PropertyType}");
        }
    }
}
