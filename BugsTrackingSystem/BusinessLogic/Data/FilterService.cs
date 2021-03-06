﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsignarDBEntities;
using AsignarServices.AzureStorage;
using BugsTrackingSystem.Models;
using System.Data.Objects.SqlClient;

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

        public int GetFiltersCount(int userId)
            => _databaseModel.Filters.Where(f => f.UserID == userId).Count();

        public FilterViewModel GetFilter(int filterId)
        {
            try
            {
                using (var dbContextTransaction = _databaseModel.Database.BeginTransaction())
                {
                    var entity = _databaseModel.Filters.First(f => f.FilterID == filterId);

                    return new FilterViewModel()
                    {
                        FilterId = filterId,
                        Title = entity.Title,
                        Search = entity.Search,
                        PriorityIDs = entity.DefectPriorities.Select(dp => dp.DefectPriorityID).ToList(),
                        ProjectIDs = entity.Projects.Select(dp => dp.ProjectID).ToList(),
                        UserIDs = entity.Users.Select(dp => dp.UserID).ToList(),
                        StatusIDs = entity.DefectStatuses.Select(dp => dp.DefectStatusID).ToList()
                    };
                }
            }
            catch (Exception)
            {
                
                //TO DO
            }
            return null;
        }

        public IEnumerable<FilterSimpleViewModel> GetFilters(int userId, int countOfSet, int page)
        {
            try
            {
                var result = _databaseModel.Filters.OrderBy((f) => f.Title).
                        Where(f => f.UserID == userId).
                        Skip(page * countOfSet).Take(countOfSet).
                        Select(f => new FilterSimpleViewModel()
                        {
                            FilterId = f.FilterID,
                            Title = f.Title,
                            Search = f.Search,
                            Projects = f.Projects.Select(p => p.ProjectName),
                            Users = f.Users.Select(u => u.FirstName + " " + u.Surname),
                            Statuses = f.DefectStatuses.Select(s => s.StatusName),
                            Priorities = f.DefectPriorities.Select(dp => dp.PriorityName)
                        }).ToList();

                return result;
            }
            catch
            {

            }

            return null;
        }

        public IEnumerable<FilterSimpleViewModel> GetAllFilters(int userId)
        {
            try
            {
                var result = _databaseModel.Filters.OrderBy((f) => f.Title).
                        Where(f => f.UserID == userId).
                        Select(f => new FilterSimpleViewModel()
                        {
                            FilterId = f.FilterID,
                            Title = f.Title,
                            Search = f.Search,
                            Projects = f.Projects.Select(p => p.ProjectName),
                            Users = f.Users.Select(u => u.FirstName + " " + u.Surname),
                            Statuses = f.DefectStatuses.Select(s => s.StatusName),
                            Priorities = f.DefectPriorities.Select(dp => dp.PriorityName)
                        }).ToList();

                return result;
            }
            catch
            {

            }

            return null;
        }

        public void DeleteFilter(int filterId)
        {
            try
            {
                _databaseModel.Filters.Remove(_databaseModel.Filters.First(f => f.FilterID == filterId));

                _databaseModel.SaveChanges();
            }
            catch
            {

            }
        }

        public IEnumerable<FilterViewModel> GetFiltersWithIds(int userId, int countOfSet, int page)
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

        public int GetDefectsCount()
            => _databaseModel.Defects.Count();

        public IEnumerable<DefectViewModel> FindDefects(FilterViewModel filter, int countOfSet, int page,
                    DefectSortProperty sortProp,
                    SortOrder sortOrder = SortOrder.Ascending)
        {
            try
            {
                string sortPropName = sortProp == DefectSortProperty.Title ? "Subject" :
                                        sortProp == DefectSortProperty.Status ? "DefectStatusID" :
                                        sortProp == DefectSortProperty.Date ? "CreationDate" :
                                        sortProp == DefectSortProperty.Priority ? "DefectPriorityID" :
                                        "AssigneeUserID";

                bool filterIsNull = false;

                if (filter == null)
                {
                    filterIsNull = true;
                    filter = new FilterViewModel();
                }

                if (filter.ProjectIDs == null)
                    filter.ProjectIDs = new List<int>();
                if (filter.UserIDs == null)
                    filter.UserIDs = new List<int>();
                if (filter.PriorityIDs == null)
                    filter.PriorityIDs = new List<int>();
                if (filter.StatusIDs == null)
                    filter.StatusIDs = new List<int>();

                if (filter.Search == null)
                    filter.Search = String.Empty;

                using (var dbContextTransaction = _databaseModel.Database.BeginTransaction())
                {
                    var result = _databaseModel.Defects.
                    Where(defect => filterIsNull ||
                                    (filter.Search.Length == 0 || defect.Subject.ToUpper().Contains(filter.Search.ToUpper())) &&
                                    (filter.ProjectIDs.Count() == 0 || filter.ProjectIDs.Any(id => id == defect.ProjectID)) &&
                                    (filter.UserIDs.Count() == 0 || filter.UserIDs.Any(id => id == defect.AssigneeUserID)) &&
                                    (filter.PriorityIDs.Count() == 0 || filter.PriorityIDs.Any(id => id == defect.DefectPriorityID)) &&
                                    (filter.StatusIDs.Count() == 0 || filter.StatusIDs.Any(id => id == defect.DefectStatusID))).
                    OrderBy(sortPropName, sortOrder == SortOrder.Descending ? true : false).
                    Skip(page * countOfSet).Take(countOfSet).
                    Select(defect => new DefectViewModel()
                    {
                        DefectId = defect.DefectID,
                        Subject = defect.Subject,
                        AssigneeUserName = defect.User.FirstName + " " + defect.User.Surname,
                        Status = defect.DefectStatus.StatusName,
                        PriorityPhoto = defect.DefectPriority.PhotoLink,
                        AssigneeUserPhoto = defect.User.PhotoLink,
                        CreationDate = defect.CreationDate,
                        ModificationDate = defect.ModificationDate,
                        ProjectName = defect.Project.ProjectName
                    }).ToList();

                    dbContextTransaction.Commit();

                    return result;
                }
            }
            catch
            {

            }

            return null;
        }

        public int CountOfDefects(FilterViewModel filter)
        {
            try
            {
                bool filterIsNull = false;

                if (filter == null)
                {
                    filterIsNull = true;
                    filter = new FilterViewModel();
                }

                if (filter.ProjectIDs == null)
                    filter.ProjectIDs = new List<int>();
                if (filter.UserIDs == null)
                    filter.UserIDs = new List<int>();
                if (filter.PriorityIDs == null)
                    filter.PriorityIDs = new List<int>();
                if (filter.StatusIDs == null)
                    filter.StatusIDs = new List<int>();

                if (filter.Search == null)
                    filter.Search = String.Empty;

                using (var dbContextTransaction = _databaseModel.Database.BeginTransaction())
                {
                    var result = _databaseModel.Defects.
                    Where(defect => filterIsNull ||
                                    (filter.Search.Length == 0 || defect.Subject.ToUpper().Contains(filter.Search.ToUpper())) &&
                                    (filter.ProjectIDs.Count() == 0 || filter.ProjectIDs.Any(id => id == defect.ProjectID)) &&
                                    (filter.UserIDs.Count() == 0 || filter.UserIDs.Any(id => id == defect.AssigneeUserID)) &&
                                    (filter.PriorityIDs.Count() == 0 || filter.PriorityIDs.Any(id => id == defect.DefectPriorityID)) &&
                                    (filter.StatusIDs.Count() == 0 || filter.StatusIDs.Any(id => id == defect.DefectStatusID))).
                    Count();

                    dbContextTransaction.Commit();

                    return result;
                }
            }
            catch
            {

            }

            return 0;
        }



    }
}