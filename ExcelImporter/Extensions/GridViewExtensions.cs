using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace ExcelImporter.Extensions
{
    public static class GridViewExtensions
    {
        public static void CleanCss(this GridView self)
        {
            self.GridLines = GridLines.None;
            self.CellSpacing = -1;

            self.DataBound += ((sender, args) =>
                {
                    self.HeaderRow.TableSection = TableRowSection.TableHeader;
                    if (self.FooterRow != null)
                        self.FooterRow.TableSection = TableRowSection.TableFooter;
                });
            self.RowDataBound += ((sender, args) =>
                {
                    if (args.Row.RowType == DataControlRowType.Header)
                    {
                        foreach (TableCell cell in args.Row.Cells)
                        {
                            ((DataControlFieldHeaderCell)cell).Scope = TableHeaderScope.NotSet;
                        }
                    }
                });
        }
    }
}