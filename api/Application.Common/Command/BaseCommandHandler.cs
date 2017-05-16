﻿namespace App.Common.Command
{
    using App.Common.Aggregate;
    using App.Common.Data;
    using Configurations;
    using Configurations.EventHandler;
    using DI;
    using Helpers;
    using Logging;
    using System;
    using System.Linq;

    public class BaseCommandHandler
    {
        protected IUnitOfWork CreateUnitOfWork<TAggregate>() where TAggregate : IBaseAggregateRoot
        {
            ILogger logger = IoC.Container.Resolve<ILogger>();
            DbContextOption dbContextOption;
            string aggregateName = typeof(TAggregate).FullName;
            Type dbContextType = null;

            AggregateOption option = Configurations.Configuration.Current.Aggregates[aggregateName];
            RepositoryType repoType = Configurations.Configuration.Current.Repository.DefaultRepoType;
            if (option != null)
            {
                repoType = option.RepoType;
                dbContextType = repoType == RepositoryType.MSSQL ? this.GetDbContextType<TAggregate>() : null;
            }

            if (option == null)
            {
                logger.Info("There is no for {0}, using default setting for UnitOfWork", aggregateName);
                dbContextOption = new DbContextOption(
                    IOMode.Write,
                    Configurations.Configuration.Current.Repository.DefaultRepoType,
                    connectionStringName: Configuration.Current.Repository.DefaultConnectionStringName,
                    dbContextType: dbContextType
                );
            }
            else
            {
                dbContextOption = new DbContextOption(
                    IOMode.Write,
                    option.RepoType,
                    connectionStringName: string.IsNullOrWhiteSpace(option.ConnectionStringName) && option.RepoType == Configuration.Current.Repository.DefaultRepoType ? Configuration.Current.Repository.DefaultConnectionStringName : option.ConnectionStringName,
                    dbContextType: dbContextType
                );
            }
            return new UnitOfWork(dbContextOption);
        }

        private Type GetDbContextType<TAggregate>()
        {
            Type contextType = AssemblyHelper.GetTypes<IDbContext<TAggregate>>().FirstOrDefault();
            //if (contextType == null)
            //{
            //    contextType = AssemblyHelper.GetTypes<IDbContext>().FirstOrDefault();
            //}
            return contextType;
        }
    }
}