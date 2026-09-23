using System.Reflection;
using System.Text;

var managedDir = args.Length > 0 ? args[0] : throw new Exception("managedDir required");
var outDir = args.Length > 1 ? args[1] : throw new Exception("outDir required");
Directory.CreateDirectory(outDir);

var dlls = Directory.GetFiles(managedDir, "UnityEngine*.dll", SearchOption.AllDirectories)
    .Where(p => !Path.GetFileName(p).Equals("UnityEngine.dll", StringComparison.OrdinalIgnoreCase))
    .OrderBy(p => p)
    .ToList();

var extra = args.Skip(2);
dlls.AddRange(extra.Where(File.Exists));

var resolver = new PathAssemblyResolver(CollectResolverFiles(managedDir, dlls));
using var mlc = new MetadataLoadContext(resolver, "netstandard");

var sb = new StringBuilder(1 << 22);
sb.AppendLine("---@meta");
sb.AppendLine("--- Auto-generated Unity API stubs for xLua (CS.*). Do not require this file.");
sb.AppendLine("---@class CS");
sb.AppendLine("CS = {}");
sb.AppendLine();

var emittedNamespaces = new HashSet<string>();
var typeCount = 0;

foreach (var dll in dlls)
{
    Assembly asm;
    try
    {
        asm = mlc.LoadFromAssemblyPath(dll);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"skip {Path.GetFileName(dll)}: {ex.Message}");
        continue;
    }

    Type[] types;
    try { types = asm.GetTypes(); }
    catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray()!; }

    foreach (var type in types.OrderBy(t => t.FullName, StringComparer.Ordinal))
    {
        if (!ShouldExport(type)) continue;
        EmitNamespaceTables(sb, type, emittedNamespaces);
        EmitType(sb, type);
        typeCount++;
    }
}

var outFile = Path.Combine(outDir, "CS_UnityEngine.lua");
File.WriteAllText(outFile, sb.ToString(), new UTF8Encoding(false));
Console.WriteLine($"Wrote {typeCount} types -> {outFile} ({new FileInfo(outFile).Length} bytes)");

static List<string> CollectResolverFiles(string managedDir, List<string> dlls)
{
    var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var dll in dlls) files.Add(dll);
    foreach (var dll in Directory.GetFiles(managedDir, "*.dll", SearchOption.AllDirectories)) files.Add(dll);
    var dataDir = Path.GetFullPath(Path.Combine(managedDir, ".."));
    var netStd = Path.Combine(dataDir, "NetStandard");
    if (Directory.Exists(netStd))
    {
        foreach (var dll in Directory.GetFiles(netStd, "*.dll", SearchOption.AllDirectories))
            files.Add(dll);
    }
    return files.ToList();
}

static bool ShouldExport(Type type)
{
    if (type == null) return false;
    if (!type.IsPublic && !type.IsNestedPublic) return false;
    if (type.IsGenericTypeDefinition) return false;
    if (type.Name.StartsWith("<", StringComparison.Ordinal)) return false;
    if (type.Name.Contains("<")) return false;
    var ns = type.Namespace ?? "";
    if (ns.StartsWith("UnityEditor", StringComparison.Ordinal)) return false;
    if (ns.StartsWith("Unity.Burst", StringComparison.Ordinal)) return false;
    if (ns.Contains("Internal")) return false;
    if (string.IsNullOrEmpty(ns)) return false;
    if (ns.StartsWith("TMPro", StringComparison.Ordinal)) return true;
    return ns.StartsWith("UnityEngine", StringComparison.Ordinal) || ns.StartsWith("Unity.", StringComparison.Ordinal);
}

static string CsName(Type type)
{
    var name = (type.FullName ?? type.Name).Replace('+', '.');
    var tick = name.IndexOf('`');
    if (tick >= 0) name = name[..tick];
    return "CS." + name;
}

static string LuaType(Type? type)
{
    if (type == null) return "any";
    if (type.IsByRef) type = type.GetElementType()!;
    if (type.IsPointer) return "any";
    if (type.IsArray) return LuaType(type.GetElementType()) + "[]";
    if (type.IsGenericType) return "any";
    var fn = type.FullName ?? type.Name;
    return fn switch
    {
        "System.Void" => "nil",
        "System.Boolean" => "boolean",
        "System.String" or "System.Char" => "string",
        "System.Object" or "System.IntPtr" or "System.UIntPtr" => "any",
        "System.Single" or "System.Double" or "System.Decimal"
            or "System.Byte" or "System.SByte"
            or "System.Int16" or "System.UInt16"
            or "System.Int32" or "System.UInt32"
            or "System.Int64" or "System.UInt64" => "number",
        _ => ((type.Namespace ?? "").StartsWith("UnityEngine") || (type.Namespace ?? "").StartsWith("Unity.") || (type.Namespace ?? "").StartsWith("TMPro"))
            ? CsName(type)
            : "any"
    };
}

