using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("Tilde ITS 2.0")]
[assembly: AssemblyCompany("Tilde")]
[assembly: AssemblyCopyright("Copyright © 2013 Tilde")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("1.0")]

[assembly: ComVisible(false)]