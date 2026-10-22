using System;

namespace AkumsPrintingApplication.PL
{
    public class PL_CartonLabelPrinting
    {
        public string RejectionRemarks { get; set; } = string.Empty;
        public string Mrp { get; set; } = string.Empty;
        public string ShipperSerial { get; set; } = string.Empty;
        public string ProcessOrder { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string BatchNo { get; set; } = string.Empty;
        public string SeqNo { get; set; } = string.Empty;
        public string PrintedMFGDate { get; set; } = string.Empty;
        public string PrintedExpDate { get; set; } = string.Empty;
        public string PrintedExpiryLabelFormat { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
        public Decimal GrossWeight { get; set; } = 0;
        public string Licence { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public int TargetShipper { get; set; } = 0;
        public string Remarks { get; set; } = string.Empty;
        public string ManufactureCode { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public string isPhysicianSample { get; set; } = string.Empty;
        public string isNotForSale { get; set; } = string.Empty;
        public string isLl { get; set; } = string.Empty;
        public string isR { get; set; } = string.Empty;
        public string isTm { get; set; } = string.Empty;
        public string isSubsidary { get; set; } = string.Empty;
        public string isPrintReqiured { get; set; } = string.Empty;
        public string isNrx { get; set; } = string.Empty;
        public string isReprint { get; set; } = string.Empty;
        public string isDiscard { get; set; } = string.Empty;
        public string PrintLine { get; set; } = string.Empty;
        public string MonthInName { get; set; } = "0";
        public string MonthInChar { get; set; } = "0";
        public string YearInTwoDigit { get; set; } = "0";
        public decimal MaxWt { get; set; } = 0;
        public decimal MinWt { get; set; } = 0;
        public decimal TareWt { get; set; } = 0;
        public Decimal NetWeight { get; set; }
        // change by pushpak two add extra column in DB

        public string MonthInCharYear { get; set; } = "0";
        public string Customer { get; set; } = string.Empty;

        public string GenericField { get; set; } = string.Empty;

        public string Medicine { get; set; } = string.Empty;

        public string checkBoxMedicine { get; set; } = string.Empty;

        public string SupervisorField { get; set; } = string.Empty;

        public string MediceCheck { get; set; } = string.Empty;

        public string CustomerLicence { get; set; } = string.Empty;

        public string YearFourDigit { get; set; } = string.Empty;

        public string PrinterIP { get; set; } = string.Empty;

        public string ScaleIP { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;






    }
}
