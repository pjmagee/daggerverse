package main

import (
	"context"
	"dagger/kiota/internal/dagger"
	"fmt"
)

type Kiota struct {
	// The supported languages
	Languages []Language
	// The log level to be used in all Kiota commands
	LogLevel LogLevel
	// The Kiota mcr.microsoft.com/openapi/kiota container
	Container *dagger.Container
	// Common parameters to be used in all Kiota commands
	Parameters []string
}

// New Kiota module
func New(
	// +optional
	ConsoleColorsEnabled *bool,
	// +optional
	ConsoleColorsSwap *bool,
	// +optional
	TutorialEnabled *bool,
	// +optional
	OfflineEnabled *bool,
	// +optional
	// +default="information"
	LogLevel *LogLevel,
) *Kiota {

	container := dag.Container().
		From("mcr.microsoft.com/openapi/kiota").
		WithUser("root").
		WithMountedCache("tmp/kiota/cache", dag.CacheVolume("kiota-cache")).
		WithDirectory("/app/output", dag.Directory())

	if ConsoleColorsEnabled != nil {
		container = container.WithEnvVariable("KIOTA_CONSOLE_COLORS_ENABLED", fmt.Sprintf("%t", *ConsoleColorsEnabled))
	}

	if ConsoleColorsSwap != nil {
		container = container.WithEnvVariable("KIOTA_CONSOLE_COLORS_SWAP", fmt.Sprintf("%t", *ConsoleColorsSwap))
	}

	if TutorialEnabled != nil {
		container = container.WithEnvVariable("KIOTA_TUTORIAL_ENABLED", fmt.Sprintf("%t", *TutorialEnabled))
	}

	if OfflineEnabled != nil {
		container = container.WithEnvVariable("KIOTA_OFFLINE_ENABLED", fmt.Sprintf("%t", *OfflineEnabled))
	}

	kiota := &Kiota{
		Languages:  []Language{Csharp, Go, Java, Php, Python, Ruby, Shell, Swift, TypeScript},
		LogLevel:   *LogLevel,
		Parameters: []string{"--log-level", string(*LogLevel)},
		Container:  container,
	}

	return kiota
}

type TypeAccessModifier string

var (
	Public    TypeAccessModifier = "public"
	Internal  TypeAccessModifier = "internal"
	Protected TypeAccessModifier = "protected"
)

type Language string

// https://learn.microsoft.com/en-us/openapi/kiota/using#accepted-values-8
var (
	Csharp     Language = "csharp"
	Go         Language = "go"
	Java       Language = "java"
	Php        Language = "php"
	Python     Language = "python"
	Ruby       Language = "ruby"
	Shell      Language = "shell"
	Swift      Language = "swift"
	TypeScript Language = "typescript"
)

type LogLevel string

var (
	LogCritical    LogLevel = "critical"
	LogDebug       LogLevel = "debug"
	LogError       LogLevel = "error"
	LogInformation LogLevel = "information"
	LogNone        LogLevel = "none"
	LogTrace       LogLevel = "trace"
	LogWarning     LogLevel = "warning"
)

func (m *Kiota) WithParameters(
	// +optional
	clearCache *bool,
	// +optional
	excludePath *string,
	// +optional
	includePath *string,
	// +optional
	version *string,
	// +optional
	searchKey *string,
	// +optional
	disableSSLValidation *bool,
	// +optional
	openApi *string,
	// +optional
	outputPath *string,
	// +optional
	cleanOutput *bool,
	// +optional
	language *Language,

) *Kiota {

	if clearCache != nil && *clearCache {
		m.Parameters = append(m.Parameters, "--clear-cache")
	}

	if excludePath != nil {
		m.Parameters = append(m.Parameters, "--exclude-path")
		m.Parameters = append(m.Parameters, *excludePath)
	}

	if includePath != nil {
		m.Parameters = append(m.Parameters, "--include-path")
		m.Parameters = append(m.Parameters, *includePath)
	}

	if cleanOutput != nil {
		m.Parameters = append(m.Parameters, "--clean-output")
	}

	if version != nil {
		m.Parameters = append(m.Parameters, "--version")
		m.Parameters = append(m.Parameters, *version)
	}

	if outputPath != nil {
		m.Parameters = append(m.Parameters, "--output")
		m.Parameters = append(m.Parameters, *outputPath)
	}

	if disableSSLValidation != nil && *disableSSLValidation {
		m.Parameters = append(m.Parameters, "--disable-ssl-validation")
	}

	if searchKey != nil {
		m.Parameters = append(m.Parameters, "--search-key")
		m.Parameters = append(m.Parameters, *searchKey)
	}

	if openApi != nil {
		m.Parameters = append(m.Parameters, "--openapi")
		m.Parameters = append(m.Parameters, *openApi)
	}

	if language != nil {
		m.Parameters = append(m.Parameters, "--language")
		m.Parameters = append(m.Parameters, string(*language))
	}

	return m
}

