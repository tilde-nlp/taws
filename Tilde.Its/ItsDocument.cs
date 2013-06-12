using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// ITS 2.0 annotated document.
    /// </summary>
    public abstract class ItsDocument
    {
        /// <summary>
        /// ITS 2.0 namespace.
        /// <see href="http://www.w3.org/TR/its20/#notation"/>
        /// </summary>
        public static readonly XNamespace ItsNamespace = XNamespace.Get("http://www.w3.org/2005/11/its");
        /// <summary>
        /// XML Linking Language (XLink) namespace.
        /// <see href="http://www.w3.org/TR/xlink11/"/>
        /// </summary>
        public static readonly XNamespace XlinkNamespace = XNamespace.Get("http://www.w3.org/1999/xlink");
        /// <summary>
        /// MIME type for ITS documents.
        /// <see href="http://www.w3.org/TR/its20/#its-mime-type"/>
        /// </summary>
        public static readonly string ItsMimeType = "application/its+xml";

        /// <summary>
        /// XML element name for global rules.
        /// </summary>
        protected const string RulesElementName = "rules";
        /// <summary>
        /// XML attribute name for the global rules XML element.
        /// </summary>
        protected const string RulesElementVersionAttributeName = "version";
        /// <summary>
        /// Supported ITS version.
        /// </summary>
        protected const string Version = "2.0";
        /// <summary>
        /// XLink attribute name. The attribute value points to the external document.
        /// </summary>
        protected const string XlinkHrefAttributeName = "href";

        /// <summary>
        /// Creates a new empty document.
        /// </summary>
        protected ItsDocument()
        {
            Document = new XDocument();
            Rules = new List<XElement>();
        }

        /// <summary>
        /// Clones an existing document.
        /// Derived classes can make this constructor public and it will preserve the type.
        /// </summary>
        /// <param name="document">The document that will be copied.</param>
        protected ItsDocument(ItsDocument document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            Document = new XDocument(document.Document);

            Rules = new List<XElement>();
            foreach (XElement rule in document.Rules)
                Rules.Add(new XElement(rule));
        }

        /// <summary>
        /// Annotated document.
        /// </summary>
        public XDocument Document
        {
            get;
            protected set;
        }

        /// <summary>
        /// Ordered list of documents that contain global rules.
        /// Rules are in the order the documents they were in were loaded
        /// and then in the order they were in each document.
        /// </summary>
        public IList<XElement> Rules
        {
            get;
            private set;
        }

        #region Annotation
        /// <summary>
        /// Annotates all nodes (elements and their attributes, or individual attributes) in the document with all data categories and AnnotatorsRef.
        /// </summary>
        public void AnnotateAll()
        {
            AnnotateAll(Document.Root.DescendantsAndSelf());
        }

        /// <summary>
        /// Annotates a node (element and its attributes, or one attribute) with all data categories including AnnotatorsRef.
        /// </summary>
        /// <param name="node">Node to annotate.</param>
        public void AnnotateAll(XObject node)
        {
            Annotate(node, dataCategories);
        }

        /// <summary>
        /// Annotatates a list of nodes (elements and their attributes, or individual attributes) with all data categories including AnnotatorsRef.
        /// </summary>
        /// <param name="nodes">List of nodes to annotate.</param>
        public void AnnotateAll(IEnumerable<XObject> nodes)
        {
            foreach (XObject node in nodes)
            {
                AnnotateAll(node);
            }
        }

        /// <summary>
        /// Annotates a node (element and its attributes, or one attribute) with a specific list of data categories.
        /// </summary>
        /// <param name="node">Node to annotate.</param>
        /// <param name="dataCategories">Types of data categories.</param>
        public void Annotate(XObject node, params Type[] dataCategories)
        {
            foreach (Type type in dataCategories)
            {
                // remove previous annotation
                node.RemoveAnnotations(type);
                // create new annotation
                node.AddAnnotation(CreateInstance(type, this, node));

                if (node is XElement)
                {
                    Annotate(((XElement)node).Attributes(), type);
                }
            }
        }

        /// <summary>
        /// Annotates a list of nodes (elements and their attributes, or individual attributes) with a specific list of data categories.
        /// </summary>
        /// <param name="nodes">Nodes to annotate.</param>
        /// <param name="dataCategories">Types of data categories.</param>
        public void Annotate(IEnumerable<XObject> nodes, params Type[] dataCategories)
        {
            foreach (XObject node in nodes)
            {
                Annotate(node, dataCategories);
            }
        }

        /// <summary>
        /// Annotates all nodes (elements and their attributes, or individual attributes) in the document with the specified data category.
        /// </summary>
        /// <typeparam name="T">Data category type.</typeparam>
        public void Annotate<T>() where T : Annotation
        {
            Annotate<T>(Document.Root.DescendantsAndSelf());
        }

        /// <summary>
        /// Annotates a node (element and its attributes, or one attribute) with the specified data category.
        /// </summary>
        /// <typeparam name="T">Data category type.</typeparam>
        /// <param name="node">Node to annotate.</param>
        public void Annotate<T>(XObject node) where T : Annotation
        {
            Annotate(node, typeof(T));
        }

        /// <summary>
        /// Annotates a list of nodes (elements and their attributes, or individual attributes) with the specified data category.
        /// </summary>
        /// <typeparam name="T">Data category type.</typeparam>
        /// <param name="nodes">Nodes to annotate.</param>
        public void Annotate<T>(IEnumerable<XObject> nodes) where T : Annotation
        {
            foreach (XObject node in nodes)
            {
                Annotate<T>(node);
            }
        }
        #endregion

        /// <summary>
        /// Loads global rules from an XML document dedicated solely to ITS rules.
        /// If the rules are invalid, they are ignored.
        /// </summary>
        /// <param name="document">Document containing global rules.</param>
        /// <param name="uri">The path of the document that is used to load linked rules that don't have absolute URIs.</param>
        protected void LoadRules(XElement document, string uri = null)
        {
            if (document == null)
                return;
            // root must be <its:rules>
            if (document.Name != ItsNamespace + RulesElementName)
                return;
            // must have a version
            if (document.Attribute(RulesElementVersionAttributeName) == null ||
                document.Attribute(RulesElementVersionAttributeName).Value.Trim() != Version)
                return;

            // The rules contained in the referenced document MUST be processed as if they were at the top of the rules element with the XLink href attribute.
            XAttribute xlink = document.Attribute(XlinkNamespace + XlinkHrefAttributeName);
            if (xlink != null)
                LoadExternalRules(ResolveUri(uri, xlink.Value));

            Rules.Add(document);
        }

        /// <summary>
        /// Loads global rules from an external document.
        /// </summary>
        /// <param name="uri">The path to the external document.</param>
        protected void LoadExternalRules(string uri)
        {
            try
            {
                XDocument externalDocument = XDocument.Load(uri);
                // The referenced document must be a valid XML document containing at most one rules element. 
                // That rules element can be the root element or anywhere within the document tree (for example, the document could be an XML Schema).
                XElement rules = externalDocument.Descendants(ItsNamespace + RulesElementName).FirstOrDefault();
                LoadRules(rules, uri);
            }
            catch (Exception)
            {
                // ignore invalid xml
            }
        }

        /// <summary>
        /// Combines two URIs.
        /// Supports absolute and relative values.
        /// Supports file system paths as well as URIs.
        /// </summary>
        /// <param name="currentPath">Current abosolute/relative path/filename/URI.</param>
        /// <param name="file">Linked absolute/relative filename/URI.</param>
        /// <returns><paramref name="file"/> if it's an absolute value; otherwise <paramref name="currentPath"/> combined with <paramref name="file"/>.</returns>
        protected static string ResolveUri(string currentPath, string file)
        {
            if (currentPath == null && file == null)
                return null;
            if (currentPath == null)
                return file;
            if (file == null)
                return currentPath;

            // absolute path
            if (Path.IsPathRooted(file))
                return file;

            // absolute uri
            Uri fileUri;
            if (Uri.TryCreate(file, UriKind.RelativeOrAbsolute, out fileUri) && fileUri.IsAbsoluteUri)
                return file;

            // relative, combine with currentPath
            return Path.Combine(Path.GetDirectoryName(currentPath), file);
        }

        #region Data category creation using reflection
        /// <summary>
        /// All possible data categories and annotations defined in this assembly.
        /// </summary>
        private static readonly Type[] dataCategories = Assembly.GetAssembly(typeof(Annotation)).GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Annotation))).ToArray();
        /// <summary>
        /// Compiled creators (that are faster than Activator.CreateInstace) for each type.
        /// </summary>
        private static readonly Dictionary<Type, ObjectActivator> activators = new Dictionary<Type, ObjectActivator>();

        private object CreateInstance(Type type, params object[] args)
        {
            if (!activators.ContainsKey(type))
            {
                ConstructorInfo ctor = type.GetConstructors().First();
                activators[type] = GetActivator(ctor);
            }

            return activators[type](args);
        }

        private delegate object ObjectActivator(params object[] args);

        /// <see href="http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/"/>
        private static ObjectActivator GetActivator(ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression param =
                Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp =
                new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                Expression paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda =
                Expression.Lambda(typeof(ObjectActivator), newExp, param);

            //compile it
            ObjectActivator compiled = (ObjectActivator)lambda.Compile();
            return compiled;
        }
        #endregion
    }
}