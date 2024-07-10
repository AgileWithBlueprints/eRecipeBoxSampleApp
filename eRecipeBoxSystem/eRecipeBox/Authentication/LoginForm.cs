/*
* MIT License
* 
* Copyright (C) 2024 SoftArc, LLC
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.Windows.Forms;
namespace eRecipeBox
{
    public partial class LoginForm : MDIChildForm
    {
        public UserProfile UserProfile { get; set; }

        public LoginForm() : base(nameof(LoginForm), null)
        {
            base.InitializeBaseComponent();
            InitializeComponent();
            InitializeComponent2();
        }
        private void InitializeComponent2()
        {
            //in memory only UserProfile for binding.  we are not saving, so we dont need an XPO session
            this.UserProfile = new UserProfile();

            //connect our error provider 
            dxErrorProvider1.DataSource = this.userProfileXPBindingSource;
            this.userProfileXPBindingSource.DataSource = UserProfile;

            SetControlConstraints();
        }

        private void CancelSimpleButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void OKSimpleButton_Click(object sender, EventArgs e)
        {
            if (ValidateFormAndNotify())
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}