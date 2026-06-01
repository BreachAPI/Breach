using System;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using MonoMod.InlineRT;
using MonoMod.Utils;

namespace MonoMod
{
    public static class MonoModRules
    {
        static MonoModRules()
        {
            var mod = MonoModRule.Modder.Module;
            var main = mod.GetType("OuterBeyond.Program").FindMethod("Main");
            main.Attributes = (main.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Assembly;

            var ctor = typeof(InternalsVisibleToAttribute).GetConstructor(new Type[] { typeof(string) });
            var attribConstructor = mod.ImportReference(ctor);

            var attribute = new CustomAttribute(attribConstructor);
            attribute.ConstructorArguments.Add(new CustomAttributeArgument(mod.ImportReference(typeof(string)), (object)"Breach.API"));
            mod.Assembly.CustomAttributes.Add(attribute);
        }
    }
}