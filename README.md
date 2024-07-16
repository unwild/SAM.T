# SAM.T
Simple application monitoring tool.  Provide healthchecks automation and history.

As the name suggest, this is a very simple and minimalist way of fetching health state from applications.

## Use case

 - You want to gather basic health informations about many .NET applications ?
 - Those applications are ranging from old .NET framework to shiny new .NET Core ones ?
 - You don't want to make a complex OpenTelemetry setup ?

This tool may be for you.

## How it works
 
 The Worker agent is fetching data from the application via simple http requests.
 
 The target applications can expose an endpoint returning an HealthCheckResponse.  
 > If not, the fetching will occur anyway and collect http status code and response time.




