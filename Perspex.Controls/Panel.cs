﻿// -----------------------------------------------------------------------
// <copyright file="Panel.cs" company="Steven Kirk">
// Copyright 2014 MIT Licence. See licence.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Perspex.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using Perspex.Collections;

    /// <summary>
    /// Base class for controls that can contain multiple children.
    /// </summary>
    public class Panel : Control, ILogical, IItemsPanel
    {
        private Controls children;

        private ILogical childLogicalParent;

        public Panel()
        {
            this.childLogicalParent = this;
        }

        public Controls Children
        {
            get
            {
                if (this.children == null)
                {
                    this.children = new Controls();
                    this.children.CollectionChanged += this.ChildrenChanged;
                }

                return this.children;
            }

            set
            {
                Contract.Requires<ArgumentNullException>(value != null);

                if (this.children != value)
                {
                    if (this.children != null)
                    {
                        this.ClearLogicalParent(this.children);
                        this.children.CollectionChanged -= this.ChildrenChanged;
                    }

                    this.children = value;
                    this.ClearVisualChildren();

                    if (this.children != null)
                    {
                        this.children.CollectionChanged += this.ChildrenChanged;
                        this.AddVisualChildren(value);
                        this.SetLogicalParent(value);
                        this.InvalidateMeasure();
                    }
                }
            }
        }

        IPerspexReadOnlyList<ILogical> ILogical.LogicalChildren
        {
            get { return this.children; }
        }

        ILogical IItemsPanel.ChildLogicalParent
        {
            get { return this.childLogicalParent; }
            set { this.childLogicalParent = value; }
        }

        private void ClearLogicalParent(IEnumerable<Control> controls)
        {
            foreach (var control in controls)
            {
                control.Parent = null;
            }
        }

        private void SetLogicalParent(IEnumerable<Control> controls)
        {
            foreach (var control in controls)
            {
                control.Parent = this.childLogicalParent as Control;
            }
        }

        private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: Handle Replace.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.SetLogicalParent(e.NewItems.OfType<Control>());
                    this.AddVisualChildren(e.NewItems.OfType<Visual>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    this.ClearLogicalParent(e.OldItems.OfType<Control>());
                    this.RemoveVisualChildren(e.OldItems.OfType<Visual>());
                    break;

                case NotifyCollectionChangedAction.Reset:
                    this.ClearLogicalParent(e.OldItems.OfType<Control>());
                    this.ClearVisualChildren();
                    this.AddVisualChildren(this.children);
                    break;
            }

            this.InvalidateMeasure();
        }
    }
}
