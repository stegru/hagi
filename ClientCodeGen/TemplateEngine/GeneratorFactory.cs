using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace ClientCodeGen.TemplateEngine
{
    /// <summary>
    /// Generates code generators, which are compiled razor templates.
    /// </summary>
    public class GeneratorFactory
    {
        private readonly string _templatePath;
        private readonly string? _namespaceName;

        private readonly RazorProjectFileSystem _fileSystem;
        private readonly RazorProjectEngine _engine;

        private readonly Dictionary<string, string> _classNames = new Dictionary<string, string>();
        private readonly Dictionary<Type, Generator> _generators = new Dictionary<Type, Generator>();

        public GeneratorFactory(string templatePath, string? namespaceName = null)
        {
            this._templatePath = templatePath;
            this._namespaceName = namespaceName ?? this.GetType().Namespace;
            this._fileSystem = RazorProjectFileSystem.Create(this._templatePath);

            this._engine = RazorProjectEngine.Create(RazorConfiguration.Default, this._fileSystem, builder =>
            {
                if (namespaceName != null)
                {
                    builder.SetNamespace(namespaceName);
                    builder.ConfigureClass((document, node) =>
                    {
                        if (this._classNames.TryGetValue(document.Source.FilePath, out string? className))
                        {
                            node.ClassName = className;
                        }
                    });
                }
            });
        }

        /// <summary>
        /// Get the code generator (compiled razor template) of the given type.
        /// </summary>
        /// <typeparam name="T">Type of code generator.</typeparam>
        /// <returns>The generator.</returns>
        public T GetGenerator<T>()
            where T : Generator
        {
            if (!this._generators.TryGetValue(typeof(T), out Generator? generator))
            {
                generator = this.CreateGenerator<T>();
                this._generators[typeof(T)] = generator;
            }

            return (T)generator;
        }

        /// <summary>Create the generator.</summary>
        private T CreateGenerator<T>()
            where T : Generator
        {
            string filename = typeof(T).GetCustomAttribute<TemplateFileAttribute>()?.Filename
                              ?? throw new ApplicationException(
                                  $"Type {typeof(T).Name} does not have a template file");

            string templatePath = Path.Combine(this._templatePath, filename);

            // Generate the C# code for the razor template.
            RazorProjectItem item = this._fileSystem.GetItem(templatePath, FileKinds.Legacy);

            string className = typeof(T).Name + "_generator";
            this._classNames[item.PhysicalPath] = className;

            string csCode = this._engine.Process(item).GetCSharpDocument().GeneratedCode;
            Console.WriteLine(csCode);

            // Add some required references.
            List<MetadataReference> references = new List<MetadataReference>()
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(
                    Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "netstandard.dll")),
                MetadataReference.CreateFromFile(typeof(RazorCompiledItemAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Program).Assembly.Location),
            };

            // Also add references referred to by this assembly.
            foreach (AssemblyName referencedAssembly in Assembly.GetCallingAssembly().GetReferencedAssemblies())
            {
                references.Add(MetadataReference.CreateFromFile(Assembly.Load(referencedAssembly.FullName).Location));
            }

            // Compile the generated template code.
            const string assemblyName = "template";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(csCode);
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[] { tree }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using MemoryStream assemblyStream = new MemoryStream();
            EmitResult emitResult = compilation.Emit(assemblyStream);

            if (!emitResult.Success)
            {
                emitResult.Diagnostics.ToList().ForEach(Console.WriteLine);
                throw new ApplicationException();
            }

            // Load the compiled assembly.
            Assembly assembly = Assembly.Load(assemblyStream.ToArray());

            string typeName = $"{this._namespaceName}.{className}";

            Type type = assembly.GetType(typeName)
                        ?? throw new ApplicationException($"Unable to load generated type '{typeName}'");

            T generator = (T?)Activator.CreateInstance(type)
                          ?? throw new ApplicationException($"Unable to instantiate generated type '{typeName}'");

            generator.Initialise(this, templatePath);

            return generator;
        }
    }
}