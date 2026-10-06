using System.Collections.Generic;

namespace Aniel.Connect.Integracoes.SerasaExperian.Models
{
    public class ConsultaSerasaResponse
    {
        public List<Report> reports { get; set; }
        
        public DescricoesErros Erros { get; set; } = new DescricoesErros();
        
    }

    public class ConsultaSerasaExperianHtml
    {
        public string Documento { get; set; }
        public string SituacaoDocumento { get; set; }
        public string NomeMae { get; set; }
        public string NomeConsumidor { get; set; }
        public bool PessoaFisica { get; set; }
        public ScoreHtml Score { get; set; }
        public PendenciasFinanceirasHtml PendenciasFinanceiras { get; set; }
        public ProtestosHtml Protestos { get; set; }   
        public ChecksHtml Checks { get; set; }

    }

    public class ChecksHtml
    {
        public string ValorTotalTotalChecks  { get; set; }
        public string TotalChecks { get; set; }
        public string UltimaOcorrenciaCheck{ get; set; }
    }

    public class PendenciasFinanceirasHtml
    {
        public string ValorTotalPendenciasFinanceiras { get; set; }
        public string TotalPendenciasDebitos { get; set; }
        public string UltimaOcorrenciaPendencias { get; set; }
    }

    public class ProtestosHtml
    {
        public string ValorTotalProtestos { get; set; }
        public List<string> CidadesProtestos { get; set; }
        public List<string> CidadesProtestosUF { get; set; }
        public string UltimaOcorrenciaProtestos { get; set; }
    }
    public class ScoreHtml
    {
        public string ScoreSerasa { get; set; }
        public string FaixaScore { get; set; }
        public string MsgScore { get; set; }
        public string ModeloScore { get; set; }
    }


    public class OptionalFeatures
    {
        public Score score { get; set; }
    }

    public class Check
    {
        public List<CheckResponse> checkResponse { get; set; }
        public Summary summary { get; set; }
    }

    public class CheckResponse
    {
        public string occurrenceDate { get; set; }
        public int alinea { get; set; }
        public int bankId { get; set; }
        public string bankName { get; set; }
        public int bankAgencyId { get; set; }
        public string checkNumber { get; set; }
        public int checkCount { get; set; }
        public string cadus { get; set; }
    }

    public class CollectionRecords
    {
        public List<object> collectionRecordsResponse { get; set; }
        public Summary summary { get; set; }
    }

    public class CreditInquiriesQuantity
    {
        public string inquiryDate { get; set; }
        public int occurrences { get; set; }
    }

    public class Facts
    {
        public Inquiry inquiry { get; set; }
        public InquirySummary inquirySummary { get; set; }
        public StolenDocuments stolenDocuments { get; set; }
        public InquiryCompanyResponse inquiryCompanyResponse { get; set; }
    }

    public class Inquiry
    {
        public List<object> inquiryResponse { get; set; }
    }

    public class InquiryQuantity
    {
        public int actual { get; set; }
        public List<CreditInquiriesQuantity> creditInquiriesQuantity { get; set; }
    }

    public class InquirySummary
    {
        public InquiryQuantity inquiryQuantity { get; set; }
    }

    public class NegativeData
    {
        public Pefin pefin { get; set; }
        public Refin refin { get; set; }
        public Notary notary { get; set; }
        public Check check { get; set; }
        public CollectionRecords collectionRecords { get; set; }
    }

    public class NegativeSummary
    {
    }

    public class Notary
    {
        public List<NotaryResponse> notaryResponse { get; set; }
        public Summary summary { get; set; }
    }
    public class NotaryResponse
    {
        public string occurrenceDate { get; set; }
        public double amount { get; set; }
        public string officeNumber { get; set; }
        public string city { get; set; }
        public string federalUnit { get; set; }
        public Dispute dispute { get; set; }
        public string cadus { get; set; }
    }
    public class Partner
    {
        public Summary summary { get; set; }
        public PartnerResponse PartnerResponse { get; set; }
    }

    public class Pefin
    {
        public List<PefinResponse> pefinResponse { get; set; }
        public Summary summary { get; set; }
    }

    public class PefinResponse
    {
        public string occurrenceDate { get; set; }
        public string legalNatureId { get; set; }
        public string legalNature { get; set; }
        public string contractId { get; set; }
        public string creditorName { get; set; }
        public double amount { get; set; }
        public bool principal { get; set; }
        public Dispute dispute { get; set; }
        public string cadus { get; set; }
    }

    public class Dispute
    {
        public bool disputeIndicativeFlag { get; set; }
    }

    public class Refin
    {
        public List<object> refinResponse { get; set; }
        public Summary summary { get; set; }
    }

    public class Registration
    {
        public string documentNumber { get; set; }
        public string consumerName { get; set; }
        public string motherName { get; set; }
        public string birthDate { get; set; }
        public string statusRegistration { get; set; }
        public string statusDate { get; set; }
        public string companyDocument { get; set; }
        public string companyName { get; set; }
        public string foundationDate { get; set; }
        public Address address { get; set; }
    }

    public class Report
    {
        public string reportName { get; set; }
        public Registration registration { get; set; } //dados de registro
        public NegativeData negativeData { get; set; }
        public Score score { get; set; }
        public NegativeSummary negativeSummary { get; set; }
        public Facts facts { get; set; }
        public Partner partner { get; set; }
    }

    public class Score
    {
        public int codeMessage { get; set; }
        public string message { get; set; }
        public int score { get; set; }
        public string scoreModel { get; set; }
        public string defaultRate { get; set; }
        public bool billing { get; set; }
        public string range { get; set; }
    }

    public class StolenDocuments
    {
        public List<object> stolenDocumentsResponse { get; set; }
        public Summary summary { get; set; }
    }

    public class Summary
    {
        public int count { get; set; }
        public double balance { get; set; }
        public string firstOccurrence { get; set; }
        public string lastOccurrence { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Address
    {
        public string city { get; set; }
        public string state { get; set; }
    }

    public class Historical
    {
        public string inquiryDate { get; set; }
        public int occurrences { get; set; }
        public int bankOccurrences { get; set; }
    }

    public class InquiryCompanyResponse
    {
        public Quantity quantity { get; set; }
    }

    public class PartnerResponse
    {
        public List<Result> results { get; set; }
    }

    public class Quantity
    {
        public int actual { get; set; }
        public int bankActual { get; set; }
        public List<Historical> historical { get; set; }
    }

    public class Result
    {
        public string documentId { get; set; }
        public string name { get; set; }
        public double participationPercentage { get; set; }
        public bool inconsistent { get; set; }
        public bool hasNegative { get; set; }
    }

    public class Root
    {
        public List<Report> reports { get; set; }
    }

}