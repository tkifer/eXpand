﻿using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.NodeWrappers;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Utils;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using eXpand.ExpressApp.SystemModule;
using GridListEditor = DevExpress.ExpressApp.Win.Editors.GridListEditor;
using System.Linq;
using NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition;
using DevExpress.XtraGrid.Columns;

namespace eXpand.ExpressApp.Win.SystemModule
{
    public partial class GridViewViewController : BaseViewController
    {
        public const string AllowExpandEmptyDetails = "AllowExpandEmptyDetails";
        public const string AutoExpandNewRow = "AutoExpandNewRow";
        public const string AutoSelectAllInEditorAttributeName = "AutoSelectAllInEditor";
        public const string DoNotLoadWhenNoFilterExists = "DoNotLoadWhenNoFilterExists";
        public const string EditorShowModeAttributeName = "EditorShowMode";
        public const string EnterMoveNextColumn = "EnterMoveNextColumn";
        public const string AutoFilterCondition = "AutoFilterCondition";
        public const string GuessAutoFilterRowValuesFromFilter = "GuessAutoFilterRowValuesFromFilter";
        public const string ImmediateUpdateAutoFilter = "ImmediateUpdateAutoFilter";
        public const string ExtraSerializationProperties = "ExtraSerializationProperties";
        public const string GroupLevelExpandIndex = "GroupLevelExpandIndex";
        public const string HideFieldCaptionOnGroup = "HideFieldCaptionOnGroup";
        public const string IsColumnHeadersVisible = "IsColumnHeadersVisible";
        //        public const string NewItemRowPositionAttributeName = "NewItemRowPosition";
        public const string SerializeFilterAttributeName = "SerializeFilter";
        public const string UseTabKey = "UseTabKey";


        private GridControl gridControl;
        private GridView mainView;
        private ListViewInfoNodeWrapper model;
        private bool newRowAdded;
        private DevExpress.ExpressApp.SystemModule.FilterController filterController;

        //        private XPDictionary xpDictionary;


        public GridViewViewController()
        {
            InitializeComponent();
            RegisterActions(components);
            TargetViewType = ViewType.ListView;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            View.ControlsCreated += View_OnControlsCreated;

            model = new ListViewInfoNodeWrapper(View.Info);
        }

        ///<summary>
        ///
        ///<para>
        ///Updates the Application Model.
        ///
        ///</para>
        ///
        ///</summary>
        ///
        ///<param name="dictionary">
        ///		A <b>Dictionary</b> object that provides access to the Application Model's nodes.
        ///
        ///            </param>
        public override void UpdateModel(Dictionary dictionary)
        {
            base.UpdateModel(dictionary);
            var wrappers = new ApplicationNodeWrapper(dictionary).Views.Items.Where(wrapper => wrapper is ListViewInfoNodeWrapper);
            foreach (ListViewInfoNodeWrapper wrapper in wrappers)
            {
                wrapper.Node.SetAttribute(IsColumnHeadersVisible, true.ToString());
                wrapper.Node.SetAttribute(UseTabKey, true.ToString());
                wrapper.Node.SetAttribute(GuessAutoFilterRowValuesFromFilter, true.ToString());

                foreach (ColumnInfoNodeWrapper column in wrapper.Columns.Items.Where(wrapper2 => wrapper2.PropertyTypeInfo.Type == typeof(string)))
                {
                    column.Node.SetAttribute(AutoFilterCondition, DevExpress.XtraGrid.Columns.AutoFilterCondition.Contains.ToString());
                    column.Node.SetAttribute(ImmediateUpdateAutoFilter, false.ToString());
                }
            }
        }

        private void View_OnControlsCreated(object sender, EventArgs e)
        {
            gridControl = View.Control as GridControl;
            if (gridControl == null)
                return;
            gridControl.HandleCreated += GridControl_OnHandleCreated;


            mainView = gridControl.MainView as GridView;
            if (mainView != null)
            {
                mainView.FocusedRowChanged += GridView_OnFocusedRowChanged;
                mainView.InitNewRow += GridView_OnInitNewRow;
                mainView.ShownEditor += MainViewOnShownEditor;
                SetOptions(mainView, model);
            }


            if (View.Info.GetAttributeBoolValue(DoNotLoadWhenNoFilterExists, false) &&
                ((GridView)gridControl.MainView).FilterPanelText ==
                string.Empty)
            {
                if (mainView != null) mainView.ActiveFilter.Changed += ActiveFilter_OnChanged;

                filterController = Frame.GetController<DevExpress.ExpressApp.SystemModule.FilterController>();
                filterController.FullTextFilterAction.Execute += FullTextFilterAction_Execute;
                SetDoNotLoadWhenFilterExistsCriteria();
            }
        }

