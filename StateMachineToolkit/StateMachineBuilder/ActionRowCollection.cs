using System;
using System.Collections;
using System.ComponentModel;

namespace Sanford.StateMachineToolkit.StateMachineBuilder
{
    /// <summary>
    /// Summary description for ActionRowCollection.
    /// </summary>
    public class ActionRowCollection : CollectionBase, IBindingList
    {
        /// <summary>
        /// Adds the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public int Add(ActionRow row)
        {
            return List.Add(row);
        }

        /// <summary>
        /// Adds the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public int Add(string name)
        {
            ActionRow newRow = new ActionRow();

            newRow.Name = name;

            return Add(newRow);
        }

        /// <summary>
        /// Performs additional custom processes when clearing the contents of the <see cref="T:System.Collections.CollectionBase"/> instance.
        /// </summary>
        protected override void OnClear()
        {
            foreach (ActionRow row in List)
            {
                row.EditCancelled -= editCancelledHandler;
            }

            base.OnClear();
        }

        /// <summary>
        /// Performs additional custom processes after clearing the contents of the <see cref="T:System.Collections.CollectionBase"/> instance.
        /// </summary>
        protected override void OnClearComplete()
        {
            onListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));

            base.OnClearComplete();
        }

        /// <summary>
        /// Performs additional custom processes after inserting a new element into the <see cref="T:System.Collections.CollectionBase"/> instance.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert <paramref name="value"/>.</param>
        /// <param name="value">The new value of the element at <paramref name="index"/>.</param>
        protected override void OnInsertComplete(int index, object value)
        {
            ActionRow newRow = (ActionRow) value;

            newRow.EditCancelled += editCancelledHandler;

            onListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));

            base.OnInsertComplete(index, value);
        }

        /// <summary>
        /// Performs additional custom processes after removing an element from the <see cref="T:System.Collections.CollectionBase"/> instance.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/> can be found.</param>
        /// <param name="value">The value of the element to remove from <paramref name="index"/>.</param>
        protected override void OnRemoveComplete(int index, object value)
        {
            ActionRow oldRow = (ActionRow) value;

            oldRow.EditCancelled -= editCancelledHandler;

            onListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));

            base.OnRemoveComplete(index, value);
        }

        /// <summary>
        /// Performs additional custom processes after setting a value in the <see cref="T:System.Collections.CollectionBase"/> instance.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="oldValue"/> can be found.</param>
        /// <param name="oldValue">The value to replace with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The new value of the element at <paramref name="index"/>.</param>
        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            if (oldValue != newValue)
            {
                ActionRow oldRow = (ActionRow) oldValue;
                ActionRow newRow = (ActionRow) newValue;

                oldRow.EditCancelled -= editCancelledHandler;
                newRow.EditCancelled += editCancelledHandler;

                onListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            }

            base.OnSetComplete(index, oldValue, newValue);
        }

        private void onListChanged(ListChangedEventArgs e)
        {
            ListChangedEventHandler handler = ListChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void editCancelledHandler(object sender, EventArgs e)
        {
            List.Remove(sender);
        }

        /// <summary>
        /// Gets or sets the <see cref="Sanford.StateMachineToolkit.StateMachineBuilder.ActionRow"/> at the specified index.
        /// </summary>
        /// <value></value>
        public ActionRow this[int index]
        {
            get { return (ActionRow) List[index]; }
            set { List[index] = value; }
        }

        #region IBindingList Members

        /// <summary>
        /// Occurs when the list changes or an item in the list changes.
        /// </summary>
        public event ListChangedEventHandler ListChanged;

        /// <summary>
        /// Adds a new item to the list.
        /// </summary>
        /// <returns>The item added to the list.</returns>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.AllowNew"/> is false.
        /// </exception>
        public object AddNew()
        {
            ActionRow newRow = new ActionRow();

            List.Add(newRow);

            return newRow;
        }

        /// <summary>
        /// Adds the <see cref="T:System.ComponentModel.PropertyDescriptor"/> to the indexes used for searching.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to add to the indexes used for searching.</param>
        public void AddIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sorts the list based on a <see cref="T:System.ComponentModel.PropertyDescriptor"/> and a <see cref="T:System.ComponentModel.ListSortDirection"/>.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to sort by.</param>
        /// <param name="direction">One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values.</param>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false.
        /// </exception>
        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor"/>.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to search on.</param>
        /// <param name="key">The value of the <paramref name="property"/> parameter to search for.</param>
        /// <returns>
        /// The index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSearching"/> is false.
        /// </exception>
        public int Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes any sort applied using <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false.
        /// </exception>
        public void RemoveSort()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes the <see cref="T:System.ComponentModel.PropertyDescriptor"/> from the indexes used for searching.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor"/> to remove from the indexes used for searching.</param>
        public void RemoveIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets whether you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew"/>.
        /// </summary>
        /// <value></value>
        /// <returns>true if you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew"/>; otherwise, false.
        /// </returns>
        public bool AllowNew
        {
            get { return true; }
        }

        /// <summary>
        /// Gets whether you can update items in the list.
        /// </summary>
        /// <value></value>
        /// <returns>true if you can update the items in the list; otherwise, false.
        /// </returns>
        public bool AllowEdit
        {
            get { return true; }
        }

        /// <summary>
        /// Gets whether you can remove items from the list, using <see cref="M:System.Collections.IList.Remove(System.Object)"/> or <see cref="M:System.Collections.IList.RemoveAt(System.Int32)"/>.
        /// </summary>
        /// <value></value>
        /// <returns>true if you can remove items from the list; otherwise, false.
        /// </returns>
        public bool AllowRemove
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the <see cref="T:System.ComponentModel.PropertyDescriptor"/> that is being used for sorting.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The <see cref="T:System.ComponentModel.PropertyDescriptor"/> that is being used for sorting.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false.
        /// </exception>
        public PropertyDescriptor SortProperty
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets whether a <see cref="E:System.ComponentModel.IBindingList.ListChanged"/> event is raised when the list changes or an item in the list changes.
        /// </summary>
        /// <value></value>
        /// <returns>true if a <see cref="E:System.ComponentModel.IBindingList.ListChanged"/> event is raised when the list changes or when an item changes; otherwise, false.
        /// </returns>
        public bool SupportsChangeNotification
        {
            get { return true; }
        }

        /// <summary>
        /// Gets whether the list supports sorting.
        /// </summary>
        /// <value></value>
        /// <returns>true if the list supports sorting; otherwise, false.
        /// </returns>
        public bool SupportsSorting
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)"/> method.
        /// </summary>
        /// <value></value>
        /// <returns>true if the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)"/> method; otherwise, false.
        /// </returns>
        public bool SupportsSearching
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether the items in the list are sorted.
        /// </summary>
        /// <value></value>
        /// <returns>true if <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)"/> has been called and <see cref="M:System.ComponentModel.IBindingList.RemoveSort"/> has not been called; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false.
        /// </exception>
        public bool IsSorted
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the direction of the sort.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        ///     <see cref="P:System.ComponentModel.IBindingList.SupportsSorting"/> is false.
        /// </exception>
        public ListSortDirection SortDirection
        {
            get { throw new NotSupportedException(); }
        }

        #endregion
    }
}