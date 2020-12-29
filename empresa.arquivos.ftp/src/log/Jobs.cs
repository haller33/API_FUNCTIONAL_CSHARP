using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Quartz;

namespace Empresa.integracao.chat.src.log
{
    class Jobs
    {



        private readonly Guid _jobGui = Guid.NewGuid();
        protected readonly Logger _logger;
        protected Scope Scope;
        protected JobContext JobContext;

        public Guid JobGuid
        {
            get { return _jobGui; }
        }


        protected AbstractJobBase()
        {
            try
            {
                _logger = LogManager.GetCurrentClassLogger();
                Scope = RegistrosUzeService.BeginLifetimeScope();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }



        public void Execute(IJobExecutionContext context)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            try
            {

                JobContext.Builder builder = new JobContext.Builder(
                    context.JobDetail.JobDataMap.GetInt(TipoConfiguracao.QuantidadeProcessamento.ToString()),
                    context.JobDetail.JobDataMap.GetString("IdInstancia"),
                    context.JobDetail.JobDataMap.GetString("NomeInstancia")
                );

                if (context.JobDetail.JobDataMap.ContainsKey(TipoConfiguracao.Argumento.ToString()))
                    builder.AndArgumento(context.JobDetail.JobDataMap.GetString(TipoConfiguracao.Argumento.ToString()));

                JobContext = builder.Build();
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    string.Format("Ocorreu um problema na leitura das configurações do job [{0:s}].",
                    JobGuid.ToString()));

                throw new Exception("Algumas configurações do job não foram lidas corretamente.", ex);
            }

            try
            {
                ExecuteJob(context);
            }
            catch (Exception)
            {
                // Não faz tratamento, pois esse já deve ter sido feito dentro do próprio JOB que realizou uma tarefa.
            }
            finally
            {
                Scope?.Dispose();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


        /// <summary>
        /// Conjunto de tarefas a serem executadas pelo job.
        /// </summary>
        /// <param name="context">Informações de contexto de execução do job.</param>
        public abstract void ExecuteJob(IJobExecutionContext context);
    }
}
