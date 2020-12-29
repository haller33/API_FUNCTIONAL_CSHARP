using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Empresa.integracao.ftp.filesystem;
using Empresa.integracao.ftp.log;
using Empresa.integracao.ftp.conection;

namespace Empresa.integracao.ftp.csvParse
{
    static class Parser
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            if (index + length > data.Length)
                length = data.Length - index;

            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        public static List<T> Slice<T>(this List<T> li, int start, int end)
        {
            if (start < 0)    // support negative indexing
            {
                start = li.Count + start;
            }
            if (end < 0)    // support negative indexing
            {
                end = li.Count + end;
            }
            if (start > li.Count)    // if the start value is too high
            {
                start = li.Count;
            }
            if (end > li.Count)    // if the end value is too high
            {
                end = li.Count;
            }
            var count = end - start;             // calculate count (number of elements)
            return li.GetRange(start, count);    // return a shallow copy of li of count elements
        }
        public static IEnumerable<CSVObject> SplitCSV(CSVObject actualySplity, int interval)
        {
            int contador = 0;
            for (int i = 0; i < actualySplity.ID_MOTIVO_MOVIMENTO.Count; i += interval)
            {
                if (interval > actualySplity.ID_MOTIVO_MOVIMENTO.Count)    // if the end value is too high
                    interval = actualySplity.ID_MOTIVO_MOVIMENTO.Count;

                contador++;

                yield return new CSVObject(Slice(actualySplity.ID_TEMPO_COMPETENCIA, i, interval * contador).Count,
                                            Slice(actualySplity.ID_TEMPO_COMPETENCIA, i, interval*contador),
                                            Slice(actualySplity.CD_OPERADORA, i, interval * contador),
                                            Slice(actualySplity.DT_INCLUSAO, i, interval * contador),
                                            Slice(actualySplity.CD_BENE_MOTV_INCLUSAO, i, interval * contador),
                                            Slice(actualySplity.IND_PORTABILIDADE, i, interval * contador),
                                            Slice(actualySplity.ID_MOTIVO_MOVIMENTO, i, interval * contador),
                                            Slice(actualySplity.LG_BENEFICIARIO_ATIVO, i, interval * contador),
                                            Slice(actualySplity.DT_NASCIMENTO, i, interval * contador),
                                            Slice(actualySplity.TP_SEXO, i, interval * contador),
                                            Slice(actualySplity.CD_PLANO_RPS, i, interval * contador),
                                            Slice(actualySplity.CD_PLANO_SCPA, i, interval * contador),
                                            Slice(actualySplity.NR_PLANO_PORTABILIDADE, i, interval * contador),
                                            Slice(actualySplity.DT_PRIMEIRA_CONTRATACAO, i, interval * contador),
                                            Slice(actualySplity.DT_CONTRATACAO, i, interval * contador),
                                            Slice(actualySplity.ID_BENE_TIPO_DEPENDENTE, i, interval * contador),
                                            Slice(actualySplity.LG_COBERTURA_PARCIAL, i, interval * contador),
                                            Slice(actualySplity.LG_ITEM_EXCLUIDO_COBERTURA, i, interval * contador),
                                            Slice(actualySplity.NM_BAIRRO, i, interval * contador),
                                            Slice(actualySplity.CD_MUNICIPIO, i, interval * contador),
                                            Slice(actualySplity.SG_UF, i, interval * contador),
                                            Slice(actualySplity.LG_RESIDE_EXTERIOR, i, interval * contador),
                                            Slice(actualySplity.DT_REATIVACAO, i, interval * contador),
                                            Slice(actualySplity.DT_ULTIMA_REATIVACAO, i, interval * contador),
                                            Slice(actualySplity.DT_ULTIMA_MUDA_CONTRATUAL, i, interval * contador),
                                            Slice(actualySplity.DT_CANCELAMENTO, i, interval * contador),
                                            Slice(actualySplity.DT_ULTIMO_CANCELAMENTO, i, interval * contador),
                                            Slice(actualySplity.DT_BENE_MOTIV_CANCELAMENTO, i, interval * contador),
                                            Slice(actualySplity.DT_CARGA, i, interval * contador));
                
            }
        }
        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 500000)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
        public static IEnumerable<CSVObject> ParseCSVFile (string csvFileNamePath, Regex regx, int maxChunck) ///, OracleConection ora, int insercao)
        {
            string line = "";

            // int lines = File.ReadLines(csvFileNamePath).Count();
            // var reader = new StreamReader(File.OpenRead(csvFileNamePath));
            
            var fileREader = File.ReadLines(csvFileNamePath);
            
            using (var iterator = fileREader.GetEnumerator())
            {
                // iterator.MoveNext();

                CSVObject noe = new CSVObject();

                bool flagFristLine = true;

                int contador = maxChunck;

                while (iterator.MoveNext())
                {
                    /*##ID_TEMPO_COMPETENCIA;CD_OPERADORA[1];DT_INCLUSAO[2];CD_BENE_MOTV_INCLUSAO[3];IND_PORTABILIDADE[4];ID_MOTIVO_MOVIMENTO[5];LG_BENEFICIARIO_ATIVO[6]
                    * ;DT_NASCIMENTO[7];TP_SEXO[8];CD_PLANO_RPS[9];CD_PLANO_SCPA[10];NR_PLANO_PORTABILIDADE[11];DT_PRIMEIRA_CONTRATACAO[12];DT_CONTRATACAO[13];ID_BENE_TIPO_DEPENDENTE[14]
                    * ;LG_COBERTURA_PARCIAL[15];LG_ITEM_EXCLUIDO_COBERTURA[16];NM_BAIRRO[17];CD_MUNICIPIO[18];SG_UF[19];LG_RESIDE_EXTERIOR[20];DT_REATIVACAO[21];DT_ULTIMA_REATIVACAO[22]
                    * ;DT_ULTIMA_MUDA_CONTRATUAL[23];DT_CANCELAMENTO[24];DT_ULTIMO_CANCELAMENTO[25];DT_BENE_MOTIV_CANCELAMENTO[26];DT_CARGA[27]*/
                    line = iterator.Current;

                    // CentralLog.LogInfo(line);

                    /// .Split(';'); // Obsolete because of ';' inside '"' double cote // .Replace("\"", "") // super slow
                    
                    if (!flagFristLine)
                    {
                        string[] colunsOfLine = regx.Split(line);
                        if (colunsOfLine.Length == 28)
                        {
                            noe.addLine(String.IsNullOrEmpty(colunsOfLine[0]) ? (int?)null : int.Parse(colunsOfLine[0].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[1]) ? (int?)null : int.Parse(colunsOfLine[1].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[2]) ? (DateTime?)null : Convert.ToDateTime(colunsOfLine[2].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[3]) ? (int?)null : int.Parse(colunsOfLine[3].Replace("\"", "")),
                                        colunsOfLine[4].Replace("\"", ""),
                                        String.IsNullOrEmpty(colunsOfLine[5]) ? (int?)null : int.Parse(colunsOfLine[5].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[6]) ? (int?)null : int.Parse(colunsOfLine[6].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[7]) ? (DateTime?)null : Convert.ToDateTime(colunsOfLine[7].Replace("\"", "")),
                                        colunsOfLine[8].Replace("\"", ""),
                                        String.IsNullOrEmpty(colunsOfLine[9]) ? (int?)null : int.Parse(colunsOfLine[9].Replace("\"", "")),
                                        colunsOfLine[10].Replace("\"", ""),
                                        String.IsNullOrEmpty(colunsOfLine[11]) ? (int?)null : int.Parse(colunsOfLine[11].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[12]) ? (DateTime?)null : Convert.ToDateTime(colunsOfLine[12].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[13]) ? (DateTime?)null : Convert.ToDateTime(colunsOfLine[13].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[14]) ? (int?)null : int.Parse(colunsOfLine[14].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[15]) ? (int?)null : int.Parse(colunsOfLine[15].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[16]) ? (int?)null : int.Parse(colunsOfLine[16].Replace("\"", "")),
                                        colunsOfLine[17].Replace("\"", ""),
                                        String.IsNullOrEmpty(colunsOfLine[18]) ? (int?)null : int.Parse(colunsOfLine[18].Replace("\"", "")),
                                        colunsOfLine[19].Replace("\"", ""),
                                        String.IsNullOrEmpty(colunsOfLine[20]) ? (int?)null : int.Parse(colunsOfLine[20].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[21]) ? (DateTime?)null : Convert.ToDateTime(colunsOfLine[21].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[22]) ? (DateTime?)null : Convert.ToDateTime(colunsOfLine[22].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[23]) ? (DateTime?)null : Convert.ToDateTime(colunsOfLine[23].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[24]) ? (DateTime?)null : Convert.ToDateTime(colunsOfLine[24].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[25]) ? (DateTime?)null : Convert.ToDateTime(colunsOfLine[25].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[26]) ? (int?)null : int.Parse(colunsOfLine[26].Replace("\"", "")),
                                        String.IsNullOrEmpty(colunsOfLine[27]) ? (DateTime?)null : Convert.ToDateTime(colunsOfLine[27].Replace("\"", "")));
                        }
                        else
                        {
                            CentralLog.LogInfo("Not Inserting Line: " + line);
                        }
                    }

                    if (contador <= 0)
                    {
                        contador = maxChunck;
                        CSVObject par1se = noe;
                        yield return par1se;
                        noe = new CSVObject();
                    }
                    else
                    {
                        contador--;
                    }

                    if (flagFristLine)
                        flagFristLine = false;
                }
                yield return noe;
            }
        }
    }
}
