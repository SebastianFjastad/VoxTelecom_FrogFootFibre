using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Models;
using FrogFoot.Models.Reports;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace FrogFoot.Utilities
{
    public static class ExcelBuilder
    {
        const string randFormat = "R#,##0.00";
        const string percFormat = "#,##0.00\\%";

        #region Public Methods

        public static FileStreamResult BuildSalesReport(ReportViewModel model)
        {
            ExcelPackage pckg = new ExcelPackage();
            ExcelWorksheet d = pckg.Workbook.Worksheets.Add("Dashboard"); //Dashboard
            ExcelWorksheet h = pckg.Workbook.Worksheets.Add("History"); //History
            ExcelWorksheet o = pckg.Workbook.Worksheets.Add("Orders"); //Orders

            BuildDashboard(d, model);
            BuildHistory(h, model);
            BuildOrders(o, model);

            return ExcelAsFileStream("SalesReport.xlsx", pckg);
        }

        public static ActionResult GetUsersReport(List<ReportDataDto> users)
        {
            ExcelPackage pckg = new ExcelPackage();
            ExcelWorksheet u = pckg.Workbook.Worksheets.Add("Users");

            u.Cells["A1"].Value = "Precinct";
            u.Cells["B1"].Value = "Suburb";
            u.Cells["C1"].Value = "Estate";
            u.Cells["D1"].Value = "Zone";
            u.Cells["E1"].Value = "Address";
            u.Cells["F1"].Value = "Latitude";
            u.Cells["G1"].Value = "Longitude";
            u.Cells["H1"].Value = "CreatedBy";
            u.Cells["I1"].Value = "FirstName";
            u.Cells["J1"].Value = "LastName";
            u.Cells["K1"].Value = "Email";
            u.Cells["L1"].Value = "CellNo";
            u.Cells["M1"].Value = "Landline";
            u.Cells["N1"].Value = "RegisteredDate";
            u.Cells["O1"].Value = "OrderedOn";

            //header styling
            AsGreenHeader(u.Cells["A1:O1"]);

            var rowIndex = 2;
            foreach (var user in users)
            {
                u.Cells["A" + rowIndex].Value = user.Precinct;
                u.Cells["B" + rowIndex].Value = user.Suburb;
                u.Cells["C" + rowIndex].Value = user.Estate;
                u.Cells["D" + rowIndex].Value = user.Zone;
                u.Cells["E" + rowIndex].Value = user.Address;
                u.Cells["F" + rowIndex].Value = user.Latitude;
                u.Cells["G" + rowIndex].Value = user.Longitude;
                u.Cells["H" + rowIndex].Value = user.UserCreatedBy;
                u.Cells["I" + rowIndex].Value = user.FirstName;
                u.Cells["J" + rowIndex].Value = user.LastName;
                u.Cells["K" + rowIndex].Value = user.Email;
                u.Cells["L" + rowIndex].Value = user.PhoneNumber;
                u.Cells["M" + rowIndex].Value = user.Landline;
                u.Cells["N" + rowIndex].Value = user.CreatedDate;
                u.Cells["O" + rowIndex].Value = user.OrderedOn;
                rowIndex++;
            }

            //fit column width to content
            u.Cells[u.Dimension.Address].AutoFitColumns();

            return ExcelAsFileStream("UsersReport.xlsx", pckg);
        }

        public static FileStreamResult GetInterestReport(UserInterestViewModel uiModel, ReportViewModel reportModel)
        {
            ExcelPackage pckg = new ExcelPackage();
            ExcelWorksheet i = pckg.Workbook.Worksheets.Add("ZoneInterest");
            ExcelWorksheet h = pckg.Workbook.Worksheets.Add("History");

            BuildInterest(i, uiModel);
            BuildInterestHistory(h, reportModel);

            return ExcelAsFileStream("InterestReport.xlsx", pckg);
        }

        public static FileStreamResult GetISPUsersReport(List<ReportDataDto> users)
        {
            ExcelPackage pckg = new ExcelPackage();
            ExcelWorksheet u = pckg.Workbook.Worksheets.Add("ISPUsers");
            BuildISPUsers(u, users);
            return ExcelAsFileStream("ISPUserReport.xlsx", pckg);
        }

        public static FileStreamResult GetUsersInZoneReport(List<User> usersInZone)
        {
            ExcelPackage pckg = new ExcelPackage();
            ExcelWorksheet u = pckg.Workbook.Worksheets.Add("Users");
            BuildUsersInZone(u, usersInZone);
            return ExcelAsFileStream("UsersInZoneReport.xlsx", pckg);
        }
        #endregion

        #region Private Methods

        private static void BuildDashboard(ExcelWorksheet d, ReportViewModel model)
        {
            //count the rows of the table plus 2 or 3 for header and footer to position next table
            var tablePrecinctRowCount = model.OrdersByLocation.Count + 2;
            var tableProductRowCount = model.OrdersByFFProduct.Count + tablePrecinctRowCount + 3;
            var tableISPOrdersRowCount = model.OrdersByISP.Count + tableProductRowCount + 3;
            var tableMonthRowCount = model.OrdersByMonth.Count + tableISPOrdersRowCount + 3;
            var tableOverviewRowCount = model.MonthlyOverview.Count + tableMonthRowCount + 2;
            var tableARPURowCount = model.ARPU.Count + tableOverviewRowCount + 3;

            #region Precinct Table
            //header row
            d.Cells["A1"].Value = "Precinct";
            d.Cells["B1"].Value = "Vox Orders";
            d.Cells["C1"].Value = "Frogfoot";
            d.Cells["D1"].Value = "Vox";
            d.Cells["E1"].Value = "Other";
            d.Cells["F1"].Value = "Group";
            d.Cells["G1"].Value = "V% Orders";
            d.Cells["H1"].Value = "V% Revenue";
            d.Cells["I1"].Value = "Penetration";
            d.Cells["J1"].Value = "Residents";
            d.Cells["K1"].Value = "FF ARPU";
            d.Cells["L1"].Value = "Group ARPU";
            AsGreenHeader(d.Cells["A1:L1"]);

            //data rows
            var formatRowCount1 = 0;
            for (int i = 0; i < model.OrdersByLocation.Count; i++)
            {
                var item = model.OrdersByLocation[i];
                var x = i + 2;
                formatRowCount1 = x;
                d.Cells["A" + x].Value = item.Name;
                d.Cells["B" + x].Value = item.CountVox;
                d.Cells["C" + x].Value = item.CountAll;
                d.Cells["D" + x].Value = item.VoxValue;
                d.Cells["E" + x].Value = item.OtherValue;
                d.Cells["F" + x].Value = item.GroupValue;
                d.Cells["G" + x].Value = item.VoxPercOfOrders;
                d.Cells["H" + x].Value = item.VoxPercOfRevenue;
                d.Cells["I" + x].Value = item.Penetration;
                d.Cells["J" + x].Value = item.Residents;
                d.Cells["K" + x].Value = item.FFARPU;
                d.Cells["L" + x].Value = item.GroupARPU;
            }

            //formatting
            d.Cells[2, 7, formatRowCount1 + 1, 9].Style.Numberformat.Format = percFormat;
            d.Cells[2, 11, formatRowCount1 + 1, 12].Style.Numberformat.Format = randFormat;
            d.Cells[2, 4, formatRowCount1 + 1, 6].Style.Numberformat.Format = randFormat;


            //footer
            //frc = footer row count
            var frc1 = tablePrecinctRowCount;
            d.Cells["A" + frc1].Value = "Total";
            d.Cells["B" + frc1].Value = model.SumVoxOrderCount;
            d.Cells["C" + frc1].Value = model.SumAllOrderCount;
            d.Cells["D" + frc1].Value = model.SumVoxValue;
            d.Cells["E" + frc1].Value = model.SumOtherValue;
            d.Cells["F" + frc1].Value = model.SumGroupValue;
            d.Cells["G" + frc1].Value = model.AvgVoxPercOrders;
            d.Cells["H" + frc1].Value = model.AvgVoxPercRevenue;
            d.Cells["I" + frc1].Value = model.AvgPercPenetration;
            d.Cells["J" + frc1].Value = model.SumResidents;
            d.Cells["K" + frc1].Value = model.AvgFFARPU;
            d.Cells["L" + frc1].Value = model.AvgGroupARPU;
            AsGreyHeader(d.Cells["A" + frc1 + ":L" + frc1]);
            #endregion

            #region Product Table
            //header row
            var hdr = tablePrecinctRowCount + 2;
            d.Cells["A" + hdr].Value = "Product";
            d.Cells["B" + hdr].Value = "Vox Orders";
            d.Cells["C" + hdr].Value = "Frogfoot";
            d.Cells["D" + hdr].Value = "Vox";
            d.Cells["E" + hdr].Value = "Other";
            d.Cells["F" + hdr].Value = "Group";
            d.Cells["G" + hdr].Value = "V% Orders";
            d.Cells["H" + hdr].Value = "V% Revenue";
            d.Cells["I" + hdr].Value = "% Total";
            d.Cells["J" + hdr].Value = "V% Total";
            d.Cells["K" + hdr].Value = "Av. Capped Price";
            d.Cells["L" + hdr].Value = "Av. Uncapped Price";
            d.Cells["M" + hdr].Value = "Av. Price";
            AsGreenHeader(d.Cells["A" + hdr + ":M" + hdr]);

            //data rows
            hdr += 1;
            var formatRowCount2 = 0;
            for (int i = 0; i < model.OrdersByFFProduct.Count; i++)
            {
                var item = model.OrdersByFFProduct[i];
                var x = hdr + i;
                formatRowCount2 = x;

                d.Cells["A" + x].Value = item.IsM2MFrogfootLink ? item.LineSpeed + " (M2M)" : item.LineSpeed + " (24M)";
                d.Cells["B" + x].Value = item.CountVox;
                d.Cells["C" + x].Value = item.CountAll;
                d.Cells["D" + x].Value = item.VoxValue;
                d.Cells["E" + x].Value = item.OtherValue;
                d.Cells["F" + x].Value = item.GroupValue;
                d.Cells["G" + x].Value = item.VoxPercOfOrders;
                d.Cells["H" + x].Value = item.VoxPercOfRevenue;
                d.Cells["I" + x].Value = item.FFProdOrderPerc;
                d.Cells["J" + x].Value = item.VoxProdOrderPerc;
                d.Cells["K" + x].Value = item.AverageCappedPrice;
                d.Cells["L" + x].Value = item.AverageUncappedPrice;
                d.Cells["M" + x].Value = item.AveragePrice;
            }

            //formatting
            d.Cells[hdr, 7, formatRowCount2 + 1, 10].Style.Numberformat.Format = percFormat;
            d.Cells[hdr, 4, formatRowCount2 + 1, 6].Style.Numberformat.Format = randFormat;
            d.Cells[hdr, 11, formatRowCount2 + 1, 13].Style.Numberformat.Format = randFormat;

            //footer
            //frc = footer row count
            var frc2 = tableProductRowCount;
            d.Cells["A" + frc2].Value = "Total";
            d.Cells["B" + frc2].Value = model.SumVoxOrderCount;
            d.Cells["C" + frc2].Value = model.SumAllOrderCount;
            d.Cells["D" + frc2].Value = model.SumVoxValue;
            d.Cells["E" + frc2].Value = model.SumOtherValue;
            d.Cells["F" + frc2].Value = model.SumGroupValue;
            d.Cells["G" + frc2].Value = model.AvgVoxPercOrders;
            d.Cells["H" + frc2].Value = model.AvgVoxPercRevenue;
            d.Cells["I" + frc2].Value = model.AvgPercTotalOrders;
            d.Cells["J" + frc2].Value = model.AvgVoxPercTotalOrders;
            d.Cells["K" + frc2].Value = model.AvgCappedPrice;
            d.Cells["L" + frc2].Value = model.AvgUncappedPrice;
            d.Cells["M" + frc2].Value = model.AvgPrice;
            AsGreyHeader(d.Cells["A" + frc2 + ":M" + frc2]);
            #endregion

            #region ISP Orders Table
            var hdr2 = tableProductRowCount + 2;
            var formatRowCount3 = 0;
            d.Cells["A" + hdr2].Value = "ISP";
            d.Cells["B" + hdr2].Value = "Orders";
            d.Cells["C" + hdr2].Value = "BB Value";
            d.Cells["D" + hdr2].Value = "Links Value";
            d.Cells["E" + hdr2].Value = "ISP % Orders";
            d.Cells["F" + hdr2].Value = "ISP % Rev.";
            d.Cells["G" + hdr2].Value = "New";
            d.Cells["H" + hdr2].Value = "Pending";
            d.Cells["I" + hdr2].Value = "Ordered";
            d.Cells["J" + hdr2].Value = "Accepted";
            d.Cells["K" + hdr2].Value = "Penetration";
            AsGreenHeader(d.Cells["A" + hdr2 + ":K" + hdr2]);

            var residentsTotal = model.Locations.Where(l => l.AllowOrder).Sum(l => l.Residents);

            //data rows
            hdr2 += 1;
            for (int i = 0; i < model.OrdersByISP.Count; i++)
            {
                var item = model.OrdersByISP[i];
                var x = hdr2 + i;
                formatRowCount3 = x;
                d.Cells["A" + x].Value = item.ISPName;
                d.Cells["B" + x].Value = item.Orders;
                d.Cells["C" + x].Value = item.BBValue;
                d.Cells["D" + x].Value = item.LinksValue;
                d.Cells["E" + x].Value = item.ISPPercOfOrders;
                d.Cells["F" + x].Value = item.ISPPercOfRevenue;
                d.Cells["G" + x].Value = item.StatusNew;
                d.Cells["H" + x].Value = item.StatusPending;
                d.Cells["I" + x].Value = item.StatusOrdered;
                d.Cells["J" + x].Value = item.StatusAccepted;
                d.Cells["K" + x].Value = residentsTotal != 0 ? item.Orders / (decimal)residentsTotal * 100 : 0;
            }

            //formatting
            d.Cells[hdr2, 3, formatRowCount3 + 1, 4].Style.Numberformat.Format = randFormat;
            d.Cells[hdr2, 5, formatRowCount3 + 1, 6].Style.Numberformat.Format = percFormat;
            d.Cells[hdr2, 11, formatRowCount3 + 1, 11].Style.Numberformat.Format = percFormat;

            var frc3 = tableISPOrdersRowCount;
            d.Cells["A" + frc3].Value = "Total";
            d.Cells["B" + frc3].Value = model.OrdersByISP.Sum(i => i.Orders);
            d.Cells["C" + frc3].Value = model.OrdersByISP.Sum(i => i.BBValue);
            d.Cells["D" + frc3].Value = model.OrdersByISP.Sum(i => i.LinksValue);
            AsGreyHeader(d.Cells["A" + frc3 + ":K" + frc3]);

            #endregion

            #region Monthly Table
            var hdr4 = tableISPOrdersRowCount + 2;
            d.Cells["A" + hdr4].Value = "Month";
            d.Cells["B" + hdr4].Value = "Vox Orders";
            d.Cells["C" + hdr4].Value = "Frogfoot";
            d.Cells["D" + hdr4].Value = "Vox";
            d.Cells["E" + hdr4].Value = "Other";
            d.Cells["F" + hdr4].Value = "Group";
            d.Cells["G" + hdr4].Value = "V% Orders";
            d.Cells["H" + hdr4].Value = "V% Revenue";
            d.Cells["I" + hdr4].Value = "FF ARPU";
            d.Cells["J" + hdr4].Value = "Group ARPU";
            AsGreenHeader(d.Cells["A" + hdr4 + ":J" + hdr4]);

            //data rows
            hdr4 += 1;
            var formatRowCount5 = 0;
            for (int i = 0; i < model.OrdersByMonth.Count; i++)
            {
                var item = model.OrdersByMonth[i];
                var x = hdr4 + i;
                formatRowCount5 = x;
                d.Cells["A" + x].Value = item.Name;
                d.Cells["B" + x].Value = item.CountVox;
                d.Cells["C" + x].Value = item.CountAll;
                d.Cells["D" + x].Value = item.VoxValue;
                d.Cells["E" + x].Value = item.OtherValue;
                d.Cells["F" + x].Value = item.GroupValue;
                d.Cells["G" + x].Value = item.VoxPercOfOrders;
                d.Cells["H" + x].Value = item.VoxPercOfRevenue;
                d.Cells["I" + x].Value = item.FFARPU;
                d.Cells["J" + x].Value = item.GroupARPU;
            }

            //formatting
            d.Cells[hdr4, 4, formatRowCount5 + 1, 6].Style.Numberformat.Format = randFormat;
            d.Cells[hdr4, 7, formatRowCount5 + 1, 8].Style.Numberformat.Format = percFormat;
            d.Cells[hdr4, 9, formatRowCount5 + 1, 10].Style.Numberformat.Format = randFormat;

            //footer
            //frc = footer row count
            var frc5 = tableMonthRowCount;
            d.Cells["A" + frc5].Value = "Total";
            d.Cells["B" + frc5].Value = model.SumVoxOrderCount;
            d.Cells["C" + frc5].Value = model.SumAllOrderCount;
            d.Cells["D" + frc5].Value = model.SumVoxValue;
            d.Cells["E" + frc5].Value = model.SumOtherValue;
            d.Cells["F" + frc5].Value = model.SumGroupValue;
            d.Cells["G" + frc5].Value = model.AvgVoxPercOrders;
            d.Cells["H" + frc5].Value = model.AvgVoxPercRevenue;
            d.Cells["I" + frc5].Value = model.AvgFFARPU;
            d.Cells["J" + frc5].Value = model.AvgGroupARPU;

            AsGreyHeader(d.Cells["A" + frc5 + ":J" + frc5]);
            #endregion

            #region Monthly Overview Table
            //header row
            var hdr5 = tableMonthRowCount + 2;
            d.Cells["A" + hdr5].Value = "Period";
            d.Cells["B" + hdr5].Value = "Penetration";
            d.Cells["C" + hdr5].Value = "Orders";
            d.Cells["D" + hdr5].Value = "FF Rev.";
            d.Cells["E" + hdr5].Value = "Vox Rev.";
            d.Cells["F" + hdr5].Value = "Group Rev.";
            d.Cells["G" + hdr5].Value = "FF ARPU";
            d.Cells["H" + hdr5].Value = "Group ARPU";
            AsGreenHeader(d.Cells["A" + hdr5 + ":H" + hdr5]);

            //data rows
            hdr5 += 1;
            var formatRowCount6 = 0;
            for (int i = 0; i < model.MonthlyOverview.Count; i++)
            {
                var item = model.MonthlyOverview[i];
                var x = hdr5 + i;
                formatRowCount6 = x;
                d.Cells["A" + x].Value = item.Month;
                d.Cells["B" + x].Value = item.Penetration;
                d.Cells["C" + x].Value = item.Orders;
                d.Cells["D" + x].Value = item.FFRevenue;
                d.Cells["E" + x].Value = item.VoxRevenue;
                d.Cells["F" + x].Value = item.GroupRevenue;
                d.Cells["G" + x].Value = item.FFARPU;
                d.Cells["H" + x].Value = item.GroupARPU;
            }

            //formatting
            d.Cells[hdr5, 2, formatRowCount6, 2].Style.Numberformat.Format = percFormat;
            d.Cells[hdr5, 4, formatRowCount6, 8].Style.Numberformat.Format = randFormat;

            #endregion

            #region ARPU
            //header row
            var hdr6 = tableOverviewRowCount + 2;
            d.Cells["A" + hdr6].Value = "ARPU";
            d.Cells["B" + hdr6].Value = "Frogfoot";
            d.Cells["C" + hdr6].Value = "Vox";
            d.Cells["D" + hdr6].Value = "Group";
            AsGreenHeader(d.Cells["A" + hdr6 + ":D" + hdr6]);

            hdr6 += 1;
            var formatRowCount7 = 0;
            for (int i = 0; i < model.ARPU.Count; i++)
            {
                var item = model.ARPU[i];
                var x = hdr6 + i;
                formatRowCount7 = x;
                d.Cells["A" + x].Value = item.FFProduct;
                d.Cells["B" + x].Value = item.Frogfoot;
                d.Cells["C" + x].Value = item.Vox;
                d.Cells["D" + x].Value = item.Group;
            }

            var frc6 = tableARPURowCount;
            d.Cells["A" + frc6].Value = "Total";
            d.Cells["B" + frc6].Value = model.ARPU.Average(x => x.Frogfoot);
            d.Cells["C" + frc6].Value = model.ARPU.Average(x => x.Vox);
            d.Cells["D" + frc6].Value = model.ARPU.Average(x => x.Group);

            //formating
            d.Cells[hdr6, 2, formatRowCount7 + 1, 4].Style.Numberformat.Format = randFormat;
            AsGreyHeader(d.Cells["A" + frc6 + ":D" + frc6]);
            #endregion

            //fit column width to content
            d.Cells[d.Dimension.Address].AutoFitColumns();
        }

        private static void BuildHistory(ExcelWorksheet h, ReportViewModel model)
        {
            h.Cells["A1"].Value = "Period";
            h.Cells["A1:A4"].Merge = true;
            var monthIndex = 5;
            foreach (OrderData o in model.OrdersByMonth)
            {
                h.Cells["A" + monthIndex].Value = o.Name;
                monthIndex++;
            }

            h.Cells["A" + monthIndex].Value = "Total";
            AsCenteredHeader(h.Cells["A" + monthIndex]);

            var startRow = 5;
            var startCol = 2;
            var endCol = 12;

            foreach (var item in model.OrdersByLocation)
            {
                var startColName = GetExcelColumnName(startCol);

                #region Headers
                //location header
                h.Cells[startColName + "1"].Value = item.Name;
                var headerLoc = h.Cells[startColName + "1:" + GetExcelColumnName(endCol) + "1"];
                AsCenteredHeader(headerLoc);

                //orders header
                var ordersEndCol = startCol + 4;
                h.Cells[startColName + "2"].Value = "Orders";
                var headerOrders = h.Cells[startColName + "2:" + GetExcelColumnName(ordersEndCol) + "2"];
                AsCenteredHeader(headerOrders);

                //revenue header
                var revenueStartCol = ordersEndCol + 1;
                var revenueEndCol = revenueStartCol + 5;
                h.Cells[GetExcelColumnName(revenueStartCol) + "2"].Value = "Revenue";
                var headerRevenue = h.Cells[GetExcelColumnName(revenueStartCol) + "2:" + GetExcelColumnName(revenueEndCol) + "2"];
                AsCenteredHeader(headerRevenue);

                //product header
                var prodIndex = startCol;

                var prodCellCol1 = GetExcelColumnName(prodIndex);
                h.Cells[prodCellCol1 + "3"].Value = "10 Mbps";
                AsCenteredHeader(h.Cells[prodCellCol1 + "3:" + prodCellCol1 + "4"]);

                var prodCellCol2 = GetExcelColumnName(prodIndex + 1);
                h.Cells[prodCellCol2 + "3"].Value = "20 Mbps";
                AsCenteredHeader(h.Cells[prodCellCol2 + "3:" + prodCellCol2 + "4"]);

                var prodCellCol3 = GetExcelColumnName(prodIndex + 2);
                h.Cells[prodCellCol3 + "3"].Value = "50 Mbps";
                AsCenteredHeader(h.Cells[prodCellCol3 + "3:" + prodCellCol3 + "4"]);

                var prodCellCol4 = GetExcelColumnName(prodIndex + 3);
                h.Cells[prodCellCol4 + "3"].Value = "100 Mbps";
                AsCenteredHeader(h.Cells[prodCellCol4 + "3:" + prodCellCol4 + "4"]);

                var prodCellCol5 = GetExcelColumnName(prodIndex + 4);
                h.Cells[prodCellCol5 + "3"].Value = "1 000 Mbps";
                AsCenteredHeader(h.Cells[prodCellCol5 + "3:" + prodCellCol5 + "4"]);

                //FF revenue header
                var revCol1 = GetExcelColumnName(revenueStartCol);
                var revCol2 = GetExcelColumnName(revenueStartCol + 1);
                h.Cells[revCol1 + "3"].Value = "Frogfoot";
                AsCenteredHeader(h.Cells[revCol1 + "3:" + revCol2 + "3"]);

                h.Cells[revCol1 + "4"].Value = "Setup";
                AsCenteredHeader(h.Cells[revCol1 + "4"]);
                h.Cells[revCol2 + "4"].Value = "Monthly";
                AsCenteredHeader(h.Cells[revCol2 + "4"]);

                //Vox revenue header
                var revCol3 = GetExcelColumnName(revenueStartCol + 2);
                var revCol4 = GetExcelColumnName(revenueStartCol + 3);
                h.Cells[revCol3 + "3"].Value = "Vox";
                AsCenteredHeader(h.Cells[revCol3 + "3:" + revCol4 + "3"]);

                h.Cells[revCol3 + "4"].Value = "Setup";
                AsCenteredHeader(h.Cells[revCol3 + "4"]);
                h.Cells[revCol4 + "4"].Value = "Monthly";
                AsCenteredHeader(h.Cells[revCol4 + "4"]);

                //Other revenue header
                var revCol5 = GetExcelColumnName(revenueStartCol + 4);
                var revCol6 = GetExcelColumnName(revenueStartCol + 5);
                h.Cells[revCol5 + "3"].Value = "Other";
                AsCenteredHeader(h.Cells[revCol5 + "3:" + revCol6 + "3"]);

                h.Cells[revCol5 + "4"].Value = "Setup";
                AsCenteredHeader(h.Cells[revCol5 + "4"]);
                h.Cells[revCol6 + "4"].Value = "Monthly";
                AsCenteredHeader(h.Cells[revCol6 + "4"]);
                #endregion

                //each month working across
                var prodCol = startCol;
                var prodRow = 5;
                List<OrderData> ordersInLocation = model.AllOrders.Where(x => x.LocationId == item.LocationId).ToList();

                var prodCol1 = GetExcelColumnName(prodCol);
                var prodCol2 = GetExcelColumnName(prodCol + 1);
                var prodCol3 = GetExcelColumnName(prodCol + 2);
                var prodCol4 = GetExcelColumnName(prodCol + 3);
                var prodCol5 = GetExcelColumnName(prodCol + 4);
                var prodCol6 = GetExcelColumnName(prodCol + 5);
                var prodCol7 = GetExcelColumnName(prodCol + 6);
                var prodCol8 = GetExcelColumnName(prodCol + 7);
                var prodCol9 = GetExcelColumnName(prodCol + 8);
                var prodCol10 = GetExcelColumnName(prodCol + 9);
                var prodCol11 = GetExcelColumnName(prodCol + 10);

                foreach (var o in model.OrdersByMonth)
                {
                    //get order for month row
                    var ordersInMonth =
                        ordersInLocation.Where(
                            x => x.CreatedDate.Month == o.MonthPeriod.Month).ToList();

                    #region Product Orders
                    //group by linespeed
                    var ten = ordersInMonth.Count(x => x.LineSpeed == LineSpeed.TenMbps);
                    var twenty = ordersInMonth.Count(x => x.LineSpeed == LineSpeed.TwentyMbps);
                    var fifty = ordersInMonth.Count(x => x.LineSpeed == LineSpeed.FiftyMbps);
                    var hundred = ordersInMonth.Count(x => x.LineSpeed == LineSpeed.HundredMbps);
                    var gig = ordersInMonth.Count(x => x.LineSpeed == LineSpeed.OneGps);

                    h.Cells[prodCol1 + prodRow].Value = ten;
                    h.Cells[prodCol2 + prodRow].Value = twenty;
                    h.Cells[prodCol3 + prodRow].Value = fifty;
                    h.Cells[prodCol4 + prodRow].Value = hundred;
                    h.Cells[prodCol5 + prodRow].Value = gig;

                    #endregion

                    #region Revenue
                    var ffOnceOff = ordersInMonth.Sum(x => x.FFSetupRevenue);
                    var ffMonthly = ordersInMonth.Sum(x => x.FFMonthlyRevenue);

                    //ISPId = 2 is Vox Id on server and local
                    var voxOrders = ordersInMonth.Where(x => x.ISPId == 2);
                    var voxSetup = voxOrders.Sum(x => x.ISPSetupRevenue);
                    var voxMonthly = voxOrders.Where(x => x.ISPId == 2).Sum(x => x.ISPSetupRevenue);

                    var otherOrders = ordersInMonth.Where(x => x.ISPId != 2);
                    var otherSetup = otherOrders.Sum(x => x.ISPSetupRevenue);
                    var otherMonthly = otherOrders.Sum(x => x.ISPMonthlyRevenue);

                    h.Cells[prodCol6 + prodRow].Value = ffOnceOff;
                    h.Cells[prodCol7 + prodRow].Value = ffMonthly;
                    h.Cells[prodCol8 + prodRow].Value = voxSetup;
                    h.Cells[prodCol9 + prodRow].Value = voxMonthly;
                    h.Cells[prodCol10 + prodRow].Value = otherSetup;
                    h.Cells[prodCol11 + prodRow].Value = otherMonthly;
                    #endregion

                    prodRow += 1;
                }

                #region Footer  
                var footerRow = prodRow;
                var calcToRow = prodRow - 1;
                h.Cells[prodCol1 + footerRow].Formula = "SUM(" + prodCol1 + startRow + ":" + prodCol1 + calcToRow + ")";
                h.Cells[prodCol2 + footerRow].Formula = "SUM(" + prodCol2 + startRow + ":" + prodCol2 + calcToRow + ")";
                h.Cells[prodCol3 + footerRow].Formula = "SUM(" + prodCol3 + startRow + ":" + prodCol3 + calcToRow + ")";
                h.Cells[prodCol4 + footerRow].Formula = "SUM(" + prodCol4 + startRow + ":" + prodCol4 + calcToRow + ")";
                h.Cells[prodCol5 + footerRow].Formula = "SUM(" + prodCol5 + startRow + ":" + prodCol5 + calcToRow + ")";
                h.Cells[prodCol6 + footerRow].Formula = "SUM(" + prodCol6 + startRow + ":" + prodCol6 + calcToRow + ")";
                h.Cells[prodCol7 + footerRow].Formula = "SUM(" + prodCol7 + startRow + ":" + prodCol7 + calcToRow + ")";
                h.Cells[prodCol8 + footerRow].Formula = "SUM(" + prodCol8 + startRow + ":" + prodCol8 + calcToRow + ")";
                h.Cells[prodCol9 + footerRow].Formula = "SUM(" + prodCol9 + startRow + ":" + prodCol9 + calcToRow + ")";
                h.Cells[prodCol10 + footerRow].Formula = "SUM(" + prodCol10 + startRow + ":" + prodCol10 + calcToRow + ")";
                h.Cells[prodCol11 + footerRow].Formula = "SUM(" + prodCol11 + startRow + ":" + prodCol11 + calcToRow + ")";
                h.Cells[prodCol1 + footerRow + ":" + prodCol11 + footerRow].Style.Font.Bold = true;

                var footerRange = h.Cells[prodCol1 + footerRow + ":" + prodCol11 + footerRow];
                AsGreyHeader(footerRange);
                #endregion

                h.Cells[5, startCol + 5, prodRow, endCol].Style.Numberformat.Format = randFormat;

                //format RHS Border
                h.Cells[1, endCol, prodRow, endCol].Style.Border.Right.Style = ExcelBorderStyle.Medium;

                startCol = endCol + 1;
                endCol += 11;
            }

            //fit column width to content
            h.Cells[h.Dimension.Address].AutoFitColumns();
        }

        private static void BuildOrders(ExcelWorksheet o, ReportViewModel model)
        {
            o.Cells["A1"].Value = "OrderDate";
            o.Cells["B1"].Value = "FirstName";
            o.Cells["C1"].Value = "LastName";
            o.Cells["D1"].Value = "Email";
            o.Cells["E1"].Value = "Cell no";
            o.Cells["F1"].Value = "Landline";
            o.Cells["G1"].Value = "IsOwner";
            o.Cells["H1"].Value = "OrderId";
            o.Cells["I1"].Value = "Status";
            o.Cells["J1"].Value = "ProductName";
            o.Cells["K1"].Value = "LineSpeed";
            o.Cells["L1"].Value = "Speed";
            o.Cells["M1"].Value = "NRR-FF";
            o.Cells["N1"].Value = "MRR-FF";
            o.Cells["O1"].Value = "UpSpeed";
            o.Cells["P1"].Value = "IsCapped";
            o.Cells["Q1"].Value = "CreatedByRole";
            o.Cells["R1"].Value = "MonthlyCost";
            o.Cells["S1"].Value = "SetupCost";
            o.Cells["T1"].Value = "NRR-ISP";
            o.Cells["U1"].Value = "MRR-ISP";
            o.Cells["V1"].Value = "ISPName";
            o.Cells["W1"].Value = "Precinct";
            o.Cells["X1"].Value = "Zone";
            o.Cells["Y1"].Value = "Address";
            o.Cells["Z1"].Value = "NewDate";
            o.Cells["AA1"].Value = "PendingDate";
            o.Cells["AB1"].Value = "OrderedDate";
            o.Cells["AC1"].Value = "AcceptedDate";
            o.Cells["AD1"].Value = "CanceledDate";

            AsGreenHeader(o.Cells["A1:AD1"]);

            var rowIndex = 2;
            foreach (var item in model.Reports)
            {
                var lineSpeed = "";
                switch (item.LineSpeed)
                {
                    case LineSpeed.TenMbps:
                        lineSpeed = "10 Mbps";
                        break;
                    case LineSpeed.TwentyMbps:
                        lineSpeed = "20 Mbps";
                        break;
                    case LineSpeed.FiftyMbps:
                        lineSpeed = "50 Mbps";
                        break;
                    case LineSpeed.HundredMbps:
                        lineSpeed = "100 Mbps";
                        break;
                    case LineSpeed.OneGps:
                        lineSpeed = "1000 Mbps";
                        break;
                }

                o.Cells["A" + rowIndex].Value = item.CreatedDate;
                o.Cells["B" + rowIndex].Value = item.FirstName;
                o.Cells["C" + rowIndex].Value = item.LastName;
                o.Cells["D" + rowIndex].Value = item.Email;
                o.Cells["E" + rowIndex].Value = item.PhoneNumber;
                o.Cells["F" + rowIndex].Value = item.Landline;
                o.Cells["G" + rowIndex].Value = item.IsOwner;
                o.Cells["H" + rowIndex].Value = item.OrderId;
                o.Cells["I" + rowIndex].Value = item.Status;
                o.Cells["J" + rowIndex].Value = item.ProductName;
                o.Cells["K" + rowIndex].Value = item.LineSpeed;
                o.Cells["L" + rowIndex].Value = lineSpeed;
                o.Cells["M" + rowIndex].Value = item.FFSetupCost;
                o.Cells["N" + rowIndex].Value = item.FFMonthlyRevenue;
                o.Cells["O" + rowIndex].Value = item.UpSpeed;
                o.Cells["P" + rowIndex].Value = item.IsCapped;
                o.Cells["Q" + rowIndex].Value = item.CreatedByRole;
                o.Cells["R" + rowIndex].Value = item.MonthlyCost;
                o.Cells["S" + rowIndex].Value = item.SetupCost;
                o.Cells["T" + rowIndex].Value = item.SetupCost / (decimal)1.14;
                o.Cells["U" + rowIndex].Value = item.MonthlyCost / (decimal)1.14;
                o.Cells["V" + rowIndex].Value = item.ISPName;
                o.Cells["W" + rowIndex].Value = item.Precinct;
                o.Cells["X" + rowIndex].Value = item.Zone;
                o.Cells["Y" + rowIndex].Value = item.Address;

                var statusNew = item.StatusList.FirstOrDefault(s => s.OrderStatus == OrderStatus.New);
                o.Cells["Z" + rowIndex].Value = statusNew != null ? statusNew.TimeStamp.ToString("yyyy-M-d HH:mm") : "";

                var statusPending = item.StatusList.FirstOrDefault(s => s.OrderStatus == OrderStatus.Pending);
                o.Cells["AA" + rowIndex].Value = statusPending != null ? statusPending.TimeStamp.ToString("yyyy-M-d HH:mm") : "";

                var statusOrdered = item.StatusList.FirstOrDefault(s => s.OrderStatus == OrderStatus.Ordered);
                o.Cells["AB" + rowIndex].Value = statusOrdered != null ? statusOrdered.TimeStamp.ToString("yyyy-M-d HH:mm") : "";

                var statusAccepted = item.StatusList.FirstOrDefault(s => s.OrderStatus == OrderStatus.Accepted);
                o.Cells["AC" + rowIndex].Value = statusAccepted != null ? statusAccepted.TimeStamp.ToString("yyyy-M-d HH:mm") : "";

                var statusCanceled = item.StatusList.FirstOrDefault(s => s.OrderStatus == OrderStatus.Canceled);
                o.Cells["AD" + rowIndex].Value = statusCanceled != null ? statusCanceled.TimeStamp.ToString("yyyy-M-d HH:mm") : "";

                rowIndex += 1;
            }

            o.Cells[2, 13, rowIndex, 14].Style.Numberformat.Format = randFormat;
            o.Cells[2, 18, rowIndex, 21].Style.Numberformat.Format = randFormat;

            //fit column width to content
            o.Cells[o.Dimension.Address].AutoFitColumns();
        }

        private static void BuildInterest(ExcelWorksheet i, UserInterestViewModel uiModel)
        {
            //header row
            i.Cells["A1"].Value = "Precinct/ Zone";
            i.Cells["B1"].Value = "Leads";
            i.Cells["C1"].Value = "Conversions";
            i.Cells["D1"].Value = "Residents";
            i.Cells["E1"].Value = "% Penetration";
            AsGreenHeader(i.Cells["A1:E1"]);

            var rowCount = 2;
            foreach (var locs in uiModel.Locations.GroupBy(l => l.PrecinctCode))
            {
                var indexOfHeaders = rowCount;
                var loc = locs.First();
                i.Cells["A" + indexOfHeaders].Value = loc.PrecinctCode;
                AsGreyHeader(i.Cells["A" + indexOfHeaders + ":" + "E" + indexOfHeaders]);

                rowCount += 1;
                foreach (var zone in uiModel.Zones.Where(z => z.PrecinctCode == loc.PrecinctCode))
                {
                    var zoneTotalUsers = zone.NoHousesInZone;
                    var usersInZone = uiModel.UsersAndOrders.Where(u => u.ZoneId == zone.ZoneId);
                    var zoneUsersCount = usersInZone.Count();
                    var zoneUsersWithOrders = usersInZone.Count(u => u.UserHasOrder == true);
                    var zonePenetrationPerc = zoneTotalUsers != 0 ? zoneUsersWithOrders / (decimal)zoneTotalUsers * 100 : 0;

                    i.Cells["A" + rowCount].Value = loc.PrecinctCode + " - " + zone.Code;
                    i.Cells["B" + rowCount].Value = zoneUsersCount;
                    i.Cells["C" + rowCount].Value = zoneUsersWithOrders;
                    i.Cells["D" + rowCount].Value = zoneTotalUsers;
                    i.Cells["E" + rowCount].Value = zonePenetrationPerc;
                    i.Cells["E" + rowCount].Style.Numberformat.Format = percFormat;
                    rowCount++;
                }
                rowCount++;
            }
            //fit column width to content
            i.Cells[i.Dimension.Address].AutoFitColumns();
        }

        private static void BuildInterestHistory(ExcelWorksheet h, ReportViewModel model)
        {
            h.Cells["A1"].Value = "Period";
            h.Cells["A1:A2"].Merge = true;

            var monthIndex = 3;
            foreach (OrderData o in model.OrdersByMonth)
            {
                h.Cells["A" + monthIndex].Value = o.Name;
                monthIndex++;
            }

            var startRow = 3;
            var startCol = 2;
            var endCol = 3;

            foreach (var item in model.OrdersByLocation)
            {
                var itemRow = 3;
                var startColName = GetExcelColumnName(startCol);

                #region Headers
                h.Cells[startColName + "1"].Value = item.Name;
                var headerLoc = h.Cells[startColName + "1:" + GetExcelColumnName(endCol) + "1"];
                AsCenteredHeader(headerLoc);
                AsGreenHeader(headerLoc);

                //Registrations header
                var registrationsCol = startCol;
                h.Cells[startColName + "2"].Value = "Registrations";
                var headerOrders = h.Cells[startColName + "2:" + GetExcelColumnName(registrationsCol) + "2"];
                AsGreyHeader(headerOrders);
                AsGreyHeader(headerOrders);

                //Orders header
                var orderCol = registrationsCol + 1;
                h.Cells[GetExcelColumnName(orderCol) + "2"].Value = "Orders";
                var headerRevenue = h.Cells[GetExcelColumnName(orderCol) + "2:" + GetExcelColumnName(orderCol) + "2"];
                AsGreyHeader(headerRevenue);
                AsGreyHeader(headerRevenue);
                #endregion

                var userCol = startCol;
                List<UserInterestData> usersInLocation = model.UsersWithOrders.Where(x => x.LocationId != null && x.LocationId == item.LocationId).ToList();

                var userCol1 = GetExcelColumnName(userCol);
                var userCol2 = GetExcelColumnName(userCol + 1);

                foreach (var o in model.OrdersByMonth)
                {
                    //get order for month row
                    var ordersInMonth =
                        usersInLocation.Where(
                            x => (x.CreatedDate != null && x.CreatedDate.Value.Month == o.MonthPeriod.Month)
                            || (x.RegisteredDate != null && x.RegisteredDate.Value.Month == o.MonthPeriod.Month)).ToList();

                    #region Users and Orders
                    var registeredCount = ordersInMonth.Count();
                    var orderedCount = ordersInMonth.Count(x => x.UserHasOrder == true);

                    h.Cells[userCol1 + itemRow].Value = registeredCount;
                    h.Cells[userCol2 + itemRow].Value = orderedCount;
                    #endregion

                    itemRow += 1;
                }

                #region Footer
                var footerRow = itemRow;
                var calcToRow = itemRow - 1;

                //Total footer values
                h.Cells["A" + footerRow].Value = "Total";
                h.Cells[userCol1 + footerRow].Formula = "SUM(" + userCol1 + startRow + ":" + userCol1 + calcToRow + ")";
                h.Cells[userCol2 + footerRow].Formula = "SUM(" + userCol2 + startRow + ":" + userCol2 + calcToRow + ")";

                //Conversion footer values
                var fr1 = footerRow + 1;
                h.Cells["A" + fr1].Value = "Conversion";
                h.Cells[userCol2 + fr1].Formula = userCol2 + footerRow + "/" + "SUM(" + userCol1 + startRow + ":" + userCol1 + calcToRow + ")*100";
                h.Cells[userCol2 + fr1].Style.Numberformat.Format = percFormat;

                //Penetration footer values
                var fr2 = footerRow + 2;
                h.Cells["A" + fr2].Value = "Penetration";
                h.Cells[userCol1 + fr2].Formula = "SUM(" + userCol1 + startRow + ":" + userCol1 + calcToRow + ")" + "/" + item.Residents + "*100";
                h.Cells[userCol2 + fr2].Formula = "SUM(" + userCol2 + startRow + ":" + userCol2 + calcToRow + ")" + "/" + item.Residents + "*100";
                h.Cells[userCol1 + fr2].Style.Numberformat.Format = percFormat;
                h.Cells[userCol2 + fr2].Style.Numberformat.Format = percFormat;

                //Potential footer values
                var fr3 = footerRow + 3;
                h.Cells["A" + fr3].Value = "Potential";
                h.Cells[userCol2 + fr3].Value = item.Residents;
                #endregion
                h.Cells[1, endCol, fr3, endCol].Style.Border.Right.Style = ExcelBorderStyle.Medium;

                startCol += 2;
                endCol += 2;
            }

            h.Cells[h.Dimension.Address].AutoFitColumns();
        }

        private static void BuildISPUsers(ExcelWorksheet u, List<ReportDataDto> users)
        {
            u.Cells["A1"].Value = "ISP";
            u.Cells["B1"].Value = "FirstName";
            u.Cells["C1"].Value = "LastName";
            u.Cells["D1"].Value = "Email";
            u.Cells["E1"].Value = "CreatedDate";
            u.Cells["F1"].Value = "Logins";
            u.Cells["G1"].Value = "LastLoginDate";
            AsGreenHeader(u.Cells["A1:G1"]);

            var index = 2;
            foreach (var user in users)
            {
                u.Cells["A" + index].Value = user.ISPName;
                u.Cells["B" + index].Value = user.FirstName;
                u.Cells["C" + index].Value = user.LastName;
                u.Cells["D" + index].Value = user.Email;
                u.Cells["E" + index].Value = user.CreatedDate;
                u.Cells["F" + index].Value = user.LogInCount;
                u.Cells["G" + index].Value = user.LastLogInDate;
                index++;
            }
            u.Cells[u.Dimension.Address].AutoFitColumns();
        }

        private static void BuildUsersInZone(ExcelWorksheet u, List<User> users)
        {
            u.Cells["A1"].Value = "FirstName";
            u.Cells["B1"].Value = "LastName";
            u.Cells["C1"].Value = "Email";
            u.Cells["D1"].Value = "Cell";
            u.Cells["E1"].Value = "Zone";
            u.Cells["F1"].Value = "UserId";
            u.Cells["G1"].Value = "FFCommsOptOut";
            u.Cells["H1"].Value = "OrderDate";
            u.Cells["I1"].Value = "OrderStatus";
            AsGreenHeader(u.Cells["A1:I1"]);

            var index = 2;
            foreach (var user in users)
            {
                var order = user.Orders.FirstOrDefault(o => o.Status != OrderStatus.Canceled);
                var optOutStatus = user.FFCommsOptOutStatus != null ? (user.FFCommsOptOutStatus == true ? "Out" : "In") : "Unresponded";

                u.Cells["A" + index].Value = user.FirstName;
                u.Cells["B" + index].Value = user.LastName;
                u.Cells["C" + index].Value = user.Email;
                u.Cells["D" + index].Value = user.PhoneNumber;
                u.Cells["E" + index].Value = user.Zone != null ? user.Zone.Code : "No Zone";
                u.Cells["F" + index].Value = user.Id;
                u.Cells["G" + index].Value = optOutStatus;
                u.Cells["H" + index].Value = order != null ? order.CreatedDate.ToShortDateString() : "none";
                u.Cells["I" + index].Value = order != null ? order.Status.ToString() : "none";
                index++;
            }
            u.Cells[u.Dimension.Address].AutoFitColumns();
        }
        #endregion

        #region Helpers
        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        private static void AsCenteredHeader(ExcelRange range)
        {
            range.Merge = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.Font.Bold = true;
        }

        private static void AsGreenHeader(ExcelRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.Green);
        }

        private static void AsGreyHeader(ExcelRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.Gray);
        }

        private static FileStreamResult ExcelAsFileStream(string fileName, ExcelPackage package)
        {
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var fileStream = new MemoryStream();
            package.SaveAs(fileStream);
            fileStream.Position = 0;
            var stream = new FileStreamResult(fileStream, contentType) { FileDownloadName = fileName };
            return stream;
        }
        #endregion
    }
}