        private void FullTextFilterAction_Execute(object sender, DevExpress.ExpressApp.Actions.ParametrizedActionExecuteEventArgs e)
        {
            if (string.IsNullOrEmpty(e.ParameterCurrentValue as string))
                SetDoNotLoadWhenFilterExistsCriteria();
            else
                ClearDoNotLoadWhenFilterExistsCriteria();
        }

        private void MainViewOnShownEditor(object sender, EventArgs args)
        {
            var view = (GridView)sender;
            if (view.IsFilterRow(view.FocusedRowHandle))
                view.ActiveEditor.Properties.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
        }


        private void SetDoNotLoadWhenFilterExistsCriteria() {
            var memberInfo = View.ObjectTypeInfo.KeyMember;
            var memberType = memberInfo.MemberType;
            var o = memberType.IsValueType ? Activator.CreateInstance(memberType) : null;
            ((ListView) View).CollectionSource.Criteria[DoNotLoadWhenNoFilterExists] = new BinaryOperator(memberInfo.Name,o);
        }

        private void ClearDoNotLoadWhenFilterExistsCriteria()
        {
            ((ListView)View).CollectionSource.Criteria[DoNotLoadWhenNoFilterExists] = null;
        }

        private void ActiveFilter_OnChanged(object sender, EventArgs e)
        {
            if (((GridView)gridControl.MainView).FilterPanelText !=
                string.Empty)
                ClearDoNotLoadWhenFilterExistsCriteria();
            else
                SetDoNotLoadWhenFilterExistsCriteria();
        }


        /*
                [Obsolete("only for 8.1")]
                private void MainView_OnCustomDrawGroupRow(object sender, CustomDrawObjectEventArgs e)
                {
                    var gridGroupRowInfo = (GridGroupRowInfo) e.Info;

                    DictionaryNode childNodeByPath = Application.Info.GetChildNodeByPath(
                        "Views\\ListView[@ID='" + View.Id + "']\\Columns\\ColumnInfo[@PropertyName='" +
                        gridGroupRowInfo.Column.FieldName.Replace("!", "") + "']");
                    string text = null;
                    var regexObj = new Regex(@"\(Count=([\d]*)");
                    string value = regexObj.Match(gridGroupRowInfo.GroupText).Groups[1].Value;
                    if (value == "")
                        return;
                    if (gridGroupRowInfo.EditValue is DateTime)
                    {
                        if (
                            !string.IsNullOrEmpty(
                                 childNodeByPath.GetAttributeValue(ColumnInfoNodeWrapper.DisplayFormatAttribute)))
                        {
                            regexObj = new Regex(@"\{0:([^}]*)");

                            string toString = ((DateTime) gridGroupRowInfo.EditValue).ToString(
                                regexObj.Match(childNodeByPath.GetAttributeValue(
                                                   ColumnInfoNodeWrapper.DisplayFormatAttribute)).Groups[1].Value);

                            text = gridGroupRowInfo.Column.Caption + ":[#image]" +
                                   toString + " (Count=" + value + ")";
                        }
                    }
                    if (childNodeByPath.GetAttributeBoolValue(HideFieldCaptionOnGroup))
                    {
                        if (text == null)
                            text = gridGroupRowInfo.GroupText;
                        gridGroupRowInfo.GroupText =
                            text.Replace(gridGroupRowInfo.Column.Caption + ":", "").Replace("Count=", "");
                    }
                }
        */

        private void GridView_OnFocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (newRowAdded && mainView.IsValidRowHandle(e.FocusedRowHandle))
            {
                newRowAdded = false;
                if (model.Node.GetAttributeBoolValue(AutoExpandNewRow))
                    mainView.ExpandMasterRow(e.FocusedRowHandle);
            }
        }

