using System;
using System.Data;
using System.Configuration;
using System.Threading.Tasks;
using Oracle.DataAccess.Client;
using Empresa.integracao.ftp.filesystem;
using Empresa.integracao.ftp.log;
using System.Collections.Generic;

namespace Empresa.integracao.ftp.conection
{
    class OracleConection
    {
        public OracleConnection con { get; set; }
        

        private string beginConectionString;

        public void burnoutConection()
        {
            this.beginConectionString = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
        }

        public string conectionString()
        {
            return this.beginConectionString;
        }

        public OracleConection()
        {
            burnoutConection();
        }
        public void Connect()
        {
            try
            {
                con = new OracleConnection();


                con.ConnectionString = this.beginConectionString;
                con.Open();
                // Console.WriteLine("Connected to Oracle " + con.ServerVersion);

                CentralLog.LogInfo("Connected to Oracle " + con.ServerVersion);

            }
            catch (Exception e)
            {
                // Console.WriteLine(e);
                // this.Close();
                CentralLog.LogError(e, " Error In Conection");
            }
        }

        public void Close()
        {
            try
            {
                con.Close();
                con.Dispose();

                CentralLog.LogInfo("Close Conection");
            }
            catch (Exception e)
            {
                // Console.WriteLine(e);

                CentralLog.LogError(e, " Error In Close Conection");
            }
        }

