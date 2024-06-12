using System.Globalization;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText7Pdf;

List<SupplierProduct> products =
[
    new SupplierProduct { Description = "Toy Car", UnitPrice = 25.33m, Quantity = 3 },
    new SupplierProduct { Description = "Teddy", UnitPrice = 3.90m, Quantity = 5 }
];
const string supplierAddress = "Acme Inc, 515 Park Street, Davenport, Iowa, 52801, United States";
const string buyerAddress = "Tailspin Toys, 61 Hill Lane, Aurora, Illinois, 60502, United States";
const string deliveryAddress = "109 Hill Terrace, Jackson, Mississippi, 39201, United States";
const string tableHeaderColor = "ADD8E6";

CreatePurchaseOrder();
return;

void CreatePurchaseOrder()
{
    const string pdfFileName = "Purchase Order.pdf";
    using var document = new Document(new PdfDocument(new PdfWriter(pdfFileName)));
    document.SetFontSize(10);
    document.SetMargins(36, 72, 36, 72);

    CreateHeader(document);

    CreateItemDetails(document);

    CreateDeliveryDetails(document);
}

void CreateHeader(Document document)
{
    var headerTable = new Table(UnitValue.CreatePercentArray([50, 50])).UseAllAvailableWidth();

    headerTable.AddCell(new Cell(1, 2)
        .Add(new Paragraph("Purchase Order"))
        .SetBorder(Border.NO_BORDER)
        .SetPaddingBottom(20)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetFontSize(18)
        .SetBold()
    );

    headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph($"Supplier: \n{supplierAddress}")));
    headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph($"Buyer: \n{buyerAddress}")));
    headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph($"\nPO No: PO{DateTime.Today:MMddyyyy}{new Random().NextInt64(10, 99)}")));
    headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph($"\nPO Date: {DateTime.Today:dd-MMM-yyyy}")));

    document.Add(headerTable);
}

void CreateItemDetails(Document document)
{
    var itemsTable = new Table(UnitValue.CreatePercentArray([55, 15, 15, 15]))
        .SetMarginTop(20)
        .SetMarginBottom(20)
        .UseAllAvailableWidth();

    itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("Description"))
        .SetBackgroundColor(WebColors.GetRGBColor(tableHeaderColor)));

    itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("Quantity"))
        .SetTextAlignment(TextAlignment.CENTER)
        .SetBackgroundColor(WebColors.GetRGBColor(tableHeaderColor)));

    itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("Unit Price ($)"))
        .SetTextAlignment(TextAlignment.CENTER)
        .SetBackgroundColor(WebColors.GetRGBColor(tableHeaderColor)));

    itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("Amount ($)"))
        .SetTextAlignment(TextAlignment.CENTER)
        .SetBackgroundColor(WebColors.GetRGBColor(tableHeaderColor)));

    foreach (var product in products)
    {
        itemsTable.AddCell(new Cell().Add(new Paragraph(product.Description)));
        itemsTable.AddCell(new Cell().SetTextAlignment(TextAlignment.CENTER)
            .Add(new Paragraph(product.Quantity.ToString(CultureInfo.InvariantCulture))));
        itemsTable.AddCell(new Cell().SetTextAlignment(TextAlignment.RIGHT)
            .Add(new Paragraph(product.UnitPrice.ToString(CultureInfo.InvariantCulture))));
        itemsTable.AddCell(new Cell().SetTextAlignment(TextAlignment.RIGHT)
            .Add(new Paragraph(product.Amount.ToString(CultureInfo.InvariantCulture))));
    }

    document.Add(itemsTable);
}

void CreateDeliveryDetails(Document document)
{
    var deliveryTable = new Table(UnitValue.CreatePercentArray([50, 50])).UseAllAvailableWidth();

    deliveryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph($"Delivery Address: \n{deliveryAddress}")));
    deliveryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph($"Delivery Date: {DateTime.Today.AddDays(60):dd-MMM-yyyy}")));
    deliveryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("\n\nAuthorized By: \n\n")));
    deliveryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("\n\nDate: \n\n")));
    deliveryTable.AddCell(new Cell(1, 2).SetBorder(Border.NO_BORDER).Add(new Paragraph("\n\nPayment Terms: \n\n")));

    document.Add(deliveryTable);
}