func (m *Kiota) Search(
	searchTerm string) (string, error) {
	return m.Container.WithExec([]string{"search", searchTerm}, dagger.ContainerWithExecOpts{UseEntrypoint: true}).Stdout(context.Background())
}

func (m *Kiota) Show(
	openApi *string,
	clearCache *bool,
	logLevel *LogLevel,
	includePath *string,
	excludePath *string,
	version *string,
	searchKey *string,
	disableSSLValidation *bool,
) *Kiota {

	parameters := make([]string, 0)

	if clearCache != nil && *clearCache {
		parameters = append(parameters, "--clear-cache")
	}

	if logLevel != nil {
		parameters = append(parameters, "--log-level")
		parameters = append(parameters, string(*logLevel))
	}

	if includePath != nil {
		parameters = append(parameters, "--include-path")
		parameters = append(parameters, *includePath)
	}

	if excludePath != nil {
		parameters = append(parameters, "--exclude-path")
		parameters = append(parameters, *excludePath)
	}

	if version != nil {
		parameters = append(parameters, "--version")
		parameters = append(parameters, *version)
	}

	if searchKey != nil {
		parameters = append(parameters, "--search-key")
		parameters = append(parameters, *searchKey)
	}

	if disableSSLValidation != nil && *disableSSLValidation {
		parameters = append(parameters, "--disable-ssl-validation")
	}

	if openApi != nil {
		parameters = append(parameters, "--openapi")
		parameters = append(parameters, *openApi)
	}

	m.Container = m.Container.WithExec(append([]string{"show"}, append(m.Parameters, parameters...)...), dagger.ContainerWithExecOpts{UseEntrypoint: true})
	return m
}

// Client generation
func (m *Kiota) Generate(
	// +optional
	openApi *string,
	// +optional
	language *Language,
	// +optional
	additionalData *bool,
	// +optional
	outputPath *string,
	// +optional
	className *string,
	// +optional
	excludeBackwardCompatible *bool,
	// +optional
	deserializer *string,
	// +optional
	serializer *string,
	// +optional
	backingStore *bool,
	// +optional
	namespaceName *string,
	// +optional
	structuredMimeTypes []string,
	typeAccessModifier *TypeAccessModifier,
) *Kiota {

	parameters := make([]string, 0)

	if openApi != nil {
		parameters = append(parameters, "--openapi")
		parameters = append(parameters, *openApi)
	}

	if language != nil {
		parameters = append(parameters, "--language")
		parameters = append(parameters, string(*language))
	}

	if additionalData != nil && *additionalData {
		parameters = append(m.Parameters, "--additional-data")
		parameters = append(m.Parameters, fmt.Sprintf("%t", additionalData))
	}

	if className != nil {
		parameters = append(parameters, "--class-name")
		parameters = append(parameters, *className)
	}

	if excludeBackwardCompatible != nil && *excludeBackwardCompatible {
		parameters = append(parameters, "--exclude-backward-compatible")
		parameters = append(parameters, fmt.Sprintf("%t", *excludeBackwardCompatible))
	}

	if deserializer != nil {
		parameters = append(parameters, "--deserializer")
		parameters = append(parameters, *deserializer)
	}

	if serializer != nil {
		parameters = append(parameters, "--serializer")
		parameters = append(parameters, *serializer)
	}

	if backingStore != nil && *backingStore {
		parameters = append(parameters, "--backing-store")
	}

	if namespaceName != nil {
		parameters = append(parameters, "--namespace-name")
		parameters = append(parameters, *namespaceName)
	}

	if structuredMimeTypes != nil {
		for _, mimeType := range structuredMimeTypes {
			parameters = append(parameters, "--structured-mime-types")
			parameters = append(parameters, mimeType)
		}
	}

	if typeAccessModifier != nil {
		parameters = append(parameters, "--type-access-modifier")
		parameters = append(parameters, string(*typeAccessModifier))
	}

	if outputPath != nil {
		parameters = append(parameters, "--output")
		parameters = append(parameters, *outputPath)
	}

	m.Container = m.Container.WithExec(append([]string{"generate"}, append(m.Parameters, parameters...)...), dagger.ContainerWithExecOpts{UseEntrypoint: true})
	return m
}

func (m *Kiota) Info() *Kiota {
	m.Container = m.Container.WithExec(append([]string{"info"}, m.Parameters...), dagger.ContainerWithExecOpts{UseEntrypoint: true})
	return m
}
