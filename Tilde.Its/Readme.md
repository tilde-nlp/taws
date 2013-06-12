Tilde.Its
=========

This project is a class library for parsing ITS 2.0 annotated XML and HTML documents.

ItsDocument
-----------

`ItsDocument` represents an ITS 2.0 annotated document. It loads and parses the document, 
finds associated global rules and provides a way to quickly annotate the document.

`ItsXmlDocument` represents an XML document.

    using Tilde.Its;

    // creates a new document from a file
    ItsXmlDocument doc = new ItsXmlDocument(uri: "document.xml");
	
	// creates a new document from a string
	ItsXmlDocument doc = new ItsXmlDocument(xml: "<root/>");
	
	// if you create a new document from a string and it has external rules with relative paths,
	// specify the base path for these rules
	ItsXmlDocument doc = new ItsXmlDocument("<root><its:rules ... xlink:href='rules.xml'/></root>", 
		uri: "http://example.com"); // will load rules from http://examples.com/rules.xml
    
	// clone an existing document
    ItsXmlDocument doc2 = new ItsXmlDocument(doc);

	// parsed document
	XDocument xmlDoc = doc.Document;
	string xml = doc.Document.ToString();

`ItsHtmlDocument` represents an HTML document. It supports HTML5 and XHTML.
It has the same constructors and behavior as `ItsXmlDocument`.

Data Categories
---------------

Each data category is represented by a class that inherits from DataCategory. 
The constructor of these classes accepts two arguments: `ItsDocument` which contains the global rules 
and the node (element or attribute) to annotate.

	ItsHtmlDocument doc = new ItsHtmlDocument("<html><body translate=no>0x01 0x02 0x03</body></html>");
	XElement html = doc.Document.Root;
	XElement body = html.Element(ItsHtmlDocument.XhtmlNamespace + "body");
	XAttribute bodyAttribute = body.Attribute("translate");
    
	TranslateDataCategory htmlTranslate = new TranslateDataCategory(doc, html);
	Assert.IsTrue(htmlTranslate.IsTranslatable); // default value

	TranslateDataCategory bodyTranslate = new TranslateDataCategory(doc, body);
	Assert.IsFalse(bodyTranslate.IsTranslatable); // local value

	TranslateDataCategory bodyAttributeTranslate = new TranslateDataCategory(doc, bodyAttribute);
	Assert.IsFalse(bodyAttributeTranslate.IsTranslatable); // default value

`annotatorsRef` is not a data category but can be used in a similar way.

    AnnotatorAnnotation annotators = new AnnotatorsAnnotation(doc, element);
	annotators.AnnotatorRef; // terminology|http://1 text-analysis|http://2
	annotators["terminology"]; // AnnotatorsRef with "terminology" and "http://1"
	// annotators is IEnumerable<AnnotatorsRef>

Annotation
----------

`System.Xml.Linq.XObject` (which `XElement` and `XAttribute` inherit from) supports adding annotations.

    html.AddAnnotation(new TranslateDataCategory(doc, html));
    TranslateDataCategory htmlTranslate = html.Annotation<TranslateDataCategory>();

`ItsDocument` takes advantage of this feature and provides a quick way to annotate all
elements and attributes in the document.

    // finds all data categories in Tilde.Its
	// and annotates all elements and attributes in the document
    doc.AnnotateAll();

	// finds all data categories in Tilde.Its
	// and annotates the html element and its attributes
	doc.AnnotateAll(html);

	// annotates the html element and its attributes with the Translate data category
	doc.Annotate<TranslateDataCategory>(html);

	// annotates all elements and attributes in the document with the Translate data category
	doc.Annotate<TranslateDataCategory>(doc.Document.Descendants());

Before adding an annotation, the previous annotations of the same type are removed. So you can use
`Annotate*()` methods to reannotate as well.

When an annotation is added, its value is not computed. Its value is computed lazily.

    // our example
    string html = "<html><body><p></p></body></html>";

    // adds all data categories to all nodes
	// no computations
	// very fast!
    doc.AnnotateAll();

	Assert.IsTrue(body.Annotation<TranslateDataCategory>().IsTranslatable);
	// computes the value for <body>: no local, no global rules
	// computes the value for <html>: no local, no global rules
	// computes default value: true
	// <p> is never touched

Once the value has been computed for an annotation, it's cached.

    Assert.IsTrue(body.Annotation<TranslateDataCategory>().IsTranslatable);
	// already computed the value for <body> in the last example: true
	// (value for <html> has also been already computed)

Some data categories support inheritance. When a node is annotated and looks for the inherited value,
it will use the annotation on the parent element if there is one (if there is no annotation, 
it is not added). Thus, if the parent elements are already annotated, they don't have to be annotated again, 
and it improves performance.

### Overriding values ###

You can override the computed values for some data categories.

    doc.AnnotateAll();

	body.Annotation<TranslateDataCategory>().IsTranslatable = false;
	// <p>   : false   (will be computed and taken from <body>)
	// <body>: false   (already computed = overriden)
	// <html>: true    (will be computed)

However, if you override a value and the values for children have already been
computed, you will get incorrect results.

    doc.AnnotateAll();

	// computes values for all elements
	Assert.IsTrue(p.Annotation<TranslateDataCategory>().IsTranslatable);

	// override the value
	body.Annotation<TranslateDataCategory>().IsTranslatable = false;
	// <p>   : true    (already computed in Assert.IsTrue)   <- INCORRECT
	// <body>: false   (already computed = overriden)
	// <html>: true    (already computed in Assert.IsTrue)

When you override a value, you should reannotate all descendants to avoid such situations.

    doc.AnnotateAll();

	// computes values for all elements
	Assert.IsTrue(p.Annotation<TranslateDataCategory>().IsTranslatable);

	// override the value
	body.Annotation<TranslateDataCategory>().IsTranslatable = false;
	// reannotate descendants
    doc.Annotate<TranslateDataCategory>(body.Descendants());

	// <p>   : false   (will be computed and taken from <body>)
	// <body>: false   (already computed = overriden)
	// <html>: true    (already computed in Assert.IsTrue)
