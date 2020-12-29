using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empresa.integracao.ftp.filesystem
{
    class CSVObject
    {
        public int Length { get; set; }
        public int indice { get; set; }
        //// public List<int> SK_IDARQUIVO_CSV { get; set; }
        public List<int?> ID_TEMPO_COMPETENCIA { get; set; }
        public List<int?> CD_OPERADORA { get; set; }
        public List<DateTime?> DT_INCLUSAO { get; set; }
        public List<int?> CD_BENE_MOTV_INCLUSAO { get; set; }
        public List<string> IND_PORTABILIDADE { get; set; }
        public List<int?> ID_MOTIVO_MOVIMENTO { get; set; }
        public List<int?> LG_BENEFICIARIO_ATIVO { get; set; }
        public List<DateTime?> DT_NASCIMENTO { get; set; }
        public List<string> TP_SEXO { get; set; }
        public List<int?> CD_PLANO_RPS { get; set; }
        public List<string> CD_PLANO_SCPA { get; set; }
        public List<int?> NR_PLANO_PORTABILIDADE { get; set; }
        public List<DateTime?> DT_PRIMEIRA_CONTRATACAO { get; set; }
        public List<DateTime?> DT_CONTRATACAO { get; set; }
        public List<int?> ID_BENE_TIPO_DEPENDENTE { get; set; }
        public List<int?> LG_COBERTURA_PARCIAL { get; set; }
        public List<int?> LG_ITEM_EXCLUIDO_COBERTURA { get; set; }
        public List<string> NM_BAIRRO { get; set; }
        public List<int?> CD_MUNICIPIO { get; set; }
        public List<string> SG_UF { get; set; }
        public List<int?> LG_RESIDE_EXTERIOR { get; set; }
        public List<DateTime?> DT_REATIVACAO { get; set; }
        public List<DateTime?> DT_ULTIMA_REATIVACAO { get; set; }
        public List<DateTime?> DT_ULTIMA_MUDA_CONTRATUAL { get; set; }
        public List<DateTime?> DT_CANCELAMENTO { get; set; }
        public List<DateTime?> DT_ULTIMO_CANCELAMENTO { get; set; }
        public List<int?> DT_BENE_MOTIV_CANCELAMENTO { get; set; }
        public List<DateTime?> DT_CARGA { get; set; }

        public CSVObject()
        {
            this.indice = 0;

            // this.SK_IDARQUIVO_CSV = new List<int>() { };
            this.ID_TEMPO_COMPETENCIA = new List<int?>() { };
            this.CD_OPERADORA = new List<int?>() { };
            this.DT_INCLUSAO = new List<DateTime?>() { };
            this.CD_BENE_MOTV_INCLUSAO = new List<int?>() { };
            this.IND_PORTABILIDADE = new List<string>() { };
            this.ID_MOTIVO_MOVIMENTO = new List<int?>() { };
            this.LG_BENEFICIARIO_ATIVO = new List<int?>() { };
            this.DT_NASCIMENTO = new List<DateTime?>() { };
            this.TP_SEXO = new List<string>() { };
            this.CD_PLANO_RPS = new List<int?>() { };
            this.CD_PLANO_SCPA = new List<string>() { };
            this.NR_PLANO_PORTABILIDADE = new List<int?>() { };
            this.DT_PRIMEIRA_CONTRATACAO = new List<DateTime?>() { };
            this.DT_CONTRATACAO = new List<DateTime?>() { };
            this.ID_BENE_TIPO_DEPENDENTE = new List<int?>() { };
            this.LG_COBERTURA_PARCIAL = new List<int?>() { };
            this.LG_ITEM_EXCLUIDO_COBERTURA = new List<int?>() { };
            this.NM_BAIRRO = new List<string>() { };
            this.CD_MUNICIPIO = new List<int?>() { };
            this.SG_UF = new List<string>() { };
            this.LG_RESIDE_EXTERIOR = new List<int?>() { };
            this.DT_REATIVACAO = new List<DateTime?>() { };
            this.DT_ULTIMA_REATIVACAO = new List<DateTime?>() { };
            this.DT_ULTIMA_MUDA_CONTRATUAL = new List<DateTime?>() { };
            this.DT_CANCELAMENTO = new List<DateTime?>() { };
            this.DT_ULTIMO_CANCELAMENTO = new List<DateTime?>() { };
            this.DT_BENE_MOTIV_CANCELAMENTO = new List<int?>() { };
            this.DT_CARGA = new List<DateTime?>() { };


        }
        public void addLine(int? ID_TEMPO_COMPETENCIA, int? CD_OPERADORA, DateTime? DT_INCLUSAO, int? CD_BENE_MOTV_INCLUSAO, string IND_PORTABILIDADE, int? ID_MOTIVO_MOVIMENTO,
                                int? LG_BENEFICIARIO_ATIVO, DateTime? DT_NASCIMENTO, string TP_SEXO, int? CD_PLANO_RPS, string CD_PLANO_SCPA, int? NR_PLANO_PORTABILIDADE,
                                DateTime? DT_PRIMEIRA_CONTRATACAO, DateTime? DT_CONTRATACAO, int? ID_BENE_TIPO_DEPENDENTE, int? LG_COBERTURA_PARCIAL,
                                int? LG_ITEM_EXCLUIDO_COBERTURA, string NM_BAIRRO, int? CD_MUNICIPIO, string SG_UF, int? LG_RESIDE_EXTERIOR, DateTime? DT_REATIVACAO,
                                DateTime? DT_ULTIMA_REATIVACAO, DateTime? DT_ULTIMA_MUDA_CONTRATUAL, DateTime? DT_CANCELAMENTO, DateTime? DT_ULTIMO_CANCELAMENTO,
                                int? DT_BENE_MOTIV_CANCELAMENTO, DateTime? DT_CARGA)
        {

            /// this.SK_IDARQUIVO_CSV.Add(SK_IDARQUIVO_CSV);

            this.ID_TEMPO_COMPETENCIA.Add(ID_TEMPO_COMPETENCIA);

            this.CD_OPERADORA.Add(CD_OPERADORA);
            this.DT_INCLUSAO.Add(DT_INCLUSAO);
            this.CD_BENE_MOTV_INCLUSAO.Add(CD_BENE_MOTV_INCLUSAO);
            this.IND_PORTABILIDADE.Add(IND_PORTABILIDADE);
            this.ID_MOTIVO_MOVIMENTO.Add(ID_MOTIVO_MOVIMENTO);
            this.LG_BENEFICIARIO_ATIVO.Add(LG_BENEFICIARIO_ATIVO);
            this.DT_NASCIMENTO.Add(DT_NASCIMENTO);
            this.TP_SEXO.Add(TP_SEXO);
            this.CD_PLANO_RPS.Add(CD_PLANO_RPS);
            this.CD_PLANO_SCPA.Add(CD_PLANO_SCPA);
            this.NR_PLANO_PORTABILIDADE.Add(NR_PLANO_PORTABILIDADE);
            this.DT_PRIMEIRA_CONTRATACAO.Add(DT_PRIMEIRA_CONTRATACAO);
            this.DT_CONTRATACAO.Add(DT_CONTRATACAO);
            this.ID_BENE_TIPO_DEPENDENTE.Add(ID_BENE_TIPO_DEPENDENTE);
            this.LG_COBERTURA_PARCIAL.Add(LG_COBERTURA_PARCIAL);
            this.LG_ITEM_EXCLUIDO_COBERTURA.Add(LG_ITEM_EXCLUIDO_COBERTURA);
            this.NM_BAIRRO.Add(NM_BAIRRO);
            this.CD_MUNICIPIO.Add(CD_MUNICIPIO);
            this.SG_UF.Add(SG_UF);
            this.LG_RESIDE_EXTERIOR.Add(LG_RESIDE_EXTERIOR);
            this.DT_REATIVACAO.Add(DT_REATIVACAO);
            this.DT_ULTIMA_REATIVACAO.Add(DT_ULTIMA_REATIVACAO);
            this.DT_ULTIMA_MUDA_CONTRATUAL.Add(DT_ULTIMA_MUDA_CONTRATUAL);
            this.DT_CANCELAMENTO.Add(DT_CANCELAMENTO);
            this.DT_ULTIMO_CANCELAMENTO.Add(DT_ULTIMO_CANCELAMENTO);
            this.DT_BENE_MOTIV_CANCELAMENTO.Add(DT_BENE_MOTIV_CANCELAMENTO);
            this.DT_CARGA.Add(DT_CARGA);

            this.Length = this.indice++;

        }
        public CSVObject(int LengthNow, List<int?> ID_TEMPO_COMPETENCIA_TT, List<int?> CD_OPERADORA_TT, List<DateTime?> DT_INCLUSAO_TT, List<int?> CD_BENE_MOTV_INCLUSAO_TT, List<string> IND_PORTABILIDADE_TT, List<int?> ID_MOTIVO_MOVIMENTO_TT,
                                List<int?> LG_BENEFICIARIO_ATIVO_TT, List<DateTime?> DT_NASCIMENTO_TT, List<string> TP_SEXO_TT, List<int?> CD_PLANO_RPS_TT, List<string> CD_PLANO_SCPA_TT, List<int?> NR_PLANO_PORTABILIDADE_TT,
                                List<DateTime?> DT_PRIMEIRA_CONTRATACAO_TT, List<DateTime?> DT_CONTRATACAO_TT, List<int?> ID_BENE_TIPO_DEPENDENTE_TT, List<int?> LG_COBERTURA_PARCIAL_TT,
                                List<int?> LG_ITEM_EXCLUIDO_COBERTURA_TT, List<string> NM_BAIRRO_TT, List<int?> CD_MUNICIPIO_TT, List<string> SG_UF_TT, List<int?> LG_RESIDE_EXTERIOR_TT, List<DateTime?> DT_REATIVACAO_TT,
                                List<DateTime?> DT_ULTIMA_REATIVACAO_TT, List<DateTime?> DT_ULTIMA_MUDA_CONTRATUAL_TT, List<DateTime?> DT_CANCELAMENTO_TT, List<DateTime?> DT_ULTIMO_CANCELAMENTO_TT,
                                List<int?> DT_BENE_MOTIV_CANCELAMENTO_TT, List<DateTime?> DT_CARGA_TT)
        {
            this.Length = this.indice = LengthNow;

            /// this.SK_IDARQUIVO_CSV = SK_IDARQUIVO_CSV_TT;

            this.ID_TEMPO_COMPETENCIA = ID_TEMPO_COMPETENCIA_TT;

            this.CD_OPERADORA = CD_OPERADORA_TT;
            this.DT_INCLUSAO = DT_INCLUSAO_TT;
            this.CD_BENE_MOTV_INCLUSAO = CD_BENE_MOTV_INCLUSAO_TT;
            this.IND_PORTABILIDADE = IND_PORTABILIDADE_TT;
            this.ID_MOTIVO_MOVIMENTO = ID_MOTIVO_MOVIMENTO_TT;
            this.LG_BENEFICIARIO_ATIVO = LG_BENEFICIARIO_ATIVO_TT;
            this.DT_NASCIMENTO = DT_NASCIMENTO_TT;
            this.TP_SEXO = TP_SEXO_TT;
            this.CD_PLANO_RPS = CD_PLANO_RPS_TT;
            this.CD_PLANO_SCPA = CD_PLANO_SCPA_TT;
            this.NR_PLANO_PORTABILIDADE = NR_PLANO_PORTABILIDADE_TT;
            this.DT_PRIMEIRA_CONTRATACAO = DT_PRIMEIRA_CONTRATACAO_TT;
            this.DT_CONTRATACAO = DT_CONTRATACAO_TT;
            this.ID_BENE_TIPO_DEPENDENTE = ID_BENE_TIPO_DEPENDENTE_TT;
            this.LG_COBERTURA_PARCIAL = LG_COBERTURA_PARCIAL_TT;
            this.LG_ITEM_EXCLUIDO_COBERTURA = LG_ITEM_EXCLUIDO_COBERTURA_TT;
            this.NM_BAIRRO = NM_BAIRRO_TT;
            this.CD_MUNICIPIO = CD_MUNICIPIO_TT;
            this.SG_UF = SG_UF_TT;
            this.LG_RESIDE_EXTERIOR = LG_RESIDE_EXTERIOR_TT;
            this.DT_REATIVACAO = DT_REATIVACAO_TT;
            this.DT_ULTIMA_REATIVACAO = DT_ULTIMA_REATIVACAO_TT;
            this.DT_ULTIMA_MUDA_CONTRATUAL = DT_ULTIMA_MUDA_CONTRATUAL_TT;
            this.DT_CANCELAMENTO = DT_CANCELAMENTO_TT;
            this.DT_ULTIMO_CANCELAMENTO = DT_ULTIMO_CANCELAMENTO_TT;
            this.DT_BENE_MOTIV_CANCELAMENTO = DT_BENE_MOTIV_CANCELAMENTO_TT;
            this.DT_CARGA = DT_CARGA_TT;
            
        }
    }
}
