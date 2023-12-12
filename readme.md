# Exploring Render Modes

Everyone's favourite seems to be *AutoInteractive* and *Per Page/Component* so deploy a *Blazor Web Project* with those settings.

You get two projects: 

1. The Web Server/Interactive Server project.
1. The Client project containing the Web Assembly project.

### Adding the RenderLogger to the Solution

Add the following Nuget packages to the projects:

Web Server :

```xml
<PackageReference Include="Blazr.RenderLogger" Version="0.1.0" />
<PackageReference Include="Blazr.RenderLogger.Server" Version="0.1.1" />
```

Client :

```xml
<PackageReference Include="Blazr.RenderLogger" Version="0.1.0" />
<PackageReference Include="Blazr.RenderLogger.WASM" Version="0.1.1" />
```

Add the following services to the Server `Program`:

```csharp
builder.AddRenderStateServerServices();
```

And the following services to the Client `Program`:

```csharp
builder.AddRenderStateWASMServices();
```

And add the following `using` to both project's `_Imports.razor`.

## The Pages/Components

Look at the two projects and notice that most of the components are in the Server project.  Only the `Counter` is in Client Project.

Add the following component to `Home`, `Counter` and `Weather` below the `Page Title` :

```csharp
@page "/"

<PageTitle>Home</PageTitle>

<RenderStateViewer />
```

