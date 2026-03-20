using System.Dynamic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SemanaIA.ServiceInvoice.Infrastructure.Xml;


public class XBuilder : DynamicObject
{
    // Uses the System.Xml.Linq types for internally modeling the XML
    XDocument root = new XDocument();

    // holds the current container being worked on in cases of nesting
    XContainer current;

    /// <summary>
    /// Replace underscore _ by dash - when its set to true
    /// </summary>
    public bool UseDashInsteadUnderscore { get; set; }

    public bool UsePropertyNamespacing { get; set; }

    public bool RemoveEmptyXmlnsOnOutput { get; set; }



    /// <summary>
    /// Returns a lambda as a strongly-typed Action for use by lambda-accepting
    /// dynamic dispatch on Xml.  Not unequivalent to simply casting the same
    /// lambda when passing to Xml, except slightly cleaner syntax.  This is only
    /// necessary since dynamic calls cannot accept weakly-typed lambdas /sigh
    /// </summary>
    /// <param name="fragmentBuilder"></param>
    /// <returns>passed block, typed as an action</returns>
    public static Action Fragment(Action fragmentBuilder)
    {
        if (fragmentBuilder == null)
        {
            throw new ArgumentNullException("fragmentBuilder");
        }

        return fragmentBuilder;
    }

    /// <summary>
    /// Returns a lambda as a strongly-typed Generic Actions of type dynamic for
    /// use by the lambda-accepting dynamic dispatch on Xml.  Not unequivalent to
    /// simply casting the same lambda when passing to Xml, except slightly cleaner syntax
    /// This is only necessary since dynamic calls cannot accept weakly-typed lambdas /sigh
    /// </summary>
    /// <param name="fragmentBuilder"></param>
    /// <returns>passed lambda, typed as an Action&lt;dynamic&gt;</returns>
    public static Action<dynamic> Fragment(Action<dynamic> fragmentBuilder)
    {
        if (fragmentBuilder == null)
        {
            throw new ArgumentNullException("fragmentBuilder");
        }

        return fragmentBuilder;
    }

