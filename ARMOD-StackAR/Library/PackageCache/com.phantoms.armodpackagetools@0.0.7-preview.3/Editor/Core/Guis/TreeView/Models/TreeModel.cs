using System;
using System.Collections.Generic;
using System.Linq;

namespace  com.Phantoms.ARMODPackageTools.Core
{
    // The TreeModel is a utility class working on a list of serializable TreeElements where the order and the depth of each TreeElement define
    // the tree structure. Note that the TreeModel itself is not serializable (in Unity we are currently limited to serializing lists/arrays) but the 
    // input list is.
    // The tree representation (parent and children references) are then build internally using TreeElementUtility.ListToTree (using depth 
    // values of the elements). 
    // The first element of the input list is required to have depth == -1 (the hiddenroot) and the rest to have
    // depth >= 0 (otherwise an exception will be thrown)

    public class TreeModel<T> where T : TreeElementModel
    {
        IList<T> data;
        T root;
        int maxId;

        public T GetRoot => root;
        public event Action ModelChanged;

        public int numberOfDataElements => data?.Count ?? 0;

        public TreeModel(IList<T> _data)
        {
            SetData(_data);
        }

        public T Find(int _id)
        {
            return data.FirstOrDefault(_element => _element.Id == _id);
        }

        public void SetData(IList<T> _data)
        {
            Init(_data);
        }

        void Init(IList<T> _data)
        {
            data = _data ?? throw new ArgumentNullException("_data",
                       "Input data is null. Ensure input is a non-null list.");
            if (data.Count > 0)
                root = TreeElementUtility.ListToTree(_data);


            //maxId = data.Max(_e => _e.id);
        }

        public int GenerateUniqueId()
        {
            return ++maxId;
        }

        public IList<int> GetAncestors(int _id)
        {
            var parents = new List<int>();
            TreeElementModel T = Find(_id);
            if (T != null)
            {
                while (T.Parent != null)
                {
                    parents.Add(T.Parent.Id);
                    T = T.Parent;
                }
            }

            return parents;
        }

        public IList<int> GetDescendantsThatHaveChildren(int _id)
        {
            T searchFromThis = Find(_id);
            if (searchFromThis != null)
            {
                return GetParentsBelowStackBased(searchFromThis);
            }

            return new List<int>();
        }

        IList<int> GetParentsBelowStackBased(TreeElementModel _searchFromThis)
        {
            Stack<TreeElementModel> stack = new Stack<TreeElementModel>();
            stack.Push(_searchFromThis);

            var parentsBelow = new List<int>();
            while (stack.Count > 0)
            {
                TreeElementModel current = stack.Pop();
                if (current.HasChildren)
                {
                    parentsBelow.Add(current.Id);
                    foreach (var T in current.Children)
                    {
                        stack.Push(T);
                    }
                }
            }

            return parentsBelow;
        }

        public void RemoveElements(IList<int> _elementIDs)
        {
            IList<T> elements = data.Where(_element => _elementIDs.Contains(_element.Id)).ToArray();
            RemoveElements(elements);
        }

        public void RemoveElements(IList<T> _elements)
        {
            foreach (var element in _elements)
                if (element == root)
                    throw new ArgumentException("It is not allowed to remove the GetRoot element");

            var commonAncestors = TreeElementUtility.FindCommonAncestorsWithinList(_elements);

            foreach (var element in commonAncestors)
            {
                element.Parent.Children.Remove(element);
                element.Parent = null;
            }

            TreeElementUtility.TreeToList(root, data);

            Changed();
        }

        public void AddElements(IList<T> _elements, TreeElementModel _parent, int _insertPosition)
        {
            if (_elements == null)
                throw new ArgumentNullException("_elements", "elements is null");
            if (_elements.Count == 0)
                throw new ArgumentNullException("_elements", "elements Count is 0: nothing to add");
            if (_parent == null)
                throw new ArgumentNullException("_parent", "parent is null");

            if (_parent.Children == null)
                _parent.Children = new List<TreeElementModel>();

            _parent.Children.InsertRange(_insertPosition, _elements.Cast<TreeElementModel>());
            foreach (var element in _elements)
            {
                element.Parent = _parent;
                element.Depth = _parent.Depth + 1;
                TreeElementUtility.UpdateDepthValues(element);
            }

            TreeElementUtility.TreeToList(root, data);

            Changed();
        }

        public void AddRoot(T _root)
        {
            if (_root == null)
                throw new ArgumentNullException("_root", "GetRoot is null");

            if (data == null)
                throw new InvalidOperationException("Internal Error: data list is null");

            if (data.Count != 0)
                throw new InvalidOperationException("AddRoot is only allowed on empty data list");

            _root.Id = GenerateUniqueId();
            _root.Depth = -1;
            data.Add(_root);
        }

        public void AddElement(T _element, TreeElementModel _parent, int _insertPosition)
        {
            if (_element == null)
                throw new ArgumentNullException("_element", "element is null");
            if (_parent == null)
                throw new ArgumentNullException("_parent", "parent is null");

            if (_parent.Children == null)
                _parent.Children = new List<TreeElementModel>();

            _parent.Children.Insert(_insertPosition, _element);
            _element.Parent = _parent;

            TreeElementUtility.UpdateDepthValues(_parent);
            TreeElementUtility.TreeToList(root, data);

            Changed();
        }

        public void MoveElements(TreeElementModel _parentElementModel, int _insertionIndex, List<TreeElementModel> _elements)
        {
            if (_insertionIndex < 0)
                throw new ArgumentException(
                    "Invalid input: insertionIndex is -1, client needs to decide what index elements should be reparented at");

            // Invalid reparenting input
            if (_parentElementModel == null)
                return;

            // We are moving items so we adjust the insertion index to accomodate that any items above the insertion index is removed before inserting
            if (_insertionIndex > 0)
                _insertionIndex -= _parentElementModel.Children.GetRange(0, _insertionIndex).Count(_elements.Contains);

            // Remove draggedItems from their parents
            foreach (var draggedItem in _elements)
            {
                draggedItem.Parent.Children.Remove(draggedItem); // remove from old parent
                draggedItem.Parent = _parentElementModel; // set new parent
            }

            if (_parentElementModel.Children == null)
                _parentElementModel.Children = new List<TreeElementModel>();

            // Insert dragged items under new parent
            _parentElementModel.Children.InsertRange(_insertionIndex, _elements);

            TreeElementUtility.UpdateDepthValues(GetRoot);
            TreeElementUtility.TreeToList(root, data);

            Changed();
        }

        void Changed()
        {
            ModelChanged?.Invoke();
        }
    }
}