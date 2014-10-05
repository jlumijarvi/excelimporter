using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Net.Http;
using System.IO;
using System.Threading;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Threading.Tasks;
using ExcelImporter.Models;
using ExcelImporter.Extensions;

namespace ExcelImporter
{
    public partial class ImportExcel : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(ImportExcelCtrl.SaveUploadedFile));
        }
    }
}