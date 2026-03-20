namespace SemanaIA.ServiceInvoice.Infrastructure.Xml;

using System.Dynamic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

public class XBuilderNew : DynamicObject
{
     private readonly XDocument _root = new();
    private XContainer _current;

    public bool UseDashInsteadUnderscore { get; set; }
    public bool UsePropertyNamespacing { get; set; }
    public bool RemoveEmptyXmlnsOnOutput { get; set; }

    public XBuilderNew()
    {
        _current = _root;
    }

    public static Action Fragment(Action fragmentBuilder)
        => fragmentBuilder ?? throw new ArgumentNullException(nameof(fragmentBuilder));

    public static Action<dynamic> Fragment(Action<dynamic> fragmentBuilder)
        => fragmentBuilder ?? throw new ArgumentNullException(nameof(fragmentBuilder));

    public static XBuilder Build(Action<dynamic> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var xml = new XBuilder();
        builder(xml);
        return xml;
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object? result)
    {
        result = null;
        var tagName = UseDashInsteadUnderscore ? binder.Name.Replace("_", "-") : binder.Name;
        Tag(tagName, args);
        return true;
    }

    public void Tag(string tagName, params object[] args)
    {
        ArgumentException.ThrowIfNullOrEmpty(tagName);

        if (tagName.StartsWith("_", StringComparison.Ordinal))
            tagName = tagName[1..];

        string? content = null;
        object? attributes = null;
        XNode? contentElement = null;
        Action? fragment = null;

        foreach (var arg in args)
        {
            if (arg is null) continue;

            if (arg is Action action)
                fragment = action;
            else if (arg is Action<dynamic> dynamicAction)
                fragment = () => dynamicAction(this);
            else if (arg is string stringArg)
                content = stringArg;
            else if (arg is XNode node)
                contentElement = node;
            else if (arg.GetType().IsValueType)
                content = arg.ToString();
            else
                attributes = arg;
        }

        XElement? element = ResolveElement(tagName, attributes);
        element ??= new XElement(tagName);

        _current.Add(element);

        if (contentElement is not null)
            element.Add(contentElement);

        if (fragment is not null)
            _current = element;

        if (!string.IsNullOrWhiteSpace(content))
            element.Add(content);

        if (attributes is not null)
            AddAttributes(element, tagName, attributes);

        if (fragment is not null)
        {
            fragment();
            _current = element.Parent;
        }
    }

    public void Comment(string comment)
    {
        ArgumentException.ThrowIfNullOrEmpty(comment);
        _current.Add(new XComment(comment));
    }

    public void CData(string data)
    {
        ArgumentException.ThrowIfNullOrEmpty(data);
        _current.Add(new XCData(data));
    }

    public void Text(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        _current.Add(new XText(text));
    }

    public void Declaration(string? version = null, string? encoding = null, string? standalone = null)
    {
        _root.Declaration = new XDeclaration(version, encoding, standalone);
    }

    public string ToString(bool indent)
    {
        Encoding encoding = new UTF8Encoding(false);

        if (_root.Declaration?.Encoding?.Equals("utf-16", StringComparison.OrdinalIgnoreCase) == true)
            encoding = new UnicodeEncoding(false, false);

        using var memoryStream = new MemoryStream();
        using var writer = XmlWriter.Create(memoryStream, new XmlWriterSettings
        {
            Encoding = encoding,
            Indent = indent,
            CloseOutput = false,
            OmitXmlDeclaration = _root.Declaration is null
        });

        var document = new XDocument(_root);
        document.Root?.DescendantsAndSelf().Attributes("usens").Remove();
        document.Save(writer);
        writer.Flush();

        return encoding is UnicodeEncoding
            ? Encoding.Unicode.GetString(memoryStream.ToArray())
            : Encoding.UTF8.GetString(memoryStream.ToArray());
    }

  //  public static implicit operator string(XBuilder xml) => xml.ToString(false);

    private XElement? ResolveElement(string tagName, object? attributes)
    {
        XElement? element = null;

        if (UsePropertyNamespacing && tagName.Contains('_'))
        {
            var split = tagName.Split('_');
            var namespaceName = split[0];
            var localName = split[1];
            if (_root.Root is not null)
            {
                var nsAttribute = _root.Root.Attributes().FirstOrDefault(x => x.Name.LocalName == namespaceName);
                if (nsAttribute is not null)
                {
                    XNamespace ns = nsAttribute.Value;
                    element = new XElement(ns + localName);
                }
            }
        }
        else if (UsePropertyNamespacing)
        {
            var cursor = _current as XElement;
            string? namespaceName = null;
            while (cursor is not null && namespaceName is null)
            {
                namespaceName = cursor.Attribute("usens")?.Value;
                cursor = cursor.Parent;
            }

            if (namespaceName is not null && _root.Root is not null)
            {
                var nsAttribute = _root.Root.Attributes().FirstOrDefault(x => x.Name.LocalName == namespaceName);
                if (nsAttribute is not null)
                {
                    XNamespace ns = nsAttribute.Value;
                    element = new XElement(ns + tagName);
                }
            }
        }

        if (element is null && RemoveEmptyXmlnsOnOutput && attributes is not null && _current is XElement parent && parent.Name.Namespace != XNamespace.None)
        {
            var hasExplicitXmlns = attributes.GetType().GetProperties().Any(p => p.Name == "xmlns");
            if (!hasExplicitXmlns)
                element = new XElement(parent.Name.Namespace + tagName);
        }

        return element;
    }

    private void AddAttributes(XElement element, string tagName, object attributes)
    {
        foreach (var property in attributes.GetType().GetProperties())
        {
            var value = property.GetValue(attributes, null);
            if (property.Name == "xmlns")
            {
                var xmlns = value as string;
                if (!string.IsNullOrEmpty(xmlns))
                    element.Name = XName.Get(element.Name.LocalName, xmlns);
            }
            else if (property.Name == "xmlnsprefix" && value is XNamespace xNamespace)
            {
                element.Name = xNamespace.GetName(tagName);
            }
            else if (property.Name.StartsWith("xmlns_", StringComparison.Ordinal) && UsePropertyNamespacing)
            {
                var xmlns = value as string;
                var split = property.Name.Split('_');
                if (!string.IsNullOrEmpty(xmlns))
                    element.Add(new XAttribute(XNamespace.Xmlns + split[1], xmlns));
            }
            else if (value is not null)
            {
                element.Add(new XAttribute(property.Name, value));
            }
        }
    }
}