        ///<summary>
        ///
        ///<para>
        ///Returns the Schema extension which is combined with the entire Schema when loading the Application Model.
        ///
        ///</para>
        ///
        ///</summary>
        ///
        ///<returns>
        ///The <b>Schema</b> object that represents the Schema extension to be added to the application's entire Schema.
        ///
        ///</returns>
        ///
        public override Schema GetSchema()
        {
            string CommonTypeInfos =
                @"<Element Name=""Application"">
                    <Element Name=""Views"">
                        <Element Name=""ListView"" >
                            <Element Name=""Columns"">
                                <Element Name=""ColumnInfo"">
                                    <Attribute Name=""" + HideFieldCaptionOnGroup + @""" Choice=""True,False""/>
                                    <Attribute Name=""" + AutoFilterCondition + @""" Choice=""{" + typeof(AutoFilterCondition).FullName + @"}""/>
                                    <Attribute Name=""" + ImmediateUpdateAutoFilter + @""" Choice=""True,False""/>
                                </Element>  
                            </Element>
                            <Attribute Name=""" + IsColumnHeadersVisible + @""" Choice=""True,False""/>
                            <Attribute Name=""" + AllowExpandEmptyDetails + @""" Choice=""False,True""/>
                            <Attribute Name=""" + AutoExpandNewRow + @""" Choice=""False,True""/>
                            <Attribute Name=""" + EnterMoveNextColumn + @""" Choice=""False,True""/>
                            <Attribute Name=""" + ExtraSerializationProperties + @""" />
                            <Attribute Name=""" + GroupLevelExpandIndex + @""" Choice=""False,True""/>
                            <Attribute Name=""" + UseTabKey + @""" Choice=""False,True""/>
                            <Attribute Name=""" + AutoSelectAllInEditorAttributeName + @""" Choice=""False,True""/>
                            <Attribute Name=""" + SerializeFilterAttributeName + @""" Choice=""False,True""/>
                            <Attribute Name=""" + DoNotLoadWhenNoFilterExists + @""" Choice=""False,True""/>
                            <Attribute Name=""" + EditorShowModeAttributeName + @""" Choice=""{" + typeof(EditorShowMode).FullName + @"}""/>
                            <Attribute Name=""" + GuessAutoFilterRowValuesFromFilter + @""" Choice=""False,True""/>
                        </Element>
                    </Element>
                    </Element>";
            return new Schema(new DictionaryXmlReader().ReadFromString(CommonTypeInfos));
        }


        private void GridView_OnInitNewRow(object sender, InitNewRowEventArgs e)
        {
            newRowAdded = true;
        }