    /// <summary>
    /// Alternate syntax for generating an XML object via this static factory
    /// method instead of expliclty creating a "dynamic" in client code.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static XBuilder Build(Action<dynamic> builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var xbuilder = new XBuilder();
        builder(xbuilder);
        return xbuilder;
    }

    /// <summary>
    /// Constructs a new Dynamic XML Builder
    /// </summary>
    public XBuilder()
    {
        current = root;
    }

    /// <summary>
    /// Converts dynamically invoked method calls into nodes.
    /// example 1:  xml.hello("world") becomes <hello>world</hello>
    /// example 2:  xml.hello("world2", new { foo = "bar" }) becomes <hello foo="bar">world</hello>
    /// </summary>
    /// <param name="binder">invoke member binder</param>
    /// <param name="args">args</param>
    /// <param name="result">result (always true)</param>
    /// <returns></returns>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        result = null;
        string tagName = binder.Name;
        if (UseDashInsteadUnderscore) tagName = tagName.Replace("_", "-");
        Tag(tagName, args);
        return true;
    }

    /// <summary>
    /// Builds an XML node along with setting its inner content, attributes, and possibly nested nodes
    /// Usually No need to call this directly as it's mainly used as the implementation for dynamicaly invoked
    /// members on an XML instance.
    /// </summary>
    /// <param name="tagName">name for node tag</param>
    /// <param name="args">text content and/or attributes represented as an anonymous object
    /// and/or lambda for generating child nodes</param>
    public void Tag(string tagName, params object[] args)
    {
        try
        {
            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentNullException(nameof(tagName));
            }

            // allow for naming tags same as reserved Xml methods
            // like 'Comment' and 'CData' by prefixing "_"
            // escape character on tag name/method call
            if (tagName.IndexOf('_') == 0)
                tagName = tagName.Substring(1);

            string content = null;
            object attributes = null;
            XNode contentElement = null;
            Action fragment = null;

            // Analyze all the arguments passed
            args.ToList().ForEach(arg =>
            {
                if (arg == null)
                    return;

                // argument was a delegate for building child nodes
                if (arg is Action)
                    fragment = arg as Action;
                else if (arg is Action<dynamic>)
                    fragment = () => (arg as Action<dynamic>)(this);

                // argument was a string literal
                else if (arg is string)
                    content = arg as string;

                // argument was a XElement instance
                else if (arg is XNode)
                    contentElement = arg as XNode;

                // argument was a value type literal
                else if (arg.GetType().IsValueType)
                    content = arg.ToString();

                // otherwise, argument is considered to be an anonymous
                // object literal which will be reflected into node attributes
                else
                    attributes = arg;
            });

            // make a new element for this Tag() call
            XElement element = null;

            if (UsePropertyNamespacing && tagName.IndexOf('_') > 0)
            {
                var splited = tagName.Split('_');
                var namespaceName = splited[0];
                var localName = splited[1];
                if (root.Root != null)
                {
                    var nsAttribute =
                        root.Root.Attributes().FirstOrDefault(x => x.Name.LocalName.Equals(namespaceName));
                    if (nsAttribute != null)
                    {
                        XNamespace ns = nsAttribute.Value;
                        element = new XElement(ns + localName);
                    }
                }
            }
            else if (UsePropertyNamespacing)
            {
                var elementCursor = current as XElement;
                string namespaceName = null;

                while (elementCursor != null && namespaceName == null)
                {
                    var usensAttr = elementCursor.Attribute("usens");
                    if (usensAttr != null)
                    {
                        namespaceName = usensAttr.Value;
                    }
                    else
                    {
                        elementCursor = elementCursor.Parent;
                    }
                }

                if (namespaceName != null && root.Root != null)
                {
                    var nsAttribute =
                        root.Root.Attributes().FirstOrDefault(x => x.Name.LocalName.Equals(namespaceName));
                    if (nsAttribute != null)
                    {
                        XNamespace ns = nsAttribute.Value;
                        element = new XElement(ns + tagName);
                    }
                }
            }

            if (element == null)
            {
                bool hasExplicitXmlns =
                    attributes != null &&
                    attributes.GetType().GetProperties().Any(p => p.Name == "xmlns");

                if (RemoveEmptyXmlnsOnOutput &&
                    !hasExplicitXmlns &&
                    current is XElement parent &&
                    parent.Name.Namespace != XNamespace.None)
                {
                    element = new XElement(parent.Name.Namespace + tagName);
                }
            }


            if (element == null)
            {
                element = new XElement(tagName);
            }


            if (current == null)
            {
                current = root;
            }

            current.Add(element);

            // if a fragment delegate was passed for building inner nodes
            // capture this element as the new current outer parent
            if (contentElement != null)
            {
                element.Add(contentElement);
            }

            // if a fragment delegate was passed for building inner nodes
            // capture this element as the new current outer parent
            if (fragment != null)
            {
                current = element;
            }

            // add literal string content if there was any
            if (!string.IsNullOrEmpty(content))
            {
                element.Add(content);
            }

            // add attributes to the element if they were passed
            if (attributes != null)
            {
                attributes.GetType().GetProperties().ToList().ForEach(prop =>
                {
                    // if the attribute was named "xmlns", let's treat it
                    // like an actual xml namespace and do the right thing by
                    // applying it as a namespace to the element.
                    if (prop.Name == "xmlns")
                    {
                        var val = prop.GetValue(attributes, null) as string;
                        if (val != string.Empty)
                        {
                            element.Name = XName.Get(element.Name.LocalName, val);
                        }
                    }
                    else if (prop.Name == "xmlnsprefix")
                    {
                        var val = prop.GetValue(attributes, null) as XNamespace;
                        if (val != null)
                        {
                            element.Name = val.GetName(tagName);
                        }
                    }
                    // otherwise, just convert the property name/value to an attribute pair
                    // on the element
                    else if (prop.Name.StartsWith("xmlns_") && UsePropertyNamespacing)
                    {
                        var val = prop.GetValue(attributes, null) as string;
                        var splitedName = prop.Name.Split('_');
                        if (val != string.Empty)
                        {
                            element.Add(new XAttribute(XNamespace.Xmlns + splitedName[1], val));
                        }
                    }
                    else
                    {
                        element.Add(new XAttribute(prop.Name, prop.GetValue(attributes, null)));
                    }
                });
            }

            // if a fragment delegate was passed for building inner nodes
            // now go ahead and execute the delegate, and then set the current outer parent
            // node back to its original value
            if (fragment != null)
            {
                fragment();
                current = element.Parent;
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Add a literal comment to the XML
    /// </summary>
    /// <param name="comment">comment content</param>
    public void Comment(string comment)
    {
        if (string.IsNullOrEmpty(comment))
        {
            throw new ArgumentNullException(nameof(comment));
        }

        current.Add(new XComment(comment));
    }

    /// <summary>
    /// Add literal CData content to the XML
    /// </summary>
    /// <param name="data">data</param>
    public void CData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            throw new ArgumentNullException(nameof(data));
        }

        current.Add(new XCData(data));
    }

    /// <summary>
    /// Add a text node to the XML (not commonly needed)
    /// </summary>
    /// <param name="text">text content</param>
    public void Text(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentNullException(nameof(text));
        }

        current.Add(new XText(text));
    }

    /// <summary>
    /// Apply a declaration to the XML
    /// </summary>
    /// <param name="version">XML version</param>
    /// <param name="encoding">XML encoding (currently only supports utf-8 or utf-16)</param>
    /// <param name="standalone">"yes" or "no"</param>
    public void Declaration(string version = null, string encoding = null, string standalone = null)
    {
        root.Declaration = new XDeclaration(version, encoding, standalone);
    }

    /// <summary>
    /// Apply a document type to the XML
    /// </summary>
    /// <param name="name">name of the DTD</param>
    /// <param name="publicId">public identifier for the DTD</param>
    /// <param name="systemId">system identifier for the DTD</param>
    /// <param name="internalSubset">internal subset for the DTD</param>
    public void DocumentType(string name, string publicId = null, string systemId = null, string internalSubset = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException("name");
        }

        root.Add(new XDocumentType(name, publicId, systemId, internalSubset));
    }

    // Convertors for exporting as several useful .NET representations of the XML contnet

    #region converters

    /// <summary>
    /// Implicit conversion to non-indented xml content string
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public static implicit operator string(XBuilder xml)
    {
        return xml.ToString(false);
    }

    /// <summary>
    /// Converts the Xml content to a string
    /// </summary>
    /// <param name="indent">whether or not to indent the output</param>
    /// <returns></returns>
    public string ToString(bool indent)
    {
        // HACK justificiation:

        // The native XDocument.ToString() method never includes the XDeclaration (bug?)
        // Thus this below hack for manually writing the document to a stream.

        // Moreover, there's no straightforward/elegant way of getting the XmlWriter to respect
        // an XDeclaration's encoding property, so manually inspecting the prop's string content.

        // This current implementation limits an XDeclaration to either utf-8 or utf-16
        // but at least it gets us *that* far.

        // default to utf-8 encoding
        Encoding encoding = new UTF8Encoding(false);

        // if there was an explicit declaration that asked for utf-16, use UnicodeEncoding instead
        if (root.Declaration != null &&
            !string.IsNullOrEmpty(root.Declaration.Encoding) &&
            root.Declaration.Encoding.ToLowerInvariant() == "utf-16")
            encoding = new UnicodeEncoding(false, false);

        var memoryStream = new MemoryStream();

        var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
        {
            Encoding = encoding,
            Indent = indent,
            CloseOutput = true,
            // if "Declaration" not eplicitly set, don't include xml declaration
            OmitXmlDeclaration = root.Declaration == null
        });

        var doc = new XDocument(root);

        if (doc.Root != null)
        {
            foreach (var el in doc.Root.DescendantsAndSelf())
            {
                el.Attributes("usens").Remove();
            }
        }

        doc.Save(xmlWriter);
        xmlWriter.Flush();
        xmlWriter.Close();

        // convert the xml stream to a string with the proper encoding
        if (encoding is UnicodeEncoding)
            return Encoding.Unicode.GetString(memoryStream.ToArray());

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    /// <summary>
    /// Exports the Xml content as a Linq-queryable XDocument
    /// </summary>
    /// <returns>Linq-queryable XDocument</returns>
    public XDocument ToXDocument()
    {
        return root;
    }

    /// <summary>
    /// Exports the Xml content as a Linq-queryable XElement
    /// </summary>
    /// <returns>Linq-queryable XElement</returns>
    public XElement ToXElement()
    {
        return root.Elements().FirstOrDefault();
    }

    /// <summary>
    /// Exports the Xml content as a Linq-queryable XElement
    /// </summary>
    /// <returns>Linq-queryable XElement</returns>
    public IEnumerable<XElement> ToXElements()
    {
        return root.Elements();
    }

    /// <summary>
    /// Exports the Xml content as a standard XmlDocument
    /// </summary>
    /// <returns>XmlDocument</returns>
    public XmlDocument ToXmlDocument()
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(root.CreateReader());
        return xmlDoc;
    }

    /// <summary>
    /// Exports the Xml content as a standard XmlNode by returning the
    /// first node in the XDocument, excluding the DocumentType if it's set
    /// </summary>
    /// <returns>XmlNode</returns>
    public XmlNode ToXmlNode()
    {
        if (root.DocumentType != null && root.Nodes().Count() > 1)
            return ToXmlDocument().ChildNodes[1] as XmlNode;
        else if (root.DocumentType == null && root.Nodes().Count() >= 1)
            return ToXmlDocument().FirstChild as XmlNode;
        else
            return null as XmlNode;
    }

    /// <summary>
    /// Exports the Xml content as a standard XmlElement
    /// </summary>
    /// <returns>XmlElement</returns>
    public XmlElement ToXmlElement()
    {
        return ToXmlNode() as XmlElement;
    }

    #endregion
}
