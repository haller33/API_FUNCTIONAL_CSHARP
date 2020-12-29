using System;
using System.ServiceProcess;
using System.Timers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Empresa.integracao.ftp.filesystem;
using System.Configuration;
using ExtensionMethods;
using System.Text.RegularExpressions;
using Empresa.integracao.ftp.log;
using Empresa.integracao.ftp.conection;
using Empresa.integracao.ftp.csvParse;
using System.Collections.Generic;


namespace Empresa.integracao.ftp
{
    public partial class Service1 : ServiceBase
    {
        static readonly Func<string, string> readFile = new Func<string, string>(textFilePath => ReadFILE.ReadFile(textFilePath));

        static readonly Func<string, Func<string, MatchCollection>> myRegex = new Func<string, string, MatchCollection>((reg, tex) => 
            (new Regex(reg)).Matches(tex)).Curry();

        static readonly Func<int, Func<MatchCollection, bool>> matchesNumberIsGratherThan = 
            new Func<int, Func<MatchCollection, bool>>(other => matches => matches.Count > other);

        static readonly Func<string, Func<string, bool>> myRegexNew = new Func<string, string, bool>((reg , tex) => (new Regex(reg)).Match(tex).Success).Curry();

        static readonly Func<string, Func<string, Match>> myRegexNewThing = new Func<string, string, Match>((reg, tex) => (new Regex(reg)).Match(tex)).Curry();

        static readonly Func<string, Func<Func<string, Match>, Func<string, string>>> genMatcherGroupStringOF = 
            new Func<string, Func<string, Match>, string, string>((groupo, myGroupGx, texto) =>
                myGroupGx(texto).Groups[groupo].Value).Curry();

        static readonly Func<bool, bool> Not = new Func<bool, bool>(some => !some);

        static readonly string FINAL_FILES_MOVE = "FINALIZADOS";
        public Service1()
        {

            CentralLog.LogInfo("Start Service");

            InitializeComponent();
        }
        
