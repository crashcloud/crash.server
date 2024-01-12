# Crash.Server

Crash.Server is a multi-user communication server designed to work alongside Crash. This project was developed
by [crashcloud](https://github.com/crashcloud) and is available for use under
the [MIT License](https://github.com/crashcloud/Crash.Server/blob/main/LICENSE.md).

## Get Hacking

1. Clone the Repo
``` bash
git clone https://github.com/crashcloud/crash.server.git
```
2. Open the project in Visual Studio

// NOTE : dotnet run and dotnet watch do not seem to work

3. Open the src folder
``` bash
cd src
```
4. run
``` bash
dotnet watch --urls http://0.0.0.0:8080
```

## Installation

### Docker

You can install Crash.Server as a Docker container by running the following command:

``` bash
docker pull crashserver/crash.server
docker run -p 8080:8080 crashserver/crash.server
```

Or you can build and run it with
``` bash
docker build . --label crash.server --tag crash.server
```

Just ensure when you 

### Release

You can download the latest release of Crash.Server from
the [GitHub Releases](https://github.com/crashcloud/Crash.Server/releases) page. Once you have downloaded the release,
extract it to a folder of your choice and then run the `crash.server.exe` file.

## Contributing

We welcome contributions to Crash.Server! To contribute, please follow these steps:

1. Fork the Crash.Server repository.
2. Make your changes.
3. Create a pull request.

We will review your pull request as soon as possible.

## Support

If you have any questions or issues with Crash.Server, please create a new issue on
the [Crash.Server GitHub page](https://github.com/crashcloud/Crash.Server/issues).

## License

Crash.Server is available for use under the [MIT License](https://github.com/crashcloud/Crash.Server/blob/main/LICENSE).
