# Unity Runtime Debug Action

RuntimeDebugAction is a runtime debug menu system for Unity that lets you register and trigger debug actions with no extra setup. With a set of built-in debug actions and a tiny logger that works out of the box, RDA is built to supercharge your mobile and desktop game development.

![](https://bennykok.github.io/runtime-debug-action-docs/images/2020-12-14-11-20-22.png)

## Links

[Preview1](https://twitter.com/BennyKokMusic/status/1405039828337455116) | [Preview2](https://twitter.com/BennyKokMusic/status/1316547829817466880) | [Documentation](https://bennykok.github.io/runtime-debug-action-docs/manuals/QuickStart/index.html) | [Discord](https://discord.gg/fHGsArj) 

## Install

Via UPM.

```
UPM install via git url -> https://github.com/BennyKok/unity-runtime-debug-action.git
```

You can also choose to add this as a submodule in your package folder.

```
git submodule add https://github.com/BennyKok/unity-runtime-debug-action.git Packages\unity-runtime-debug-action
```

## Examples

With RDA, you can add action via [code](https://bennykok.github.io/runtime-debug-action-docs/manuals/CustomActions/fluent-api.html), [component](https://bennykok.github.io/runtime-debug-action-docs/manuals/CustomActions/debug-action-component.html), [reflection](https://bennykok.github.io/runtime-debug-action-docs/manuals/CustomActions/attribute-reflection.html)

Here's a glimpse of adding via code.

```csharp
RuntimeDebugSystem.RegisterActions(
    DebugActionBuilder.Button()
        .WithName("Your actions")
        .WithAction(()=>{ });
);
```

## Explore
Feel free to check me out!! :)

[Twitter](https://twitter.com/BennyKokMusic) | [Website](https://bennykok.com) | [AssetStore](https://assetstore.unity.com/publishers/28510)
