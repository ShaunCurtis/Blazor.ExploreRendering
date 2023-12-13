# Where Do I Put Components?

If you have tried out some of the *Blazor Web App* template options you will see the pages/components move about between the Server and Client applications.

## So, Is Location Important?

To answer that we need to understand the difference between pages and components.  

Components can live anywhere, as long as you can reference them.  Most of your components should live in Component Lbraries, not in deployment projects.  You should be using the same components for different deployments of the same application.

Pages are components with a `Page` attribute.  They can live anywhere that you can reference from the `Router` component.  

That does apply a restriction.  The Server project has a dependency on the Client project, so there can be no reciprical arrangement.  Pages that the Client project needs to see can't reside in the Server project because a router in the Client project can't reference them.

Pre Net8 I put everything in libraries.  My solution would contain a `Blazr.App.Components` project.  It would contain a folder structure like this:

 - Weather
    - Components
    - Routes
    - Services
  - ToDo
    - Components
    - Routes
    - Services

There were no render modes.  I still plan to use the same structure in general because I don't plan on using `Per Page/Component` rendering.  

> To be continued