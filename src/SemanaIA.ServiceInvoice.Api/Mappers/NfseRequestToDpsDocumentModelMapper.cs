using SemanaIA.ServiceInvoice.Api.Requests;
using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.Api.Mappers;

public class NfseRequestToDpsDocumentModelMapper
{
    public static DpsDocument Map(NfseGenerateXmlRequest request)
    {
        return new DpsDocument
        {
            Environment = 2,
            Series = request.RpsSerialNumber ?? "00001",
            Number = request.RpsNumber ?? 1,
            IssuedOn = request.IssuedOn,
            CompetenceDate = DateOnly.FromDateTime(request.IssuedOn.Date),
            Provider = new Provider
            {
                Cnpj = "00000000000000",
                MunicipalTaxNumber = null,
                MunicipalityCode = request.Location.City.Code ?? string.Empty
            },
            Borrower = new Borrower
            {
                Name = request.Borrower.Name,
                FederalTaxNumber = request.Borrower.FederalTaxNumber,
                Address = new Address
                {
                    Country = request.Borrower.Address.Country,
                    PostalCode = request.Borrower.Address.PostalCode,
                    Street = request.Borrower.Address.Street,
                    Number = request.Borrower.Address.Number,
                    AdditionalInformation = request.Borrower.Address.AdditionalInformation,
                    District = request.Borrower.Address.District,
                    City = new  City
                    {
                       Code = request.Borrower.Address.City.Code ?? string.Empty,
                       Name = request.Borrower.Address.City.Name,
                    },
                    State = request.Borrower.Address.State
                }
            },
            Service = new Service
            {
                FederalServiceCode = request.FederalServiceCode,
                Description = request.Description,
                NbsCode = request.NbsCode,
                MunicipalityCode = request.Location.City.Code ?? string.Empty
            },
            Values = new Values
            {
                ServicesAmount = request.ServicesAmount,
                TaxationType = request.TaxationType
            }
        };
    }
}