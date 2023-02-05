using Mati36.Vinyl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEngine;

internal class CategoryTreeView : TreeViewWithTreeModel<CategoryTreeElement>
{
    public CategoryTreeView(TreeViewState state, TreeModel<CategoryTreeElement> model) : base(state, model)
    {
        showAlternatingRowBackgrounds = true;
        Reload();
    }

    protected override bool CanMultiSelect(TreeViewItem item)
    {
        return false;
    }

    protected override bool CanRename(TreeViewItem item)
    {
        return true;
    }

    protected override void RenameEnded(RenameEndedArgs args)
    {
        if (!args.acceptedRename) return;

        var element = treeModel.Find(args.itemID);
        element.name = args.newName;
        VinylSerializationUtility.RenameCategory(element.vinylCategory, args.newName);
        Reload();
    }

    public event Action<int> e_OnDoubleClickedItem;
    protected override void DoubleClickedItem(int id)
    {
        e_OnDoubleClickedItem?.Invoke(id);
    }

    protected override void ContextClickedItem(int id)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddDisabledItem(new GUIContent(treeModel.Find(id).vinylCategory.name));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Add Child Category"), false, AddNewChildCategory, id);
        menu.AddItem(new GUIContent("Remove Category"), false, RemoveCategory, id);
        menu.ShowAsContext();
        Event.current.Use();
    }

    protected override void ContextClicked()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add Base Category"), false, AddNewBaseCategory);
        menu.ShowAsContext();
        Event.current.Use();
    }

    private void AddNewChildCategory(object data)
    {
        int parentId = (int)data;
        CategoryTreeElement parentElement = treeModel.Find(parentId);
        var parent = parentElement.vinylCategory;
        var newCat = VinylSerializationUtility.CreateCategory("New Category", parentId == -1 ? null : parent as VinylCategory);

        treeModel.AddElement(new CategoryTreeElement(newCat, newCat.GetInstanceID(), parentElement.depth + 1), parentElement, parentElement.hasChildren ? parentElement.children.Count : 0);

        Reload();
    }

    private void AddNewBaseCategory()
    {
        var root = treeModel.Find(rootItem.id);
        var newCat = VinylSerializationUtility.CreateCategory("New Category", null);
        treeModel.AddElement(new CategoryTreeElement(newCat, newCat.GetInstanceID(), 0), root, root.children.Count);
        Reload();
    }

    private void RemoveCategory(object data)
    {
        int id = (int)data;
        RemoveCategory(id);
    }

    private void RemoveCategory(int id)
    {
        CategoryTreeElement element = treeModel.Find(id);
        var category = element.vinylCategory;
        int opt = VinylSerializationUtility.RemoveCategory(category);
        if (opt == 1) return;
        if (opt == 2)
            treeModel.MoveElements(element.parent, element.hasChildren ? element.children.Count : 0, new List<TreeElement>(element.children));
        treeModel.RemoveElements(new List<CategoryTreeElement>() { element });
        Reload();
    }

    protected override void KeyEvent()
    {
        if (state.selectedIDs.Count == 0) return;
        var e = Event.current;
        if (e.isKey && e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
        {
            RemoveCategory(state.selectedIDs[0]);
            e.Use();
        }
    }

    //DRAG & DROP
    public override void OnDropDraggedElementsAtIndex(List<TreeViewItem> draggedRows, CategoryTreeElement parent, int insertIndex)
    {
        base.OnDropDraggedElementsAtIndex(draggedRows, parent, insertIndex);

        foreach (var row in draggedRows)
        {
            var element = treeModel.Find(row.id);
            if (parent.vinylCategory != null &&
                parent.vinylCategory.Childs.Contains(element.vinylCategory)) continue;

            //OLD PARENT REMOVE CHILD
            var oldParent = element.vinylCategory.Parent;
            if (oldParent != null)
            {
                var serializedExParent = new SerializedObject(oldParent);
                var oldParentChilds = serializedExParent.FindProperty("_childs");
                for (int i = 0; i < oldParentChilds.arraySize; i++)
                {
                    var oldParentChild = oldParentChilds.GetArrayElementAtIndex(i);
                    if (oldParentChild.objectReferenceValue == element.vinylCategory)
                    {
                        oldParentChild.objectReferenceValue = null;
                        oldParentChilds.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
                serializedExParent.ApplyModifiedProperties();
            }
            else
            {
                VinylSerializationUtility.RemoveBaseCategory(element.vinylCategory);
            }

            //CHILD REPARENT
            var serializedChild = new SerializedObject(element.vinylCategory);
            serializedChild.FindProperty("_parent").objectReferenceValue = parent.vinylCategory;
            serializedChild.ApplyModifiedProperties();

            //NEW PARENT ADD CHILD
            if (parent.vinylCategory != null)
            {
                var serializedParent = new SerializedObject(parent.vinylCategory);
                var childsProperty = serializedParent.FindProperty("_childs");

                childsProperty.InsertArrayElementAtIndex(insertIndex);
                serializedParent.ApplyModifiedProperties();
                childsProperty.GetArrayElementAtIndex(insertIndex).objectReferenceValue = element.vinylCategory;
                serializedParent.ApplyModifiedProperties();
            }
            else
            {
                VinylSerializationUtility.AddBaseCategory(element.vinylCategory);
            }
        }
    }
}

public class CategoryTreeElement : TreeElement
{
    [SerializeField] public VinylCategory vinylCategory;

    public CategoryTreeElement(VinylCategory category, int id, int depth)
    {
        vinylCategory = category;
        name = category.name;
        this.id = id;
        this.depth = depth;
    }

    public CategoryTreeElement(string name, int id, int depth)
    {
        this.name = name;
        this.id = id;
        this.depth = depth;
    }
}