        public static async void Machine(string path)
        {

            var extract = new Func<string, string, string>((source, destiny) => ReadFILE.extracZip(source, destiny));

            var applySameInputChangeOtherWith = new Func<Func<string, string>, Func<string, string, string>, string, string>((change, functionToApply, inputOf) =>
                                                functionToApply(inputOf, change(inputOf))).Curry();


            var applySameInputChangeTheFirstWith = new Func<Func<string, string>, Func<string, string, string>, string, string>((change, functionToApply, inputOf) =>
                                                functionToApply(change(inputOf), inputOf)).Curry();
            

            var getOnlyPathOfFileTo = applySameInputChangeOtherWith(ReadFILE.pathName);

            var extractFilesWhereIs = getOnlyPathOfFileTo(extract);


            var moveFilesOf = new Func<string, string, string>((source, pathTo) => ReadFILE.moveFile(source, pathTo));

            var concatStrings = new Func<string, string, string>((stringToConat, stringSource) => stringSource + stringToConat).Curry();

            var AddDirectoryToFinsh = concatStrings(Path.DirectorySeparatorChar + Service1.FINAL_FILES_MOVE);

            var getDirectoryNameParent = new Func<string, string>(pathOfFile => ReadFILE.getParent(pathOfFile));

            var getFileName = new Func<string, string>(pathOfFile => ReadFILE.fileNameInPath(pathOfFile));



            var ConcatOnlyDirectoryToFinsh = AddDirectoryToFinsh.Compose(getDirectoryNameParent);

            var concatyStrings = new Func<string, string, string>((some, sometoo) => some + sometoo).Curry();

            var concatyFilenameToNewDirectoryInFinalizados = new Func<string, string>(somePathFile => /// TODO: not pure function wiet
                   ConcatOnlyDirectoryToFinsh(somePathFile) + Path.DirectorySeparatorChar + getFileName(somePathFile));


            var getOnlyThePathForFinalizadosWithNameAnd = applySameInputChangeOtherWith(concatyFilenameToNewDirectoryInFinalizados);

            var extractFilesToFinalizado = getOnlyThePathForFinalizadosWithNameAnd(moveFilesOf);

            var extractAndMoveFilesToFinalizados = extractFilesToFinalizado.Compose(extractFilesWhereIs);


            try
            {
                //// List of Finalizados

                string groupoNaoProcessadoTestar = "FILEPARAPROCESSAR";
                string groupoFinlizado = "FILEFINALIZADO";

                string myregexForZipFilesInFinalizados = ConfigurationManager.AppSettings["regexForGetFinalizedFiles"];
                string myregexForZipFilesFinalizados = ConfigurationManager.AppSettings["regexForPathIsInFinalizados"];
                string myregexForExtracFileOfNaoProcessado = ConfigurationManager.AppSettings["regexForExtracFileNameZipInNaoProcessados"];
                string myregexForExtracFileNameOfCSVInNaoProcessado = ConfigurationManager.AppSettings["regexForExtracFileNameCSVInNaoProcessados"];

                var pathCheckerZipFinalizados = myRegexNewThing(myregexForZipFilesInFinalizados);

                var getGroupoOfFinalizadosInRegex = genMatcherGroupStringOF(groupoNaoProcessadoTestar);

                var pathToFileFinalizado = getGroupoOfFinalizadosInRegex(pathCheckerZipFinalizados);


                var pathExtractNaoProcessadoFileZip = myRegexNewThing(myregexForExtracFileOfNaoProcessado);

                var fileCSVNameExtracInNaoProcessados = myRegexNewThing(myregexForExtracFileNameOfCSVInNaoProcessado); 

                var getGroupoNaoProcesadosTestar = genMatcherGroupStringOF(groupoNaoProcessadoTestar);

                var extraiFileNameZipNaoProcessados = getGroupoOfFinalizadosInRegex(pathExtractNaoProcessadoFileZip);

                var extraiCSVFileNameInNaoProcessados = getGroupoOfFinalizadosInRegex(fileCSVNameExtracInNaoProcessados);

                var genSha256SumOfFilePath = new Func<string, string>( pathof => ReadFILE.GetSha256(pathof) );


                var concatStringWithVirgulaIfNotNull = new Func<string, string>(stringFinalizados => stringFinalizados.Length > 1 ?
                    stringFinalizados + "," : stringFinalizados);

                //// GET LIST OF FINALIZADOS
                var funcToGenPathStringForArray = concatStringWithVirgulaIfNotNull.Compose(genSha256SumOfFilePath);

                var checkIfPAthIsFinalizado = myRegexNew(myregexForZipFilesFinalizados);


                OpAmp myCheckPath = new OpAmp(checkIfPAthIsFinalizado);
                OpBack myComposeFinalResult = new OpBack(funcToGenPathStringForArray);

                CentralLog.LogInfo("Gerando Lista de Ja Finalizados");

                string ListOfFinalizadosWithVirgula = ReadFILE.recursiveCallByDelegate(path, myCheckPath, myComposeFinalResult);



                // Debug
                //CentralLog.LogInfo(ListOfFinalizadosWithVirgula);

                char[] trimChars = { ' ', ',' };

                ListOfFinalizadosWithVirgula = ListOfFinalizadosWithVirgula.Trim(trimChars);

                // CentralLog.LogInfo(ListOfFinalizadosWithVirgula);

                List<string> listFinalizadosHashsum = new List<string>(ListOfFinalizadosWithVirgula.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));

                /// CentralLog.LogInfo(ListOfFinalizadosWithVirgula);
                CentralLog.LogInfo(listFinalizadosHashsum.Count.ToString());


                /// Download From FTP ZIP FILE

                string caminho = ConfigurationManager.AppSettings["ftpPath"];
                string arquivo = ConfigurationManager.AppSettings["ftpFile"];
                string destiny = ConfigurationManager.AppSettings["ftpDownloadPath"];

                string pathDownloadTo = path + Path.DirectorySeparatorChar + destiny;

                string logFtpDownloiad = pathDownloadTo + Path.DirectorySeparatorChar + arquivo;

                // string arquivos = Ftp.AcessarFtp(caminho, "zip");

                CentralLog.LogInfo("Download File From FTP");

                CentralLog.LogInfo(logFtpDownloiad);
                Ftp.Download(caminho, arquivo, pathDownloadTo);


                var testStringIsNotInArrayOfStringWithFunctionForComparation = new Func<Func<string, string>, List<string>, Func<string, Func<string, Func<string, bool>>>, string, bool>(
                    (funcToConvert, arrayOF, functionToHandlerOf, stringNow) => {

                        var checkActualStringAndItemOfListWithProcessedString = functionToHandlerOf(funcToConvert(stringNow)); // can be changed

                        return (arrayOF.Count > 0) ? Not(arrayOF.Aggregate(false, (acc, nowOfList) =>
                            acc || checkActualStringAndItemOfListWithProcessedString(stringNow)(nowOfList))) : 
                                Not(checkActualStringAndItemOfListWithProcessedString(stringNow)(""));
                    }).Curry();

                var verificaseZipJaFoiFinalizado = new Func<Func<string, bool>, Func<string, string>, string, string>
                    ((test, run, stringOf) => test(stringOf) ? run(stringOf) : "").Curry();

                var checkIfStringIsEqualToAnother = new Func<string, string, bool>((stringToCompareOf, stringCompared) => 
                    stringToCompareOf.Equals(stringCompared));

                var handlerStringAndUsingFunctionToProcessWith = new Func<Func<string, bool>, Func<string, Func<string, bool>>, 
                                    Func<string, string, bool>, string, string, string, bool>( 
                    (checkPrcess, processData, funcForProcessingOfStrings, stringComparation, stringOf, compareToNow) =>
                    {
                        bool resut = funcForProcessingOfStrings(compareToNow, stringComparation);
                        
                        if (resut)
                            return resut;

                        if (checkPrcess(stringComparation))
                            return false;

                        processData(stringOf)(stringComparation);

                        return resut;
                    }).Curry();

                var checkElementExistInTable = new Func<OracleConection, string, string, string, bool>((oraNow, tableName, ColumnName, dataToCheck) =>
                   {
                       return OracleConection.verifyIfSomeValueExistInTableWithColumn(oraNow, tableName, ColumnName, dataToCheck);
                   }).Curry();

                var getaFunctionThatInsertInDatabaseTwoStringsProcessingWith = new Func<Func<string, string>, string, string, OracleConection, string, string, bool>(
                                (handleString, tableOf, columnOftableOf, oraNow, pathOfCSV, hashOfCSV) => {

                    OracleConection.insertFileNameAndHash(oraNow, tableOf, columnOftableOf, handleString(pathOfCSV), hashOfCSV);
                    return false;
                }).Curry();


                var InsertingCSVFileHashInDataBaseOn = getaFunctionThatInsertInDatabaseTwoStringsProcessingWith(extraiFileNameZipNaoProcessados);

                OracleConection ora = new OracleConection();

                string tableForInsertingZIP = ConfigurationManager.AppSettings["tabeladeInsercaoDoHistoricoDosArquivoZipsBaixados"];

                string columnForIncrementZIP = ConfigurationManager.AppSettings["columnDaTabelaDosZipsQueDeveSerIncrementado"];

                string columnOfZipTableToCheck = ConfigurationManager.AppSettings["columnDaTabelaZipParaCheckarHash"];


                var checkHashOfRowWithTableWith = checkElementExistInTable(ora);

                var checkRowOfColumnOfHashWith = checkHashOfRowWithTableWith(tableForInsertingZIP);

                var checkDataInTableZipsForHashOn = checkRowOfColumnOfHashWith(columnOfZipTableToCheck);


                string tableOfZipFiles = tableForInsertingZIP;

                string skColoumnOftableOfZipFiles = columnForIncrementZIP;

                var genSha256OfFilePathAndCompareToListOf = testStringIsNotInArrayOfStringWithFunctionForComparation(genSha256SumOfFilePath);

                var testaPathDeFileNaoExisteNaListaDeFinalizados = genSha256OfFilePathAndCompareToListOf(listFinalizadosHashsum);

                

                var insertingInTableDimWithColumn = InsertingCSVFileHashInDataBaseOn(tableOfZipFiles);

                var InsertingNameAndHsahOfFileInTableDimWithConection = insertingInTableDimWithColumn(skColoumnOftableOfZipFiles);


                var insertingPathAndHashOfCSVFile =  InsertingNameAndHsahOfFileInTableDimWithConection(ora);

                var alsoCheckingTheHashsInTableOfZipAndProcessDataWith = handlerStringAndUsingFunctionToProcessWith(checkDataInTableZipsForHashOn);

                var insertingStringOfPathAndHashThenCheckStringWith = alsoCheckingTheHashsInTableOfZipAndProcessDataWith(insertingPathAndHashOfCSVFile);

                var getPahAndStringOfListAndCompareToPathOfCSVFile = insertingStringOfPathAndHashThenCheckStringWith(checkIfStringIsEqualToAnother);


                var checkIfFileExistInListOfFinalizadosThenInsertFileAndHashInDataBaseDim = testaPathDeFileNaoExisteNaListaDeFinalizados(getPahAndStringOfListAndCompareToPathOfCSVFile);


                var verificasePathEhFinalizadoEntao = verificaseZipJaFoiFinalizado(checkIfFileExistInListOfFinalizadosThenInsertFileAndHashInDataBaseDim);

                var testaPahSeFoiFinalizadoSenaoExtraieMoveArquivosPAraFinalizado = verificasePathEhFinalizadoEntao(extractAndMoveFilesToFinalizados);


                //// Find and Extract all zip Fiels
                string myregexForZipFilestoExtract = ConfigurationManager.AppSettings["regexForZipFilestoExtract"];

                var pathCheckerZip = myRegexNew(myregexForZipFilestoExtract);

                var finalComutation = testaPahSeFoiFinalizadoSenaoExtraieMoveArquivosPAraFinalizado;


                myCheckPath = new OpAmp(pathCheckerZip);
                myComposeFinalResult = new OpBack(finalComutation);

                CentralLog.LogInfo("Movendo e Extraindo Arquivos");

                string logSucess = ReadFILE.recursiveCallByDelegate(path, myCheckPath, myComposeFinalResult);

                CentralLog.LogInfo("Extraidos:: " + logSucess);
                


                // Find and Join all Json Files
                string myregexForCSVFilesexp = ConfigurationManager.AppSettings["regexForCSVFilesexp"];

                var pathcheckisJsonFile = myRegexNew(myregexForCSVFilesexp);

                CentralLog.LogInfo("Inserindo Dados");



                string splitCSVFilesREGEX = ConfigurationManager.AppSettings["SplitRegexFileCSV"];

                Regex regxForSplit = new Regex(splitCSVFilesREGEX);

                int MAXREADER = int.Parse(ConfigurationManager.AppSettings["MAXREADERFILECSV"]);
                int MAXINSERTION = int.Parse(ConfigurationManager.AppSettings["MAXINSERTIONTOORACLEDB"]);



                string rowIdentifyOfTable = ConfigurationManager.AppSettings["identificadorDeLinhaDaTabela"];

                string tableZipForSharedTable = ConfigurationManager.AppSettings["tableOfZIPInserted"]; 

                string tableForInsertingCSVFilesHash = ConfigurationManager.AppSettings["tableForCSVInserting"];

                string principalTableForInserting = ConfigurationManager.AppSettings["tablePrincipalForInsertingDetalhe"];



                var myHandlerCSV = new Func<Regex, int, int, string, Task>(async (regexOfSplit, maxReadByFile, maxInsertByChunck, somePath) =>
                {

                    int idArquivoCSV = await OracleConection.insertFileNameAndHashWithRelation(ora, rowIdentifyOfTable, tableZipForSharedTable,
                                                tableForInsertingCSVFilesHash, rowIdentifyOfTable, extraiCSVFileNameInNaoProcessados(somePath), 
                                                                    genSha256SumOfFilePath(somePath));

                    CentralLog.LogInfo("Arquivo: " + somePath);
                    foreach (CSVObject s in Parser.ParseCSVFile(somePath, regexOfSplit, maxReadByFile))
                    {

                        foreach (var i in Parser.SplitCSV(s, maxInsertByChunck))
                        {
                            await Task.Run(async () =>
                            {
                                CentralLog.LogInfo("Populando CSV Insertion");

                                CentralLog.LogInfo(somePath + ":: inserindo Bloco +:" + maxInsertByChunck.ToString());
                                
                                await OracleConection.insereDIMDetalhes(i, ora, principalTableForInserting, rowIdentifyOfTable, tableForInsertingCSVFilesHash, 
                                                                                        rowIdentifyOfTable, idArquivoCSV);

                                CentralLog.LogInfo("Requisição de Liberação de Memoria");
                                GC.Collect();
                            });
                        }
                    }
                }).Curry();

                var SplitFileWithComaInsideAndDotInQuotationSETING = myHandlerCSV(regxForSplit);
                var maxReadByFileIsMAXREADER = SplitFileWithComaInsideAndDotInQuotationSETING(MAXREADER);
                var maxInsertionByChunckIsMAXINSERTION = maxReadByFileIsMAXREADER(MAXINSERTION);

                CentralLog.LogInfo("Lendo CSV's");

                myCheckPath = new OpAmp(pathcheckisJsonFile);
                OpConBack myComposeFinalResultAsync = new OpConBack(maxInsertionByChunckIsMAXINSERTION);

                await ReadFILE.linearCallByFile(path, myCheckPath, myComposeFinalResultAsync);
                // 4x

                ///// Close Conection
                /// ora.Close();


                //// Find and Remove All Processed Files
                string myregexForRemoveFilesYetProcessed = ConfigurationManager.AppSettings["regexForRemoveFilesYetProcessed"];


                var checkisTheFileInDirectoryToDelete = myRegexNew(myregexForRemoveFilesYetProcessed);

                var deleteFile = new Func<string, string>( pathFile =>
                {
                    ReadFILE.deleteFile(pathFile);
                    
                    return pathFile + ",";

                });
                

                myCheckPath = new OpAmp(checkisTheFileInDirectoryToDelete);
                myComposeFinalResult = new OpBack(deleteFile);

                CentralLog.LogInfo("Removendo Arquivos");

                string deletion = ReadFILE.linearCallByFileForDeleting(path, myCheckPath, myComposeFinalResult);


                CentralLog.LogInfo("Deletados:: " + deletion);
                /// 4X

                CentralLog.LogInfo("Finsh");

                int horas = Convert.ToInt16(ConfigurationManager.AppSettings["TempoEmHoras"]);

                CentralLog.LogInfo("Next Rerun in " + horas.ToString() + " horas");


            }
            catch (Exception e)
            {
                CentralLog.LogError(e, $@"::ERROR:: MACHINE HAVE SOME ERROR ::ERROR::");
            }
            finally
            {
            }
        }


