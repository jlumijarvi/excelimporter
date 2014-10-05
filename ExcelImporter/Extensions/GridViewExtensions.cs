using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace ExcelImporter.Extensions
{
    public static class GridViewExtensions
    {
        /// <summary>
        /// Cleans ASP.Net GridView from ASP.Net stylings
        /// </summary>
        /// <param name="self"></param>
        public static void CleanCss(this GridView self)
        {
            self.GridLines = GridLines.None;
            self.CellSpacing = -1; // => cellspacing will be removed

            // add thead, tbody and tfooter
            self.DataBound += ((sender, args) =>
                {
                    self.HeaderRow.TableSection = TableRowSection.TableHeader;
                    if (self.FooterRow != null)
                        self.FooterRow.TableSection = TableRowSection.TableFooter;
                });

            // remove scope elements
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