        public static void SetOptions(GridView gridView, ListViewInfoNodeWrapper listViewInfoNodeWrapper)
        {
            gridView.OptionsView.NewItemRowPosition = (NewItemRowPosition)Enum.Parse(typeof(NewItemRowPosition), new SupportNewItemRowNodeWrapper(listViewInfoNodeWrapper.Node).NewItemRowPosition.ToString());
            gridView.OptionsBehavior.EditorShowMode = EditorShowMode.Click;
            gridView.OptionsBehavior.Editable = true;
            gridView.OptionsBehavior.AllowIncrementalSearch = true;
            gridView.OptionsBehavior.AutoSelectAllInEditor = false;
            gridView.OptionsBehavior.AutoPopulateColumns = false;
            gridView.OptionsBehavior.FocusLeaveOnTab = true;
            gridView.OptionsBehavior.AutoExpandAllGroups = listViewInfoNodeWrapper.Node.GetAttributeBoolValue(GridListEditor.AutoExpandAllGroups, false);
            gridView.OptionsSelection.MultiSelect = true;
            gridView.OptionsSelection.EnableAppearanceFocusedCell = true;
            gridView.OptionsNavigation.AutoFocusNewRow = true;
            gridView.OptionsNavigation.AutoMoveRowFocus = true;
            gridView.OptionsView.ShowDetailButtons = false;
            gridView.OptionsDetail.EnableMasterViewMode = false;
            gridView.OptionsView.ShowIndicator = true;
            gridView.OptionsView.ShowGroupPanel = listViewInfoNodeWrapper.Node.GetAttributeBoolValue(GridListEditor.IsGroupPanelVisible, false);
            gridView.OptionsView.ShowFooter = listViewInfoNodeWrapper.Node.GetAttributeBoolValue(GridListEditor.IsFooterVisible, true);
            gridView.OptionsView.ShowAutoFilterRow = listViewInfoNodeWrapper.IsFilterPanelVisible;
            gridView.FocusRectStyle = DrawFocusRectStyle.RowFocus;
            gridView.ShowButtonMode = ShowButtonModeEnum.ShowOnlyInEditor;
            gridView.ActiveFilterEnabled = listViewInfoNodeWrapper.Node.GetAttributeBoolValue(GridListEditor.IsActiveFilterEnabled, true);

            gridView.OptionsDetail.AllowExpandEmptyDetails =
                listViewInfoNodeWrapper.Node.GetAttributeBoolValue(AllowExpandEmptyDetails,
                                                                   false);

            gridView.OptionsNavigation.EnterMoveNextColumn =
                listViewInfoNodeWrapper.Node.GetAttributeBoolValue(EnterMoveNextColumn,
                                                                   false);

            gridView.OptionsNavigation.UseTabKey = listViewInfoNodeWrapper.Node.GetAttributeBoolValue(UseTabKey,
                                                                                                      false);


            gridView.OptionsView.ShowColumnHeaders =
                listViewInfoNodeWrapper.Node.GetAttributeBoolValue(IsColumnHeadersVisible, true);
            //            DevExpress.XtraGrid.Views.Grid.NewItemRowPosition newItemRowPosition =
            //                listViewInfoNodeWrapper.Node.GetAttributeEnumValue(NewItemRowPositionAttributeName,
            //                                                                   DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.
            //                                                                       None);
            //            gridView.OptionsView.NewItemRowPosition = newItemRowPosition;
            gridView.OptionsBehavior.AutoSelectAllInEditor =
                listViewInfoNodeWrapper.Node.GetAttributeBoolValue(AutoSelectAllInEditorAttributeName,
                                                                   true);
            gridView.OptionsBehavior.EditorShowMode =
                listViewInfoNodeWrapper.Node.GetAttributeEnumValue(EditorShowModeAttributeName,
                                                                   EditorShowMode.MouseUp);
            gridView.OptionsView.ShowFooter = listViewInfoNodeWrapper.Node.GetAttributeBoolValue(GridListEditor.IsFooterVisible, true);

            SetColumnOptions(gridView, listViewInfoNodeWrapper);

            if (listViewInfoNodeWrapper.ShowAutoFilterRow && listViewInfoNodeWrapper.Node.GetAttributeBoolValue(GuessAutoFilterRowValuesFromFilter))
            {
                gridView.GuessAutoFilterRowValuesFromFilter();
            }
        }

        public static void SetColumnOptions(GridView gridView, ListViewInfoNodeWrapper listViewInfoNodeWrapper)
        {
            foreach (GridColumn column in gridView.Columns)
            {
                ColumnInfoNodeWrapper columnInfo = listViewInfoNodeWrapper.Columns.FindColumnInfo(column.FieldName.Replace("!", string.Empty));
                if (columnInfo != null)
                {
                    column.OptionsFilter.AutoFilterCondition = columnInfo.GetEnumValue<AutoFilterCondition>(AutoFilterCondition, column.OptionsFilter.AutoFilterCondition);
                    column.OptionsFilter.ImmediateUpdateAutoFilter = columnInfo.Node.GetAttributeBoolValue(ImmediateUpdateAutoFilter);
                }
            }
        }

        private void GridControl_OnHandleCreated(object sender, EventArgs e)
        {
            mainView.GridControl.ForceInitialize();

            string value = View.Info.GetAttributeValue(GroupLevelExpandIndex);
            if (!string.IsNullOrEmpty(value))
            {
                int int32 = Convert.ToInt32(value);
                if (mainView.GroupCount == int32)
                    for (int i = -1; ; i--)
                    {
                        if (!mainView.IsValidRowHandle(i)) return;
                        if (mainView.GetRowLevel(i) < int32 - 1)
                            mainView.SetRowExpanded(i, true);
                    }
            }
        }


    }
}