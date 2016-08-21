# Command Central Backend

The Command Central Backend is a RESTful service that provides data access, authentication, validation, and authorization services to Navy personnel data.  The service includes a few features which we've encapsulated in "modules":

  - Core
    * Personnel data management (update, search, etc. user profiles)
    * Comprehenisive and extensible permissions system
    * Email system
    * Extensible logging system
    * Windows service support
  - News
    * Update, add, delete, load news items (resembles a blog style system)
  - Muster
    * Take daily accountability of all active users in the data base.
    * Generate reports based on previous days' muster.
    * Automatically turn over muster every day at set times.
    
### The Goal 

> Provide a common access point for personnel data from across the command, creating a more simple user experience for the Sailors, reducing administrative overhead, and opening new avenues of synergy.

### References

Command Central sits atop the shoulders of giants.  Without their work, this project simply would never have happened.  In no particular order:

* [NHibernate] - NHibernate is a mature, open source object-relational mapper for the .NET framework. 
* [FluentNHibnerate] - Fluent, XML-less, compile safe, automated, convention-based mappings for NHibernate.
* [CommandLineParser] - Offers a clean and concise API for manipulating and validating command line arguments.
* [fluent-email] - Though we are no longer using fluent-email, the current email module's design drew heavily on inspriation gained from this project.
* [FluentScheduler] - Automated job scheduler with fluent interface.
* [FluentValidation] - A validation library with a fluent interface.
* [lesi.collections] - Heavily extends the System.Collections namespace.
* [Microsoft.AspNet.Razor] - The runtime render/view engine used by ASP.NET applications.  We use this to render our emails.
* [RazorEngine] - A templating engine built on top of Microsoft's ASP.NET Razor view engine.  We use this to render our emails.
* [MySql.Data] - ADO.NET driver for MySQL.
* [Newtonsoft.Json] - Litterally the best JSON serialization library there is.  This thing can serialize anything and ask for more.
* [NHibernate.Caches.SysCache] - Cache provider for NHibernate which uses the ASP.NET caching engine.
* [Polly] - Allows us to express transient exception handling.  We use this in the Email module to enable the retry behavior on failed sends.

### Acknowledgements

TODO

### Operation

The service may by launched in two modes:
* Interactive
* Windows Service

#### Interactive

Launching the service in interactive mode means little more than executing the service from the command line. 

_Note: In this mode, the service does not require elevated permissions._

```sh
CCServ.exe launch -u %mysql username% -p %mysql password% -s %mysql database address% -d %mysql database/schema%
```

TODO

#### Windows Service

TODO

### Development

Want to contribute? Great!

Atwood is responsible for all changes to the branches master, Pre-Production and Production.  Please feel free to fork or make ask for access to the repository and make your own branch.

Please communicate with the development team to understand the current direction of the project and what we're working on next.

#### Building for source

Clone the repository in Visual Studio 2012, 2013 or 2015 and then simply build the solution.  Visual Studio will reacquire all the Nuget packages and then build the project into your output path at either \Debug or \Release depending on your settings.

When running the service for the first time, it will attempt to populate its required schema into the targeted database so make sure your mysql user has the necessary permissions to do that.

License
----

The Please Don't Sue Us License 2016

[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job. There is no need to format nicely because it shouldn't be seen. Thanks SO - http://stackoverflow.com/questions/4823468/store-comments-in-markdown-syntax)

[NHibernate]: <http://nhibernate.info/>
[FluentNHibnerate]: <http://www.fluentnhibernate.org/>
[CommandLineParser]: <https://github.com/gsscoder/commandline>
[fluent-email]: <https://github.com/lukencode/FluentEmail>
[FluentScheduler]: <https://github.com/fluentscheduler/FluentScheduler>
[FluentValidation]: <https://github.com/JeremySkinner/FluentValidation>
[lesi.collections]: <https://github.com/nhibernate/iesi.collections>
[Microsoft.AspNet.Razor]: <https://www.nuget.org/packages/microsoft.aspnet.razor/>
[RazorEngine]: <https://github.com/Antaris/RazorEngine>
[MySql.Data]: <http://dev.mysql.com/downloads/connector/net/>
[Newtonsoft.Json]: <https://github.com/JamesNK/Newtonsoft.Json>
[NHibernate.Caches.SysCache]: <https://github.com/diegose/NHibernate.Diegose>
[Polly]: <https://github.com/App-vNext/Polly>