static void EmitNamespaceTables(StringBuilder sb, Type type, HashSet<string> emitted)
{
    var full = CsName(type);
    var parts = full.Split('.');
    var current = parts[0];
    for (var i = 1; i < parts.Length - 1; i++)
    {
        current += "." + parts[i];
        if (emitted.Add(current))
        {
            sb.AppendLine($"---@class {current}");
            sb.AppendLine($"{current} = {{}}");
            sb.AppendLine();
        }
    }
}

static bool IsLuaKeyword(string name)
{
    return name is "and" or "break" or "do" or "else" or "elseif" or "end" or "false" or "for"
        or "function" or "goto" or "if" or "in" or "local" or "nil" or "not" or "or"
        or "repeat" or "return" or "then" or "true" or "until" or "while";
}

static void EmitType(StringBuilder sb, Type type)
{
    var cs = CsName(type);
    sb.Append($"---@class {cs}");
    try
    {
        var bt = type.BaseType;
        if (bt != null && bt.FullName != "System.Object" && bt.FullName != "System.ValueType"
            && bt.FullName != "System.Enum" && ShouldExport(bt))
            sb.Append($" : {CsName(bt)}");
    }
    catch { /* metadata */ }
    sb.AppendLine();

    var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    foreach (var f in Safe(() => type.GetFields(flags)))
    {
        try
        {
            if (f.IsSpecialName) continue;
            if (IsLuaKeyword(f.Name)) continue;
            sb.AppendLine($"---@field {f.Name} {LuaType(f.FieldType)}");
        }
        catch { }
    }
    foreach (var p in Safe(() => type.GetProperties(flags)))
    {
        try
        {
            if (p.GetIndexParameters().Length > 0) continue;
            if (IsLuaKeyword(p.Name)) continue;
            sb.AppendLine($"---@field {p.Name} {LuaType(p.PropertyType)}");
        }
        catch { }
    }

    sb.AppendLine($"{cs} = {{}}");

    if (type.IsEnum)
    {
        foreach (var f in Safe(() => type.GetFields(BindingFlags.Public | BindingFlags.Static)))
        {
            if (!f.IsLiteral) continue;
            if (IsLuaKeyword(f.Name)) continue;
            sb.AppendLine($"{cs}.{f.Name} = 0");
        }
        sb.AppendLine();
        return;
    }

    foreach (var m in Safe(() => type.GetMethods(flags)))
    {
        try
        {
            if (m.IsSpecialName) continue;
            if (m.Name.StartsWith("get_") || m.Name.StartsWith("set_") || m.Name.StartsWith("add_") || m.Name.StartsWith("remove_")) continue;
            if (m.Name.StartsWith("op_")) continue;
            if (IsLuaKeyword(m.Name)) continue;
            ParameterInfo[] ps;
            try { ps = m.GetParameters(); } catch { continue; }
            if (ps.Any(p => p.ParameterType.IsByRef || p.ParameterType.IsPointer)) continue;
            var call = m.IsStatic ? "." : ":";
            var args = string.Join(", ", ps.Select(p => SanitizeArg(p.Name ?? "arg")));
            var ret = LuaType(m.ReturnType);
            if (ret != "nil")
                sb.AppendLine($"---@return {ret}");
            for (var i = 0; i < ps.Length; i++)
                sb.AppendLine($"---@param {SanitizeArg(ps[i].Name ?? "arg")} {LuaType(ps[i].ParameterType)}");
            sb.AppendLine($"function {cs}{call}{m.Name}({args}) end");
        }
        catch { }
    }

    sb.AppendLine();
}

static string SanitizeLocal(string name)
{
    name = name.Replace("`", "_").Replace("+", "_").Replace("<", "_").Replace(">", "_");
    if (IsLuaKeyword(name) || char.IsDigit(name[0])) return "_" + name;
    return name;
}

static string SanitizeArg(string name)
{
    name = string.IsNullOrWhiteSpace(name) ? "arg" : name;
    if (IsLuaKeyword(name)) return "_" + name;
    return name.Replace(" ", "_");
}

static IEnumerable<T> Safe<T>(Func<T[]> getter)
{
    try { return getter() ?? Array.Empty<T>(); }
    catch { return Array.Empty<T>(); }
}
