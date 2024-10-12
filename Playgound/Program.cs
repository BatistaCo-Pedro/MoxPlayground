// See https://aka.ms/new-console-template for more information

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;

Console.WriteLine("Hello, World!");

public record SomeDto(string Name, int Age);

public class SomeEntity
{
    public string Name { get; set; }
    public int Age { get; set; }
}

[Map]
public partial class Mapper
{
    public partial SomeDto ToDto(SomeEntity entity);
}

[AttributeUsage(AttributeTargets.Class)]
public class MapAttribute : Attribute
{
    public MapAttribute() { }
}