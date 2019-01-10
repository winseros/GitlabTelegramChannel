# Gitlab Telegram Channel

The application integrating [GitLab](https://about.gitlab.com/) with [Telegram](https://telegram.org/).

## How it works
The application listens for [GitLab webhooks](https://docs.gitlab.com/ce/user/project/integrations/webhooks.html) and calls the Telegram [Bot API](https://core.telegram.org/bots/api#available-methods).

## Which Webhook events are supported
* [Comment on merge request](https://docs.gitlab.com/ce/user/project/integrations/webhooks.html#comment-on-merge-request)
* [Comment on code snippet](https://docs.gitlab.com/ce/user/project/integrations/webhooks.html#comment-on-code-snippet)
* [Merge request events](https://docs.gitlab.com/ce/user/project/integrations/webhooks.html#merge-request-events)
* [Pipeline events](https://docs.gitlab.com/ce/user/project/integrations/webhooks.html#pipeline-events) (`'failed'` event only)

## How to setup
1) Plan the endpoint for the application to run.
2) Setup GitLab webhooks and obtain a [GitLab secret token](https://docs.gitlab.com/ce/user/project/integrations/webhooks.html#secret-token). Use the following url as a webhook target: `<your_endpoint>/gitlab_hook`, e.g. https://my-hook-endpoint.com/gitlab_hook
3) [Setup](https://core.telegram.org/bots#6-botfather) a Telegram bot and obtain a `Bot token`
4) [Setup](https://telegram.org/faq_channels) a Telegram channel to which your bot would post messages
5) Configure and deploy the app, see the `Configuration reference` section below

## Configuring

### If you're familiar with the [.NET Core configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2) framework

Use any of the standard ways to provide the app configuration: 
* appsettigns.json 
* appsettings.{environment}.json 
* environment variables
* command line arguments

### If not

There are several ways to configure a .NET Core application

1) `appsettings.json` file - normally contains the most of the configuration.
2) `appsettings.{environment}.json` file - may be used to make some environment-dependent configuration overrides. The environment name normally comes from the `ASPNETCORE_ENVIRONEMNT` environment variable or from an `--environment` command line arg. By-default the environment name is `Production`, so the filename would be the `appsettings.production.json`.
3) Environment variables. Every setting from the `appsettings.json` may be overriden by an environment variable, having a name, same to the setting name in  `appsettings.json`. The `:` symbol in a variable name is used as a hierarchy level separator. E.g. configuration setting `TGram > Connection > Timeout` may be overriden by an environemnt variable `TGRAM:CONNECTION:TIMEOUT`.
4) Command line arguments. Every setting from the `appsettings.json` may be overriden by a command-line key. The `:` symbol in a key name is used as a hierarchy-level separator. E.g. configuration setting `TGram > Connection > Timeout` may be overriden by a `--tgram:connection:timeout` parameter.

## Running the app
1) Ensure you have the [runtime of a compatible version](https://dotnet.microsoft.com/download/dotnet-core/2.2) installed. At the moment the application was developed, the `2.2.x` version of the runtime was current. Pay attention you don't need the whole SDK to run the app - a bare runtime is enough.
2) run `dotnet TGramWeb.dll`, optionally providing command-line args. e.g: `dotnet TGramWeb.dll --urls http://localhost:9000 --environment azure`

## Configuration reference
### Listen addresses
A set of addresses the app would be listening at. A semicolon `;` may be used as a separator for multiple addresses. E.g. `http://localhost:5000;https://localhost:5001`

| appsettings.json | Command line | Env variable    |
|------------------|--------------|-----------------|
| n/a              | --urls       | ASPNETCORE_URLS |

### Runtime environment
Determines the application environment. The default value is `Production`

| appsettings.json | Command line  | Env variable           |
|------------------|---------------|------------------------|
| n/a              | --environment | ASPNETCORE_ENVIRONMENT |

### Gitlab secure token
A secure token you obtain when configuring a gitlab web hook.

| appsettings.json | Command line   | Env variable |
|------------------|----------------|--------------|
| Gitlab>Token     | --gitlab:token | GITLAB:TOKEN |

### Telegram API endpoint
Address of the telegram API. Normally `https://api.telegram.org`

| appsettings.json | Command line     | Env variable   |
|------------------|------------------|----------------|
| TGram>Endpoint   | --tgram:endpoint | TGRAM:ENDPOINT |

### Telegram bot token
Telegram bot token you obtain when creating a bot.

| appsettings.json | Command line   | Env variable |
|------------------|----------------|--------------|
| TGram>Token      | --trgram:token | TGRAM:TOKEN  |

### Telegram channel
Telegram channel name. E.g. `@mytgramchannel`

| appsettings.json | Command line    | Env variable  |
|------------------|-----------------|---------------|
| TGram>Channel    | --tgram:channel | TGRAM:CHANNEL |

### Telegram API connection timeout
Connection timeout when trying to send a message to telegram.

| appsettings.json         | Command line               | Env variable             |
|--------------------------|----------------------------|--------------------------|
| TGram>Connection>Timeout | --tgram:connection:timeout | TGRAM:CONNECTION:TIMEOUT |

### Telegram API connection attempts
Number of attempts to send a message to telegram before giving up.

| appsettings.json          | Command line                | Env variable              |
|---------------------------|-----------------------------|---------------------------|
| TGRAM:CONNECTION:ATTEMPTS | --tgram:connection:attempts | TGRAM:CONNECTION:ATTEMPTS |

### Telegram API connection interval
An interval between telegram call attempts.

| appsettings.json          | Command line                | Env variable              |
|---------------------------|-----------------------------|---------------------------|
| TGram>Connection>Interval | --tgram:connection:interval | TGRAM:CONNECTION:INTERVAL |

### Internal daemon address
A localhost address used to communicate between the web server and internal message handlers.

| appsettings.json | Command line     | Env variable   |
|------------------|------------------|----------------|
| Daemon>Address   | --daemon:address | DAEMON:ADDRESS |

### Number of internal message handlers

| appsettings.json   | Command line         | Env variable       |
|--------------------|----------------------|--------------------|
| Daemon>ThreadCount | --daemon:threadcount | DAEMON:THREADCOUNT |

### Serilog
[Serilog](https://serilog.net) standard configuration. [See here](https://github.com/serilog/serilog-settings-configuration).

| appsettings.json | Command line  | Env variable |
|------------------|---------------|--------------|
| Serilog>...      | --serilog:... | SERILOG:...  |