using System.ComponentModel;
using DevExpress.CodeRush.Core;
using DevExpress.DXCore.Controls.XtraEditors.Controls;

namespace eXpandAddIns
{
    [UserLevel(UserLevel.Expert)]
    public partial class Options : OptionsPage {
        // DXCore-generated code...
        #region Initialize
        protected override void Initialize() {
            base.Initialize();

            DecoupledStorage storage = GetStorage();
            connectionStringName.Text = storage.ReadString(PageName, connectionStringName.Name, connectionStringName.Text);

            buttonEdit2.Text = storage.ReadString(PageName, "modelEditorPath", buttonEdit2.Text);
            buttonEdit1.Text = storage.ReadString(PageName, "projectConverterPath", buttonEdit1.Text);
            textEdit1.Text = storage.ReadString(PageName, "token", textEdit1.Text);
            openFileDialog1.FileName = storage.ReadString(PageName, "modelEditorPath", buttonEdit2.Text);
            openFileDialog2.FileName = storage.ReadString(PageName, "projectConverterPath", buttonEdit1.Text);

        }
        #endregion

        #region GetCategory
        public static string GetCategory() {
            return @"XAF";
        }
        #endregion
        #region GetPageName
        public static string GetPageName() {
            return @"XafAddins";
        }
        #endregion

        private void buttonEdit1_ButtonClick(object sender, ButtonPressedEventArgs e) {
            openFileDialog1.ShowDialog();
        }


        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {
            buttonEdit2.Text = openFileDialog1.FileName;
        }

        private void Options_CommitChanges(object sender, CommitChangesEventArgs ea) {
            ea.Storage.WriteString(PageName, "token", textEdit1.Text);
            ea.Storage.WriteString(PageName, "modelEditorPath", buttonEdit2.Text);
            ea.Storage.WriteString(PageName, "projectConverterPath", buttonEdit1.Text);
            ea.Storage.WriteString(PageName, connectionStringName.Name, connectionStringName.Text);
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            buttonEdit1.Text = openFileDialog2.FileName;
        }

        private void buttonEdit1_ButtonClick_1(object sender, ButtonPressedEventArgs e)
        {
            openFileDialog2.ShowDialog();
        }
    }
}