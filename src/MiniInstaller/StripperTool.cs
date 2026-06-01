using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Breach.MiniInstaller
{
    public static partial class Program
    {
        public struct StripStats
        {
            public int TypeCount;
            public int FieldCount;
            public int PropertyCount;
            public int MethodCount;

            public StripStats(int typeCount, int fieldCount, int propertyCount, int methodCount)
            {
                TypeCount = typeCount;
                FieldCount = fieldCount;
                PropertyCount = propertyCount;
                MethodCount = methodCount;
            }

            public static StripStats operator +(StripStats left, StripStats right)
            {
                return new StripStats(left.TypeCount + right.TypeCount, left.FieldCount + right.FieldCount, left.PropertyCount + right.PropertyCount, left.MethodCount + right.MethodCount);
            }
        }

        public static StripStats PublicizeAssembly(string fileName, string outputname, bool members = true, bool strip = true, bool game = true)
        {
            var module = ModuleDefinition.ReadModule(fileName, new ReaderParameters(ReadingMode.Immediate));
            StripStats stats = new StripStats();

            // Zip it ...
            foreach (TypeDefinition type in module.Types)
            {
                stats += PublicizeType(type, members, strip, game);
            }

            // ... vacuum it ...
            if (strip)
            {
                module.Resources.Clear();
                module.CustomDebugInformations.Clear();
            }

            // ... and ship it
            module.Write(outputname);
            return stats;
        }

        public static StripStats PublicizeType(TypeDefinition type, bool members = true, bool strip = true, bool game = true)
        {
            if (type.IsSpecialName || type.IsRuntimeSpecialName)
            {
                // Skip special types
                return new StripStats(0, 0, 0, 0);
            }

            if (type.CustomAttributes.Any(a => a.AttributeType.FullName is "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
            {
                // Skip compiler-generated types (closures, iterators, etc.)
                return new StripStats(0, 0, 0, 0);
            }

            StripStats stats = new StripStats(1, 0, 0, 0);

            // Only publicize classes for the game's assembly
            if (game && type.Namespace.StartsWith("OuterBeyond"))
            {
                // TODO: don't publicize Program, THDebug, etc.
                if (type.IsNested)
                    type.Attributes = (type.Attributes & ~TypeAttributes.VisibilityMask) | TypeAttributes.NestedPublic;
                else
                    type.Attributes = (type.Attributes & ~TypeAttributes.VisibilityMask) | TypeAttributes.Public;
            }

            // Recurse
            foreach (var nested in type.NestedTypes) stats += PublicizeType(nested, members, strip);

            // Method
            foreach (var method in type.Methods)
            {
                bool methodChanged = false;

                if (members && !ShouldSkip(method))
                {
                    methodChanged = true;
                    method.Attributes = (method.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Public;
                }

                // Strip
                if (strip && method.HasBody)
                {
                    methodChanged = true;
                    var body = method.Body;
                    body.Instructions.Clear();
                    body.Variables.Clear();
                    body.ExceptionHandlers.Clear();
                    body.InitLocals = false;
                    var ret = Instruction.Create(OpCodes.Ret);
                    body.Instructions.Add(ret);
                }

                if (methodChanged) stats.MethodCount++;
            }

            // Skip members, if applicable
            if (!members) return stats;

            // Field
            foreach (var field in type.Fields)
            {
                if (ShouldSkip(field))
                {
                    // Skip compiler-generated fields (backing fields, etc.)
                    continue;
                }

                stats.FieldCount++;
                field.Attributes = (field.Attributes & ~FieldAttributes.FieldAccessMask) | FieldAttributes.Public;
            }

            // Property
            foreach (var prop in type.Properties)
            {
                if (ShouldSkip(prop))
                {
                    // Skip compiler-generated properties
                    continue;
                }

                stats.PropertyCount++;
                if (prop.GetMethod != null)
                    prop.GetMethod.Attributes = (prop.GetMethod.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Public;
                if (prop.SetMethod != null)
                    prop.SetMethod.Attributes = (prop.SetMethod.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Public;
            }

            return stats;
        }

        private static bool ShouldSkip(IMemberDefinition member)
        {
            return
                member.IsSpecialName ||
                member.IsRuntimeSpecialName ||
                member.CustomAttributes.Any(a =>
                    a.AttributeType.FullName is "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
        }
    }
}