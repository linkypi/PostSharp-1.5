This sample demonstrates how to use PostSharp at runtime.

This sample allows to transform an assembly (.EXE) before executing it. You have to pass
the project and the program you want, and optionally some parameters for the project
and some for the program (see command line).

The host will transform not only the program assembly, but also all dependencies residing
in the same directory as the program, or in one of its subdirectories.

This sample works with three AppDomains.

- The the initial AppDomain, called the system AppDomain, is responsible for starting the other 
  AppDomains: the client and the PostSharp ones. The principal class in this AppDomain is the
  Host class,  which implements the IPostSharpHost interface.
  
- The client AppDomain is the domain in which the user program (after weaving by PostSharp)
  is executed. This AppDomain is managed by the ClientDomainManager class, which starts
  the program assembly and resolves assembly references.
  
- The PostSharp AppDomain is where the initial assemblies are loaded and transformed. This is
  totally transparent to the host. The Host class just creates a PostSharp Object and has
  not to manage this AppDomain.
  
  
Note that all dependencies are processed in deep-first order (the referenced module is processed 
first, then the referencing one). Another approach (not implemented in this sample) is to
load dependencies lazily, as they are requested by the VRE. This requires the
ClientDomainManager's implementation of AssemblyResolve calls IPostSharpObject.InvokeProjects,
and tell PostSharp *not* to process references first recursively. In this scenario, 
you have to overwrite assembly names yourself.
  