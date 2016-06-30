using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using FluentScheduler;

namespace CommandCentral.CronOperations
{
    /// <summary>
    /// Initializes the cron operation methods found throughout the application and provides methods for interacting with the running operations.
    /// </summary>
    public static class CronOperationsManager
    {
        /// <summary>
        /// Initializes the cron operation methods found throughout the application.
        /// </summary>
        public static void InitializeCronOperations()
        {
            Communicator.PostMessageToHost("Stopping cron operations...", Communicator.MessageTypes.CronOperation);

            //Stop the job manager until we're done.
            JobManager.Stop();

            //Remove all jobs
            foreach (var job in JobManager.AllSchedules)
            {
                JobManager.RemoveJob(job.Name);
            }

            //Now scan for all jobs
            var methods = ScanForCronOperationMethods();

            //And then validate them and get the things back.
            IEnumerable<Tuple<CronMethodAttribute, Action>> compiledCronOperations = ValidateAndCompileCronOperationMethods(methods);

            Communicator.PostMessageToHost("Running cron operation methods...", Communicator.MessageTypes.CronOperation);

            //And finally, run all the methods.
            foreach (var group in compiledCronOperations)
            {
                group.Item2();
            }

            //And then tell the host what happened.
            Communicator.PostMessageToHost("{0} cron operation(s) registered from {1} method(s).".FormatS(JobManager.AllSchedules.Count(), compiledCronOperations.Count()), Communicator.MessageTypes.CronOperation);

            Communicator.PostMessageToHost("Starting cron operations...", Communicator.MessageTypes.CronOperation);
            JobManager.Start();
            Communicator.PostMessageToHost("Cron operations started...", Communicator.MessageTypes.CronOperation);
        }

        /// <summary>
        /// Scans the entire executing assembly for cron operation methods.
        /// </summary>
        /// <returns></returns>
        private static List<MethodInfo> ScanForCronOperationMethods()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                    .SelectMany(x => x.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
                    .Where(x => x.GetCustomAttribute<CronMethodAttribute>() != null)
                    .ToList();
        }

        /// <summary>
        /// Validates and then compiles the given cron operation method infos.
        /// </summary>
        /// <param name="methods"></param>
        /// <returns></returns>
        private static IEnumerable<Tuple<CronMethodAttribute, Action>> ValidateAndCompileCronOperationMethods(IEnumerable<MethodInfo> methods)
        {
            foreach (var method in methods)
            {
                if (method.ReturnType != typeof(void) || method.GetParameters().Length != 0)
                    throw new ArgumentException("The method, '{0}', in the type, '{1}', does not match the signature of a cron operation method!".FormatS(method.Name, method.DeclaringType.Name));

                var cronMethodAttribute = method.GetCustomAttribute<CronMethodAttribute>();
                if (String.IsNullOrWhiteSpace(cronMethodAttribute.Name))
                    cronMethodAttribute.Name = method.Name;

                //Compile the method.
                var parameters = method.GetParameters()
                           .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                           .ToArray();
                var call = Expression.Call(null, method, parameters);
                yield return new Tuple<CronMethodAttribute, Action>(cronMethodAttribute, (Action)Expression.Lambda(call, parameters).Compile());
            }
        }

    }
}
