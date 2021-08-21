using System;
using System.IO;
using System.Threading.Tasks;

namespace ClientCodeGen.TemplateEngine
{
    public abstract class Generator
    {
        /// <summary>Include another template.</summary>
        /// <param name="model">The model to use.</param>
        /// <typeparam name="T">The type of the template.</typeparam>
        public void Include<T>(object? model = null)
            where T : Generator
        {
            T generator = this._generatorFactory.GetGenerator<T>();
            generator.ModelObject = model;
            if (model != null)
            {
                generator.CheckModelType();
            }

            generator.Generate(this._writer).Wait();
        }

        protected object? ModelObject;

        protected abstract void CheckModelType();

        private TextWriter? _writer;
        private GeneratorFactory _generatorFactory = null!;
        private string _templatePath = null!;

        internal void Initialise(GeneratorFactory generatorFactory, string templatePath)
        {
            this._generatorFactory = generatorFactory;
            this._templatePath = templatePath;
            this.OnCreate();
        }

        public async Task Generate(TextWriter? writer)
        {
            try
            {
                this._writer = writer;
                this.OnStart();
                await this.ExecuteAsync();
            }
            finally
            {
                this.OnFinish();
                this._writer = null;
            }
        }

        /// <summary>Called by the compiled razor template to start the code generation.</summary>
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        // ReSharper disable once MemberCanBeProtected.Global
        public virtual async Task ExecuteAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>Called by the compiled razor template to write literal text.</summary>
        // ReSharper disable once UnusedMember.Global
        protected void WriteLiteral(string text)
        {
            this._writer?.Write(text);
        }

        /// <summary>Called by the compiled razor template to write an object.</summary>
        // ReSharper disable once UnusedMember.Global
        protected void Write(object obj)
        {
            this._writer?.Write(obj);
        }

        /// <summary>Called when the generator has been created.</summary>
        protected virtual void OnCreate()
        {
        }

        /// <summary>Called before the code is generated.</summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>Called after the code is generated.</summary>
        protected virtual void OnFinish()
        {
        }
    }

    public class Generator<TModel> : Generator
    {
        protected TModel Model
        {
            get => (TModel)this.ModelObject!;
            set => this.ModelObject = value;
        }

        public async Task Generate(TModel model, TextWriter writer)
        {
            this.Model = model;
            await this.Generate(writer);
        }

        protected override void CheckModelType()
        {
            if (!(this.ModelObject is TModel))
            {
                throw new InvalidCastException("Model is the wrong type for this class");
            }
        }
    }
}