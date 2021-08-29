using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClientCodeGen.TemplateEngine;

namespace ClientCodeGen
{
    using cs = ClientCodeGen.Templates.cs;
    using bash = ClientCodeGen.Templates.bash;

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
            //await this.GenerateCS();
            await this.GenerateBash();
        }

        private async Task GenerateCS()
        {
            GeneratorFactory generatorFactory = new GeneratorFactory(
                Path.Combine(Program.GetSourceRootDir(), "Templates/cs"), typeof(cs.RequestOptionsTemplate).Namespace);

            Templates.AllRequestsTemplate generator = generatorFactory.GetGenerator<cs.RequestOptionsTemplate>();

            string outputDir = Path.Combine(Program.GetSourceRootDir(), "../GuestClient/Generated");
            Directory.CreateDirectory(outputDir);

            await using TextWriter writer = new StreamWriter(Path.Combine(outputDir, "Requests.cs"));
            await generator.Generate(writer);

        }

        private async Task GenerateBash()
        {
            GeneratorFactory generatorFactory = new GeneratorFactory(
                Path.Combine(Program.GetSourceRootDir(), "Templates/bash"), typeof(bash.GuestScriptTemplate).Namespace);

            Templates.AllRequestsTemplate generator = generatorFactory.GetGenerator<bash.GuestScriptTemplate>();

            string outputDir = Path.Combine(Program.GetSourceRootDir(), "../HostServer/Scripts");
            Directory.CreateDirectory(outputDir);

            await using TextWriter writer = new StreamWriter(Path.Combine(outputDir, "hagi-guest.sh"));
            await generator.Generate(writer);

        }
    }
}