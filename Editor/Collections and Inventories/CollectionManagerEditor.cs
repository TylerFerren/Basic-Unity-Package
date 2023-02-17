using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codesign.Collections;
using Sirenix;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class CollectionManagerEditor : EditorWindow
{
    public VisualTreeAsset visualTree;

    Toolbar toolbar;
    ToolbarButton collectionsButton;
    ToolbarButton attributeButton;
    ToolbarButton settingsButton;

    VisualElement collectionsView;
    VisualElement attributeView;
    VisualElement settingsView;

    [MenuItem("Tools/Collection Manager")]
    public static void ShowWindow() {
        var window = GetWindow<CollectionManagerEditor>();
        window.titleContent = new GUIContent("Collection Manager");
        window.minSize = new Vector2(300, 300);
    }

    public void OnEnable()
    {
        TemplateContainer template = visualTree.Instantiate();
        template.style.flexGrow = 1;
        rootVisualElement.Add(template);

        ManagerMenu();

        collectionsView = rootVisualElement.Query<VisualElement>("Collections");

        //CollectionsDrawer();
    }

    public void CreateGUI()
    {

    }

    public void ManagerMenu() {
         toolbar = rootVisualElement.Query<Toolbar>("MainToolbar");

        collectionsButton = toolbar.Query<ToolbarButton>("CollectionsMenuButton");
        attributeButton = toolbar.Query<ToolbarButton>("AttributeMenuButton");
        settingsButton = toolbar.Query<ToolbarButton>("SettingsMenuButton");

        collectionsButton.clicked += CollectionsDrawer;
        attributeButton.clicked += AttributeDrawer;
        settingsButton.clicked += SettingsDrawer;
    }

    

    public void CollectionsDrawer() {
        collectionsView.style.display = DisplayStyle.Flex;
        //gets a list of collections from the Assets Database

        //FindTypeOf(out Collection[] collections);

        //ListView collectionList = rootVisualElement.Query<ListView>("CollectionList").First();

        //collectionList.itemsSource = collections;
        //collectionList.makeItem = () => new Label();
        //collectionList.bindItem = (element, i) => (element as Label).text = collections[i].name.AddCamelCasingSpace();
        //collectionList.Rebuild();

        //collectionList.onSelectionChange += CollectionObjectSelectionDrawer;


        //ListView objectsTypes = rootVisualElement.Query<ListView>("ObjectsTypes").First();

        //var nestedClasses = typeof(CollectionObject).GetChildrenTypes().ToList();
        //nestedClasses.Add(typeof(CollectionObject));

        //objectsTypes.itemsSource = nestedClasses;
        //objectsTypes.makeItem = () => new Label();
        //objectsTypes.bindItem = (element, i) => (element as Label).text = nestedClasses[i].Name.AddCamelCasingSpace() + "s";
        //objectsTypes.Rebuild();

        //objectsTypes.onSelectionChange += TypeObjectSelectionDrawer;
    }

    public void AttributeDrawer() {
        collectionsView.style.display = DisplayStyle.None;
    }

    public void SettingsDrawer() {
        collectionsView.style.display = DisplayStyle.None;
    }

    private void CollectionObjectSelectionDrawer(IEnumerable<object> Enumerable) {
        foreach (object it in Enumerable)
        {
            Collection collection = it as Collection;
            ListView collectionObjects = rootVisualElement.Query<ListView>("CollectionsObjects");

            collectionObjects.itemsSource = collection.collectionObjects.Keys.ToList();
            collectionObjects.makeItem = () => new Label();
            collectionObjects.bindItem = (element, i) => (element as Label).text = collection.collectionObjects.Keys.ElementAt(i).name;
            

            collectionObjects.Rebuild();
            
            collectionObjects.onSelectionChange += (Enumerable) => { ObjectInspectorDrawer(collectionObjects); };
        }


    }

    private void TypeObjectSelectionDrawer(IEnumerable<object> Enumerable) {
        foreach (object it in Enumerable)
        {
            Type type = it as Type;
            ListView collectionObjects = rootVisualElement.Query<ListView>("CollectionsObjects");
            FindTypeOf(out CollectionObject[] objects);

            collectionObjects.itemsSource = objects;
            collectionObjects.makeItem = () => new Label();
            collectionObjects.bindItem = (element, i) => (element as Label).text = objects[i].name.AddCamelCasingSpace();
            collectionObjects.Rebuild();

            collectionObjects.onSelectionChange += (Enumerable) => { ObjectInspectorDrawer(collectionObjects); };
        }

    } 

    private void ObjectInspectorDrawer( ListView collectionObjects) {
        IMGUIContainer SelectedObjectContainer = rootVisualElement.Query<IMGUIContainer>("SelectedObject");
        var selectedObject = collectionObjects.selectedItem as CollectionObject;
        var selectionEditor = Editor.CreateEditor(selectedObject);
        SelectedObjectContainer.onGUIHandler = () => selectionEditor.DrawDefaultInspector();
        
    }

    private void FindTypeOf<T>(out T[] types) where T: UnityEngine.Object{
        types = new T[0];
        var guids = AssetDatabase.FindAssets("t:" + types.GetType().ToString());
        types = new T[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            types[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
