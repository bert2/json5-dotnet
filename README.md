new ConfigurationBuilder().AddJsonFile("appsettings.json")

HostApplicationBuilder b = Host.CreateApplicationBuilder(args)
b.Configuration.AddJsonFile("appsettings.json");

HostBuilder b = Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration(Action<HostBuilderContext,IConfigurationBuilder>)
	
JsonConfigurationProvider, JsonStreamConfigurationProvider -> JsonConfigurationFileParser

