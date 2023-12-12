# Going For Broke

> This is an early draft.  I've made it available as there are lots of questions on Stack Overflow and other forums on the topic.

Everyone's favourite seems to be *AutoInteractive* and *Per Page/Component*.  Lets have the full works, so we'll basr this discussion on a solution built using that template.

Create a solution using the *Blazor Web Project* with those settings.

You get two projects: 

1. The Web Server/Interactive Server project.
1. The Client project containing the Web Assembly project.

A word on acronyms and terminology.  I'll talk about three modes of rendering:

1. **SSSR** - classic Static Server Side Rendering.
2. **ASSR** - Active Server Side Rendering.  Blazor Server.
3. **CSR** - Client Side Rendering.  Blazor WASM/Web Assembly.

I'll use the acronyms through the rest of this article.

### Adding the RenderLogger to the Solution

*RenderLogger* is a Nuget package that provides some infrastructure to log and display the render mode of components.  See [Blazr.RenderLogger Repo on GitHub](https://github.com/ShaunCurtis/Blazr.RenderLogger)

Add the following Nuget packages to the projects:

Web Server :

```xml
<PackageReference Include="Blazr.RenderLogger" Version="0.1.2" />
<PackageReference Include="Blazr.RenderLogger.Server" Version="0.1.2" />
```

Client :

```xml
<PackageReference Include="Blazr.RenderLogger" Version="0.1.2" />
<PackageReference Include="Blazr.RenderLogger.WASM" Version="0.1.2" />
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

```csharp
@using Blazr.RenderLogger
```

## The Pages/Components

Look at the two projects and notice that most of the components are in the Server project.  Only  `Counter` is in Client Project.

Add the following component to `Home`, `Counter` and `Weather` below the `Page Title` :

```csharp
@page "/"
<PageTitle>Home</PageTitle>
<RenderStateViewer Parent="this" />
//...
```

Add it to `MainLayout`

```csharp
        <article class="content px-4">
            <RenderStateViewer Parent="this" />
            @Body
        </article>
```

Move `Weather` to the *Pages* folder in the *Client* project and modify it to run in interactive auto mode.

```csharp
@page "/weather"
@rendermode InteractiveAuto
```

Modify `Home` to run in interactive server mode.

```csharp
@page "/"
@rendermode InteractiveServer
```

### Run the Solution

You will see this:

![Home Server Rendered](./images/Home-ServerRendered.png)

The three pieces of information displayed are:

```text
Parent Compoment Name => Unique ID of the Scoped Session Service => Render Mode of the Component
```

The eye opener may be the render mode of the `MainLayout`.  It's SSSR.

Why?  Look at `App.razor`.  The two top level components have no render mode set.  The default is SSSR, so the `Router` is a statically rendered component.

```csharp
<!DOCTYPE html>
<html lang="en">

<head>
//..
    <HeadOutlet />
</head>

<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>

</html>
```

You can't set the render mode on a Layout or the `RouteView` component within `Routes`, so you either set up at the top in `App` or you set it on the pages or lower level components.

## Other Behaviours

If you navigate to Counter you will see a full ASSR.

![Counter Auto Rendered](./images/Counter-AutoRendered.png).

Note that you have two service IDs.  The ASSR ID is the same as `Home`.  There's a Hub session running maintaining the Scoped services for the duration of the SPA session. The `Layout` Id has changed as would be expected for classic SSSR: scoped services only exist for the period of the Http request.

Also note that although the render mode is `InteractiveAuto`, `Counter` was rendered as ASSR.

Navigate to `Weather`.  There's a jerkiness to the display as it switches modes.  And it still stays in ASSR.

Why hasn't the application switched to WASM CSR.  Out initial component was `Home` which was set to `InteractiveServer`, so the application has remained on that mode.

On the `Weather` page, hit `F5`.  Watch the component statically render and then switch to CSR when the WASM code has downloaded.  Now switch back to `Counter` you'll find it's also now in CSR mode.

Switch back to `Home` and then back to `Counter` and you're back in ASSR.

## More to Come

I'll add some more detail as I do more work on this.

## My Peronal Initial Conclusions ans Observations

> Note these are personal views and opinions.

My gut feeling is that using **Per Page/Component** mode is not hybrid, it's mongrel.  Most people who came to old Blazor struggled with the component concept and were confused with the lifecycle and events.  Throwing in render modes adds another level of complexity. I can see so many scenarios where components are talking to the wrong instances or types of services.  Throw `Auto` to the mix and the complexity spirals out of control.  Think of the exotic concoctions people will come up!

So my recommendation is go with either *Interactive Server* or *Interactive WebAssembly* and *Global* application.  Only go hybrid if you really knoiw what you are doing and have a deep understanding of components.

The one area where I can see I'll switch between WASM and Server is where I want to do high security stuff such as User log in and account management, but want to run the main UI in CSR.

