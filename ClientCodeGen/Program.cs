using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClientCodeGen.TemplateEngine;
using ClientCodeGen.Templates;

namespace ClientCodeGen
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Generate().Wait();
        }

        public static string GetSourceRootDir()
        {
            return Program.GetCallerDir();
        }

        private static string GetCallerDir([CallerFilePath] string? path = null)
        {
            return Path.GetDirectoryName(path)!;
        }

        public async Task Generate()
        {
            GeneratorFactory generatorFactory = new GeneratorFactory(
                Path.Combine(Program.GetSourceRootDir(), "Templates/cs"), typeof(AllRequestsTemplate).Namespace);

            AllRequestsTemplate generator = generatorFactory.GetGenerator<AllRequestsTemplate>();

            string outputDir = Path.Combine(Program.GetSourceRootDir(), "../GuestClient/Generated");
            Directory.CreateDirectory(outputDir);


            await using TextWriter writer = new StreamWriter(Path.Combine(outputDir, "Requests.cs"));
            await generator.Generate(writer);
        }
    }
}