        public void OnDebug ()
        {
            OnStart(null);
        }

        public static void Burn (Object source, System.Timers.ElapsedEventArgs e)
        {
            BeginBurn();
        } 
        public static void BeginBurn  ()
        {
            try
            {
                CentralLog.LogInfo("Begin Burn");

                string path = ConfigurationManager.AppSettings["Path"];

                CentralLog.LogInfo(path);

                Machine(path);

            }
            catch (Exception err)
            {
                CentralLog.LogError(err, "Error in Execut Burn Execution");
                throw err;
            }
        }
        private static void RunService()
        {
            int horas = Convert.ToInt16(ConfigurationManager.AppSettings["TempoEmHoras"]);
            var aTimer = new System.Timers.Timer( horas * 60 * 60 * 1000); //one hour in milliseconds
            aTimer.Elapsed += new ElapsedEventHandler(Burn);
            aTimer.Start();
        }
        private static void BeginService()
        {
            var aTimer = new System.Timers.Timer(3 * 1000); 
            aTimer.Elapsed += new ElapsedEventHandler(Burn);
            aTimer.AutoReset = false;
            aTimer.Start();
        }
        protected override void OnStart(string[] args)
        {
            RunService();
            BeginService();
        }

        protected override void OnStop()
        {
            CentralLog.LogInfo("Stop Service");
        }
    }
}
