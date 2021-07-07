using System;
using System.Collections.Generic;

namespace com.Phantoms.ARMODPackageTools.Core
{
    // TreeElementUtility and TreeElement are useful helper classes for backend tree data structures.
    // See tests at the bottom for examples of how to use.

    public static class TreeElementUtility
    {
        public static void TreeToList<T>(T _root, IList<T> _result) where T : TreeElementModel
        {
            if (_result == null)
                throw new NullReferenceException("The input 'IList<T> result' list is null");
            _result.Clear();

            var tmp_Stack = new Stack<T>();
            tmp_Stack.Push(_root);

            while (tmp_Stack.Count > 0)
            {
                var tmp_Current = tmp_Stack.Pop();
                _result.Add(tmp_Current);

                if (tmp_Current.Children == null || tmp_Current.Children.Count <= 0) continue;
                for (var tmp_Index = tmp_Current.Children.Count - 1; tmp_Index >= 0; tmp_Index--)
                {
                    tmp_Stack.Push((T) tmp_Current.Children[tmp_Index]);
                }
            }
        }

        // Returns the GetRoot of the tree parsed from the list (always the first element).
        // Important: the first item and is required to have a depth value of -1. 
        // The rest of the items should have depth >= 0. 
        public static T ListToTree<T>(IList<T> _list) where T : TreeElementModel
        {
            // Validate input
            ValidateDepthValues(_list);

            // Clear old states
            foreach (var tmp_Element in _list)
            {
                tmp_Element.Parent = null;
                tmp_Element.Children = null;
            }

            // Set child and parent references using depth info
            for (int tmp_ParentIndex = 0; tmp_ParentIndex < _list.Count; tmp_ParentIndex++)
            {
                var tmp_Parent = _list[tmp_ParentIndex];
                bool tmp_AlreadyHasValidChildren = tmp_Parent.Children != null;
                if (tmp_AlreadyHasValidChildren)
                    continue;

                int tmp_ParentDepth = tmp_Parent.Depth;
                int tmp_ChildCount = 0;

                // Count children based depth value, we are looking at children until it's the same depth as this object
                for (int tmp_Index = tmp_ParentIndex + 1; tmp_Index < _list.Count; tmp_Index++)
                {
                    if (_list[tmp_Index].Depth == tmp_ParentDepth + 1)
                        tmp_ChildCount++;
                    if (_list[tmp_Index].Depth <= tmp_ParentDepth)
                        break;
                }

                // Fill child array
                List<TreeElementModel> tmp_ChildList = null;
                if (tmp_ChildCount != 0)
                {
                    tmp_ChildList = new List<TreeElementModel>(tmp_ChildCount); // Allocate once
                    tmp_ChildCount = 0;
                    for (int tmp_Index = tmp_ParentIndex + 1; tmp_Index < _list.Count; tmp_Index++)
                    {
                        if (_list[tmp_Index].Depth == tmp_ParentDepth + 1)
                        {
                            _list[tmp_Index].Parent = tmp_Parent;
                            tmp_ChildList.Add(_list[tmp_Index]);
                            tmp_ChildCount++;
                        }

                        if (_list[tmp_Index].Depth <= tmp_ParentDepth)
                            break;
                    }
                }

                tmp_Parent.Children = tmp_ChildList;
            }

            return _list[0];
        }

        // Check state of input list
        private static void ValidateDepthValues<T>(IList<T> _list) where T : TreeElementModel
        {
            if (_list.Count == 0)
                throw new ArgumentException(
                    "list should have items, count is 0, check before calling ValidateDepthValues", nameof(_list));

            if (_list[0].Depth != -1)
                throw new ArgumentException(
                    "list item at index 0 should have a depth of -1 (since this should be the hidden GetRoot of the tree). Depth is: " +
                    _list[0].Depth, nameof(_list));

            for (int tmp_Index = 0; tmp_Index < _list.Count - 1; tmp_Index++)
            {
                int tmp_Depth = _list[tmp_Index].Depth;
                int tmp_NextDepth = _list[tmp_Index + 1].Depth;
                if (tmp_NextDepth > tmp_Depth && tmp_NextDepth - tmp_Depth > 1)
                    throw new ArgumentException(
                        $"Invalid depth info in input list. Depth cannot increase more than 1 per row. Index {tmp_Index} has depth {tmp_Depth} while index {tmp_Index + 1} has depth {tmp_NextDepth}");
            }

            for (int tmp_Index = 1; tmp_Index < _list.Count; ++tmp_Index)
                if (_list[tmp_Index].Depth < 0)
                    throw new ArgumentException("Invalid depth value for item at index " + tmp_Index +
                                                ". Only the first item (the GetRoot) should have depth below 0.");

            if (_list.Count > 1 && _list[1].Depth != 0)
                throw new ArgumentException("Input list item at index 1 is assumed to have a depth of 0",
                    nameof(_list));
        }


        // For updating depth values below any given element e.g after reparenting elements
        public static void UpdateDepthValues<T>(T _root) where T : TreeElementModel
        {
            if (_root == null)
                throw new ArgumentNullException(nameof(_root), "The GetRoot is null");

            if (!_root.HasChildren)
                return;

            Stack<TreeElementModel> tmp_Stack = new Stack<TreeElementModel>();
            tmp_Stack.Push(_root);
            while (tmp_Stack.Count > 0)
            {
                TreeElementModel tmp_Current = tmp_Stack.Pop();
                if (tmp_Current.Children == null) continue;
                foreach (var tmp_Child in tmp_Current.Children)
                {
                    tmp_Child.Depth = tmp_Current.Depth + 1;
                    tmp_Stack.Push(tmp_Child);
                }
            }
        }

        // Returns true if there is an ancestor of child in the elements list
        static bool IsChildOf<T>(T _child, IList<T> _elements) where T : TreeElementModel
        {
            while (_child != null)
            {
                _child = (T) _child.Parent;
                if (_elements.Contains(_child))
                    return true;
            }

            return false;
        }

        public static IList<T> FindCommonAncestorsWithinList<T>(IList<T> _elements) where T : TreeElementModel
        {
            if (_elements.Count == 1)
                return new List<T>(_elements);

            var tmp_Result = new List<T>(_elements);
            tmp_Result.RemoveAll(_g => IsChildOf(_g, _elements));
            return tmp_Result;
        }
    }
}