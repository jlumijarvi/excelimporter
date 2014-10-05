using ExcelImporter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ExcelImporter.Extensions;
using System.IO;

namespace ExcelImporter.UserControls
{
    public partial class ExcelImporter : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ColumnMappings.CleanCss();
            MappingPanel.Style.Add(HtmlTextWriterStyle.Display, "none");
        }

        /// <summary>
        /// Page must register this as an async task in PageLoad()
        /// Eg. RegisterAsyncTask(new PageAsyncTask(ImportExcelCtrl.SaveUploadedFile));
        /// </summary>
        /// <returns></returns>
        public async Task SaveUploadedFile()
        {
            if (IsPostBack && FileUpload.HasFiles)
            {
                var headerData = default(IEnumerable<object>);
                try
                {
                    var postedFile = FileUpload.PostedFiles.First();

                    SelectedFile.Text = postedFile.FileName;

                    var repo = new RegistryRepository();
                    FileId.Value = await repo.SaveImportedFile(postedFile);

                    headerData = await repo.GetHeaderData(FileId.Value);
                }
                catch (FormatException)
                {
                    if (!fileForm.CssClass.Contains("has-error"))
                        fileForm.CssClass += " has-error";
                    fileFormError.Visible = true;
                    return;
                }

                ColumnMappings.DataSource = headerData;
                ColumnMappings.DataBind();

                for (int i = 0; i < ColumnMappings.Rows.Count; i++)
                {
                    var header = ((dynamic)ColumnMappings.DataSource)[i];
                    if (!string.IsNullOrEmpty(header.SelectedTable))
                    {
                        var gridRow = ColumnMappings.Rows[i];

                        var ddlTables = gridRow.FindControl("myDropDownListTables") as DropDownList;
                        var selTableInd = Array.IndexOf(ddlTables.Items.OfType<ListItem>().Select(it => it.Value).ToArray(), header.SelectedTable);
                        ddlTables.SelectedIndex = selTableInd;

                        var ddlColumns = gridRow.FindControl("myDropDownListColumns") as DropDownList;
                        var selColumnInd = Array.IndexOf(ddlColumns.Items.OfType<ListItem>().Select(it => it.Value).ToArray(), header.SelectedColumn);
                        ddlColumns.SelectedIndex = selColumnInd;
                    }
                }
            }
        }
    }
}
