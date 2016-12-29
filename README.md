# VroomJs [![NuGet](https://img.shields.io/nuget/v/VroomJs.svg?maxAge=2592000)](https://www.nuget.org/packages/VroomJs/)

This is a forked version of VrooMJs that focuses on cross platform.

## Examples

Execute some Javascript:

```c#
using (var engine = new JsEngine())
{
    using (var context = engine.CreateContext())
    {
        var x = (double)context.Execute("3.14159+2.71828");
        Console.WriteLine(x);  // prints 5.85987
    }
}
```

Create and return a Javascript object, then call a method on it:

```c#
using (JsEngine js = new JsEngine(4, 32))
{
    using (JsContext context = js.CreateContext())
    {
        // Create a global variable on the JS side.
        context.Execute("var x = {'answer':42, 'tellme':function (x) { return x+' '+this.answer; }}");
        // Get it and use "dynamic" to tell the compiler to use runtime binding.
        dynamic x = context.GetVariable("x");
        // Call the method and print the result. This will print:
        // "What is the answer to ...? 42"
        Console.WriteLine(x.tellme("What is the answer to ...?"));
    }
}
```

Access properties and call methods on CLR objects from Javascript:

```c#
class Test
{
    public int Value { get; set; }
    public void PrintValue(string msg)
    {
        Console.WriteLine(msg+" "+Value);
    }
}

using (JsEngine js = new JsEngine(4, 32))
{
    using (JsContext context = js.CreateContext())
    {
        context.SetVariable("m", new Test());
        // Sets the property from Javascript.
        context.Execute("m.Value = 42");
        // Call a method on the CLR object from Javascript. This prints:
        // "And the answer is (again!): 42"
        context.Execute("m.PrintValue('And the answer is (again!):')");
    }
}
```
## Platforms

### Windows

There are embedded .dlls (x64/x86) in the project that can be loaded dynamically.

```c#
VroomJs.AssemblyLoader.EnsureLoaded(); // windows only
```

Call this method on start of your application.

### Mac/Linux

And here things get complicated.

v8 is now at version 6 or more as of writing and only available from package managers on Ubuntu/Debian up to version 3. Firstly, that must be installed and can be done so either through `sudo apt-get install libv8-dev` on Linux. As of writing, the repository version is v3.14. Next you need a binary of VrooMJs. Good luck.
