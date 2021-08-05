# **Mapalyzer** (ie: Mapping with Analyzers)

**Guess what? It's a piece of cake** :cake:

A boilerplate was built using C# Analyzers for object-to-object mapping in DotNET.

Beacuase this project was developed using the standard [**Roslyn Source Generators**](https://github.com/dotnet/roslyn-sdk/tree/main/samples/CSharp/SourceGenerators), you won't need to consider any configuration codes in your setup classess.

You only need to:

1. add the *library* in your project's refrences
2. decorate your destination classes with our valid *attributes*.

:thinking: **But wait! what about the more complex conversions?** Don't worry. We considered a way of injecting your out-of-box mapping rules whenever you want to use the mapping.

# Manual

### First, Adding the library to your project

1. via **`dotnet-cli`**:

    ```sh
    dotnet add package Mapalyzer --version 1.2.1-alpha
    ```

2. via **NuGet console**:

   ```powershell
    Install-Package Mapalyzer -Version 1.2.1-alpha
    ```

3. via **.csproj**
    ```xml
    <PackageReference Include="Mapalyzer" Version="1.2.1-alpha" />
    ```

### :warning: Attentions

- If using **Visual Studio**, you must clean the solution, restart the Visual Studio and rebuild the solution in order to get the library to work.
- If using **VSCode**, you must execute `dotnet restore && dotnet build` and restart VSCode in order to get the library to work.

### Then, Decorating the destination class

:ship: Now the ship is ready to sail. You just need to decorate your desired destination classes and/or their properties in order to connect them to your source classes.

# Scenario

The *MapFrom* attribute is used to decorate the destination class and accepts an argument which is a `Type` value of the source class. As soon as you decorate the class, the analyzer of the library seeks the source class and mapps all twin properties implicitly.

Then the mapping code is being generated in background and added to the assembly of your project.

Note that since all mapping methods are being defined under the assembley of the project, you don't need to import any references whenever you want to use them.

For instance, consider you have a class named `Entity` in our domain layer which is specified as database object and another class in our application layer named `EntityModel`. After a successful mapping, whenever you declare an `Entity`, you immediately and **easily** will access an extension method called `ToEntityModel` which returns an `EntityModel` object.

Plus, we also considered a way of [**custom mapping**](#custom-mapping-rules).

# Usage

#### Class Definition

The destination class should have the `partial` modifier beacuase the mapper will generated a partial class with the same namespace and name. If your desired destination class has not the `partial` modifier, there will be not mapping class.

#### Using `MapFrom` Attribute

Then you need to decorate the destination class with the [`MapFrom`](#attribute-definitions) attribute which you can check out the attribute's signature in the following table.

#### Using `MapPropertyFrom` Attribute

If there are any properties in destination class which its signature is different from all properties of the source class and you still want to map it with one of them, you can easily decorate your desired destination property with the [`MapPropertyFrom`](#attribute-definitions) attribute.

#### Custom Mapping Rules

Consider you have your own rules for mapping a specific model. How would you do that? Don't worry! In this case, you can define a method which returns an object with the type `Action<SourceClassType, DestinationClassType>`. Then you can pass it as an delegate argument to extension methods([check out the sample below](#sample)).

 #### Attribute definitions

|Name|Parameters|Description
|:---|:---|:---|
|MapFrom|*serviceType*: `Type`|The type value of the source class.</br>*ex: `typeof(Your.Core.Project.Domain.Entity)`*|
|MapPropertyFrom|*sourcePropertyName*: `string`</br></br>*ignoreTypeDifference*: `bool`|`sourcePropertyName`: The name of the desired property in the source class.</br></br>`ignoreTypeDifference`: The default is false. If you set it as `true`, while the type of the both properties implemented `IConvertible`, the source property will be convert to the destination property.

# Sample

### Source class

```csharp
namespace Your.Core.Project.Domain 
{
    public class Entity
    {
        // Your source properties definitions
        // ...

        public string Buz { get; set; }
        public byte Qux { get; set; }

        // Consider it holds multiple values separated by a ',' like "43,2,289,13"
        public string Bar { get; set; }
    }
}
```

### Destination class

```csharp
using System;
// All attributes you're going to use are being generated under the System namespace \
// which is why there is no need to reference to any namespaces but System.
using System.Linq;

namespace Your.Application.Project.Models
{
    // Don't forget to add partial modifier to the class
    [MapFrom(sourceType: typeof(Your.Core.Project.Domain.Entity))]
    public partial class EntityModel 
    {
        // Your destination properties definitions which are being implicity mapped \
        // because the mapper found their twins in source class
        // ...

        // You can enforce the mapper to map a specific destination property \
        // differing either in name or type with another decorator.
        [MapPropertyFrom(sourcePropertyName: "Buz")]
        public string Foo { get; set; }

        [MapPropertyFrom(sourcePropertyName: "Qux", ignoreTypeDifference: true)]
        public short Qux { get; set; }

        // It will not mapped beacuse there is no corresponding source property to match exactly with its signature. Now we need to map it in our CustomMapper.
        public short[] Bar { get; set; }

        // Custom Mapper
        public static Action<Your.Core.Project.Domain.Entity, EntityModel> CustomMapper() => (entity, entityModel) => 
        {
            entityModel.Bar = entity.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                    .Select(b => Convert.ToInt16(b))
                                    .ToArray();
            
            // Other custom rules
            // ...
        }
    }
}
```

# Contact

:email: E-mail: [shojajou@gmail.com](mailto:shojajou@gmail.com)
