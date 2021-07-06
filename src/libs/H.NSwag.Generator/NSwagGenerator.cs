﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using NSwag;
using NSwag.CodeGeneration.CSharp;

namespace H.NSwag.Generator
{
    [Generator]
    public class NSwagGenerator : ISourceGenerator
    {
        #region Methods

        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var text in context.AdditionalFiles
                .Where(static text => text.Path.EndsWith(
                    ".nswag", 
                    StringComparison.InvariantCultureIgnoreCase)))
            {
                try
                {
                    var source = Task.Run(() => GenerateAsync(text.Path, context.CancellationToken)).Result;

                    context.AddSource(
                        $"{Path.GetFileName(text.Path)}.cs", 
                        SourceText.From(source, Encoding.UTF8));
                }
                catch (Exception exception)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "NSG0001",
                                "Exception: ",
                                $"{exception}",
                                "Usage",
                                DiagnosticSeverity.Error,
                                true),
                            Location.None));
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public static async Task<string> GenerateAsync(
            string path, 
            CancellationToken cancellationToken = default)
        {
            path = path ?? throw new ArgumentNullException(nameof(path));

            var folder = Path.GetDirectoryName(path) ?? string.Empty;
            var json = File.ReadAllText(path);
            var document = 
                JsonConvert.DeserializeObject<NSwagDocument>(json) ??
                throw new InvalidOperationException("Document is null.");
            var settings = document.CodeGenerators.OpenApiToCSharpClient;
            var fromDocument = document.DocumentGenerator.FromDocument;

            var openApi = string.IsNullOrWhiteSpace(fromDocument.Url)
                ? fromDocument.Json.StartsWith("{", StringComparison.OrdinalIgnoreCase)
                    ? await OpenApiDocument.FromJsonAsync(fromDocument.Json, cancellationToken).ConfigureAwait(false)
                    : await OpenApiYamlDocument.FromYamlAsync(fromDocument.Json, cancellationToken).ConfigureAwait(false)
                : fromDocument.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? await OpenApiDocument.FromUrlAsync(fromDocument.Url, cancellationToken).ConfigureAwait(false)
                    : await OpenApiDocument.FromFileAsync(Path.Combine(folder, fromDocument.Url), cancellationToken).ConfigureAwait(false);
            var generator = new CSharpClientGenerator(openApi, new CSharpClientGeneratorSettings
            {
                ClassName = settings.ClassName,
                AdditionalContractNamespaceUsages = settings.AdditionalContractNamespaceUsages,
                AdditionalNamespaceUsages = settings.AdditionalNamespaceUsages,
                ChecksumCacheEnabled = settings.ChecksumCacheEnabled,
                ClientBaseClass = settings.ClientBaseClass,
                ClientBaseInterface = settings.ClientBaseInterface,
                ClientClassAccessModifier = settings.ClientClassAccessModifier,
                ConfigurationClass = settings.ConfigurationClass,
                DisposeHttpClient = settings.DisposeHttpClient,
                ExceptionClass = settings.ExceptionClass,
                ProtectedMethods = settings.ProtectedMethods,
                CSharpGeneratorSettings =
                {
                    Namespace = settings.Namespace,
                    GenerateNullableReferenceTypes = settings.GenerateNullableReferenceTypes,
                    GenerateOptionalPropertiesAsNullable = settings.GenerateOptionalPropertiesAsNullable,
                    GenerateDataAnnotations = settings.GenerateDataAnnotations,
                    GenerateDefaultValues = settings.GenerateDefaultValues,
                    GenerateImmutableArrayProperties = settings.GenerateImmutableArrayProperties,
                    GenerateImmutableDictionaryProperties = settings.GenerateImmutableDictionaryProperties,
                    GenerateJsonMethods = settings.GenerateJsonMethods,
                    EnforceFlagEnums = settings.EnforceFlagEnums,
                    ExcludedTypeNames = settings.ExcludedTypeNames,
                    DictionaryInstanceType = settings.DictionaryInstanceType,
                    DictionaryType = settings.DictionaryType,
                    HandleReferences = settings.HandleReferences,
                    InlineNamedAny = settings.InlineNamedAny,
                    InlineNamedArrays = settings.InlineNamedArrays,
                    InlineNamedDictionaries = settings.InlineNamedDictionaries,
                    InlineNamedTuples = settings.InlineNamedTuples,
                    RequiredPropertiesMustBeDefined = settings.RequiredPropertiesMustBeDefined,
                    TypeAccessModifier = settings.TypeAccessModifier,
                    TimeType = settings.TimeType,
                    TemplateDirectory = settings.TemplateDirectory,
                    TimeSpanType = settings.TimeSpanType,
                    JsonConverters = settings.JsonConverters,
                    AnyType = settings.AnyType,
                    ArrayBaseType = settings.ArrayBaseType,
                    ArrayInstanceType = settings.ArrayInstanceType,
                    ArrayType = settings.ArrayType,
                    ClassStyle = settings.ClassStyle,
                    DateTimeType = settings.DateTimeType,
                    DateType = settings.DateType,
                    DictionaryBaseType = settings.DictionaryBaseType,
                },
                CodeGeneratorSettings =
                {
                    ExcludedTypeNames = settings.ExcludedTypeNames,
                    InlineNamedAny = settings.InlineNamedAny,
                    GenerateDefaultValues = settings.GenerateDefaultValues,
                    TemplateDirectory = settings.TemplateDirectory,
                },
                UseBaseUrl = settings.UseBaseUrl,
                UseHttpClientCreationMethod = settings.UseHttpClientCreationMethod,
                UseHttpRequestMessageCreationMethod = settings.UseHttpRequestMessageCreationMethod,
                UseRequestAndResponseSerializationSettings = settings.UseRequestAndResponseSerializationSettings,
                WrapDtoExceptions = settings.WrapDtoExceptions,
                WrapResponseMethods = settings.WrapResponseMethods,
                WrapResponses = settings.WrapResponses,
                ParameterArrayType = settings.ParameterArrayType,
                ParameterDateFormat = settings.ParameterDateFormat,
                ParameterDateTimeFormat = settings.ParameterDateTimeFormat,
                ParameterDictionaryType = settings.ParameterDictionaryType,
                InjectHttpClient = settings.InjectHttpClient,
                QueryNullValue = settings.QueryNullValue,
                HttpClientType = settings.HttpClientType,
                ResponseArrayType = settings.ResponseArrayType,
                ResponseClass = settings.ResponseClass,
                ResponseDictionaryType = settings.ResponseDictionaryType,
                SerializeTypeInformation = settings.SerializeTypeInformation,
                ExposeJsonSerializerSettings = settings.ExposeJsonSerializerSettings,
                ExcludedParameterNames = settings.ExcludedParameterNames,
                GenerateOptionalParameters = settings.GenerateOptionalParameters,
                GenerateBaseUrlProperty = settings.GenerateBaseUrlProperty,
                GenerateClientClasses = settings.GenerateClientClasses,
                GenerateClientInterfaces = settings.GenerateClientInterfaces,
                GenerateDtoTypes = settings.GenerateDtoTypes,
                GenerateExceptionClasses = settings.GenerateExceptionClasses,
                GeneratePrepareRequestAndProcessResponseAsAsyncMethods = settings.GeneratePrepareRequestAndProcessResponseAsAsyncMethods,
                GenerateResponseClasses = settings.GenerateResponseClasses,
                GenerateSyncMethods = settings.GenerateSyncMethods,
                GenerateUpdateJsonSerializerSettingsMethod = settings.GenerateUpdateJsonSerializerSettingsMethod,
            });

            return generator.GenerateFile();
        }

        #endregion
    }
}