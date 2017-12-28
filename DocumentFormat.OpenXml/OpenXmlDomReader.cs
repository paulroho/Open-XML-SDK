﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DocumentFormat.OpenXml
{
    /// <summary>
    /// Represents the Open XML document reader class.
    /// </summary>
    public class OpenXmlDomReader : OpenXmlReader
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static ReadOnlyCollection<OpenXmlAttribute> _emptyReadOnlyList;

        private OpenXmlElement _rootElement;
        private Stack<OpenXmlElement> _elementStack;
        private ElementState _elementState;

        private OpenXmlDomReader()
            : base()
        {
            this._elementStack = new Stack<OpenXmlElement>();
            this._elementState = ElementState.Null;
        }

        private OpenXmlDomReader(bool readMiscNodes)
            : base(readMiscNodes)
        {
            this._elementStack = new Stack<OpenXmlElement>();
            this._elementState = ElementState.Null;
        }

        /// <summary>
        /// Initializes a new instance of the OpenXmlDomReader class.
        /// </summary>
        /// <param name="openXmlElement">The OpenXmlElement to read.</param>
        public OpenXmlDomReader(OpenXmlElement openXmlElement) : this()
        {
            if (openXmlElement == null)
            {
                throw new ArgumentNullException(nameof(openXmlElement));
            }

            this.Init(openXmlElement);
        }

        /// <summary>
        /// Initializes a new instance of the OpenXmlDomReader class using the supplied OpenXmlElement and Boolean values.
        /// </summary>
        /// <param name="openXmlElement">The OpenXmlElement to read.</param>
        /// <param name="readMiscNodes">Specify false to indicate to the reader to skip all miscellaneous nodes. The default value is false.</param>
        public OpenXmlDomReader(OpenXmlElement openXmlElement, bool readMiscNodes)
            : this(readMiscNodes)
        {
            if (openXmlElement == null)
            {
                throw new ArgumentNullException(nameof(openXmlElement));
            }

            this.Init(openXmlElement);
        }

        /// <summary>
        /// Gets the list of attributes of the current element.
        /// </summary>
        public override ReadOnlyCollection<OpenXmlAttribute> Attributes
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNull();
                ThrowIfEof();

                if (this._elementState == ElementState.Start)
                {
                    Debug.Assert(this._elementStack.Count > 0);

                    OpenXmlElement element = this._elementStack.Peek();

                    return new ReadOnlyCollection<OpenXmlAttribute>(element.GetAttributes());
                }
                else
                {
                    if (_emptyReadOnlyList == null)
                    {
                        _emptyReadOnlyList = new ReadOnlyCollection<OpenXmlAttribute>(new List<OpenXmlAttribute>());
                    }
                    return _emptyReadOnlyList;
                }
            }
        }

        /// <summary>
        /// Gets the namespace declarations of the current element.
        /// </summary>
        public override IEnumerable<KeyValuePair<string, string>> NamespaceDeclarations
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNull();
                ThrowIfEof();

                if (this._elementState == ElementState.Start)
                {
                    Debug.Assert(this._elementStack.Count > 0);

                    OpenXmlElement element = this._elementStack.Peek();

                    return element.NamespaceDeclarations;
                }
                else
                {
                    return Enumerable.Empty<KeyValuePair<string, string>>();
                }
            }
        }

        /// <summary>
        /// Gets the type of the corresponding strong typed class of the current element.
        /// </summary>
        public override Type ElementType
        {
           get
            {
                ThrowIfObjectDisposed();
                ThrowIfNull();
                ThrowIfEof();
                return this._elementStack.Peek().GetType();
            }
        }

        ///// <summary>
        ///// When overridden in a derived class, gets a value indicating whether the current node is an empty element (for example, <MyElement/>).
        ///// </summary>
        //public abstract bool IsEmptyElement { get; }

        /// <summary>
        /// Gets a value that indicates whether the current node is a miscellaneous XML node (non element).
        /// </summary>
        /// <remarks>IsStartElement and IsEndElement will be false when IsMiscNode==true.</remarks>
        public override bool IsMiscNode
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNull();
                ThrowIfEof();

                if (this._elementState == ElementState.MiscNode)
                {
                    Debug.Assert(this.ElementType == typeof(OpenXmlMiscNode));
                    return true;
                }
                else
                {
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the current node is an element start.
        /// </summary>
        /// <remarks>IsStartElement and IsEndElement will be false when IsMiscNode==true.</remarks>
        public override bool IsStartElement
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNull();
                ThrowIfEof();

                if (!IsMiscNode)
                {
                    if (this._elementState == ElementState.Start || this._elementState == ElementState.LeafStart)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the current node is an element end.
        /// </summary>
        /// <remarks>IsStartElement and IsEndElement will be false when IsMiscNode==true.</remarks>
        public override bool IsEndElement
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNull();
                ThrowIfEof();

                if (!IsMiscNode)
                {
                    if (this._elementState == ElementState.End)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the depth of the current node in the XML document. The depth of the root element is 0.
        /// </summary>
        public override int Depth
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNull();
                ThrowIfEof();

                Debug.Assert(this._elementStack.Count > 0);
                return this._elementStack.Count - 1;  // depth of root element is 0
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the reader is positioned at the end of the stream.
        /// </summary>
        public override bool EOF
        {
            get
            {
                ThrowIfObjectDisposed();
                //Debug.Assert(this._elementStack.Count == 0);

                return this._elementState == ElementState.EOF;
            }
        }

        /// <summary>
        /// Gets the local name of the current node.
        /// </summary>
        public override string LocalName
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNull();
                ThrowIfEof();
                return this._elementStack.Peek().LocalName;
            }
        }

        /// <summary>
        /// Gets the namespace URI (as defined in the W3C Namespace specification) of the node on which the reader is positioned.
        /// </summary>
        public override string NamespaceUri
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNull();
                ThrowIfEof();
                return this._elementStack.Peek().NamespaceUri;
            }
        }

        ///// <summary>
        ///// When overridden in a derived class, gets the qualified name of the current node.
        ///// </summary>
        //public virtual string Name { get; }

        /// <summary>
        /// Gets the namespace prefix associated with the current node.
        /// </summary>
        public override string Prefix
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNull();
                ThrowIfEof();
                return this._elementStack.Peek().Prefix;
            }
        }

        /// <summary>
        /// Moves to read the next element.
        /// </summary>
        /// <returns>Returns true if the next element was read successfully; false if there are no more elements to read. </returns>
        public override bool Read()
        {
            ThrowIfObjectDisposed();
            bool result = MoveToNextElement();

            if (result && !ReadMiscNodes)
            {
                // skip miscellaneous node
                while (result && this.IsMiscNode)
                {
                    result = MoveToNextElement();
                }
            }

            return result;
        }

        /// <summary>
        /// Moves to read the first child element.
        /// </summary>
        /// <returns>Returns true if the first child element was read successfully; false if there are no child elements to read. </returns>
        /// <remarks>This method can only be called on element start. At the current node, the reader will move to the end tag if there is no child element.</remarks>
        public override bool ReadFirstChild()
        {
            ThrowIfObjectDisposed();
            bool result = MoveToFirstChild();

            if (result && !ReadMiscNodes)
            {
                // skip miscellaneous node
                while (result && this.IsMiscNode)
                {
                    result = MoveToNextSibling();
                }
            }

            return result;
        }

        /// <summary>
        /// Moves to read the next sibling element.
        /// </summary>
        /// <returns>Returns true if the next sibling element was read successfully; false if there are no more sibling elements to read. </returns>
        /// <remarks>At the current node, the reader will move to the end tag of the parent if there are no more sibling elements.</remarks>
        public override bool ReadNextSibling()
        {
            ThrowIfObjectDisposed();
            bool result = MoveToNextSibling();

            if (result && !ReadMiscNodes)
            {
                // skip miscellaneous node
                while (result && this.IsMiscNode)
                {
                    result = MoveToNextSibling();
                }
            }

            return result;
        }

        /// <summary>
        /// Skips the children of the current node.
        /// </summary>
        public override void Skip()
        {
            ThrowIfObjectDisposed();
            this.InnerSkip();

            if (!this.EOF && !ReadMiscNodes)
            {
                // skip miscellaneous node
                while (!this.EOF && this.IsMiscNode)
                {
                    this.InnerSkip();
                }
            }
        }