        public static void PreparaDados(List<CSVObject> jobs, OracleConection ora, string destTableName)
        {
            try
            {
                int maximo = 200000;

                int contador = 0;

                do
                {

                    DataTable dt = new DataTable();
                    dt.Columns.Add("SK_ID");
                    dt.Columns.Add("SK_IDARQUIVO_CSV");
                    dt.Columns.Add("ID_TEMPO_COMPETENCIA");
                    dt.Columns.Add("CD_OPERADORA");
                    dt.Columns.Add("DT_ULTIMO_CANCELAMENTO");
                    dt.Columns.Add("DT_BENE_MOTIV_CANCELAMENTO");
                    dt.Columns.Add("DT_CARGA");
                    dt.Columns.Add("DT_INSERTED");

                    
                    for (int i = 0; i < maximo && (jobs.Count > (i + contador)); i++)
                    {

                        DataRow dr = dt.NewRow();
                        dr["SK_ID"] = i+contador;
                        dr["SK_IDARQUIVO_CSV"] = 1;
                     
                        dr["DT_ULTIMO_CANCELAMENTO"] = jobs[contador + i].DT_ULTIMO_CANCELAMENTO;
                        dr["DT_BENE_MOTIV_CANCELAMENTO"] = jobs[contador + i].DT_BENE_MOTIV_CANCELAMENTO;
                        dr["DT_CARGA"] = jobs[contador + i].DT_CARGA;
                        dr["DT_INSERTED"] = DateTime.Now;

                        dt.Rows.Add(dr);
                        
                    }

                    OracleConection.insereDIMDetalhesOld(dt, ora, destTableName);

                    contador = maximo+contador;

                } while ( ( contador + 1 ) > jobs.Count);


            } catch (Exception e)
            {
                CentralLog.LogError(e, $@"::ERROR:: Insert Row! ::ERROR::");
            }
        
        }
        public OracleCommand CreateCommand()
        {
            return this.con.CreateCommand();
        }
        private static OracleDbType GetOracleDbType(object o)
        {
            if (o is string) return OracleDbType.Varchar2;
            if (o is DateTime) return OracleDbType.Date;
            if (o is Int64) return OracleDbType.Int64;
            if (o is Int32) return OracleDbType.Int32;
            if (o is Int16) return OracleDbType.Int16;
            if (o is sbyte) return OracleDbType.Byte;
            if (o is byte) return OracleDbType.Int16; /// == unverified
            if (o is decimal) return OracleDbType.Decimal;
            if (o is float) return OracleDbType.Single;
            if (o is double) return OracleDbType.Double;
            if (o is byte[]) return OracleDbType.Blob;

            return OracleDbType.Varchar2;
        }
        public static bool verifyIfSomeValueExistInTableWithColumn (OracleConection conLocal, string tableToCheck, string columnToGet, string dataToVerify)
        {
            try
            {
                using (var connection = new Oracle.DataAccess.Client.OracleConnection(conLocal.conectionString()))
                {
                    connection.Open();

                    OracleCommand cmd = new OracleCommand();
                    cmd.CommandText = $"SELECT COUNT({columnToGet}) FROM {tableToCheck} WHERE {columnToGet} = '{dataToVerify}'";
                    cmd.CommandType = CommandType.Text;

                    cmd.Connection = connection;

                    CentralLog.LogInfo($"Check {columnToGet} In Table {tableToCheck}");
                    
                    return (int.Parse(cmd.ExecuteScalar().ToString()) > 0);
                }
            } catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN EXECUTION VERIFICATION OF LINE OF COLUMN::");
                return false;
            }
        }
        public static async Task<int> insertFileNameAndHashWithRelation(OracleConection conLocal, string columnOfRelation, string tableOfRelation, string tableName, string ColumnOfTableToCount,
                                                            string nomeArquivo, string hashFile)
        {
            try
            {
                using (var connection = new Oracle.DataAccess.Client.OracleConnection(conLocal.conectionString()))
                {
                    CentralLog.LogInfo("Create Conection");
                    connection.Open();
                    

                    string sql = $" INSERT INTO {tableName} (SK_ID, SK_IDARQUIVO_ZIP, NM_ARQUIVO, DT_ARQUIVO, DT_CRIACAO, NM_HASH)" +

                                " VALUES (:SK_ID_TT, :SK_IDARQUIVO_ZIP_TT ,:NM_IDARQUIVO_TT, :DT_ARQUIVO_TT, :DT_CRIACAO_TT, :NM_HASH_TT) ";


                    int relationToAgree = await OracleConection.countTheMaxVaueOfSomeColumn(conLocal, columnOfRelation, tableOfRelation);


                    int numberOfLines = await OracleConection.countTheMaxVaueOfSomeColumn(conLocal, ColumnOfTableToCount, tableName);

                    numberOfLines++;

                    OracleCommand cmd = new OracleCommand(sql, connection);

                    OracleParameter[] parameters = new OracleParameter[] {
                            new OracleParameter("SK_ID_TT", numberOfLines),
                            new OracleParameter("SK_IDARQUIVO_ZIP_TT", relationToAgree),
                            new OracleParameter("NM_IDARQUIVO_TT", nomeArquivo),
                            new OracleParameter("DT_ARQUIVO_TT", DateTime.Now),
                            new OracleParameter("DT_CRIACAO_TT", DateTime.Now),
                            new OracleParameter("NM_HASH_TT", hashFile)
                      };

                    cmd.Parameters.AddRange(parameters);

                    return await Task.Run(() => {
                        try {

                            CentralLog.LogInfo("Inserting CSV Hash " + nomeArquivo);
                            cmd.ExecuteNonQuery();
                            return numberOfLines;
                        }
                        catch (Exception e)
                        {
                            CentralLog.LogError(e, "::ERROR IN EXECUTION INSERTING LINE OF HASH FILE::");
                            return 0;
                        }
                    });
                }
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, $@"::ERROR:: INSERTING LINE! ::ERROR::");
                return await Task.Run(() => 0);
            }
        }

        public static async Task<int> countTheMaxVaueOfSomeColumn(OracleConection conLocal, string column, string tableName)
        {
            try
            {
                using (var connection = new Oracle.DataAccess.Client.OracleConnection(conLocal.conectionString()))
                {
                    connection.Open();

                    IDbCommand command = new OracleCommand();
                    command.CommandText = $"SELECT NVL(MAX({column}), 0) AS MAX_VALUE FROM {tableName}";
                    command.CommandType = CommandType.Text;

                    command.Connection = connection;
                    return await Task.Run(() => {
                        try
                        {
                            return int.Parse(command.ExecuteScalar().ToString());
                        }
                        catch (Exception e)
                        {
                            CentralLog.LogError(e, "::ERROR IN EXECUTION MAX VALUE OF COLUMN::");
                            return 0;
                        }
                    });
                }
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN MAX VALUE OF ELEMENTS::");
                return await Task.Run(() => 0);
            }
        }
        public static async void insertFileNameAndHash(OracleConection conLocal, string tableName, string ColumnOfTableToCount,
                                                            string nomeArquivo, string hashFile)
        {
            try
            {
                using (var connection = new Oracle.DataAccess.Client.OracleConnection(conLocal.conectionString()))
                {
                    CentralLog.LogInfo("Create Conection");
                    connection.Open();


                    string sql = $" INSERT INTO {tableName} (SK_ID, NM_ARQUIVO, DT_ARQUIVO, DT_CRIACAO, NM_HASH)" +

                                " VALUES (:SK_ID_TT, :NM_IDARQUIVO_TT, :DT_ARQUIVO_TT, :DT_CRIACAO_TT, :NM_HASH_TT) ";

                    int numberOfLines = await OracleConection.countTheMaxVaueOfSomeColumn(conLocal, ColumnOfTableToCount, tableName);

                    numberOfLines++;

                    OracleCommand cmd = new OracleCommand(sql, connection);

                    OracleParameter[] parameters = new OracleParameter[] {
                            new OracleParameter("SK_ID_TT", numberOfLines),
                            new OracleParameter("NM_IDARQUIVO_TT", nomeArquivo),
                            new OracleParameter("DT_ARQUIVO_TT", DateTime.Now),
                            new OracleParameter("DT_CRIACAO_TT", DateTime.Now),
                            new OracleParameter("NM_HASH_TT", hashFile)
                      };

                    cmd.Parameters.AddRange(parameters);

                    await Task.Run(() => {
                        try
                        {

                            CentralLog.LogInfo("Inserting ZIP Hash");
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            CentralLog.LogError(e, "::ERROR IN EXECUTION INSERTING LINE OF HASH FILE::");
                        }
                    });
                }
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, $@"::ERROR:: INSERTING LINE! ::ERROR::");
            }
        }
        public static async Task<int> countNumberOfElements(OracleConection conLocal, string column, string tableName)
        {
            try
            {
                using (var connection = new Oracle.DataAccess.Client.OracleConnection(conLocal.conectionString()))
                {
                    connection.Open();

                    IDbCommand command = new OracleCommand();
                    command.CommandText = $"SELECT COUNT({column}) FROM {tableName}";
                    command.CommandType = CommandType.Text;

                    command.Connection = connection;
                    return await Task.Run(() => {
                        try {
                            return int.Parse(command.ExecuteScalar().ToString());
                        } catch (Exception e)
                        {
                            CentralLog.LogError(e, "::ERROR IN EXECUTION COUNT NUMBER OF ELEMENTS::");
                            return 0;
                        }
                    });
                }
            } catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN COUNT NUMBER OF ELEMENTS::");
                return await Task.Run(() => 0);
            }
        }

        public static async Task<bool> insereDIMDetalhes(CSVObject jobst, OracleConection conLocal, string destTableName, string idofTableInserting, string tableCSVRelation,
                                                string columnOfTableCSV, int idArquivoCSV)
        {
            try
            {
                using (var connection = new Oracle.DataAccess.Client.OracleConnection(conLocal.conectionString()))
                {

                    List<int> SK_ID_CSV = new List<int>() { };
                    List<int> SK_IDARQUIVO_CSV = new List<int>() { };
                    List<DateTime> DT_INSERTED_TT = new List<DateTime> { };


                    int masID = await OracleConection.countTheMaxVaueOfSomeColumn(conLocal, idofTableInserting, destTableName);

                    for (int j = 0; j < jobst.Length; j++)
                    {
                        SK_ID_CSV.Add(++masID);
                        SK_IDARQUIVO_CSV.Add(idArquivoCSV); ///relationToAgree;
                        DT_INSERTED_TT.Add(DateTime.Now);
                    }


                    //OracleParameter SK_ID_SS = new OracleParameter();
                    //SK_ID_SS.OracleDbType = GetOracleDbType(jobst.SK_ID[0]);
                    //SK_ID_SS.Value = jobst.SK_ID;
                    

                    OracleParameter SK_ID_SS = new OracleParameter();
                    SK_ID_SS.OracleDbType = OracleDbType.Int32;  ///  OracleConection.GetOracleDbType(SK_ID_CSV[0]); ///jobst.SK_IDARQUIVO_CSV[0]);
                    SK_ID_SS.Value = SK_ID_CSV.ToArray(); //// jobst.SK_IDARQUIVO_CSV.ToArray();
                    
                    OracleParameter SK_IDARQUIVO_CSV_SS = new OracleParameter();
                    SK_IDARQUIVO_CSV_SS.OracleDbType = OracleDbType.Int32; ///OracleConection.GetOracleDbType(SK_IDARQUIVO_CSV[0]); ///jobst.SK_IDARQUIVO_CSV[0]);
                    SK_IDARQUIVO_CSV_SS.Value = SK_IDARQUIVO_CSV.ToArray(); //// jobst.SK_IDARQUIVO_CSV.ToArray();

                    OracleParameter ID_TEMPO_COMPETENCIA_SS = new OracleParameter();
                    ID_TEMPO_COMPETENCIA_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.ID_TEMPO_COMPETENCIA[0]);
                    ID_TEMPO_COMPETENCIA_SS.Value = jobst.ID_TEMPO_COMPETENCIA.ToArray();

                    OracleParameter CD_OPERADORA_SS = new OracleParameter();
                    CD_OPERADORA_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.CD_OPERADORA[0]);
                    CD_OPERADORA_SS.Value = jobst.CD_OPERADORA.ToArray();

                    OracleParameter DT_INCLUSAO_SS = new OracleParameter();
                    DT_INCLUSAO_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_INCLUSAO[0]);
                    DT_INCLUSAO_SS.Value = jobst.DT_INCLUSAO.ToArray();

                    OracleParameter CD_BENE_MOTV_INCLUSAO_SS = new OracleParameter();
                    CD_BENE_MOTV_INCLUSAO_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.CD_BENE_MOTV_INCLUSAO[0]);
                    CD_BENE_MOTV_INCLUSAO_SS.Value = jobst.CD_BENE_MOTV_INCLUSAO.ToArray();

                    OracleParameter IND_PORTABILIDADE_SS = new OracleParameter();
                    IND_PORTABILIDADE_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.IND_PORTABILIDADE[0]);
                    IND_PORTABILIDADE_SS.Value = jobst.IND_PORTABILIDADE.ToArray();

                    OracleParameter ID_MOTIVO_MOVIMENTO_SS = new OracleParameter();
                    ID_MOTIVO_MOVIMENTO_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.ID_MOTIVO_MOVIMENTO[0]);
                    ID_MOTIVO_MOVIMENTO_SS.Value = jobst.ID_MOTIVO_MOVIMENTO.ToArray();

                    OracleParameter LG_BENEFICIARIO_ATIVO_SS = new OracleParameter();
                    LG_BENEFICIARIO_ATIVO_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.LG_BENEFICIARIO_ATIVO[0]);
                    LG_BENEFICIARIO_ATIVO_SS.Value = jobst.LG_BENEFICIARIO_ATIVO.ToArray();

                    OracleParameter DT_NASCIMENTO_SS = new OracleParameter();
                    DT_NASCIMENTO_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_NASCIMENTO[0]);
                    DT_NASCIMENTO_SS.Value = jobst.DT_NASCIMENTO.ToArray();

                    OracleParameter TP_SEXO_SS = new OracleParameter();
                    TP_SEXO_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.TP_SEXO[0]);
                    TP_SEXO_SS.Value = jobst.TP_SEXO.ToArray();

                    OracleParameter CD_PLANO_RPS_SS = new OracleParameter();
                    CD_PLANO_RPS_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.CD_PLANO_RPS[0]);
                    CD_PLANO_RPS_SS.Value = jobst.CD_PLANO_RPS.ToArray();

                    OracleParameter CD_PLANO_SCPA_SS = new OracleParameter();
                    CD_PLANO_SCPA_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.CD_PLANO_SCPA[0]);
                    CD_PLANO_SCPA_SS.Value = jobst.CD_PLANO_SCPA.ToArray();

                    OracleParameter NR_PLANO_PORTABILIDADE_SS = new OracleParameter();
                    NR_PLANO_PORTABILIDADE_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.NR_PLANO_PORTABILIDADE[0]);
                    NR_PLANO_PORTABILIDADE_SS.Value = jobst.NR_PLANO_PORTABILIDADE.ToArray();

                    OracleParameter DT_PRIMEIRA_CONTRATACAO_SS = new OracleParameter();
                    DT_PRIMEIRA_CONTRATACAO_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_PRIMEIRA_CONTRATACAO[0]);
                    DT_PRIMEIRA_CONTRATACAO_SS.Value = jobst.DT_PRIMEIRA_CONTRATACAO.ToArray();

                    OracleParameter DT_CONTRATACAO_SS = new OracleParameter();
                    DT_CONTRATACAO_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_CONTRATACAO[0]);
                    DT_CONTRATACAO_SS.Value = jobst.DT_CONTRATACAO.ToArray();

                    OracleParameter ID_BENE_TIPO_DEPENDENTE_SS = new OracleParameter();
                    ID_BENE_TIPO_DEPENDENTE_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.ID_BENE_TIPO_DEPENDENTE[0]);
                    ID_BENE_TIPO_DEPENDENTE_SS.Value = jobst.ID_BENE_TIPO_DEPENDENTE.ToArray();

                    OracleParameter LG_COBERTURA_PARCIAL_SS = new OracleParameter();
                    LG_COBERTURA_PARCIAL_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.LG_COBERTURA_PARCIAL[0]);
                    LG_COBERTURA_PARCIAL_SS.Value = jobst.LG_COBERTURA_PARCIAL.ToArray();

                    OracleParameter LG_ITEM_EXCLUIDO_COBERTURA_SS = new OracleParameter();
                    LG_ITEM_EXCLUIDO_COBERTURA_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.LG_ITEM_EXCLUIDO_COBERTURA[0]);
                    LG_ITEM_EXCLUIDO_COBERTURA_SS.Value = jobst.LG_ITEM_EXCLUIDO_COBERTURA.ToArray();

                    OracleParameter NM_BAIRRO_SS = new OracleParameter();
                    NM_BAIRRO_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.NM_BAIRRO[0]);
                    NM_BAIRRO_SS.Value = jobst.NM_BAIRRO.ToArray();

                    OracleParameter CD_MUNICIPIO_SS = new OracleParameter();
                    CD_MUNICIPIO_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.CD_MUNICIPIO[0]);
                    CD_MUNICIPIO_SS.Value = jobst.CD_MUNICIPIO.ToArray();

                    OracleParameter SG_UF_SS = new OracleParameter();
                    SG_UF_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.SG_UF[0]);
                    SG_UF_SS.Value = jobst.SG_UF.ToArray();

                    OracleParameter LG_RESIDE_EXTERIOR_SS = new OracleParameter();
                    LG_RESIDE_EXTERIOR_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.LG_RESIDE_EXTERIOR[0]);
                    LG_RESIDE_EXTERIOR_SS.Value = jobst.LG_RESIDE_EXTERIOR.ToArray();

                    OracleParameter DT_REATIVACAO_SS = new OracleParameter();
                    DT_REATIVACAO_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_REATIVACAO[0]);
                    DT_REATIVACAO_SS.Value = jobst.DT_REATIVACAO.ToArray();

                    OracleParameter DT_ULTIMA_REATIVACAO_SS = new OracleParameter();
                    DT_ULTIMA_REATIVACAO_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_ULTIMA_REATIVACAO[0]);
                    DT_ULTIMA_REATIVACAO_SS.Value = jobst.DT_ULTIMA_REATIVACAO.ToArray();

                    OracleParameter DT_ULTIMA_MUDA_CONTRATUAL_SS = new OracleParameter();
                    DT_ULTIMA_MUDA_CONTRATUAL_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_ULTIMA_MUDA_CONTRATUAL[0]);
                    DT_ULTIMA_MUDA_CONTRATUAL_SS.Value = jobst.DT_ULTIMA_MUDA_CONTRATUAL.ToArray();

                    OracleParameter DT_CANCELAMENTO_SS = new OracleParameter();
                    DT_CANCELAMENTO_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_CANCELAMENTO[0]);
                    DT_CANCELAMENTO_SS.Value = jobst.DT_CANCELAMENTO.ToArray();

                    OracleParameter DT_ULTIMO_CANCELAMENTO_SS = new OracleParameter();
                    DT_ULTIMO_CANCELAMENTO_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_ULTIMO_CANCELAMENTO[0]);
                    DT_ULTIMO_CANCELAMENTO_SS.Value = jobst.DT_ULTIMO_CANCELAMENTO.ToArray();

                    OracleParameter DT_BENE_MOTIV_CANCELAMENTO_SS = new OracleParameter();
                    DT_BENE_MOTIV_CANCELAMENTO_SS.OracleDbType = OracleConection.GetOracleDbType(jobst.DT_BENE_MOTIV_CANCELAMENTO[0]);
                    DT_BENE_MOTIV_CANCELAMENTO_SS.Value = jobst.DT_BENE_MOTIV_CANCELAMENTO.ToArray();

                    OracleParameter DT_CARGA_SS = new OracleParameter();
                    DT_CARGA_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_CARGA[0]);
                    DT_CARGA_SS.Value = jobst.DT_CARGA.ToArray();

                    OracleParameter DT_INSERTED_SS = new OracleParameter();
                    DT_INSERTED_SS.OracleDbType = OracleDbType.Date; //OracleConection.GetOracleDbType(jobst.DT_INSERTED[0]);
                    DT_INSERTED_SS.Value = DT_INSERTED_TT.ToArray();


                    // create command and set properties  
                    // OracleCommand cmd = conLocal.CreateCommand();

                    CentralLog.LogInfo("Create Conection");
                    connection.Open(); 
                    OracleCommand cmd = connection.CreateCommand(); // DIM_ARQUIVOS_DETALHE
                    cmd.CommandText = @"INSERT INTO DIM_ARQUIVOS_DETALHE (SK_ID, SK_IDARQUIVO_CSV, ID_TEMPO_COMPETENCIA, CD_OPERADORA, 
                            DT_INCLUSAO, CD_BENE_MOTV_INCLUSAO, IND_PORTABILIDADE, ID_MOTIVO_MOVIMENTO, LG_BENEFICIARIO_ATIVO, 
                            DT_NASCIMENTO, TP_SEXO, CD_PLANO_RPS, CD_PLANO_SCPA, NR_PLANO_PORTABILIDADE, DT_PRIMEIRA_CONTRATACAO, 
                            DT_CONTRATACAO, ID_BENE_TIPO_DEPENDENTE, LG_COBERTURA_PARCIAL, LG_ITEM_EXCLUIDO_COBERTURA, 
                            NM_BAIRRO, CD_MUNICIPIO, SG_UF, LG_RESIDE_EXTERIOR, DT_REATIVACAO, DT_ULTIMA_REATIVACAO, 
                            DT_ULTIMA_MUDA_CONTRATUAL, DT_CANCELAMENTO, DT_ULTIMO_CANCELAMENTO, DT_BENE_MOTIV_CANCELAMENTO, 
                            DT_CARGA, DT_INSERTED)

                            VALUES (:1, :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, 
                                        :12, :13, :14, :15, :16, :17, :18, :19, :20, :21, 
                                        :22, :23, :24, :25, 
                                        :26, :27, :28, 
                                        :29, :30, :31 )  ";

                    cmd.ArrayBindCount = jobst.Length;

                    /// cmd.Parameters.Add(SK_ID_SS); // [1] // BECAUSE THIS IS NOT MORE NECESSARY. 
                    /// WE DONT NEED TO COUNT MORE.
                    /// 
                    cmd.Parameters.Add(SK_ID_SS); // [1]
                    cmd.Parameters.Add(SK_IDARQUIVO_CSV_SS); // [2]
                    cmd.Parameters.Add(ID_TEMPO_COMPETENCIA_SS); // [3]
                    cmd.Parameters.Add(CD_OPERADORA_SS); // [4]
                    cmd.Parameters.Add(DT_INCLUSAO_SS); // [5]
                    cmd.Parameters.Add(CD_BENE_MOTV_INCLUSAO_SS); // [6]
                    cmd.Parameters.Add(IND_PORTABILIDADE_SS); // [7]
                    cmd.Parameters.Add(ID_MOTIVO_MOVIMENTO_SS); // [8]
                    cmd.Parameters.Add(LG_BENEFICIARIO_ATIVO_SS);  // [9]
                    cmd.Parameters.Add(DT_NASCIMENTO_SS); // [10]
                    cmd.Parameters.Add(TP_SEXO_SS); /// [11]
                    cmd.Parameters.Add(CD_PLANO_RPS_SS); // [12]
                    cmd.Parameters.Add(CD_PLANO_SCPA_SS); // [13]
                    cmd.Parameters.Add(NR_PLANO_PORTABILIDADE_SS); // [14]
                    cmd.Parameters.Add(DT_PRIMEIRA_CONTRATACAO_SS); // [15]
                    cmd.Parameters.Add(DT_CONTRATACAO_SS); // [16]
                    cmd.Parameters.Add(ID_BENE_TIPO_DEPENDENTE_SS); // [17]
                    cmd.Parameters.Add(LG_COBERTURA_PARCIAL_SS); // [18]
                    cmd.Parameters.Add(LG_ITEM_EXCLUIDO_COBERTURA_SS);  // [19]
                    cmd.Parameters.Add(NM_BAIRRO_SS); // [20]
                    cmd.Parameters.Add(CD_MUNICIPIO_SS);  // [21]
                    cmd.Parameters.Add(SG_UF_SS); // [22]
                    cmd.Parameters.Add(LG_RESIDE_EXTERIOR_SS); // [23]
                    cmd.Parameters.Add(DT_REATIVACAO_SS); /// [24]
                    cmd.Parameters.Add(DT_ULTIMA_REATIVACAO_SS); // [25]
                    cmd.Parameters.Add(DT_ULTIMA_MUDA_CONTRATUAL_SS); // [26]
                    cmd.Parameters.Add(DT_CANCELAMENTO_SS); // [27]
                    cmd.Parameters.Add(DT_ULTIMO_CANCELAMENTO_SS); // [28]
                    cmd.Parameters.Add(DT_BENE_MOTIV_CANCELAMENTO_SS); // [29]
                    cmd.Parameters.Add(DT_CARGA_SS); /// [30]
                    cmd.Parameters.Add(DT_INSERTED_SS); // [31]

                    CentralLog.LogInfo("inserting");
                   
                    return await Task.Run(() => {
                        try
                        {
                            cmd.ExecuteNonQuery();
                            return true;
                        }
                        catch (Exception e)
                        {
                            CentralLog.LogError(e, "::ERROR IN EXECUTION COUNT NUMBER OF ELEMENTS::");
                            return false;
                        }
                    });
                    // connection.Close();
                    // connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                CentralLog.LogError(ex, "::ERROR IN INSERTIN CSV FILE::");
                return await Task.Run(() => false);
            }
        }

        public static void insereDIMDetalhesOld(DataTable dt, OracleConection conLocal, string destTableName)
        {
            try
            {
                using (var connection = new Oracle.DataAccess.Client.OracleConnection(conLocal.conectionString()))
                {
                    connection.Open();
                    using (var bulkCopy = new Oracle.DataAccess.Client.OracleBulkCopy(connection, Oracle.DataAccess.Client.OracleBulkCopyOptions.UseInternalTransaction))
                    {
                        bulkCopy.DestinationTableName = destTableName;
                        bulkCopy.BulkCopyTimeout = 600;
                        bulkCopy.WriteToServer(dt);
                    }
                    connection.Close();
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void insere(CSVObject job, OracleConection conLocal, int numc)
        {
            try
            {
                string sql = @"INSERT INTO DIM_ARQUIVOS_DETALHE (SK_ID, SK_IDARQUIVO_CSV, ID_TEMPO_COMPETENCIA, CD_OPERADORA, 
                                DT_INCLUSAO, CD_BENE_MOTV_INCLUSAO, IND_PORTABILIDADE, ID_MOTIVO_MOVIMENTO, LG_BENEFICIARIO_ATIVO, 
                               DT_NASCIMENTO, TP_SEXO, CD_PLANO_RPS, CD_PLANO_SCPA, NR_PLANO_PORTABILIDADE, DT_PRIMEIRA_CONTRATACAO, 
                                DT_CONTRATACAO, ID_BENE_TIPO_DEPENDENTE, LG_COBERTURA_PARCIAL, LG_ITEM_EXCLUIDO_COBERTURA, 
                                NM_BAIRRO, CD_MUNICIPIO, SG_UF, LG_RESIDE_EXTERIOR, DT_REATIVACAO, DT_ULTIMA_REATIVACAO, 
                               DT_ULTIMA_MUDA_CONTRATUAL, DT_CANCELAMENTO, DT_ULTIMO_CANCELAMENTO, DT_BENE_MOTIV_CANCELAMENTO, 
                                DT_CARGA, DT_INSERTED)

                    VALUES (:SK_ID_TT, :SK_IDARQUIVO_CSV_TT, :ID_TEMPO_COMPETENCIA_TT, :CD_OPERADORA_TT, 
                                :DT_INCLUSAO_TT, :CD_BENE_MOTV_INCLUSAO_TT, :IND_PORTABILIDADE_TT, :ID_MOTIVO_MOVIMENTO_TT, :LG_BENEFICIARIO_ATIVO_TT, 
                               :DT_NASCIMENTO_TT, :TP_SEXO_TT, :CD_PLANO_RPS_TT, :CD_PLANO_SCPA_TT, :NR_PLANO_PORTABILIDADE_TT, :DT_PRIMEIRA_CONTRATACAO_TT, 
                                :DT_CONTRATACAO_TT, :ID_BENE_TIPO_DEPENDENTE_TT, :LG_COBERTURA_PARCIAL_TT, :LG_ITEM_EXCLUIDO_COBERTURA_TT, 
                                :NM_BAIRRO_TT, :CD_MUNICIPIO_TT, :SG_UF_TT, :LG_RESIDE_EXTERIOR_TT, :DT_REATIVACAO_TT, :DT_ULTIMA_REATIVACAO_TT, 
                               :DT_ULTIMA_MUDA_CONTRATUAL_TT, :DT_CANCELAMENTO_TT, :DT_ULTIMO_CANCELAMENTO_TT, :DT_BENE_MOTIV_CANCELAMENTO_TT, 
                                :DT_CARGA_TT, :DT_INSERTED_TT) ";


                OracleCommand cmd = new OracleCommand(sql, conLocal.con);

                OracleParameter[] parameters = new OracleParameter[] {
                        new OracleParameter("SK_ID_TT", numc),
                        new OracleParameter("SK_IDARQUIVO_CSV_TT", 1),
                        new OracleParameter("ID_TEMPO_COMPETENCIA_TT", job.ID_TEMPO_COMPETENCIA),
                        new OracleParameter("CD_OPERADORA_TT", job.CD_OPERADORA),
                        new OracleParameter("DT_INCLUSAO_TT", job.DT_INCLUSAO),
                        new OracleParameter("CD_BENE_MOTV_INCLUSAO_TT", job.CD_BENE_MOTV_INCLUSAO),
                        new OracleParameter("IND_PORTABILIDADE_TT", job.IND_PORTABILIDADE),
                        new OracleParameter("ID_MOTIVO_MOVIMENTO_TT", job.ID_MOTIVO_MOVIMENTO),
                        new OracleParameter("LG_BENEFICIARIO_ATIVO_TT", job.LG_BENEFICIARIO_ATIVO),
                        new OracleParameter("DT_NASCIMENTO_TT", job.DT_NASCIMENTO),
                        new OracleParameter("TP_SEXO_TT", job.TP_SEXO),
                        new OracleParameter("CD_PLANO_RPS_TT", job.CD_PLANO_RPS),
                        new OracleParameter("CD_PLANO_SCPA_TT", job.CD_PLANO_SCPA),
                        new OracleParameter("NR_PLANO_PORTABILIDADE_TT", job.NR_PLANO_PORTABILIDADE),
                        new OracleParameter("DT_PRIMEIRA_CONTRATACAO_TT", job.DT_PRIMEIRA_CONTRATACAO),
                        new OracleParameter("DT_CONTRATACAO_TT", job.DT_CONTRATACAO),
                        new OracleParameter("ID_BENE_TIPO_DEPENDENTE_TT", job.ID_BENE_TIPO_DEPENDENTE),
                        new OracleParameter("LG_COBERTURA_PARCIAL_TT", job.LG_COBERTURA_PARCIAL),
                        new OracleParameter("LG_ITEM_EXCLUIDO_COBERTURA_TT", job.LG_ITEM_EXCLUIDO_COBERTURA),
                        new OracleParameter("NM_BAIRRO_TT", job.NM_BAIRRO),
                        new OracleParameter("CD_MUNICIPIO_TT", job.CD_MUNICIPIO),
                        new OracleParameter("SG_UF_TT", job.SG_UF),
                        new OracleParameter("LG_RESIDE_EXTERIOR_TT", job.LG_RESIDE_EXTERIOR),
                        new OracleParameter("DT_REATIVACAO_TT", job.DT_REATIVACAO),
                        new OracleParameter("DT_ULTIMA_REATIVACAO_TT", job.DT_ULTIMA_REATIVACAO),
                        new OracleParameter("DT_ULTIMA_MUDA_CONTRATUAL_TT", job.DT_ULTIMA_MUDA_CONTRATUAL),
                        new OracleParameter("DT_CANCELAMENTO_TT", job.DT_CANCELAMENTO),
                        new OracleParameter("DT_ULTIMO_CANCELAMENTO_TT", job.DT_ULTIMO_CANCELAMENTO),
                        new OracleParameter("DT_BENE_MOTIV_CANCELAMENTO_TT", job.DT_BENE_MOTIV_CANCELAMENTO),
                        new OracleParameter("DT_CARGA_TT", job.DT_CARGA),
                        new OracleParameter("DT_INSERTED_TT",  DateTime.Now)
                  };

                cmd.Parameters.AddRange(parameters);

                // cmd.Parameters.Add(new OracleParameter("1", OracleDbType.Varchar2, 4, "fred", ));


                cmd.ExecuteNonQuery();

                // Console.WriteLine("Row Done!");
                // Console.Write(job.SK_MARCA + " ");
                // Console.WriteLine(id);

                // CentralLog.LogInfo("Insert Row:: " + job.SK_MARCA + " - " + id); // super bloat

            }
            catch (Exception e)
            {
                ;
                // Console.WriteLine(e);
                // CentralLog.LogError(e, $@"::ERROR:: Insert Row! ::ERROR:: {job.SK_MARCA} - {id}
             //                            ### {MensagemTableDim.ConvertToString(job)}");
            }
        }
    }
}
