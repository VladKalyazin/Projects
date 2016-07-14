﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsignarDBEntities;
using AsignarServices.AzureStorage;
using BugsTrackingSystem.Models;

namespace AsignarServices.Data
{
    public partial class AsignarDataService : IDisposable
    {
        public void AddFilter(int ownerId, FilterViewModel filter)
        {
            try
            {
                using (var dbContextTransaction = _databaseModel.Database.BeginTransaction())
                {
                    var entity = new Filter();
                    entity.Title = filter.Title;
                    entity.UserID = ownerId;
                    entity.Search = filter.Search;

                    foreach (var projectId in filter.ProjectIDs)
                    {
                        entity.Projects.Add(_databaseModel.Projects.FirstOrDefault(p => p.ProjectID == projectId));
                    }
                    foreach (var userId in filter.UserIDs)
                    {
                        entity.Users.Add(_databaseModel.Users.FirstOrDefault(u => u.UserID == userId));
                    }
                    foreach (var priorityId in filter.PriorityIDs)
                    {
                        entity.DefectPriorities.Add(_databaseModel.DefectPriorities.FirstOrDefault(dp => dp.DefectPriorityID == priorityId));
                    }
                    foreach (var statusId in filter.StatusIDs)
                    {
                        entity.DefectStatuses.Add(_databaseModel.DefectStatuses.FirstOrDefault(p => p.DefectStatusID == statusId));
                    }

                    _databaseModel.Filters.Add(entity);
                    _databaseModel.SaveChanges();

                    dbContextTransaction.Commit();
                }
            }
            catch
            {

            }
        }

        public IEnumerable<FilterViewModel> GetFilters(int userId, int countOfSet, int page)
        {
            try
            {
                var result = _databaseModel.Filters.OrderBy((f) => f.Title).
                        Where(f => f.UserID == userId).
                        Skip(page * countOfSet).Take(countOfSet).
                        Select(f => new FilterViewModel()
                        {
                            FilterId = f.FilterID,
                            Title = f.Title,
                            Search = f.Search,
                            ProjectIDs = f.Projects.Select(p => p.ProjectID),
                            UserIDs = f.Users.Select(u => u.UserID),
                            StatusIDs = f.DefectStatuses.Select(s => s.DefectStatusID),
                            PriorityIDs = f.DefectPriorities.Select(dp => dp.DefectPriorityID)
                        }).ToList();

                return result;
            }
            catch
            {

            }

            return null;
        }

    }
}