#region private methods

        /// <summary>
        /// Move to next element
        /// </summary>
        /// <returns>true if the next element was read successfully; false if there are no more elements to read. </returns>
        private bool MoveToNextElement()
        {
            if (this._elementState == ElementState.Null)
            {
                return this.ReadRoot();
            }

            OpenXmlElement element;

            switch (this._elementState)
            {
                case ElementState.EOF:
                    return false;

                case ElementState.Start:
                    {
                        element = this._elementStack.Peek();
                        if (element.HasChildren)
                        {
                            this._elementStack.Push(element.FirstChild);
                            if (element.FirstChild is OpenXmlMiscNode)
                            {
                                this._elementState = ElementState.MiscNode;
                            }
                            else
                            {
                                this._elementState = ElementState.Start;
                            }
                        }
                        else
                        {
                            // at start state
                            this._elementState = ElementState.End;
                        }
                    }
                    break;

                case ElementState.End:
                case ElementState.MiscNode:
                    {
                        // at end state, find next element
                        element = this._elementStack.Pop();

                        if (this._elementStack.Count > 0)
                        {
                            element = element.NextSibling();
                            if (element != null)
                            {
                                this._elementStack.Push(element);

                                if (element is OpenXmlMiscNode)
                                {
                                    this._elementState = ElementState.MiscNode;
                                }
                                else
                                {
                                    this._elementState = ElementState.Start;
                                }
                            }
                            else
                            {
                                this._elementState = ElementState.End;
                            }
                        }
                        else
                        {
                            Debug.Assert(element == this._rootElement);
                            // no more elements
                            this._elementState = ElementState.EOF;
                            return false;
                        }
                    }
                    break;

                default:
                    Debug.Assert(this._elementState == ElementState.Start ||
                        this._elementState == ElementState.End ||
                        this._elementState == ElementState.EOF);
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Move to first child
        /// </summary>
        /// <returns>true if the first child element was read successfully; false if there are no child elements to read. </returns>
        /// <remarks>Only can be called on element start. Current will move to the end tag if no child element.</remarks>
        private bool MoveToFirstChild()
        {
            ThrowIfNull();

            if (this._elementState != ElementState.Start)
            {
                return false;
            }

            OpenXmlElement element = this._elementStack.Peek();
            if (element.HasChildren)
            {
                this._elementStack.Push(element.FirstChild);
                if (element.FirstChild is OpenXmlMiscNode)
                {
                    this._elementState = ElementState.MiscNode;
                }
                else
                {
                    this._elementState = ElementState.Start;
                }
                return true;
            }
            else
            {
                this._elementState = ElementState.End;
                return false;
            }
        }

        /// <summary>
        /// Move to  next sibling element
        /// </summary>
        /// <returns>true if the next sibling element was read successfully; false if there are no more sibling elements to read. </returns>
        /// <remarks>Current will move to the end tag of the parent if no more sibling element.</remarks>
        private bool MoveToNextSibling()
        {
            if (this._elementState == ElementState.EOF)
            {
                return false;
            }

            ThrowIfNull();

            if (this._elementStack.Count == 0)
            {
                this._elementState = ElementState.EOF;
                return false;
            }

            OpenXmlElement element = this._elementStack.Pop();

            // Fix bug #253890, case: the element used to constructor this DOM reader is not root element ( aka. it has parents and siblings. )
            // the stack is empty means we should move to EOF.
            if (this._elementStack.Count == 0)
            {
                this._elementState = ElementState.EOF;
                return false;
            }

            element = element.NextSibling();
            if (element != null)
            {
                this._elementStack.Push(element);
                if (element is OpenXmlMiscNode)
                {
                    this._elementState = ElementState.MiscNode;
                }
                else
                {
                    this._elementState = ElementState.Start;
                }
                return true;
            }
            else
            {
                if (this._elementStack.Count > 0)
                {
                    this._elementState = ElementState.End;
                }
                else
                {
                    // no more element, EOF
                    this._elementState = ElementState.EOF;
                }
                return false;
            }
        }

        /// <summary>
        /// Skips the children of the current node.
        /// </summary>
        private void InnerSkip()
        {
            switch (this._elementState)
            {
                case ElementState.Null:
                    ThrowIfNull();
                    return;

                case ElementState.EOF:
                    return;

                case ElementState.End:
                case ElementState.MiscNode:
                    this.MoveToNextElement();
                    return;

                case ElementState.Start:
                   this.MoveToNextSibling();
                   return;

                default:
                    Debug.Assert(this._elementState == ElementState.Start ||
                        this._elementState == ElementState.End ||
                        this._elementState == ElementState.EOF);
                    return;
            }
        }

#endregion

        /// <summary>
        /// Loads the element at the current cursor.
        /// </summary>
        /// <returns>The OpenXmlElement object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the current element is element end.</exception>
        /// <remarks>The new current element is the end of the element after LoadCurrentElement().</remarks>
        public override OpenXmlElement LoadCurrentElement()
        {
            // TODO: should we return a clone?
            // The user expect the element in the tree returned? Is this assumption correct?
            ThrowIfObjectDisposed();
            ThrowIfNull();
            ThrowIfEof();

            if (this._elementState == ElementState.Start)
            {
                OpenXmlElement element = this._elementStack.Peek();

                this._elementState = ElementState.End;

                return element;
            }
            else if (this._elementState == ElementState.MiscNode)
            {
                Debug.Assert(this.ReadMiscNodes);

                OpenXmlElement element = this._elementStack.Peek();

                this.Skip();

                return element;
            }
            else
            {
                throw new InvalidOperationException(ExceptionMessages.ReaderInEndState);
            }
        }

        ///// <summary>
        ///// Create an empty element of the element at current cursor.
        ///// </summary>
        ///// <returns>An empty ( no children, no attributes ) OpenXmlElement.</returns>
        //public override OpenXmlElement CreateElement()
        //{
        //    if (this._elementState == ElementState.Start ||
        //        this._elementState == ElementState.MiscNode)
        //    {
        //        OpenXmlElement element = this._elementStack.Peek();
        //        OpenXmlElement newElement = element.CloneNode(false);
        //        newElement.ClearAllAttributes();
        //        return newElement;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException();
        //    }
        //}

        /// <summary>
        /// Gets the text of the element if the element is an OpenXmlLeafTextElement. Returns String.Empty for other elements.
        /// </summary>
        /// <returns>
        /// The text of the element if the element is an OpenXmlLeafTextElement. Returns String.Empty for other elements.
        /// </returns>
        public override string GetText()
        {
            ThrowIfObjectDisposed();
            if (this._elementState == ElementState.Start)
            {
                OpenXmlElement element = this._elementStack.Peek();

                OpenXmlLeafTextElement textElement = element as OpenXmlLeafTextElement;

                if (textElement != null)
                {
                    return textElement.Text;
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Closes the reader.
        /// </summary>
        public override void Close()
        {
            ThrowIfObjectDisposed();
            this._elementState = ElementState.EOF;
            this._elementStack.Clear();
            this._rootElement = null;
        }

#region private methods

        private void Init(OpenXmlElement openXmlElement)
        {
            this._rootElement = openXmlElement;
            this._elementState = ElementState.Null;
        }

        private bool ReadRoot()
        {
            this._elementStack.Push(this._rootElement);
            if (this._rootElement is OpenXmlMiscNode)
            {
                this._elementState = ElementState.MiscNode;
            }
            else
            {
                this._elementState = ElementState.Start;
            }
            return true;
        }

        private void ThrowIfNull()
        {
            if (this._elementState == ElementState.Null)
            {
                throw new InvalidOperationException(ExceptionMessages.ReaderInNullState);
            }
        }

        private void ThrowIfEof()
        {
            if ( this._elementState == ElementState.EOF || this._elementStack.Count <= 0)
            {
                throw new InvalidOperationException(ExceptionMessages.ReaderInEofState);
            }
        }

#endregion
    }
}
